using System.Net.NetworkInformation;
using System.Windows.Threading;

using Bloxstrap.Integrations;

namespace Bloxstrap.Overlay
{
    // Owns the overlay windows for one running Roblox process. Created and started on the UI thread.
    public sealed class OverlayManager : IDisposable
    {
        private const string LOG_IDENT = "OverlayManager";

        private readonly int _processId;
        private readonly ActivityWatcher? _activity;
        private readonly Stopwatch _session = Stopwatch.StartNew();

        private StatsOverlayWindow? _stats;
        private CrosshairOverlayWindow? _crosshair;
        private EtwFpsCounter? _fps;
        private DispatcherTimer? _layoutTimer;
        private DispatcherTimer? _statsTimer;
        private Process? _process;

        private TimeSpan _lastCpuTotal;
        private DateTime _lastCpuAt;
        private double? _cpuPercent;

        private long _pingMs = -1;
        private bool _pingBusy;
        private int _statsTicks;

        private bool _loggedFault;
        private bool _disposed;

        public OverlayManager(int processId, ActivityWatcher? activity)
        {
            _processId = processId;
            _activity = activity;
        }

        private static Dispatcher? UiDispatcher => System.Windows.Application.Current?.Dispatcher;

        public void Start()
        {
            var settings = App.Settings.Prop;

            if (!settings.OverlayEnabled && !settings.CrosshairOverlayEnabled)
                return;

            UiDispatcher?.Invoke(StartOnUiThread);
        }

        private void StartOnUiThread()
        {
            var settings = App.Settings.Prop;

            if (settings.OverlayEnabled)
            {
                if (settings.OverlayShowFps || settings.OverlayShowFrameTime)
                {
                    _fps = new EtwFpsCounter();
                    _fps.Start(_processId);
                }

                _stats = new StatsOverlayWindow(
                    Math.Clamp(settings.OverlayOpacity, 0.2, 1.0),
                    Math.Clamp(settings.OverlayScale, 0.5, 3.0));
                _stats.Show();
            }

            if (settings.CrosshairOverlayEnabled)
            {
                _crosshair = new CrosshairOverlayWindow(
                    CrosshairConfig.ResourceFor(settings.CrosshairOverlayType),
                    Math.Clamp(settings.CrosshairOverlayScale, 0.25, 4.0));
                _crosshair.Show();
            }

            _layoutTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
            _layoutTimer.Tick += (_, _) => Guard(LayoutTick);
            _layoutTimer.Start();

            _statsTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _statsTimer.Tick += (_, _) => Guard(StatsTick);
            _statsTimer.Start();

            App.Logger.WriteLine(LOG_IDENT, $"Overlays started (stats={_stats is not null}, crosshair={_crosshair is not null}, fps={_fps?.Status})");
        }

        private void Guard(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                if (_loggedFault)
                    return;

                _loggedFault = true;
                App.Logger.WriteLine(LOG_IDENT, "Overlay tick failed (further failures are not logged)");
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        private bool TryGetRobloxClient(out IntPtr hwnd, out PixelRect client)
        {
            hwnd = IntPtr.Zero;
            client = default;

            try
            {
                _process ??= Process.GetProcessById(_processId);
                _process.Refresh();

                if (_process.HasExited)
                    return false;

                hwnd = _process.MainWindowHandle;
            }
            catch (Exception)
            {
                return false;
            }

            if (hwnd == IntPtr.Zero || !OverlayNative.IsWindow(hwnd) || OverlayNative.IsIconic(hwnd))
                return false;

            if (!OverlayNative.GetClientRect(hwnd, out var rect))
                return false;

            var origin = new OverlayNative.POINT { X = 0, Y = 0 };

            if (!OverlayNative.ClientToScreen(hwnd, ref origin))
                return false;

            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            if (width <= 0 || height <= 0)
                return false;

            client = new PixelRect(origin.X, origin.Y, width, height);
            return true;
        }

        private void LayoutTick()
        {
            if (_disposed)
                return;

            bool haveClient = TryGetRobloxClient(out IntPtr robloxWindow, out PixelRect client);
            IntPtr foreground = OverlayNative.GetForegroundWindow();

            bool active = haveClient && (foreground == robloxWindow
                || (_stats is not null && foreground == _stats.Handle)
                || (_crosshair is not null && foreground == _crosshair.Handle));

            if (_stats is not null)
            {
                bool hasContent = _stats.Content is not null && _lastLineCount > 0;
                _stats.Visibility = active && hasContent ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;

                if (active && hasContent)
                {
                    var (width, height) = _stats.PixelSize();
                    int margin = (int)Math.Round(12 * _stats.ScaleFactor);

                    _stats.MoveTo(OverlayMath.CornerPosition(App.Settings.Prop.OverlayCorner, client, width, height, margin));
                }
            }

            if (_crosshair is not null)
            {
                _crosshair.Visibility = active ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;

                if (active)
                {
                    var (width, height) = _crosshair.PixelSize();
                    _crosshair.MoveTo(OverlayMath.CenterPosition(client, width, height));
                }
            }
        }

        private int _lastLineCount;

        private void StatsTick()
        {
            if (_disposed || _stats is null)
                return;

            var settings = App.Settings.Prop;
            var lines = new List<OverlayLine>();

            FpsSnapshot? snapshot = _fps?.Snapshot();

            if (settings.OverlayShowFps)
                lines.Add(BuildFpsLine(snapshot));

            if (settings.OverlayShowFrameTime)
            {
                var summary = snapshot?.LastSecond;
                lines.Add(new OverlayLine(summary is null ? "frame --" : $"frame {summary.Value.AverageFrameMs:F1} ms"));
            }

            if (settings.OverlayShowPing)
            {
                _statsTicks++;

                if (_statsTicks % 2 == 1)
                    _ = PingAsync();

                long ping = Interlocked.Read(ref _pingMs);
                lines.Add(new OverlayLine(ping < 0 ? "ping --" : $"ping {ping} ms"));
            }

            if (settings.OverlayShowCpu)
            {
                UpdateCpu();
                lines.Add(new OverlayLine(_cpuPercent is null ? "cpu --" : $"cpu {_cpuPercent.Value:F0}%"));
            }

            if (settings.OverlayShowRam)
                lines.Add(new OverlayLine($"ram {OverlayMath.FormatMegabytes(ReadWorkingSetBytes())}"));

            if (settings.OverlayShowClock)
                lines.Add(new OverlayLine(DateTime.Now.ToString("HH:mm", CultureInfo.InvariantCulture)));

            if (settings.OverlayShowSessionTime)
                lines.Add(new OverlayLine($"time {OverlayMath.FormatSession(_session.Elapsed)}"));

            _lastLineCount = lines.Count;
            _stats.SetLines(lines);
        }

        private OverlayLine BuildFpsLine(FpsSnapshot? snapshot)
        {
            switch (_fps?.Status)
            {
                case FpsSourceStatus.NeedsPermission:
                    return new OverlayLine("fps -- (run as admin)", OverlaySeverity.Warn);
                case FpsSourceStatus.Failed:
                    return new OverlayLine("fps unavailable", OverlaySeverity.Warn);
            }

            var summary = snapshot?.LastSecond;

            if (summary is null)
                return new OverlayLine("fps --");

            string text = $"fps {summary.Value.Fps:F0}";

            if (snapshot?.LastFiveSeconds is FrameSummary longer)
                text += $"  1% {longer.OnePercentLowFps:F0}";

            return new OverlayLine(text, OverlayMath.FpsSeverity(summary.Value.Fps));
        }

        private void UpdateCpu()
        {
            try
            {
                if (_process is null)
                    return;

                _process.Refresh();

                TimeSpan total = _process.TotalProcessorTime;
                DateTime now = DateTime.UtcNow;

                if (_lastCpuAt != default)
                    _cpuPercent = OverlayMath.CpuPercent(total - _lastCpuTotal, now - _lastCpuAt, Environment.ProcessorCount);

                _lastCpuTotal = total;
                _lastCpuAt = now;
            }
            catch (Exception)
            {
                _cpuPercent = null;
            }
        }

        private long ReadWorkingSetBytes()
        {
            try
            {
                if (_process is null)
                    return -1;

                _process.Refresh();
                return _process.WorkingSet64;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        // ICMP to the game server address the activity watcher found in the Roblox log.
        // Many servers do not answer ICMP, in which case the overlay keeps showing "--".
        private async Task PingAsync()
        {
            if (_pingBusy)
                return;

            var data = _activity?.Data;

            if (data is null || !data.MachineAddressValid || !IPAddress.TryParse(data.MachineAddress, out var address))
            {
                Interlocked.Exchange(ref _pingMs, -1);
                return;
            }

            _pingBusy = true;

            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(address, 1000);
                Interlocked.Exchange(ref _pingMs, reply.Status == IPStatus.Success ? reply.RoundtripTime : -1);
            }
            catch (Exception)
            {
                Interlocked.Exchange(ref _pingMs, -1);
            }
            finally
            {
                _pingBusy = false;
            }
        }

        private void Cleanup()
        {
            _layoutTimer?.Stop();
            _statsTimer?.Stop();

            _stats?.Close();
            _crosshair?.Close();
            _fps?.Dispose();

            _stats = null;
            _crosshair = null;
            _fps = null;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            var dispatcher = UiDispatcher;

            try
            {
                if (dispatcher is null || dispatcher.CheckAccess())
                    Cleanup();
                else
                    dispatcher.Invoke(Cleanup, DispatcherPriority.Send, CancellationToken.None, TimeSpan.FromSeconds(2));
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Overlay cleanup did not finish cleanly");
                App.Logger.WriteException(LOG_IDENT, ex);
            }

            _process?.Dispose();
        }
    }
}
