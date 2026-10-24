using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using MazeOfEgos.Core;
internal static class ArenaChecks
{
    static readonly JsonSerializerOptions Json = new(){IncludeFields=true,IgnoreReadOnlyProperties=true};
    static MazeDefinition Definition()=>JsonSerializer.Deserialize<MazeDefinition>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"classic_reference.json")),Json);
    static int passed,failed;
    static void Assert(bool b,string message="Assertion failed"){if(!b)throw new Exception(message);}
    static void Run(string name,Action action){try{action();passed++;Console.WriteLine("PASS "+name);}catch(Exception e){failed++;Console.WriteLine("FAIL "+name+": "+e);}}
    static void Step(CompetitionSession s,int ticks){for(int t=0;t<ticks;t++)s.Tick();}
    static string Text(CompetitionSession s)=>JsonSerializer.Serialize(s.Snapshot(),Json);
    static CompetitionSession Fresh(){var s=new CompetitionSession(Definition(),countdown:false,profile:new CompetitionRules{movingWalls=false,randomRoundStarts=false,sharedMinimap=false,endWhenHeartsCollected=false});var f=s.Snapshot();foreach(var p in f.participants)p.moving=false;f.hearts=Array.Empty<CompetitionHeart>();Apply(s,f);return s;}
    static void Apply(CompetitionSession s,CompetitionSnapshot f){s.Restore(f);s.Resume(false);}
    static void Place(CompetitionSnapshot f,int i,double x,double y,int heading=1,bool moving=false){var p=f.participants[i];p.position=new Point2(x,y);p.heading=heading;p.queuedHeading=-1;p.moving=moving;p.trail=new(){p.position};for(int j=0;j<f.crates.Length;j++)p.insideCrates[j]=(p.position-f.crates[j].cell.Position).Length<=CompetitionSession.PickupRadius;}
    static void Reject(Action a){try{a();}catch(ArgumentException){return;}throw new Exception("Invalid data accepted");}
    static int Main(string[] args)
    {
        Run("All five actors have the identical five-item pool and effects",()=>{
            for(int i=0;i<5;i++)for(int item=1;item<=5;item++){
                var s=Fresh();var f=s.Snapshot();Place(f,i,6,5);f.participants[i].inventory=(CompetitionItem)item;if(item==2)f.participants[i].trail.Insert(0,new Point2(5.4,5));Apply(s,f);
                Assert(s.UseItem(f.participants[i].id,0));Assert(s.Actor(i).Inventory==CompetitionItem.None&&s.Statistics.usesByActor[i]==1);
                if(item==3)Assert(Math.Abs(s.Speed(f.participants[i].id)-2.56)<1e-8);else Assert(s.AttackCount==1);
            }
        });
        Run("Cream pie warns before flight, uses heading and knocks out once without score/inventory loss",()=>{
            var s=Fresh();var f=s.Snapshot();Place(f,0,6,5);Place(f,1,8,5);f.participants[0].inventory=CompetitionItem.CreamPie;f.participants[1].roundScore=19;f.participants[1].inventory=CompetitionItem.Boost;Apply(s,f);
            s.UseItem("kim",0);Assert(s.Attack(0).launchTick==21);Step(s,21);Assert(s.Actor(1).Life==CompetitionLife.Active);Step(s,40);Assert(s.Actor(1).Life==CompetitionLife.KnockedOut&&s.Actor(1).RoundScore==19&&s.Actor(1).Inventory==CompetitionItem.Boost&&s.Statistics.knockouts==1);
        });
        Run("Water balloon has a warned small landing area and can hit two nearby opponents",()=>{
            var s=Fresh();var f=s.Snapshot();Place(f,0,6,5);Place(f,1,9,5);Place(f,2,9.5,5);Place(f,3,10.05,5);f.participants[0].inventory=CompetitionItem.WaterBalloon;Apply(s,f);s.UseItem("kim",0);Step(s,56);Assert(s.Statistics.knockouts==0);s.Tick();Assert(s.Actor(1).Life==CompetitionLife.KnockedOut&&s.Actor(2).Life==CompetitionLife.KnockedOut&&s.Actor(3).Life==CompetitionLife.Active);
        });
        Run("Balloon hits every opponent inside or on its one-cell landing radius, but none outside",()=>{
            var s=Fresh();var f=s.Snapshot();Place(f,0,6,5);Place(f,1,9.8,5);Place(f,2,10,5);Place(f,3,10.0001,5);f.participants[0].inventory=CompetitionItem.WaterBalloon;Apply(s,f);s.UseItem("kim",0);Step(s,57);
            Assert(s.SplashRadius==1&&s.Actor(1).Life==CompetitionLife.KnockedOut&&s.Actor(2).Life==CompetitionLife.KnockedOut&&s.Actor(3).Life==CompetitionLife.Active);
            Assert(s.AttackCount==1,"Landing splash must remain briefly visible");f=s.Snapshot();Place(f,3,9.5,5);Apply(s,f);Step(s,19);
            Assert(s.Actor(3).Life==CompetitionLife.Active&&s.Statistics.knockouts==2&&s.AttackCount==0,"Splash must hit once at landing, not become a lingering trap");
        });
        Run("Balloon landing restores at its last airborne tick with identical hits and cleanup",()=>{
            var a=Fresh();var f=a.Snapshot();Place(f,0,6,5);Place(f,1,9.9,5);f.participants[0].inventory=CompetitionItem.WaterBalloon;Apply(a,f);a.UseItem("kim",0);Step(a,56);
            var b=Fresh();b.Restore(a.Snapshot());b.Resume(false);Step(a,20);Step(b,20);Assert(Text(a)==Text(b)&&a.Statistics.knockouts==1&&a.AttackCount==0);
        });
        Run("Banana slips and oil slows; neither knocks out or steals points; owner is immune",()=>{
            foreach(var item in new[]{CompetitionItem.Banana,CompetitionItem.Oil}){
                var s=Fresh();var f=s.Snapshot();Place(f,0,6,5);Place(f,1,item==CompetitionItem.Banana?8:5.5,5);f.participants[0].inventory=item;f.participants[0].trail.Insert(0,new Point2(5.4,5));f.participants[1].roundScore=8;Apply(s,f);s.UseItem("kim",0);Step(s,23);
                Assert(s.Actor(1).Life==CompetitionLife.Active&&s.Actor(1).RoundScore==8&&s.Statistics.knockouts==0);Assert(item==CompetitionItem.Banana?s.Actor(1).SlipUntil>s.TickIndex:s.Actor(1).OilUntil>s.TickIndex);Assert(s.Actor(0).SlipUntil==0&&s.Actor(0).OilUntil==0);
            }
        });
        Run("KO waits 300 active ticks, respawns at centre and protects for 180; body contact is harmless",()=>{
            var s=Fresh();var f=s.Snapshot();Place(f,1,9,5);f.participants[1].life=CompetitionLife.KnockedOut;f.participants[1].knockoutUntil=300;f.participants[1].roundScore=31;Apply(s,f);Step(s,300);Assert(s.Actor(1).Life==CompetitionLife.KnockedOut);s.Tick();Assert(s.Actor(1).Life==CompetitionLife.Active&&s.Actor(1).ProtectedUntil==480&&s.Actor(1).RoundScore==31);Assert(s.Actor(1).Position.y>10&&s.Actor(1).Position.y<16);Assert(s.Statistics.respawns==1);
        });
        Run("Own attacks end respawn protection; boost retains it; protected targets ignore attacks",()=>{
            foreach(var item in new[]{CompetitionItem.Boost,CompetitionItem.CreamPie,CompetitionItem.WaterBalloon}){
                var s=Fresh();var f=s.Snapshot();Place(f,0,6,5);Place(f,1,9,5);f.participants[0].protectedUntil=f.participants[1].protectedUntil=180;f.participants[0].inventory=item;Apply(s,f);s.UseItem("kim",0);Assert(s.Actor(0).ProtectedUntil==(item==CompetitionItem.Boost?180:0));Step(s,100);Assert(s.Actor(1).Life==CompetitionLife.Active);
            }
        });
        Run("Walls block heading attacks and splash; moving gate reservations include projectiles",()=>{
            var s=Fresh();var f=s.Snapshot();Place(f,0,12,1,1);Place(f,1,15,1);f.participants[0].inventory=CompetitionItem.CreamPie;Apply(s,f);s.UseItem("kim",0);Assert(s.Attack(0).target.x<13);Step(s,100);Assert(s.Actor(1).Life==CompetitionLife.Active);
            var map=new MazeMap(Definition());for(int h=0;h<4;h++){var d=CorridorNavigation.Direction(h);var origin=new Point2(6,5);Assert(map.CanOccupy(origin,.05,true));}
        });
        Run("Pause/countdown freeze active attacks, KO, protection and all AI memory across JSON restore",()=>{
            var s=Fresh();var f=s.Snapshot();Place(f,0,6,5);f.participants[0].inventory=CompetitionItem.WaterBalloon;f.participants[1].life=CompetitionLife.KnockedOut;f.participants[1].knockoutUntil=300;Apply(s,f);s.UseItem("kim",0);Step(s,12);s.Pause();string before=Text(s);Step(s,1000);Assert(Text(s)==before);
            var copy=new CompetitionSession(Definition());copy.Restore(JsonSerializer.Deserialize<CompetitionSnapshot>(before,Json));Assert(Text(copy)==before);s.Resume();Step(s,180);copy.Resume(false);Assert(Text(copy)==Text(s));Step(s,100);Step(copy,100);Assert(Text(s)==Text(copy));
        });
        Run("Attacks arriving exactly at the round deadline do not knock out; earlier landing does",()=>{
            foreach(bool early in new[]{true,false}){var s=Fresh();var f=s.Snapshot();Place(f,0,6,5);Place(f,1,9,5);f.participants[0].inventory=CompetitionItem.WaterBalloon;f.tick=f.roundElapsedTicks=early?7142:7143;f.nextHeartWaveTick=7800;f.nextWallAttemptTick=8100;Apply(s,f);s.UseItem("kim",s.TickIndex);Step(s,58);Assert((s.Actor(1).Life==CompetitionLife.KnockedOut)==early);}
        });
        Run("Version-one competition save migrates without mutating its envelope or classic enum values",()=>{
            var s=Fresh();var f=s.Snapshot();f.version=1;f.rules.id="competition-n2-v1";f.attacks=null;f.brains=null;f.statistics=null;var old=f.rules.id;var copy=new CompetitionSession(Definition());copy.Restore(f);Assert(copy.Snapshot().version==4&&f.version==1&&f.rules.id==old);Assert((int)ItemKind.SpeedBoost==3);
        });
        Run("Malformed combat and brain snapshots reject atomically",()=>{
            foreach(Action<CompetitionSnapshot> mutate in new Action<CompetitionSnapshot>[] {f=>f.attacks=null,f=>f.brains=null,f=>f.brains[0].random=0,f=>f.participants[0].trail=null,f=>f.participants[0].knockoutUntil=99999,f=>f.viewAspect=double.NaN,f=>f.statistics.usesByActor=null,f=>f.brains[0].resources.Add(new(){cell=new Cell(-1,0)})}){
                var s=Fresh();string before=Text(s);var f=s.Snapshot();mutate(f);Reject(()=>s.Restore(f));Assert(Text(s)==before);
            }
        });
        Run("AI observations cannot see distant actors or resources and decisions ignore unseen changes",()=>{
            var a=Fresh();var f=a.Snapshot();Place(f,0,6,5);Place(f,1,21,23);Apply(a,f);var b=Fresh();var changed=a.Snapshot();Place(changed,1,1,29);changed.hearts=changed.hearts.Where(h=>(h.cell.Position-new Point2(20,23)).Length>2).ToArray();Apply(b,changed);
            string x=JsonSerializer.Serialize(a.Observe("kim"),Json),y=JsonSerializer.Serialize(b.Observe("kim"),Json);Assert(x==y,"Hidden actor/resource leaked into observation");
            var memory=a.Snapshot().brains[0];var brain=new CompetitionBrain(Definition());var d=brain.Decide(a.Observe("kim"),memory,a.Rules);var memory2=b.Snapshot().brains[0];var e=new CompetitionBrain(Definition()).Decide(b.Observe("kim"),memory2,b.Rules);Assert(d.Heading==e.Heading&&d.UseItem==e.UseItem&&JsonSerializer.Serialize(memory,Json)==JsonSerializer.Serialize(memory2,Json));Assert(memory.resources.Count>0&&memory.actors.All(p=>!p.seen));
        });
        Run("Shared minimap gives all five identical hearts and active positions, with combat details kept local",()=>{
            var s=new CompetitionSession(Definition(),countdown:false);var f=s.Snapshot();Place(f,0,6,5);Place(f,1,21,23);f.participants[1].inventory=CompetitionItem.CreamPie;f.participants[2].life=CompetitionLife.KnockedOut;f.participants[2].knockoutUntil=300;Apply(s,f);
            foreach(string id in CompetitionSession.ParticipantIds){var obs=s.Observe(id);Assert(obs.sharedMinimap&&obs.mapHearts.Length==s.HeartCount&&obs.mapActors.Length==4);Assert(obs.mapActors.All(a=>a.Id!="trump"));Assert(obs.mapHearts.SequenceEqual(s.Hearts.Select(h=>h.cell)));}
            var local=s.Observe("kim");Assert(!local.actors.Any(a=>a.Id=="putin")&&local.mapActors.Any(a=>a.Id=="putin"&&(a.Position-s.Actor(1).Position).Length<1e-9));
            Assert(typeof(CompetitionMapActor).GetFields().Length==2&&local.visibleShortcuts.Length==s.Walls.Count);
            string before=JsonSerializer.Serialize(local,Json);var changed=s.Snapshot();changed.participants[1].inventory=CompetitionItem.Boost;changed.participants[1].protectedUntil=180;Apply(s,changed);
            Assert(JsonSerializer.Serialize(s.Observe("kim"),Json)==before,"Minimap leaked hidden inventory or protection");
        });
        Run("AI uses distant map hearts for routes and removes globally collected hearts from memory",()=>{
            var s=new CompetitionSession(Definition(),countdown:false);var f=s.Snapshot();Place(f,0,6,5);foreach(var box in f.crates)box.availableAtTick=600;
            f.hearts=new[]{new CompetitionHeart{id=1,cell=new Cell(21,23)}};f.nextHeartId=2;Apply(s,f);
            var brain=new CompetitionBrain(Definition());var memory=s.Snapshot().brains[0];brain.Decide(s.Observe("kim"),memory,s.Rules);
            Assert(memory.hasGoal&&memory.goal==new Cell(21,23)&&memory.resources.Any(r=>!r.crate&&r.cell==new Cell(21,23)));
            var obs=s.Observe("kim");obs.tick=18;obs.mapHearts=Array.Empty<Cell>();obs.resources=Array.Empty<ObservedResource>();brain.Decide(obs,memory,s.Rules);
            Assert(!memory.resources.Any(r=>!r.crate));
        });
        Run("Brain decisions and reaction delay restore deterministically with production AI and attacks",()=>{
            var a=new CompetitionSession(Definition(),"trump",123,countdown:false,computerPlayers:true);for(int i=0;i<1500;i++){a.DriveHumanForAutomation();a.Tick();a.Events.Clear();}
            var b=new CompetitionSession(Definition());b.Restore(JsonSerializer.Deserialize<CompetitionSnapshot>(Text(a),Json));b.Resume(false);
            for(int i=0;i<1500;i++){a.DriveHumanForAutomation();b.DriveHumanForAutomation();a.Tick();b.Tick();a.Events.Clear();b.Events.Clear();}Assert(Text(a)==Text(b));
        });
        Run("An opening projectile reserves a closing shortcut until the shot has passed",()=>{
            var s=Fresh();var f=s.Snapshot();f.rules.movingWalls=true;f.walls[0].phase=WallPhase.Open;f.walls[0].untilTick=1;Place(f,0,12,1);Place(f,1,16,1);f.participants[0].inventory=CompetitionItem.CreamPie;Apply(s,f);Assert(s.UseItem("kim",0)&&s.PassageReserved(0));Step(s,30);Assert(s.ShortcutPhase(0)==WallPhase.Open);Step(s,100);Assert(s.ShortcutPhase(0)!=WallPhase.Open);
        });
        Run("Simultaneous opposing pies can knock out both throwers; future projectiles survive owner KO",()=>{
            var s=Fresh();var f=s.Snapshot();Place(f,0,6,5);Place(f,1,9,5,3);f.participants[0].inventory=f.participants[1].inventory=CompetitionItem.CreamPie;Apply(s,f);s.UseItem("kim",0);s.UseItem("putin",0);Step(s,80);Assert(s.Actor(0).Life==CompetitionLife.KnockedOut&&s.Actor(1).Life==CompetitionLife.KnockedOut&&s.Statistics.knockouts==2);
        });
        Run("Occupied central starts do not delay respawn or block body movement",()=>{
            var s=Fresh();var f=s.Snapshot();Place(f,0,13,14);Place(f,1,14,14);Place(f,2,12,14);Place(f,3,15,14);Place(f,4,9,5);f.participants[4].life=CompetitionLife.KnockedOut;f.participants[4].knockoutUntil=300;Apply(s,f);Step(s,301);Assert(s.Actor(4).Life==CompetitionLife.Active&&s.Actor(4).ProtectedUntil==480);Assert((s.Actor(4).Position-new Point2(13,15)).Length<.1);
        });
        Run("Round settlement during KO and an airborne item preserves points and clears effects next round",()=>{
            var s=Fresh();var f=s.Snapshot();f.tick=f.roundElapsedTicks=7199;f.nextHeartWaveTick=7800;f.nextWallAttemptTick=8100;Place(f,0,6,5);f.participants[0].inventory=CompetitionItem.WaterBalloon;f.participants[1].life=CompetitionLife.KnockedOut;f.participants[1].knockoutAt=7199;f.participants[1].knockoutUntil=7499;f.participants[1].roundScore=30;Apply(s,f);s.UseItem("kim",s.TickIndex);s.Tick();Assert(s.Phase==CompetitionPhase.RoundResults&&s.Actor(1).TotalScore==30&&s.Actor(1).Trophies==1);s.StartNextRound(false);Assert(s.AttackCount==0&&s.Actor(1).Life==CompetitionLife.Active&&s.Actor(1).TotalScore==30);
        });
        Run("Ten seeded live-AI rounds stress five actors, wall cycles, combat and checkpoint continuation",()=>{
            for(uint seed=1;seed<=10;seed++){
                var s=new CompetitionSession(Definition(),CompetitionSession.ParticipantIds[(int)(seed%5)],seed*7919,countdown:false,computerPlayers:true);
                for(int tick=0;tick<7200;tick++){
                    s.DriveHumanForAutomation();s.Tick();s.Events.Clear();
                    if(tick%600==599){for(int i=0;i<5;i++){var a=s.Actor(i);Assert(a.Position.IsFinite&&(a.Life==CompetitionLife.KnockedOut||s.Maze.CanOccupy(a.Position,.01,true)));}string before=Text(s);s.Restore(JsonSerializer.Deserialize<CompetitionSnapshot>(before,Json));s.Resume(false);Assert(Text(s)==before);}
                }
                Assert(s.Phase==CompetitionPhase.RoundResults||s.Phase==CompetitionPhase.MatchResults);Assert(s.Statistics.hearts>100);
            }
        });
        var reports=new List<MatchReport>();
        Run("Five complete matches use the production shared-map AI, local attacks, KO and centre respawn",()=>{
            uint seed=400;foreach(string human in CompetitionSession.ParticipantIds){var s=new CompetitionSession(Definition(),human,seed++,countdown:false,computerPlayers:true);int ticks=0,rounds=0;
                while(s.Phase!=CompetitionPhase.MatchResults&&ticks<79201){s.DriveHumanForAutomation();s.Tick();s.Events.Clear();ticks++;
                    if(ticks%1800==0){var state=s.Snapshot();s.Restore(state);s.Resume(false);}
                    if(s.Phase==CompetitionPhase.RoundResults||s.Phase==CompetitionPhase.MatchResults){rounds++;if(s.Phase==CompetitionPhase.RoundResults)s.StartNextRound(false);}
                }
                var stats=s.Statistics;Console.WriteLine("MATCH "+human+" rounds="+rounds+" hearts="+stats.hearts+" uses="+string.Join(',',stats.usesByActor)+" KO="+stats.knockouts+" itemTypes="+string.Join(',',stats.usesByItem));Assert(s.Phase==CompetitionPhase.MatchResults&&rounds>=3&&rounds<=11);Assert(s.Participants.All(p=>p.bankedScore>0)&&stats.usesByActor.All(n=>n>0)&&stats.knockouts>0&&stats.respawns>0);
                reports.Add(new MatchReport{humanId=human,seed=seed-1,rounds=rounds,activeTicks=ticks,simulatedSeconds=ticks/60,statistics=stats,winners=s.MatchWinners.ToArray(),participants=s.Participants.ToArray()});
            }
        });
        string report=args.Length>0?args[0]:"Logs/arena-full-matches.json";Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(report)));File.WriteAllText(report,JsonSerializer.Serialize(new ArenaReport{passed=passed,failed=failed,matches=reports},new JsonSerializerOptions(Json){WriteIndented=true}));Console.WriteLine($"{passed} arena checks passed, {failed} failed");return failed==0?0:1;
    }
    private sealed class ArenaReport { public int passed,failed; public List<MatchReport> matches; }
    private sealed class MatchReport { public string humanId; public uint seed; public int rounds,activeTicks,simulatedSeconds; public CompetitionStatistics statistics; public string[] winners; public CompetitionParticipant[] participants; }
}
