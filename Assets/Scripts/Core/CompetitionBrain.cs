using System;
using System.Collections.Generic;

namespace MazeOfEgos.Core
{
    public static class LocalArenaView
    {
        public const double Pitch = 55, Yaw = 25, HalfHeight = 4.5, TargetHeight = .65, Distance = 35;
        private static readonly double SinYaw = Math.Sin(Yaw * Math.PI / 180), CosYaw = Math.Cos(Yaw * Math.PI / 180);
        private static readonly double SinPitch = Math.Sin(Pitch * Math.PI / 180), CosPitch = Math.Cos(Pitch * Math.PI / 180);
        public static Point2 Project(Point2 offset, double height = 1) => new Point2(
            (CosYaw * offset.x + SinYaw * offset.y) * MazeMap.GridMeters,
            (SinPitch * SinYaw * offset.x - SinPitch * CosYaw * offset.y) * MazeMap.GridMeters + (height - TargetHeight) * CosPitch);
        public static bool Contains(Point2 offset, double aspect, double height = 1)
        { Point2 p = Project(offset, height); return Math.Abs(p.x) <= HalfHeight * aspect && Math.Abs(p.y) <= HalfHeight; }
        public static bool SegmentVisible(Point2 a, Point2 b, double aspect)
        {
            a = Project(a); b = Project(b); Point2 d = b - a; double from = 0, until = 1;
            return Clip(-d.x, a.x + HalfHeight * aspect, ref from, ref until) && Clip(d.x, HalfHeight * aspect - a.x, ref from, ref until) &&
                Clip(-d.y, a.y + HalfHeight, ref from, ref until) && Clip(d.y, HalfHeight - a.y, ref from, ref until);
        }
        private static bool Clip(double p, double q, ref double from, ref double until)
        {
            if (Math.Abs(p) < 1e-10) return q >= 0;
            double value = q / p;
            if (p < 0) { if (value > until) return false; from = Math.Max(from, value); }
            else { if (value < from) return false; until = Math.Min(until, value); }
            return true;
        }
    }
    [Serializable] public struct CompetitionResourceMemory { public Cell cell; public bool crate; public long seenTick; }
    [Serializable] public struct CompetitionActorMemory { public string id; public Point2 position; public int heading; public long seenTick; public bool seen; }
    [Serializable]
    public sealed class CompetitionBrainState
    {
        public string id;
        public uint random;
        public long nextDecisionTick, nextGoalTick;
        public Cell goal;
        public bool hasGoal;
        public bool[] knownShortcuts;
        public int[] visits;
        public List<CompetitionResourceMemory> resources = new List<CompetitionResourceMemory>();
        public CompetitionActorMemory[] actors = new CompetitionActorMemory[5];
        public CompetitionBrainState Copy() => new CompetitionBrainState { id = id, random = random, nextDecisionTick = nextDecisionTick, nextGoalTick = nextGoalTick,
            goal = goal, hasGoal = hasGoal, knownShortcuts = (bool[])knownShortcuts.Clone(), visits = (int[])visits.Clone(),
            resources = new List<CompetitionResourceMemory>(resources), actors = (CompetitionActorMemory[])actors.Clone() };
    }
    public readonly struct ObservedResource
    {
        public readonly Cell Cell; public readonly bool Crate, Available;
        public ObservedResource(Cell cell, bool crate, bool available) { Cell = cell; Crate = crate; Available = available; }
    }
    public readonly struct CompetitionMapActor
    {
        public readonly string Id; public readonly Point2 Position;
        public CompetitionMapActor(string id,Point2 position) { Id=id;Position=position; }
    }
    public sealed class CompetitionObservation
    {
        public long tick;
        public CompetitionActorView self;
        public CompetitionActorView[] actors;
        public ObservedResource[] resources;
        public CompetitionAttack[] attacks;
        public bool sharedMinimap;
        public Cell[] mapHearts;
        public CompetitionMapActor[] mapActors;
        public Cell[] visibleCells;
        public int[] visibleShortcuts;
        public bool[] shortcutOpen;
    }
    public readonly struct CompetitionDecision
    {
        public readonly int Heading; public readonly bool UseItem;
        public CompetitionDecision(int heading, bool useItem) { Heading = heading; UseItem = useItem; }
    }
    // This class receives only observations, memory, fixed layout and the shared
    // profile. Global map positions/hearts are shared; combat details remain local.
    // It has no reference to a session or hidden inventory, attacks or crate states.
    public sealed class CompetitionBrain
    {
        private readonly MazeMap known;
        private readonly Cell[] adjacent = new Cell[4];
        public CompetitionBrain(MazeDefinition definition) { known = new MazeMap(definition); }
        public CompetitionDecision Decide(CompetitionObservation obs, CompetitionBrainState memory, CompetitionRules rules)
        {
            if (obs.tick < memory.nextDecisionTick || obs.self.Life != CompetitionLife.Active) return new CompetitionDecision(-1, false);
            memory.nextDecisionTick = obs.tick + rules.reactionTicks;
            for (int i = 0; i < obs.visibleShortcuts.Length; i++) memory.knownShortcuts[obs.visibleShortcuts[i]] = obs.shortcutOpen[i];
            for (int i = 0; i < memory.knownShortcuts.Length; i++) known.SetShortcut(i, memory.knownShortcuts[i]);
            for (int i = memory.resources.Count - 1; i >= 0; i--)
            {
                var old = memory.resources[i]; bool inView = Array.IndexOf(obs.visibleCells, old.cell) >= 0, present = false;
                foreach (ObservedResource resource in obs.resources) if (resource.Cell == old.cell && resource.Crate == old.crate && resource.Available) present = true;
                if (obs.sharedMinimap && !old.crate) { inView=true; present=Array.IndexOf(obs.mapHearts,old.cell)>=0; }
                if ((inView && !present) || obs.tick - old.seenTick > 3600) memory.resources.RemoveAt(i);
            }
            foreach (ObservedResource resource in obs.resources)
            {
                if (!resource.Available) continue;
                int index = memory.resources.FindIndex(r => r.cell == resource.Cell && r.crate == resource.Crate);
                var remembered = new CompetitionResourceMemory { cell = resource.Cell, crate = resource.Crate, seenTick = obs.tick };
                if (index < 0) memory.resources.Add(remembered); else memory.resources[index] = remembered;
            }
            if (obs.sharedMinimap)
            {
                foreach (Cell cell in obs.mapHearts)
                {
                    int index=memory.resources.FindIndex(r=>!r.crate&&r.cell==cell);
                    var resource=new CompetitionResourceMemory {cell=cell,crate=false,seenTick=obs.tick};
                    if(index<0)memory.resources.Add(resource);else memory.resources[index]=resource;
                }
                foreach(var actor in obs.mapActors)
                {
                    int index=0;while(CompetitionSession.ParticipantIds[index]!=actor.Id)index++;
                    // The map exposes position only; local observation supplies heading.
                    var old=memory.actors[index];
                    memory.actors[index]=new CompetitionActorMemory {id=actor.Id,position=actor.Position,heading=old.heading,seenTick=obs.tick,seen=true};
                }
            }
            foreach (CompetitionActorView actor in obs.actors)
            {
                int index = 0; while (CompetitionSession.ParticipantIds[index] != actor.Id) index++;
                memory.actors[index] = new CompetitionActorMemory { id = actor.Id, position = actor.Position, heading = actor.Heading, seenTick = obs.tick, seen = true };
            }
            Cell here = new Cell((int)Math.Round(obs.self.Position.x), (int)Math.Round(obs.self.Position.y)); here = known.Canonical(here);
            if (known.InBounds(here)) memory.visits[here.y * known.Definition.width + here.x]++;
            var random = new RandomState(memory.random);
            int heading = -1; bool use = obs.self.Inventory == CompetitionItem.Boost && obs.self.BoostUntil <= obs.tick;
            bool danger = false; Point2 dangerAt = obs.self.Position;
            foreach (CompetitionAttack attack in obs.attacks)
            {
                if (attack.owner == obs.self.Id) continue;
                Point2 target = known.Near(attack.target, obs.self.Position);
                bool splash = attack.kind == CompetitionItem.WaterBalloon && attack.landTick > obs.tick && (target - obs.self.Position).Length < rules.splashRadius + .35;
                bool cake = attack.kind == CompetitionItem.CreamPie && !double.IsPositiveInfinity(Geometry.ContactTime(
                    known.Near(attack.Position(known, obs.tick), obs.self.Position), target, obs.self.Position, obs.self.Position, .65));
                bool trap = (attack.kind == CompetitionItem.Banana || attack.kind == CompetitionItem.Oil) &&
                    (target - (obs.self.Position + CorridorNavigation.Direction(obs.self.Heading) * .5)).Length < .6;
                if (splash || cake || trap) { danger = true; dangerAt = target; break; }
            }
            if (danger && obs.self.ProtectedUntil <= obs.tick)
            {
                heading = (obs.self.Heading + 2) % 4;
                int count = known.Neighbors(here, true, adjacent); double best = -1;
                for (int i = 0; i < count; i++)
                {
                    Point2 delta = known.Near(adjacent[i].Position, obs.self.Position) - obs.self.Position;
                    if (delta.Length > 1.2) continue;
                    double distance = (known.Near(adjacent[i].Position, dangerAt) - dangerAt).Length;
                    if (distance > best) { best = distance; heading = Direction(delta); }
                }
            }
            else
            {
                Cell goal = memory.goal; double best = double.PositiveInfinity; bool resourceGoal = false;
                foreach (CompetitionResourceMemory resource in memory.resources)
                {
                    double distance = (known.Near(resource.cell.Position, obs.self.Position) - obs.self.Position).Length;
                    double weight = resource.crate ? (obs.self.Inventory == CompetitionItem.None ? .65 : 2.5) : 1;
                    if (obs.sharedMinimap && !resource.crate)
                        foreach(var other in obs.mapActors) if(other.Id!=obs.self.Id && (known.Near(resource.cell.Position,other.Position)-other.Position).Length < distance*.7) weight*=1.35;
                    if (distance * weight < best) { best = distance * weight; goal = resource.cell; resourceGoal = true; }
                }
                if (!resourceGoal && (!memory.hasGoal || obs.tick >= memory.nextGoalTick || (goal.Position - obs.self.Position).Length < .6))
                {
                    int lowest = int.MaxValue;
                    for (int attempt = 0; attempt < 16; attempt++)
                    {
                        Cell candidate = known.PlayerCells[random.Range(known.PlayerCells.Count)];
                        int visits = memory.visits[candidate.y * known.Definition.width + candidate.x];
                        if (visits < lowest) { lowest = visits; goal = candidate; }
                    }
                    memory.nextGoalTick = obs.tick + 420;
                }
                memory.goal = goal; memory.hasGoal = true;
                Cell anchor = NextCentre(obs.self.Position, obs.self.Heading); anchor = known.Canonical(anchor);
                if (!known.IsWalkable(anchor, true)) anchor = here;
                Cell next = known.PathStep(anchor, goal, true);
                Point2 delta = known.Near(next.Position, anchor.Position) - anchor.Position;
                heading = delta.Length > .1 ? Direction(delta) : obs.self.Heading;
            }
            foreach (CompetitionActorView other in obs.actors)
            {
                if (other.Life != CompetitionLife.Active || other.ProtectedUntil > obs.tick) continue;
                Point2 offset = known.Near(other.Position, obs.self.Position) - obs.self.Position;
                Point2 forward = CorridorNavigation.Direction(obs.self.Heading);
                double along = Point2.Dot(offset, forward), side = Math.Abs(offset.x * forward.y - offset.y * forward.x);
                if (obs.self.Inventory == CompetitionItem.CreamPie && along > .3 && along < rules.cakeRange && side < .28 && known.HasLineOfSight(obs.self.Position, other.Position, true)) use = true;
                if (obs.self.Inventory == CompetitionItem.WaterBalloon && along > 1 && along < rules.balloonRange + .6 && side < .65 && known.HasLineOfSight(obs.self.Position, other.Position, true)) use = true;
                if (obs.self.Inventory == CompetitionItem.Banana && offset.Length < 4) use = true;
                if (obs.self.Inventory == CompetitionItem.Oil && along < 0 && offset.Length < 4) use = true;
            }
            memory.random = random.state;
            return new CompetitionDecision(heading, use);
        }
        private static int Direction(Point2 p) => Math.Abs(p.x) > Math.Abs(p.y) ? (p.x > 0 ? 1 : 3) : (p.y > 0 ? 2 : 0);
        private static Cell NextCentre(Point2 p, int heading)
        {
            const double e = 1e-8;
            return heading == 0 ? new Cell((int)Math.Round(p.x), (int)Math.Floor(p.y + e)) :
                heading == 2 ? new Cell((int)Math.Round(p.x), (int)Math.Ceiling(p.y - e)) :
                heading == 1 ? new Cell((int)Math.Ceiling(p.x - e), (int)Math.Round(p.y)) : new Cell((int)Math.Floor(p.x + e), (int)Math.Round(p.y));
        }
    }
    public sealed partial class CompetitionSession
    {
        private CompetitionBrainState[] brainMemory;
        private CompetitionBrain[] brainEngines;
        private double viewAspect = 16.0 / 9;
        public void ConfigureView(double aspect)
        { if (!double.IsNaN(aspect) && aspect >= 1 && aspect <= 3) viewAspect = aspect; }
        private void InitializeBrains(uint seed)
        {
            brainMemory = new CompetitionBrainState[5]; brainEngines = new CompetitionBrain[5];
            for (int i = 0; i < 5; i++)
            {
                brainMemory[i] = NewBrain(i, Stream(seed, (uint)(0xB00 + i)).state, Maze.Definition);
                brainEngines[i] = new CompetitionBrain(Maze.Definition);
            }
        }
        private static CompetitionBrainState NewBrain(int index, uint random, MazeDefinition definition) => new CompetitionBrainState {
            id = Ids[index], random = random, knownShortcuts = new bool[definition.shortcutCandidates.Length], visits = new int[definition.width * definition.height]
        };
        private void ResetBrainRound()
        {
            if (brainMemory == null) return;
            for (int i = 0; i < 5; i++) brainMemory[i] = NewBrain(i, brainMemory[i].random, Maze.Definition);
        }
        private CompetitionBrainState[] CopyBrains()
        { var result = new CompetitionBrainState[5]; for (int i = 0; i < 5; i++) result[i] = brainMemory[i].Copy(); return result; }
        private void RestoreBrains(CompetitionBrainState[] saved)
        {
            brainMemory = new CompetitionBrainState[5]; brainEngines = new CompetitionBrain[5];
            for (int i = 0; i < 5; i++)
            { brainMemory[i] = Array.Find(saved, b => b.id == Ids[i]).Copy(); brainEngines[i] = new CompetitionBrain(Maze.Definition); }
        }
        private void DriveComputers()
        {
            if (!ComputerPlayers) return;
            for (int i = 0; i < 5; i++) if (i != HumanIndex) DriveObservedActor(i);
        }
        public void DriveHumanForAutomation() { if (Phase == CompetitionPhase.Playing) DriveObservedActor(HumanIndex); }
        private void DriveObservedActor(int index)
        {
            if (participants[index].life != CompetitionLife.Active || brainMemory[index].nextDecisionTick > TickIndex) return;
            CompetitionDecision decision = brainEngines[index].Decide(Observe(Ids[index]), brainMemory[index], rules);
            if (decision.Heading >= 0) ApplyCommand(new CompetitionCommand(Ids[index], TickIndex, decision.Heading));
            if (decision.UseItem) UseItem(Ids[index], TickIndex);
        }
        public CompetitionObservation Observe(string id)
        {
            CompetitionParticipant self = participants[IndexOf(id)];
            var actors = new List<CompetitionActorView>(); var resources = new List<ObservedResource>();
            var visibleAttacks = new List<CompetitionAttack>(); var cells = new List<Cell>(); var shortcuts = new List<int>(); var opened = new List<bool>();
            for (int i = 0; i < 5; i++) if (Ids[i] != id && Visible(self.position, participants[i].position)) actors.Add(Actor(i));
            foreach (CompetitionHeart heart in hearts) if (Visible(self.position, heart.cell.Position)) resources.Add(new ObservedResource(heart.cell, false, true));
            foreach (CompetitionCrate crate in crates) if (Visible(self.position, crate.cell.Position)) resources.Add(new ObservedResource(crate.cell, true, crate.availableAtTick <= TickIndex));
            foreach (CompetitionAttack attack in attacks)
            {
                Point2 start = Maze.Near(attack.Position(Maze, TickIndex), self.position) - self.position;
                Point2 end = Maze.Near(attack.target, self.position) - self.position;
                if (LocalArenaView.SegmentVisible(start, end, viewAspect)) visibleAttacks.Add(attack);
            }
            for (int y = 0; y < Maze.Definition.height; y++) for (int x = 0; x < Maze.Definition.width; x++)
            { var cell = new Cell(x, y); if (Maze.IsWalkable(cell, true) && Visible(self.position, cell.Position)) cells.Add(cell); }
            for (int i = 0; i < walls.Length; i++)
                foreach (int[] pair in Maze.Definition.shortcutCandidates[i].wallCells)
                    if (rules.sharedMinimap || Visible(self.position, MazeMap.FromPair(pair).Position)) { shortcuts.Add(i); opened.Add(Maze.OpenShortcuts[i]); break; }
            var mapHearts=new List<Cell>();var mapActors=new List<CompetitionMapActor>();
            if(rules.sharedMinimap) {foreach(var heart in hearts)mapHearts.Add(heart.cell);foreach(var actor in participants)if(actor.life==CompetitionLife.Active)mapActors.Add(new CompetitionMapActor(actor.id,actor.position));}
            return new CompetitionObservation { sharedMinimap=rules.sharedMinimap,mapHearts=mapHearts.ToArray(),mapActors=mapActors.ToArray(),tick = TickIndex, self = Actor(IndexOf(id)), actors = actors.ToArray(), resources = resources.ToArray(),
                attacks = visibleAttacks.ToArray(), visibleCells = cells.ToArray(), visibleShortcuts = shortcuts.ToArray(), shortcutOpen = opened.ToArray() };
        }
        private bool Visible(Point2 observer, Point2 target) => LocalArenaView.Contains(Maze.Near(target, observer) - observer, viewAspect);
    }
}
