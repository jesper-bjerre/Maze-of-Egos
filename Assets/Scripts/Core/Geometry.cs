using System;
using System.Runtime.Serialization;

namespace MazeOfEgos.Core
{
    [Serializable, DataContract]
    public struct Point2
    {
        [DataMember] public double x, y;
        public Point2(double x, double y) { this.x = x; this.y = y; }
        public double LengthSquared => x * x + y * y;
        public double Length => Math.Sqrt(LengthSquared);
        public Point2 Normalized => Length > 1e-9 ? this / Length : new Point2();
        public bool IsFinite => !double.IsNaN(x) && !double.IsInfinity(x) && !double.IsNaN(y) && !double.IsInfinity(y);
        public static Point2 operator +(Point2 a, Point2 b) => new Point2(a.x + b.x, a.y + b.y);
        public static Point2 operator -(Point2 a, Point2 b) => new Point2(a.x - b.x, a.y - b.y);
        public static Point2 operator *(Point2 a, double b) => new Point2(a.x * b, a.y * b);
        public static Point2 operator /(Point2 a, double b) => new Point2(a.x / b, a.y / b);
        public static double Dot(Point2 a, Point2 b) => a.x * b.x + a.y * b.y;
        public static Point2 Lerp(Point2 a, Point2 b, double t) => a + (b - a) * t;
        public static double Distance(Point2 a, Point2 b) => (a - b).Length;
        public static Point2 Forward(double yaw) => new Point2(Math.Sin(yaw * Math.PI / 180), -Math.Cos(yaw * Math.PI / 180));
        public static Point2 Right(double yaw) => new Point2(Math.Cos(yaw * Math.PI / 180), Math.Sin(yaw * Math.PI / 180));
        public override string ToString() => $"({x:F3}, {y:F3})";
    }

    [Serializable, DataContract]
    public struct Cell : IEquatable<Cell>
    {
        [DataMember] public int x, y;
        public Cell(int x, int y) { this.x = x; this.y = y; }
        public Point2 Position => new Point2(x, y);
        public bool Equals(Cell other) => x == other.x && y == other.y;
        public override bool Equals(object obj) => obj is Cell other && Equals(other);
        public override int GetHashCode() => unchecked(x * 397 ^ y);
        public static bool operator ==(Cell a, Cell b) => a.Equals(b);
        public static bool operator !=(Cell a, Cell b) => !a.Equals(b);
        public override string ToString() => $"[{x},{y}]";
    }

    public static class Geometry
    {
        // Earliest contact of two moving circles, including actors exchanging positions.
        public static double ContactTime(Point2 a0, Point2 a1, Point2 b0, Point2 b1, double radius)
        {
            Point2 start = a0 - b0, delta = (a1 - a0) - (b1 - b0);
            double c = start.LengthSquared - radius * radius;
            if (c <= 1e-10) return 0;
            double a = delta.LengthSquared;
            if (a < 1e-14) return double.PositiveInfinity;
            double b = 2 * Point2.Dot(start, delta), discriminant = b * b - 4 * a * c;
            if (discriminant < 0) return double.PositiveInfinity;
            double t = (-b - Math.Sqrt(discriminant)) / (2 * a);
            return t >= -1e-9 && t <= 1 + 1e-9 ? Math.Max(0, Math.Min(1, t)) : double.PositiveInfinity;
        }
        public static double Clamp(double value, double min, double max) => Math.Max(min, Math.Min(max, value));
    }

    [Serializable]
    public struct RandomState
    {
        public uint state;
        public RandomState(uint seed) { state = seed == 0 ? 0x9E3779B9u : seed; }
        public uint Next() { uint n = state; n ^= n << 13; n ^= n >> 17; n ^= n << 5; return state = n; }
        public int Range(int maximum) => (int)(Next() % (uint)maximum);
        public double Unit() => Next() / 4294967296.0;
    }
}
