#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using MazeOfEgos.Core;
using Newtonsoft.Json;
using UnityEngine;

namespace MazeOfEgos.Runtime
{
    // The external runner opens Settings and reactivates this *same* process.
    // This test therefore requires real OS pause/resume callbacks.
    public sealed class DevelopmentLifecycle : MonoBehaviour
    {
        public static bool Enabled => Environment.GetEnvironmentVariable("MAZE_LIFECYCLE_TEST") == "1";
        public static readonly string StorageDirectory = Path.Combine(Application.temporaryCachePath, "maze-lifecycle-save");
        private string output;
        private GameApp app;
        private double pausedAt, power, boost, wall;
        private float backgroundAt;
        private bool ready, paused, resumed;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot() { if (Enabled) new GameObject("Actual OS lifecycle check").AddComponent<DevelopmentLifecycle>(); }
        private IEnumerator Start()
        {
            output = Path.Combine(Application.temporaryCachePath, "maze-lifecycle"); Directory.CreateDirectory(output);
            foreach (string file in Directory.GetFiles(output)) File.Delete(file);
            yield return new WaitForSecondsRealtime(1);
            app = FindFirstObjectByType<GameApp>(); app.Settings.haptics = false;
            app.NewGame(); app.Session.Resume(0);
            var fixture = app.Session.Snapshot(); fixture.phase = fixture.resumePhase = SessionPhase.Playing;
            fixture.powerUntil = fixture.time + 40; fixture.boostUntil = fixture.time + 30;
            fixture.walls[0].phase = WallPhase.Warning; fixture.walls[0].phaseUntil = fixture.time + 20;
            foreach (var p in fixture.pursuers) { p.releaseAt = fixture.time + 100; p.stunUntil = fixture.time + 100; }
            app.Session.Restore(fixture); app.Session.Resume(0);
            yield return new WaitForSecondsRealtime(.3f);
            ready = true; File.WriteAllText(Path.Combine(output, "ready.txt"), "READY: open Settings, wait >=5 seconds, reactivate this process.");
            Debug.Log("LIFECYCLE READY");
            float deadline = Time.realtimeSinceStartup + 120;
            while (!resumed && Time.realtimeSinceStartup < deadline) yield return null;
            bool ok = paused && resumed && Time.realtimeSinceStartup - backgroundAt >= 5 &&
                app.Session.Phase == SessionPhase.Paused && Math.Abs(app.Session.Time - pausedAt) < .00001 &&
                Math.Abs(app.Session.PowerUntil - app.Session.Time - power) < .00001 &&
                Math.Abs(app.Session.BoostUntil - app.Session.Time - boost) < .00001 &&
                Math.Abs(app.Session.Walls[0].phaseUntil - app.Session.Time - wall) < .00001;
            string saved = Path.Combine(StorageDirectory, "session-v1.json");
            SessionSnapshot snapshot = File.Exists(saved) ? JsonConvert.DeserializeObject<SessionSnapshot>(File.ReadAllText(saved)) : null;
            ok &= snapshot != null && snapshot.phase == SessionPhase.Paused && Math.Abs(snapshot.time - pausedAt) < .00001;
            bool playbackAudio = PlatformAudio.PlaybackPolicyActive;
            ok &= playbackAudio;
            double frozenTime = app.Session.Time; app.Session.Resume(3);
            yield return new WaitForSecondsRealtime(.4f);
            ok &= app.Session.Phase == SessionPhase.Countdown && Math.Abs(app.Session.Time - frozenTime) < .00001;
            string result = JsonConvert.SerializeObject(new { passed = ok, realOsPause = paused, realOsResume = resumed,
                backgroundSeconds = Time.realtimeSinceStartup - backgroundAt, frozenSimulationTime = pausedAt,
                powerRemaining = power, boostRemaining = boost, warningRemaining = wall, checkpointVerified = snapshot != null,
                playbackAudioAfterForeground = playbackAudio }, Formatting.Indented);
            File.WriteAllText(Path.Combine(output, "report.json"), result);
            File.WriteAllText(Path.Combine(output, "result.txt"), ok ? "PASS actual OS background/resume, frozen timers, disk checkpoint, resume countdown." : "FAIL actual OS lifecycle check.");
            Debug.Log("LIFECYCLE " + (ok ? "PASSED" : "FAILED")); Application.Quit(ok ? 0 : 1);
        }
        private void OnApplicationPause(bool value)
        {
            if (!ready) return;
            if (value)
            {
                paused = true; backgroundAt = Time.realtimeSinceStartup;
                pausedAt = app.Session.Time; power = app.Session.PowerUntil - pausedAt;
                boost = app.Session.BoostUntil - pausedAt; wall = app.Session.Walls[0].phaseUntil - pausedAt;
            }
            else if (paused) resumed = true;
        }
    }
}
#endif
