using System;
using System.Globalization;
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
        private readonly GameApp app;
        private readonly Font font;
        private readonly RectTransform safe;
        private readonly GameObject menu, controls, pause, settings, results, help, backdrop;
        private readonly HeroPresentation hero;
        private readonly RawImage celebration, menuArtwork;
        private readonly RectTransform backdropRect;
        private readonly MenuEdgeFade menuFade;
        private readonly Text stats, hearts, center, effects, toast, danger, prototype;
        private readonly Button itemButton, resumeButton;
        private readonly Text itemText, livesLabel, resumeText;
        private readonly TouchArtwork[] lifeHearts = new TouchArtwork[4];
        private readonly TouchArtwork itemIcon;
        private readonly Image scare;
        private readonly Text resultText;
        public bool SettingsOpen => settings.activeSelf;
        public void CloseSettings() => settings.SetActive(false);
        private float toastUntil, scareUntil;
        private bool menuNotice;
        private Rect lastSafeArea;
        private int lastScreenWidth, lastScreenHeight;
        private long shownScore = -1, shownRecord = -1;
        private int shownLevel = -1, shownChances = -1, shownHearts = -1, shownPower = -1, shownBoost = -1, shownCountdown = -1;
        private SessionPhase shownPhase = (SessionPhase)(-1);
        private readonly Color ink = new Color(0.95f, 0.94f, 0.86f);
        private readonly Color panelColor = new Color(0.055f, 0.10f, 0.12f, 1);
        private readonly Color coverColor = new Color(.012f, .039f, .063f, 1);

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
            backdrop = Panel("Menu backdrop", root.transform, panelColor);
            backdropRect = (RectTransform)backdrop.transform;
            RectTransform artworkRect = Rect("Menu artwork", backdrop.transform, new Vector2(0, .5f), new Vector2(0, .5f), Vector2.zero, Vector2.zero);
            artworkRect.pivot = new Vector2(0, .5f);
            menuArtwork = artworkRect.gameObject.AddComponent<RawImage>();
            menuArtwork.texture = Resources.Load<Texture2D>("Brand/MenuBackground");
            menuArtwork.raycastTarget = false;
            RectTransform fade = Rect("Cover right edge", backdrop.transform, new Vector2(0, .5f), new Vector2(0, .5f), Vector2.zero, Vector2.zero);
            fade.pivot = new Vector2(0, .5f);
            menuFade = fade.gameObject.AddComponent<MenuEdgeFade>(); menuFade.color = coverColor; menuFade.raycastTarget = false;
            safe = Rect("Safe area", root.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            controls = Rect("Gameplay", safe, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject;
            RectTransform joystickRect = Rect("Movement joystick", controls.transform, Vector2.zero, Vector2.zero, new Vector2(150, 155), new Vector2(300, 300));
            Image joystickImage = joystickRect.gameObject.AddComponent<Image>(); joystickImage.color = new Color(0, 0, 0, 0.001f);
            Joystick = joystickRect.gameObject.AddComponent<TouchControls>(); Joystick.joystick = true;
            RectTransform stickArt = Rect("Joystick artwork", joystickRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(220, 220));
            Joystick.artwork = stickArt.gameObject.AddComponent<CanvasGroup>(); Joystick.artwork.alpha = 0.42f;
            Artwork(stickArt, TouchArtwork.Shape.Base, ink);
            RectTransform knob = Rect("Joystick thumb", stickArt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(76, 76));
            Artwork(knob, TouchArtwork.Shape.Thumb, new Color(1, 0.88f, 0.59f));
            Joystick.knob = knob;
            Label("MOVE", stickArt, new Vector2(0.5f, 0.5f), new Vector2(0, -125), new Vector2(150, 35), 18, TextAnchor.MiddleCenter);
            stats = Label("", controls.transform, new Vector2(0, 1), new Vector2(190, -42), new Vector2(330, 65), 29);
            hearts = Label("", controls.transform, Vector2.one, new Vector2(-320, -65), new Vector2(380, 35), 29, TextAnchor.MiddleRight);
            RectTransform lives = Rect("Life hearts", controls.transform, Vector2.one, Vector2.one, new Vector2(-320, -26), new Vector2(380, 38));
            livesLabel = Label("Lives:", lives, new Vector2(1, .5f), Vector2.zero, new Vector2(80, 38), 29, TextAnchor.MiddleRight);
            for (int i = 0; i < lifeHearts.Length; i++)
                lifeHearts[i] = Artwork(Rect("Life " + (i + 1), lives, new Vector2(1, .5f), new Vector2(1, .5f), Vector2.zero, new Vector2(29, 29)), TouchArtwork.Shape.Heart, new Color(.96f, .16f, .30f));
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
            itemText = Label("ITEM", useRect, new Vector2(0.5f, 0.5f), new Vector2(0, -115), new Vector2(240, 45), 19, TextAnchor.MiddleCenter);
            toast = Label("", safe, new Vector2(0.5f, 0), new Vector2(0, 60), new Vector2(900, 90), 27, TextAnchor.MiddleCenter);
            menu = Rect("Main menu", safe, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject;
            RectTransform actions = Rect("Main menu actions", menu.transform, new Vector2(.64f, 0), new Vector2(.96f, 1), Vector2.zero, Vector2.zero);
            MenuButton("PLAY", actions, 96, 86, app.NewGame, true);
            resumeButton = MenuButton("CONTINUE SAVED GAME", actions, -10, 78, app.ContinueSaved);
            resumeText = resumeButton.GetComponentInChildren<Text>();
            MenuButton("HELP", actions, -112, 68, () => help.SetActive(true));
            hero = new HeroPresentation();
            var victoryRect = Rect("Hero celebration", controls.transform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0, -145), new Vector2(150, 200));
            celebration = victoryRect.gameObject.AddComponent<RawImage>(); celebration.texture = hero.Texture; celebration.raycastTarget = false;
            pause = Panel("Pause", safe, panelColor);
            Label("PAUSED", pause.transform, new Vector2(0.5f, 0.5f), new Vector2(0, 190), new Vector2(700, 90), 58, TextAnchor.MiddleCenter);
            Button("RESUME", pause.transform, new Vector2(0.5f, 0.5f), new Vector2(0, 75), new Vector2(420, 80), app.TogglePause);
            Button("SETTINGS", pause.transform, new Vector2(0.5f, 0.5f), new Vector2(0, -25), new Vector2(420, 70), () => settings.SetActive(true));
            Button("NEW GAME", pause.transform, new Vector2(0.5f, 0.5f), new Vector2(0, -120), new Vector2(420, 70), app.NewGame);
            Button("MAIN MENU", pause.transform, new Vector2(0.5f, 0.5f), new Vector2(0, -210), new Vector2(420, 65), app.GoToMenu);
            results = Panel("Game Over", safe, panelColor);
            resultText = Label("", results.transform, new Vector2(0.5f, 0.5f), new Vector2(0, 140), new Vector2(900, 220), 45, TextAnchor.MiddleCenter);
            Button("PLAY AGAIN", results.transform, new Vector2(0.5f, 0.5f), new Vector2(0, -50), new Vector2(420, 80), app.NewGame);
            Button("MAIN MENU", results.transform, new Vector2(0.5f, 0.5f), new Vector2(0, -155), new Vector2(420, 70), app.GoToMenu);
            settings = Panel("Settings", safe, panelColor);
            BuildSettings();
            help = Panel("Help and privacy", safe, panelColor);
            Label("HOW TO PLAY", help.transform, new Vector2(.5f, .5f), new Vector2(0, 320), new Vector2(1000, 75), 44, TextAnchor.MiddleCenter);
            Label("Collect all hearts to reach the next level. You start with three lives.\n\nYou move automatically along the center of each corridor. A high camera shows nearby corridors.\nPoint the stick toward a corridor on the screen to choose your next direction.\nRelease the stick to keep moving. A turn is remembered until it is possible.\nDesktop: WASD/arrows: choose direction · Space: use item · Esc: pause.\n\nA large Power Heart turns the chase around. Catch the blue pursuers!\nBananas land ahead. Oil drops behind you. Boost makes you faster.\nYellow floor lights warn that a mechanical shortcut is about to open.",
                help.transform, new Vector2(.5f, .5f), new Vector2(0, 65), new Vector2(1120, 400), 25, TextAnchor.UpperLeft);
            Label("Fictional political satire. Not affiliated with the people depicted.\nPlay offline. Your best score, settings and saved game stay on this device.\nNo ads, analytics or accounts. Support opens your browser.",
                help.transform, new Vector2(.5f, .5f), new Vector2(0, -180), new Vector2(1120, 140), 24, TextAnchor.MiddleCenter);
            Button("SUPPORT", help.transform, new Vector2(.5f, .5f), new Vector2(-220, -320), new Vector2(370, 65), () => Application.OpenURL("https://donkeytrump-api-p.azurewebsites.net/support"));
            Button("BACK", help.transform, new Vector2(.5f, .5f), new Vector2(220, -320), new Vector2(370, 65), () => help.SetActive(false));
            help.SetActive(false);
            RectTransform flash = Rect("Encounter flash", safe, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            scare = flash.gameObject.AddComponent<Image>(); scare.color = Color.clear; scare.raycastTarget = false;
            // The notice is visible only in development builds, as required for temporary assets.
            if (Debug.isDebugBuild || Application.isEditor)
                prototype = Label("PROTOTYPE · temporary 3D characters", safe, new Vector2(0.5f, 1), new Vector2(0, -28), new Vector2(700, 35), 17, TextAnchor.MiddleCenter);
            menu.SetActive(true); pause.SetActive(false); results.SetActive(false); settings.SetActive(false); controls.SetActive(false);
            RefreshSafeArea(); ApplyHandedness();
        }

        private void BuildSettings()
        {
            Label("SETTINGS", settings.transform, new Vector2(0.5f, 0.5f), new Vector2(0, 320), new Vector2(900, 80), 48, TextAnchor.MiddleCenter);
            ToggleRow("Sound", 235, () => app.Settings.sound, value => app.Settings.sound = value);
            ToggleRow("Music", 165, () => app.Settings.music, value => app.Settings.music = value);
            ToggleRow("Haptics", 95, () => app.Settings.haptics, value => app.Settings.haptics = value);
            ToggleRow("Reduce scares / motion", 25, () => app.Settings.reducedMotion, value => app.Settings.reducedMotion = value);
            ToggleRow("Left-handed layout", -45, () => app.Settings.leftHanded, value => { app.Settings.leftHanded = value; ApplyHandedness(); });
            ToggleRow("Low graphics mode", -115, () => app.Settings.economyGraphics, value => { app.Settings.economyGraphics = value; GraphicsProfile.Apply(value); });
            Button("BACK", settings.transform, new Vector2(0.5f, 0.5f), new Vector2(0, -290), new Vector2(420, 70), () => { settings.SetActive(false); app.SaveSettings(); });
        }
        private void ToggleRow(string title, float y, Func<bool> read, Action<bool> write)
        {
            Button button = null;
            button = Button(title + ": " + (read() ? "ON" : "OFF"), settings.transform, new Vector2(0.5f, 0.5f), new Vector2(0, y), new Vector2(840, 60), () => {
                write(!read()); button.GetComponentInChildren<Text>().text = title + ": " + (read() ? "ON" : "OFF"); app.SaveSettings();
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
            Joystick.Clear();
        }
        public void Notify(string message, float seconds = 3, bool showInMenu = false)
        { toast.text = message; menuNotice = showInMenu; toastUntil = UnityEngine.Time.unscaledTime + seconds; toast.transform.SetAsLastSibling(); }
        public void Encounter(string name)
        {
            Notify(name + "!", 1.0f);
            if (!app.Settings.reducedMotion) scareUntil = UnityEngine.Time.unscaledTime + 0.25f;
        }
        public void Update(GameSession session, bool inMenu)
        {
            RefreshSafeArea();
            bool celebrate = !inMenu && session.Phase == SessionPhase.LevelClear;
            hero.Update(celebrate, celebrate, app.Settings.reducedMotion);
            celebration.gameObject.SetActive(celebrate);
            bool isPaused = !inMenu && session.Phase == SessionPhase.Paused;
            menu.SetActive(inMenu); pause.SetActive(isPaused);
            bool gameOver = !inMenu && session.Phase == SessionPhase.GameOver;
            if (prototype)
            {
                prototype.gameObject.SetActive(!inMenu);
                prototype.rectTransform.anchoredPosition = new Vector2(0, isPaused || gameOver || settings.activeSelf ? -28 : -105);
            }
            toast.gameObject.SetActive(!settings.activeSelf && (!inMenu || menuNotice));
            controls.SetActive(!inMenu && !isPaused && !gameOver && !settings.activeSelf);
            results.SetActive(gameOver);
            backdrop.SetActive(inMenu || isPaused || gameOver || settings.activeSelf || help.activeSelf);
            menuArtwork.gameObject.SetActive(inMenu && !settings.activeSelf && !help.activeSelf);
            menuFade.gameObject.SetActive(menuArtwork.gameObject.activeSelf && menuArtwork.rectTransform.rect.width < backdropRect.rect.width - 1);
            backdrop.GetComponent<Image>().color = menuArtwork.gameObject.activeSelf ? coverColor : panelColor;
            bool scoreChanged = shownScore != session.Score || shownLevel != session.Level;
            bool recordChanged = shownRecord != app.Settings.highScore;
            if (scoreChanged || recordChanged)
                resultText.text = "GAME OVER\nScore: " + session.Score.ToString("N0", CultureInfo.InvariantCulture) + " · Level " + session.Level.ToString(CultureInfo.InvariantCulture) + "\nBest: " + app.Settings.highScore.ToString("N0", CultureInfo.InvariantCulture);
            if (recordChanged) shownRecord = app.Settings.highScore;
            resumeButton.interactable = app.HasSavedGame;
            resumeText.color = app.HasSavedGame ? ink : new Color(ink.r, ink.g, ink.b, .4f);
            if (UnityEngine.Time.unscaledTime > toastUntil) toast.text = "";
            float flash = Mathf.Clamp01((scareUntil - UnityEngine.Time.unscaledTime) / 0.25f);
            scare.color = new Color(1, 0.81f, 0.3f, flash * 0.12f);
            if (scoreChanged) stats.text = "SCORE " + session.Score.ToString("N0", CultureInfo.InvariantCulture) + "\nLEVEL " + session.Level.ToString(CultureInfo.InvariantCulture);
            if (shownChances != session.Chances || shownHearts != session.RemainingHearts)
            {
                hearts.text = "Hearts left: " + session.RemainingHearts.ToString(CultureInfo.InvariantCulture);
                livesLabel.rectTransform.anchoredPosition = new Vector2(-session.Chances * 34 - 40, 0);
                for (int i = 0; i < lifeHearts.Length; i++)
                { lifeHearts[i].gameObject.SetActive(i < session.Chances); lifeHearts[i].rectTransform.anchoredPosition = new Vector2(-16 - (session.Chances - 1 - i) * 34, 0); }
                shownChances = session.Chances; shownHearts = session.RemainingHearts;
            }
            itemText.text = session.Inventory == ItemKind.None ? "ITEM" : session.Inventory == ItemKind.BananaPeel ? "BANANA" : session.Inventory == ItemKind.OilSlick ? "OIL" : "BOOST";
            itemIcon.Artwork = session.Inventory == ItemKind.BananaPeel ? TouchArtwork.Shape.Banana : session.Inventory == ItemKind.OilSlick ? TouchArtwork.Shape.Oil : session.Inventory == ItemKind.SpeedBoost ? TouchArtwork.Shape.Boost : TouchArtwork.Shape.Empty;
            itemIcon.color = session.Inventory == ItemKind.None ? new Color(1, 1, 1, 0.25f) : session.Inventory == ItemKind.BananaPeel ? new Color(1, 0.83f, 0.25f) : session.Inventory == ItemKind.OilSlick ? new Color(0.76f, 0.68f, 0.93f) : new Color(0.51f, 0.95f, 0.82f);
            itemButton.interactable = session.Phase == SessionPhase.Playing && session.Inventory != ItemKind.None;
            int powerTenths = Mathf.Max(0, Mathf.CeilToInt((float)(session.PowerUntil - session.Time) * 10));
            int boostTenths = Mathf.Max(0, Mathf.CeilToInt((float)(session.BoostUntil - session.Time) * 10));
            if (shownPower != powerTenths || shownBoost != boostTenths)
            {
                effects.text = (powerTenths > 0 ? "POWER HEART  " + (powerTenths / 10f).ToString("F1", CultureInfo.InvariantCulture) + " s" : "") +
                    (boostTenths > 0 ? "   BOOST " + (boostTenths / 10f).ToString("F1", CultureInfo.InvariantCulture) + " s" : "");
                shownPower = powerTenths; shownBoost = boostTenths;
            }
            int countdown = Mathf.CeilToInt((float)session.PhaseRemaining);
            if (shownPhase != session.Phase || (session.Phase == SessionPhase.Countdown && shownCountdown != countdown) ||
                (session.Phase == SessionPhase.GameOver && scoreChanged))
                center.text = session.Phase == SessionPhase.Countdown ? "Collect all hearts\n" + countdown.ToString(CultureInfo.InvariantCulture) :
                session.Phase == SessionPhase.Caught ? "Caught! Try again" : session.Phase == SessionPhase.LevelClear ? "All hearts collected!" :
                session.Phase == SessionPhase.GameOver ? "GAME OVER\nScore: " + session.Score.ToString("N0", CultureInfo.InvariantCulture) + "\nPause/menu: ESC or Ⅱ" : "";
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
                    Point2 screenOffset = app.ScreenDirection(offset).Normalized;
                    dangerText = Math.Abs(screenOffset.x) > .5 ? (screenOffset.x < 0 ? "‹ Danger nearby" : "Danger nearby ›") :
                        screenOffset.y < 0 ? "Danger below you" : "Danger above you";
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
            Joystick?.Clear();
            Canvas.ForceUpdateCanvases();
            // Fit the complete cover at its original aspect ratio. The dark backdrop
            // extends its right side on wide screens; buttons use the safe area.
            if (menuArtwork.texture)
            {
                float scale = Mathf.Min(backdropRect.rect.width / menuArtwork.texture.width, backdropRect.rect.height / menuArtwork.texture.height);
                menuArtwork.rectTransform.sizeDelta = new Vector2(menuArtwork.texture.width * scale, menuArtwork.texture.height * scale);
                menuFade.rectTransform.sizeDelta = new Vector2(menuArtwork.rectTransform.rect.width * .16f, menuArtwork.rectTransform.rect.height);
                menuFade.rectTransform.anchoredPosition = new Vector2(menuArtwork.rectTransform.rect.width * .84f, 0);
            }
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
        private Button MenuButton(string title, Transform parent, float y, float height, Action action, bool primary = false)
        {
            Button button = Button(title, parent, new Vector2(.5f, .5f), new Vector2(0, y), new Vector2(390, height), action);
            var image = button.GetComponent<Image>();
            image.color = primary ? new Color(.96f, .76f, .31f, 1) : new Color(.015f, .055f, .085f, 1);
            ColorBlock colors = button.colors; colors.disabledColor = new Color(.45f, .45f, .45f, 1); button.colors = colors;
            var outline = button.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(.96f, .76f, .38f, primary ? 1 : .7f); outline.effectDistance = Vector2.one;
            Text label = button.GetComponentInChildren<Text>(); label.fontSize = primary ? 32 : 25;
            label.color = primary ? new Color(.025f, .065f, .085f) : ink;
            if (primary) label.GetComponent<Shadow>().enabled = false;
            return button;
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
