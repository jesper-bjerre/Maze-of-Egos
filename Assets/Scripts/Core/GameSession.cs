using System;
using System.Collections.Generic;

namespace MazeOfEgos.Core
{
    public sealed class GameSession
    {
        public const double FixedDelta = 1.0 / 60;
        public const double BananaFlightDuration = 0.36;
        public const double TrapRadius = 0.18;
        public readonly MazeMap Maze;
        public RuleProfile Rules { get; private set; }
        public SessionPhase Phase { get; private set; }
        public Point2 Player { get; private set; }
        public double Yaw { get; set; }
        public double Pitch { get; set; }
        public double Time { get; private set; }
        public double LevelTime { get; private set; }
        public double PhaseRemaining { get; private set; }
        public long Score { get; private set; }
        public int Level { get; private set; }
        public int Chances { get; private set; }
        public int RemainingHearts { get; private set; }
        public double PowerUntil { get; private set; }
        public double BoostUntil { get; private set; }
        public int PowerId { get; private set; }
        public ItemKind Inventory { get; private set; }
        public bool[] Collected { get; private set; }
        public bool[] ItemAvailable { get; private set; }
        public PursuerState[] Pursuers { get; private set; }
        public WallState[] Walls { get; private set; }
        public readonly List<TrapState> Traps = new List<TrapState>();
        public readonly List<GameEvent> Events = new List<GameEvent>();
        private readonly List<Point2> trail = new List<Point2>();
        private readonly List<Contact> contacts = new List<Contact>(64);
        private readonly Point2[] oldPursuerPositions = new Point2[4];
        private readonly Cell[] neighbors = new Cell[4];
        private RandomState aiRandom, itemRandom, wallRandom;
        private SessionPhase resumePhase;
        private double savedPhaseRemaining, nextWallAttempt;
        private int powerChain, nextTrapId;
        private bool extraChanceGiven, scatter;
        private Point2 moveInput;
        private static readonly Point2[] ScatterTargets = { new Point2(26, 1), new Point2(1, 1), new Point2(26, 29), new Point2(1, 29) };
        private struct Contact { public double time; public int kind, index, other; }
        private sealed class ContactComparer : IComparer<Contact>
        {
            public static readonly ContactComparer Instance = new ContactComparer();
            public int Compare(Contact a, Contact b)
            {
                int c = a.time.CompareTo(b.time);
                if (c != 0) return c;
                c = a.kind.CompareTo(b.kind); if (c != 0) return c;
                c = a.index.CompareTo(b.index); return c != 0 ? c : a.other.CompareTo(b.other);
            }
        }

        public GameSession(MazeMap maze, uint seed = 1, RuleProfile rules = null, bool countdown = true)
        {
            Maze = maze; Rules = rules?.Copy() ?? new RuleProfile();
            aiRandom = new RandomState(seed ^ 0xA53A9E17u);
            itemRandom = new RandomState(seed ^ 0x3C6EF372u);
            wallRandom = new RandomState(seed ^ 0xBB67AE85u);
            Level = 1; Chances = 3;
            Collected = new bool[Maze.Hearts.Count]; ItemAvailable = new bool[Maze.Definition.itemCells.Length];
            Walls = new WallState[Maze.OpenShortcuts.Length];
            for (int i = 0; i < Walls.Length; i++) Walls[i] = new WallState();
            ResetLevel(); Phase = countdown ? SessionPhase.Countdown : SessionPhase.Playing;
            PhaseRemaining = countdown ? 3 : 0;
        }

        public bool IsFrightened(PursuerState pursuer) => pursuer.life == PursuerLife.Active && Time < PowerUntil && pursuer.consumedPowerId != PowerId;
        public bool IsDangerous(PursuerState pursuer) => pursuer.life == PursuerLife.Active && !IsFrightened(pursuer);
        public double PursuerSpeed(PursuerState p)
        {
            if (p.life == PursuerLife.InHouse || p.life == PursuerLife.Recovering || Time < p.stunUntil) return 0;
            double speed = p.life == PursuerLife.Returning ? 4.0 : IsFrightened(p) ? LevelBalance.FrightenedSpeed(Level) : LevelBalance.PursuerSpeed(Level);
            Point2 position = p.Position(Maze);
            if (p.life != PursuerLife.Returning && Math.Abs(position.y - Maze.Definition.horizontalTunnelRow) < 0.1 &&
                (position.x < 6 || position.x > Maze.Definition.width - 7)) speed *= 0.5;
            if (Time < p.oilUntil) speed *= 0.55;
            return speed;
        }
        public double PlayerSpeed
        {
            get
            {
                double normal = LevelBalance.PlayerSpeed(Level);
                if (Time >= BoostUntil) return normal;
                double fastest = 0;
                foreach (PursuerState p in Pursuers) if (IsDangerous(p)) fastest = Math.Max(fastest, PursuerSpeed(p));
                return Math.Max(normal * 1.6, fastest * 1.15);
            }
        }
        public void SetMovement(Point2 input)
        {
            moveInput = input.IsFinite ? (input.LengthSquared > 1 ? input.Normalized : input) : new Point2();
        }
        public void Pause()
        {
            if (Phase == SessionPhase.Paused || Phase == SessionPhase.GameOver) return;
            resumePhase = Phase; savedPhaseRemaining = PhaseRemaining;
            Phase = SessionPhase.Paused; moveInput = new Point2();
        }
        public void Resume(double countdown = 1.5)
        {
            if (Phase != SessionPhase.Paused) return;
            if (resumePhase != SessionPhase.Playing) { Phase = resumePhase; PhaseRemaining = savedPhaseRemaining; return; }
            Phase = countdown > 0 ? SessionPhase.Countdown : SessionPhase.Playing;
            PhaseRemaining = countdown;
        }

        public void Tick(double delta = FixedDelta)
        {
            if (double.IsNaN(delta) || delta <= 0 || delta > FixedDelta + 1e-8) throw new ArgumentOutOfRangeException(nameof(delta), "Use fixed simulation ticks.");
            Events.Clear();
            if (Phase == SessionPhase.Paused || Phase == SessionPhase.GameOver) return;
            if (Phase != SessionPhase.Playing)
            {
                PhaseRemaining = Math.Max(0, PhaseRemaining - delta);
                if (PhaseRemaining > 1e-8) return;
                PhaseRemaining = 0;
                if (Phase == SessionPhase.Countdown) Phase = SessionPhase.Playing;
                else if (Phase == SessionPhase.Caught)
                {
                    if (Chances == 0) Phase = SessionPhase.GameOver;
                    else if (RemainingHearts == 0) BeginLevelClear();
                    else { ResetActors(); Phase = SessionPhase.Countdown; PhaseRemaining = 2; }
                }
                else if (Phase == SessionPhase.LevelClear)
                {
                    if (Level < int.MaxValue) Level++;
                    ResetLevel(); Phase = SessionPhase.Countdown; PhaseRemaining = 2;
                    Events.Add(new GameEvent(EventKind.NewLevel));
                }
                return;
            }
            // Microsteps bound collision travel for both actors. All event contacts are swept.
            int slices = Math.Max(1, (int)Math.Ceiling(Math.Max(4.0, PlayerSpeed) * delta / 0.025));
            double sliceDelta = delta / slices;
            for (int slice = 0; slice < slices && Phase == SessionPhase.Playing; slice++)
            {
                Time += sliceDelta; LevelTime += sliceDelta;
                bool nextScatter = LevelTime < 7 || (LevelTime >= 27 && LevelTime < 34);
                if (nextScatter != scatter)
                {
                    scatter = nextScatter;
                    foreach (PursuerState p in Pursuers) if (p.life == PursuerLife.Active && !IsFrightened(p)) Reverse(p);
                }
                UpdateWalls();
                for (int i = Traps.Count - 1; i >= 0; i--) if (Time >= Traps[i].expiresAt) Traps.RemoveAt(i);
                SimulateMovement(sliceDelta);
            }
        }

        private void ResetLevel()
        {
            Array.Clear(Collected, 0, Collected.Length);
            for (int i = 0; i < ItemAvailable.Length; i++) ItemAvailable[i] = true;
            RemainingHearts = Collected.Length;
            ResetActors();
        }
        private void ResetActors()
        {
            Player = new Point2(Maze.Definition.playerStart.x, Maze.Definition.playerStart.y);
            Yaw = 270; Pitch = 0; moveInput = new Point2();
            PowerUntil = BoostUntil = Time; Inventory = ItemKind.None; powerChain = 0;
            LevelTime = 0; scatter = true; Traps.Clear(); trail.Clear(); trail.Add(Player);
            nextWallAttempt = Time + 15;
            for (int i = 0; i < Walls.Length; i++) { Walls[i] = new WallState(); Maze.SetShortcut(i, false); }
            Pursuers = new PursuerState[4];
            string[] ids = { "putin", "xi", "kim", "trump" };
            for (int i = 0; i < Pursuers.Length; i++)
            {
                Cell home = new Cell(12 + i, 14);
                Pursuers[i] = new PursuerState { id = ids[i], role = (PursuerRole)i,
                    life = i == 0 ? PursuerLife.Active : PursuerLife.InHouse,
                    from = i == 0 ? new Cell(14, 11) : home,
                    to = i == 0 ? new Cell(13, 11) : home,
                    previous = i == 0 ? new Cell(15, 11) : home,
                    progress = i == 0 ? 0.5 : 0,
                    releaseAt = Time + (i == 1 ? 2 : i == 2 ? 5 : 8), consumedPowerId = PowerId };
            }
        }
        private void SimulateMovement(double delta)
        {
            double endTime = Time;
            Point2 oldPlayer = Player;
            Point2 worldInput = Point2.Right(Yaw) * moveInput.x + Point2.Forward(Yaw) * moveInput.y;
            Player = Maze.MovePlayer(Player, worldInput * (PlayerSpeed * delta));
            Point2 endPlayer = Maze.Near(Player, oldPlayer);
            if ((Maze.Near(Player, trail[trail.Count - 1]) - trail[trail.Count - 1]).Length > 0.04)
            {
                trail.Add(Player); if (trail.Count > 128) trail.RemoveAt(0);
            }
            for (int i = 0; i < Pursuers.Length; i++)
            {
                oldPursuerPositions[i] = Pursuers[i].Position(Maze);
                MovePursuer(Pursuers[i], delta);
            }
            contacts.Clear();
            for (int i = 0; i < Collected.Length; i++)
                if (!Collected[i]) AddContact(oldPlayer, endPlayer, Maze.Near(Maze.Hearts[i].Position, oldPlayer), 0.30, 0, i);
            if (Rules.itemsEnabled && Inventory == ItemKind.None)
                for (int i = 0; i < ItemAvailable.Length; i++)
                    if (ItemAvailable[i]) AddContact(oldPlayer, endPlayer, Maze.Near(MazeMap.FromPair(Maze.Definition.itemCells[i]).Position, oldPlayer), 0.30, 1, i);
            for (int i = 0; i < Pursuers.Length; i++)
            {
                PursuerState p = Pursuers[i];
                if (p.life != PursuerLife.Active) continue;
                Point2 oldGhost = Maze.Near(oldPursuerPositions[i], oldPlayer);
                Point2 endGhost = Maze.Near(p.Position(Maze), oldGhost);
                double t = Geometry.ContactTime(oldPlayer, endPlayer, oldGhost, endGhost, MazeMap.PlayerRadius + MazeMap.PursuerRadius);
                if (!double.IsPositiveInfinity(t)) contacts.Add(new Contact { time = t, kind = 3, index = i });
                if (Time >= p.slipImmunityUntil)
                    for (int j = 0; j < Traps.Count; j++)
                    {
                        TrapState trapState = Traps[j];
                        if (trapState.landsAt > endTime) continue;
                        // Sweep only the part of this slice after landing, including an enemy
                        // already overlapping the landing point at that exact instant.
                        double armedFraction = Geometry.Clamp((trapState.landsAt - (endTime - delta)) / delta, 0, 1);
                        Point2 trap = Maze.Near(trapState.position, oldGhost);
                        t = Geometry.ContactTime(Point2.Lerp(oldGhost, endGhost, armedFraction), endGhost, trap, trap, 0.30);
                        if (!double.IsPositiveInfinity(t)) contacts.Add(new Contact {
                            time = armedFraction + t * (1 - armedFraction), kind = 2, index = i, other = trapState.id });
                    }
            }
            contacts.Sort(ContactComparer.Instance);
            int cursor = 0;
            while (cursor < contacts.Count && Phase == SessionPhase.Playing)
            {
                double moment = contacts[cursor].time;
                int end = cursor + 1;
                while (end < contacts.Count && Math.Abs(contacts[end].time - moment) < 1e-8) end++;
                Time = endTime - delta + moment * delta;
                int capture = -1;
                // Resolve every simultaneous contact before applying a single capture.
                for (int kind = 0; kind <= 3; kind++)
                    for (int k = cursor; k < end && Phase == SessionPhase.Playing; k++)
                    {
                        Contact contact = contacts[k]; if (contact.kind != kind) continue;
                        if (kind == 0) CollectHeart(contact.index);
                        else if (kind == 1) PickItem(contact.index);
                        else if (kind == 2) ApplyTrap(contact.index, contact.other);
                        else if (ContactPursuer(contact.index) && capture < 0) capture = contact.index;
                    }
                if (capture >= 0)
                {
                    Chances--; Phase = SessionPhase.Caught; PhaseRemaining = 1.2;
                    moveInput = new Point2(); Events.Add(new GameEvent(EventKind.Caught, capture));
                }
                if (Phase == SessionPhase.Playing && RemainingHearts == 0) BeginLevelClear();
                cursor = end;
            }
            Time = endTime;
        }
        private void AddContact(Point2 start, Point2 end, Point2 target, double radius, int kind, int index)
        {
            double time = Geometry.ContactTime(start, end, target, target, radius);
            if (!double.IsPositiveInfinity(time)) contacts.Add(new Contact { time = time, kind = kind, index = index });
        }
        private void CollectHeart(int index)
        {
            if (Collected[index]) return;
            Collected[index] = true; RemainingHearts--;
            bool power = Maze.PowerHearts[index]; AddScore(power ? 50 : 10);
            if (power)
            {
                PowerId++; powerChain = 0; PowerUntil = Time + LevelBalance.PowerDuration(Level);
                foreach (PursuerState p in Pursuers) if (p.life == PursuerLife.Active) Reverse(p);
            }
            Events.Add(new GameEvent(power ? EventKind.PowerHeart : EventKind.Heart, index, power ? 50 : 10));
        }
        private bool ContactPursuer(int index)
        {
            PursuerState p = Pursuers[index];
            if (p.life != PursuerLife.Active) return false;
            if (IsFrightened(p))
            {
                long points = 200L << Math.Min(powerChain++, 3); AddScore(points);
                p.consumedPowerId = PowerId; p.life = PursuerLife.Returning;
                p.stunUntil = p.oilUntil = Time;
                Events.Add(new GameEvent(EventKind.PursuerPaused, index, points));
                return false;
            }
            return true;
        }
        private void AddScore(long amount)
        {
            Score += amount;
            if (!extraChanceGiven && Score >= 10000)
            {
                Chances++; extraChanceGiven = true;
                Events.Add(new GameEvent(EventKind.ExtraChance));
            }
        }
        private void BeginLevelClear()
        {
            if (Phase == SessionPhase.LevelClear || Phase == SessionPhase.GameOver) return;
            long bonus = Rules.levelClearBonusEnabled ? 1000L * Level : 0;
            AddScore(bonus); Phase = SessionPhase.LevelClear; PhaseRemaining = 2;
            moveInput = new Point2(); Events.Add(new GameEvent(EventKind.LevelClear, points: bonus));
        }

        private void Reverse(PursuerState p)
        {
            if (p.from == p.to) return;
            Cell swap = p.from; p.from = p.to; p.to = swap; p.progress = 1 - p.progress;
            p.previous = p.from;
        }
        private void MovePursuer(PursuerState p, double delta)
        {
            if (p.life == PursuerLife.InHouse && Time >= p.releaseAt) p.life = PursuerLife.Exiting;
            if (p.life == PursuerLife.Recovering && Time >= p.recoverUntil) p.life = PursuerLife.Exiting;
            if (p.life == PursuerLife.InHouse || p.life == PursuerLife.Recovering) return;
            double remaining = PursuerSpeed(p) * delta;
            int safety = 0;
            while (remaining > 1e-10 && ++safety < 32)
            {
                if (p.from == p.to)
                {
                    if (p.life == PursuerLife.Returning && p.from == new Cell(13, 14))
                    {
                        p.life = PursuerLife.Recovering; p.recoverUntil = Time + 2; return;
                    }
                    if (p.life == PursuerLife.Exiting && p.from == new Cell(13, 11)) p.life = PursuerLife.Active;
                    p.to = ChooseNext(p);
                    if (p.from == p.to) return;
                }
                double advance = Math.Min(remaining, 1 - p.progress);
                p.progress += advance; remaining -= advance;
                if (p.progress >= 1 - 1e-9)
                {
                    p.previous = p.from; p.from = p.to; p.progress = 0;
                }
            }
        }
        private Cell ChooseNext(PursuerState p)
        {
            if (p.life == PursuerLife.Returning) return Maze.PathStep(p.from, new Cell(13, 14), true);
            if (p.life == PursuerLife.Exiting) return Maze.PathStep(p.from, new Cell(13, 11), true);
            int count = Maze.Neighbors(p.from, false, neighbors);
            int allowed = 0;
            for (int i = 0; i < count; i++) if (neighbors[i] != p.previous) neighbors[allowed++] = neighbors[i];
            if (allowed == 0) return Maze.IsWalkable(p.previous) ? p.previous : p.from;
            if (IsFrightened(p)) return neighbors[aiRandom.Range(allowed)];
            Point2 target = Target(p); Cell best = neighbors[0]; double bestDistance = double.PositiveInfinity;
            for (int i = 0; i < allowed; i++)
            {
                double distance = (neighbors[i].Position - target).LengthSquared;
                if (distance < bestDistance - 1e-10) { best = neighbors[i]; bestDistance = distance; }
            }
            return best;
        }
        public Point2 Target(PursuerState p)
        {
            if (scatter) return ScatterTargets[(int)p.role];
            Point2 player = Maze.NearestPlayerCell(Player).Position;
            Point2 forward = Point2.Forward(Yaw);
            if (moveInput.LengthSquared > 0.01)
                forward = (Point2.Right(Yaw) * moveInput.x + forward * moveInput.y).Normalized;
            // Targets use the dominant movement axis, preserving the four classic roles.
            forward = Math.Abs(forward.x) > Math.Abs(forward.y) ? new Point2(Math.Sign(forward.x), 0) : new Point2(0, Math.Sign(forward.y));
            if (p.role == PursuerRole.Chaser) return player;
            if (p.role == PursuerRole.Ambusher) return player + forward * 4;
            if (p.role == PursuerRole.Flanker) return (player + forward * 2) * 2 - Pursuers[0].Position(Maze);
            return (p.Position(Maze) - player).Length > 8 ? player : ScatterTargets[3];
        }

        private void PickItem(int index)
        {
            if (!Rules.itemsEnabled || Inventory != ItemKind.None || !ItemAvailable[index]) return;
            int roll = itemRandom.Range(100);
            Inventory = roll < 35 ? ItemKind.BananaPeel : roll < 70 ? ItemKind.OilSlick : ItemKind.SpeedBoost;
            ItemAvailable[index] = false; Events.Add(new GameEvent(EventKind.ItemPicked));
        }
        public bool UseItem()
        {
            if (Phase != SessionPhase.Playing || Inventory == ItemKind.None) return false;
            ItemKind kind = Inventory;
            if (kind == ItemKind.SpeedBoost) BoostUntil = Time + 4;
            else
            {
                Point2 location = Player;
                if (kind == ItemKind.BananaPeel)
                {
                    Point2 direction = Point2.Forward(Yaw);
                    for (double distance = 0.1; distance <= 2 + 1e-8; distance += 0.1)
                    {
                        Point2 candidate = Maze.Wrap(Player + direction * distance);
                        if (!Maze.CanOccupy(candidate, TrapRadius)) break;
                        location = candidate;
                    }
                    if ((Maze.Near(location, Player) - Player).Length < 0.25) return false;
                }
                else
                {
                    double traveled = 0; Point2 previous = Player; bool found = false;
                    for (int i = trail.Count - 1; i >= 0; i--)
                    {
                        Point2 next = Maze.Near(trail[i], previous); double length = (next - previous).Length;
                        if (traveled + length >= 0.5)
                        {
                            location = Maze.Wrap(Point2.Lerp(previous, next, (0.5 - traveled) / Math.Max(length, 1e-9)));
                            found = true; break;
                        }
                        traveled += length; previous = next;
                    }
                    if (!found || !Maze.CanOccupy(location, TrapRadius)) return false;
                }
                double landing = kind == ItemKind.BananaPeel ? Time + BananaFlightDuration : Time;
                Traps.Add(new TrapState { id = ++nextTrapId, kind = kind, position = location,
                    launchPosition = kind == ItemKind.BananaPeel ? Player : location,
                    launchedAt = kind == ItemKind.BananaPeel ? Time : 0, landsAt = kind == ItemKind.BananaPeel ? landing : 0,
                    expiresAt = landing + (kind == ItemKind.BananaPeel ? 20 : 15) });
            }
            Inventory = ItemKind.None; Events.Add(new GameEvent(EventKind.ItemUsed)); return true;
        }
        private void ApplyTrap(int pursuerIndex, int trapId)
        {
            PursuerState p = Pursuers[pursuerIndex];
            if (p.life != PursuerLife.Active || Time < p.slipImmunityUntil) return;
            for (int i = 0; i < Traps.Count; i++) if (Traps[i].id == trapId)
            {
                if (Time < Traps[i].landsAt || Time >= Traps[i].expiresAt) return;
                if (Traps[i].kind == ItemKind.BananaPeel) p.stunUntil = Time + 1.2;
                else p.oilUntil = Time + 2.5;
                p.slipImmunityUntil = Time + 3; Traps.RemoveAt(i);
                Events.Add(new GameEvent(EventKind.Slip, pursuerIndex)); return;
            }
        }

        private void UpdateWalls()
        {
            if (!Rules.movingWallsEnabled) return;
            for (int i = 0; i < Walls.Length; i++)
            {
                WallState wall = Walls[i];
                if (wall.phase == WallPhase.Closed || Time < wall.phaseUntil) continue;
                if (wall.phase == WallPhase.Warning) { wall.phase = WallPhase.Opening; wall.phaseUntil = Time + 1.25; }
                else if (wall.phase == WallPhase.Opening)
                {
                    wall.phase = WallPhase.Open; wall.phaseUntil = Time + 5; Maze.SetShortcut(i, true);
                    Events.Add(new GameEvent(EventKind.WallOpened, i));
                }
                else if (wall.phase == WallPhase.Open)
                {
                    if (PassageReserved(i)) continue;
                    Maze.SetShortcut(i, false); wall.phase = WallPhase.Closing; wall.phaseUntil = Time + 1.25;
                }
                else if (wall.phase == WallPhase.Closing)
                {
                    wall.phase = WallPhase.Closed; wall.cooldownUntil = Time + 15;
                    Events.Add(new GameEvent(EventKind.WallClosed, i));
                }
            }
            if (Time < nextWallAttempt) return;
            nextWallAttempt = Time + 12 + wallRandom.Unit() * 6;
            foreach (WallState wall in Walls) if (wall.phase != WallPhase.Closed) return;
            int start = wallRandom.Range(Walls.Length);
            for (int j = 0; j < Walls.Length; j++)
            {
                int i = (start + j) % Walls.Length;
                if (Time < Walls[i].cooldownUntil) continue;
                Walls[i].phase = WallPhase.Warning; Walls[i].phaseUntil = Time + 1.5;
                Events.Add(new GameEvent(EventKind.WallWarning, i)); break;
            }
        }
        public bool PassageReserved(int index)
        {
            foreach (int[] pair in Maze.Definition.shortcutCandidates[index].wallCells)
            {
                Cell c = MazeMap.FromPair(pair);
                foreach (TrapState trap in Traps)
                {
                    if (Time >= trap.expiresAt) continue;
                    Point2 start = trap.PositionAt(Maze, Time), end = Maze.Near(trap.position, start);
                    int steps = Math.Max(1, (int)Math.Ceiling((end - start).Length / 0.05));
                    for (int step = 0; step <= steps; step++)
                    {
                        Point2 point = Point2.Lerp(start, end, (double)step / steps);
                        if (Math.Abs(point.x - c.x) < 0.5 + MazeMap.WallPadding + TrapRadius &&
                            Math.Abs(point.y - c.y) < 0.5 + MazeMap.WallPadding + TrapRadius) return true;
                    }
                }
                if (Math.Abs(Player.x - c.x) < 0.5 + MazeMap.PlayerRadius + 0.2 && Math.Abs(Player.y - c.y) < 0.5 + MazeMap.PlayerRadius + 0.2) return true;
                foreach (PursuerState p in Pursuers)
                {
                    Point2 position = p.Position(Maze);
                    if (p.to == c || (Math.Abs(position.x - c.x) < 0.5 + MazeMap.PursuerRadius + 0.2 &&
                        Math.Abs(position.y - c.y) < 0.5 + MazeMap.PursuerRadius + 0.2)) return true;
                }
            }
            return false;
        }

        public SessionSnapshot Snapshot()
        {
            var state = new SessionSnapshot {
                mazeId = Maze.Definition.id, mazeFingerprint = Maze.Fingerprint, rules = Rules.Copy(),
                phase = Phase, resumePhase = Phase == SessionPhase.Paused ? resumePhase : Phase, time = Time, levelTime = LevelTime,
                phaseRemaining = PhaseRemaining, savedPhaseRemaining = Phase == SessionPhase.Paused ? savedPhaseRemaining : PhaseRemaining,
                powerUntil = PowerUntil, boostUntil = BoostUntil, nextWallAttempt = nextWallAttempt,
                yaw = Yaw, pitch = Pitch, player = Player, level = Level, chances = Chances, remainingHearts = RemainingHearts,
                powerId = PowerId, powerChain = powerChain, nextTrapId = nextTrapId, score = Score,
                extraChanceGiven = extraChanceGiven, scatter = scatter,
                itemRandom = itemRandom.state, aiRandom = aiRandom.state, wallRandom = wallRandom.state, inventory = Inventory,
                collected = (bool[])Collected.Clone(), itemAvailable = (bool[])ItemAvailable.Clone(),
                pursuers = new PursuerState[4], walls = new WallState[Walls.Length], traps = new TrapState[Traps.Count], trail = trail.ToArray() };
            for (int i = 0; i < 4; i++) state.pursuers[i] = Pursuers[i].Copy();
            for (int i = 0; i < Walls.Length; i++) state.walls[i] = Walls[i].Copy();
            for (int i = 0; i < Traps.Count; i++) state.traps[i] = Traps[i].Copy();
            return state;
        }
        public void Restore(SessionSnapshot state)
        {
            if (state == null || (state.version != 1 && state.version != 2) || state.mazeId != Maze.Definition.id || state.mazeFingerprint != Maze.Fingerprint ||
                state.rules == null || state.collected == null || state.collected.Length != Collected.Length ||
                state.itemAvailable == null || state.itemAvailable.Length != ItemAvailable.Length ||
                state.pursuers == null || state.pursuers.Length != 4 || state.walls == null || state.walls.Length != Walls.Length ||
                state.traps == null || state.traps.Length > 4 || state.trail == null || state.trail.Length == 0 || state.trail.Length > 128 ||
                state.level < 1 || state.chances < 0 || state.chances > 4 || state.score < 0 ||
                !WithinMazeFrame(state.player) || !FiniteNonnegative(state.time) || !FiniteNonnegative(state.levelTime) ||
                !FiniteNonnegative(state.phaseRemaining) || !FiniteNonnegative(state.savedPhaseRemaining) ||
                !FiniteNonnegative(state.powerUntil) || !FiniteNonnegative(state.boostUntil) || !FiniteNonnegative(state.nextWallAttempt) ||
                double.IsNaN(state.yaw) || double.IsInfinity(state.yaw) || Math.Abs(state.pitch) > 20 ||
                double.IsNaN(state.pitch) || state.itemRandom == 0 || state.aiRandom == 0 || state.wallRandom == 0 ||
                !Enum.IsDefined(typeof(SessionPhase), state.phase) || !Enum.IsDefined(typeof(SessionPhase), state.resumePhase) ||
                !Enum.IsDefined(typeof(ItemKind), state.inventory)) throw new ArgumentException("Snapshot is incompatible or corrupt.");
            int remaining = 0; foreach (bool c in state.collected) if (!c) remaining++;
            if (remaining != state.remainingHearts) throw new ArgumentException("Snapshot heart count is inconsistent.");
            string[] expectedIds = { "putin", "xi", "kim", "trump" };
            for (int i = 0; i < state.pursuers.Length; i++)
            {
                PursuerState p = state.pursuers[i];
                if (p == null || p.id != expectedIds[i] || p.role != (PursuerRole)i ||
                    !Enum.IsDefined(typeof(PursuerLife), p.life) || !Maze.InBounds(p.from) || !Maze.InBounds(p.to) ||
                    !FiniteNonnegative(p.progress) || p.progress > 1 ||
                    !FiniteNonnegative(p.releaseAt) || !FiniteNonnegative(p.recoverUntil) || !FiniteNonnegative(p.stunUntil) ||
                    !FiniteNonnegative(p.oilUntil) || !FiniteNonnegative(p.slipImmunityUntil))
                    throw new ArgumentException("Snapshot pursuer data is invalid.");
            }
            for (int i = 0; i < state.walls.Length; i++)
                if (state.walls[i] == null || !Enum.IsDefined(typeof(WallPhase), state.walls[i].phase) ||
                    !FiniteNonnegative(state.walls[i].phaseUntil) || !FiniteNonnegative(state.walls[i].cooldownUntil))
                    throw new ArgumentException("Snapshot wall data is invalid.");
            foreach (TrapState trap in state.traps)
                if (trap == null || !WithinMazeFrame(trap.position) || !FiniteNonnegative(trap.expiresAt) || trap.id < 1 || trap.id > state.nextTrapId ||
                    (trap.kind != ItemKind.BananaPeel && trap.kind != ItemKind.OilSlick)) throw new ArgumentException("Invalid trap.");
            if (state.version == 2) foreach (TrapState trap in state.traps)
                if (!WithinMazeFrame(trap.launchPosition) || !FiniteNonnegative(trap.launchedAt) || !FiniteNonnegative(trap.landsAt) ||
                    (trap.landsAt == 0 && trap.launchedAt != 0) ||
                    (trap.landsAt > 0 && (trap.kind != ItemKind.BananaPeel || trap.launchedAt > state.time ||
                        Math.Abs(trap.landsAt - trap.launchedAt - BananaFlightDuration) > 1e-8 || trap.expiresAt < trap.landsAt ||
                        (Maze.Near(trap.position, trap.launchPosition) - trap.launchPosition).Length > 2 + 1e-8)))
                    throw new ArgumentException("Invalid banana flight.");
            foreach (Point2 point in state.trail) if (!WithinMazeFrame(point)) throw new ArgumentException("Invalid movement trail.");
            // Validate topology on a temporary map so rejected data cannot mutate a running game.
            var restoredMap = new MazeMap(Maze.Definition);
            for (int i = 0; i < state.walls.Length; i++) restoredMap.SetShortcut(i, state.walls[i].phase == WallPhase.Open);
            if (!restoredMap.CanOccupy(state.player)) throw new ArgumentException("Snapshot player is inside a wall.");
            if (state.version == 2) foreach (TrapState trap in state.traps)
                if (trap.landsAt > state.time)
                {
                    Point2 start = trap.PositionAt(restoredMap, state.time), end = restoredMap.Near(trap.position, start);
                    int steps = Math.Max(1, (int)Math.Ceiling((end - start).Length / 0.05));
                    for (int step = 0; step <= steps; step++)
                        if (!restoredMap.CanOccupy(Point2.Lerp(start, end, (double)step / steps), TrapRadius))
                            throw new ArgumentException("Snapshot banana flight intersects a wall.");
                }
            foreach (PursuerState pursuer in state.pursuers)
            {
                bool house = pursuer.life != PursuerLife.Active;
                if (!restoredMap.IsWalkable(pursuer.from, house) || !restoredMap.IsWalkable(pursuer.to, house) ||
                    (pursuer.from != pursuer.to && Math.Abs((restoredMap.Near(pursuer.to.Position, pursuer.from.Position) - pursuer.from.Position).Length - 1) > 1e-8))
                    throw new ArgumentException("Snapshot pursuer is outside its navigation graph.");
            }
            for (int i = 0; i < state.walls.Length; i++) { Walls[i] = state.walls[i].Copy(); Maze.SetShortcut(i, Walls[i].phase == WallPhase.Open); }
            Rules = state.rules.Copy(); Time = state.time; LevelTime = state.levelTime; PhaseRemaining = state.phaseRemaining;
            PowerUntil = state.powerUntil; BoostUntil = state.boostUntil; nextWallAttempt = state.nextWallAttempt;
            Yaw = state.yaw; Pitch = state.pitch; Player = state.player; Level = state.level; Chances = state.chances;
            RemainingHearts = state.remainingHearts; PowerId = state.powerId; powerChain = state.powerChain;
            nextTrapId = state.nextTrapId; Score = state.score; extraChanceGiven = state.extraChanceGiven; scatter = state.scatter;
            itemRandom.state = state.itemRandom; aiRandom.state = state.aiRandom; wallRandom.state = state.wallRandom;
            Inventory = state.inventory; Collected = (bool[])state.collected.Clone(); ItemAvailable = (bool[])state.itemAvailable.Clone();
            for (int i = 0; i < 4; i++) Pursuers[i] = state.pursuers[i].Copy();
            Traps.Clear(); foreach (TrapState trap in state.traps)
            {
                TrapState copy = trap.Copy();
                if (state.version == 1) { copy.launchPosition = copy.position; copy.launchedAt = copy.landsAt = 0; }
                Traps.Add(copy);
            }
            trail.Clear(); trail.AddRange(state.trail); moveInput = new Point2();
            resumePhase = state.phase == SessionPhase.Paused ? state.resumePhase : state.phase;
            savedPhaseRemaining = state.phase == SessionPhase.Paused ? state.savedPhaseRemaining : state.phaseRemaining;
            Phase = SessionPhase.Paused; Events.Clear();
        }
        private static bool FiniteNonnegative(double value) => !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0;
        private bool WithinMazeFrame(Point2 point) => point.IsFinite && point.x >= -0.5 && point.x < Maze.Definition.width - 0.5 &&
            point.y >= 0 && point.y < Maze.Definition.height;
    }
}
