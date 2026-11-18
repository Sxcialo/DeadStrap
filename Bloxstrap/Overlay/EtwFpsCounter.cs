using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;

namespace Bloxstrap.Overlay
{
    public enum FpsSourceStatus { NotStarted, Running, NeedsPermission, Failed }

    public readonly record struct FpsSnapshot(FrameSummary? LastSecond, FrameSummary? LastFiveSeconds);

    // Counts frames of one process by listening to DXGI present events over ETW (the approach PresentMon uses).
    public sealed class EtwFpsCounter : IDisposable
    {
        private const string LOG_IDENT = "EtwFpsCounter";

        // SOURCE: assumption, from my memory of PresentMon. Not from your artifacts, not verified on your machine.
        // KIND: ETW provider GUID (Microsoft-Windows-DXGI) and the event id of "Present start".
        // If FPS stays "--" while the game runs, check these first (logman query providers, or PresentMon source).
        private static readonly Guid DxgiProvider = new("CA11C036-0102-4A2D-A6AD-F03CFED5D3C9");
        private const int DxgiPresentStartEventId = 42;

        private const int MaxStoredPresents = 20_000;
        private const double KeepMs = 15_000;

        private readonly object _lock = new();
        private readonly List<double> _presentTimesMs = new();

        private TraceEventSession? _session;
        private Task? _processingTask;
        private int _processId;

        public FpsSourceStatus Status { get; private set; } = FpsSourceStatus.NotStarted;

        public void Start(int processId)
        {
            _processId = processId;

            try
            {
                _session = new TraceEventSession($"DeadStrapFps-{Environment.ProcessId}") { StopOnDispose = true };
                _session.Source.AllEvents += OnEvent;
                _session.EnableProvider(DxgiProvider, TraceEventLevel.Verbose);

                var source = _session.Source;

                _processingTask = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        source.Process();
                    }
                    catch (Exception ex)
                    {
                        App.Logger.WriteLine(LOG_IDENT, "ETW processing loop ended with an error");
                        App.Logger.WriteException(LOG_IDENT, ex);
                        Status = FpsSourceStatus.Failed;
                    }
                }, TaskCreationOptions.LongRunning);

                Status = FpsSourceStatus.Running;
            }
            catch (UnauthorizedAccessException)
            {
                App.Logger.WriteLine(LOG_IDENT, "Not allowed to start an ETW session (needs admin or Performance Log Users)");
                ReleaseSession();
                Status = FpsSourceStatus.NeedsPermission;
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Could not start the ETW session");
                App.Logger.WriteException(LOG_IDENT, ex);
                ReleaseSession();
                Status = FpsSourceStatus.Failed;
            }
        }

        private void OnEvent(TraceEvent data)
        {
            if (data.ProcessID != _processId || data.ProviderGuid != DxgiProvider || (int)data.ID != DxgiPresentStartEventId)
                return;

            double ms = data.TimeStamp.Ticks / 10_000.0;

            lock (_lock)
            {
                _presentTimesMs.Add(ms);

                if (_presentTimesMs.Count > MaxStoredPresents)
                    _presentTimesMs.RemoveRange(0, _presentTimesMs.Count - MaxStoredPresents);
            }
        }

        public FpsSnapshot Snapshot()
        {
            double now = DateTime.Now.Ticks / 10_000.0;
            List<double> copy;

            lock (_lock)
            {
                int stale = _presentTimesMs.FindIndex(t => t >= now - KeepMs);

                if (stale < 0)
                    _presentTimesMs.Clear();
                else if (stale > 0)
                    _presentTimesMs.RemoveRange(0, stale);

                copy = new List<double>(_presentTimesMs);
            }

            return new FpsSnapshot(
                OverlayMath.SummarizeFrames(copy, now, 1000),
                OverlayMath.SummarizeFrames(copy, now, 5000));
        }

        private void ReleaseSession()
        {
            try { _session?.Dispose(); } catch { }
            _session = null;
        }

        public void Dispose()
        {
            ReleaseSession();

            try { _processingTask?.Wait(1000); } catch { }
        }
    }
}
