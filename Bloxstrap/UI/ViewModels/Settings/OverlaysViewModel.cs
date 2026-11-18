namespace Bloxstrap.UI.ViewModels.Settings
{
    public class OverlaysViewModel : NotifyPropertyChangedViewModel
    {
        public IReadOnlyList<OverlayCorner> Corners { get; } = Enum.GetValues<OverlayCorner>();

        public IReadOnlyList<OverlayCrosshair> CrosshairTypes { get; } = Enum.GetValues<OverlayCrosshair>();

        public bool OverlayEnabled
        {
            get => App.Settings.Prop.OverlayEnabled;
            set => App.Settings.Prop.OverlayEnabled = value;
        }

        public bool OverlayShowFps
        {
            get => App.Settings.Prop.OverlayShowFps;
            set => App.Settings.Prop.OverlayShowFps = value;
        }

        public bool OverlayShowFrameTime
        {
            get => App.Settings.Prop.OverlayShowFrameTime;
            set => App.Settings.Prop.OverlayShowFrameTime = value;
        }

        public bool OverlayShowPing
        {
            get => App.Settings.Prop.OverlayShowPing;
            set => App.Settings.Prop.OverlayShowPing = value;
        }

        public bool OverlayShowCpu
        {
            get => App.Settings.Prop.OverlayShowCpu;
            set => App.Settings.Prop.OverlayShowCpu = value;
        }

        public bool OverlayShowRam
        {
            get => App.Settings.Prop.OverlayShowRam;
            set => App.Settings.Prop.OverlayShowRam = value;
        }

        public bool OverlayShowClock
        {
            get => App.Settings.Prop.OverlayShowClock;
            set => App.Settings.Prop.OverlayShowClock = value;
        }

        public bool OverlayShowSessionTime
        {
            get => App.Settings.Prop.OverlayShowSessionTime;
            set => App.Settings.Prop.OverlayShowSessionTime = value;
        }

        public OverlayCorner OverlayCorner
        {
            get => App.Settings.Prop.OverlayCorner;
            set => App.Settings.Prop.OverlayCorner = value;
        }

        public double OverlayOpacity
        {
            get => App.Settings.Prop.OverlayOpacity;
            set => App.Settings.Prop.OverlayOpacity = value;
        }

        public double OverlayScale
        {
            get => App.Settings.Prop.OverlayScale;
            set => App.Settings.Prop.OverlayScale = value;
        }

        public bool CrosshairOverlayEnabled
        {
            get => App.Settings.Prop.CrosshairOverlayEnabled;
            set => App.Settings.Prop.CrosshairOverlayEnabled = value;
        }

        public OverlayCrosshair CrosshairOverlayType
        {
            get => App.Settings.Prop.CrosshairOverlayType;
            set => App.Settings.Prop.CrosshairOverlayType = value;
        }

        public double CrosshairOverlayScale
        {
            get => App.Settings.Prop.CrosshairOverlayScale;
            set => App.Settings.Prop.CrosshairOverlayScale = value;
        }
    }
}
