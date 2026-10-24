using System;
using System.Collections.Generic;

namespace MazeOfEgos.Core
{
    public sealed partial class CompetitionSession
    {
        public const double FixedDelta = 1.0 / 60, PickupRadius = .20;
        private static readonly string[] Ids = { "kim", "putin", "trump", "xi", "zelenskyy" };
        private static readonly Cell[] Starts = { new Cell(13, 14), new Cell(14, 14), new Cell(12, 14), new Cell(15, 14), new Cell(13, 15) };
        public static IReadOnlyList<string> ParticipantIds { get; } = Array.AsReadOnly(Ids);
        public MazeMap Maze { get; private set; }
        public string HumanId { get; private set; }
        public bool ComputerPlayers { get; private set; }
        public CompetitionPhase Phase { get; private set; }
        public long TickIndex { get; private set; }
        public int RoundNumber { get; private set; } = 1;
        public int RoundElapsedTicks { get; private set; }
        public long RoundStartedTick { get; private set; }
        public CompetitionRoundEnd RoundEnd { get; private set; }
        public double RoundSecondsRemaining => (rules.roundDurationTicks - RoundElapsedTicks) * FixedDelta;
        public int CountdownRemaining { get; private set; }
        public CompetitionRules Rules => rules.Copy();
        public IReadOnlyList<CompetitionParticipant> Participants => Array.AsReadOnly(CopyParticipants());
        public IReadOnlyList<CompetitionHeart> Hearts => Array.AsReadOnly(CopyHearts());
        public IReadOnlyList<CompetitionCrate> Crates => Array.AsReadOnly(CopyCrates());
        public IReadOnlyList<CompetitionWall> Walls => Array.AsReadOnly(CopyWalls());
        public IReadOnlyList<string> RoundWinners => Array.AsReadOnly(roundWinners);
        public IReadOnlyList<string> MatchWinners => Array.AsReadOnly(matchWinners);
        private CompetitionRules rules;
        private CompetitionParticipant[] participants;
        private readonly CorridorNavigation[] navigation = new CorridorNavigation[5];
        private readonly List<CorridorSegment>[] paths = new List<CorridorSegment>[5];
        private readonly List<CompetitionHeart> hearts = new List<CompetitionHeart>();
        private CompetitionCrate[] crates;
        private CompetitionWall[] walls;
        private CompetitionPhase resumePhase;
        private long nextHeartWaveTick, nextWallAttemptTick;
        private int settledRound, nextHeartId;
        private RandomState resourceRandom, itemRandom, tieRandom, wallRandom, spawnRandom;
        private string[] roundWinners = Array.Empty<string>(), matchWinners = Array.Empty<string>();
        private readonly List<Contact> contacts = new List<Contact>();
        private readonly HashSet<int> collected = new HashSet<int>();
        private readonly int[] tied = new int[5];
        private struct Contact { public double time; public int kind, index, participant; }

        public CompetitionSession(MazeDefinition definition, string humanId = "zelenskyy", uint seed = 42,
            CompetitionRules profile = null, bool countdown = true, bool computerPlayers = false)
        {
            IndexOf(humanId); HumanId = humanId; Maze = new MazeMap(definition);
            ComputerPlayers = computerPlayers;
            rules = (profile ?? new CompetitionRules()).Copy(); rules.Validate();
            resourceRandom = Stream(seed, 0x52A7u); itemRandom = Stream(seed, 0x17E4u);
            tieRandom = Stream(seed, 0x71E5u); wallRandom = Stream(seed, 0xA411u); spawnRandom = Stream(seed, 0x5A47u);
            participants = new CompetitionParticipant[5];
            for (int i = 0; i < 5; i++)
            {
                participants[i] = new CompetitionParticipant { id = Ids[i] };
                navigation[i] = new CorridorNavigation(); paths[i] = new List<CorridorSegment>(4);
            }
            ResetRound(countdown);
            InitializeBrains(seed);
        }
        private static RandomState Stream(uint seed, uint salt)
        {
            // Avalanche consecutive seeds before xorshift's first draw. Each
            // gameplay concern then advances only its own persisted stream.
            uint value = unchecked(seed + salt + 0x9e3779b9u);
            value = unchecked((value ^ (value >> 16)) * 0x85ebca6bu);
            value = unchecked((value ^ (value >> 13)) * 0xc2b2ae35u);
            return new RandomState(value ^ (value >> 16));
        }
        private static int IndexOf(string id)
        {
            int index = Array.IndexOf(Ids, id);
            if (index < 0) throw new ArgumentException("Unknown competition participant: " + id);
            return index;
        }
        public CompetitionParticipant Participant(string id) => participants[IndexOf(id)].Copy();
        public long TotalScore(string id)
        {
            CompetitionParticipant p = participants[IndexOf(id)];
            return p.bankedScore + (settledRound == RoundNumber ? 0 : p.roundScore);
        }
        public bool ApplyCommand(CompetitionCommand command)
        {
            int index = IndexOf(command.ParticipantId);
            if (command.Heading < 0 || command.Heading > 3) throw new ArgumentException("Invalid heading.");
            if (Phase != CompetitionPhase.Playing || command.Tick != TickIndex || participants[index].life != CompetitionLife.Active) return false;
            navigation[index].RequestHeading(command.Heading); SyncNavigation(index); return true;
        }
        private void SyncNavigation(int index)
        {
            participants[index].heading = navigation[index].Heading;
            participants[index].queuedHeading = navigation[index].QueuedHeading;
            participants[index].moving = navigation[index].Moving;
        }
        public void Pause()
        {
            if (Phase == CompetitionPhase.Paused) return;
            resumePhase = Phase; Phase = CompetitionPhase.Paused;
        }
        public void Resume(bool countdown = true)
        {
            if (Phase != CompetitionPhase.Paused) return;
            Phase = resumePhase;
            if (Phase == CompetitionPhase.Playing && countdown)
            { Phase = CompetitionPhase.Countdown; CountdownRemaining = 180; }
        }
        public bool StartNextRound(bool countdown = true)
        {
            if (Phase != CompetitionPhase.RoundResults) return false;
            RoundNumber++; ResetRound(countdown); return true;
        }
        private void ResetRound(bool countdown)
        {
            RoundStartedTick = TickIndex; RoundElapsedTicks = 0; RoundEnd = CompetitionRoundEnd.None; nextHeartId = 1; hearts.Clear();
            roundWinners = matchWinners = Array.Empty<string>();
            for (int i = 0; i < Maze.Hearts.Count && hearts.Count < rules.maximumHearts; i++)
                if (!Maze.PowerHearts[i]) hearts.Add(new CompetitionHeart { id = nextHeartId++, cell = Maze.Hearts[i] });
            crates = new CompetitionCrate[Maze.Definition.itemCells.Length];
            for (int i = 0; i < crates.Length; i++) crates[i] = new CompetitionCrate { cell = MazeMap.FromPair(Maze.Definition.itemCells[i]), availableAtTick = TickIndex };
            walls = new CompetitionWall[Maze.OpenShortcuts.Length];
            for (int i = 0; i < walls.Length; i++) { walls[i] = new CompetitionWall(); Maze.SetShortcut(i, false); }
            Cell[] starts = rules.randomRoundStarts ? RandomRoundStarts() : Starts;
            for (int i = 0; i < 5; i++)
            {
                Cell start = starts[(i + RoundNumber - 1) % 5];
                CompetitionParticipant p = participants[i]; p.position = start.Position;
                if (!Maze.IsWalkable(start, true)) throw new ArgumentException("Competition start is blocked.");
                p.roundScore = 0; p.inventory = CompetitionItem.None; p.insideCrates = new bool[crates.Length];
                int heading = 0;
                if (rules.randomRoundStarts) { var legal = new int[4]; int count = 0; for (int h = 0; h < 4; h++) if (Maze.IsWalkable(Maze.Canonical(new Cell(start.x + (int)CorridorNavigation.Direction(h).x, start.y + (int)CorridorNavigation.Direction(h).y)), true)) legal[count++] = h; heading = legal[spawnRandom.Range(count)]; }
                navigation[i].Restore(heading, -1, true); SyncNavigation(i);
            }
            nextHeartWaveTick = TickIndex + rules.heartWaveTicks; nextWallAttemptTick = TickIndex + 900;
            CountdownRemaining = countdown ? 180 : 0;
            Phase = countdown ? CompetitionPhase.Countdown : CompetitionPhase.Playing; resumePhase = Phase;
            ResetCombatRound(); ResetBrainRound();
        }
        private Cell[] RandomRoundStarts()
        {
            // Shuffle once, then rejection-sample spread-out cells. Every actor has
            // the same lottery, independent of human selection and item RNG.
            var candidates = new List<Cell>();
            foreach (Cell cell in Maze.PlayerCells)
            {
                bool crate = false;
                foreach (var box in crates) if (box.cell == cell) crate = true;
                if (!crate && !Maze.IsHome(cell) && (cell.Position - new Point2(13.5, 14)).Length >= 5) candidates.Add(cell);
            }
            for (int i = candidates.Count - 1; i > 0; i--) { int j = spawnRandom.Range(i + 1); Cell swap = candidates[i]; candidates[i] = candidates[j]; candidates[j] = swap; }
            var chosen = new List<Cell>();
            foreach (Cell cell in candidates)
            {
                bool separated = true;
                foreach (Cell other in chosen) if ((Maze.Near(cell.Position, other.Position) - other.Position).Length < 6) separated = false;
                if (separated) chosen.Add(cell);
                if (chosen.Count == 5) return chosen.ToArray();
            }
            throw new InvalidOperationException("Maze has too few separated round starts.");
        }
        // Competition ranking: shared places skip occupied ranks (1, 1, 3).
        public int RoundPlace(int index)
        {
            int place = 1; for (int i = 0; i < 5; i++) if (participants[i].roundScore > participants[index].roundScore) place++;
            return place;
        }
        public int OverallPlace(int index)
        {
            int place = 1; var actor = Actor(index);
            for (int i = 0; i < 5; i++) { var other = Actor(i); if (other.Trophies > actor.Trophies || (other.Trophies == actor.Trophies && other.TotalScore > actor.TotalScore)) place++; }
            return place;
        }
        public void Tick()
        {
            if (Phase == CompetitionPhase.Countdown)
            { if (--CountdownRemaining == 0) Phase = CompetitionPhase.Playing; return; }
            if (Phase != CompetitionPhase.Playing) return;
            BeforeCombatTick(); DriveComputers();
            UpdateWalls();
            for (int i = 0; i < 5; i++)
            {
                paths[i].Clear(); CompetitionParticipant p = participants[i];
                p.position = navigation[i].Advance(Maze, p.position, SpeedOf(p) * FixedDelta, true, paths[i]);
                double until = paths[i].Count == 0 ? 0 : paths[i][paths[i].Count - 1].Until;
                if (until < 1) paths[i].Add(new CorridorSegment(p.position, p.position, until, 1));
                SyncNavigation(i);
                RecordTrail(p);
            }
            contacts.Clear();
            for (int i = 0; i < hearts.Count; i++) AddContact(hearts[i].cell, 0, i);
            for (int i = 0; i < crates.Length; i++) if (crates[i].availableAtTick <= TickIndex) AddContact(crates[i].cell, 1, i);
            contacts.Sort((a, b) => { int c = a.time.CompareTo(b.time); if (c != 0) return c; c = a.kind.CompareTo(b.kind); return c != 0 ? c : a.index.CompareTo(b.index); });
            bool finalTick = RoundElapsedTicks + 1 == rules.roundDurationTicks;
            collected.Clear();
            foreach (Contact contact in contacts)
            {
                // A tolerance of 1e-9 of one tick treats numerical equality as the deadline.
                if (finalTick && contact.time >= 1 - 1e-9) continue;
                CompetitionParticipant p = participants[contact.participant];
                if (contact.kind == 0) { collected.Add(contact.index); p.roundScore++; statistics.hearts++; Events.Add(new CompetitionEvent(CompetitionEventKind.Heart, p.id)); }
                else
                {
                    p.inventory = (CompetitionItem)(1 + itemRandom.Range(5));
                    crates[contact.index].availableAtTick = TickIndex + rules.crateCooldownTicks + (contact.time > 1e-9 ? 1 : 0);
                    Events.Add(new CompetitionEvent(CompetitionEventKind.ItemPicked, p.id));
                }
            }
            for (int i = hearts.Count - 1; i >= 0; i--) if (collected.Contains(i)) hearts.RemoveAt(i);
            for (int i = 0; i < 5; i++) for (int j = 0; j < crates.Length; j++)
                participants[i].insideCrates[j] = Inside(participants[i].position, crates[j].cell);
            ResolveCombat(); TickIndex++; RoundElapsedTicks++;
            if (RoundElapsedTicks == rules.roundDurationTicks || (rules.endWhenHeartsCollected && hearts.Count == 0))
            { RoundEnd = RoundElapsedTicks == rules.roundDurationTicks ? CompetitionRoundEnd.TimeExpired : CompetitionRoundEnd.HeartsCollected; SettleRound(); return; }
            if (TickIndex >= nextHeartWaveTick) { if (rules.replenishHearts) SpawnHearts(); nextHeartWaveTick += rules.heartWaveTicks; }
        }
        private bool Inside(Point2 position, Cell cell) => (Maze.Near(cell.Position, position) - position).Length <= PickupRadius + 1e-9;
        private void AddContact(Cell cell, int kind, int resource)
        {
            double best = double.PositiveInfinity; int count = 0;
            for (int i = 0; i < 5; i++)
            {
                if (participants[i].life != CompetitionLife.Active) continue;
                if (kind == 1 && participants[i].insideCrates[resource]) continue;
                double earliest = double.PositiveInfinity;
                foreach (CorridorSegment segment in paths[i])
                {
                    Point2 target = Maze.Near(cell.Position, segment.Start);
                    double t = Geometry.ContactTime(segment.Start, segment.End, target, target, PickupRadius);
                    if (!double.IsPositiveInfinity(t)) earliest = Math.Min(earliest, segment.From + t * (segment.Until - segment.From));
                }
                if (double.IsPositiveInfinity(earliest)) continue;
                if (earliest < best - 1e-9) { best = earliest; count = 0; }
                if (Math.Abs(earliest - best) <= 1e-9) tied[count++] = i;
            }
            // Canonical stable IDs, then an independent seeded draw; never human/index priority.
            if (RoundElapsedTicks + 1 == rules.roundDurationTicks && best >= 1 - 1e-9) return;
            if (count > 0) contacts.Add(new Contact { time = best, kind = kind, index = resource,
                participant = tied[count == 1 ? 0 : tieRandom.Range(count)] });
        }
        private void SettleRound()
        {
            if (settledRound == RoundNumber) return;
            long maximum = 0; foreach (CompetitionParticipant p in participants) maximum = Math.Max(maximum, p.roundScore);
            var winners = new List<string>();
            foreach (CompetitionParticipant p in participants)
            {
                p.bankedScore += p.roundScore;
                if (p.roundScore == maximum) { p.trophies++; winners.Add(p.id); }
            }
            roundWinners = winners.ToArray(); settledRound = RoundNumber;
            matchWinners = WinningMatchIds(participants, rules.trophiesToWin);
            Phase = matchWinners.Length > 0 ? CompetitionPhase.MatchResults : CompetitionPhase.RoundResults;
            Events.Add(new CompetitionEvent(matchWinners.Length > 0 ? CompetitionEventKind.MatchEnded : CompetitionEventKind.RoundEnded, HumanId));
        }
        private static string[] WinningMatchIds(CompetitionParticipant[] players, int target)
        {
            long best = -1; var winners = new List<string>();
            foreach (CompetitionParticipant p in players)
            {
                if (p.trophies < target || p.bankedScore < best) continue;
                if (p.bankedScore > best) { best = p.bankedScore; winners.Clear(); }
                winners.Add(p.id);
            }
            return winners.ToArray();
        }
        private void SpawnHearts()
        {
            var available = new List<Cell>(); var occupied = new HashSet<Cell>();
            foreach (CompetitionHeart heart in hearts) occupied.Add(heart.cell);
            foreach (CompetitionCrate crate in crates) occupied.Add(crate.cell);
            foreach (ShortcutDefinition shortcut in Maze.Definition.shortcutCandidates)
                foreach (int[] endpoint in shortcut.endpoints) occupied.Add(MazeMap.FromPair(endpoint));
            foreach (Cell cell in Maze.PlayerCells)
            {
                if (occupied.Contains(cell) || Maze.IsHome(cell)) continue;
                bool nearPlayer = false;
                foreach (CompetitionParticipant p in participants) if ((Maze.Near(cell.Position, p.position) - p.position).Length < .6) nearPlayer = true;
                if (!nearPlayer) available.Add(cell);
            }
            var eligible = new HashSet<Cell>(available);
            var adjacent = new Cell[4];
            for (int group = 0; group < rules.groupsPerWave && available.Count > 0 && hearts.Count < rules.maximumHearts; group++)
            {
                Cell anchor = available[resourceRandom.Range(available.Count)];
                var queue = new Queue<Cell>(); var seen = new HashSet<Cell>(); queue.Enqueue(anchor); seen.Add(anchor);
                int added = 0;
                while (queue.Count > 0 && added < rules.heartsPerGroup && hearts.Count < rules.maximumHearts)
                {
                    Cell cell = queue.Dequeue(); if (!eligible.Remove(cell)) continue;
                    available.Remove(cell); hearts.Add(new CompetitionHeart { id = nextHeartId++, cell = cell }); added++;
                    int count = Maze.Neighbors(cell, false, adjacent);
                    for (int i = 0; i < count; i++) if (eligible.Contains(adjacent[i]) && seen.Add(adjacent[i])) queue.Enqueue(adjacent[i]);
                }
            }
        }
        public bool PassageReserved(int index)
        {
            foreach (int[] pair in Maze.Definition.shortcutCandidates[index].wallCells)
            {
                Cell cell = MazeMap.FromPair(pair);
                foreach (CompetitionParticipant p in participants)
                {
                    if (p.life != CompetitionLife.Active) continue;
                    Cell a = Maze.Canonical(new Cell((int)Math.Floor(p.position.x + 1e-8), (int)Math.Floor(p.position.y + 1e-8)));
                    Cell b = Maze.Canonical(new Cell((int)Math.Ceiling(p.position.x - 1e-8), (int)Math.Ceiling(p.position.y - 1e-8)));
                    if (cell == a || cell == b) return true;
                }
                if (AttackReserves(cell)) return true;
            }
            return false;
        }
        private void UpdateWalls()
        {
            if (!rules.movingWalls) return;
            for (int i = 0; i < walls.Length; i++)
            {
                CompetitionWall wall = walls[i]; if (wall.phase == WallPhase.Closed || TickIndex < wall.untilTick) continue;
                if (wall.phase == WallPhase.Warning) { wall.phase = WallPhase.Opening; wall.untilTick = TickIndex + 75; }
                else if (wall.phase == WallPhase.Opening) { wall.phase = WallPhase.Open; wall.untilTick = TickIndex + 300; Maze.SetShortcut(i, true); }
                else if (wall.phase == WallPhase.Open)
                {
                    if (PassageReserved(i)) continue;
                    Maze.SetShortcut(i, false); wall.phase = WallPhase.Closing; wall.untilTick = TickIndex + 75;
                }
                else { wall.phase = WallPhase.Closed; wall.cooldownUntilTick = TickIndex + 900; }
            }
            if (TickIndex < nextWallAttemptTick || walls.Length == 0) return;
            nextWallAttemptTick = TickIndex + 720 + wallRandom.Range(361);
            foreach (CompetitionWall wall in walls) if (wall.phase != WallPhase.Closed) return;
            int start = wallRandom.Range(walls.Length);
            for (int j = 0; j < walls.Length; j++)
            {
                CompetitionWall wall = walls[(start + j) % walls.Length]; if (TickIndex < wall.cooldownUntilTick) continue;
                wall.phase = WallPhase.Warning; wall.untilTick = TickIndex + 90; break;
            }
        }
        private CompetitionParticipant[] CopyParticipants() { var result = new CompetitionParticipant[5]; for (int i = 0; i < 5; i++) result[i] = participants[i].Copy(); return result; }
        private CompetitionHeart[] CopyHearts() { var result = new CompetitionHeart[hearts.Count]; for (int i = 0; i < result.Length; i++) result[i] = hearts[i].Copy(); return result; }
        private CompetitionCrate[] CopyCrates() { var result = new CompetitionCrate[crates.Length]; for (int i = 0; i < result.Length; i++) result[i] = crates[i].Copy(); return result; }
        private CompetitionWall[] CopyWalls() { var result = new CompetitionWall[walls.Length]; for (int i = 0; i < result.Length; i++) result[i] = walls[i].Copy(); return result; }
        public CompetitionSnapshot Snapshot() => new CompetitionSnapshot {
            format = "maze-of-egos-competition", version = 4,
            mazeId = Maze.Definition.id, mazeFingerprint = Maze.Fingerprint, humanId = HumanId, rules = rules.Copy(),
            phase = Phase, resumePhase = Phase == CompetitionPhase.Paused ? resumePhase : Phase,
            tick = TickIndex, nextHeartWaveTick = nextHeartWaveTick, nextWallAttemptTick = nextWallAttemptTick,
            roundStartedTick = RoundStartedTick, roundEnd = RoundEnd, roundNumber = RoundNumber, settledRound = settledRound, roundElapsedTicks = RoundElapsedTicks,
            countdownRemaining = CountdownRemaining, nextHeartId = nextHeartId,
            resourceRandom = resourceRandom.state, itemRandom = itemRandom.state, tieRandom = tieRandom.state, wallRandom = wallRandom.state, spawnRandom = spawnRandom.state,
            participants = CopyParticipants(), hearts = CopyHearts(), crates = CopyCrates(), walls = CopyWalls(),
            roundWinners = (string[])roundWinners.Clone(), matchWinners = (string[])matchWinners.Clone(),
            attacks = attacks.ToArray(), nextAttackId = nextAttackId, statistics = statistics.Copy(),
            computerPlayers = ComputerPlayers, viewAspect = viewAspect, brains = CopyBrains()
        };
        public void Restore(CompetitionSnapshot state)
        {
            state = MigrateSnapshot(state);
            // Validation uses another map. A rejected file cannot partially mutate a live match.
            MazeMap candidate = ValidateSnapshot(state);
            Maze = candidate; rules = state.rules.Copy(); HumanId = state.humanId;
            ComputerPlayers = state.computerPlayers; viewAspect = state.viewAspect;
            attacks.Clear(); attacks.AddRange(state.attacks); nextAttackId = state.nextAttackId; statistics = state.statistics.Copy();
            RestoreBrains(state.brains); Events.Clear();
            TickIndex = state.tick; RoundNumber = state.roundNumber; RoundElapsedTicks = state.roundElapsedTicks;
            RoundStartedTick = state.roundStartedTick; RoundEnd = state.roundEnd; settledRound = state.settledRound; CountdownRemaining = state.countdownRemaining; nextHeartId = state.nextHeartId;
            nextHeartWaveTick = state.nextHeartWaveTick; nextWallAttemptTick = state.nextWallAttemptTick;
            resourceRandom.state = state.resourceRandom; itemRandom.state = state.itemRandom; tieRandom.state = state.tieRandom; wallRandom.state = state.wallRandom; spawnRandom.state = state.spawnRandom;
            for (int i = 0; i < 5; i++)
            {
                participants[i] = Array.Find(state.participants, p => p.id == Ids[i]).Copy();
                navigation[i].Restore(participants[i].heading, participants[i].queuedHeading, participants[i].moving);
            }
            hearts.Clear(); foreach (CompetitionHeart heart in state.hearts) hearts.Add(heart.Copy());
            crates = new CompetitionCrate[state.crates.Length]; for (int i = 0; i < crates.Length; i++) crates[i] = state.crates[i].Copy();
            walls = new CompetitionWall[state.walls.Length]; for (int i = 0; i < walls.Length; i++) walls[i] = state.walls[i].Copy();
            roundWinners = (string[])state.roundWinners.Clone(); matchWinners = (string[])state.matchWinners.Clone();
            resumePhase = state.phase == CompetitionPhase.Paused ? state.resumePhase : state.phase;
            Phase = CompetitionPhase.Paused;
        }
        private MazeMap ValidateSnapshot(CompetitionSnapshot s)
        {
            if (s == null || s.format != "maze-of-egos-competition" || s.version != 4 || s.rules == null ||
                s.mazeId != Maze.Definition.id || s.mazeFingerprint != Maze.Fingerprint ||
                s.participants == null || s.participants.Length != 5 || s.hearts == null || s.crates == null || s.walls == null ||
                s.roundWinners == null || s.matchWinners == null || s.crates.Length != Maze.Definition.itemCells.Length ||
                s.walls.Length != Maze.OpenShortcuts.Length || s.roundNumber < 1 || s.roundNumber > 11 ||
                s.roundElapsedTicks < 0 || s.roundElapsedTicks > 7200 || s.roundStartedTick < s.roundNumber - 1L || s.roundStartedTick > (s.roundNumber - 1L) * 7200 || s.tick != s.roundStartedTick + s.roundElapsedTicks ||
                s.countdownRemaining < 0 || s.countdownRemaining > 180 || s.nextHeartWaveTick < s.tick ||
                s.nextHeartWaveTick > s.tick + 7200 || s.nextWallAttemptTick < 0 || s.nextWallAttemptTick > s.tick + 1800 ||
                s.resourceRandom == 0 || s.itemRandom == 0 || s.tieRandom == 0 || s.wallRandom == 0 || s.spawnRandom == 0 ||
                !Enum.IsDefined(typeof(CompetitionPhase), s.phase) || !Enum.IsDefined(typeof(CompetitionPhase), s.resumePhase))
                throw new ArgumentException("Invalid competition snapshot header.");
            IndexOf(s.humanId); s.rules.Validate();
            CompetitionPhase active = s.phase == CompetitionPhase.Paused ? s.resumePhase : s.phase;
            bool finished = active == CompetitionPhase.RoundResults || active == CompetitionPhase.MatchResults;
            if (active == CompetitionPhase.Paused || s.settledRound != s.roundNumber - (finished ? 0 : 1) ||
                finished != (s.roundEnd != CompetitionRoundEnd.None) ||
                !Enum.IsDefined(typeof(CompetitionRoundEnd), s.roundEnd) ||
                (finished && s.roundElapsedTicks == 0) || (!finished && s.roundElapsedTicks == 7200) ||
                (s.roundEnd == CompetitionRoundEnd.TimeExpired && s.roundElapsedTicks != 7200) ||
                (s.roundEnd == CompetitionRoundEnd.HeartsCollected && (!s.rules.endWhenHeartsCollected || s.hearts.Length != 0)) || (active == CompetitionPhase.Countdown) != (s.countdownRemaining > 0))
                throw new ArgumentException("Inconsistent competition phase/settlement.");
            var map = new MazeMap(Maze.Definition);
            for (int i = 0; i < s.walls.Length; i++)
            {
                CompetitionWall wall = s.walls[i];
                if (wall == null || !Enum.IsDefined(typeof(WallPhase), wall.phase) || wall.untilTick < 0 || wall.untilTick > s.tick + 7200 ||
                    wall.cooldownUntilTick < 0 || wall.cooldownUntilTick > s.tick + 7200 || (!s.rules.movingWalls && wall.phase != WallPhase.Closed))
                    throw new ArgumentException("Invalid competition shortcut.");
                map.SetShortcut(i, wall.phase == WallPhase.Open);
            }
            var ids = new HashSet<string>();
            int trophyCount = 0;
            foreach (CompetitionParticipant p in s.participants)
            {
                if (p == null || !ids.Add(p.id)) throw new ArgumentException("Duplicate/missing participant.");
                IndexOf(p.id);
                if (!p.position.IsFinite || p.position.x < -.5 || p.position.x >= map.Definition.width - .5 || p.position.y < 0 || p.position.y >= map.Definition.height ||
                    p.heading < 0 || p.heading > 3 || p.queuedHeading < -1 || p.queuedHeading > 3 ||
                    (p.life == CompetitionLife.Active && !CorridorNavigation.HasLegalPosition(map, p.position, p.heading, true)) ||
                    p.roundScore < 0 || p.roundScore > 1000000 || p.bankedScore < 0 || p.bankedScore > 1000000000 ||
                    p.trophies < 0 || p.trophies > 3 || (!finished && p.trophies >= 3) ||
                    !Enum.IsDefined(typeof(CompetitionItem), p.inventory) || p.insideCrates == null || p.insideCrates.Length != s.crates.Length)
                    throw new ArgumentException("Invalid competition participant.");
                if (p.trophies > s.settledRound || (s.settledRound == 0 && p.bankedScore != 0))
                    throw new ArgumentException("Competition ledger precedes its settled rounds.");
                trophyCount += p.trophies;
            }
            if (trophyCount < s.settledRound || trophyCount > s.settledRound * 5)
                throw new ArgumentException("Inconsistent competition trophy ledger.");
            for (int i = 0; i < s.crates.Length; i++)
            {
                CompetitionCrate crate = s.crates[i];
                if (crate == null || crate.cell != MazeMap.FromPair(map.Definition.itemCells[i]) || crate.availableAtTick < 0 || crate.availableAtTick > s.tick + s.rules.crateCooldownTicks)
                    throw new ArgumentException("Invalid competition crate.");
                foreach (CompetitionParticipant p in s.participants)
                    if (p.insideCrates[i] != ((map.Near(crate.cell.Position, p.position) - p.position).Length <= PickupRadius + 1e-9))
                        throw new ArgumentException("Inconsistent crate entry latch.");
            }
            var cells = new HashSet<Cell>(); var heartIds = new HashSet<int>(); int previous = 0;
            int maximumMinted = s.rules.maximumHearts + s.roundElapsedTicks / s.rules.heartWaveTicks * s.rules.groupsPerWave * s.rules.heartsPerGroup;
            if (s.hearts.Length > s.rules.maximumHearts || s.nextHeartId < 1 || s.nextHeartId > maximumMinted + 1) throw new ArgumentException("Invalid heart count.");
            foreach (CompetitionHeart heart in s.hearts)
            {
                if (heart == null || heart.id <= previous || heart.id >= s.nextHeartId || !heartIds.Add(heart.id) || !cells.Add(heart.cell) ||
                    !map.PlayerCells.Contains(heart.cell)) throw new ArgumentException("Invalid/duplicate competition heart.");
                previous = heart.id;
            }
            long maximum = 0; foreach (CompetitionParticipant p in s.participants) maximum = Math.Max(maximum, p.roundScore);
            var expectedRound = new List<string>();
            foreach (string id in Ids)
            {
                CompetitionParticipant p = Array.Find(s.participants, value => value.id == id);
                if (finished && p.roundScore == maximum) expectedRound.Add(id);
                if (finished && p.bankedScore < p.roundScore) throw new ArgumentException("Unbanked result.");
            }
            var canonical = new CompetitionParticipant[5]; for (int i = 0; i < 5; i++) canonical[i] = Array.Find(s.participants, p => p.id == Ids[i]);
            string[] expectedMatch = finished ? WinningMatchIds(canonical, s.rules.trophiesToWin) : Array.Empty<string>();
            if (!SameIds(s.roundWinners, expectedRound.ToArray()) || !SameIds(s.matchWinners, expectedMatch) ||
                (active == CompetitionPhase.MatchResults) != (expectedMatch.Length > 0))
                throw new ArgumentException("Invalid competition winners.");
            foreach (string id in expectedRound) if (Array.Find(s.participants, p => p.id == id).trophies == 0) throw new ArgumentException("Round trophy missing.");
            ValidateCombatAndBrains(s, map);
            return map;
        }
        private static bool SameIds(string[] a, string[] b)
        { if (a.Length != b.Length) return false; for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false; return true; }
    }
}
