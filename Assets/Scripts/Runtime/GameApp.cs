using System;
using System.IO;
using System.Threading.Tasks;
using MazeOfEgos.Core;
using Newtonsoft.Json;
using UnityEngine;

namespace MazeOfEgos.Runtime
{
    [Serializable]
    public sealed class GameSettings
    {
        public int version = 1;
        public bool sound = true, music = true, haptics = true, reducedMotion, leftHanded, economyGraphics;
        public bool tutorialSeen, powerHelpSeen, itemHelpSeen;
        public float fov = 80;
        public long highScore;
    }

    public sealed class GameApp : MonoBehaviour
    {
        public GameSettings Settings { get; private set; }
        public GameSession Session { get; private set; }
        public bool HasSavedGame => savedGame != null;
        private MazeDefinition definition;
        private RuleProfile rules;
        private MazeView view;
        private GameHud hud;
        private GameAudio audioView;
        private DesktopActions input;
        private readonly JoystickTurnLatch turnInput = new JoystickTurnLatch();
        private bool inMenu = true;
        private SessionSnapshot savedGame;
        private string runPath, settingsPath;
        private double accumulator;
        private Point2 previousPlayer;
        private readonly Point2[] previousGhosts = new Point2[4];
        private Task settingsWrite = Task.CompletedTask;
        private readonly object writeGate = new object();
        private volatile string saveFailure;
        private static readonly JsonSerializerSettings Json = new JsonSerializerSettings {
            TypeNameHandling = TypeNameHandling.None, MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            MaxDepth = 64, ObjectCreationHandling = ObjectCreationHandling.Replace };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!FindFirstObjectByType<GameApp>()) new GameObject("Maze of Egos application").AddComponent<GameApp>();
        }
        private void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToLandscapeLeft = Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = Screen.autorotateToPortraitUpsideDown = false;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            string storage = Application.persistentDataPath;
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (DevelopmentSmoke.Enabled) storage = DevelopmentSmoke.StorageDirectory;
            if (DevelopmentLifecycle.Enabled) storage = DevelopmentLifecycle.StorageDirectory;
#endif
#if DEVELOPMENT_BUILD || UNITY_EDITOR || MAZE_PERFORMANCE
            if (DevelopmentPerformance.Enabled) storage = DevelopmentPerformance.StorageDirectory;
#endif
            runPath = Path.Combine(storage, "session-v1.json");
            settingsPath = Path.Combine(storage, "settings-v1.json");
            Settings = ReadSettings();
            GraphicsProfile.Apply(Settings.economyGraphics);
            TextAsset data = Resources.Load<TextAsset>("Data/classic_reference");
            if (!data) throw new InvalidOperationException("Missing classic_reference.json.");
            definition = JsonConvert.DeserializeObject<MazeDefinition>(data.text, Json);
            TextAsset profile = Resources.Load<TextAsset>("Data/rules");
            rules = profile ? JsonConvert.DeserializeObject<RuleProfile>(profile.text, Json) : new RuleProfile();
            Session = new GameSession(new MazeMap(definition), rules: rules);
            LoadSavedGame(); CopyPrevious();
            view = new MazeView(this, Session.Maze); audioView = new GameAudio(this); hud = new GameHud(this);
            input = new DesktopActions();
        }
        private GameSettings ReadSettings()
        {
            return AtomicFile.TryRead(settingsPath, text => {
                try
                {
                    GameSettings result = JsonConvert.DeserializeObject<GameSettings>(text, Json);
                    if (result == null || result.version != 1 || result.highScore < 0 || float.IsNaN(result.fov) || result.fov < 70 || result.fov > 95)
                        throw new FormatException("Invalid settings.");
                    return result;
                }
                catch (JsonException e) { throw new FormatException("Unreadable settings.", e); }
            }, out GameSettings settings) ? settings : new GameSettings();
        }
        private void LoadSavedGame()
        {
            AtomicFile.TryRead(runPath, text => {
                try
                {
                    SessionSnapshot result = JsonConvert.DeserializeObject<SessionSnapshot>(text, Json);
                    var validator = new GameSession(new MazeMap(definition)); validator.Restore(result);
                    if (result.chances == 0 || result.phase == SessionPhase.GameOver) throw new FormatException("Finished session.");
                    return result;
                }
                catch (JsonException e) { throw new FormatException("Unreadable session.", e); }
            }, out savedGame);
        }
        public void NewGame()
        {
            Session = new GameSession(Session.Maze, unchecked((uint)DateTime.UtcNow.Ticks), rules);
            inMenu = false; accumulator = 0; CopyPrevious(); hud.CloseSettings(); view.ResetEncounters();
            hud.Joystick.Clear(); turnInput.Clear(); SaveRun();
            if (!Settings.tutorialSeen)
            {
                hud.Notify("Collect all hearts.\nYou move automatically. Use the stick to choose your next turn.\nDown: turn around · Release: keep moving", 9);
                Settings.tutorialSeen = true; SaveSettings();
            }
        }
        public void ContinueSaved()
        {
            if (savedGame == null) return;
            try
            {
                Session.Restore(savedGame); inMenu = false; accumulator = 0; CopyPrevious(); view.ResetEncounters();
                hud.Notify("Your game is restored. Tap Resume when you are ready.", 4);
            }
            catch (ArgumentException) { savedGame = null; hud.Notify("Your saved game could not be restored. Your best score is safe.", 6, showInMenu: true); }
        }
        public void TogglePause()
        {
            if (hud.SettingsOpen) { hud.CloseSettings(); return; }
            if (inMenu) return;
            if (Session.Phase == SessionPhase.GameOver) { GoToMenu(); return; }
            if (Session.Phase == SessionPhase.Paused) { Session.Resume(); hud.Notify("", 0); }
            else { Session.Pause(); SaveRun(); }
            hud.Joystick.Clear(); turnInput.Clear(); accumulator = 0; CopyPrevious();
        }
        public void GoToMenu()
        {
            if (Session.Phase != SessionPhase.GameOver) { Session.Pause(); SaveRun(); }
            inMenu = true; hud.CloseSettings(); hud.Joystick.Clear(); turnInput.Clear(); accumulator = 0;
        }
        public void UseItem()
        {
            if (inMenu) return;
            if (Session.UseItem()) HandleEvents();
            else if (Session.Inventory != ItemKind.None) hud.Notify("There is no room here. Try another spot.", 2);
        }
        private void Update()
        {
            if (Session == null || hud == null) return;
            float delta = Mathf.Min(UnityEngine.Time.unscaledDeltaTime, 0.15f);
#if DEVELOPMENT_BUILD || UNITY_EDITOR || MAZE_PERFORMANCE
            if (DevelopmentPerformance.Enabled) DevelopmentPerformance.Drive(Session, delta);
#endif
            if (input.Pause.WasPressedThisFrame()) TogglePause();
            if (!inMenu && !hud.SettingsOpen && Session.Phase == SessionPhase.Playing)
            {
                Vector2 stick = hud.Joystick.Movement + input.Move.ReadValue<Vector2>();
                RelativeTurn? turn = turnInput.Read(new Point2(stick.x, stick.y));
#if DEVELOPMENT_BUILD || UNITY_EDITOR || MAZE_PERFORMANCE
                if (!DevelopmentPerformance.Enabled && turn.HasValue) Session.RequestTurn(turn.Value);
#else
                if (turn.HasValue) Session.RequestTurn(turn.Value);
#endif
                if (input.Use.WasPressedThisFrame()) UseItem();
            }
            else turnInput.Clear();
            if (!inMenu && Session.Phase != SessionPhase.Paused && Session.Phase != SessionPhase.GameOver)
            {
                accumulator += delta;
                while (accumulator >= GameSession.FixedDelta && Session.Phase != SessionPhase.GameOver)
                {
                    CopyPrevious(); Session.Tick(); HandleEvents(); accumulator -= GameSession.FixedDelta;
                }
            }
            view.Render(Session, previousPlayer, previousGhosts, Mathf.Clamp01((float)(accumulator / GameSession.FixedDelta)), !inMenu);
            audioView.Update(Session, inMenu || Session.Phase != SessionPhase.Playing);
            hud.Update(Session, inMenu);
            if (saveFailure != null)
            {
                Debug.LogWarning("Save failed: " + saveFailure); saveFailure = null;
                hud.Notify("Your progress could not be saved. Please try again.", 6, showInMenu: true);
            }
        }
        private void CopyPrevious()
        {
            previousPlayer = Session.Player;
            for (int i = 0; i < 4; i++) previousGhosts[i] = Session.Pursuers[i].Position(Session.Maze);
        }
#if DEVELOPMENT_BUILD || UNITY_EDITOR || MAZE_PERFORMANCE
        internal void SynchronizeAutomationPresentation() { accumulator = 0; CopyPrevious(); view.ResetCamera(); }
        internal void ProcessAutomationEvents() => HandleEvents();
#endif
        private void HandleEvents()
        {
            foreach (GameEvent e in Session.Events)
            {
                audioView.Event(e);
                if (e.Kind == EventKind.PursuerPaused) hud.Notify("Time out! +" + e.Points, 1.2f);
                else if (e.Kind == EventKind.ExtraChance) hud.Notify("An extra life!", 3);
                else if (e.Kind == EventKind.ItemPicked && !Settings.itemHelpSeen)
                { hud.Notify("Tap the item button to use what you found.", 5); Settings.itemHelpSeen = true; SaveSettings(); }
                else if (e.Kind == EventKind.PowerHeart && !Settings.powerHelpSeen)
                { hud.Notify("The chase is reversed! Send the surprised pursuers back for a break.", 4); Settings.powerHelpSeen = true; SaveSettings(); }
                else if (e.Kind == EventKind.WallWarning) hud.Notify("A shortcut will open soon", 2);
                else if (e.Kind == EventKind.LevelClear) SaveRun();
                if (Settings.haptics) GameHaptics.Play(e.Kind);
            }
            if (Session.Score > Settings.highScore) { Settings.highScore = Session.Score; SaveSettings(); }
            if (Session.Phase == SessionPhase.GameOver) ClearRun();
            Session.Events.Clear();
        }
        public void Encounter(int index)
        {
            string[] names = { "Putin", "Xi", "Kim", "Trump" };
            hud.Encounter(names[index]); audioView.Encounter(index);
        }
        public void SaveSettings()
        {
            if (Settings == null) return;
            string text = JsonConvert.SerializeObject(Settings, Json);
            // Serial background writes keep record changes off the rendering thread.
            lock (writeGate)
                settingsWrite = settingsWrite.ContinueWith(_ => {
                    try { AtomicFile.Write(settingsPath, text); }
                    catch (Exception e) when (e is IOException || e is UnauthorizedAccessException) { saveFailure = e.Message; }
                }, TaskScheduler.Default);
        }
        private void SaveRun()
        {
            if (Session == null || Session.Phase == SessionPhase.GameOver || Session.Chances == 0) return;
            try { savedGame = Session.Snapshot(); AtomicFile.Write(runPath, JsonConvert.SerializeObject(savedGame, Json)); }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException) { saveFailure = e.Message; }
            SaveSettings();
        }
        private void ClearRun()
        {
            savedGame = null;
            foreach (string suffix in new[] { "", ".bak", ".tmp" })
                try { File.Delete(runPath + suffix); }
                catch (IOException e) { saveFailure = e.Message; }
        }
        private void OnApplicationPause(bool paused) { if (paused) Checkpoint(); }
        private void OnApplicationFocus(bool focus)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR || MAZE_PERFORMANCE
            if (DevelopmentPerformance.Enabled && !Application.isMobilePlatform) return;
#endif
            if (!focus) Checkpoint();
        }
        private void Checkpoint()
        {
            if (Session == null || Settings == null) return;
            if (!inMenu && Session.Phase != SessionPhase.GameOver) { Session.Pause(); SaveRun(); }
            SaveSettings(); lock (writeGate) settingsWrite.GetAwaiter().GetResult();
        }
        private void OnApplicationQuit() => Checkpoint();
        private void OnDestroy() { input?.Dispose(); }
    }
}
