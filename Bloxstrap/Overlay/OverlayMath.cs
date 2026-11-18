namespace Bloxstrap.Overlay
{
    public readonly record struct PixelRect(int X, int Y, int Width, int Height);

    public readonly record struct PixelPoint(int X, int Y);

    public readonly record struct FrameSummary(double Fps, double AverageFrameMs, double OnePercentLowFps);

    public enum OverlaySeverity { Normal, Good, Warn, Bad }

    public sealed record OverlayLine(string Text, OverlaySeverity Severity = OverlaySeverity.Normal);

    // Pure functions, no game or window access, so they can be unit tested on their own.
    // Screen space: pixels, origin top left, y grows downward.
    public static class OverlayMath
    {
        // presentTimesMs must be ascending, all in the same time base as nowMs.
        // Returns null when fewer than two frames fall inside the window.
        public static FrameSummary? SummarizeFrames(IReadOnlyList<double> presentTimesMs, double nowMs, double windowMs)
        {
            if (windowMs <= 0 || !double.IsFinite(nowMs))
                return null;

            double cutoff = nowMs - windowMs;
            var deltas = new List<double>();
            double? previous = null;

            foreach (double t in presentTimesMs)
            {
                if (!double.IsFinite(t) || t < cutoff)
                    continue;

                if (previous is double p)
                {
                    double delta = t - p;

                    if (delta > 0 && delta < 10_000)
                        deltas.Add(delta);
                }

                previous = t;
            }

            if (deltas.Count == 0)
                return null;

            double mean = deltas.Average();

            deltas.Sort();
            int worstCount = Math.Max(1, (int)Math.Ceiling(deltas.Count * 0.01));
            double worstMean = deltas.Skip(deltas.Count - worstCount).Average();

            return new FrameSummary(1000.0 / mean, mean, 1000.0 / worstMean);
        }

        public static OverlaySeverity FpsSeverity(double fps)
        {
            if (!double.IsFinite(fps)) return OverlaySeverity.Normal;
            if (fps >= 60) return OverlaySeverity.Good;
            if (fps >= 30) return OverlaySeverity.Warn;
            return OverlaySeverity.Bad;
        }

        // Share of the whole machine's CPU used by one process, 0 to 100.
        public static double? CpuPercent(TimeSpan cpuDelta, TimeSpan wallDelta, int logicalCores)
        {
            if (wallDelta <= TimeSpan.Zero || logicalCores <= 0 || cpuDelta < TimeSpan.Zero)
                return null;

            double percent = cpuDelta.TotalMilliseconds / (wallDelta.TotalMilliseconds * logicalCores) * 100.0;
            return Math.Clamp(percent, 0, 100);
        }

        public static string FormatSession(TimeSpan elapsed)
        {
            if (elapsed < TimeSpan.Zero)
                elapsed = TimeSpan.Zero;

            return elapsed.TotalHours >= 1
                ? $"{(int)elapsed.TotalHours}:{elapsed.Minutes:00}:{elapsed.Seconds:00}"
                : $"{elapsed.Minutes:00}:{elapsed.Seconds:00}";
        }

        public static string FormatMegabytes(long bytes) =>
            bytes < 0 ? "--" : $"{bytes / (1024.0 * 1024.0):F0} MB";

        public static PixelPoint CornerPosition(OverlayCorner corner, PixelRect client, int overlayWidth, int overlayHeight, int margin)
        {
            int left = client.X + margin;
            int right = client.X + client.Width - overlayWidth - margin;
            int top = client.Y + margin;
            int bottom = client.Y + client.Height - overlayHeight - margin;

            return corner switch
            {
                OverlayCorner.TopRight => new PixelPoint(right, top),
                OverlayCorner.BottomLeft => new PixelPoint(left, bottom),
                OverlayCorner.BottomRight => new PixelPoint(right, bottom),
                _ => new PixelPoint(left, top)
            };
        }

        public static PixelPoint CenterPosition(PixelRect client, int width, int height) =>
            new(client.X + (client.Width - width) / 2, client.Y + (client.Height - height) / 2);
    }
}
