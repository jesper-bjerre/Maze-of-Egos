using System;
using System.IO;
using MazeOfEgos.Core;
using Newtonsoft.Json;
using UnityEngine;

namespace MazeOfEgos.Runtime
{
    public sealed class CompetitionApp : MonoBehaviour
    {
        public GameSettings Settings { get; private set; }
        public CompetitionSession Session { get; private set; }
        public CompetitionHud Hud { get; private set; }
        public CompetitionView View { get; private set; }
        public bool InMenu { get; private set; } = true;
        public bool Selecting { get; private set; }
        public bool HasSavedGame { get; private set; }
        public int BackgroundCount { get; private set; }
        public int ForegroundCount { get; private set; }
        private MazeDefinition definition;
        private CompetitionRules rules;
        private CompetitionSaveStore store;
        private CompetitionSession saved;
        private DesktopActions input;
        private CompetitionAudio sounds;
        private readonly ScreenHeadingInput turns = new ScreenHeadingInput();
        private string settingsPath;
        private double accumulator;
        private readonly Point2[] previous = new Point2[5];
        private static readonly JsonSerializerSettings Json = new JsonSerializerSettings {
            TypeNameHandling = TypeNameHandling.None, ObjectCreationHandling = ObjectCreationHandling.Replace, MaxDepth = 64 };
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot() { if (!FindFirstObjectByType<CompetitionApp>()) new GameObject("Maze of Egos competition").AddComponent<CompetitionApp>(); }
        private void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToLandscapeLeft = Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = Screen.autorotateToPortraitUpsideDown = false;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            string directory = Application.persistentDataPath;
#if DEVELOPMENT_BUILD || UNITY_EDITOR || MAZE_PERFORMANCE
            if (CompetitionAutomation.Enabled) directory = CompetitionAutomation.StorageDirectory;
#endif
            settingsPath = Path.Combine(directory, "settings-v1.json");
            AtomicFile.TryRead(settingsPath, text => {
                try {
                    var value = JsonConvert.DeserializeObject<GameSettings>(text, Json);
                    if (value == null || value.version != 1 || value.highScore < 0 || float.IsNaN(value.fov) || value.fov < 70 || value.fov > 95) throw new FormatException("Invalid settings.");
                    return value;
                } catch (JsonException e) { throw new FormatException("Invalid settings JSON.", e); }
            }, out GameSettings loaded);
            Settings = loaded ?? new GameSettings(); GraphicsProfile.Apply(Settings.economyGraphics);
            definition = JsonConvert.DeserializeObject<MazeDefinition>(Resources.Load<TextAsset>("Data/classic_reference").text, Json);
            TextAsset profile = Resources.Load<TextAsset>("Data/competition_rules");
            rules = profile ? JsonConvert.DeserializeObject<CompetitionRules>(profile.text, Json) : new CompetitionRules(); rules.Validate();
            Session = Create("zelenskyy", 42); store = new CompetitionSaveStore(directory);
            saved = Create("zelenskyy", 42); HasSavedGame = store.TryLoad(saved);
            View = new CompetitionView(this); Hud = new CompetitionHud(this); sounds = new CompetitionAudio(Settings); input = new DesktopActions(); CopyPrevious();
            AudioSettings.OnAudioConfigurationChanged += AudioChanged;
        }
        private CompetitionSession Create(string id, uint seed) => new CompetitionSession(definition, id, seed, rules, true, true);
        public void ChooseCharacter() { Selecting = true; InMenu = true; ClearInput(); Hud.CloseSettings(); }
        public void NewMatch(string id)
        {
            Session = Create(id, unchecked((uint)DateTime.UtcNow.Ticks)); InMenu = Selecting = false;
            ClearInput(); CopyPrevious(); View.ResetPoses(); Save();
        }
        public void ContinueSaved()
        {
            if (!HasSavedGame) return;
            Session = Create(saved.HumanId, 42); Session.Restore(saved.Snapshot()); InMenu = Selecting = false; ClearInput(); CopyPrevious(); View.ResetPoses();
        }
        public void NextRound() { if (Session.StartNextRound()) { ClearInput(); CopyPrevious(); View.ResetPoses(); Save(); } }
        public void TogglePause()
        {
            if (Hud.SettingsOpen) { Hud.CloseSettings(); return; }
            if (InMenu) return;
            if (Session.Phase == CompetitionPhase.Paused) Session.Resume(); else Session.Pause();
            ClearInput(); CopyPrevious(); Save();
        }
        public void GoToMenu()
        {
            if (!InMenu) { Session.Pause(); Save(); }
            InMenu = true; Selecting = false; ClearInput(); Hud.CloseSettings();
        }
        public void UseItem()
        {
            if (InMenu) return;
            if (!Session.UseItem(Session.HumanId, Session.TickIndex) && Session.Actor(Session.HumanIndex).Inventory != CompetitionItem.None)
                Hud.Notify("No room here. Try another spot.");
            HandleEvents();
        }
        private void ClearInput() { accumulator = 0; turns.Clear(); Hud?.Joystick.Clear(); }
        private void CopyPrevious() { for (int i = 0; i < 5; i++) previous[i] = Session.Actor(i).Position; }
        private void Update()
        {
            if (Session == null || Hud == null) return;
            Session.ConfigureView((double)Screen.width / Math.Max(1, Screen.height));
            if (input.Pause.WasPressedThisFrame()) TogglePause();
            if (!InMenu && Session.Phase == CompetitionPhase.Playing)
            {
                Vector2 stick = Hud.Joystick.Movement + input.Move.ReadValue<Vector2>();
                int? heading = turns.Read(new Point2(stick.x, stick.y), View.ScreenDirection(CorridorNavigation.Direction(0)), View.ScreenDirection(CorridorNavigation.Direction(1)));
                if (heading.HasValue) Session.ApplyCommand(new CompetitionCommand(Session.HumanId, Session.TickIndex, heading.Value));
                if (input.Use.WasPressedThisFrame()) UseItem();
            }
            else turns.Clear();
#if DEVELOPMENT_BUILD || UNITY_EDITOR || MAZE_PERFORMANCE
            if (CompetitionAutomation.Enabled) CompetitionAutomation.Drive(this);
#endif
            if (!InMenu && (Session.Phase == CompetitionPhase.Playing || Session.Phase == CompetitionPhase.Countdown))
            {
                accumulator += Math.Min(Time.unscaledDeltaTime, .15f);
                while (accumulator >= CompetitionSession.FixedDelta)
                {
                    CopyPrevious();
#if DEVELOPMENT_BUILD || UNITY_EDITOR || MAZE_PERFORMANCE
                    if (CompetitionAutomation.Performance) Session.DriveHumanForAutomation();
#endif
                    Session.Tick(); HandleEvents(); accumulator -= CompetitionSession.FixedDelta;
                    if (Session.Phase != CompetitionPhase.Playing && Session.Phase != CompetitionPhase.Countdown) { accumulator = 0; break; }
                }
            }
            View.Render(Session, previous, (float)(accumulator / CompetitionSession.FixedDelta), !InMenu);
            Hud.Render(); sounds.Update();
        }
        private void HandleEvents()
        {
            bool save = false;
            foreach (var e in Session.Events)
            {
                bool own = e.Actor == Session.HumanId;
                if (own || e.Kind == CompetitionEventKind.RoundEnded || e.Kind == CompetitionEventKind.MatchEnded ||
                    LocalArenaView.Contains(Session.Maze.Near(Session.Participant(e.Actor).position, Session.Actor(Session.HumanIndex).Position) - Session.Actor(Session.HumanIndex).Position, (double)Screen.width / Screen.height)) sounds.Event(e.Kind, own);
                if (own && e.Kind == CompetitionEventKind.ItemPicked && !Settings.itemHelpSeen) { Hud.Notify("Tap your item to use it. The next box replaces it.", 5); Settings.itemHelpSeen = true; SaveSettings(); }
                if (own && Settings.haptics && e.Kind == CompetitionEventKind.Knockout) GameHaptics.Play(EventKind.Caught);
                if (e.Kind == CompetitionEventKind.RoundEnded || e.Kind == CompetitionEventKind.MatchEnded) save = true;
            }
            Session.Events.Clear(); if (save) Save();
        }
        public void SaveSettings()
        {
            try { AtomicFile.Write(settingsPath, JsonConvert.SerializeObject(Settings, Json)); }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException) { Hud?.Notify("Settings could not be saved. Please try again."); Debug.LogWarning(e.Message); }
        }
        public void Save()
        {
            try { store.Save(Session); saved = Create(Session.HumanId, 42); saved.Restore(Session.Snapshot()); HasSavedGame = true; SaveSettings(); }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException) { Hud?.Notify("Your match could not be saved. Please try again."); Debug.LogWarning(e.Message); }
        }
        private void OnApplicationPause(bool paused)
        {
            if (paused) { BackgroundCount++; if (!InMenu) { Session.Pause(); ClearInput(); CopyPrevious(); Save(); } }
            else { ForegroundCount++; PlatformAudio.Configure(); }
        }
        private void OnApplicationFocus(bool focused)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR || MAZE_PERFORMANCE
            if (CompetitionAutomation.Enabled) return;
#endif
            if (!focused && !Application.isMobilePlatform && !InMenu) { Session.Pause(); ClearInput(); Save(); }
            if (focused) PlatformAudio.Configure();
        }
        private void OnApplicationQuit() { if (!InMenu) { Session.Pause(); Save(); } }
        private void AudioChanged(bool changed) => PlatformAudio.Configure();
        private void OnDestroy() { input?.Dispose(); AudioSettings.OnAudioConfigurationChanged -= AudioChanged; }
#if DEVELOPMENT_BUILD || UNITY_EDITOR || MAZE_PERFORMANCE
        public void RestoreForCheck(CompetitionSnapshot state) { Session.Restore(state); Session.Resume(false); InMenu = Selecting = false; ClearInput(); CopyPrevious(); View.ResetPoses(); }
#endif
    }
}
