using System;
using System.Collections.Generic;

namespace MazeOfEgos.Core
{
    [Serializable] public sealed class StartDefinition { public double x, y; public string heading; }
    [Serializable] public sealed class ShortcutDefinition { public string id; public int[][] wallCells, endpoints; }
    [Serializable]
    public sealed class MazeDefinition
    {
        public string id, rightsStatus;
        public int schemaVersion, width, height, expectedSmallDots, expectedPowerDots, horizontalTunnelRow;
        public string[] rows;
        public StartDefinition playerStart, leaderStart, ghostHomeCenter;
        public int[][] ghostDoorCells, itemCells;
        public ShortcutDefinition[] shortcutCandidates;
    }

    public sealed class MazeMap
    {
        public const double GridMeters = 2.5;
        public const double WallPadding = 0.14;
        public const double PlayerRadius = 0.12;
        public const double PursuerRadius = 0.18;
        public readonly MazeDefinition Definition;
        public readonly List<Cell> PlayerCells = new List<Cell>();
        public readonly List<Cell> Hearts = new List<Cell>();
        public readonly List<bool> PowerHearts = new List<bool>();
        public readonly bool[] OpenShortcuts;
        public int TopologyVersion { get; private set; }
        public string Fingerprint { get; private set; }
        private readonly HashSet<Cell> playerFloor = new HashSet<Cell>();
        private readonly Dictionary<Cell, int> shortcutCells = new Dictionary<Cell, int>();
        private static readonly Cell[] Directions = { new Cell(0, -1), new Cell(-1, 0), new Cell(0, 1), new Cell(1, 0) };

        public MazeMap(MazeDefinition definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            ValidateShape();
            OpenShortcuts = new bool[definition.shortcutCandidates.Length];
            for (int i = 0; i < OpenShortcuts.Length; i++)
                foreach (int[] c in definition.shortcutCandidates[i].wallCells)
                {
                    Cell cell = FromPair(c);
                    if (!InBounds(cell) || Character(cell) != '#') throw new ArgumentException("Shortcut must replace an existing wall.");
                    shortcutCells.Add(cell, i);
                }
            var queue = new Queue<Cell>();
            Cell start = new Cell((int)Math.Floor(definition.playerStart.x), (int)Math.Round(definition.playerStart.y));
            if (!RawPlayerFloor(start)) throw new ArgumentException("Player start is blocked.");
            queue.Enqueue(start); playerFloor.Add(start);
            while (queue.Count > 0)
            {
                Cell cell = queue.Dequeue();
                foreach (Cell direction in Directions)
                {
                    Cell next = Canonical(new Cell(cell.x + direction.x, cell.y + direction.y));
                    if (RawPlayerFloor(next) && playerFloor.Add(next)) queue.Enqueue(next);
                }
            }
            int small = 0, power = 0;
            uint hash = 2166136261;
            for (int y = 0; y < definition.height; y++)
                for (int x = 0; x < definition.width; x++)
                {
                    char ch = definition.rows[y][x];
                    hash = unchecked((hash ^ ch) * 16777619);
                    Cell cell = new Cell(x, y);
                    if (playerFloor.Contains(cell)) PlayerCells.Add(cell);
                    if (ch != '.' && ch != 'o') continue;
                    if (!playerFloor.Contains(cell)) throw new ArgumentException($"Heart {cell} is unreachable.");
                    Hearts.Add(cell); PowerHearts.Add(ch == 'o');
                    if (ch == 'o') power++; else small++;
                }
            if (small != definition.expectedSmallDots || power != definition.expectedPowerDots)
                throw new ArgumentException($"Expected {definition.expectedSmallDots}/{definition.expectedPowerDots} hearts; got {small}/{power}.");
            foreach (int[] c in definition.itemCells)
                if (!playerFloor.Contains(FromPair(c))) throw new ArgumentException("Item is outside the player graph.");
            foreach (ShortcutDefinition shortcut in definition.shortcutCandidates)
            {
                if (shortcut.endpoints.Length != 2) throw new ArgumentException("Shortcut needs two endpoints.");
                foreach (int[] c in shortcut.endpoints)
                    if (!playerFloor.Contains(FromPair(c))) throw new ArgumentException("Shortcut endpoint is unreachable.");
            }
            // Include gameplay metadata so a changed spawn/item/shortcut cannot silently
            // reuse an incompatible save merely because its maze id and wall rows match.
            hash = HashValue(HashValue(HashValue(hash, definition.schemaVersion), definition.width), definition.height);
            hash = HashValue(hash, definition.horizontalTunnelRow);
            foreach (StartDefinition spawn in new[] { definition.playerStart, definition.leaderStart, definition.ghostHomeCenter })
            {
                long x = BitConverter.DoubleToInt64Bits(spawn.x), y = BitConverter.DoubleToInt64Bits(spawn.y);
                hash = HashValue(HashValue(HashValue(HashValue(hash, (int)x), (int)(x >> 32)), (int)y), (int)(y >> 32));
            }
            foreach (int[] pair in definition.itemCells) hash = HashCell(hash, FromPair(pair));
            foreach (int[] pair in definition.ghostDoorCells) hash = HashCell(hash, FromPair(pair));
            foreach (ShortcutDefinition shortcut in definition.shortcutCandidates)
            {
                foreach (int[] pair in shortcut.wallCells) hash = HashCell(hash, FromPair(pair));
                foreach (int[] pair in shortcut.endpoints) hash = HashCell(hash, FromPair(pair));
            }
            Fingerprint = hash.ToString("x8");
        }
        private static uint HashValue(uint hash, int value) => unchecked((hash ^ (uint)value) * 16777619);
        private static uint HashCell(uint hash, Cell cell) => HashValue(HashValue(hash, cell.x), cell.y);

        private void ValidateShape()
        {
            if (Definition.schemaVersion != 1 || Definition.width < 3 || Definition.height < 3 ||
                Definition.rows == null || Definition.rows.Length != Definition.height ||
                Definition.playerStart == null || Definition.leaderStart == null || Definition.ghostHomeCenter == null ||
                Definition.itemCells == null || Definition.shortcutCandidates == null || Definition.ghostDoorCells == null ||
                Definition.horizontalTunnelRow < 0 || Definition.horizontalTunnelRow >= Definition.height)
                throw new ArgumentException("Invalid or unsupported maze definition.");
            foreach (string row in Definition.rows)
                if (row == null || row.Length != Definition.width) throw new ArgumentException("Maze row width is invalid; preserve spaces.");
        }
        public static Cell FromPair(int[] values)
        {
            if (values == null || values.Length != 2) throw new ArgumentException("Expected a coordinate pair.");
            return new Cell(values[0], values[1]);
        }
        public bool InBounds(Cell c) => c.x >= 0 && c.x < Definition.width && c.y >= 0 && c.y < Definition.height;
        public char Character(Cell c) => InBounds(c) ? Definition.rows[c.y][c.x] : '#';
        public bool IsHome(Cell c) => c.x >= 11 && c.x <= 16 && c.y >= 13 && c.y <= 15;
        private bool RawPlayerFloor(Cell c) => InBounds(c) && Character(c) != '#' && Character(c) != '-' && !IsHome(c);
        public Cell Canonical(Cell c)
        {
            if (c.y == Definition.horizontalTunnelRow) c.x = (c.x % Definition.width + Definition.width) % Definition.width;
            return c;
        }
        public Point2 Wrap(Point2 p)
        {
            if (Math.Abs(p.y - Definition.horizontalTunnelRow) < 0.5)
            {
                while (p.x < -0.5) p.x += Definition.width;
                while (p.x >= Definition.width - 0.5) p.x -= Definition.width;
            }
            return p;
        }
        public Point2 Near(Point2 p, Point2 reference)
        {
            if (Math.Abs(p.y - Definition.horizontalTunnelRow) < 0.6 && Math.Abs(reference.y - Definition.horizontalTunnelRow) < 0.6)
            {
                if (p.x - reference.x > Definition.width / 2.0) p.x -= Definition.width;
                if (reference.x - p.x > Definition.width / 2.0) p.x += Definition.width;
            }
            return p;
        }
        public bool IsWalkable(Cell cell, bool homeAccess = false)
        {
            cell = Canonical(cell);
            return playerFloor.Contains(cell) ||
                (shortcutCells.TryGetValue(cell, out int index) && OpenShortcuts[index]) ||
                (homeAccess && InBounds(cell) && (IsHome(cell) || Character(cell) == '-'));
        }
        public void SetShortcut(int index, bool open)
        {
            if (OpenShortcuts[index] == open) return;
            OpenShortcuts[index] = open; TopologyVersion++;
        }
        public int Neighbors(Cell c, bool homeAccess, Cell[] output)
        {
            int count = 0;
            foreach (Cell direction in Directions)
            {
                Cell next = Canonical(new Cell(c.x + direction.x, c.y + direction.y));
                if (IsWalkable(next, homeAccess)) output[count++] = next;
            }
            return count;
        }
        public Cell NearestPlayerCell(Point2 p)
        {
            Cell nearest = PlayerCells[0]; double distance = double.PositiveInfinity;
            foreach (Cell c in PlayerCells)
            {
                double d = (Near(c.Position, p) - p).LengthSquared;
                if (d < distance) { nearest = c; distance = d; }
            }
            for (int i = 0; i < OpenShortcuts.Length; i++)
                if (OpenShortcuts[i]) foreach (int[] pair in Definition.shortcutCandidates[i].wallCells)
                {
                    Cell c = FromPair(pair); double d = (c.Position - p).LengthSquared;
                    if (d < distance) { nearest = c; distance = d; }
                }
            return nearest;
        }
        public bool CanOccupy(Point2 p, double radius = PlayerRadius)
        {
            if (!p.IsFinite) return false;
            const double half = 0.5 + WallPadding;
            for (int y = (int)Math.Floor(p.y - radius - half); y <= (int)Math.Ceiling(p.y + radius + half); y++)
                for (int x = (int)Math.Floor(p.x - radius - half); x <= (int)Math.Ceiling(p.x + radius + half); x++)
                {
                    if (IsWalkable(new Cell(x, y))) continue;
                    double dx = Math.Max(Math.Abs(p.x - x) - half, 0);
                    double dy = Math.Max(Math.Abs(p.y - y) - half, 0);
                    if (dx * dx + dy * dy < radius * radius - 1e-10) return false;
                }
            return true;
        }
        public Point2 MovePlayer(Point2 p, Point2 delta)
        {
            // Small swept simulation slices bound travel. Separate axes allow wall sliding.
            Point2 next = new Point2(p.x + delta.x, p.y);
            if (CanOccupy(next)) p = next;
            next = new Point2(p.x, p.y + delta.y);
            if (CanOccupy(next)) p = next;
            return Wrap(p);
        }
        public bool HasLineOfSight(Point2 start, Point2 end)
        {
            end = Near(end, start);
            int steps = Math.Max(1, (int)Math.Ceiling((end - start).Length / 0.05));
            for (int i = 1; i < steps; i++)
                if (!CanOccupy(Point2.Lerp(start, end, (double)i / steps), 0.01)) return false;
            return true;
        }
        public Cell PathStep(Cell start, Cell goal, bool homeAccess)
        {
            if (start == goal) return start;
            var queue = new Queue<Cell>(); var previous = new Dictionary<Cell, Cell>();
            var neighbors = new Cell[4]; queue.Enqueue(start); previous[start] = start;
            while (queue.Count > 0)
            {
                Cell c = queue.Dequeue(); int count = Neighbors(c, homeAccess, neighbors);
                for (int i = 0; i < count; i++)
                {
                    Cell n = neighbors[i];
                    if (previous.ContainsKey(n)) continue;
                    previous[n] = c;
                    if (n == goal)
                    {
                        while (previous[n] != start) n = previous[n];
                        return n;
                    }
                    queue.Enqueue(n);
                }
            }
            return start;
        }
    }
}
