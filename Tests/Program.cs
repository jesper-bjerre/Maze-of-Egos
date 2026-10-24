using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MazeOfEgos.Core;

internal static class Program
{
    private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true, IgnoreReadOnlyProperties = true };
    private static readonly string MazeJson = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "classic_reference.json"));
    private static int passed, failed;
    private static MazeDefinition Definition() => JsonSerializer.Deserialize<MazeDefinition>(MazeJson, Json);
    private static GameSession Fresh(bool additions = false, uint seed = 42)
    {
        var session = new GameSession(new MazeMap(Definition()), seed,
            new RuleProfile { itemsEnabled = additions, movingWallsEnabled = additions, levelClearBonusEnabled = true }, false);
        Hold(session); return session;
    }
    // Stationary rule fixtures isolate scoring, timers and contact from the automatic motor.
    private static void Hold(GameSession session)
    { var state = session.Snapshot(); state.autoMoving = false; state.queuedHeading = -1; Apply(session, state); }
    private static void Assert(bool condition, string message = "Assertion failed") { if (!condition) throw new Exception(message); }
    private static void Equal(double a, double b, double tolerance = 1e-7) => Assert(Math.Abs(a - b) <= tolerance, $"Expected {b}, got {a}");
    private static void Reject(Action action) { try { action(); } catch (ArgumentException) { return; } throw new Exception("Invalid data was accepted"); }
    private static void Run(string name, Action action)
    {
        try { action(); passed++; Console.WriteLine("PASS " + name); }
        catch (Exception e) { failed++; Console.WriteLine("FAIL " + name + ": " + e.Message); }
    }
    private static void Apply(GameSession session, SessionSnapshot snapshot)
    { session.Restore(snapshot); session.Resume(0); }
    private static void Place(GameSession session, Point2 position, double yaw = 0)
    {
        SessionSnapshot state = session.Snapshot(); state.player = position; state.yaw = yaw; state.trail = new[] { position };
        state.autoMoving = false; state.queuedHeading = -1;
        foreach (PursuerState p in state.pursuers) { p.life = PursuerLife.InHouse; p.releaseAt = 100000; }
        Apply(session, state);
    }
    private static void Advance(GameSession session, int ticks)
    { for (int i = 0; i < ticks; i++) session.Tick(); }
    private static void OnlyHeart(SessionSnapshot s, int index)
    { Array.Fill(s.collected, true); s.collected[index] = false; s.remainingHearts = 1; }
    private static void ActiveAt(SessionSnapshot s, int index, Cell cell)
    {
        PursuerState p = s.pursuers[index]; p.life = PursuerLife.Active; p.from = p.to = p.previous = cell;
        p.progress = 0; p.consumedPowerId = s.powerId;
    }
    private static int Main()
    {
        Run("Screen input follows projected corridors at different camera angles and scales", () => {
            foreach (double pitch in new[] { 35.0, 55.0, 70.0 })
            for (int yaw = -90; yaw <= 360; yaw += 25)
            {
                double a = yaw * Math.PI / 180, p = pitch * Math.PI / 180;
                Point2 n = new Point2(-Math.Sin(a), Math.Sin(p) * Math.Cos(a));
                Point2 e = new Point2(Math.Cos(a), Math.Sin(p) * Math.Sin(a));
                Assert(ScreenHeadingInput.HeadingFor(n, n, e) == 0);
                Assert(ScreenHeadingInput.HeadingFor(e, n, e) == 1);
                Assert(ScreenHeadingInput.HeadingFor(n * -1, n, e) == 2);
                Assert(ScreenHeadingInput.HeadingFor(e * -1, n, e) == 3);
                Assert(ScreenHeadingInput.HeadingFor(e, n * 3, e * .7) == 1);
            }
            Reject(() => ScreenHeadingInput.HeadingFor(new Point2(1, 0), new Point2(), new Point2(1, 0)));
        });
        Run("Screen stick latches a world heading until release or a different direction", () => {
            var latch = new ScreenHeadingInput(); Point2 n = new Point2(-.4, .8), e = new Point2(.8, .4);
            Assert(latch.Read(n, n, e) == 0 && latch.Read(n, n, e) == null);
            Assert(latch.Read(e, n, e) == 1 && latch.Read(e, n, e) == null);
            Assert(latch.Read(new Point2(), n, e) == null && latch.Read(e, n, e) == 1);
            latch.Clear(); Assert(latch.Read(e, n, e) == 1);
        });
        Run("A screen-selected world turn remains queued through a change of heading", () => {
            var s = Fresh(); Place(s, new Point2(10.25, 5), 90);
            int north = ScreenHeadingInput.HeadingFor(new Point2(-.4, .8), new Point2(-.4, .8), new Point2(.8, .4));
            s.RequestHeading(north); Advance(s, 95);
            Equal(s.Player.x, 12); Assert(s.Player.y < 5 && s.Yaw == 0 && s.QueuedHeading == -1);
        });
        Run("Item-use feedback identifies banana, oil and boost even during an existing boost", () => {
            var session = Fresh(true); Place(session, new Point2(6, 5), 90);
            foreach (ItemKind kind in new[] { ItemKind.SpeedBoost, ItemKind.BananaPeel, ItemKind.OilSlick })
            {
                var state = session.Snapshot(); state.inventory = kind; state.trail = new[] { new Point2(5.5, 5), new Point2(6, 5) }; Apply(session, state);
                Assert(session.UseItem(), "Fixture could not use " + kind);
                Assert(session.Events.Any(e => e.Kind == EventKind.ItemUsed && e.Actor == (int)kind), "Feedback misidentified " + kind);
            }
        });
        Run("Gait follows world distance across speed changes and freezes with simulation time", () => {
            var gait = new GaitClock(); gait.Update(0, 3.75);
            for (int i = 1; i <= 60; i++) gait.Update(i / 60.0, 3.75);
            Equal(gait.DistanceMeters, 3.75); gait.Update(1.5, 1); Equal(gait.DistanceMeters, 4.25);
            double phase = gait.Cycles;
            for (int i = 0; i < 120; i++) gait.Update(1.5, 3.75);
            Equal(gait.Cycles, phase); gait.Update(2, 0); Equal(gait.Cycles, phase);
            gait.Update(2.5, 1); Equal(gait.DistanceMeters, 4.75);
        });
        Run("Gait is frame-partition independent and rebases restored or reset timelines", () => {
            foreach (int fps in new[] { 30, 60, 120 })
            {
                var gait = new GaitClock(); gait.Update(0, 3.75);
                for (int i = 1; i <= 3 * fps; i++) gait.Update(i / (double)fps, i <= fps ? 3.75 : 1.5);
                Equal(gait.DistanceMeters, 6.75); Equal(gait.Cycles, 6.75 / GaitClock.StrideMeters);
                gait.Update(.5, 1); Equal(gait.Cycles, 0);
                gait.Update(1, 1); Equal(gait.DistanceMeters, .5);
                gait.Reset(); gait.Update(100, 3.75); Equal(gait.Cycles, 0);
                Reject(() => gait.Update(double.NaN, 1)); Reject(() => gait.Update(101, -1));
            }
        });
        Run("Frame statistics measure time-weighted FPS and preserve long stalls", () => {
            var stats = new FrameStatistics(); for (int i = 0; i < 60; i++) stats.Add(1000.0 / 60);
            Equal(stats.FramesPerSecond, 60); Equal(stats.AverageMs, 1000.0 / 60);
            stats.Add(1000); Assert(stats.Count == 61 && stats.Over33Ms == 1 && stats.Over50Ms == 1);
            Equal(stats.FramesPerSecond, 30.5); Equal(stats.WorstMs, 1000); Equal(stats.Percentile(1), 1000);
        });
        Run("Frame percentiles bound a known distribution to 0.25ms", () => {
            var stats = new FrameStatistics(); for (int i = 0; i < 95; i++) stats.Add(16.67);
            for (int i = 0; i < 4; i++) stats.Add(33.34); stats.Add(100);
            Equal(stats.Percentile(0.5), 16.75); Equal(stats.Percentile(0.95), 16.75);
            Equal(stats.Percentile(0.99), 33.5); Assert(stats.Over33Ms == 5 && stats.Over50Ms == 1);
        });
        Run("Frame collector rejects invalid times and clears all accumulated state", () => {
            var stats = new FrameStatistics();
            foreach (double value in new[] { 0, -1, double.NaN, double.PositiveInfinity })
                try { stats.Add(value); throw new Exception("Invalid frame accepted"); } catch (ArgumentOutOfRangeException) { }
            stats.Add(70); stats.Clear(); Assert(stats.Count == 0 && stats.Over50Ms == 0 && stats.WorstMs == 0 && stats.Percentile(0.95) == 0);
            stats.Add(5); Equal(stats.Percentile(0.95), 5); Equal(stats.FramesPerSecond, 200);
        });
        Run("A01 reference dimensions, exact counts, and preserved spaces", () => {
            var maze = new MazeMap(Definition()); Assert(maze.Definition.width == 28 && maze.Definition.height == 31);
            Assert(maze.Hearts.Count == 244 && maze.PowerHearts.Count(p => p) == 4);
            Assert(maze.Definition.rows.All(r => r.Length == 28));
            Assert(maze.Definition.rows[10].StartsWith("     "));
        });
        Run("A02 all hearts reachable; base and exterior void excluded", () => {
            var maze = new MazeMap(Definition()); Assert(maze.Hearts.All(c => maze.IsWalkable(c)));
            Assert(!maze.IsWalkable(new Cell(13, 14))); Assert(maze.IsWalkable(new Cell(13, 14), true));
            Assert(!maze.IsWalkable(new Cell(0, 10))); Assert(!maze.IsWalkable(new Cell(13, 12)));
        });
        Run("Malformed map is rejected", () => {
            var d = Definition(); d.rows[0] = "#"; Reject(() => new MazeMap(d));
            d = Definition(); d.expectedSmallDots++; Reject(() => new MazeMap(d));
        });
        Run("A03 tunnel wraps automatic centre-line movement in both directions", () => {
            var s = Fresh(); Place(s, new Point2(-0.48, 14), 270); s.RequestTurn(RelativeTurn.Forward); s.Tick();
            Assert(s.Player.x > 27.45 && s.Player.x < 27.5); Equal(s.Player.y, 14); Equal(s.Yaw, 270);
            s.RequestTurn(RelativeTurn.Reverse); s.Tick(); Assert(s.Player.x < -0.45); Equal(s.Yaw, 90);
        });
        Run("A04 a new game walks automatically and a wall stops it at the cell centre", () => {
            var automatic = new GameSession(new MazeMap(Definition()), countdown: false);
            Point2 before = automatic.Player; automatic.Tick(); Assert(automatic.Player.x < before.x && automatic.Pitch == 0);
            var s = Fresh(); Place(s, new Point2(25, 5), 90); s.RequestTurn(RelativeTurn.Forward); Advance(s, 120);
            Equal(s.Player.x, 26); Equal(s.Player.y, 5); Assert(s.Maze.CanOccupy(s.Player));
            Point2 stopped = s.Player; Advance(s, 60); Equal((s.Player - stopped).Length, 0);
            s.RequestTurn(RelativeTurn.Reverse); s.Tick(); Assert(s.Player.x < 26);
        });
        Run("A04 stick deflection issues one turn until released or another direction is selected", () => {
            var latch = new JoystickTurnLatch();
            Assert(latch.Read(new Point2(-.9, .1)) == RelativeTurn.Left);
            for (int i = 0; i < 120; i++) Assert(latch.Read(new Point2(-1, 0)) == null);
            Assert(latch.Read(new Point2(0, 1)) == RelativeTurn.Forward);
            Assert(latch.Read(new Point2(0, .25)) == null);
            Assert(latch.Read(new Point2()) == null);
            Assert(latch.Read(new Point2(-.4, 0)) == RelativeTurn.Left);
            Assert(latch.Read(new Point2(double.NaN, 0)) == null);
        });
        Run("A04 releasing the stick preserves full-speed automatic movement", () => {
            var s = Fresh(); Place(s, new Point2(12, 5), 90); var latch = new JoystickTurnLatch();
            s.RequestTurn(latch.Read(new Point2(0, .4)).Value); s.Tick();
            Equal(s.Player.x, 12 + LevelBalance.PlayerSpeed(1) * GameSession.FixedDelta);
            Assert(latch.Read(new Point2()) == null); Advance(s, 60);
            Equal(s.Player.x, 12 + LevelBalance.PlayerSpeed(1) * GameSession.FixedDelta * 61); Equal(s.Player.y, 5);
        });
        Run("A04 queued left waits for the next legal junction and consumes the remaining movement budget", () => {
            var map = new MazeMap(Definition()); var motor = new CorridorNavigation(); motor.Restore(1, -1, true);
            motor.Request(RelativeTurn.Left);
            Point2 position = motor.Advance(map, new Point2(10.25, 5), 2);
            Equal(position.x, 12); Equal(position.y, 4.75); Assert(motor.Heading == 0 && motor.QueuedHeading == -1);
            position = motor.Advance(map, position, .5); Equal(position.x, 12); Equal(position.y, 4.25);
        });
        Run("A04 newest command replaces a queued turn and forward cancels it", () => {
            var map = new MazeMap(Definition()); var motor = new CorridorNavigation(); motor.Restore(1, -1, true);
            motor.Request(RelativeTurn.Left); motor.Request(RelativeTurn.Right); Assert(motor.QueuedHeading == 2);
            motor.Request(RelativeTurn.Forward); Assert(motor.QueuedHeading == 1);
            Point2 position = motor.Advance(map, new Point2(7.25, 5), 2); Equal(position.x, 9.25); Equal(position.y, 5);
            motor.Request(RelativeTurn.Reverse); position = motor.Advance(map, position, .1);
            Equal(position.x, 9.15); Equal(position.y, 5); Assert(motor.Heading == 3 && motor.QueuedHeading == -1);
        });
        Run("A04 an automatic player waiting at a closed shortcut continues when it opens", () => {
            var map = new MazeMap(Definition()); var motor = new CorridorNavigation(); motor.Restore(1, -1, true);
            Point2 position = motor.Advance(map, new Point2(12, 1), .1); Equal(position.x, 12);
            map.SetShortcut(0, true); position = motor.Advance(map, position, .1); Equal(position.x, 12.1);
        });
        Run("A14 shortcut retains the full navigation edge until the player reaches the exit centre", () => {
            var s = Fresh(true); var state = s.Snapshot();
            state.player = new Point2(14.83, 20); state.yaw = 90; state.autoMoving = true;
            state.walls[2].phase = WallPhase.Open; state.walls[2].phaseUntil = 0;
            foreach (var p in state.pursuers) { p.life = PursuerLife.InHouse; p.releaseAt = 10000; }
            Apply(s, state); s.Tick(); Assert(s.Walls[2].phase == WallPhase.Open && s.PassageReserved(2));
            Advance(s, 30); Assert(s.Player.x > 15 && s.Walls[2].phase == WallPhase.Closing);
            Assert(CorridorNavigation.HasLegalPosition(s.Maze, s.Player, 1));
        });
        Run("A17 pause and JSON save preserve the queued turn and automatic motion", () => {
            var s = Fresh(); Place(s, new Point2(10.25, 5), 90); s.RequestTurn(RelativeTurn.Left); s.Pause();
            var state = JsonSerializer.Deserialize<SessionSnapshot>(JsonSerializer.Serialize(s.Snapshot(), Json), Json);
            Advance(s, 120); Equal(s.Player.x, 10.25); Equal(s.Time, state.time);
            var copy = Fresh(); copy.Restore(state); copy.Resume(.5); Advance(copy, 29); Equal(copy.Player.x, 10.25);
            Advance(copy, 121); Assert(copy.Yaw == 0 && copy.Player.x == 12 && copy.Player.y < 5 && copy.Pitch == 0);
        });
        Run("A17 legacy free movement saves migrate to a legal centre line without losing progress", () => {
            var s = Fresh(); var state = s.Snapshot(); state.version = 2; state.player = new Point2(7.25, 5.12); state.yaw = 76; state.pitch = 15;
            state.score = 1230; state.time = 25; state.powerUntil = 29; state.boostUntil = 27;
            Apply(s, state); Assert(s.Snapshot().version == 3 && s.Yaw == 90 && s.Pitch == 0 && s.AutoMoving);
            Equal(s.Player.x, 7.25); Equal(s.Player.y, 5); Equal(s.Score, 1230); Equal(s.Time, 25);
            Equal(s.PowerUntil, 29); Equal(s.BoostUntil, 27); Assert(s.RemainingHearts == state.remainingHearts);
            string before = JsonSerializer.Serialize(s.Snapshot(), Json);
            state = s.Snapshot(); state.player.y = 5.1; Reject(() => s.Restore(state));
            state = s.Snapshot(); state.queuedHeading = 4; Reject(() => s.Restore(state));
            state = s.Snapshot(); state.pitch = 1; Reject(() => s.Restore(state));
            Assert(before == JsonSerializer.Serialize(s.Snapshot(), Json));
        });
        Run("Pursuer body blocks a corridor even at the player's maximum wall offset", () => {
            var s = Fresh(); Place(s, new Point2(5.76, 5));
            SessionSnapshot state = s.Snapshot(); ActiveAt(state, 0, new Cell(6, 5)); Apply(s, state); s.Tick();
            Assert(s.Phase == SessionPhase.Caught);
        });
        Run("A05 power heart wins a simultaneous encounter and gives chain points", () => {
            var s = Fresh(); int index = s.Maze.PowerHearts.FindIndex(p => p); Cell cell = s.Maze.Hearts[index];
            Place(s, cell.Position); var state = s.Snapshot(); ActiveAt(state, 0, cell); Apply(s, state); s.Tick();
            Assert(s.Chances == 3 && s.Score == 250); Assert(s.Pursuers[0].life == PursuerLife.Returning);
            Assert(s.PowerUntil > s.Time);
        });
        Run("A06 returning pursuer cannot award points twice", () => {
            var s = Fresh(); int index = s.Maze.PowerHearts.FindIndex(p => p); Cell cell = s.Maze.Hearts[index];
            Place(s, cell.Position); var state = s.Snapshot(); ActiveAt(state, 0, cell); Apply(s, state); s.Tick();
            long points = s.Score; Advance(s, 10); Assert(s.Score == points);
        });
        Run("Power chain is 200/400/800/1600 and second power resets it", () => {
            var s = Fresh(); int index = s.Maze.PowerHearts.FindIndex(p => p); Cell cell = s.Maze.Hearts[index];
            Place(s, cell.Position); var state = s.Snapshot(); for (int i = 0; i < 4; i++) ActiveAt(state, i, cell);
            Apply(s, state); s.Tick(); Assert(s.Score == 50 + 200 + 400 + 800 + 1600);
            int next = s.Maze.PowerHearts.FindIndex(index + 1, p => p); Cell second = s.Maze.Hearts[next];
            state = s.Snapshot(); state.player = second.Position; state.trail = new[] { second.Position };
            ActiveAt(state, 0, second); Apply(s, state); s.Tick(); Assert(s.Score == 3300);
        });
        Run("Power expiry at contact is dangerous", () => {
            var s = Fresh(); Cell cell = new Cell(6, 5); Place(s, cell.Position);
            var state = s.Snapshot(); state.powerId = 1; state.powerUntil = state.time;
            ActiveAt(state, 0, cell); state.pursuers[0].consumedPowerId = 0; Apply(s, state); s.Tick();
            Assert(s.Phase == SessionPhase.Caught && s.Chances == 2);
        });
        Run("Power still applies at a contact before expiry within the same tick", () => {
            var s = Fresh(); Cell cell = new Cell(6, 5); Place(s, cell.Position);
            var state = s.Snapshot(); state.powerId = 1; state.powerUntil = state.time + 0.001;
            ActiveAt(state, 0, cell); state.pursuers[0].consumedPowerId = 0; Apply(s, state); s.Tick();
            Assert(s.Phase == SessionPhase.Playing && s.Pursuers[0].life == PursuerLife.Returning && s.Score == 210);
        });
        Run("Simultaneous dangerous and vulnerable contacts both resolve before capture", () => {
            for (int dangerous = 0; dangerous < 2; dangerous++)
            {
                var s = Fresh(); Cell cell = new Cell(6, 5); Place(s, cell.Position);
                var state = s.Snapshot(); state.powerId = 1; state.powerUntil = 100;
                ActiveAt(state, 0, cell); ActiveAt(state, 1, cell);
                state.pursuers[1 - dangerous].consumedPowerId = 0; Apply(s, state); s.Tick();
                Assert(s.Phase == SessionPhase.Caught && s.Chances == 2 && s.Score == 210);
                Assert(s.Pursuers[1 - dangerous].life == PursuerLife.Returning);
            }
        });
        Run("A07 a capture loses exactly one chance and preserves hearts and score", () => {
            var s = Fresh(); Cell cell = new Cell(6, 5); Place(s, cell.Position);
            var state = s.Snapshot(); ActiveAt(state, 0, cell); Apply(s, state); s.Tick(); long score = s.Score;
            int remaining = s.RemainingHearts; Advance(s, 100);
            Assert(s.Chances == 2 && s.Score == score && s.RemainingHearts == remaining);
        });
        Run("A08 last heart clears the level once and gives exactly one bonus", () => {
            var s = Fresh(); int index = 0; Place(s, s.Maze.Hearts[index].Position);
            var state = s.Snapshot(); OnlyHeart(state, index); Apply(s, state); s.Tick();
            Assert(s.Phase == SessionPhase.LevelClear && s.RemainingHearts == 0 && s.Score == 1010);
            Advance(s, 121); Assert(s.Level == 2 && s.Score == 1010 && s.RemainingHearts == 244);
        });
        Run("A09 last heart and capture clear after capture without an empty playable level", () => {
            var s = Fresh(); int index = 0; Cell cell = s.Maze.Hearts[index]; Place(s, cell.Position);
            var state = s.Snapshot(); OnlyHeart(state, index); ActiveAt(state, 0, cell); Apply(s, state); s.Tick();
            Assert(s.Phase == SessionPhase.Caught && s.RemainingHearts == 0 && s.Score == 10);
            Advance(s, 73); Assert(s.Phase == SessionPhase.LevelClear && s.Score == 1010);
        });
        Run("Complete physical traversal collects all 244 hearts and clears a real round", () => {
            var s = Fresh(); Place(s, s.Player); int ticks = 0;
            Action<Point2> walkTo = target => {
                while (s.Phase == SessionPhase.Playing && (s.Maze.Near(target, s.Player) - s.Player).Length > 0.025)
                {
                    Point2 difference = s.Maze.Near(target, s.Player) - s.Player;
                    s.RequestHeading(Math.Abs(difference.x) > Math.Abs(difference.y) ? (difference.x > 0 ? 1 : 3) : (difference.y > 0 ? 2 : 0)); s.Tick();
                    if (++ticks > 150000) throw new Exception("Traversal became stuck at " + s.Player + " toward " + target);
                    Assert(s.Maze.CanOccupy(s.Player), "Player entered a wall");
                }
            };
            Cell cursor = new Cell(13, 23); walkTo(cursor.Position);
            foreach (Cell heart in s.Maze.Hearts)
            {
                while (cursor != heart && s.Phase == SessionPhase.Playing)
                {
                    Cell next = s.Maze.PathStep(cursor, heart, false); Assert(next != cursor);
                    walkTo(next.Position); cursor = next;
                }
            }
            Assert(s.RemainingHearts == 0 && s.Phase == SessionPhase.LevelClear && s.Score == 3600,
                "Incomplete round: " + s.RemainingHearts + " hearts, " + s.Phase + ", " + s.Score);
        });
        Run("A09 last chance and last heart produce Game Over without level bonus", () => {
            var s = Fresh(); int index = 0; Cell cell = s.Maze.Hearts[index]; Place(s, cell.Position);
            var state = s.Snapshot(); OnlyHeart(state, index); state.chances = 1; ActiveAt(state, 0, cell); Apply(s, state); s.Tick();
            Advance(s, 73); Assert(s.Phase == SessionPhase.GameOver && s.Score == 10 && s.Level == 1);
        });
        Run("A10 extra chance crosses 10000 once, including via level bonus", () => {
            var s = Fresh(); Place(s, s.Maze.Hearts[0].Position); var state = s.Snapshot(); OnlyHeart(state, 0); state.score = 9990;
            Apply(s, state); s.Tick(); Assert(s.Chances == 4 && s.Score == 11000);
            Advance(s, 121); Assert(s.Chances == 4);
        });
        Run("A11 full inventory preserves question mark while collecting its heart", () => {
            var s = Fresh(true); Cell cell = MazeMap.FromPair(s.Maze.Definition.itemCells[0]); Place(s, cell.Position);
            var state = s.Snapshot(); state.inventory = ItemKind.BananaPeel; Apply(s, state); s.Tick();
            Assert(s.Score == 10 && s.ItemAvailable[0] && s.Inventory == ItemKind.BananaPeel);
            state = s.Snapshot(); state.inventory = ItemKind.None; Apply(s, state); s.Tick();
            Assert(!s.ItemAvailable[0] && s.Inventory != ItemKind.None && s.Score == 10);
        });
        Run("A12 banana without landing space is not consumed", () => {
            var s = Fresh(true); Place(s, new Point2(1, 1), 270); var state = s.Snapshot(); state.inventory = ItemKind.BananaPeel;
            Apply(s, state); Assert(!s.UseItem() && s.Inventory == ItemKind.BananaPeel);
        });
        Run("Banana starts immediately, flies two cells, then lives 20 seconds after landing", () => {
            var s = Fresh(true); Place(s, new Point2(6, 5), 90); var state = s.Snapshot(); state.inventory = ItemKind.BananaPeel;
            Apply(s, state); Assert(s.UseItem() && s.Inventory == ItemKind.None && s.Traps.Count == 1);
            TrapState trap = s.Traps[0]; Equal(trap.position.x, 8); Equal(trap.position.y, 5);
            Equal(trap.landsAt, GameSession.BananaFlightDuration); Equal(trap.expiresAt - trap.landsAt, 20);
            Equal(trap.PositionAt(s.Maze, 0.18).x, 7);
            s.RequestTurn(RelativeTurn.Forward); s.Tick(); Assert(s.Player.x > 6, "Throw locked player movement");
            Hold(s); Advance(s, 1220); Assert(s.Traps.Count == 1);
            s.Tick(); Assert(s.Traps.Count == 0);
        });
        Run("Banana stops before a wall with clearance for its visible peel", () => {
            var s = Fresh(true); Place(s, new Point2(11, 1), 90); var state = s.Snapshot(); state.inventory = ItemKind.BananaPeel;
            Apply(s, state); Assert(s.UseItem()); TrapState trap = s.Traps[0];
            Assert(trap.position.x > 11 && trap.position.x < 12.3 && s.Maze.CanOccupy(trap.position, GameSession.TrapRadius));
        });
        Run("An enemy at the landing point is hit only when the banana lands", () => {
            var s = Fresh(true); Place(s, new Point2(6, 5), 90); var state = s.Snapshot(); state.inventory = ItemKind.BananaPeel;
            ActiveAt(state, 0, new Cell(8, 5)); state.pursuers[0].stunUntil = GameSession.BananaFlightDuration;
            Apply(s, state); Assert(s.UseItem()); Advance(s, 21);
            Assert(s.Traps.Count == 1 && s.Pursuers[0].slipImmunityUntil == 0);
            s.Tick(); Assert(s.Traps.Count == 0); Equal(s.Pursuers[0].stunUntil, GameSession.BananaFlightDuration + 1.2);
        });
        Run("An enemy crossing before landing does not trigger an airborne banana", () => {
            var s = Fresh(true); Place(s, new Point2(6, 5), 90); var state = s.Snapshot(); state.inventory = ItemKind.BananaPeel;
            ActiveAt(state, 0, new Cell(8, 5)); state.pursuers[0].to = new Cell(9, 5); state.pursuers[0].previous = new Cell(7, 5);
            Apply(s, state); Assert(s.UseItem()); Advance(s, 25);
            Assert(s.Traps.Count == 1 && s.Pursuers[0].slipImmunityUntil == 0);
        });
        Run("Landing within a simulation slice resolves at the true landing time", () => {
            var s = Fresh(true); Place(s, new Point2(6, 5), 90); var state = s.Snapshot(); state.inventory = ItemKind.BananaPeel;
            Apply(s, state); s.UseItem(); state = s.Snapshot(); state.time = state.levelTime = 0.359;
            ActiveAt(state, 0, new Cell(8, 5)); state.pursuers[0].stunUntil = 0.36;
            Apply(s, state); s.Tick(); Assert(s.Traps.Count == 0); Equal(s.Pursuers[0].stunUntil, 1.56);
        });
        Run("An airborne banana freezes through pause, save and resume countdown", () => {
            var s = Fresh(true); Place(s, new Point2(6, 5), 90); var state = s.Snapshot(); state.inventory = ItemKind.BananaPeel;
            Apply(s, state); s.UseItem(); Advance(s, 8); s.Pause();
            string json = JsonSerializer.Serialize(s.Snapshot(), Json); var copy = Fresh(true);
            copy.Restore(JsonSerializer.Deserialize<SessionSnapshot>(json, Json)); Advance(copy, 60);
            Equal(copy.Time, s.Time); Equal(copy.Traps[0].FlightProgress(copy.Time), s.Traps[0].FlightProgress(s.Time));
            copy.Resume(0.5); Advance(copy, 30); Equal(copy.Time, s.Time);
            s.Resume(0); Advance(s, 20); Advance(copy, 20);
            Assert(JsonSerializer.Serialize(copy.Snapshot(), Json) == JsonSerializer.Serialize(s.Snapshot(), Json));
        });
        Run("Build 2 version 1 saves migrate ground traps without replaying throws", () => {
            var s = Fresh(true); Place(s, new Point2(6, 5)); var state = s.Snapshot(); state.version = 1; state.nextTrapId = 1;
            state.traps = new[] { new TrapState { id = 1, kind = ItemKind.BananaPeel, position = new Point2(8, 5), expiresAt = 20 } };
            Apply(s, state); Assert(s.Snapshot().version == 3 && s.Traps[0].FlightProgress(s.Time) == 1);
            Equal(s.Traps[0].PositionAt(s.Maze, s.Time).x, 8); Equal(s.Traps[0].expiresAt, 20);
            Assert(state.version == 1 && state.traps[0].launchPosition.x == 0, "Restore modified source snapshot");
        });
        Run("Invalid flight snapshots are rejected without changing the live round", () => {
            var s = Fresh(true); Place(s, new Point2(6, 5), 90); var state = s.Snapshot(); state.inventory = ItemKind.BananaPeel;
            Apply(s, state); s.UseItem(); string before = JsonSerializer.Serialize(s.Snapshot(), Json);
            state = s.Snapshot(); state.traps[0].landsAt = double.NaN; Reject(() => s.Restore(state));
            state = s.Snapshot(); state.traps[0].launchPosition = new Point2(7, 4); Reject(() => s.Restore(state));
            state = s.Snapshot(); state.traps[0].kind = ItemKind.OilSlick; Reject(() => s.Restore(state));
            state = s.Snapshot(); state.traps[0].position = state.traps[0].launchPosition = new Point2(1e300, 14); Reject(() => s.Restore(state));
            state = s.Snapshot(); state.version = 4; Reject(() => s.Restore(state));
            Assert(before == JsonSerializer.Serialize(s.Snapshot(), Json));
        });
        Run("A throw through a shortcut reserves the passage until its landing trap expires", () => {
            var s = Fresh(true); Place(s, new Point2(12, 1), 90); var state = s.Snapshot();
            state.walls[0].phase = WallPhase.Open; state.walls[0].phaseUntil = state.time + 0.05;
            state.inventory = ItemKind.BananaPeel; Apply(s, state); Assert(s.UseItem());
            state = s.Snapshot(); state.player = new Point2(11, 1); state.trail = new[] { state.player };
            Apply(s, state); Advance(s, 24);
            Assert(s.Walls[0].phase == WallPhase.Open && s.PassageReserved(0));
            state = s.Snapshot(); state.traps[0].expiresAt = state.time + 0.01; Apply(s, state); s.Tick();
            Assert(s.Traps.Count == 0 && s.Walls[0].phase == WallPhase.Closing);
        });
        Run("Banana trajectory wraps through the tunnel without crossing the maze", () => {
            var s = Fresh(true); Place(s, new Point2(27, 14), 90); var state = s.Snapshot(); state.inventory = ItemKind.BananaPeel;
            Apply(s, state); Assert(s.UseItem()); TrapState trap = s.Traps[0];
            Equal(trap.position.x, 1); Equal(trap.PositionAt(s.Maze, 0.18).x, 0);
            var copy = Fresh(true); copy.Restore(JsonSerializer.Deserialize<SessionSnapshot>(JsonSerializer.Serialize(s.Snapshot(), Json), Json));
            Equal(copy.Traps[0].PositionAt(copy.Maze, 0.18).x, 0);
        });
        Run("A12 banana stuns once and shares slip immunity with oil", () => {
            var s = Fresh(true); Place(s, new Point2(1, 1)); var state = s.Snapshot(); ActiveAt(state, 0, new Cell(6, 5));
            state.traps = new[] { new TrapState { id = 1, kind = ItemKind.BananaPeel, position = new Point2(6, 5), expiresAt = 20 } };
            state.nextTrapId = 1; Apply(s, state); s.Tick();
            Assert(s.Traps.Count == 0 && s.PursuerSpeed(s.Pursuers[0]) == 0);
            state = s.Snapshot(); state.traps = new[] { new TrapState { id = 2, kind = ItemKind.OilSlick, position = s.Pursuers[0].Position(s.Maze), expiresAt = 20 } };
            state.nextTrapId = 2; Apply(s, state); s.Tick(); Assert(s.Traps.Count == 1 && s.Pursuers[0].oilUntil == 0);
        });
        Run("A13 boost collects every crossed heart and is faster than threats", () => {
            var s = Fresh(true); Place(s, new Point2(1, 29), 90); var state = s.Snapshot(); state.inventory = ItemKind.SpeedBoost;
            Apply(s, state); Assert(s.UseItem()); Assert(s.PlayerSpeed > LevelBalance.PursuerSpeed(1));
            s.RequestTurn(RelativeTurn.Forward); Advance(s, 180);
            for (int x = 1; x <= 8; x++) Assert(s.Collected[s.Maze.Hearts.IndexOf(new Cell(x, 29))], "Skipped heart " + x);
        });
        Run("A13 swept contact detects actors exchanging sides", () => {
            double time = Geometry.ContactTime(new Point2(0, 0), new Point2(2, 0), new Point2(2, 0), new Point2(0, 0), 0.3);
            Assert(time > 0 && time < 0.5);
        });
        Run("A14 occupied shortcut delays closure without teleporting", () => {
            var s = Fresh(true); Place(s, new Point2(12, 1)); var state = s.Snapshot(); state.player = new Point2(13, 1);
            state.trail = new[] { state.player }; state.walls[0].phase = WallPhase.Open; state.walls[0].phaseUntil = 0;
            Apply(s, state); Advance(s, 180);
            Assert(s.Walls[0].phase == WallPhase.Open && s.Maze.IsWalkable(new Cell(13, 1))); Equal(s.Player.x, 13);
            state = s.Snapshot(); state.player = new Point2(12, 1); state.trail = new[] { state.player };
            Apply(s, state); s.Tick(); Assert(s.Walls[0].phase == WallPhase.Closing && !s.Maze.IsWalkable(new Cell(13, 1)));
        });
        Run("A15 all eight stable wall combinations preserve every heart", () => {
            for (int mask = 0; mask < 8; mask++)
            {
                var maze = new MazeMap(Definition()); for (int i = 0; i < 3; i++) maze.SetShortcut(i, (mask & (1 << i)) != 0);
                var seen = new HashSet<Cell>(); var queue = new Queue<Cell>(); var adjacent = new Cell[4];
                Cell start = new Cell(13, 23); seen.Add(start); queue.Enqueue(start);
                while (queue.Count > 0)
                {
                    Cell c = queue.Dequeue(); int count = maze.Neighbors(c, false, adjacent);
                    for (int i = 0; i < count; i++) if (seen.Add(adjacent[i])) queue.Enqueue(adjacent[i]);
                }
                Assert(maze.Hearts.All(seen.Contains), "Inaccessible heart, wall mask " + mask);
            }
        });
        Run("A16 return routing uses the door and reacts to current topology", () => {
            var maze = new MazeMap(Definition()); Cell c = new Cell(1, 1), target = new Cell(13, 14); int count = 0;
            while (c != target && count++ < 300) { Cell next = maze.PathStep(c, target, true); Assert(next != c); c = next; }
            Assert(c == target);
            maze.SetShortcut(0, true); Assert(maze.PathStep(new Cell(12, 1), new Cell(15, 1), false) == new Cell(13, 1));
            maze.SetShortcut(0, false); Assert(maze.PathStep(new Cell(12, 1), new Cell(15, 1), false) != new Cell(13, 1));
        });
        Run("A17 pause and resume countdown consume no effect or simulation time", () => {
            var s = Fresh(); Place(s, new Point2(1, 1)); var state = s.Snapshot(); state.powerUntil = 6; state.boostUntil = 4;
            Apply(s, state); s.Pause(); Advance(s, 600); Equal(s.Time, 0); Equal(s.PowerUntil, 6);
            s.Resume(1); Advance(s, 60); Equal(s.Time, 0); Assert(s.Phase == SessionPhase.Playing);
        });
        Run("A17 snapshot JSON roundtrip resumes paused and preserves all systems", () => {
            var s = Fresh(true); Place(s, new Point2(1, 1)); Advance(s, 1000);
            string json = JsonSerializer.Serialize(s.Snapshot(), Json);
            var copy = Fresh(true); copy.Restore(JsonSerializer.Deserialize<SessionSnapshot>(json, Json));
            Assert(copy.Phase == SessionPhase.Paused); Equal(copy.Time, s.Time); Assert(copy.Score == s.Score);
            copy.Resume(0); s.Tick(); copy.Tick(); Assert(JsonSerializer.Serialize(copy.Snapshot(), Json) == JsonSerializer.Serialize(s.Snapshot(), Json));
        });
        Run("Corrupt snapshots are rejected without modifying the running game", () => {
            var s = Fresh(); var state = s.Snapshot(); state.collected[0] = true; Reject(() => s.Restore(state));
            state = s.Snapshot(); state.player = new Point2(double.NaN, 1); Reject(() => s.Restore(state));
            state = s.Snapshot(); state.pursuers[0].id = "trump"; Reject(() => s.Restore(state));
            Assert(s.Phase == SessionPhase.Playing && s.RemainingHearts == 244 && s.Score == 0);
        });
        Run("Changed gameplay metadata invalidates old saves even with unchanged rows/id", () => {
            var original = Fresh(); var definition = Definition(); definition.itemCells[0][0] = 7;
            var changed = new GameSession(new MazeMap(definition), countdown: false);
            Reject(() => changed.Restore(original.Snapshot()));
        });
        Run("Atomic save retains a readable backup after a damaged latest file", () => {
            string directory = Path.Combine(Path.GetTempPath(), "maze-check-" + Guid.NewGuid().ToString("N"));
            string path = Path.Combine(directory, "save.json");
            try
            {
                AtomicFile.Write(path, "{\"value\":1}"); AtomicFile.Write(path, "{\"value\":2}"); File.WriteAllText(path, "broken");
                bool success = AtomicFile.TryRead(path, text => {
                    try { return JsonSerializer.Deserialize<Dictionary<string, int>>(text); }
                    catch (JsonException e) { throw new FormatException("Bad JSON", e); }
                }, out Dictionary<string, int> read);
                Assert(success && read["value"] == 1);
            }
            finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
        });
        Run("A18 same seed and inputs reproduce a full 60-second simulation", () => {
            var a = Fresh(true, 99); var b = Fresh(true, 99);
            for (int i = 0; i < 3600; i++) { if (i % 60 == 0) { a.RequestTurn(RelativeTurn.Reverse); b.RequestTurn(RelativeTurn.Reverse); } a.Tick(); b.Tick(); }
            Assert(JsonSerializer.Serialize(a.Snapshot(), Json) == JsonSerializer.Serialize(b.Snapshot(), Json));
        });
        Run("A19 rendering at 30/60/120 fps produces identical fixed-tick state", () => {
            string[] snapshots = new string[3]; int sample = 0;
            foreach (int fps in new[] { 30, 60, 120 })
            {
                var s = Fresh(true, 11); double accumulator = 0;
                for (int frame = 0; frame < fps * 30; frame++)
                {
                    accumulator += 1.0 / fps;
                    while (accumulator + 1e-9 >= GameSession.FixedDelta) { s.Tick(); accumulator -= GameSession.FixedDelta; }
                }
                snapshots[sample++] = JsonSerializer.Serialize(s.Snapshot(), Json);
            }
            Assert(snapshots[0] == snapshots[1] && snapshots[1] == snapshots[2]);
        });
        Run("A21 progression crosses level 255; power remains useful forever", () => {
            var s = Fresh(); var state = s.Snapshot(); state.level = 255; state.phase = SessionPhase.LevelClear; state.phaseRemaining = 0;
            Apply(s, state); s.Tick(); Assert(s.Level == 256 && s.RemainingHearts == 244);
            for (int level = 1; level <= 1000; level++) Assert(LevelBalance.PowerDuration(level) >= 2);
        });
        Run("A23 character IDs are bound to the agreed AI roles", () => {
            var s = Fresh(); Assert(s.Pursuers[0].id == "putin" && s.Pursuers[0].role == PursuerRole.Chaser);
            Assert(s.Pursuers[1].id == "xi" && s.Pursuers[1].role == PursuerRole.Ambusher);
            Assert(s.Pursuers[2].id == "kim" && s.Pursuers[2].role == PursuerRole.Flanker);
            Assert(s.Pursuers[3].id == "trump" && s.Pursuers[3].role == PursuerRole.Wanderer);
        });
        Run("AI roles choose distinct chase targets from movement direction", () => {
            var s = Fresh(); Place(s, new Point2(6, 5), 90); var state = s.Snapshot(); state.scatter = false; state.levelTime = 8; Apply(s, state);
            Equal(s.Target(s.Pursuers[0]).x, 6); Equal(s.Target(s.Pursuers[1]).x, 10);
            Equal(s.Target(s.Pursuers[2]).x, 16 - s.Pursuers[0].Position(s.Maze).x);
            Assert((s.Target(s.Pursuers[3]) - new Point2(6, 5)).Length < 1e-7);
        });
        Run("A24 oil slows to 55 percent for 2.5 seconds and consumes one trap", () => {
            var s = Fresh(true); Place(s, new Point2(1, 1)); var state = s.Snapshot(); ActiveAt(state, 0, new Cell(6, 5));
            state.traps = new[] { new TrapState { id = 1, kind = ItemKind.OilSlick, position = new Point2(6, 5), expiresAt = 15 } };
            state.nextTrapId = 1; Apply(s, state); s.Tick(); Equal(s.PursuerSpeed(s.Pursuers[0]), 1.5 * 0.55);
            Assert(s.Traps.Count == 0); Assert(s.Pursuers[0].oilUntil - s.Time > 2.48);
            Advance(s, 151); Equal(s.PursuerSpeed(s.Pursuers[0]), 1.5);
        });
        Run("Oil follows the actual movement trail rather than camera direction", () => {
            var s = Fresh(true); Place(s, new Point2(6, 5), 90); s.RequestTurn(RelativeTurn.Forward); Advance(s, 30);
            var state = s.Snapshot(); state.inventory = ItemKind.OilSlick; state.yaw = 270; Apply(s, state);
            Assert(s.UseItem()); Assert(s.Traps[0].position.x < s.Player.x); Equal(s.Player.x - s.Traps[0].position.x, 0.5, 0.01);
        });
        Console.WriteLine($"\n{passed} passed, {failed} failed");
        return failed == 0 ? 0 : 1;
    }
}
