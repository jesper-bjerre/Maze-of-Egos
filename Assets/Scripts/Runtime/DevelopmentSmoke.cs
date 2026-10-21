#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using MazeOfEgos.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace MazeOfEgos.Runtime
{
    // Opt-in integration check in a real graphical development player. No OS input is controlled.
    public sealed class DevelopmentSmoke : MonoBehaviour
    {
        public static bool Enabled => Array.IndexOf(Environment.GetCommandLineArgs(), "--maze-smoke-test") >= 0 ||
            Environment.GetEnvironmentVariable("MAZE_SMOKE_TEST") == "1";
        public static readonly string StorageDirectory = Path.Combine(Application.temporaryCachePath, "maze-smoke", Guid.NewGuid().ToString("N"));
        private string output, runtimeError;
        private Touchscreen screen;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartCheck()
        {
            if (Enabled) new GameObject("Development runtime check").AddComponent<DevelopmentSmoke>();
        }
        private void Start()
        {
            Debug.Log("RUNTIME SMOKE ENABLED");
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "--maze-smoke-output");
            output = index >= 0 && index + 1 < args.Length ? args[index + 1] : Path.Combine(Application.temporaryCachePath, "maze-smoke-captures");
            Directory.CreateDirectory(output);
            Application.runInBackground = true;
            Application.logMessageReceived += OnLog;
            StartCoroutine(Guarded());
        }
        private void OnLog(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) runtimeError = message;
        }
        private IEnumerator Guarded()
        {
            IEnumerator steps = Run();
            while (true)
            {
                object next = null; bool more = false; Exception failure = null;
                try
                {
                    if (runtimeError != null) throw new InvalidOperationException("Runtime log: " + runtimeError);
                    more = steps.MoveNext(); if (more) next = steps.Current;
                }
                catch (Exception e) { failure = e; }
                if (failure != null)
                {
                    File.WriteAllText(Path.Combine(output, "result.txt"), "FAIL " + failure);
                    Debug.LogError("RUNTIME SMOKE FAILED: " + failure); Application.Quit(1); yield break;
                }
                if (!more) break;
                yield return next;
            }
            File.WriteAllText(Path.Combine(output, "result.txt"), "PASS native rendering, portraits, menu, three-touch controls in both layouts, release, pause, settings, save/resume, banana flight/hand/landing, mid-flight save/resume, reduced motion, banana/oil visuals, shortcut signals/opening/reservation/closure, pursuer gates, rigged Putin chase/ground contact/pause/slip/power/return/profile/back, level transition, Game Over and replay.\nSynthetic touch and controlled scene fixtures; manual device testing, full rounds against AI and performance remain separate.\n");
            Debug.Log("RUNTIME SMOKE PASSED"); Application.Quit(0);
        }
        private IEnumerator Run()
        {
            yield return new WaitForSecondsRealtime(1);
            GameApp app = FindFirstObjectByType<GameApp>();
            Require(app && app.Session != null, "Application did not boot.");
            Require(GameObject.Find("Main menu"), "Main menu is missing.");
            Require(FindObjectsByType<RawImage>(FindObjectsSortMode.None).Length == 5, "Five menu portraits were not rendered.");
            foreach (Renderer renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                foreach (Material material in renderer.sharedMaterials)
                    Require(material && material.shader && material.shader.isSupported && !material.shader.name.Contains("InternalError"), "Unsupported scene material.");
            screen = InputSystem.AddDevice<Touchscreen>();
            yield return new WaitForEndOfFrame(); Capture("01-menu.png");
            Vector2 play = ButtonPosition("SPIL"); Touch(1, TouchPhase.Began, play);
            yield return new WaitForSecondsRealtime(0.1f); Touch(1, TouchPhase.Ended, play);
            yield return new WaitForSecondsRealtime(3.6f);
            Require(app.Session.Phase == SessionPhase.Playing && !GameObject.Find("Main menu"), "Touch on SPIL did not start gameplay.");

            // A controlled fixture gives an item and delays threats while testing the actual UI/input pipeline.
            SessionSnapshot fixture = app.Session.Snapshot(); fixture.inventory = ItemKind.SpeedBoost;
            foreach (PursuerState pursuer in fixture.pursuers) { pursuer.releaseAt = fixture.time + 120; pursuer.stunUntil = fixture.time + 120; }
            app.Session.Restore(fixture); app.Session.Resume(0);
            yield return null;
            TouchControls joystick = GameObject.Find("Movement joystick").GetComponent<TouchControls>();
            Vector2 joy = RectTransformUtility.WorldToScreenPoint(null, joystick.transform.position);
            float scale = joystick.GetComponentInParent<Canvas>().scaleFactor;
            Vector2 joyUp = joy + Vector2.up * (70 * scale);
            Vector2 look = new Vector2(Screen.width * 0.72f, Screen.height * 0.55f);
            Point2 before = app.Session.Player; double yaw = app.Session.Yaw;
            Touch(2, TouchPhase.Began, joyUp); Touch(3, TouchPhase.Began, look);
            yield return new WaitForSecondsRealtime(0.1f);
            Touch(3, TouchPhase.Moved, look + Vector2.right * (30 * scale));
            yield return new WaitForSecondsRealtime(0.1f);
            Require(joystick.Movement.sqrMagnitude > 0.1f && Math.Abs(app.Session.Yaw - yaw) > 1, "Simultaneous movement and look failed.");
            double afterLook = app.Session.Yaw;
            Vector2 use = ButtonPosition("", "BOOST"); Touch(4, TouchPhase.Began, use);
            yield return new WaitForSecondsRealtime(0.1f); Touch(4, TouchPhase.Ended, use);
            yield return new WaitForSecondsRealtime(0.35f);
            Require(app.Session.Inventory == ItemKind.None && app.Session.BoostUntil > app.Session.Time, "Third touch did not use the held item.");
            Require(Math.Abs(app.Session.Yaw - afterLook) < 0.01 && joystick.Movement.sqrMagnitude > 0.1f, "Item touch interfered with movement or camera.");
            // Wait for simulation progress, allowing shader warm-up or a concurrent build to stall rendering.
            float deadline = Time.realtimeSinceStartup + 5;
            while (app.Session.RemainingHearts == 244 && Time.realtimeSinceStartup < deadline) yield return null;
            Require((app.Session.Player - before).Length > 0.15 && app.Session.RemainingHearts < 244,
                "Touch movement did not collect hearts: distance=" + (app.Session.Player - before).Length +
                ", remaining=" + app.Session.RemainingHearts + ", phase=" + app.Session.Phase + ", time=" + app.Session.Time);
            Touch(2, TouchPhase.Ended, joyUp); Touch(3, TouchPhase.Ended, look + Vector2.right * (30 * scale));
            yield return new WaitForSecondsRealtime(0.15f); Point2 stopped = app.Session.Player;
            yield return new WaitForSecondsRealtime(0.2f);
            Require(joystick.Movement == Vector2.zero && (app.Session.Player - stopped).Length < 0.001, "Release did not stop the player.");
            RequireHud(app.Session);
            yield return new WaitForEndOfFrame(); Capture("02-gameplay.png");

            Vector2 pause = ButtonPosition("Ⅱ"); Touch(5, TouchPhase.Began, pause);
            yield return new WaitForSecondsRealtime(0.1f); Touch(5, TouchPhase.Ended, pause);
            yield return new WaitForSecondsRealtime(0.15f); double pausedAt = app.Session.Time;
            Require(app.Session.Phase == SessionPhase.Paused, "Pause button failed.");
            yield return new WaitForSecondsRealtime(0.3f);
            Require(app.Session.Time == pausedAt, "Paused simulation advanced.");
            Vector2 settings = ButtonPosition("INDSTILLINGER"); Touch(6, TouchPhase.Began, settings);
            yield return new WaitForSecondsRealtime(0.1f); Touch(6, TouchPhase.Ended, settings);
            yield return new WaitForSecondsRealtime(0.2f);
            Require(GameObject.Find("Settings"), "Settings did not open.");
            yield return new WaitForEndOfFrame(); Capture("03-settings.png");
            Vector2 sound = ButtonPosition("", "Lyd:"); bool wasSound = app.Settings.sound;
            Touch(7, TouchPhase.Began, sound);
            yield return new WaitForSecondsRealtime(0.1f); Touch(7, TouchPhase.Ended, sound);
            yield return new WaitForSecondsRealtime(0.15f);
            Require(app.Settings.sound != wasSound, "Settings touch did not change sound.");
            Vector2 back = ButtonPosition("TILBAGE"); Touch(8, TouchPhase.Began, back);
            yield return new WaitForSecondsRealtime(0.1f); Touch(8, TouchPhase.Ended, back);
            yield return new WaitForSecondsRealtime(0.15f);
            Vector2 menu = ButtonPosition("HOVEDMENU"); Touch(9, TouchPhase.Began, menu);
            yield return new WaitForSecondsRealtime(0.1f); Touch(9, TouchPhase.Ended, menu);
            yield return new WaitForSecondsRealtime(0.15f);
            Require(app.HasSavedGame, "Pause/menu did not save the run.");
            Vector2 resume = ButtonPosition("FORTSÆT GEMT SPIL"); Touch(10, TouchPhase.Began, resume);
            yield return new WaitForSecondsRealtime(0.1f); Touch(10, TouchPhase.Ended, resume);
            yield return new WaitForSecondsRealtime(0.15f);
            Require(app.Session.Phase == SessionPhase.Paused && app.Session.Time == pausedAt && (app.Session.Player - stopped).Length < 0.001, "Saved game did not restore paused at the same position/time.");
            Require(File.Exists(Path.Combine(StorageDirectory, "session-v1.json")), "No snapshot was written.");

            // Change the layout through the actual settings button and repeat all three touches.
            Vector2 mirroredSettings = ButtonPosition("INDSTILLINGER"); Touch(11, TouchPhase.Began, mirroredSettings);
            yield return new WaitForSecondsRealtime(0.1f); Touch(11, TouchPhase.Ended, mirroredSettings);
            yield return new WaitForSecondsRealtime(0.1f);
            Vector2 handedness = ButtonPosition("", "Venstrehåndslayout:"); Touch(12, TouchPhase.Began, handedness);
            yield return new WaitForSecondsRealtime(0.1f); Touch(12, TouchPhase.Ended, handedness);
            yield return new WaitForSecondsRealtime(0.1f);
            Require(app.Settings.leftHanded, "Left-handed setting did not change.");
            back = ButtonPosition("TILBAGE"); Touch(13, TouchPhase.Began, back);
            yield return new WaitForSecondsRealtime(0.1f); Touch(13, TouchPhase.Ended, back);
            yield return new WaitForSecondsRealtime(0.1f);
            InstallFixture(app, ItemKind.BananaPeel, new Point2(6, 5), 90);
            yield return new WaitForSecondsRealtime(0.15f);
            joy = RectTransformUtility.WorldToScreenPoint(null, joystick.transform.position);
            Require(joy.x > Screen.width * 0.5f, "Movement joystick did not move to the right.");
            joyUp = joy + Vector2.up * (70 * scale); look = new Vector2(Screen.width * 0.28f, Screen.height * 0.55f);
            before = app.Session.Player; yaw = app.Session.Yaw;
            Touch(14, TouchPhase.Began, joyUp); Touch(15, TouchPhase.Began, look);
            yield return new WaitForSecondsRealtime(0.1f);
            Touch(15, TouchPhase.Moved, look + Vector2.right * (20 * scale));
            yield return new WaitForSecondsRealtime(0.1f);
            Require(joystick.Movement.sqrMagnitude > 0.1f && Math.Abs(app.Session.Yaw - yaw) > 1, "Mirrored movement/look failed.");
            afterLook = app.Session.Yaw; use = ButtonPosition("Use item");
            Require(use.x < Screen.width * 0.5f, "Item button did not move to the left.");
            Touch(16, TouchPhase.Began, use);
            yield return new WaitForSecondsRealtime(0.1f); Touch(16, TouchPhase.Ended, use);
            yield return new WaitForSecondsRealtime(0.2f);
            Require(app.Session.Inventory == ItemKind.None && app.Session.Traps.Count == 1 && app.Session.Traps[0].kind == ItemKind.BananaPeel, "Mirrored item touch did not place a banana.");
            Require(Math.Abs(app.Session.Yaw - afterLook) < 0.01 && joystick.Movement.sqrMagnitude > 0.1f && (app.Session.Player - before).Length > 0.15,
                "Mirrored item touch interfered with movement/look.");
            Touch(14, TouchPhase.Ended, joyUp); Touch(15, TouchPhase.Ended, look + Vector2.right * (20 * scale));
            yield return new WaitForSecondsRealtime(0.1f);
            Require(joystick.Movement == Vector2.zero, "Mirrored joystick did not stop on release.");
            yield return new WaitForEndOfFrame(); Capture("04-left-handed.png");

            // The simulation clock drives the flight and hand as well as the trap.
            IEnumerator handednessSteps = ChangeHandedness(app, false);
            while (handednessSteps.MoveNext()) yield return handednessSteps.Current;
            InstallFixture(app, ItemKind.BananaPeel, new Point2(6, 5), 90);
            yield return null;
            app.UseItem(); TrapState flight = app.Session.Traps[0];
            deadline = Time.realtimeSinceStartup + 4;
            while (flight.FlightProgress(app.Session.Time) < 0.33 && Time.realtimeSinceStartup < deadline) yield return null;
            Require(app.Session.Time < flight.landsAt, "Banana flight was not visible before landing.");
            yield return new WaitForEndOfFrame(); Capture("11-banana-flight.png");
            Transform flyingMesh = GameObject.Find("Banana peel").transform.parent;
            Transform hand = GameObject.Find("Zelenskyj throwing hand (prototype)").transform;
            Require(flyingMesh.position.y > 0.4f && hand, "Flight/hand geometry was not visible.");
            Camera firstPerson = hand.GetComponentInParent<Camera>(); float fieldOfView = firstPerson.fieldOfView;
            Require(firstPerson.WorldToViewportPoint(hand.position).x > 0.5f, "Standard layout used the wrong throwing hand.");
            app.Session.Pause(); double flightPausedAt = app.Session.Time; Vector3 frozenMesh = flyingMesh.position, frozenHand = hand.position;
            yield return new WaitForSecondsRealtime(0.2f);
            Require(app.Session.Time == flightPausedAt && Vector3.Distance(flyingMesh.position, frozenMesh) < 0.001f &&
                Vector3.Distance(hand.position, frozenHand) < 0.001f, "Paused throw or hand continued moving.");
            app.GoToMenu(); app.ContinueSaved();
            Require(app.Session.Phase == SessionPhase.Paused && app.Session.Time == flightPausedAt &&
                app.Session.Traps.Count == 1 && app.Session.Traps[0].landsAt == flight.landsAt, "Mid-flight save/resume lost the throw.");
            Require(File.ReadAllText(Path.Combine(StorageDirectory, "session-v1.json")).Contains("landsAt"), "Flight was not persisted to disk.");
            app.TogglePause();
            yield return new WaitForSecondsRealtime(0.3f);
            Require(app.Session.Time == flightPausedAt, "Resume countdown consumed banana flight time.");
            deadline = Time.realtimeSinceStartup + 5;
            while ((app.Session.Phase != SessionPhase.Playing || app.Session.Time < flight.landsAt + 0.1) && Time.realtimeSinceStartup < deadline) yield return null;
            Require(app.Session.Traps.Count == 1 && Math.Abs(flyingMesh.position.y) < 0.001 &&
                !GameObject.Find("Zelenskyj throwing hand (prototype)"), "Banana did not settle on the floor after resume.");
            Require(Math.Abs(firstPerson.fieldOfView - fieldOfView) < 0.001, "Throw changed the camera FOV.");
            yield return new WaitForEndOfFrame(); Capture("12-banana-landed.png");

            handednessSteps = ChangeHandedness(app, true);
            while (handednessSteps.MoveNext()) yield return handednessSteps.Current;
            app.Settings.reducedMotion = true;
            InstallFixture(app, ItemKind.BananaPeel, new Point2(6, 5), 90);
            yield return null;
            app.UseItem(); flight = app.Session.Traps[0];
            deadline = Time.realtimeSinceStartup + 4;
            while (flight.FlightProgress(app.Session.Time) < 0.33 && Time.realtimeSinceStartup < deadline) yield return null;
            yield return new WaitForEndOfFrame(); Capture("13-banana-reduced-motion.png");
            flyingMesh = GameObject.Find("Banana peel").transform.parent;
            hand = GameObject.Find("Zelenskyj throwing hand (prototype)").transform;
            Require(app.Session.Time < flight.landsAt && Quaternion.Angle(flyingMesh.rotation, Quaternion.identity) < 0.01f &&
                firstPerson.WorldToViewportPoint(hand.position).x < 0.5f, "Reduced-motion throw spun or used the wrong hand.");
            app.Settings.reducedMotion = false;

            // Exercise the oil button and check the actual pooled representations.
            InstallFixture(app, ItemKind.OilSlick, new Point2(6, 5), 90);
            yield return new WaitForSecondsRealtime(0.1f); use = ButtonPosition("Use item");
            Touch(17, TouchPhase.Began, use);
            yield return new WaitForSecondsRealtime(0.1f); Touch(17, TouchPhase.Ended, use);
            yield return new WaitForSecondsRealtime(0.15f);
            Require(app.Session.Inventory == ItemKind.None && app.Session.Traps.Count == 1 && app.Session.Traps[0].kind == ItemKind.OilSlick, "Oil button failed.");
            Require(GameObject.Find("Oil slick"), "Oil pool did not appear.");
            SessionSnapshot gallery = app.Session.Snapshot(); gallery.player = new Point2(6, 5); gallery.yaw = 90; gallery.pitch = -12;
            gallery.traps = new[] {
                new TrapState { id = 100, kind = ItemKind.BananaPeel, position = new Point2(7.5, 5), expiresAt = gallery.time + 30 },
                new TrapState { id = 101, kind = ItemKind.OilSlick, position = new Point2(9, 5), expiresAt = gallery.time + 30 } };
            gallery.nextTrapId = 101; gallery.inventory = ItemKind.OilSlick;
            app.Session.Restore(gallery); app.Session.Resume(0);
            yield return new WaitForSecondsRealtime(0.1f);
            Require(GameObject.Find("Banana peel") && GameObject.Find("Oil slick"), "Trap representations were not independently enabled.");
            yield return new WaitForEndOfFrame(); Capture("05-items.png");

            InstallFixture(app, ItemKind.None, new Point2(11, 1), 90);
            SessionSnapshot shortcut = app.Session.Snapshot(); shortcut.nextWallAttempt = shortcut.time + 999;
            shortcut.pitch = -10;
            shortcut.walls[0].phase = WallPhase.Warning; shortcut.walls[0].phaseUntil = shortcut.time + 0.5;
            app.Session.Restore(shortcut); app.Session.Resume(0);
            yield return new WaitForSecondsRealtime(0.2f);
            Require(app.Session.Walls[0].phase == WallPhase.Warning, "Shortcut warning was skipped.");
            Renderer signal = GameObject.Find("Shortcut floor signal 0").GetComponent<Renderer>();
            Require(signal.sharedMaterial.GetColor("_EmissionColor").maxColorComponent > 0.2f, "Shortcut warning signal is not illuminated.");
            yield return new WaitForEndOfFrame(); Capture("06-shortcut-warning.png");
            yield return new WaitForSecondsRealtime(1.8f);
            Require(app.Session.Walls[0].phase == WallPhase.Open && app.Session.Maze.IsWalkable(new Cell(13, 1)), "Shortcut did not become passable.");
            Require(GameObject.Find("Retracting wall 0").transform.position.y < -2, "Shortcut geometry did not retract.");
            yield return new WaitForEndOfFrame(); Capture("07-shortcut-open.png");
            shortcut = app.Session.Snapshot(); shortcut.player = new Point2(13, 1); shortcut.trail = new[] { shortcut.player };
            shortcut.walls[0].phaseUntil = shortcut.time + 0.1;
            app.Session.Restore(shortcut); app.Session.Resume(0);
            yield return new WaitForSecondsRealtime(0.35f);
            Require(app.Session.Walls[0].phase == WallPhase.Open && (app.Session.Player - new Point2(13, 1)).Length < 0.01, "Occupied shortcut closed on the player.");
            shortcut = app.Session.Snapshot(); shortcut.player = new Point2(12, 1); shortcut.trail = new[] { shortcut.player };
            app.Session.Restore(shortcut); app.Session.Resume(0);
            yield return new WaitForSecondsRealtime(1.6f);
            Require(app.Session.Walls[0].phase == WallPhase.Closed && !app.Session.Maze.IsWalkable(new Cell(13, 1)) &&
                GameObject.Find("Retracting wall 0").transform.position.y > 2.2, "Unoccupied shortcut did not close consistently.");

            InstallFixture(app, ItemKind.None, new Point2(13.5, 11), 180);
            SessionSnapshot gate = app.Session.Snapshot(); gate.pursuers[0].life = PursuerLife.Exiting;
            gate.pursuers[0].from = new Cell(13, 12); gate.pursuers[0].to = new Cell(13, 11); gate.pursuers[0].progress = 0.3;
            app.Session.Restore(gate); app.Session.Resume(0);
            yield return new WaitForSecondsRealtime(0.6f);
            Require(GameObject.Find("Pursuer gate 0").transform.localEulerAngles.y > 80, "Pursuer gate did not open for an exiting character.");
            yield return new WaitForEndOfFrame(); Capture("08-base.png");
            gate = app.Session.Snapshot(); gate.pursuers[0].life = PursuerLife.InHouse;
            gate.pursuers[0].from = gate.pursuers[0].to = new Cell(13, 14); gate.pursuers[0].progress = 0;
            app.Session.Restore(gate); app.Session.Resume(0);
            yield return new WaitForSecondsRealtime(0.6f);
            Require(Mathf.Abs(Mathf.DeltaAngle(GameObject.Find("Pursuer gate 0").transform.localEulerAngles.y, 0)) < 1, "Pursuer gate did not close.");

            IEnumerator characterSteps = CheckCharacter(app);
            while (characterSteps.MoveNext()) yield return characterSteps.Current;

            InstallFixture(app, ItemKind.None, new Point2(1, 29), 90);
            SessionSnapshot ending = app.Session.Snapshot(); ending.score = 0;
            for (int i = 0; i < ending.collected.Length; i++) ending.collected[i] = true;
            ending.collected[app.Session.Maze.Hearts.IndexOf(new Cell(1, 29))] = false; ending.remainingHearts = 1;
            app.Session.Restore(ending); app.Session.Resume(0);
            yield return new WaitForSecondsRealtime(0.15f);
            Require(app.Session.Phase == SessionPhase.LevelClear && app.Session.RemainingHearts == 0 && app.Session.Score == 1010,
                "Last-heart scene did not clear with the correct score.");
            yield return new WaitForEndOfFrame(); Capture("09-level-clear.png");
            deadline = Time.realtimeSinceStartup + 7;
            while ((app.Session.Level != 2 || app.Session.Phase != SessionPhase.Playing) && Time.realtimeSinceStartup < deadline) yield return null;
            Require(app.Session.Level == 2 && app.Session.Phase == SessionPhase.Playing && app.Session.RemainingHearts == 244 && app.Session.Score == 1010,
                "Next level did not start with a fresh maze and preserved score.");

            InstallFixture(app, ItemKind.None, new Point2(6, 5), 90);
            ending = app.Session.Snapshot(); ending.chances = 1;
            ending.pursuers[0].life = PursuerLife.Active; ending.pursuers[0].from = new Cell(6, 5);
            ending.pursuers[0].to = new Cell(7, 5); ending.pursuers[0].progress = 0; ending.pursuers[0].stunUntil = 0;
            app.Session.Restore(ending); app.Session.Resume(0);
            deadline = Time.realtimeSinceStartup + 4;
            while (app.Session.Phase != SessionPhase.GameOver && Time.realtimeSinceStartup < deadline) yield return null;
            yield return null;
            Require(app.Session.Phase == SessionPhase.GameOver && app.Session.Chances == 0 && GameObject.Find("Game Over") && !app.HasSavedGame,
                "Final capture did not show Game Over and clear the saved run.");
            RequireVisibleText("SLUT FOR NU\nPoint: " + app.Session.Score.ToString("N0") + " · Level " + app.Session.Level + "\nRekord: " + app.Settings.highScore.ToString("N0"));
            yield return new WaitForEndOfFrame(); Capture("10-game-over.png");
            Vector2 replay = ButtonPosition("SPIL IGEN"); Touch(18, TouchPhase.Began, replay);
            yield return new WaitForSecondsRealtime(0.1f); Touch(18, TouchPhase.Ended, replay);
            yield return new WaitForSecondsRealtime(3.6f);
            Require(app.Session.Phase == SessionPhase.Playing && app.Session.Level == 1 && app.Session.Score == 0 && app.Session.Chances == 3 && app.Session.RemainingHearts == 244,
                "Replay did not reset the game.");
            RequireHud(app.Session);
            yield return new WaitForSecondsRealtime(0.3f);
            foreach (string name in new[] { "01-menu.png", "02-gameplay.png", "03-settings.png", "04-left-handed.png", "05-items.png", "06-shortcut-warning.png", "07-shortcut-open.png", "08-base.png", "09-level-clear.png", "10-game-over.png", "11-banana-flight.png", "12-banana-landed.png", "13-banana-reduced-motion.png", "14-putin-chase.png", "15-putin-slip.png", "16-putin-power.png", "17-putin-return.png", "18-putin-profile.png", "19-putin-back.png" })
                Require(File.Exists(Path.Combine(output, name)), "Screenshot was not written: " + name);
            InputSystem.RemoveDevice(screen);
        }
        private IEnumerator CheckCharacter(GameApp app)
        {
            bool originalMotion = app.Settings.reducedMotion; app.Settings.reducedMotion = false;
            InstallFixture(app, ItemKind.None, new Point2(6, 5), 90);
            SessionSnapshot fixture = app.Session.Snapshot();
            // Clear only the review corridor's hearts so they do not obscure the model.
            fixture.remainingHearts = 0;
            for (int i = 0; i < fixture.collected.Length; i++)
            {
                Cell cell = app.Session.Maze.Hearts[i];
                if (cell.y == 5 && cell.x >= 4 && cell.x <= 11) fixture.collected[i] = true;
                if (!fixture.collected[i]) fixture.remainingHearts++;
            }
            PursuerState putin = fixture.pursuers[0]; putin.life = PursuerLife.Active;
            putin.from = new Cell(10, 5); putin.to = new Cell(9, 5); putin.progress = .4;
            putin.releaseAt = putin.stunUntil = putin.oilUntil = 0;
            app.Session.Restore(fixture); app.Session.Resume(0);
            yield return new WaitForSecondsRealtime(.2f);
            Transform avatar = GameObject.Find("Putin imported pilot").transform;
            SkinnedMeshRenderer skin = avatar.GetComponentInChildren<SkinnedMeshRenderer>();
            Require(skin && skin.bones.Length >= 17 && skin.sharedMesh, "Imported Putin rig/skin is missing.");
            Require(skin.bounds.size.y > 1.6f && skin.bounds.size.y < 2.4f, "Putin world scale is incorrect: " + skin.bounds.size);
            Animation animation = skin.GetComponentInParent<Animation>();
            foreach (string clip in new[] { "Idle", "Chase", "Surprise", "Slip", "Return" })
                Require(animation && animation[clip] != null && animation[clip].length > .1f, "Missing Putin clip " + clip);
            Transform facing = Array.Find(avatar.GetComponentsInChildren<Transform>(), t => t.name == "FacingForward");
            Vector3 forward = (facing.position - animation.transform.position).normalized;
            Require(Vector3.Dot(forward, avatar.forward) > .95f, "Putin faces away from his movement direction.");
            Mesh baked = new Mesh(); skin.BakeMesh(baked); Vector3[] before = baked.vertices;
            yield return new WaitForSecondsRealtime(.14f);
            skin.BakeMesh(baked);
            Require(VertexDifference(before, baked.vertices) > .01f, "Chase clip did not deform the model.");
            Transform leftFoot = Array.Find(skin.bones, b => b.name == "Foot.L");
            Transform rightFoot = Array.Find(skin.bones, b => b.name == "Foot.R");
            Require(leftFoot && rightFoot, "Missing foot bones for ground check.");
            float contactError = 0; int contactSamples = 0; bool liftedFoot = false;
            float end = Time.realtimeSinceStartup + .35f;
            while (Time.realtimeSinceStartup < end)
            {
                yield return new WaitForEndOfFrame();
                if (animation["Chase"].weight < .5f) continue;
                float left = leftFoot.position.y - avatar.position.y, right = rightFoot.position.y - avatar.position.y;
                contactError = Mathf.Max(contactError, Mathf.Abs(Mathf.Min(left, right) - .12f));
                Require(left > .105f && right > .105f, "Imported chase foot penetrated the floor.");
                liftedFoot |= Mathf.Max(left, right) > .16f; contactSamples++;
            }
            Require(contactSamples >= 4 && contactError < .015f && liftedFoot,
                "Imported gait lost planted/swing foot contact: samples=" + contactSamples + ", error=" + contactError);
            File.WriteAllText(Path.Combine(output, "character-check.json"), JsonUtility.ToJson(new CharacterCheck {
                groundSamples = contactSamples, maxAnkleContactErrorMeters = contactError, swingFootObserved = liftedFoot }, true));
            yield return new WaitForEndOfFrame(); Capture("14-putin-chase.png");
            app.Session.Pause();
            yield return null;
            skin.BakeMesh(baked); before = baked.vertices; Vector3 root = avatar.position;
            yield return new WaitForSecondsRealtime(.2f);
            skin.BakeMesh(baked);
            Require(VertexDifference(before, baked.vertices) < .00001f && Vector3.Distance(root, avatar.position) < .00001f,
                "Paused Putin skeleton or root moved.");
            app.Session.Resume(0);
            fixture = app.Session.Snapshot(); fixture.pursuers[0].stunUntil = fixture.time + 4;
            app.Session.Restore(fixture); app.Session.Resume(0);
            yield return new WaitForSecondsRealtime(.15f);
            skin.BakeMesh(baked); Vector3[] slip = baked.vertices;
            Require(VertexDifference(before, slip) > .01f, "Slip did not change the skeletal pose.");
            yield return new WaitForEndOfFrame(); Capture("15-putin-slip.png");
            app.Settings.reducedMotion = true;
            yield return null;
            skin.BakeMesh(baked); before = baked.vertices;
            yield return new WaitForSecondsRealtime(.15f);
            skin.BakeMesh(baked);
            Require(VertexDifference(before, baked.vertices) < .00001f, "Reduced-motion slip still wobbled.");
            fixture = app.Session.Snapshot(); fixture.pursuers[0].stunUntil = 0;
            fixture.powerUntil = fixture.time + 5; fixture.powerId++;
            app.Settings.reducedMotion = false;
            app.Session.Restore(fixture); app.Session.Resume(0);
            yield return new WaitForSecondsRealtime(.15f);
            MaterialPropertyBlock tint = new MaterialPropertyBlock(); bool blueSuit = false;
            for (int i = 0; i < skin.sharedMaterials.Length; i++)
                if (skin.sharedMaterials[i].name == "Putin-SuitNavy")
                { skin.GetPropertyBlock(tint, i); blueSuit = tint.GetColor("_BaseColor").b > .8f; }
            Require(blueSuit, "Power did not tint Putin's suit.");
            yield return new WaitForEndOfFrame(); Capture("16-putin-power.png");
            fixture = app.Session.Snapshot(); fixture.pursuers[0].life = PursuerLife.Returning;
            app.Session.Restore(fixture); app.Session.Resume(0);
            yield return new WaitForSecondsRealtime(.1f);
            Require(Math.Abs(avatar.localScale.x - .38f) < .001f && avatar.Find("Return bubble").gameObject.activeSelf,
                "Return did not miniaturise the imported model with its bubble.");
            yield return new WaitForEndOfFrame(); Capture("17-putin-return.png");
            // Held, controlled idle poses provide comparable native profile/back views.
            InstallFixture(app, ItemKind.None, new Point2(4, 5), 90);
            fixture = app.Session.Snapshot(); putin = fixture.pursuers[0];
            putin.life = PursuerLife.Recovering; putin.recoverUntil = fixture.time + 120;
            putin.stunUntil = putin.oilUntil = 0;
            putin.from = new Cell(6, 4); putin.to = new Cell(6, 5); putin.progress = 1;
            app.Session.Restore(fixture); app.Session.Resume(0);
            yield return new WaitForSecondsRealtime(.1f);
            Require(animation["Idle"].weight > .5f, "Recovered character kept its earlier surprise pose.");
            yield return new WaitForEndOfFrame(); Capture("18-putin-profile.png");
            fixture = app.Session.Snapshot(); fixture.pursuers[0].from = new Cell(5, 5);
            app.Session.Restore(fixture); app.Session.Resume(0);
            yield return new WaitForSecondsRealtime(.1f);
            yield return new WaitForEndOfFrame(); Capture("19-putin-back.png");
            app.Settings.reducedMotion = originalMotion; Destroy(baked);
        }
        [Serializable] private sealed class CharacterCheck
        {
            public int groundSamples;
            public float maxAnkleContactErrorMeters;
            public bool swingFootObserved;
        }
        private static float VertexDifference(Vector3[] first, Vector3[] second)
        {
            Require(first.Length == second.Length, "Skinned vertex count changed.");
            float distance = 0;
            for (int i = 0; i < first.Length; i++) distance = Mathf.Max(distance, Vector3.Distance(first[i], second[i]));
            return distance;
        }
        private static void InstallFixture(GameApp app, ItemKind item, Point2 position, double yaw)
        {
            SessionSnapshot state = app.Session.Snapshot(); state.phase = state.resumePhase = SessionPhase.Playing;
            state.phaseRemaining = state.savedPhaseRemaining = 0; state.player = position; state.yaw = yaw; state.pitch = 0;
            state.inventory = item; state.boostUntil = state.powerUntil = state.time;
            state.trail = new[] { position - Point2.Forward(yaw) * 0.5, position }; state.traps = Array.Empty<TrapState>();
            for (int i = 0; i < state.itemAvailable.Length; i++) state.itemAvailable[i] = false;
            foreach (PursuerState pursuer in state.pursuers) { pursuer.releaseAt = state.time + 120; pursuer.stunUntil = state.time + 120; }
            foreach (WallState wall in state.walls) { wall.phase = WallPhase.Closed; wall.phaseUntil = 0; }
            state.nextWallAttempt = state.time + 999;
            app.Session.Restore(state); app.Session.Resume(0);
        }
        private void Touch(int id, TouchPhase phase, Vector2 position)
        {
            InputSystem.QueueStateEvent(screen, new TouchState { touchId = id, phase = phase, position = position, pressure = 1 });
        }
        private IEnumerator ChangeHandedness(GameApp app, bool left)
        {
            if (app.Settings.leftHanded == left) yield break;
            app.TogglePause();
            yield return new WaitForSecondsRealtime(0.1f); // HUD constructs the pause controls on its next update.
            foreach (string label in new[] { "INDSTILLINGER", "Venstrehåndslayout:", "TILBAGE" })
            {
                Vector2 button = label.EndsWith(":", StringComparison.Ordinal) ? ButtonPosition("", label) : ButtonPosition(label);
                Touch(200, TouchPhase.Began, button);
                yield return new WaitForSecondsRealtime(0.1f); Touch(200, TouchPhase.Ended, button);
                yield return new WaitForSecondsRealtime(0.1f);
            }
            Require(app.Settings.leftHanded == left, "Handedness did not change through the settings UI.");
            app.TogglePause(); // Also dismiss the restored-game toast as a real resume does.
        }
        private static Vector2 ButtonPosition(string name, string textPrefix = null)
        {
            foreach (Button button in FindObjectsByType<Button>(FindObjectsSortMode.None))
                if (button.interactable && (textPrefix != null ? button.GetComponentInChildren<Text>().text.StartsWith(textPrefix, StringComparison.Ordinal) : button.name == name))
                    return RectTransformUtility.WorldToScreenPoint(null, button.transform.position);
            throw new InvalidOperationException("Missing active button: " + (textPrefix ?? name));
        }
        private void Capture(string name)
        {
            // CaptureScreenshot(filename) prepends Documents on iOS even for absolute paths.
            // Write the actual framebuffer texture ourselves to keep the destination portable.
            Texture2D capture = ScreenCapture.CaptureScreenshotAsTexture();
            try { File.WriteAllBytes(Path.Combine(output, name), capture.EncodeToPNG()); }
            finally { Destroy(capture); }
        }
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        private static void RequireHud(GameSession session)
        {
            RequireVisibleText("POINT " + session.Score.ToString("N0") + "\nLEVEL " + session.Level);
            RequireVisibleText("Chancer × " + session.Chances + "\nHjerter tilbage: " + session.RemainingHearts);
        }
        private static void RequireVisibleText(string expected)
        {
            foreach (Text text in FindObjectsByType<Text>(FindObjectsSortMode.None)) if (text.text == expected) return;
            throw new InvalidOperationException("HUD did not refresh: " + expected);
        }
        private void OnDestroy() => Application.logMessageReceived -= OnLog;
    }
}
#endif
