using System;

namespace MazeOfEgos.Core
{
    // Camera projections are supplied by presentation; navigation stores a world
    // heading, never a screen direction that changes when a view is resized.
    public sealed class ScreenHeadingInput
    {
        private int held = -1;
        public void Clear() => held = -1;
        public int? Read(Point2 stick, Point2 north, Point2 east)
        {
            if (!stick.IsFinite || stick.Length < .20) { Clear(); return null; }
            if (stick.Length < .35) return null;
            int heading = HeadingFor(stick, north, east);
            if (held == heading) return null;
            held = heading; return heading;
        }
        public static int HeadingFor(Point2 stick, Point2 north, Point2 east)
        {
            if (!stick.IsFinite || !north.IsFinite || !east.IsFinite || stick.Length < 1e-8 ||
                north.Length < 1e-8 || east.Length < 1e-8 ||
                Math.Abs(north.x * east.y - north.y * east.x) < 1e-8)
                throw new ArgumentException("Screen directions must be finite and non-degenerate.");
            double n = Point2.Dot(stick.Normalized, north.Normalized);
            double e = Point2.Dot(stick.Normalized, east.Normalized);
            return Math.Abs(n) >= Math.Abs(e) ? (n >= 0 ? 0 : 2) : (e >= 0 ? 1 : 3);
        }
    }
}
