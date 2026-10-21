using System;

namespace MazeOfEgos.Core
{
    // Fixed storage: collecting a frame never allocates or sorts. Percentiles are
    // upper bounds in 0.25 ms buckets; overflow uses the actual worst frame.
    public sealed class FrameStatistics
    {
        private readonly long[] histogram = new long[1001];
        public long Count { get; private set; }
        public long Over33Ms { get; private set; }
        public long Over50Ms { get; private set; }
        public double TotalMs { get; private set; }
        public double WorstMs { get; private set; }
        public double AverageMs => Count == 0 ? 0 : TotalMs / Count;
        public double FramesPerSecond => TotalMs == 0 ? 0 : Count * 1000 / TotalMs;
        public void Add(double milliseconds)
        {
            if (double.IsNaN(milliseconds) || double.IsInfinity(milliseconds) || milliseconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(milliseconds));
            Count++; TotalMs += milliseconds; WorstMs = Math.Max(WorstMs, milliseconds);
            if (milliseconds > 1000.0 / 30) Over33Ms++;
            if (milliseconds > 50) Over50Ms++;
            int bucket = milliseconds >= 250 ? 1000 : (int)Math.Ceiling(milliseconds * 4);
            histogram[bucket]++;
        }
        public double Percentile(double percentile)
        {
            if (percentile <= 0 || percentile > 1 || double.IsNaN(percentile)) throw new ArgumentOutOfRangeException(nameof(percentile));
            if (Count == 0) return 0;
            long target = (long)Math.Ceiling(Count * percentile), accumulated = 0;
            for (int i = 0; i < histogram.Length; i++)
            {
                accumulated += histogram[i];
                if (accumulated >= target) return i == 1000 ? WorstMs : i / 4.0;
            }
            return WorstMs;
        }
        public void Clear()
        { Array.Clear(histogram, 0, histogram.Length); Count = Over33Ms = Over50Ms = 0; TotalMs = WorstMs = 0; }
    }
}
