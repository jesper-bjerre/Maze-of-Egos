using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MazeOfEgos.Core;

internal static class CompetitionChecks
{
    private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true, IgnoreReadOnlyProperties = true };
    private static readonly string MazeJson = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "classic_reference.json"));
    private static int passed, failed;
    private static MazeDefinition Definition() => JsonSerializer.Deserialize<MazeDefinition>(MazeJson, Json);
    private static string Text(CompetitionSession s) => JsonSerializer.Serialize(s.Snapshot(), Json);
    private static void Assert(bool ok, string message = "Assertion failed") { if (!ok) throw new Exception(message); }
    private static void Equal(double a, double b) => Assert(Math.Abs(a - b) < 1e-7, $"Expected {b}, got {a}");
    private static void Reject(Action action) { try { action(); } catch (ArgumentException) { return; } throw new Exception("Invalid data accepted"); }
    private static void Run(string name, Action action)
    { try { action(); passed++; Console.WriteLine("PASS " + name); } catch (Exception error) { failed++; Console.WriteLine("FAIL " + name + ": " + error.Message); } }
    private static void Step(CompetitionSession s, int ticks) { for (int i = 0; i < ticks; i++) s.Tick(); }
    private static void Apply(CompetitionSession s, CompetitionSnapshot state) { s.Restore(state); s.Resume(false); }
    private static CompetitionSession Fresh(uint seed = 42, string human = "zelenskyy")
    {
        var s = new CompetitionSession(Definition(), human, seed, new CompetitionRules { movingWalls = false, randomRoundStarts = false, replenishHearts = true, endWhenHeartsCollected = false }, false);
        var state = s.Snapshot(); foreach (CompetitionParticipant p in state.participants) p.moving = false;
        Apply(s, state); return s;
    }
    private static void Place(CompetitionSnapshot state, string id, Point2 position, int heading = 1, bool moving = false)
    {
        var p = Array.Find(state.participants, p => p.id == id);
        p.position = position; p.heading = heading; p.queuedHeading = -1; p.moving = moving;
        for (int i = 0; i < state.crates.Length; i++) p.insideCrates[i] = (position - state.crates[i].cell.Position).Length <= CompetitionSession.PickupRadius + 1e-9;
    }
    private static void OnlyHeart(CompetitionSnapshot state, Cell cell)
    { state.hearts = new[] { new CompetitionHeart { id = 1, cell = cell } }; state.nextHeartId = 2; }
    private static void EndRound(CompetitionSession s, params long[] scores)
    {
        var state = s.Snapshot(); state.phase = state.resumePhase = CompetitionPhase.Playing; state.countdownRemaining = 0;
        state.tick = state.roundStartedTick + 7199; state.roundElapsedTicks = 7199;
        state.nextHeartWaveTick = state.tick + 600; state.nextWallAttemptTick = state.tick + 900;
        state.hearts = Array.Empty<CompetitionHeart>();
        for (int i = 0; i < 5; i++) { state.participants[i].roundScore = scores[i]; state.participants[i].moving = false; }
        Apply(s, state); s.Tick();
    }
    private static int Main(string[] args)
    {
        Run("Round starts are random, distinct, separated, outside the centre and immediately traversable", () => {
            var seen = new HashSet<Cell>[5]; for(int i=0;i<5;i++)seen[i]=new HashSet<Cell>();
            for(uint seed=1;seed<=100;seed++) {
                var s=new CompetitionSession(Definition(),seed:seed,countdown:false);
                for(int i=0;i<5;i++) {var a=s.Actor(i);var cell=new Cell((int)a.Position.x,(int)a.Position.y);seen[i].Add(cell);
                    Assert(s.Maze.PlayerCells.Contains(cell)&&!s.Maze.IsHome(cell)&&(a.Position-new Point2(13.5,14)).Length>=5);
                    Assert(s.Maze.IsWalkable(new Cell(cell.x+(int)CorridorNavigation.Direction(a.Heading).x,cell.y+(int)CorridorNavigation.Direction(a.Heading).y),true));
                    for(int j=0;j<i;j++)Assert((s.Maze.Near(a.Position,s.Actor(j).Position)-s.Actor(j).Position).Length>=6);
                }
            }
            Assert(seen.All(c=>c.Count>40),"Random starts lack variation");
        });
        Run("Spawn lottery is human-independent and survives saving into the next round", () => {
            var a=new CompetitionSession(Definition(),seed:91,countdown:false);
            var b=new CompetitionSession(Definition(),humanId:"kim",seed:91,countdown:false);
            Assert(a.Participants.Select(p=>p.position).SequenceEqual(b.Participants.Select(p=>p.position)));
            EndRound(a,5,4,3,2,1);var saved=a.Snapshot();b.Restore(saved);b.Resume(false);
            a.StartNextRound(false);b.StartNextRound(false);Assert(Text(a)==Text(b));
            Assert(a.Snapshot().spawnRandom!=saved.spawnRandom);
        });
        Run("Production hearts never replenish during a round and reset only at the next round", () => {
            var s=new CompetitionSession(Definition(),seed:74,countdown:false,computerPlayers:true);int previous=s.HeartCount;
            for(int tick=0;tick<7200;tick++){s.DriveHumanForAutomation();s.Tick();Assert(s.HeartCount<=previous);previous=s.HeartCount;s.Events.Clear();}
            Assert(s.Participants.Sum(p=>p.roundScore)<=240&&s.HeartCount<140);
            Assert(s.StartNextRound(false)&&s.HeartCount==240&&s.Participants.All(p=>p.roundScore==0));
        });
        Run("An exhausted board ends immediately, preserves scores and settles only once", () => {
            var s=new CompetitionSession(Definition(),countdown:false);var f=s.Snapshot();f.hearts=Array.Empty<CompetitionHeart>();foreach(var p in f.participants)p.moving=false;f.participants[0].roundScore=9;Apply(s,f);
            s.Tick();Assert(s.Phase==CompetitionPhase.RoundResults&&s.RoundEnd==CompetitionRoundEnd.HeartsCollected&&s.TickIndex==1&&s.Actor(0).Trophies==1);
            string done=Text(s);Step(s,100);Assert(Text(s)==done);var restored=new CompetitionSession(Definition());restored.Restore(s.Snapshot());restored.Resume(false);Assert(Text(restored)==done);
        });
        Run("The last collected heart awards its point before early round settlement",()=>{
            var s=new CompetitionSession(Definition(),countdown:false);var f=s.Snapshot();foreach(var p in f.participants)p.moving=false;OnlyHeart(f,new Cell(6,5));Place(f,"trump",new Point2(6,5));Apply(s,f);s.Tick();
            Assert(s.RoundEnd==CompetitionRoundEnd.HeartsCollected&&s.Actor(2).RoundScore==1&&s.Actor(2).TotalScore==1&&s.RoundWinners.SequenceEqual(new[]{"trump"}));
        });
        Run("Consecutive short rounds keep their real cumulative tick and restore deterministically",()=>{
            var a=new CompetitionSession(Definition(),countdown:false);var f=a.Snapshot();f.hearts=Array.Empty<CompetitionHeart>();foreach(var p in f.participants)p.moving=false;Apply(a,f);a.Tick();a.StartNextRound(false);
            Assert(a.TickIndex==1&&a.RoundStartedTick==1);Step(a,123);var b=new CompetitionSession(Definition());b.Restore(a.Snapshot());b.Resume(false);Step(a,120);Step(b,120);Assert(Text(a)==Text(b)&&a.TickIndex==244);
            f=a.Snapshot();f.hearts=Array.Empty<CompetitionHeart>();Apply(a,f);a.Tick();Assert(a.RoundElapsedTicks==244&&a.TickIndex==245&&a.RoundEnd==CompetitionRoundEnd.HeartsCollected);
            var c=new CompetitionSession(Definition());c.Restore(a.Snapshot());c.Resume(false);Assert(Text(a)==Text(c));
        });
        Run("V3 live and result saves migrate to V4 with original clocks and a fresh early-end policy",()=>{
            var original=Fresh();Step(original,90);var f=original.Snapshot();f.version=3;f.rules.id="competition-v3";f.rules.endWhenHeartsCollected=false;f.rules.splashRadius=.75;
            var copy=new CompetitionSession(Definition());copy.Restore(f);Assert(copy.Snapshot().version==4&&copy.TickIndex==90&&copy.RoundStartedTick==0&&copy.Rules.endWhenHeartsCollected&&copy.SplashRadius==1&&f.version==3);
            EndRound(original,5,4,3,2,1);f=original.Snapshot();f.version=3;f.rules.id="competition-v3";copy.Restore(f);Assert(copy.RoundEnd==CompetitionRoundEnd.TimeExpired&&copy.Actor(0).Trophies==1);
        });
        Run("Round medals use round points, share ties and skip occupied ranks", () => {
            var s=new CompetitionSession(Definition(),countdown:false);EndRound(s,20,10,10,3,1);
            Assert(Enumerable.Range(0,5).Select(s.RoundPlace).SequenceEqual(new[]{1,2,2,4,5}));
            Assert(s.Actor(0).Trophies==1&&s.Actor(1).Trophies==0&&s.Actor(2).Trophies==0);
            s.StartNextRound(false);EndRound(s,5,5,4,3,1);Assert(Enumerable.Range(0,5).Select(s.RoundPlace).SequenceEqual(new[]{1,1,3,4,5}));
            Assert(s.OverallPlace(0)==1&&s.OverallPlace(1)==2);
        });
        Run("Build 15 saves retain current positions and resources while upgrading future round rules", () => {
            var s=new CompetitionSession(Definition(),countdown:false);Step(s,700);var f=s.Snapshot();f.version=2;f.rules.id="competition-v2";f.rules.replenishHearts=true;f.rules.randomRoundStarts=false;f.spawnRandom=0;
            var copy=new CompetitionSession(Definition());copy.Restore(f);var restored=copy.Snapshot();
            Assert(restored.version==4&&restored.rules.randomRoundStarts&&!restored.rules.replenishHearts&&restored.spawnRandom!=0);
            Assert(restored.participants.Select(p=>p.position).SequenceEqual(f.participants.Select(p=>p.position))&&restored.hearts.Length==f.hearts.Length);
            Assert(restored.resourceRandom==f.resourceRandom&&f.version==2&&f.rules.id=="competition-v2");
        });
        Run("A corrupt spawn random stream rejects atomically", () => {
            var s=new CompetitionSession(Definition());string before=Text(s);var f=s.Snapshot();f.spawnRandom=0;Reject(()=>s.Restore(f));Assert(Text(s)==before);
        });
        Run("N2 starts five equal stable IDs, 240 ordinary hearts and an independent human ID", () => {
            foreach (string id in CompetitionSession.ParticipantIds)
            {
                var s = Fresh(human: id); Assert(s.HumanId == id && s.Participants.Count == 5 && s.Hearts.Count == 240);
                Assert(s.Participants.Select(p => p.id).Distinct().Count() == 5 && s.RoundSecondsRemaining == 120);
                foreach (CompetitionHeart heart in s.Hearts) Assert(s.Maze.Character(heart.cell) == '.');
                Assert(s.Participants.All(p => p.roundScore == 0 && p.bankedScore == 0 && p.trophies == 0));
            }
        });
        Run("All five actors leave the central base through the same doors; classic access stays closed", () => {
            var s = Fresh(); var state = s.Snapshot();
            foreach (string id in CompetitionSession.ParticipantIds) Place(state, id, new Point2(13, 14), 0, true);
            Apply(s, state); Step(s, 120);
            foreach (var p in s.Participants) { Equal(p.position.x, 13); Equal(p.position.y, 11); }
            var classic = new CorridorNavigation(); classic.Restore(0, -1, true);
            Equal(classic.Advance(new MazeMap(Definition()), new Point2(13, 14), 1).y, 14);
        });
        Run("Actor commands reject stale ticks and inactive phases without changing a match", () => {
            var s = Fresh(); string before = Text(s);
            Assert(!s.ApplyCommand(new CompetitionCommand("trump", s.TickIndex + 1, 1)) && Text(s) == before);
            Reject(() => s.ApplyCommand(new CompetitionCommand("missing", s.TickIndex, 0)));
            Reject(() => s.ApplyCommand(new CompetitionCommand("kim", s.TickIndex, 4)));
            s.Pause(); before = Text(s);
            Assert(!s.ApplyCommand(new CompetitionCommand("kim", s.TickIndex, 0)) && Text(s) == before);
        });
        Run("All five use identical speed and retain a world-heading turn until the junction", () => {
            foreach (string id in CompetitionSession.ParticipantIds)
            {
                var s = Fresh(); var state = s.Snapshot(); Place(state, id, new Point2(10.25, 5), 1, true); Apply(s, state);
                Assert(s.ApplyCommand(new CompetitionCommand(id, s.TickIndex, 0))); Step(s, 95);
                var p = s.Participant(id); Equal(p.position.x, 12); Equal(p.position.y, 5 - (95 * 1.6 / 60 - 1.75));
                Assert(p.heading == 0 && p.queuedHeading == -1);
            }
        });
        Run("Body contact has no damage, blocking or point penalty", () => {
            var s = Fresh(); var state = s.Snapshot(); state.hearts = Array.Empty<CompetitionHeart>();
            Place(state, "kim", new Point2(6.5, 5), 1, true); Place(state, "trump", new Point2(7.5, 5), 3, true);
            Apply(s, state); Step(s, 60);
            Equal(s.Participant("kim").position.x, 8.1); Equal(s.Participant("trump").position.x, 5.9);
            Assert(s.Phase == CompetitionPhase.Playing && s.Participants.All(p => p.roundScore == 0));
        });
        Run("Tunnel wrapping and corner traces never sweep diagonally or across the whole map", () => {
            var map = new MazeMap(Definition()); var nav = new CorridorNavigation(); var trace = new List<CorridorSegment>();
            nav.Restore(1, 0, true); nav.Advance(map, new Point2(11.99, 5), .03, true, trace);
            Assert(trace.Count == 2); Equal(trace[0].End.x, 12); Equal(trace[0].End.y, 5);
            Equal(trace[1].Start.x, 12); Equal(trace[1].End.x, 12); Equal(trace[1].End.y, 4.98);
            var s = Fresh(); var state = s.Snapshot(); Place(state, "xi", new Point2(27.49, 14), 1, true);
            Apply(s, state); s.Tick(); Assert(s.Participant("xi").position.x < 0);
            s.ApplyCommand(new CompetitionCommand("xi", s.TickIndex, 3)); Step(s, 2);
            Assert(s.Participant("xi").position.x > 27 && s.Participant("xi").heading == 3);
        });
        Run("Each actor can collect an ordinary heart, regardless of human selection", () => {
            foreach (string id in CompetitionSession.ParticipantIds)
            {
                var s = Fresh(human: "trump"); var state = s.Snapshot(); OnlyHeart(state, new Cell(7, 5));
                Place(state, id, new Point2(7, 5)); Apply(s, state); s.Tick();
                Assert(s.Participant(id).roundScore == 1 && s.TotalScore(id) == 1 && s.Hearts.Count == 0);
                Assert(s.Phase == CompetitionPhase.Playing);
            }
        });
        Run("A shared resource goes to the earliest contact, not the lowest ID", () => {
            var s = Fresh(); var state = s.Snapshot(); OnlyHeart(state, new Cell(7, 5));
            Place(state, "zelenskyy", new Point2(6.795, 5), 1, true); Place(state, "kim", new Point2(6.79, 5), 1, true);
            Apply(s, state); s.Tick(); Assert(s.Participant("zelenskyy").roundScore == 1 && s.Participant("kim").roundScore == 0);
        });
        Run("Exact contact ties vary by seed, never by human ID or snapshot array order", () => {
            var wins = CompetitionSession.ParticipantIds.ToDictionary(id => id, id => 0);
            for (uint seed = 1; seed <= 100; seed++)
            {
                string winner = null;
                foreach (string human in new[] { "kim", "trump" })
                {
                    var s = Fresh(seed, human); var state = s.Snapshot(); OnlyHeart(state, new Cell(7, 5));
                    foreach (string id in CompetitionSession.ParticipantIds) Place(state, id, new Point2(7, 5));
                    if (human == "trump") Array.Reverse(state.participants);
                    Apply(s, state); s.Tick(); string actual = s.Participants.Single(p => p.roundScore == 1).id;
                    Assert(winner == null || winner == actual); winner = actual;
                    Assert(s.Participants.Sum(p => p.roundScore) == 1);
                }
                wins[winner]++;
            }
            Assert(wins.All(p => p.Value > 0), string.Join(", ", wins));
        });
        Run("Contacts before the 120-second deadline score; exact-deadline heart/crate contacts do not", () => {
            foreach (bool before in new[] { false, true })
            {
                var s = Fresh(); var state = s.Snapshot(); state.tick = state.roundElapsedTicks = 7199;
                state.nextHeartWaveTick = state.tick + 600; state.nextWallAttemptTick = state.tick + 900;
                OnlyHeart(state, new Cell(6, 5)); double travel = 1.6 / 60;
                Place(state, "putin", new Point2(6 - CompetitionSession.PickupRadius - travel * (before ? .5 : 1), 5), 1, true);
                Apply(s, state); s.Tick();
                Assert(s.Participant("putin").roundScore == (before ? 1 : 0));
                Assert((s.Participant("putin").inventory != CompetitionItem.None) == before);
                Assert(s.Phase == CompetitionPhase.RoundResults && s.TickIndex == 7200 && s.RoundSecondsRemaining == 0);
                long total = s.TotalScore("putin"); Step(s, 60); Assert(s.TotalScore("putin") == total && s.TickIndex == 7200);
            }
        });
        Run("Legacy explicit profile: an empty board keeps playing, silently replenishes small groups, and ends only at 120 seconds", () => {
            var s = Fresh(); var state = s.Snapshot(); state.hearts = Array.Empty<CompetitionHeart>(); Apply(s, state);
            Step(s, 599); Assert(s.Hearts.Count == 0 && s.Phase == CompetitionPhase.Playing);
            s.Tick(); Assert(s.Hearts.Count == 6 && s.RoundSecondsRemaining == 110);
            var newHearts = s.Hearts.ToArray();
            foreach (var h in newHearts) Assert(s.Maze.PlayerCells.Contains(h.cell) && !s.Maze.IsHome(h.cell));
            foreach (var group in newHearts.Select((h, i) => (h, i)).GroupBy(p => p.i / 3))
            {
                var cells = group.Select(p => p.h.cell).ToArray();
                Assert(cells.All(c => cells.Any(other => c != other && (c.Position - other.Position).Length == 1)));
            }
            Step(s, 6600); Assert(s.Phase == CompetitionPhase.RoundResults && s.TickIndex == 7200);
        });
        Run("Legacy explicit profile: replenishment respects capacity, occupied cells, crates and moving-wall approaches", () => {
            var s = Fresh(); Step(s, 600); Assert(s.Hearts.Count == 240);
            var state = s.Snapshot(); state.hearts = state.hearts.Take(20).ToArray();
            Place(state, "kim", new Point2(10, 5)); Apply(s, state); Step(s, 600);
            Assert(s.Hearts.Count <= 26 && s.Hearts.Select(h => h.cell).Distinct().Count() == s.Hearts.Count);
            foreach (var heart in s.Hearts.Where(h => h.id >= 241))
            {
                Assert(s.Participants.All(p => (s.Maze.Near(heart.cell.Position, p.position) - p.position).Length >= .6));
                Assert(s.Crates.All(c => c.cell != heart.cell));
                Assert(s.Maze.Definition.shortcutCandidates.All(c => c.endpoints.All(pair => MazeMap.FromPair(pair) != heart.cell)));
            }
        });
        Run("Every actor replaces a full inventory; a regenerated crate needs exit and re-entry", () => {
            foreach (string id in CompetitionSession.ParticipantIds)
            {
                var s = Fresh(); var state = s.Snapshot(); state.hearts = Array.Empty<CompetitionHeart>();
                Place(state, id, new Point2(5.79, 5), 1, true);
                var random = new RandomState(state.itemRandom); CompetitionItem expected = (CompetitionItem)(1 + random.Range(5));
                Array.Find(state.participants, p => p.id == id).inventory = expected == CompetitionItem.Banana ? CompetitionItem.Oil : CompetitionItem.Banana;
                Apply(s, state); s.Tick(); Assert(s.Participant(id).inventory == expected && s.Crates[0].availableAtTick > s.TickIndex);
                state = s.Snapshot(); Array.Find(state.participants, p => p.id == id).moving = false; Apply(s, state);
                long readyAt = s.Crates[0].availableAtTick; Step(s, 601);
                Assert(s.Crates[0].availableAtTick == readyAt && s.Participant(id).inventory == expected);
                s.ApplyCommand(new CompetitionCommand(id, s.TickIndex, 3)); Step(s, 20);
                s.ApplyCommand(new CompetitionCommand(id, s.TickIndex, 1)); Step(s, 20);
                Assert(s.Crates[0].availableAtTick > s.TickIndex, "Re-entry did not consume the replenished crate.");
            }
        });
        Run("Item pool includes all five distinct types; legacy item values remain unchanged", () => {
            var found = new HashSet<CompetitionItem>();
            for (uint seed = 1; seed <= 40; seed++)
            {
                var s = Fresh(seed); var state = s.Snapshot(); Place(state, "xi", new Point2(5.79, 5), 1, true);
                Apply(s, state); s.Tick(); found.Add(s.Participant("xi").inventory);
            }
            Assert(found.Count == 5 && !found.Contains(CompetitionItem.None));
            Assert((int)ItemKind.BananaPeel == 1 && (int)ItemKind.OilSlick == 2 && (int)ItemKind.SpeedBoost == 3);
        });
        Run("Pause and resume countdown freeze round, resource, navigation and random clocks", () => {
            var s = Fresh(); Step(s, 100); s.Pause(); string paused = Text(s); Step(s, 1000); Assert(Text(s) == paused);
            s.Resume(); Assert(s.Phase == CompetitionPhase.Countdown); long tick = s.TickIndex;
            Step(s, 179); Assert(s.Phase == CompetitionPhase.Countdown && s.TickIndex == tick);
            s.Pause(); var state = s.Snapshot(); var restored = Fresh(); restored.Restore(state); restored.Resume();
            restored.Tick(); Assert(restored.Phase == CompetitionPhase.Playing && restored.TickIndex == tick);
            restored.Tick(); Assert(restored.TickIndex == tick + 1);
        });
        Run("Shared round trophies and banked points are settled exactly once across restore", () => {
            var s = Fresh(); EndRound(s, 10, 10, 2, 1, 0);
            Assert(s.RoundWinners.SequenceEqual(new[] { "kim", "putin" }) && s.Participant("kim").trophies == 1);
            Assert(s.TotalScore("kim") == 10 && s.Participant("kim").bankedScore == 10);
            var restored = Fresh(); restored.Restore(s.Snapshot()); restored.Resume(false); Step(restored, 20);
            Assert(Text(restored) == Text(s)); Assert(restored.StartNextRound(false));
            Assert(restored.RoundNumber == 2 && restored.Participant("kim").roundScore == 0 && restored.TotalScore("kim") == 10);
            Assert(restored.Hearts.Count == 240 && restored.Participants.All(p => p.inventory == CompetitionItem.None));
        });
        Run("Simultaneous third trophies use total points; later results never award again", () => {
            var s = Fresh(); EndRound(s, 10, 5, 0, 0, 0); s.StartNextRound(false);
            EndRound(s, 0, 10, 0, 0, 0); s.StartNextRound(false);
            EndRound(s, 10, 10, 0, 0, 0); s.StartNextRound(false); EndRound(s, 10, 10, 0, 0, 0);
            Assert(s.Phase == CompetitionPhase.MatchResults && s.MatchWinners.SequenceEqual(new[] { "putin" }));
            Assert(s.Participant("kim").trophies == 3 && s.TotalScore("putin") == 35 && s.TotalScore("kim") == 30);
            string final = Text(s); Step(s, 100); Assert(Text(s) == final && !s.StartNextRound(false));
        });
        Run("Fewer trophies cannot win via points; full ties share the match", () => {
            var s = Fresh(); EndRound(s, 0, 0, 0, 200, 0); s.StartNextRound(false);
            EndRound(s, 0, 0, 0, 200, 0); s.StartNextRound(false);
            for (int i = 0; i < 3; i++) { EndRound(s, 10, 10, 0, 0, 0); if (i < 2) s.StartNextRound(false); }
            Assert(s.MatchWinners.SequenceEqual(new[] { "kim", "putin" }) && s.TotalScore("xi") == 400 && s.Participant("xi").trophies == 2);
            var zero = Fresh(); for (int i = 0; i < 3; i++) { EndRound(zero, 0, 0, 0, 0, 0); if (i < 2) zero.StartNextRound(false); }
            Assert(zero.MatchWinners.Count == 5 && zero.Participants.All(p => p.trophies == 3));
        });
        Run("Any of the five IDs can win a whole match", () => {
            foreach (string id in CompetitionSession.ParticipantIds)
            {
                var s = Fresh(human: id);
                for (int round = 0; round < 3; round++)
                {
                    long[] scores = CompetitionSession.ParticipantIds.Select(p => p == id ? 1L : 0L).ToArray();
                    EndRound(s, scores); if (round < 2) Assert(s.StartNextRound(false));
                }
                Assert(s.MatchWinners.SequenceEqual(new[] { id }));
            }
        });
        Run("Automatic shortcut cycle retains every phase and reserves every participant's current edge", () => {
            var cycle = new CompetitionSession(Definition(), countdown: false); var seen = new HashSet<WallPhase>();
            var state = cycle.Snapshot(); foreach (var p in state.participants) p.moving = false; Apply(cycle, state);
            for (int i = 0; i < 1800; i++) { cycle.Tick(); foreach (var wall in cycle.Walls) seen.Add(wall.phase); }
            Assert(seen.Count == 5);
            foreach (string id in CompetitionSession.ParticipantIds)
            {
                var s = new CompetitionSession(Definition(), countdown: false); state = s.Snapshot();
                foreach (var p in state.participants) p.moving = false;
                state.walls[0].phase = WallPhase.Open; state.walls[0].untilTick = 0;
                Place(state, id, new Point2(14.5, 1), 1); Apply(s, state); Step(s, 10);
                Assert(s.PassageReserved(0) && s.Walls[0].phase == WallPhase.Open);
                s.ApplyCommand(new CompetitionCommand(id, s.TickIndex, 1)); Step(s, 22);
                Assert(s.Walls[0].phase == WallPhase.Closing && !s.Maze.OpenShortcuts[0]);
            }
        });
        Run("Snapshots and public participant/resource copies never share mutable state", () => {
            var s = Fresh(); string before = Text(s); var state = s.Snapshot();
            state.participants[0].position.x += 1; state.hearts[0].cell.x += 1; state.rules.speed = 3;
            s.Participants[0].trophies = 3; s.Hearts[0].cell.x += 2; s.Rules.speed = 4;
            Assert(Text(s) == before);
            state = s.Snapshot(); var copy = Fresh(); Apply(copy, state); state.participants[0].trophies = 3;
            Assert(Text(copy) == before);
        });
        Run("Invalid snapshots reject atomically, including corrupt identity, geometry, resources and ledger", () => {
            var s = Fresh(); string before = Text(s);
            Action<CompetitionSnapshot>[] mutations = {
                x => x.format = null, x => x.version = 99, x => x.mazeFingerprint = "wrong", x => x.humanId = "missing",
                x => x.participants[1].id = x.participants[0].id, x => x.participants[0].position.x = double.NaN,
                x => x.participants[0].heading = 9, x => x.participants[0].queuedHeading = 4,
                x => x.participants[0].roundScore = -1, x => x.participants[0].trophies = 3,
                x => x.participants[0].bankedScore = 1, x => x.participants[0].trophies = 1,
                x => x.participants[0].inventory = (CompetitionItem)99, x => x.participants[0].insideCrates[0] = true,
                x => x.hearts[1].cell = x.hearts[0].cell, x => x.hearts[0].cell = new Cell(13, 14),
                x => x.crates[0].availableAtTick = -1, x => x.crates[0].cell = new Cell(1, 1),
                x => x.rules.roundDurationTicks = 60, x => x.rules.speed = double.PositiveInfinity,
                x => x.resourceRandom = 0, x => x.settledRound = 1, x => x.roundWinners = new[] { "kim" },
                x => x.walls[0].phase = WallPhase.Open, x => x.phase = CompetitionPhase.MatchResults,
                x => x.roundStartedTick = -1, x => x.roundStartedTick = 1, x => x.roundEnd = CompetitionRoundEnd.HeartsCollected, x => x.roundElapsedTicks = 7200, x => x.tick = 100, x => x.nextHeartId = int.MaxValue
            };
            foreach (var mutate in mutations) { var state = s.Snapshot(); mutate(state); Reject(() => s.Restore(state)); Assert(Text(s) == before); }
        });
        Run("Versioned JSON restore resumes the same seeded five-actor simulation and resources", () => {
            var original = new CompetitionSession(Definition(), "trump", 123, countdown: false);
            var random = new RandomState(777);
            for (int i = 0; i < 1300; i++)
            {
                if (i % 12 == 0) foreach (string id in CompetitionSession.ParticipantIds) original.ApplyCommand(new CompetitionCommand(id, original.TickIndex, random.Range(4)));
                original.Tick();
            }
            string serialized = Text(original);
            Assert(!serialized.Contains("Normalized") && !serialized.Contains("IsFinite"));
            var restored = Fresh(); restored.Restore(JsonSerializer.Deserialize<CompetitionSnapshot>(serialized, Json));
            Assert(restored.Phase == CompetitionPhase.Paused); restored.Resume(false); Assert(Text(restored) == serialized);
            for (int i = 0; i < 1800; i++)
            {
                if (i % 12 == 0) foreach (string id in CompetitionSession.ParticipantIds)
                { var command = new CompetitionCommand(id, original.TickIndex, random.Range(4)); original.ApplyCommand(command); restored.ApplyCommand(command); }
                original.Tick(); restored.Tick();
            }
            Assert(Text(original) == Text(restored));
        });
        var matchReports = new List<MatchReport>();
        Run("Five full scripted matches use real ticks, changing resources, wall cycles and each human ID", () => {
            foreach (string human in CompetitionSession.ParticipantIds)
            {
                uint seed = (uint)(100 + matchReports.Count);
                var s = new CompetitionSession(Definition(), human, seed, countdown: false);
                var commands = new RandomState(seed); var goals = new Cell[5];
                int rounds = 0, ticks = 0; long lastTotal = 0;
                while (s.Phase != CompetitionPhase.MatchResults && ticks < 11 * 7200)
                {
                    if (s.TickIndex % 60 == 0)
                    {
                        var resources = s.Hearts;
                        for (int i = 0; i < 5; i++) goals[i] = resources.Count > 0 ? resources[commands.Range(resources.Count)].cell : s.Maze.PlayerCells[commands.Range(s.Maze.PlayerCells.Count)];
                    }
                    if (s.TickIndex % 15 == 0)
                    {
                        var actors = s.Participants;
                        for (int i = 0; i < 5; i++)
                        {
                            Cell current = s.Maze.Canonical(new Cell((int)Math.Round(actors[i].position.x), (int)Math.Round(actors[i].position.y)));
                            Cell next = s.Maze.PathStep(current, goals[i], true);
                            Point2 direction = s.Maze.Near(next.Position, current.Position) - current.Position;
                            if (direction.Length < .1) continue;
                            int heading = Math.Abs(direction.x) > Math.Abs(direction.y) ? (direction.x > 0 ? 1 : 3) : (direction.y > 0 ? 2 : 0);
                            s.ApplyCommand(new CompetitionCommand(actors[i].id, s.TickIndex, heading));
                        }
                    }
                    s.Tick(); ticks++;
                    if (s.Phase == CompetitionPhase.RoundResults || s.Phase == CompetitionPhase.MatchResults)
                    {
                        rounds++; long total = s.Participants.Sum(p => p.bankedScore); Assert(total >= lastTotal); lastTotal = total;
                        var saved = s.Snapshot(); s.Restore(saved); s.Resume(false);
                        Assert(s.Participants.Sum(p => p.bankedScore) == total && s.RoundWinners.Count > 0);
                        if (s.Phase == CompetitionPhase.RoundResults) Assert(s.StartNextRound(false));
                    }
                }
                Assert(s.Phase == CompetitionPhase.MatchResults && s.MatchWinners.Count > 0 && rounds >= 3 && rounds <= 11);
                Assert(ticks == rounds * 7200 && s.Participants.All(p => p.bankedScore > 0));
                matchReports.Add(new MatchReport { humanId = human, seed = seed, rounds = rounds, activeTicks = ticks,
                    simulatedSeconds = ticks / 60, totalHeartsCollected = lastTotal, winners = s.MatchWinners.ToArray(), participants = s.Participants.ToArray() });
                Console.WriteLine($"MATCH {human}: {rounds} rounds, {ticks / 60} simulated seconds, {lastTotal} collected hearts, winner {string.Join(", ", s.MatchWinners)}");
            }
        });
        if (args.Length == 2 && args[0] == "--report")
        {
            string directory = Path.GetDirectoryName(Path.GetFullPath(args[1])); Directory.CreateDirectory(directory);
            File.WriteAllText(args[1], JsonSerializer.Serialize(matchReports, new JsonSerializerOptions(Json) { WriteIndented = true }));
        }
        Console.WriteLine($"\n{passed} competition checks passed, {failed} failed");
        return failed == 0 ? 0 : 1;
    }
    private sealed class MatchReport
    {
        public string humanId;
        public uint seed;
        public int rounds, activeTicks, simulatedSeconds;
        public long totalHeartsCollected;
        public string[] winners;
        public CompetitionParticipant[] participants;
    }
}
