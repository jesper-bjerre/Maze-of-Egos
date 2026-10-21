using System;

namespace MazeOfEgos.Core
{
    // Presentation distance, in world metres. Simulation time freezes during
    // pause/countdown; a restored earlier timeline starts a fresh gait phase.
    public sealed class GaitClock
    {
        public const double StrideMeters = 0.88;
        private double previousTime = double.NaN;
        public double DistanceMeters { get; private set; }
        public double Cycles => DistanceMeters / StrideMeters;
        public void Reset() { previousTime = double.NaN; DistanceMeters = 0; }
        public void Update(double time, double speedMetersPerSecond)
        {
            if (double.IsNaN(time) || double.IsInfinity(time) || time < 0 ||
                double.IsNaN(speedMetersPerSecond) || double.IsInfinity(speedMetersPerSecond) || speedMetersPerSecond < 0)
                throw new ArgumentOutOfRangeException(nameof(time), "Gait needs finite nonnegative time and speed.");
            if (!double.IsNaN(previousTime))
            {
                if (time < previousTime) DistanceMeters = 0;
                else DistanceMeters += (time - previousTime) * speedMetersPerSecond;
            }
            previousTime = time;
        }
    }
}
