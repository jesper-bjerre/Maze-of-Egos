using System;
using System.Collections.Generic;

namespace MazeOfEgos.Core
{
    public sealed partial class CompetitionSession
    {
        private CompetitionSnapshot MigrateSnapshot(CompetitionSnapshot source)
        {
            if (source == null || (source.version != 1 && source.version != 2 && source.version != 3)) return source;
            if (source.version == 3)
            {
                if (source.format != "maze-of-egos-competition" || source.rules == null || source.rules.id != "competition-v3" || source.roundNumber < 1 || source.roundNumber > 11)
                    throw new ArgumentException("Unsupported competition save.");
                var upgraded = source.CopyEnvelope(); upgraded.version = 4; upgraded.rules = source.rules.Copy(); upgraded.rules.id = "competition-v4";
                upgraded.rules.endWhenHeartsCollected = true; upgraded.rules.splashRadius = 1;
                upgraded.roundStartedTick = (source.roundNumber - 1L) * 7200;
                var active = source.phase == CompetitionPhase.Paused ? source.resumePhase : source.phase;
                upgraded.roundEnd = active == CompetitionPhase.RoundResults || active == CompetitionPhase.MatchResults ? CompetitionRoundEnd.TimeExpired : CompetitionRoundEnd.None;
                return upgraded;
            }
            if (source.version == 2)
            {
                if (source.format != "maze-of-egos-competition" || source.rules == null || source.rules.id != "competition-v2") throw new ArgumentException("Unsupported competition save.");
                var upgraded = source.CopyEnvelope(); upgraded.version = 3; upgraded.rules = source.rules.Copy(); upgraded.rules.id = "competition-v3";
                upgraded.rules.randomRoundStarts = true; upgraded.rules.replenishHearts = false; upgraded.rules.sharedMinimap = true;
                upgraded.spawnRandom = Stream(source.resourceRandom, 0x5A47u).state;
                return MigrateSnapshot(upgraded);
            }
            if (source.format != "maze-of-egos-competition" || source.rules == null || source.rules.id != "competition-n2-v1" || source.participants == null)
                throw new ArgumentException("Unsupported competition save.");
            var s = source.CopyEnvelope(); s.version = 2; s.rules = source.rules.Copy(); s.rules.id = "competition-v2";
            s.participants = new CompetitionParticipant[source.participants.Length];
            for (int i = 0; i < s.participants.Length; i++)
            {
                if (source.participants[i] == null) throw new ArgumentException("Missing participant.");
                s.participants[i] = source.participants[i].Copy();
                s.participants[i].trail = new List<Point2> { s.participants[i].position };
            }
            s.attacks = Array.Empty<CompetitionAttack>(); s.nextAttackId = 1; s.statistics = new CompetitionStatistics();
            s.computerPlayers = false; s.viewAspect = 16.0 / 9; s.brains = new CompetitionBrainState[5];
            for (int i = 0; i < 5; i++) s.brains[i] = NewBrain(i, Stream(s.resourceRandom, (uint)(i + 1)).state, Maze.Definition);
            return MigrateSnapshot(s);
        }
        private static bool ValidPoint(Point2 p, MazeMap map) => p.IsFinite && p.x >= -.5 && p.x < map.Definition.width - .5 && p.y >= 0 && p.y < map.Definition.height;
        private static void ValidateCombatAndBrains(CompetitionSnapshot s, MazeMap map)
        {
            if (s.attacks == null || s.attacks.Length > 100 || s.nextAttackId < 1 || s.nextAttackId > 1000000 ||
                double.IsNaN(s.viewAspect) || s.viewAspect < 1 || s.viewAspect > 3 || s.brains == null || s.brains.Length != 5 || s.statistics == null)
                throw new ArgumentException("Invalid combat envelope.");
            foreach (var p in s.participants)
            {
                if (!Enum.IsDefined(typeof(CompetitionLife), p.life) || !Enum.IsDefined(typeof(CompetitionItem), p.lastUsedItem) ||
                    p.trail == null || p.trail.Count < 1 || p.trail.Count > 64 || p.knockoutAt < 0 || p.knockoutAt > s.tick ||
                    p.knockoutUntil < 0 || p.knockoutUntil > s.tick + 300 || p.protectedUntil < 0 || p.protectedUntil > s.tick + 180 ||
                    p.slipUntil < 0 || p.slipUntil > s.tick + 120 || p.oilUntil < 0 || p.oilUntil > s.tick + 600 ||
                    p.boostUntil < 0 || p.boostUntil > s.tick + 600 || p.throwUntil < 0 || p.throwUntil > s.tick + 120 ||
                    p.lastUseTick < 0 || p.lastUseTick > s.tick || p.trapImmunityUntil < 0 || p.trapImmunityUntil > s.tick + 180 ||
                    (p.life == CompetitionLife.KnockedOut && (p.knockoutUntil < s.tick || p.knockoutUntil != p.knockoutAt + 300)))
                    throw new ArgumentException("Invalid combat participant.");
                foreach (Point2 point in p.trail) if (!ValidPoint(point, map)) throw new ArgumentException("Invalid movement history.");
            }
            int previous = 0;
            foreach (var a in s.attacks)
            {
                IndexOf(a.owner);
                if (a.id <= previous || a.id >= s.nextAttackId || a.kind == CompetitionItem.None || a.kind == CompetitionItem.Boost ||
                    !Enum.IsDefined(typeof(CompetitionItem), a.kind) || !ValidPoint(a.origin, map) || !ValidPoint(a.target, map) ||
                    a.createdTick < 0 || a.createdTick > s.tick || a.launchTick < a.createdTick || a.launchTick > a.createdTick + 90 ||
                    (map.Near(a.target,a.origin)-a.origin).Length > 5.01 || !map.CanOccupy(a.target,.01,true) ||
                    a.landTick < a.launchTick || a.landTick > a.launchTick + 360 || a.expiresTick <= a.landTick ||
                    a.expiresTick > a.landTick + 1200 || a.expiresTick < s.tick ||
                    !map.HasLineOfSight(a.Position(map, s.tick), a.target, true))
                    throw new ArgumentException("Invalid attack.");
                previous = a.id;
            }
            var stats = s.statistics;
            if (stats.hearts < 0 || stats.itemUses < 0 || stats.knockouts < 0 || stats.respawns < 0 ||
                stats.usesByItem == null || stats.usesByItem.Length != 6 || stats.usesByActor == null || stats.usesByActor.Length != 5 ||
                stats.knockoutsByActor == null || stats.knockoutsByActor.Length != 5) throw new ArgumentException("Invalid statistics.");
            foreach (var values in new[] { stats.usesByItem, stats.usesByActor, stats.knockoutsByActor }) foreach (int n in values) if (n < 0) throw new ArgumentException("Invalid statistics count.");
            var ids = new HashSet<string>();
            foreach (var b in s.brains)
            {
                if (b == null || !ids.Add(b.id)) throw new ArgumentException("Invalid AI identities.");
                IndexOf(b.id);
                if (b.random == 0 || b.nextDecisionTick < 0 || b.nextDecisionTick > s.tick + 60 || b.nextGoalTick < 0 || b.nextGoalTick > s.tick + 420 ||
                    b.knownShortcuts == null || b.knownShortcuts.Length != s.walls.Length || b.visits == null || b.visits.Length != map.Definition.width * map.Definition.height ||
                    b.resources == null || b.resources.Count > 300 || b.actors == null || b.actors.Length != 5 || (b.hasGoal && !map.PlayerCells.Contains(b.goal)))
                    throw new ArgumentException("Invalid AI memory.");
                foreach (int n in b.visits) if (n < 0) throw new ArgumentException("Invalid visit count.");
                var cells = new HashSet<string>();
                foreach (var r in b.resources)
                    if (!map.PlayerCells.Contains(r.cell) || r.seenTick < 0 || r.seenTick > s.tick || !cells.Add(r.cell.ToString() + r.crate)) throw new ArgumentException("Invalid resource memory.");
                for (int i = 0; i < 5; i++)
                {
                    var a = b.actors[i];
                    if (a.seen && (a.id != Ids[i] || !ValidPoint(a.position, map) || a.heading < 0 || a.heading > 3 || a.seenTick < 0 || a.seenTick > s.tick))
                        throw new ArgumentException("Invalid opponent memory.");
                }
            }
        }
    }
}
