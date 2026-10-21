#if DEVELOPMENT_BUILD || UNITY_EDITOR || MAZE_PERFORMANCE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using MazeOfEgos.Core;
using Newtonsoft.Json;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

namespace MazeOfEgos.Runtime
{
    // Explicit opt-in in development or dedicated performance players only.
    // No telemetry leaves the device. This mode never opens normal save files.
    public sealed class DevelopmentPerformance : MonoBehaviour
    {
        public static readonly bool Enabled = Environment.GetEnvironmentVariable("MAZE_PERFORMANCE_TEST") == "1";
        public static readonly string StorageDirectory = Path.Combine(Application.temporaryCachePath, "maze-performance-save", Guid.NewGuid().ToString("N"));
        private readonly FrameStatistics frames = new FrameStatistics(), window = new FrameStatistics();
        private readonly FrameStatistics cpu = new FrameStatistics(), gpu = new FrameStatistics();
        private readonly List<Sample> samples = new List<Sample>(200);
        private readonly Stopwatch timer = new Stopwatch();
        private ProfilerRecorder gcAlloc, cpuTime, gpuTime;
        private static DevelopmentPerformance instance;
        private GameApp app;
        private string output, error;
        private double duration, warmup, previousTime, measuredStart = -1, nextSample, nextProgress, lastGameplayProgress;
        private long gcBytes;
        private int measuredGcFrames, backgroundInterruptions, rounds, collected, usedItems, maxLevel = 1;
        private int previousHearts, previousLevel, waypointLevel, previousChances, captures;
        private int activePursuerMask;
        private long residentPeak = -1, unityPeak, managedPeak;
        private double playingSeconds, countdownSeconds, caughtSeconds, otherSeconds;
        private bool finished, hasWaypoint;
        private Point2 waypoint;
        private readonly Cell[] neighbors = new Cell[4];
        private int[] previous, queue;
        private GameSession routeSession;

        [Serializable] private sealed class Sample
        {
            public double elapsedSeconds, fps, p95Ms, p99Ms, worstMs;
            public long residentBytes, unityAllocatedBytes, managedBytes;
            public int thermalState, batteryState, lowPowerMode;
            public float batteryLevel;
            public int rounds, heartsCollected, captures, itemsUsed, level, activePursuers;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        { if (Enabled) new GameObject("Opt-in performance recording").AddComponent<DevelopmentPerformance>(); }
        private void Start()
        {
            instance = this;
            try
            {
                if (DevelopmentSmokeEnabled()) throw new InvalidOperationException("Smoke and performance flags are mutually exclusive.");
                duration = ReadSeconds("MAZE_PERFORMANCE_SECONDS", 900, 10, 3600);
                warmup = ReadSeconds("MAZE_PERFORMANCE_WARMUP_SECONDS", 15, 1, 120);
                output = Environment.GetEnvironmentVariable("MAZE_PERFORMANCE_OUTPUT") ?? Path.Combine(Application.temporaryCachePath, "maze-performance");
                Directory.CreateDirectory(output);
                Application.runInBackground = true; // Desktop automation must keep running when a build/editor gets focus.
                foreach (string name in new[] { "result.txt", "report.json", "samples.csv" }) File.Delete(Path.Combine(output, name));
                Application.logMessageReceived += OnLog;
                app = FindFirstObjectByType<GameApp>();
                if (!app) throw new InvalidOperationException("GameApp did not boot.");
                previous = new int[app.Session.Maze.Definition.width * app.Session.Maze.Definition.height]; queue = new int[previous.Length];
                app.Settings.tutorialSeen = app.Settings.itemHelpSeen = app.Settings.powerHelpSeen = true;
                app.Settings.haptics = false; // Repeated automated captures should not vibrate a resting phone.
                app.NewGame(); rounds = 1; previousHearts = app.Session.RemainingHearts; previousChances = app.Session.Chances; previousLevel = app.Session.Level;
                gcAlloc = StartCounter("GC Allocated In Frame");
                cpuTime = StartCounter("CPU Main Thread Frame Time");
                gpuTime = StartCounter("GPU Frame Time");
                DeviceMetrics.Begin();
                timer.Start(); nextProgress = warmup + 30;
                Debug.Log("PERFORMANCE START seconds=" + duration + " warmup=" + warmup + " development=" + Debug.isDebugBuild);
            }
            catch (Exception e) { error = e.ToString(); Finish(false); }
        }
        private static ProfilerRecorder StartCounter(string name)
        {
            var handles = new List<ProfilerRecorderHandle>(); ProfilerRecorderHandle.GetAvailable(handles);
            foreach (ProfilerRecorderHandle handle in handles)
            {
                ProfilerRecorderDescription description = ProfilerRecorderHandle.GetDescription(handle);
                if (description.Name == name) return ProfilerRecorder.StartNew(description.Category, name, 1);
            }
            return default;
        }
        private static bool DevelopmentSmokeEnabled()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            return DevelopmentSmoke.Enabled;
#else
            return false;
#endif
        }
        private static double ReadSeconds(string name, double fallback, double min, double max)
        {
            string value = Environment.GetEnvironmentVariable(name);
            if (value == null) return fallback;
            if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double result) ||
                double.IsNaN(result) || result < min || result > max) throw new ArgumentException("Invalid " + name);
            return result;
        }
        private void LateUpdate()
        {
            if (finished || !timer.IsRunning) return;
            try
            {
                double now = timer.Elapsed.TotalSeconds, delta = now - previousTime; previousTime = now;
                GameSession session = app.Session;
                if (session.RemainingHearts != previousHearts || session.Chances != previousChances || session.Level != previousLevel)
                    lastGameplayProgress = now;
                if (session.RemainingHearts < previousHearts && session.Level == previousLevel) collected += previousHearts - session.RemainingHearts;
                if (session.Chances < previousChances) captures += previousChances - session.Chances;
                previousHearts = session.RemainingHearts; previousChances = session.Chances; previousLevel = session.Level;
                maxLevel = Math.Max(maxLevel, session.Level);
                if (error != null) { Finish(false); return; }
                if (!session.Maze.CanOccupy(session.Player)) throw new InvalidOperationException("Autoplay entered a wall.");
                if (session.Phase == SessionPhase.Paused) throw new InvalidOperationException("Performance run was interrupted/paused.");
                if (now < warmup) return;
                if (measuredStart < 0)
                {
                    measuredStart = now; nextSample = now + 5; previousTime = now;
                    collected = captures = usedItems = activePursuerMask = 0; rounds = 1; maxLevel = session.Level; lastGameplayProgress = now;
                    TakeSample(0); return; // Exclude the warm-up boundary frame.
                }
                frames.Add(delta * 1000); window.Add(delta * 1000);
                if (gcAlloc.Valid && gcAlloc.Count > 0) { gcBytes += gcAlloc.LastValue; measuredGcFrames++; }
                if (cpuTime.Valid && cpuTime.Count > 0 && cpuTime.LastValue > 0) cpu.Add(cpuTime.LastValue * 1e-6);
                if (gpuTime.Valid && gpuTime.Count > 0 && gpuTime.LastValue > 0) gpu.Add(gpuTime.LastValue * 1e-6);
                if (session.Phase == SessionPhase.Playing) playingSeconds += delta;
                else if (session.Phase == SessionPhase.Countdown) countdownSeconds += delta;
                else if (session.Phase == SessionPhase.Caught) caughtSeconds += delta;
                else otherSeconds += delta;
                for (int i = 0; i < session.Pursuers.Length; i++) if (session.Pursuers[i].life == PursuerLife.Active) activePursuerMask |= 1 << i;
                if (now - lastGameplayProgress > 45 && session.Phase == SessionPhase.Playing)
                    throw new InvalidOperationException("Autoplay made no gameplay progress for 45 seconds.");
                if (now >= nextSample) { TakeSample(now - measuredStart); nextSample = now + 5; }
                if (now >= nextProgress)
                {
                    Debug.Log(string.Format(CultureInfo.InvariantCulture, "PERFORMANCE PROGRESS elapsed={0:F0}s fps={1:F2} p95={2:F2}ms hearts={3} captures={4} rounds={5} thermal={6}",
                        now - measuredStart, frames.FramesPerSecond, frames.Percentile(0.95), collected, captures, rounds, DeviceMetrics.Thermal()));
                    nextProgress = now + 30;
                }
                if (now - measuredStart >= duration) Finish(true);
            }
            catch (Exception e) { error = e.ToString(); Finish(false); }
        }
        // Invoked by GameApp before its normal fixed-tick simulation. It supplies
        // movement only: no teleporting, invulnerability, restored fixtures or frozen AI.
        public static Vector2 ReadMovement(GameSession session, float delta)
        {
            DevelopmentPerformance run = instance;
            if (!run || !run.timer.IsRunning || run.finished) return Vector2.zero;
            if (session.Phase == SessionPhase.GameOver)
            {
                run.app.NewGame(); run.rounds++; run.hasWaypoint = false;
                run.lastGameplayProgress = run.timer.Elapsed.TotalSeconds;
                run.previousHearts = run.app.Session.RemainingHearts; run.previousChances = run.app.Session.Chances;
                return Vector2.zero;
            }
            if (session.Phase != SessionPhase.Playing) { run.hasWaypoint = false; return Vector2.zero; }
            if (session.Inventory != ItemKind.None && session.Time >= run.nextItemAttempt)
            {
                if (session.UseItem()) { run.usedItems++; run.app.ProcessAutomationEvents(); }
                run.nextItemAttempt = session.Time + 0.5;
            }
            if (run.routeSession != session || run.waypointLevel != session.Level)
            { run.routeSession = session; run.waypointLevel = session.Level; run.hasWaypoint = false; run.nextItemAttempt = session.Time; }
            if (!run.hasWaypoint || (session.Maze.Near(run.waypoint, session.Player) - session.Player).Length < 0.025 || !session.Maze.CanOccupy(run.waypoint))
            {
                Cell start = session.Maze.NearestPlayerCell(session.Player);
                run.waypoint = (session.Maze.Near(start.Position, session.Player) - session.Player).Length > 0.04 ? start.Position : run.NextStep(session, start).Position;
                run.hasWaypoint = true;
            }
            Point2 difference = session.Maze.Near(run.waypoint, session.Player) - session.Player;
            if (difference.Length < 1e-8) return Vector2.zero;
            double yaw = Math.Atan2(difference.x, -difference.y) * 180 / Math.PI;
            session.Yaw = Mathf.MoveTowardsAngle((float)session.Yaw, (float)yaw, 150 * delta);
            session.Pitch = Math.Sin(session.Time * 0.3) * 8;
            Point2 movement = difference.Normalized * Math.Min(1, difference.Length / Math.Max(0.001, session.PlayerSpeed * delta));
            return new Vector2((float)Point2.Dot(movement, Point2.Right(session.Yaw)), (float)Point2.Dot(movement, Point2.Forward(session.Yaw)));
        }
        private double nextItemAttempt;
        private Cell NextStep(GameSession session, Cell start)
        {
            MazeMap maze = session.Maze; int width = maze.Definition.width;
            Array.Fill(previous, -1); int origin = start.y * width + start.x, tail = 1, head = 0;
            previous[origin] = origin; queue[0] = origin;
            while (head < tail)
            {
                int index = queue[head++]; Cell cell = new Cell(index % width, index / width);
                int heart = maze.Hearts.IndexOf(cell);
                if (index != origin && heart >= 0 && !session.Collected[heart])
                {
                    while (previous[index] != origin) index = previous[index];
                    return new Cell(index % width, index / width);
                }
                int count = maze.Neighbors(cell, false, neighbors);
                for (int i = 0; i < count; i++)
                {
                    int next = neighbors[i].y * width + neighbors[i].x;
                    if (previous[next] >= 0) continue;
                    previous[next] = index; queue[tail++] = next;
                }
            }
            return start;
        }
        private void TakeSample(double elapsed)
        {
            long resident = DeviceMetrics.Resident();
            long unity = Profiler.GetTotalAllocatedMemoryLong(), managed = GC.GetTotalMemory(false);
            residentPeak = Math.Max(residentPeak, resident); unityPeak = Math.Max(unityPeak, unity); managedPeak = Math.Max(managedPeak, managed);
            int active = 0; foreach (PursuerState p in app.Session.Pursuers) if (p.life == PursuerLife.Active) active++;
            samples.Add(new Sample { elapsedSeconds = elapsed, fps = window.FramesPerSecond, p95Ms = window.Percentile(0.95),
                p99Ms = window.Percentile(0.99), worstMs = window.WorstMs, residentBytes = resident, unityAllocatedBytes = unity, managedBytes = managed,
                thermalState = DeviceMetrics.Thermal(), batteryState = DeviceMetrics.BatteryState(), batteryLevel = DeviceMetrics.BatteryLevel(),
                lowPowerMode = DeviceMetrics.LowPower(), rounds = rounds, heartsCollected = collected, captures = captures, itemsUsed = usedItems,
                level = app.Session.Level, activePursuers = active });
            window.Clear();
            // Bounded checkpoint file; it remains readable if a test is interrupted.
            AtomicFile.Write(Path.Combine(output, "checkpoint.json"), JsonConvert.SerializeObject(samples[samples.Count - 1]));
        }
        private void Finish(bool completed)
        {
            if (finished) return; finished = true; timer.Stop();
            double measured = measuredStart < 0 ? 0 : timer.Elapsed.TotalSeconds - measuredStart;
            if (output != null)
            {
                try
                {
                    if (app && measuredStart >= 0) TakeSample(measured);
                    bool stable = completed && error == null && backgroundInterruptions == 0 && collected >= Math.Max(1, duration / 10) && playingSeconds >= duration * 0.5;
                    var report = new {
                        schemaVersion = 1, completed = completed, stabilityPassed = stable, error, backgroundInterruptions,
                        requestedSeconds = duration, warmupSeconds = warmup, measuredSeconds = measured,
                        utc = DateTime.UtcNow.ToString("O"), unityVersion = Application.unityVersion, appVersion = Application.version,
                        buildGuid = Application.buildGUID, automationPolicy = "nearest-heart-v1",
                        developmentBuild = Debug.isDebugBuild, platform = Application.platform.ToString(), deviceModel = SystemInfo.deviceModel,
                        operatingSystem = SystemInfo.operatingSystem, gpu = SystemInfo.graphicsDeviceName,
                        width = Screen.width, height = Screen.height, targetFps = Application.targetFrameRate,
                        frameCount = frames.Count, averageFps = frames.FramesPerSecond, averageMs = frames.AverageMs,
                        p50Ms = frames.Percentile(0.5), p95Ms = frames.Percentile(0.95), p99Ms = frames.Percentile(0.99), worstMs = frames.WorstMs,
                        framesOver33Ms = frames.Over33Ms, framesOver50Ms = frames.Over50Ms,
                        cpuTimingFrames = cpu.Count, cpuMainThreadAverageMs = cpu.AverageMs, cpuMainThreadP95Ms = cpu.Percentile(0.95),
                        gpuTimingFrames = gpu.Count, gpuAverageMs = gpu.AverageMs, gpuP95Ms = gpu.Percentile(0.95),
                        gcCounterAvailable = gcAlloc.Valid && measuredGcFrames > 0, gcAllocatedBytes = gcBytes, gcMeasuredFrames = measuredGcFrames,
                        residentPeakBytes = residentPeak, unityAllocatedPeakBytes = unityPeak, managedPeakBytes = managedPeak,
                        playingSeconds, countdownSeconds, caughtSeconds, otherSeconds, rounds, heartsCollected = collected, captures, itemsUsed = usedItems, maxLevel, activePursuerMask,
                        notes = "Automated nearest-heart traversal with live AI and normal captures/replay. Haptics disabled. Includes recorder/checkpoint overhead. USB charging invalidates battery-drain conclusions. Not manual play or final character performance.", samples };
                    AtomicFile.Write(Path.Combine(output, "report.json"), JsonConvert.SerializeObject(report, Formatting.Indented));
                    using (var csv = new StreamWriter(Path.Combine(output, "samples.csv")))
                    {
                        csv.WriteLine("elapsed_seconds,fps,p95_ms,p99_ms,worst_ms,resident_bytes,unity_allocated_bytes,managed_bytes,thermal_state,battery_level,battery_state,rounds,hearts,captures,items,level,active_pursuers");
                        foreach (Sample s in samples) csv.WriteLine(string.Join(",", new object[] { s.elapsedSeconds, s.fps, s.p95Ms, s.p99Ms, s.worstMs, s.residentBytes,
                            s.unityAllocatedBytes, s.managedBytes, s.thermalState, s.batteryLevel, s.batteryState, s.rounds, s.heartsCollected, s.captures, s.itemsUsed, s.level, s.activePursuers }
                            .ConvertInvariant()));
                    }
                    File.WriteAllText(Path.Combine(output, "result.txt"), (stable ? "PASS " : "FAIL ") + "performance stability; inspect report.json for frame budget and thermal/memory results.\n" + error);
                    Debug.Log(stable ? "PERFORMANCE COMPLETED" : "PERFORMANCE FAILED: " + error);
                }
                catch (Exception e) { Debug.LogError("PERFORMANCE REPORT FAILED: " + e); }
            }
            gcAlloc.Dispose(); cpuTime.Dispose(); gpuTime.Dispose(); DeviceMetrics.End(); Application.Quit(completed && error == null ? 0 : 1);
        }
        private void OnLog(string message, string stack, LogType type)
        { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) error = message + "\n" + stack; }
        private void OnApplicationPause(bool paused) { if (paused && timer.IsRunning) { backgroundInterruptions++; error = "Application backgrounded."; Finish(false); } }
        private void OnApplicationQuit() { if (!finished && timer.IsRunning) { error = "Application quit before completion."; Finish(false); } }
        private void OnDestroy() { Application.logMessageReceived -= OnLog; gcAlloc.Dispose(); cpuTime.Dispose(); gpuTime.Dispose(); if (instance == this) instance = null; }
    }
    internal static class PerformanceFormatting
    {
        public static string[] ConvertInvariant(this object[] values)
        { var result = new string[values.Length]; for (int i = 0; i < values.Length; i++) result[i] = Convert.ToString(values[i], CultureInfo.InvariantCulture); return result; }
    }
    internal static class DeviceMetrics
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void MazeMetricsBegin();
        [DllImport("__Internal")] private static extern void MazeMetricsEnd();
        [DllImport("__Internal")] private static extern int MazeThermalState();
        [DllImport("__Internal")] private static extern int MazeLowPowerMode();
        [DllImport("__Internal")] private static extern float MazeBatteryLevel();
        [DllImport("__Internal")] private static extern int MazeBatteryState();
        [DllImport("__Internal")] private static extern long MazeResidentBytes();
        public static void Begin() => MazeMetricsBegin();
        public static void End() => MazeMetricsEnd();
        public static int Thermal() => MazeThermalState();
        public static int LowPower() => MazeLowPowerMode();
        public static float BatteryLevel() => MazeBatteryLevel();
        public static int BatteryState() => MazeBatteryState();
        public static long Resident() => MazeResidentBytes();
#else
        public static void Begin() { }
        public static void End() { }
        public static int Thermal() => -1;
        public static int LowPower() => -1;
        public static float BatteryLevel() => SystemInfo.batteryLevel;
        public static int BatteryState() => (int)SystemInfo.batteryStatus;
        public static long Resident() => -1;
#endif
    }
}
#endif
