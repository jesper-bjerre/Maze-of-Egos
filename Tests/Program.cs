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
    private static GameSession Fresh(bool additions = false, uint seed = 42) => new GameSession(new MazeMap(Definition()), seed,
        new RuleProfile { itemsEnabled = additions, movingWallsEnabled = additions, levelClearBonusEnabled = true }, false);
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
        Run("A03 tunnel preserves position offset, yaw, and movement", () => {
            var s = Fresh(); Place(s, new Point2(-0.48, 14.08), 270); s.SetMovement(new Point2(0, 1)); s.Tick();
            Assert(s.Player.x > 27.45 && s.Player.x < 27.5); Equal(s.Player.y, 14.08); Equal(s.Yaw, 270);
            s.SetMovement(new Point2(0, -1)); s.Tick(); Assert(s.Player.x < -0.45);
        });
        Run("Free movement stops on release and cannot cross a wall", () => {
            var s = Fresh(); Place(s, new Point2(13.5, 23)); s.SetMovement(new Point2(0, 1)); Advance(s, 120);
            Assert(s.Player.y >= 22.75 && s.Player.y < 23); Assert(s.Maze.CanOccupy(s.Player));
            Point2 stopped = s.Player; s.SetMovement(new Point2()); Advance(s, 60); Equal((s.Player - stopped).Length, 0);
        });
        Run("Analog diagonals do not move faster", () => {
            var straight = Fresh(); var diagonal = Fresh();
            Place(straight, new Point2(12, 5), 90); Place(diagonal, new Point2(12, 5), 90);
            straight.SetMovement(new Point2(0, 1)); diagonal.SetMovement(new Point2(1, 1));
            straight.Tick(); diagonal.Tick(); Equal((straight.Player - new Point2(12, 5)).Length, (diagonal.Player - new Point2(12, 5)).Length);
        });
        Run("A04 updated input contract: small joystick magnitude gives proportional speed", () => {
            var s = Fresh(); Place(s, new Point2(12, 5), 90); s.SetMovement(new Point2(0, 0.5)); s.Tick();
            Equal(s.Player.x, 12 + LevelBalance.PlayerSpeed(1) * GameSession.FixedDelta * 0.5);
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
                    double speed = Math.Min(1, difference.Length / (s.PlayerSpeed * GameSession.FixedDelta));
                    Point2 movement = difference.Normalized * speed;
                    s.SetMovement(new Point2(movement.x, -movement.y)); s.Tick();
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
            s.SetMovement(new Point2(0, 1)); Advance(s, 180);
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
            for (int i = 0; i < 3600; i++) { a.SetMovement(new Point2(0, i % 120 < 60 ? 0.7 : -0.7)); b.SetMovement(new Point2(0, i % 120 < 60 ? 0.7 : -0.7)); a.Tick(); b.Tick(); }
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
            var s = Fresh(true); Place(s, new Point2(6, 5), 90); s.SetMovement(new Point2(0, 1)); Advance(s, 30);
            var state = s.Snapshot(); state.inventory = ItemKind.OilSlick; state.yaw = 270; Apply(s, state);
            Assert(s.UseItem()); Assert(s.Traps[0].position.x < s.Player.x); Equal(s.Player.x - s.Traps[0].position.x, 0.5, 0.01);
        });
        Console.WriteLine($"\n{passed} passed, {failed} failed");
        return failed == 0 ? 0 : 1;
    }
}
