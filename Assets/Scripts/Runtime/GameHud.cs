using System;
using MazeOfEgos.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace MazeOfEgos.Runtime
{
    public sealed class GameHud
    {
        public TouchControls Joystick { get; private set; }
        public TouchControls Look { get; private set; }
        private readonly GameApp app;
        private readonly Font font;
        private readonly RectTransform safe;
        private readonly GameObject menu, controls, pause, settings, results;
        private readonly Text stats, hearts, center, effects, toast, danger, record, prototype;
        private readonly Button itemButton, resumeButton;
        private readonly Text itemText;
        private readonly TouchArtwork itemIcon;
        private readonly Image scare;
        private readonly Text resultText;
        public bool SettingsOpen => settings.activeSelf;
        public void CloseSettings() => settings.SetActive(false);
        private float toastUntil, scareUntil;
        private Rect lastSafeArea;
        private int lastScreenWidth, lastScreenHeight;
        private long shownScore = -1, shownRecord = -1;
        private int shownLevel = -1, shownChances = -1, shownHearts = -1, shownPower = -1, shownBoost = -1, shownCountdown = -1;
        private SessionPhase shownPhase = (SessionPhase)(-1);
        private readonly Color ink = new Color(0.95f, 0.94f, 0.86f);
        private readonly Color panelColor = new Color(0.055f, 0.10f, 0.12f, 1);

        public GameHud(GameApp app)
        {
            this.app = app; font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var root = new GameObject("Interface", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900); scaler.matchWidthOrHeight = 0.5f;
            if (!UnityEngine.Object.FindFirstObjectByType<EventSystem>())
            {
                var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            safe = Rect("Safe area", root.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            controls = Rect("Gameplay", safe, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject;
            RectTransform lookRect = Rect("Look area", controls.transform, new Vector2(0.5f, 0), Vector2.one, Vector2.zero, Vector2.zero);
            var lookImage = lookRect.gameObject.AddComponent<Image>(); lookImage.color = new Color(0, 0, 0, 0.001f);
            Look = lookRect.gameObject.AddComponent<TouchControls>();
            RectTransform joystickRect = Rect("Movement joystick", controls.transform, Vector2.zero, Vector2.zero, new Vector2(150, 155), new Vector2(300, 300));
            Image joystickImage = joystickRect.gameObject.AddComponent<Image>(); joystickImage.color = new Color(0, 0, 0, 0.001f);
            Joystick = joystickRect.gameObject.AddComponent<TouchControls>(); Joystick.joystick = true;
            RectTransform stickArt = Rect("Joystick artwork", joystickRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(220, 220));
            Joystick.artwork = stickArt.gameObject.AddComponent<CanvasGroup>(); Joystick.artwork.alpha = 0.42f;
            Artwork(stickArt, TouchArtwork.Shape.Base, ink);
            RectTransform knob = Rect("Joystick thumb", stickArt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(76, 76));
            Artwork(knob, TouchArtwork.Shape.Thumb, new Color(1, 0.88f, 0.59f));
            Joystick.knob = knob;
            Label("GÅ", stickArt, new Vector2(0.5f, 0.5f), new Vector2(0, -125), new Vector2(150, 35), 18, TextAnchor.MiddleCenter);
            stats = Label("", controls.transform, new Vector2(0, 1), new Vector2(190, -42), new Vector2(330, 65), 29);
            hearts = Label("", controls.transform, Vector2.one, new Vector2(-320, -42), new Vector2(380, 65), 29, TextAnchor.MiddleRight);
            Button("Ⅱ", controls.transform, Vector2.one, new Vector2(-65, -55), new Vector2(80, 70), app.TogglePause);
            center = Label("", controls.transform, new Vector2(0.5f, 0.5f), new Vector2(0, 70), new Vector2(900, 160), 47, TextAnchor.MiddleCenter);
            effects = Label("", controls.transform, new Vector2(0.5f, 1), new Vector2(0, -45), new Vector2(600, 60), 26, TextAnchor.MiddleCenter);
            danger = Label("", controls.transform, new Vector2(0.5f, 0), new Vector2(0, 135), new Vector2(620, 60), 25, TextAnchor.MiddleCenter);
            RectTransform useRect = Rect("Use item", controls.transform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-150, 155), new Vector2(170, 170));
            TouchArtwork useArt = Artwork(useRect, TouchArtwork.Shape.Button, ink); useArt.raycastTarget = true;
            itemButton = useRect.gameObject.AddComponent<Button>(); itemButton.targetGraphic = useArt;
            var useColors = itemButton.colors; useColors.highlightedColor = new Color(1, 0.88f, 0.59f);
            useColors.pressedColor = new Color(0.6f, 0.8f, 0.75f); useColors.disabledColor = new Color(1, 1, 1, 0.32f); itemButton.colors = useColors;
            itemButton.onClick.AddListener(app.UseItem);
            RectTransform iconRect = Rect("Item icon", useRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(92, 92));
            itemIcon = Artwork(iconRect, TouchArtwork.Shape.Empty, ink);
            itemText = Label("GENSTAND", useRect, new Vector2(0.5f, 0.5f), new Vector2(0, -115), new Vector2(240, 45), 19, TextAnchor.MiddleCenter);
            toast = Label("", safe, new Vector2(0.5f, 0), new Vector2(0, 60), new Vector2(900, 90), 27, TextAnchor.MiddleCenter);
            menu = Panel("Main menu", safe, panelColor);
            Label("MAZE OF EGOS", menu.transform, new Vector2(0.5f, 1), new Vector2(0, -110), new Vector2(1000, 100), 68, TextAnchor.MiddleCenter);
            Label("Saml hjerter. Undvig store egoer.", menu.transform, new Vector2(0.5f, 1), new Vector2(0, -195), new Vector2(950, 70), 29, TextAnchor.MiddleCenter);
            Portrait("zelenskyy", menu.transform, new Vector2(0.5f, 0.5f), new Vector2(-330, 0), new Vector2(225, 260));
            Label("ZELENSKYJ", menu.transform, new Vector2(0.5f, 0.5f), new Vector2(-330, -165), new Vector2(270, 50), 28, TextAnchor.MiddleCenter);
            Button("SPIL", menu.transform, new Vector2(0.5f, 0.5f), new Vector2(45, 60), new Vector2(390, 85), app.NewGame);
            resumeButton = Button("FORTSÆT GEMT SPIL", menu.transform, new Vector2(0.5f, 0.5f), new Vector2(45, -45), new Vector2(390, 75), app.ContinueSaved);
            Button("INDSTILLINGER", menu.transform, new Vector2(0.5f, 0.5f), new Vector2(45, -140), new Vector2(390, 65), () => settings.SetActive(true));
            record = Label("", menu.transform, new Vector2(0.5f, 0.5f), new Vector2(45, 160), new Vector2(400, 50), 25, TextAnchor.MiddleCenter);
            string[] ids = { "putin", "xi", "kim", "trump" };
            string[] names = { "Putin", "Xi", "Kim", "Trump" };
            for (int i = 0; i < 4; i++)
            {
                Portrait(ids[i], menu.transform, new Vector2(1, 0.5f), new Vector2(-180, 175 - i * 115), new Vector2(78, 92));
                Label(names[i], menu.transform, new Vector2(1, 0.5f), new Vector2(-78, 175 - i * 115), new Vector2(110, 50), 24);
            }
            Button("HJÆLP / OM SPILLET", menu.transform, new Vector2(0.5f, 0), new Vector2(0, 85), new Vector2(430, 55), () => {
                Notify("Saml alle hjerter. Joystick: gå · Træk til højre: kig · Knap: brug genstand.\nFiktiv politisk satire uden tilknytning til de afbildede personer.", 12);
                toast.transform.SetAsLastSibling();
            });
            pause = Panel("Pause", safe, panelColor);
            Label("PAUSE", pause.transform, new Vector2(0.5f, 0.5f), new Vector2(0, 190), new Vector2(700, 90), 58, TextAnchor.MiddleCenter);
            Button("FORTSÆT", pause.transform, new Vector2(0.5f, 0.5f), new Vector2(0, 75), new Vector2(420, 80), app.TogglePause);
            Button("INDSTILLINGER", pause.transform, new Vector2(0.5f, 0.5f), new Vector2(0, -25), new Vector2(420, 70), () => settings.SetActive(true));
            Button("NYT SPIL", pause.transform, new Vector2(0.5f, 0.5f), new Vector2(0, -120), new Vector2(420, 70), app.NewGame);
            Button("HOVEDMENU", pause.transform, new Vector2(0.5f, 0.5f), new Vector2(0, -210), new Vector2(420, 65), app.GoToMenu);
            results = Panel("Game Over", safe, panelColor);
            resultText = Label("", results.transform, new Vector2(0.5f, 0.5f), new Vector2(0, 140), new Vector2(900, 220), 45, TextAnchor.MiddleCenter);
            Button("SPIL IGEN", results.transform, new Vector2(0.5f, 0.5f), new Vector2(0, -50), new Vector2(420, 80), app.NewGame);
            Button("HOVEDMENU", results.transform, new Vector2(0.5f, 0.5f), new Vector2(0, -155), new Vector2(420, 70), app.GoToMenu);
            settings = Panel("Settings", safe, panelColor);
            BuildSettings();
            RectTransform flash = Rect("Encounter flash", safe, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            scare = flash.gameObject.AddComponent<Image>(); scare.color = Color.clear; scare.raycastTarget = false;
            // The notice is visible only in development builds, as required for temporary assets.
            if (Debug.isDebugBuild || Application.isEditor)
                prototype = Label("PROTOTYPE · midlertidige 3D-figurer", safe, new Vector2(0.5f, 1), new Vector2(0, -28), new Vector2(700, 35), 17, TextAnchor.MiddleCenter);
            menu.SetActive(true); pause.SetActive(false); results.SetActive(false); settings.SetActive(false); controls.SetActive(false);
            RefreshSafeArea(); ApplyHandedness();
        }

        private void BuildSettings()
        {
            Label("INDSTILLINGER", settings.transform, new Vector2(0.5f, 0.5f), new Vector2(0, 320), new Vector2(900, 80), 48, TextAnchor.MiddleCenter);
            ToggleRow("Lyd", 225, () => app.Settings.sound, value => app.Settings.sound = value);
            ToggleRow("Musik", 145, () => app.Settings.music, value => app.Settings.music = value);
            ToggleRow("Haptik", 65, () => app.Settings.haptics, value => app.Settings.haptics = value);
            ToggleRow("Dæmp forskrækkelser / reduceret bevægelse", -15, () => app.Settings.reducedMotion, value => app.Settings.reducedMotion = value);
            ToggleRow("Venstrehåndslayout", -95, () => app.Settings.leftHanded, value => { app.Settings.leftHanded = value; ApplyHandedness(); });
            Button("SYNSFELT −", settings.transform, new Vector2(0.5f, 0.5f), new Vector2(-180, -185), new Vector2(300, 60), () => { app.Settings.fov = Mathf.Max(70, app.Settings.fov - 5); app.SaveSettings(); Notify("Synsfelt: " + app.Settings.fov + "°"); });
            Button("SYNSFELT +", settings.transform, new Vector2(0.5f, 0.5f), new Vector2(180, -185), new Vector2(300, 60), () => { app.Settings.fov = Mathf.Min(95, app.Settings.fov + 5); app.SaveSettings(); Notify("Synsfelt: " + app.Settings.fov + "°"); });
            Button("TILBAGE", settings.transform, new Vector2(0.5f, 0.5f), new Vector2(0, -290), new Vector2(420, 70), () => { settings.SetActive(false); app.SaveSettings(); });
        }
        private void ToggleRow(string title, float y, Func<bool> read, Action<bool> write)
        {
            Button button = null;
            button = Button(title + ": " + (read() ? "TIL" : "FRA"), settings.transform, new Vector2(0.5f, 0.5f), new Vector2(0, y), new Vector2(840, 60), () => {
                write(!read()); button.GetComponentInChildren<Text>().text = title + ": " + (read() ? "TIL" : "FRA"); app.SaveSettings();
            });
        }
        public void ApplyHandedness()
        {
            bool left = app.Settings.leftHanded;
            RectTransform joy = (RectTransform)Joystick.transform;
            joy.anchorMin = joy.anchorMax = new Vector2(left ? 1 : 0, 0);
            joy.anchoredPosition = new Vector2(left ? -150 : 150, 155);
            RectTransform use = (RectTransform)itemButton.transform;
            use.anchorMin = use.anchorMax = new Vector2(left ? 0 : 1, 0);
            use.anchoredPosition = new Vector2(left ? 150 : -150, 155);
            RectTransform look = (RectTransform)Look.transform;
            look.anchorMin = new Vector2(left ? 0 : 0.5f, 0); look.anchorMax = new Vector2(left ? 0.5f : 1, 1);
            Joystick.Clear(); Look.Clear();
        }
        public void Notify(string message, float seconds = 3)
        { toast.text = message; toastUntil = UnityEngine.Time.unscaledTime + seconds; toast.transform.SetAsLastSibling(); }
        public void Encounter(string name)
        {
            Notify(name + "!", 1.0f);
            if (!app.Settings.reducedMotion) scareUntil = UnityEngine.Time.unscaledTime + 0.25f;
        }
        public void Update(GameSession session, bool inMenu)
        {
            RefreshSafeArea();
            bool isPaused = !inMenu && session.Phase == SessionPhase.Paused;
            menu.SetActive(inMenu); pause.SetActive(isPaused);
            bool gameOver = !inMenu && session.Phase == SessionPhase.GameOver;
            if (prototype) prototype.rectTransform.anchoredPosition = new Vector2(0, inMenu || isPaused || gameOver || settings.activeSelf ? -28 : -105);
            toast.gameObject.SetActive(!settings.activeSelf);
            controls.SetActive(!inMenu && !isPaused && !gameOver && !settings.activeSelf);
            results.SetActive(gameOver);
            bool scoreChanged = shownScore != session.Score || shownLevel != session.Level;
            bool recordChanged = shownRecord != app.Settings.highScore;
            if (scoreChanged || recordChanged)
                resultText.text = "SLUT FOR NU\nPoint: " + session.Score.ToString("N0") + " · Level " + session.Level + "\nRekord: " + app.Settings.highScore.ToString("N0");
            if (recordChanged) { record.text = "Rekord: " + app.Settings.highScore.ToString("N0"); shownRecord = app.Settings.highScore; }
            resumeButton.interactable = app.HasSavedGame;
            if (UnityEngine.Time.unscaledTime > toastUntil) toast.text = "";
            float flash = Mathf.Clamp01((scareUntil - UnityEngine.Time.unscaledTime) / 0.25f);
            scare.color = new Color(1, 0.81f, 0.3f, flash * 0.12f);
            if (scoreChanged) stats.text = "POINT " + session.Score.ToString("N0") + "\nLEVEL " + session.Level;
            if (shownChances != session.Chances || shownHearts != session.RemainingHearts)
            { hearts.text = "Chancer × " + session.Chances + "\nHjerter tilbage: " + session.RemainingHearts; shownChances = session.Chances; shownHearts = session.RemainingHearts; }
            itemText.text = session.Inventory == ItemKind.None ? "GENSTAND" : session.Inventory == ItemKind.BananaPeel ? "BANAN" : session.Inventory == ItemKind.OilSlick ? "OLIE" : "BOOST";
            itemIcon.Artwork = session.Inventory == ItemKind.BananaPeel ? TouchArtwork.Shape.Banana : session.Inventory == ItemKind.OilSlick ? TouchArtwork.Shape.Oil : session.Inventory == ItemKind.SpeedBoost ? TouchArtwork.Shape.Boost : TouchArtwork.Shape.Empty;
            itemIcon.color = session.Inventory == ItemKind.None ? new Color(1, 1, 1, 0.25f) : session.Inventory == ItemKind.BananaPeel ? new Color(1, 0.83f, 0.25f) : session.Inventory == ItemKind.OilSlick ? new Color(0.76f, 0.68f, 0.93f) : new Color(0.51f, 0.95f, 0.82f);
            itemButton.interactable = session.Phase == SessionPhase.Playing && session.Inventory != ItemKind.None;
            int powerTenths = Mathf.Max(0, Mathf.CeilToInt((float)(session.PowerUntil - session.Time) * 10));
            int boostTenths = Mathf.Max(0, Mathf.CeilToInt((float)(session.BoostUntil - session.Time) * 10));
            if (shownPower != powerTenths || shownBoost != boostTenths)
            {
                effects.text = (powerTenths > 0 ? "KRAFTHJERTE  " + (powerTenths / 10f).ToString("F1") + " s" : "") +
                    (boostTenths > 0 ? "   BOOST " + (boostTenths / 10f).ToString("F1") + " s" : "");
                shownPower = powerTenths; shownBoost = boostTenths;
            }
            int countdown = Mathf.CeilToInt((float)session.PhaseRemaining);
            if (shownPhase != session.Phase || (session.Phase == SessionPhase.Countdown && shownCountdown != countdown) ||
                (session.Phase == SessionPhase.GameOver && scoreChanged))
                center.text = session.Phase == SessionPhase.Countdown ? "Saml alle hjerter\n" + countdown :
                session.Phase == SessionPhase.Caught ? "Fanget! Prøv igen" : session.Phase == SessionPhase.LevelClear ? "Alle hjerter samlet!" :
                session.Phase == SessionPhase.GameOver ? "SLUT FOR NU\nPoint: " + session.Score + "\nPause/menu: ESC eller Ⅱ" : "";
            shownScore = session.Score; shownLevel = session.Level; shownPhase = session.Phase; shownCountdown = countdown;
            string dangerText = "";
            if (session.Phase == SessionPhase.Playing)
            {
                double nearest = 4.5; Point2 offset = new Point2();
                foreach (PursuerState p in session.Pursuers) if (session.IsDangerous(p))
                {
                    Point2 difference = session.Maze.Near(p.Position(session.Maze), session.Player) - session.Player;
                    if (difference.Length < nearest) { nearest = difference.Length; offset = difference; }
                }
                if (nearest < 4.5)
                {
                    double side = Point2.Dot(offset.Normalized, Point2.Right(session.Yaw));
                    double ahead = Point2.Dot(offset.Normalized, Point2.Forward(session.Yaw));
                    dangerText = side < -0.4 ? "‹ Fare i nærheden" : side > 0.4 ? "Fare i nærheden ›" : ahead < 0 ? "Fare bag dig" : "Fare i nærheden";
                }
            }
            danger.text = dangerText;
        }
        private void RefreshSafeArea()
        {
            if (lastSafeArea == Screen.safeArea && lastScreenWidth == Screen.width && lastScreenHeight == Screen.height) return;
            lastSafeArea = Screen.safeArea; lastScreenWidth = Screen.width; lastScreenHeight = Screen.height;
            safe.anchorMin = new Vector2(lastSafeArea.xMin / Screen.width, lastSafeArea.yMin / Screen.height);
            safe.anchorMax = new Vector2(lastSafeArea.xMax / Screen.width, lastSafeArea.yMax / Screen.height);
            safe.offsetMin = safe.offsetMax = Vector2.zero;
            Joystick?.Clear(); Look?.Clear();
        }
        private GameObject Panel(string name, Transform parent, Color color)
        {
            RectTransform rect = Rect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            rect.gameObject.AddComponent<Image>().color = color; return rect.gameObject;
        }
        private static TouchArtwork Artwork(RectTransform rect, TouchArtwork.Shape shape, Color tint)
        {
            var graphic = rect.gameObject.AddComponent<TouchArtwork>(); graphic.Artwork = shape;
            graphic.color = tint; graphic.raycastTarget = false; return graphic;
        }
        private Text Label(string value, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            RectTransform rect = Rect(value.Length > 0 ? value : "Label", parent, anchor, anchor, position, size);
            Text text = rect.gameObject.AddComponent<Text>(); text.font = font; text.text = value; text.fontSize = fontSize;
            text.alignment = alignment; text.color = ink; text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Overflow;
            var shadow = rect.gameObject.AddComponent<Shadow>(); shadow.effectColor = new Color(0, 0, 0, 0.65f); shadow.effectDistance = new Vector2(1, -2);
            return text;
        }
        private Button Button(string title, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, Action action)
        {
            RectTransform rect = Rect(title, parent, anchor, anchor, position, size);
            var image = rect.gameObject.AddComponent<Image>(); image.color = new Color(0.20f, 0.35f, 0.35f, 0.95f);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var colors = button.colors; colors.highlightedColor = new Color(1, 0.89f, 0.63f); colors.pressedColor = new Color(0.7f, 0.8f, 0.7f); button.colors = colors;
            button.onClick.AddListener(() => action());
            Label(title, rect, new Vector2(0.5f, 0.5f), Vector2.zero, size - new Vector2(20, 6), 25, TextAnchor.MiddleCenter);
            return button;
        }
        private void Portrait(string id, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var texture = Resources.Load<Texture2D>("Characters/" + id);
            if (!texture) return;
            RectTransform rect = Rect(id + " reference portrait", parent, anchor, anchor, position, size);
            var image = rect.gameObject.AddComponent<RawImage>(); image.texture = texture;
            image.uvRect = new Rect(0.32f, 0.075f, 0.215f, 0.395f); image.raycastTarget = false;
        }
        private static RectTransform Rect(string name, Transform parent, Vector2 minimum, Vector2 maximum, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = minimum; rect.anchorMax = maximum;
            if (minimum == maximum) { rect.sizeDelta = size; rect.anchoredPosition = position; }
            else { rect.offsetMin = rect.offsetMax = Vector2.zero; }
            return rect;
        }
    }
}
