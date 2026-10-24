using System;
namespace MazeOfEgos.Core
{
    public enum RelativeTurn { Forward, Right, Reverse, Left }
    public sealed class JoystickTurnLatch
    {
        private int held = -1;
        public void Clear() => held = -1;
        public RelativeTurn? Read(Point2 stick)
        {
            if (!stick.IsFinite || stick.Length < .20) { Clear(); return null; }
            if (stick.Length < .35) return null;
            int sector = Math.Abs(stick.x) > Math.Abs(stick.y) ? (stick.x > 0 ? 1 : 3) : (stick.y > 0 ? 0 : 2);
            if (held == sector) return null;
            held = sector; return (RelativeTurn)sector;
        }
    }
    public sealed class CorridorNavigation
    {
        public int Heading { get; private set; } = 3;
        public int QueuedHeading { get; private set; } = -1;
        public bool Moving { get; private set; } = true;
        public static int FromYaw(double yaw) => ((int)Math.Round(yaw / 90, MidpointRounding.AwayFromZero) % 4 + 4) % 4;
        public static Point2 Direction(int heading) => heading == 0 ? new Point2(0, -1) : heading == 1 ? new Point2(1, 0) : heading == 2 ? new Point2(0, 1) : new Point2(-1, 0);
        public void Restore(int heading, int queued, bool moving) { Heading = heading; QueuedHeading = queued; Moving = moving; }
        public void Request(RelativeTurn turn) => RequestHeading((Heading + (int)turn) % 4);
        public void RequestHeading(int heading)
        {
            if (heading < 0 || heading > 3) throw new ArgumentOutOfRangeException(nameof(heading));
            QueuedHeading = heading; Moving = true;
        }
        public Point2 Advance(MazeMap map, Point2 position, double distance)
        {
            const double epsilon = 1e-8;
            if (!Moving) return position;
            for (int step = 0; step < 8 && distance > epsilon; step++)
            {
                // Reversal is legal everywhere along a corridor; other turns require a centre.
                if (QueuedHeading == (Heading + 2) % 4) { Heading = QueuedHeading; QueuedHeading = -1; }
                bool atCentre = Math.Abs(position.x - Math.Round(position.x)) < epsilon && Math.Abs(position.y - Math.Round(position.y)) < epsilon;
                if (atCentre)
                {
                    position = new Point2(Math.Round(position.x), Math.Round(position.y));
                    if (QueuedHeading >= 0 && CanStep(map, position, QueuedHeading)) { Heading = QueuedHeading; QueuedHeading = -1; }
                    if (!CanStep(map, position, Heading)) { break; }
                }
                Point2 direction = Direction(Heading);
                double coordinate = Heading % 2 == 0 ? position.y : position.x;
                double sign = Heading == 0 || Heading == 3 ? -1 : 1;
                double centre = sign > 0 ? Math.Floor(coordinate + epsilon) + 1 : Math.Ceiling(coordinate - epsilon) - 1;
                double travel = Math.Min(distance, Math.Abs(centre - coordinate));
                position = map.Wrap(position + direction * travel); distance -= travel;
            }
            return position;
        }
        private static bool CanStep(MazeMap map, Point2 position, int heading)
        {
            Point2 next = position + Direction(heading);
            return map.IsWalkable(new Cell((int)Math.Round(next.x), (int)Math.Round(next.y)));
        }
        public static Point2 AlignLegacy(MazeMap map, Point2 point, int heading)
        {
            Point2 aligned = heading % 2 == 0 ? new Point2(Math.Round(point.x), point.y) : new Point2(point.x, Math.Round(point.y));
            return map.CanOccupy(aligned) && HasLegalPosition(map, aligned, heading) ? map.Wrap(aligned) : map.NearestPlayerCell(point).Position;
        }
        public static bool HasLegalPosition(MazeMap map, Point2 point, int heading)
        {
            if (!OnCentreLine(point, heading)) return false;
            Cell a = new Cell((int)Math.Floor(point.x + 1e-8), (int)Math.Floor(point.y + 1e-8));
            Cell b = new Cell((int)Math.Ceiling(point.x - 1e-8), (int)Math.Ceiling(point.y - 1e-8));
            return map.IsWalkable(a) && map.IsWalkable(b);
        }
        public static bool OnCentreLine(Point2 point, int heading) =>
            Math.Abs((heading % 2 == 0 ? point.x : point.y) - Math.Round(heading % 2 == 0 ? point.x : point.y)) < 1e-7;
    }
}
