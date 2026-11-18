using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Bloxstrap.Overlay
{
    // Borderless, transparent, always on top, never takes focus, mouse clicks pass straight through.
    public abstract class OverlayWindowBase : Window
    {
        protected OverlayWindowBase()
        {
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            Topmost = true;
            ShowInTaskbar = false;
            ShowActivated = false;
            ResizeMode = ResizeMode.NoResize;
            SizeToContent = SizeToContent.WidthAndHeight;
            Focusable = false;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = -10000;
            Top = -10000;
        }

        public IntPtr Handle => new WindowInteropHelper(this).Handle;

        public double ScaleFactor => VisualTreeHelper.GetDpi(this).DpiScaleX;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            OverlayNative.MakeClickThrough(Handle);
        }

        public void MoveTo(PixelPoint point) =>
            OverlayNative.SetWindowPos(Handle, OverlayNative.HWND_TOPMOST, point.X, point.Y, 0, 0,
                OverlayNative.SWP_NOSIZE | OverlayNative.SWP_NOACTIVATE);

        // Size in physical pixels, matching the coordinates GetClientRect and SetWindowPos use.
        public (int Width, int Height) PixelSize()
        {
            var dpi = VisualTreeHelper.GetDpi(this);
            return ((int)Math.Ceiling(ActualWidth * dpi.DpiScaleX), (int)Math.Ceiling(ActualHeight * dpi.DpiScaleY));
        }
    }

    public sealed class StatsOverlayWindow : OverlayWindowBase
    {
        private static readonly Brush NormalBrush = Freeze(Color.FromRgb(0xFF, 0xFF, 0xFF));
        private static readonly Brush GoodBrush = Freeze(Color.FromRgb(0x4C, 0xE0, 0x6C));
        private static readonly Brush WarnBrush = Freeze(Color.FromRgb(0xF5, 0xC5, 0x42));
        private static readonly Brush BadBrush = Freeze(Color.FromRgb(0xF0, 0x56, 0x56));

        private readonly StackPanel _panel = new();

        public StatsOverlayWindow(double opacity, double scale)
        {
            Opacity = opacity;

            Content = new Border
            {
                Background = Freeze(Color.FromArgb(0xB0, 0x00, 0x00, 0x00)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10, 6, 10, 6),
                LayoutTransform = new ScaleTransform(scale, scale),
                Child = _panel
            };
        }

        public void SetLines(IReadOnlyList<OverlayLine> lines)
        {
            _panel.Children.Clear();

            foreach (var line in lines)
            {
                _panel.Children.Add(new TextBlock
                {
                    Text = line.Text,
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = line.Severity switch
                    {
                        OverlaySeverity.Good => GoodBrush,
                        OverlaySeverity.Warn => WarnBrush,
                        OverlaySeverity.Bad => BadBrush,
                        _ => NormalBrush
                    }
                });
            }
        }

        private static Brush Freeze(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }
    }

    public sealed class CrosshairOverlayWindow : OverlayWindowBase
    {
        private readonly Image _image = new();
        private readonly int _pixelWidth;
        private readonly int _pixelHeight;
        private readonly double _scale;

        public CrosshairOverlayWindow(string resourceName, double scale)
        {
            _scale = scale;

            var bitmap = new BitmapImage();

            using (var stream = Resource.GetStream(resourceName))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
            }

            bitmap.Freeze();

            _pixelWidth = bitmap.PixelWidth;
            _pixelHeight = bitmap.PixelHeight;

            _image.Source = bitmap;
            _image.Stretch = Stretch.Fill;
            RenderOptions.SetBitmapScalingMode(_image, BitmapScalingMode.HighQuality);

            Content = _image;
            ApplySize(VisualTreeHelper.GetDpi(this));
        }

        // The image is shown at its own pixel size times the scale setting, regardless of display scaling.
        private void ApplySize(DpiScale dpi)
        {
            _image.Width = _pixelWidth * _scale / dpi.DpiScaleX;
            _image.Height = _pixelHeight * _scale / dpi.DpiScaleY;
        }

        protected override void OnDpiChanged(DpiScale oldDpiScaleInfo, DpiScale newDpiScaleInfo)
        {
            base.OnDpiChanged(oldDpiScaleInfo, newDpiScaleInfo);
            ApplySize(newDpiScaleInfo);
        }
    }
}
