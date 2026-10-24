using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MazeOfEgos.Core;

internal static class StressChecks
{
    private static void Require(bool ok, string message) { if (!ok) throw new InvalidOperationException(message); }
    private static int Main()
    {
        var json = new JsonSerializerOptions { IncludeFields = true, IgnoreReadOnlyProperties = true };
        string source = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "classic_reference.json"));
        var definition = JsonSerializer.Deserialize<MazeDefinition>(source, json);
        int captures = 0, hearts = 0, items = 0, checkpoints = 0, restarts = 0, wallTransitions = 0;
        int activeMask = 0; double playingSeconds = 0;
        var seeds = new List<object>();
        for (uint seed = 1; seed <= 10; seed++)
        {
            var random = new RandomState(seed * 7919);
            var session = new GameSession(new MazeMap(definition), seed, countdown: false);
            var neighbors = new Cell[4]; Cell previous = new Cell(-1, -1);
            Point2 target = session.Player; int localRestarts = 0;
            for (int tick = 0; tick < 900 * 60; tick++)
            {
                if (session.Phase == SessionPhase.GameOver)
                {
                    session = new GameSession(new MazeMap(definition), random.Next(), countdown: false);
                    target = session.Player; previous = new Cell(-1, -1); restarts++; localRestarts++;
                }
                if (session.Phase == SessionPhase.Playing)
                {
                    if ((session.Maze.Near(target, session.Player) - session.Player).Length < .04 || !session.Maze.CanOccupy(target))
                    {
                        Cell center = session.Maze.NearestPlayerCell(session.Player);
                        if ((session.Maze.Near(center.Position, session.Player) - session.Player).Length > .04) target = center.Position;
                        else
                        {
                            int count = session.Maze.Neighbors(center, false, neighbors);
                            Require(count > 0, "Player stranded at " + center);
                            int choice = random.Range(count);
                            if (count > 1 && neighbors[choice] == previous) choice = (choice + 1) % count;
                            target = neighbors[choice].Position; previous = center;
                        }
                    }
                    Point2 offset = session.Maze.Near(target, session.Player) - session.Player;
                    int heading = Math.Abs(offset.x) > Math.Abs(offset.y) ? (offset.x > 0 ? 1 : 3) : (offset.y > 0 ? 2 : 0);
                    session.RequestHeading(heading);
                    if (session.Inventory != ItemKind.None && tick % 30 == 0 && session.UseItem()) items++;
                }
                else target = session.Player;
                double before = session.Time; session.Tick();
                playingSeconds += session.Time - before;
                foreach (GameEvent e in session.Events)
                {
                    if (e.Kind == EventKind.Heart || e.Kind == EventKind.PowerHeart) hearts++;
                    if (e.Kind == EventKind.Caught) { captures++; target = session.Player; }
                    if (e.Kind == EventKind.WallOpened || e.Kind == EventKind.WallClosed) wallTransitions++;
                }
                Require(session.Player.IsFinite && session.Maze.CanOccupy(session.Player), "Illegal player position, seed=" + seed + " tick=" + tick);
                Require(CorridorNavigation.HasLegalPosition(session.Maze, session.Player, CorridorNavigation.FromYaw(session.Yaw)), "Player left corridor centre line: seed=" + seed + " tick=" + tick + " position=" + session.Player + " yaw=" + session.Yaw + " phase=" + session.Phase);
                Require(session.RemainingHearts == session.Collected.Count(c => !c), "Heart counter drift");
                Require(session.Chances >= 0 && session.Chances <= 4 && session.Score >= 0, "Invalid chance/score domain");
                foreach (var actor in session.Pursuers)
                {
                    if (actor.life == PursuerLife.Active) activeMask |= 1 << Array.IndexOf(session.Pursuers, actor);
                    Require(actor.Position(session.Maze).IsFinite && actor.progress >= 0 && actor.progress <= 1, "Invalid actor path progress");
                }
                if (tick % 600 == 0 && session.Phase != SessionPhase.GameOver)
                {
                    session.Pause(); double time = session.Time;
                    var state = JsonSerializer.Deserialize<SessionSnapshot>(JsonSerializer.Serialize(session.Snapshot(), json), json);
                    for (int i = 0; i < 120; i++) session.Tick();
                    Require(session.Time == time, "Paused stress session advanced");
                    session.Restore(state); session.Resume(0); checkpoints++;
                }
            }
            seeds.Add(new { seed, nominalSeconds = 900, restarts = localRestarts });
        }
        Require(hearts > 1000 && captures > 0 && items > 0 && wallTransitions > 0 && activeMask == 15 && checkpoints > 500, "Stress run missed important systems");
        var report = new { passed = true, seedCount = 10, fixedTicks = 540000, nominalSimulatedSeconds = 9000,
            playingSeconds, hearts, captures, items, wallTransitions, checkpoints, restarts, activeMask, seeds,
            scope = "Core simulation with randomized legal junction routes, automatic centre-line movement and queued turns, live AI, normal captures/restarts and checkpoint roundtrips. No Unity rendering or physical-device claims." };
        Directory.CreateDirectory(".artifacts"); File.WriteAllText(".artifacts/stress-build10.json", JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        Console.WriteLine("PASS 10 seeds / 540000 fixed ticks; hearts=" + hearts + ", captures=" + captures + ", items=" + items + ", wall transitions=" + wallTransitions + ", checkpoints=" + checkpoints);
        return 0;
    }
}
