using System;
using System.Text;
using MazeOfEgos.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace MazeOfEgos.Runtime
{
    public sealed class CompetitionHud
    {
        private readonly CompetitionApp app;
        private readonly Font font;
        private readonly RectTransform safe, backdropRect;
        private readonly GameObject menu, selection, controls, pause, settings, help, results, backdrop;
        private readonly RawImage menuArtwork;
        private readonly MenuEdgeFade menuFade;
        private readonly Text timer, centre, status, toast, resultTitle, resultRows;
        private readonly Text[] standings = new Text[5];
        private readonly Text[,] table = new Text[5,4];
        private readonly Text[] podium = new Text[3], mapNumbers = new Text[5], hudTrophies = new Text[5];
        private readonly RectTransform[] podiumGroups = new RectTransform[3];
        private readonly RectTransform[,] podiumCards = new RectTransform[3,5];
        private readonly RawImage[,] podiumPhotos = new RawImage[3,5];
        private readonly Text[,] podiumNames = new Text[3,5], podiumPoints = new Text[3,5];
        private readonly Texture2D[] portraits = new Texture2D[5];
        private readonly TouchArtwork[,] trophyIcons = new TouchArtwork[5,3];
        private readonly CompetitionMinimap minimap;
        private static readonly Color Gold = new Color(1,.8f,.23f);
        private readonly Button continueButton, itemButton, resultNext;
        private readonly Text itemText;
        private readonly TouchArtwork itemIcon;
        private readonly Color ink = new Color(.95f,.94f,.86f), panelColor = new Color(.055f,.10f,.12f,1), coverColor = new Color(.012f,.039f,.063f,1);
        private Rect lastSafeArea;
        private int lastScreenWidth, lastScreenHeight;
        private float toastUntil;
        private string renderedResults;
        public TouchControls Joystick { get; }
        public bool SettingsOpen => settings.activeSelf;
        public string ScoreboardText => resultRows.text;
        public void CloseSettings() => settings.SetActive(false);
        public CompetitionHud(CompetitionApp app)
        {
            this.app = app; font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var root = new GameObject("Competition interface",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)); root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1600,900); scaler.matchWidthOrHeight = .5f;
            if (!UnityEngine.Object.FindFirstObjectByType<EventSystem>()) new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule)).GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            backdrop = Panel("Menu backdrop",root.transform,panelColor); backdropRect = (RectTransform)backdrop.transform;
            var art = Rect("Menu artwork",backdrop.transform,new Vector2(0,.5f),new Vector2(0,.5f),Vector2.zero,Vector2.zero); art.pivot = new Vector2(0,.5f);
            menuArtwork = art.gameObject.AddComponent<RawImage>(); menuArtwork.texture = Resources.Load<Texture2D>("Brand/MenuBackground"); menuArtwork.raycastTarget = false;
            var fade = Rect("Cover right edge",backdrop.transform,new Vector2(0,.5f),new Vector2(0,.5f),Vector2.zero,Vector2.zero); fade.pivot = new Vector2(0,.5f);
            menuFade = fade.gameObject.AddComponent<MenuEdgeFade>(); menuFade.color = coverColor; menuFade.raycastTarget = false;
            safe = Rect("Safe area",root.transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            menu = Rect("Main menu",safe,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero).gameObject;
            var actions = Rect("Main menu actions",menu.transform,new Vector2(.64f,0),new Vector2(.96f,1),Vector2.zero,Vector2.zero);
            MenuButton("PLAY",actions,96,86,app.ChooseCharacter,true);
            continueButton = MenuButton("CONTINUE SAVED GAME",actions,-10,78,app.ContinueSaved);
            MenuButton("HELP",actions,-112,68,()=>help.SetActive(true));
            selection = Panel("Character selection",safe,panelColor);
            Label("CHOOSE YOUR CHARACTER",selection.transform,new Vector2(.5f,1),new Vector2(0,-90),new Vector2(1200,80),48,TextAnchor.MiddleCenter);
            Label("Same abilities. Four rivals. First to 3 trophies.",selection.transform,new Vector2(.5f,1),new Vector2(0,-170),new Vector2(1200,60),27,TextAnchor.MiddleCenter);
            for (int i = 0; i < 5; i++) {
                string id = CompetitionSession.ParticipantIds[i]; float x = (i-2)*238;
                portraits[i]=Resources.Load<Texture2D>("Characters/"+id);
                var card = Button(CharacterRoster.Get(id).displayName,selection.transform,new Vector2(.5f,.5f),new Vector2(x,-10),new Vector2(215,330),()=>app.NewMatch(id));
                card.name="Character card "+id;
                Text name=card.GetComponentInChildren<Text>();name.rectTransform.sizeDelta=new Vector2(195,50);name.rectTransform.anchoredPosition=new Vector2(0,-120);
                var portrait = Rect("Portrait " + id,card.transform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(0,35),new Vector2(205,205));
                var image=portrait.gameObject.AddComponent<RawImage>();image.texture=portraits[i];image.raycastTarget=false;
                image.uvRect=new Rect(0,.5f,.25f,.5f);

            }
            Button("BACK",selection.transform,new Vector2(.5f,0),new Vector2(0,90),new Vector2(380,65),app.GoToMenu);
            controls = Rect("Competition HUD",safe,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero).gameObject;
            for (int i = 0; i < 5; i++) {
                float x = .23f + (i+.5f)*.77f/5;
                standings[i] = Label("",controls.transform,new Vector2(x,1),new Vector2(0,-38),new Vector2(180,60),20,TextAnchor.MiddleCenter);
                Artwork(Rect("HUD trophy "+i,controls.transform,new Vector2(x,1),new Vector2(x,1),new Vector2(-28,-83),new Vector2(29,29)),TouchArtwork.Shape.Trophy,Gold);
                hudTrophies[i]=Label("",controls.transform,new Vector2(x,1),new Vector2(22,-83),new Vector2(60,32),20,TextAnchor.MiddleLeft);
            }
            var mapBox = Rect("Minimap panel",controls.transform,new Vector2(0,1),new Vector2(0,1),new Vector2(122,-140),new Vector2(224,268));
            mapBox.gameObject.AddComponent<Image>().color = new Color(.015f,.035f,.045f,.94f);
            Label("MAP  ·  NORTH UP",mapBox,new Vector2(.5f,1),new Vector2(0,-14),new Vector2(218,25),14,TextAnchor.MiddleCenter);
            var mapRect=Rect("Minimap",mapBox,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(0,-9),new Vector2(210,232));
            minimap=mapRect.gameObject.AddComponent<CompetitionMinimap>();minimap.raycastTarget=false;
            for(int i=0;i<5;i++) mapNumbers[i]=Label((i+1).ToString(),mapRect,new Vector2(.5f,.5f),Vector2.zero,new Vector2(16,16),12,TextAnchor.MiddleCenter);
            timer = Label("",controls.transform,new Vector2(.6f,1),new Vector2(0,-115),new Vector2(540,50),30,TextAnchor.MiddleCenter);
            Button("PAUSE",controls.transform,new Vector2(1,1),new Vector2(-80,-115),new Vector2(140,50),app.TogglePause);
            centre = Label("",controls.transform,new Vector2(.5f,.5f),new Vector2(0,90),new Vector2(800,170),44,TextAnchor.MiddleCenter);
            status = Label("",controls.transform,new Vector2(.5f,0),new Vector2(0,125),new Vector2(600,70),25,TextAnchor.MiddleCenter);
            var joy = Rect("Movement joystick",controls.transform,Vector2.zero,Vector2.zero,new Vector2(150,155),new Vector2(300,300));
            joy.gameObject.AddComponent<Image>().color = new Color(0,0,0,.001f); Joystick = joy.gameObject.AddComponent<TouchControls>(); Joystick.joystick = true;
            var stick = Rect("Joystick artwork",joy,new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(220,220));
            Joystick.artwork = stick.gameObject.AddComponent<CanvasGroup>(); Artwork(stick,TouchArtwork.Shape.Base,ink);
            var knob = Rect("Joystick thumb",stick,new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(76,76)); Artwork(knob,TouchArtwork.Shape.Thumb,new Color(1,.88f,.59f)); Joystick.knob = knob;
            Label("MOVE",stick,new Vector2(.5f,.5f),new Vector2(0,-125),new Vector2(150,35),18,TextAnchor.MiddleCenter);
            var use = Rect("Use item",controls.transform,new Vector2(1,0),new Vector2(1,0),new Vector2(-150,155),new Vector2(170,170));
            var graphic = Artwork(use,TouchArtwork.Shape.Button,ink); graphic.raycastTarget = true; itemButton = use.gameObject.AddComponent<Button>(); itemButton.targetGraphic = graphic; itemButton.onClick.AddListener(app.UseItem);
            itemIcon = Artwork(Rect("Item icon",use,new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(92,92)),TouchArtwork.Shape.Empty,ink);
            itemText = Label("ITEM",use,new Vector2(.5f,.5f),new Vector2(0,-115),new Vector2(240,45),19,TextAnchor.MiddleCenter);
            pause = Panel("Paused",safe,panelColor);
            Label("PAUSED",pause.transform,new Vector2(.5f,.5f),new Vector2(0,230),new Vector2(900,80),54,TextAnchor.MiddleCenter);
            Button("RESUME",pause.transform,new Vector2(.5f,.5f),new Vector2(0,105),new Vector2(440,75),app.TogglePause);
            Button("SETTINGS",pause.transform,new Vector2(.5f,.5f),new Vector2(0,10),new Vector2(440,70),()=>settings.SetActive(true));
            Button("HELP",pause.transform,new Vector2(.5f,.5f),new Vector2(0,-85),new Vector2(440,70),()=>help.SetActive(true));
            Button("SAVE & MAIN MENU",pause.transform,new Vector2(.5f,.5f),new Vector2(0,-180),new Vector2(440,70),app.GoToMenu);
            results = Panel("Competition results",safe,panelColor);
            resultTitle = Label("",results.transform,new Vector2(.5f,.5f),new Vector2(0,330),new Vector2(1230,70),30,TextAnchor.MiddleCenter);
            resultRows = Label("",results.transform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(1120,320),24,TextAnchor.UpperCenter);
            resultRows.gameObject.SetActive(false);
            string[] medals={"GOLD","SILVER","BRONZE"};Color[] metalColours={Gold,new Color(.78f,.85f,.91f),new Color(.88f,.53f,.28f)};
            for(int i=0;i<3;i++) {
                float x=(i-1)*385;
                var group=Rect("Round "+medals[i],results.transform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(x,180),new Vector2(360,220));podiumGroups[i]=group;
                var heading=Rect(medals[i]+" heading",group,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(0,88),new Vector2(350,45));
                Artwork(Rect(medals[i]+" medal",heading,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(-64,0),new Vector2(44,44)),TouchArtwork.Shape.Medal,metalColours[i]);
                Label(medals[i],heading,new Vector2(.5f,.5f),new Vector2(25,0),new Vector2(165,38),24,TextAnchor.MiddleCenter).color=metalColours[i];
                podium[i]=Label("—",group,new Vector2(.5f,.5f),Vector2.zero,new Vector2(200,45),23,TextAnchor.MiddleCenter);
                for(int j=0;j<5;j++) {
                    var card=Rect("Round portrait "+i+"-"+j,group,new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(170,180));podiumCards[i,j]=card;
                    var photo=Rect("Result photo",card,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(0,10),new Vector2(108,108));
                    var image=photo.gameObject.AddComponent<RawImage>();image.raycastTarget=false;image.uvRect=new Rect(0,.5f,.25f,.5f);podiumPhotos[i,j]=image;
                    podiumNames[i,j]=Label("",card,new Vector2(.5f,.5f),new Vector2(0,-62),new Vector2(170,28),23,TextAnchor.MiddleCenter);
                    podiumPoints[i,j]=Label("",card,new Vector2(.5f,.5f),new Vector2(0,-94),new Vector2(170,27),21,TextAnchor.MiddleCenter);
                }
            }
            Label("OVERALL STANDINGS",results.transform,new Vector2(.5f,.5f),new Vector2(0,31),new Vector2(1000,36),27,TextAnchor.MiddleCenter);
            float[] columns = { -420,-110,130,385 };
            string[] headers = { "PLAYER","ROUND POINTS","TOTAL POINTS","TROPHIES" };
            for(int c=0;c<4;c++) {
                Label(headers[c],results.transform,new Vector2(.5f,.5f),new Vector2(columns[c],-16),new Vector2(230,38),20,TextAnchor.MiddleCenter);
                for(int r=0;r<5;r++) table[r,c]=Label("",results.transform,new Vector2(.5f,.5f),new Vector2(c==3?475:columns[c],-65-r*48),new Vector2(c==3?60:230,44),25,TextAnchor.MiddleCenter);
            }
            for(int r=0;r<5;r++) for(int t=0;t<3;t++) trophyIcons[r,t]=Artwork(Rect("Overall trophy "+r+"-"+t,results.transform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(330+t*43,-65-r*48),new Vector2(38,38)),TouchArtwork.Shape.Trophy,Gold);
            resultNext = Button("NEXT ROUND",results.transform,new Vector2(.5f,.5f),new Vector2(-240,-350),new Vector2(420,70),()=>{if(app.Session.Phase==CompetitionPhase.MatchResults)app.ChooseCharacter();else app.NextRound();});
            Button("SAVE & MAIN MENU",results.transform,new Vector2(.5f,.5f),new Vector2(240,-350),new Vector2(420,70),app.GoToMenu);
            settings = Panel("Settings",safe,panelColor);
            Label("SETTINGS",settings.transform,new Vector2(.5f,.5f),new Vector2(0,310),new Vector2(900,80),48,TextAnchor.MiddleCenter);
            Toggle("Sound",210,()=>app.Settings.sound,v=>app.Settings.sound=v);
            Toggle("Music",140,()=>app.Settings.music,v=>app.Settings.music=v);
            Toggle("Haptics",70,()=>app.Settings.haptics,v=>app.Settings.haptics=v);
            Toggle("Reduced motion",0,()=>app.Settings.reducedMotion,v=>app.Settings.reducedMotion=v);
            Toggle("Left-handed layout",-70,()=>app.Settings.leftHanded,v=>{app.Settings.leftHanded=v;ApplyHandedness();});
            Toggle("Low graphics",-140,()=>app.Settings.economyGraphics,v=>{app.Settings.economyGraphics=v;GraphicsProfile.Apply(v);});
            Button("BACK",settings.transform,new Vector2(.5f,.5f),new Vector2(0,-280),new Vector2(420,70),()=>{CloseSettings();app.SaveSettings();});
            help = Panel("How to play",safe,panelColor);
            Label("HOW TO PLAY",help.transform,new Vector2(.5f,.5f),new Vector2(0,310),new Vector2(1000,75),44,TextAnchor.MiddleCenter);
            Label("Collect hearts: each is 1 point. They stay collected until the next round.\nRounds end at 2 minutes or when every heart is collected. Most points wins gold.\nFirst to 3 trophies wins. Total points break a tie; equal totals share victory.\nRandom round starts. The map shows hearts and numbered players.\n\nPoint the joystick toward a corridor on screen. Movement is automatic.\nTurns are remembered. Release to continue; a wall stops you.\nBoxes refill and replace your one item. Aim follows your heading.\nBanana: slip. Oil: slow. Boost: speed. Water balloon or cream pie: knockout.\nBlue circles and lines warn of attacks. Walls block them. Turn away in time!\nKnockout: return to the centre after 5 seconds. You keep every point.\nProtection lasts 3 seconds and ends when you attack. Contact is harmless.\n\nDesktop: WASD/arrows move · Space uses item · Esc pauses.",help.transform,new Vector2(.5f,.5f),new Vector2(0,35),new Vector2(1160,470),24,TextAnchor.UpperLeft);
            Label("Fictional satire. Not affiliated with the people depicted. Offline play.\nSettings and matches stay on this device. No ads, analytics or accounts.",help.transform,new Vector2(.5f,.5f),new Vector2(0,-225),new Vector2(1160,85),22,TextAnchor.MiddleCenter);
            Button("SUPPORT",help.transform,new Vector2(.5f,.5f),new Vector2(-220,-325),new Vector2(370,65),()=>Application.OpenURL("https://donkeytrump-api-p.azurewebsites.net/support"));
            Button("BACK",help.transform,new Vector2(.5f,.5f),new Vector2(220,-325),new Vector2(370,65),()=>help.SetActive(false));
            toast = Label("",safe,new Vector2(.5f,0),new Vector2(0,52),new Vector2(850,75),25,TextAnchor.MiddleCenter);
            settings.SetActive(false); help.SetActive(false); ApplyHandedness(); RefreshSafeArea(); Render();
        }
        private void Toggle(string title,float y,Func<bool> read,Action<bool> write)
        {
            Button b = null; b = Button(title+": "+(read()?"ON":"OFF"),settings.transform,new Vector2(.5f,.5f),new Vector2(0,y),new Vector2(840,60),()=>{write(!read());b.GetComponentInChildren<Text>().text=title+": "+(read()?"ON":"OFF");app.SaveSettings();});
        }
        public void ApplyHandedness()
        {
            bool left = app.Settings.leftHanded; var j = (RectTransform)Joystick.transform; j.anchorMin=j.anchorMax=new Vector2(left?1:0,0); j.anchoredPosition=new Vector2(left?-150:150,155);
            var u = (RectTransform)itemButton.transform;u.anchorMin=u.anchorMax=new Vector2(left?0:1,0);u.anchoredPosition=new Vector2(left?150:-150,155);Joystick.Clear();
        }
        public void Notify(string message,float seconds=3) { toast.text=message;toastUntil=Time.unscaledTime+seconds;toast.transform.SetAsLastSibling(); }
        public static string ItemName(CompetitionItem item) => item==CompetitionItem.WaterBalloon?"WATER BALLOON":item==CompetitionItem.CreamPie?"CREAM PIE":item==CompetitionItem.None?"ITEM":item.ToString().ToUpperInvariant();
        public void Render()
        {
            RefreshSafeArea(); var s=app.Session;bool inMenu=app.InMenu, paused=!inMenu&&s.Phase==CompetitionPhase.Paused, result=!inMenu&&(s.Phase==CompetitionPhase.RoundResults||s.Phase==CompetitionPhase.MatchResults);
            menu.SetActive(inMenu&&!app.Selecting);selection.SetActive(inMenu&&app.Selecting);pause.SetActive(paused);results.SetActive(result);controls.SetActive(!inMenu&&!paused&&!result);
            bool art = inMenu&&!app.Selecting&&!help.activeSelf;backdrop.SetActive(inMenu||paused||result||help.activeSelf||settings.activeSelf);menuArtwork.gameObject.SetActive(art);menuFade.gameObject.SetActive(art&&menuArtwork.rectTransform.rect.width<backdropRect.rect.width-1);backdrop.GetComponent<Image>().color=art?coverColor:panelColor;
            continueButton.interactable=app.HasSavedGame;
            var human=s.Actor(s.HumanIndex);
            minimap.Render(s);
            for(int i=0;i<5;i++) {var a=s.Actor(i);string name=a.Id==s.HumanId?"YOU":CharacterRoster.Get(a.Id).displayName;standings[i].text=(i+1)+" · "+name+"\n"+a.RoundScore+" pts  ·  "+a.TotalScore+" total";standings[i].color=CompetitionMinimap.Colours[i];hudTrophies[i].text="× "+a.Trophies;
                mapNumbers[i].gameObject.SetActive(a.Life==CompetitionLife.Active);mapNumbers[i].rectTransform.anchoredPosition=minimap.MapPoint(a.Position);mapNumbers[i].color=new Color(.01f,.025f,.035f);
            }
            int seconds=(int)Math.Ceiling(s.RoundSecondsRemaining);timer.text="ROUND "+s.RoundNumber+"     "+(seconds/60)+":"+(seconds%60).ToString("00");
            centre.text=s.Phase==CompetitionPhase.Countdown?"GET READY\n"+((s.CountdownRemaining+59)/60):human.Life==CompetitionLife.KnockedOut?"BACK IN "+Math.Max(1,(human.KnockoutUntil-s.TickIndex+59)/60)+"\nYour points are safe":"";
            status.text=human.ProtectedUntil>s.TickIndex?"PROTECTED · "+((human.ProtectedUntil-s.TickIndex+59)/60)+"s":human.SlipUntil>s.TickIndex?"WHOOPS!":human.OilUntil>s.TickIndex?"SLOWED":human.BoostUntil>s.TickIndex?"BOOST":"";
            itemText.text=ItemName(human.Inventory);itemButton.interactable=s.Phase==CompetitionPhase.Playing&&human.Life==CompetitionLife.Active&&human.Inventory!=CompetitionItem.None;
            itemIcon.Artwork=human.Inventory==CompetitionItem.Banana?TouchArtwork.Shape.Banana:human.Inventory==CompetitionItem.Oil?TouchArtwork.Shape.Oil:human.Inventory==CompetitionItem.Boost?TouchArtwork.Shape.Boost:human.Inventory==CompetitionItem.WaterBalloon?TouchArtwork.Shape.Balloon:human.Inventory==CompetitionItem.CreamPie?TouchArtwork.Shape.Pie:TouchArtwork.Shape.Empty;
            itemIcon.color=human.Inventory==CompetitionItem.WaterBalloon?new Color(.3f,.8f,1):human.Inventory==CompetitionItem.Banana?new Color(1,.8f,.2f):ink;
            if(result) {
                var winners=s.Phase==CompetitionPhase.MatchResults?s.MatchWinners:s.RoundWinners;var w=new StringBuilder();foreach(string id in winners){if(w.Length>0)w.Append(" & ");w.Append(id==s.HumanId?"YOU":CharacterRoster.Get(id).displayName);}
                resultTitle.text=(s.Phase==CompetitionPhase.MatchResults?(winners.Count>1?"SHARED MATCH VICTORY":"MATCH WINNER: "+w):"ROUND "+s.RoundNumber+" RESULTS")+"\n"+(s.RoundEnd==CompetitionRoundEnd.HeartsCollected?"ALL HEARTS COLLECTED":"TIME'S UP");
                int goldCount=0;for(int i=0;i<5;i++)if(s.RoundPlace(i)==1)goldCount++;
                bool expandedGold=goldCount>=3;
                for(int place=1;place<=3;place++) {
                    int column=place-1,count=0;for(int i=0;i<5;i++)if(s.RoundPlace(i)==place)count++;
                    bool visible=!expandedGold||place==1;podiumGroups[column].gameObject.SetActive(visible);if(!visible)continue;
                    podiumGroups[column].anchoredPosition=new Vector2(expandedGold?0:(column-1)*385,180);
                    podium[column].gameObject.SetActive(count==0);
                    float width=expandedGold?1080:360,slot=width/Math.Max(1,count);int n=0;
                    for(int i=0;i<5;i++)if(s.RoundPlace(i)==place) {
                        var actor=s.Actor(i);var card=podiumCards[column,n];card.gameObject.SetActive(true);card.anchoredPosition=new Vector2((n-(count-1)*.5f)*slot,0);
                        float size=Mathf.Min(108,slot-20);podiumPhotos[column,n].texture=portraits[i];podiumPhotos[column,n].rectTransform.sizeDelta=new Vector2(size,size);
                        var name=podiumNames[column,n];name.text=CharacterRoster.Get(actor.Id).displayName;name.fontSize=slot<150?18:23;name.rectTransform.sizeDelta=new Vector2(slot-8,28);name.color=actor.Id==s.HumanId?Gold:ink;
                        var points=podiumPoints[column,n];points.text=actor.RoundScore+" pts";points.rectTransform.sizeDelta=new Vector2(slot-8,27);points.color=name.color;n++;
                    }
                    for(;n<5;n++)podiumCards[column,n].gameObject.SetActive(false);
                }
                var rows=new StringBuilder("PLAYER          ROUND POINTS     TOTAL POINTS     TROPHIES\n\n");
                var sorted=new System.Collections.Generic.List<CompetitionActorView>();for(int i=0;i<5;i++)sorted.Add(s.Actor(i));sorted.Sort((a,b)=>{int c=b.Trophies.CompareTo(a.Trophies);return c!=0?c:b.TotalScore.CompareTo(a.TotalScore);});
                for(int r=0;r<5;r++) {var a=sorted[r];string name=a.Id==s.HumanId?"YOU":CharacterRoster.Get(a.Id).displayName;table[r,0].text=s.OverallPlace(Array.IndexOf(new[]{"kim","putin","trump","xi","zelenskyy"},a.Id))+". "+name;table[r,1].text=a.RoundScore.ToString();table[r,2].text=a.TotalScore.ToString();table[r,3].text="× "+a.Trophies;for(int t=0;t<3;t++)trophyIcons[r,t].color=t<a.Trophies?Gold:new Color(.18f,.25f,.27f);for(int c=0;c<4;c++)table[r,c].color=a.Id==s.HumanId?new Color(1,.84f,.35f):ink;rows.Append(name).Append(' ').Append(a.RoundScore).Append(' ').Append(a.TotalScore).Append(' ').Append(a.Trophies).Append('\n');}
                string text=rows.ToString();if(text!=renderedResults){resultRows.text=text;renderedResults=text;}resultNext.GetComponentInChildren<Text>().text=s.Phase==CompetitionPhase.MatchResults?"NEW MATCH":"NEXT ROUND";
            }
            if(Time.unscaledTime>toastUntil)toast.text="";
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
