#if DEVELOPMENT_BUILD || UNITY_EDITOR || MAZE_PERFORMANCE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using MazeOfEgos.Core;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.EventSystems;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace MazeOfEgos.Runtime
{
    // Excluded from normal Release. All automated saves use a separate directory.
    public sealed class CompetitionAutomation : MonoBehaviour
    {
        public static bool Performance => Environment.GetEnvironmentVariable("MAZE_PERFORMANCE_TEST")=="1";
        private static bool Smoke => Environment.GetEnvironmentVariable("MAZE_SMOKE_TEST")=="1" || Array.IndexOf(Environment.GetCommandLineArgs(),"--maze-smoke-test")>=0;
        private static bool Lifecycle => Environment.GetEnvironmentVariable("MAZE_LIFECYCLE_TEST")=="1";
        public static bool Enabled => Performance || Smoke || Lifecycle;
        public static readonly string StorageDirectory = Path.Combine(Application.temporaryCachePath,"competition-automation",Guid.NewGuid().ToString("N"));
        private static CompetitionAutomation instance;
        private CompetitionApp app;
        private string output,error;
        private bool finished,ready,paused,resumed;
        private float backgroundAt;
        private long pausedTick;
        private string pausedState;
        private readonly Stopwatch clock = new Stopwatch();
        private readonly FrameStatistics frames=new FrameStatistics(),cpu=new FrameStatistics(),gpu=new FrameStatistics();
        private readonly List<object> samples=new List<object>();
        private ProfilerRecorder gcRecorder,cpuRecorder,gpuRecorder;
        private double duration,warmup,measurement=-1,previous,nextSample,nextProgress,resultSince=-1;
        private double playingSeconds,countdownSeconds,resultsSeconds;
        private long gcBytes,residentPeak,unityPeak,managedPeak;
        private int rounds=1,matches=1,backgroundInterruptions;
        private readonly long[] uses=new long[5],hearts=new long[5];
        private long knockouts, collected, itemUses;
        private CompetitionStatistics baseline;
        private Touchscreen touch;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot(){if(Enabled)new GameObject("Opt-in competition checks").AddComponent<CompetitionAutomation>();}
        private IEnumerator Start()
        {
            instance=this;Application.runInBackground=true;Application.logMessageReceived+=OnLog;
            if(Smoke)InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            string mode=Performance?"performance":Lifecycle?"lifecycle":"smoke-captures";
            output=Path.Combine(Application.temporaryCachePath,"maze-"+mode);
            if(Performance && Environment.GetEnvironmentVariable("MAZE_PERFORMANCE_OUTPUT")!=null)output=Environment.GetEnvironmentVariable("MAZE_PERFORMANCE_OUTPUT");
            string[] args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"--maze-smoke-output");if(Smoke&&at>=0)output=args[at+1];
            Directory.CreateDirectory(output);
            foreach(string file in Directory.GetFiles(output))File.Delete(file);
            yield return new WaitForSecondsRealtime(1);
            app=FindFirstObjectByType<CompetitionApp>();
            IEnumerator steps=Performance?BeginPerformance():Lifecycle?CheckLifecycle():CheckSmoke();
            while(true){object next=null;bool more=false;Exception failure=null;try{if(error!=null)throw new Exception(error);more=steps.MoveNext();if(more)next=steps.Current;}catch(Exception e){failure=e;}
                if(failure!=null){error=failure.ToString();FinishSimple(false);yield break;}if(!more)break;yield return next;}
            if(!Performance)FinishSimple(error==null);
        }
        private static void Require(bool value,string message){if(!value)throw new Exception(message);}
        private void OnLog(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)error=message+"\n"+stack;}
        private void Capture(string name)
        {
            foreach(var text in FindObjectsByType<Text>(FindObjectsSortMode.None))Require(text.preferredHeight<=text.rectTransform.rect.height+4,"Text exceeds layout: "+text.text);
            // The filename overload prepends Documents on iOS even for absolute paths.
            Texture2D capture=ScreenCapture.CaptureScreenshotAsTexture();
            try{File.WriteAllBytes(Path.Combine(output,name+".png"),capture.EncodeToPNG());}finally{Destroy(capture);}
        }
        private static Button FindButton(string title){foreach(var b in FindObjectsByType<Button>(FindObjectsSortMode.None))if(b.gameObject.activeInHierarchy&&b.name==title)return b;throw new Exception("Missing button "+title);}
        private IEnumerator Tap(string title) => TapAt((RectTransform)FindButton(title).transform);
        private IEnumerator TapAt(RectTransform rect)
        {
            Vector2 p=RectTransformUtility.WorldToScreenPoint(null,rect.position);
            Touch(90,TouchPhase.Began,p);yield return null;yield return new WaitForSecondsRealtime(.08f);Touch(90,TouchPhase.Ended,p);yield return null;yield return new WaitForSecondsRealtime(.12f);
        }
        private void Touch(int id,TouchPhase phase,Vector2 point){InputSystem.QueueStateEvent(touch,new TouchState{touchId=id,phase=phase,position=point});}
        private CompetitionSnapshot Fixture(string human="zelenskyy")
        {
            app.NewMatch(human);var s=app.Session.Snapshot();s.phase=s.resumePhase=CompetitionPhase.Playing;s.countdownRemaining=0;s.computerPlayers=false;s.rules.endWhenHeartsCollected=false;s.hearts=Array.Empty<CompetitionHeart>();
            for(int i=0;i<5;i++)Place(s,i,new Point2(12+i%4,14+i/4));
            return s;
        }
        private static void Place(CompetitionSnapshot s,int i,Point2 pos,int heading=1,bool moving=false)
        {
            var p=s.participants[i];p.position=pos;p.heading=heading;p.moving=moving;p.queuedHeading=-1;p.trail=new List<Point2>{pos};
            for(int j=0;j<s.crates.Length;j++)p.insideCrates[j]=(s.crates[j].cell.Position-pos).Length<=CompetitionSession.PickupRadius;
        }
        private IEnumerator CheckSmoke()
        {
            CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo("da-DK");CultureInfo.CurrentUICulture=CultureInfo.GetCultureInfo("da-DK");
            Require(app&&app.InMenu,"Competition app did not boot");Require(PlatformAudio.PlaybackPolicyActive,"Playback audio policy missing");
            touch=InputSystem.AddDevice<Touchscreen>();
            Require(GameObject.Find("Main menu actions").GetComponentsInChildren<Button>().Length==3,"Main menu must contain exactly three actions");
            yield return new WaitForEndOfFrame();Capture("01-menu");
            yield return Tap("HELP");Require(GameObject.Find("How to play"),"Help did not open");yield return new WaitForEndOfFrame();Capture("02-help");yield return Tap("BACK");
            yield return Tap("PLAY");Require(app.Selecting&&GameObject.Find("Character selection"),"Selection did not open");yield return new WaitForEndOfFrame();Capture("03-selection");
            foreach(string id in CompetitionSession.ParticipantIds){
                app.ChooseCharacter();yield return null;
                var card=FindButton("Character card "+id);var photo=card.GetComponentInChildren<RawImage>();
                Require(photo&&photo.GetComponentInParent<Button>()==card,"Photo must belong to its character card");
                yield return TapAt(photo.rectTransform);Require(!app.InMenu&&app.Session.HumanId==id,"Photo tap did not select "+id);
                app.ChooseCharacter();yield return null;card=FindButton("Character card "+id);
                yield return TapAt(card.GetComponentInChildren<Text>().rectTransform);Require(!app.InMenu&&app.Session.HumanId==id&&app.Session.ComputerPlayers,"Name tap did not select "+id);
                Require(app.View.Camera.orthographic&&Math.Abs(app.View.Camera.orthographicSize-4.5f)<.01,"Close camera changed");
            }
            float readyDeadline=Time.realtimeSinceStartup+20;
            while(app.Session.Phase==CompetitionPhase.Countdown&&Time.realtimeSinceStartup<readyDeadline)yield return null;
            Require(app.Session.Phase==CompetitionPhase.Playing,"Countdown did not end");
            Require(GameObject.Find("Minimap")&&GameObject.Find("Minimap panel"),"Minimap missing");
            yield return new WaitForEndOfFrame();Capture("03b-random-start-minimap");
            for(int handed=0;handed<2;handed++){
                app.Settings.leftHanded=handed==1;app.Hud.ApplyHandedness();yield return null;
                for(int h=0;h<4;h++){
                    var f=Fixture();Place(f,app.Session.HumanIndex,new Point2(6,5),1,true);f.participants[app.Session.HumanIndex].inventory=CompetitionItem.Boost;app.RestoreForCheck(f);yield return null;
                    var joy=app.Hud.Joystick;Vector2 p=RectTransformUtility.WorldToScreenPoint(null,joy.transform.position);Point2 projected=app.View.ScreenDirection(CorridorNavigation.Direction(h));Vector2 d=new Vector2((float)projected.x,(float)projected.y).normalized*70*joy.GetComponentInParent<Canvas>().scaleFactor;
                    Touch(101,TouchPhase.Began,p+d);yield return new WaitForSecondsRealtime(.08f);
                    var a=app.Session.Participant(app.Session.HumanId);Require(a.heading==h||a.queuedHeading==h,"Screen heading mismatch "+h);
                    yield return Tap("Use item");Require(app.Session.Actor(app.Session.HumanIndex).BoostUntil>app.Session.TickIndex,"Simultaneous item touch failed");
                    Touch(101,TouchPhase.Ended,p+d);yield return new WaitForSecondsRealtime(.1f);Require(joy.Movement==Vector2.zero,"Joystick release failed");
                }
            }
            app.Settings.leftHanded=false;app.Hud.ApplyHandedness();
            for(int item=1;item<=5;item++){
                var f=Fixture();int hi=app.Session.HumanIndex;Place(f,hi,new Point2(6,5),1,true);f.participants[hi].inventory=(CompetitionItem)item;
                if(item==2)f.participants[hi].trail=new List<Point2>{new Point2(5.4,5),new Point2(6,5)};
                app.RestoreForCheck(f);yield return null;yield return Tap("Use item");Require(app.Session.Actor(hi).Inventory==CompetitionItem.None,"Item was not used "+item);
                yield return new WaitForEndOfFrame();Capture("0"+(item+3)+"-item-"+item);
                app.TogglePause();long tick=app.Session.TickIndex;app.Save();app.GoToMenu();app.ContinueSaved();Require(app.Session.Phase==CompetitionPhase.Paused&&app.Session.TickIndex==tick,"Mid-item checkpoint changed clock");app.Session.Resume(false);yield return new WaitForSecondsRealtime(1.1f);
            }
            var splash=Fixture();int splashHuman=app.Session.HumanIndex;Place(splash,0,new Point2(6,5));Place(splash,splashHuman,new Point2(9.8,5));splash.participants[0].inventory=CompetitionItem.WaterBalloon;
            app.RestoreForCheck(splash);app.Session.UseItem("kim",app.Session.TickIndex);float splashDeadline=Time.realtimeSinceStartup+6;
            while(app.Session.Actor(splashHuman).Life==CompetitionLife.Active&&Time.realtimeSinceStartup<splashDeadline)yield return null;
            Require(app.Session.Actor(splashHuman).Life==CompetitionLife.KnockedOut,"Balloon radius did not hit nearby opponent");yield return new WaitForEndOfFrame();Capture("08b-balloon-splash");
            var ko=Fixture();int human=app.Session.HumanIndex;Place(ko,human,new Point2(9,5));Place(ko,0,new Point2(6,5));ko.participants[0].inventory=CompetitionItem.CreamPie;ko.participants[human].roundScore=42;app.RestoreForCheck(ko);app.Session.UseItem("kim",app.Session.TickIndex);
            yield return new WaitForSecondsRealtime(1.3f);Require(app.Session.Actor(human).Life==CompetitionLife.KnockedOut&&app.Session.Actor(human).RoundScore==42,"KO must retain score");yield return new WaitForEndOfFrame();Capture("09-knockout");
            app.TogglePause();long frozen=app.Session.TickIndex;yield return new WaitForSecondsRealtime(.3f);Require(app.Session.TickIndex==frozen,"Pause advanced KO timer");yield return Tap("SETTINGS");yield return new WaitForEndOfFrame();Capture("10-settings");yield return Tap("BACK");app.TogglePause();app.Session.Resume(false);
            float respawnDeadline=Time.realtimeSinceStartup+20;
            while((app.Session.Phase==CompetitionPhase.Countdown||app.Session.Actor(human).Life!=CompetitionLife.Active)&&Time.realtimeSinceStartup<respawnDeadline)yield return null;
            Require(app.Session.Actor(human).Life==CompetitionLife.Active,"Respawn missing");yield return new WaitForEndOfFrame();Capture("11-respawn");
            foreach(var renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.None))foreach(var material in renderer.sharedMaterials)Require(material&&material.shader&&material.shader.isSupported&&!material.shader.name.Contains("InternalError"),"Unsupported scene material");
            var lastHeart=Fixture();lastHeart.rules.endWhenHeartsCollected=true;int lastHuman=app.Session.HumanIndex;Place(lastHeart,lastHuman,new Point2(6,5));lastHeart.hearts=new[]{new CompetitionHeart{id=1,cell=new Cell(6,5)}};lastHeart.nextHeartId=2;
            app.RestoreForCheck(lastHeart);yield return null;yield return null;
            Require(app.Session.Phase==CompetitionPhase.RoundResults&&app.Session.RoundEnd==CompetitionRoundEnd.HeartsCollected&&app.Session.RoundElapsedTicks<60,"Last heart did not end round immediately");
            Require(app.Session.Actor(lastHuman).TotalScore==1&&app.Session.Actor(lastHuman).Trophies==1,"Last point/trophy lost");
            app.Save();app.GoToMenu();app.ContinueSaved();app.Session.Resume(false);yield return null;
            Require(app.Session.Phase==CompetitionPhase.RoundResults&&app.Session.Actor(lastHuman).Trophies==1,"Early result restore duplicated or lost trophy");yield return new WaitForEndOfFrame();Capture("11b-last-heart-results");
            foreach(bool match in new[]{false,true}){
                var f=Fixture();f.tick=7199;f.roundElapsedTicks=7199;f.nextHeartWaveTick=7800;f.nextWallAttemptTick=8100;
                for(int i=0;i<5;i++){f.participants[i].roundScore=match?10:50-i*8;f.participants[i].moving=false;}
                if(match){f.roundNumber=3;f.settledRound=2;f.roundStartedTick=14400;f.tick=21599;f.nextHeartWaveTick=f.tick+600;f.nextWallAttemptTick=f.tick+900;foreach(var p in f.participants){p.trophies=2;p.bankedScore=20;}}
                app.RestoreForCheck(f);yield return new WaitForSecondsRealtime(.1f);Require(app.Session.Phase==(match?CompetitionPhase.MatchResults:CompetitionPhase.RoundResults),"Result flow missing");Require(app.Session.RoundWinners.Count==(match?5:1),"Round trophies incorrect");Require(GameObject.Find("Competition results").GetComponentsInChildren<RawImage>().Length==(match?5:3),"Result photos missing");yield return new WaitForEndOfFrame();Capture(match?"13-match-results":"12-round-results");
                if(!match){yield return Tap("NEXT ROUND");Require(app.Session.RoundNumber==2,"Next round failed");}
            }
            if(Application.isMobilePlatform){
                var orientationFixture=Fixture();Place(orientationFixture,app.Session.HumanIndex,new Point2(6.4,5));orientationFixture.participants[app.Session.HumanIndex].inventory=CompetitionItem.WaterBalloon;app.RestoreForCheck(orientationFixture);
                foreach(var orientation in new[]{ScreenOrientation.LandscapeLeft,ScreenOrientation.LandscapeRight}){Screen.orientation=orientation;float until=Time.realtimeSinceStartup+8;while(Screen.orientation!=orientation&&Time.realtimeSinceStartup<until)yield return null;yield return new WaitForSecondsRealtime(.6f);Require(Screen.orientation==orientation&&Screen.width>Screen.height,"Landscape failed");var safe=GameObject.Find("Safe area").GetComponent<RectTransform>();Vector3[] corners=new Vector3[4];safe.GetWorldCorners(corners);Require(Mathf.Abs(corners[0].x-Screen.safeArea.xMin)<2&&Mathf.Abs(corners[2].x-Screen.safeArea.xMax)<2&&Mathf.Abs(corners[0].y-Screen.safeArea.yMin)<2&&Mathf.Abs(corners[2].y-Screen.safeArea.yMax)<2,"Safe area failed");Require(GameObject.Find("Competition HUD")&&GameObject.Find("Movement joystick")&&FindButton("Use item"),"Gameplay controls missing after rotation");yield return new WaitForEndOfFrame();Capture("14-"+orientation);}Screen.orientation=ScreenOrientation.AutoRotation;
            }
            app.GoToMenu();yield return new WaitForEndOfFrame();Capture("15-menu-after-match");InputSystem.RemoveDevice(touch);
        }
        private IEnumerator CheckLifecycle()
        {
            var f=Fixture();f.computerPlayers=true;f.participants[app.Session.HumanIndex].inventory=CompetitionItem.CreamPie;Place(f,app.Session.HumanIndex,new Point2(6,5),1,true);app.RestoreForCheck(f);app.Session.UseItem(app.Session.HumanId,app.Session.TickIndex);
            // Freeze a real in-flight attack, active boost and KO in the background.
            var k=app.Session.Snapshot();k.participants[1].life=CompetitionLife.KnockedOut;k.participants[1].knockoutAt=k.tick;k.participants[1].knockoutUntil=k.tick+300;k.participants[2].boostUntil=k.tick+240;app.RestoreForCheck(k);
            ready=true;Debug.Log("LIFECYCLE READY");float deadline=Time.realtimeSinceStartup+120;
            while(!resumed&&Time.realtimeSinceStartup<deadline)yield return null;
            yield return null;
            Require(paused&&resumed&&Time.realtimeSinceStartup-backgroundAt>=5,"Real OS lifecycle callbacks missing");
            Require(app.Session.Phase==CompetitionPhase.Paused&&app.Session.TickIndex==pausedTick,"Background advanced clock");
            Require(JsonConvert.SerializeObject(app.Session.Snapshot())==pausedState,"Background changed combat/AI snapshot");
            var restored=new CompetitionSession(app.Session.Maze.Definition);Require(new CompetitionSaveStore(StorageDirectory).TryLoad(restored),"Background save missing");
            Require(JsonConvert.SerializeObject(restored.Snapshot())==pausedState,"Disk checkpoint differs");Require(PlatformAudio.PlaybackPolicyActive,"Audio policy lost after foreground");
            app.TogglePause();yield return new WaitForSecondsRealtime(.4f);Require(app.Session.Phase==CompetitionPhase.Countdown&&app.Session.TickIndex==pausedTick,"Resume countdown advanced active clocks");
            AtomicFile.Write(Path.Combine(output,"report.json"),JsonConvert.SerializeObject(new{passed=true,model="competition-v4",realOsPause=paused,realOsResume=resumed,backgroundSeconds=Time.realtimeSinceStartup-backgroundAt,frozenTick=pausedTick,checkpointVerified=true,combatAndAiMemoryFrozen=true,playbackAudioAfterForeground=PlatformAudio.PlaybackPolicyActive},Formatting.Indented));
        }
        private void OnApplicationPause(bool value)
        {
            if(Performance&&value&&clock.IsRunning){backgroundInterruptions++;error="Application backgrounded during performance run";}
            if(!Lifecycle||!ready)return;
            if(value){paused=true;backgroundAt=Time.realtimeSinceStartup;app.Session.Pause();pausedTick=app.Session.TickIndex;pausedState=JsonConvert.SerializeObject(app.Session.Snapshot());}
            else if(paused)resumed=true;
        }
        private IEnumerator BeginPerformance()
        {
            duration=Read("MAZE_PERFORMANCE_SECONDS",900);warmup=Read("MAZE_PERFORMANCE_WARMUP_SECONDS",15);
            Require(duration>=10&&duration<=3600,"Invalid duration");app.Settings.haptics=false;app.Settings.itemHelpSeen=true;app.NewMatch("zelenskyy");
            gcRecorder=Counter("GC Allocated In Frame");cpuRecorder=Counter("CPU Main Thread Frame Time");gpuRecorder=Counter("GPU Frame Time");DeviceMetrics.Begin();clock.Start();nextProgress=warmup+30;
            Debug.Log("PERFORMANCE START competition-v4 seconds="+duration+" development="+Debug.isDebugBuild);yield break;
        }
        private static double Read(string name,double fallback)=>double.TryParse(Environment.GetEnvironmentVariable(name),NumberStyles.Float,CultureInfo.InvariantCulture,out double v)?v:fallback;
        private static ProfilerRecorder Counter(string name)
        {
            var handles=new List<ProfilerRecorderHandle>();ProfilerRecorderHandle.GetAvailable(handles);foreach(var h in handles){var d=ProfilerRecorderHandle.GetDescription(h);if(d.Name==name)return ProfilerRecorder.StartNew(d.Category,name,1);}return default;
        }
        public static void Drive(CompetitionApp app)
        {
            var run=instance;if(!run||!Performance||!run.clock.IsRunning)return;var s=app.Session;
            if(s.Phase==CompetitionPhase.RoundResults||s.Phase==CompetitionPhase.MatchResults){if(run.resultSince<0)run.resultSince=run.clock.Elapsed.TotalSeconds;
                if(run.clock.Elapsed.TotalSeconds-run.resultSince>2){run.Accumulate();if(s.Phase==CompetitionPhase.MatchResults){run.matches++;app.NewMatch(CompetitionSession.ParticipantIds[run.matches%5]);run.baseline=null;}else app.NextRound();run.rounds++;run.resultSince=-1;}}
        }
        private void Accumulate()
        {
            var current=app.Session.Statistics;for(int i=0;i<5;i++){uses[i]+=current.usesByActor[i]-(baseline?.usesByActor[i]??0);hearts[i]=Math.Max(hearts[i],app.Session.Actor(i).TotalScore);}knockouts+=current.knockouts-(baseline?.knockouts??0);collected+=current.hearts-(baseline?.hearts??0);itemUses+=current.itemUses-(baseline?.itemUses??0);baseline=current;
        }
        private void LateUpdate()
        {
            if(!Performance||!clock.IsRunning||finished)return;
            try{
                double now=clock.Elapsed.TotalSeconds,delta=now-previous;previous=now;if(error!=null)throw new Exception(error);
                if(now<warmup)return;if(measurement<0){measurement=now;nextSample=now;baseline=app.Session.Statistics;return;}
                frames.Add(delta*1000);if(cpuRecorder.Valid&&cpuRecorder.LastValue>0)cpu.Add(cpuRecorder.LastValue*1e-6);if(gpuRecorder.Valid&&gpuRecorder.LastValue>0)gpu.Add(gpuRecorder.LastValue*1e-6);if(gcRecorder.Valid)gcBytes+=gcRecorder.LastValue;
                if(app.Session.Phase==CompetitionPhase.Playing)playingSeconds+=delta;else if(app.Session.Phase==CompetitionPhase.Countdown)countdownSeconds+=delta;else resultsSeconds+=delta;
                if(app.Session.Phase==CompetitionPhase.Paused)throw new Exception("Unexpected pause");
                for(int i=0;i<5;i++){var a=app.Session.Actor(i);if(a.Life==CompetitionLife.Active&&!app.Session.Maze.CanOccupy(a.Position,.01,true))throw new Exception("Actor entered wall");}
                if(now>=nextSample){Sample(now-measurement);nextSample=now+5;}if(now>=nextProgress){Debug.Log("PERFORMANCE PROGRESS elapsed="+(now-measurement).ToString("F0")+"s fps="+frames.FramesPerSecond.ToString("F2")+" hearts="+app.Session.Statistics.hearts+" knockouts="+app.Session.Statistics.knockouts+" thermal="+DeviceMetrics.Thermal());nextProgress=now+30;}
                if(now-measurement>=duration)FinishPerformance(true);
            }catch(Exception e){error=e.ToString();FinishPerformance(false);}
        }
        private void Sample(double elapsed)
        {
            long resident=DeviceMetrics.Resident(),unity=Profiler.GetTotalAllocatedMemoryLong(),managed=GC.GetTotalMemory(false);residentPeak=Math.Max(residentPeak,resident);unityPeak=Math.Max(unityPeak,unity);managedPeak=Math.Max(managedPeak,managed);
            var sample=new{elapsedSeconds=elapsed,fps=frames.FramesPerSecond,p95Ms=frames.Percentile(.95),residentBytes=resident,unityAllocatedBytes=unity,managedBytes=managed,thermalState=DeviceMetrics.Thermal(),lowPowerMode=DeviceMetrics.LowPower(),batteryLevel=DeviceMetrics.BatteryLevel(),round=app.Session.RoundNumber,statistics=app.Session.Statistics};samples.Add(sample);AtomicFile.Write(Path.Combine(output,"checkpoint.json"),JsonConvert.SerializeObject(sample));
        }
        private void FinishPerformance(bool complete)
        {
            if(finished)return;finished=true;Accumulate();clock.Stop();double measured=measurement<0?0:clock.Elapsed.TotalSeconds-measurement;bool stable=complete&&error==null&&backgroundInterruptions==0&&playingSeconds>=duration*.8;
            foreach(long n in uses)if(n<1)stable=false;
            Sample(measured);AtomicFile.Write(Path.Combine(output,"report.json"),JsonConvert.SerializeObject(new{schemaVersion=2,model="competition-v4",completed=complete,stabilityPassed=stable,error,developmentBuild=Debug.isDebugBuild,requestedSeconds=duration,measuredSeconds=measured,warmupSeconds=warmup,automationPolicy="same-shared-minimap-brain-local-combat-for-human-v3",utc=DateTime.UtcNow.ToString("O"),appVersion=Application.version,unityVersion=Application.unityVersion,buildGuid=Application.buildGUID,deviceModel=SystemInfo.deviceModel,operatingSystem=SystemInfo.operatingSystem,gpu=SystemInfo.graphicsDeviceName,width=Screen.width,height=Screen.height,frameCount=frames.Count,averageFps=frames.FramesPerSecond,p95Ms=frames.Percentile(.95),p99Ms=frames.Percentile(.99),worstMs=frames.WorstMs,framesOver33Ms=frames.Over33Ms,framesOver50Ms=frames.Over50Ms,cpuTimingFrames=cpu.Count,cpuMainThreadAverageMs=cpu.AverageMs,gpuTimingFrames=gpu.Count,gpuAverageMs=gpu.AverageMs,gcCounterAvailable=gcRecorder.Valid,gcAllocatedBytes=gcBytes,residentPeakBytes=residentPeak,unityAllocatedPeakBytes=unityPeak,managedPeakBytes=managedPeak,playingSeconds,countdownSeconds,resultsSeconds,rounds,matches,heartsCollected=collected,itemsUsed=itemUses,captures=knockouts,usesByActor=uses,maximumTotalPointsByActor=hearts,knockouts,backgroundInterruptions,samples,notes="Production four AI plus the same shared-map/local-combat brain supplying human input. Normal attacks, KO, centre respawn and rounds. No fixtures or invulnerability. Recorder and checkpoints add overhead. USB charging; battery drain not assessed. Automated evidence does not establish subjective fun or manual accessibility."},Formatting.Indented));
            File.WriteAllText(Path.Combine(output,"result.txt"),(stable?"PASS ":"FAIL ")+"competition Release performance stability; inspect report.json. "+error);Debug.Log(stable?"PERFORMANCE COMPLETED":"PERFORMANCE FAILED: "+error);gcRecorder.Dispose();cpuRecorder.Dispose();gpuRecorder.Dispose();DeviceMetrics.End();Application.Quit(stable?0:1);
        }
        private void FinishSimple(bool passed)
        {
            if(finished)return;finished=true;string token=Lifecycle?"LIFECYCLE":"RUNTIME SMOKE";
            File.WriteAllText(Path.Combine(output,"result.txt"),(passed?"PASS ":"FAIL ")+"competition-v4 "+token+"; "+(passed?"real app flow, selected characters, combat, save/resume and English UI. Controlled fixtures and synthetic touch are distinct from live AI performance.":error));
            Debug.Log(token+(passed?" PASSED":" FAILED: "+error));Application.Quit(passed?0:1);
        }
        private void OnDestroy(){Application.logMessageReceived-=OnLog;gcRecorder.Dispose();cpuRecorder.Dispose();gpuRecorder.Dispose();instance=null;}
    }
}
#endif
