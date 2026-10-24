using System;
using System.Collections.Generic;

namespace MazeOfEgos.Core
{
    public sealed partial class CompetitionSession
    {
        private readonly List<CompetitionAttack> attacks = new List<CompetitionAttack>();
        private int nextAttackId = 1;
        private CompetitionStatistics statistics = new CompetitionStatistics();
        public readonly List<CompetitionEvent> Events = new List<CompetitionEvent>();
        public CompetitionStatistics Statistics => statistics.Copy();
        public int HumanIndex => IndexOf(HumanId);
        public CompetitionActorView Actor(int index) => new CompetitionActorView(participants[index], settledRound == RoundNumber);
        public double SplashRadius => rules.splashRadius;
        public int HeartCount => hearts.Count;
        public Cell HeartCell(int index) => hearts[index].cell;
        public int HeartId(int index) => hearts[index].id;
        public int CrateCount => crates.Length;
        public Cell CrateCell(int index) => crates[index].cell;
        public bool CrateAvailable(int index) => crates[index].availableAtTick <= TickIndex;
        public int AttackCount => attacks.Count;
        public CompetitionAttack Attack(int index) => attacks[index];
        public WallPhase ShortcutPhase(int index) => walls[index].phase;
        public long ShortcutUntil(int index) => walls[index].untilTick;
        public double Speed(string id) => SpeedOf(participants[IndexOf(id)]);
        private double SpeedOf(CompetitionParticipant p) => p.life != CompetitionLife.Active || p.slipUntil > TickIndex ? 0 :
            rules.speed * (p.boostUntil > TickIndex ? rules.boostMultiplier : 1) * (p.oilUntil > TickIndex ? rules.oilMultiplier : 1);

        public Point2 Aim(string id)
        {
            CompetitionParticipant p = participants[IndexOf(id)];
            if (p.inventory == CompetitionItem.Oil) return TrailPoint(p, .5);
            double range = p.inventory == CompetitionItem.CreamPie ? rules.cakeRange : p.inventory == CompetitionItem.WaterBalloon ? rules.balloonRange : 2;
            return TargetAhead(p.position, p.heading, range);
        }
        private Point2 TargetAhead(Point2 origin, int heading, double range)
        {
            Point2 result = origin, direction = CorridorNavigation.Direction(heading);
            for (double d = .05; d <= range + 1e-8; d += .05)
            {
                Point2 candidate = Maze.Wrap(origin + direction * d);
                if (!Maze.CanOccupy(candidate, .05, true)) break;
                result = candidate;
            }
            return result;
        }
        private Point2 TrailPoint(CompetitionParticipant p, double distance)
        {
            Point2 previous = p.position;
            for (int i = p.trail.Count - 1; i >= 0; i--)
            {
                Point2 next = Maze.Near(p.trail[i], previous); double length = (next - previous).Length;
                if (length >= distance) return Maze.Wrap(Point2.Lerp(previous, next, distance / Math.Max(length, 1e-9)));
                distance -= length; previous = next;
            }
            return p.position;
        }
        public bool UseItem(string id, long tick)
        {
            int index = IndexOf(id); CompetitionParticipant p = participants[index];
            if (Phase != CompetitionPhase.Playing || tick != TickIndex || p.life != CompetitionLife.Active || p.inventory == CompetitionItem.None) return false;
            CompetitionItem kind = p.inventory; Point2 target = Aim(id);
            if (kind == CompetitionItem.Oil && (Maze.Near(target, p.position) - p.position).Length < .25) return false;
            if ((kind == CompetitionItem.Banana || kind == CompetitionItem.CreamPie || kind == CompetitionItem.WaterBalloon) &&
                (Maze.Near(target, p.position) - p.position).Length < .20) return false;
            p.inventory = CompetitionItem.None; p.lastUsedItem = kind; p.lastUseTick = TickIndex;
            if (kind == CompetitionItem.Boost) p.boostUntil = TickIndex + rules.boostTicks;
            else
            {
                p.protectedUntil = TickIndex; p.throwUntil = TickIndex + 36;
                long launch = TickIndex + (kind == CompetitionItem.CreamPie || kind == CompetitionItem.WaterBalloon ? rules.windupTicks : 0);
                long landing = kind == CompetitionItem.CreamPie ? launch + Math.Max(1, (long)Math.Ceiling((Maze.Near(target, p.position) - p.position).Length / rules.cakeSpeed * 60)) :
                    kind == CompetitionItem.WaterBalloon ? launch + rules.balloonFlightTicks : kind == CompetitionItem.Banana ? launch + rules.bananaFlightTicks : launch;
                attacks.Add(new CompetitionAttack { id = nextAttackId++, owner = id, kind = kind, origin = p.position, target = target,
                    createdTick = TickIndex, launchTick = launch, landTick = landing,
                    expiresTick = landing + (kind == CompetitionItem.Banana ? 1200 : kind == CompetitionItem.Oil ? 900 : kind == CompetitionItem.WaterBalloon ? 18 : 1) });
            }
            statistics.itemUses++; statistics.usesByItem[(int)kind]++; statistics.usesByActor[index]++;
            Events.Add(new CompetitionEvent(CompetitionEventKind.ItemUsed, id)); return true;
        }
        private void ResetCombatRound()
        {
            attacks.Clear(); nextAttackId = 1; Events.Clear();
            foreach (CompetitionParticipant p in participants)
            {
                p.life = CompetitionLife.Active; p.knockoutUntil = p.knockoutAt = p.protectedUntil = p.slipUntil = p.oilUntil = p.boostUntil = p.throwUntil = p.lastUseTick = p.trapImmunityUntil = 0;
                p.lastUsedItem = CompetitionItem.None; p.trail = new List<Point2> { p.position };
            }
        }
        private void BeforeCombatTick()
        {
            for (int i = attacks.Count - 1; i >= 0; i--) if (attacks[i].expiresTick <= TickIndex) attacks.RemoveAt(i);
            for (int i = 0; i < 5; i++)
            {
                CompetitionParticipant p = participants[i];
                if (p.life != CompetitionLife.KnockedOut || p.knockoutUntil > TickIndex) continue;
                int start = (i + RoundNumber + (int)(TickIndex / 300)) % Starts.Length, chosen = start;
                for (int j = 0; j < Starts.Length; j++)
                {
                    int cell = (start + j) % Starts.Length; bool occupied = false;
                    for (int k = 0; k < 5; k++) if (k != i && participants[k].life == CompetitionLife.Active &&
                        (participants[k].position - Starts[cell].Position).Length < .5) occupied = true;
                    if (!occupied) { chosen = cell; break; }
                }
                // If all five centres are occupied, harmless overlap plus protection
                // gives an immediate exit rather than an unbounded respawn wait.
                p.position = Starts[chosen].Position; p.life = CompetitionLife.Active; p.knockoutUntil = 0;
                p.protectedUntil = TickIndex + rules.protectionTicks; p.slipUntil = p.oilUntil = p.trapImmunityUntil = 0;
                p.trail.Clear(); p.trail.Add(p.position); Array.Clear(p.insideCrates, 0, p.insideCrates.Length);
                navigation[i].Restore(0, -1, true); SyncNavigation(i);
                statistics.respawns++; Events.Add(new CompetitionEvent(CompetitionEventKind.Respawn, p.id));
            }
        }
        private void RecordTrail(CompetitionParticipant p)
        {
            if (p.life != CompetitionLife.Active) return;
            if (p.trail.Count == 0 || (Maze.Near(p.position, p.trail[p.trail.Count - 1]) - p.trail[p.trail.Count - 1]).Length > .04)
            { p.trail.Add(p.position); if (p.trail.Count > 64) p.trail.RemoveAt(0); }
        }
        private double AttackContact(CompetitionAttack attack, int index, double radius, double armedFrom, bool flying)
        {
            double earliest = double.PositiveInfinity;
            foreach (CorridorSegment segment in paths[index])
            {
                double from = Math.Max(segment.From, armedFrom), until = segment.Until; if (from > until) continue;
                double fraction = segment.Until > segment.From ? (from - segment.From) / (segment.Until - segment.From) : 0;
                Point2 actor0 = Point2.Lerp(segment.Start, segment.End, fraction), actor1 = segment.End;
                Point2 shot0 = flying ? attack.Position(Maze, TickIndex + from) : attack.target;
                Point2 shot1 = flying ? Maze.Near(attack.Position(Maze, TickIndex + until), shot0) : shot0;
                actor0 = Maze.Near(actor0, shot0); actor1 = Maze.Near(actor1, actor0);
                double contact = Geometry.ContactTime(shot0, shot1, actor0, actor1, radius);
                if (!double.IsPositiveInfinity(contact)) earliest = Math.Min(earliest, from + contact * (until - from));
            }
            return earliest;
        }
        private bool BeforeDeadline(double fraction) => RoundElapsedTicks + 1 < rules.roundDurationTicks || fraction < 1 - 1e-9;
        private void ResolveCombat()
        {
            for (int shot = attacks.Count - 1; shot >= 0; shot--)
            {
                CompetitionAttack attack = attacks[shot];
                if (attack.kind == CompetitionItem.WaterBalloon)
                {
                    if (TickIndex >= attack.landTick || TickIndex + 1 < attack.landTick) continue;
                    double moment = attack.landTick - TickIndex;
                    if (!BeforeDeadline(moment)) continue;
                    for (int i = 0; i < 5; i++)
                    {
                        CompetitionParticipant p = participants[i];
                        if (!CanHit(p, attack.owner, false)) continue;
                        if ((Maze.Near(p.position, attack.target) - attack.target).Length <= rules.splashRadius + 1e-9 && Maze.HasLineOfSight(attack.target, p.position, true)) KnockOut(i, attack.owner);
                    }
                    continue;
                }
                bool cake = attack.kind == CompetitionItem.CreamPie;
                if (cake ? TickIndex < attack.launchTick : TickIndex + 1 < attack.landTick) continue;
                double armed = cake ? 0 : Math.Max(0, attack.landTick - TickIndex);
                int count = 0; double earliest = double.PositiveInfinity;
                for (int i = 0; i < 5; i++)
                {
                    CompetitionParticipant p = participants[i]; if (!CanHit(p, attack.owner, !cake)) continue;
                    double contact = AttackContact(attack, i, .25, armed, cake);
                    if (double.IsPositiveInfinity(contact) || !BeforeDeadline(contact)) continue;
                    if (contact < earliest - 1e-9) { earliest = contact; count = 0; }
                    if (Math.Abs(contact - earliest) <= 1e-9) tied[count++] = i;
                }
                if (count == 0) continue;
                int hit = tied[count == 1 ? 0 : tieRandom.Range(count)];
                if (cake) KnockOut(hit, attack.owner);
                else
                {
                    CompetitionParticipant p = participants[hit];
                    if (attack.kind == CompetitionItem.Banana) p.slipUntil = TickIndex + rules.slipTicks;
                    else p.oilUntil = TickIndex + rules.oilTicks;
                    p.trapImmunityUntil = TickIndex + 180; Events.Add(new CompetitionEvent(CompetitionEventKind.Slip, p.id, attack.owner));
                }
                attacks.RemoveAt(shot);
            }
        }
        private bool CanHit(CompetitionParticipant p, string owner, bool nonlethal) => p.id != owner && p.life == CompetitionLife.Active &&
            p.protectedUntil <= TickIndex && (!nonlethal || p.trapImmunityUntil <= TickIndex);
        private void KnockOut(int index, string owner)
        {
            CompetitionParticipant p = participants[index]; if (p.life != CompetitionLife.Active) return;
            p.life = CompetitionLife.KnockedOut; p.knockoutAt = TickIndex; p.knockoutUntil = TickIndex + rules.knockoutTicks;
            statistics.knockouts++; statistics.knockoutsByActor[index]++;
            Events.Add(new CompetitionEvent(CompetitionEventKind.Knockout, p.id, owner));
        }
        private bool AttackReserves(Cell cell)
        {
            foreach (CompetitionAttack attack in attacks)
            {
                if (attack.expiresTick <= TickIndex) continue;
                bool airborne = TickIndex < attack.landTick;
                Point2 start = airborne ? attack.Position(Maze, TickIndex) : attack.target;
                Point2 end = airborne ? Maze.Near(attack.target, start) : start, wall = Maze.Near(cell.Position, start);
                if (!double.IsPositiveInfinity(Geometry.ContactTime(start, end, wall, wall, .72))) return true;
            }
            return false;
        }
    }
}
