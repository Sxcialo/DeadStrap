using System.Collections.ObjectModel;

namespace Bloxstrap.Models.Persistable
{
    public class Settings
    {
        // bloxstrap configuration
        public BootstrapperStyle BootstrapperStyle { get; set; } = BootstrapperStyle.FluentDialog;
        public BootstrapperIcon BootstrapperIcon { get; set; } = BootstrapperIcon.IconBloxstrap;
        public string BootstrapperTitle { get; set; } = App.ProjectName;
        public string BootstrapperIconCustomLocation { get; set; } = "";
        public Theme Theme { get; set; } = Theme.Dark;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool DeveloperMode { get; set; } = false;
        public bool CheckForUpdates { get; set; } = false;
        public bool ConfirmLaunches { get; set; } = false;
        public string Locale { get; set; } = "nil";
        public bool UseFastFlagManager { get; set; } = true;
        public bool WPFSoftwareRender { get; set; } = false;
        public bool EnableAnalytics { get; set; } = false;
        public bool BackgroundUpdatesEnabled { get; set; } = false;
        public bool DebugDisableVersionPackageCleanup { get; set; } = false;
        public string? SelectedCustomTheme { get; set; } = null;
        public WebEnvironment WebEnvironment { get; set; } = WebEnvironment.Production;

        // integration configuration
        public bool EnableActivityTracking { get; set; } = true;
        public bool UseDiscordRichPresence { get; set; } = true;
        public bool HideRPCButtons { get; set; } = true;
        public bool ShowAccountOnRichPresence { get; set; } = false;
        public bool ShowServerDetails { get; set; } = false;
        public ObservableCollection<CustomIntegration> CustomIntegrations { get; set; } = new();

        // mod preset configuration
        public bool UseDisableAppPatch { get; set; } = false;

        // overlay configuration
        public bool OverlayEnabled { get; set; } = false;
        public bool OverlayShowFps { get; set; } = true;
        public bool OverlayShowFrameTime { get; set; } = true;
        public bool OverlayShowPing { get; set; } = true;
        public bool OverlayShowCpu { get; set; } = false;
        public bool OverlayShowRam { get; set; } = false;
        public bool OverlayShowClock { get; set; } = false;
        public bool OverlayShowSessionTime { get; set; } = false;
        public OverlayCorner OverlayCorner { get; set; } = OverlayCorner.TopLeft;
        public double OverlayOpacity { get; set; } = 0.85;
        public double OverlayScale { get; set; } = 1.0;
        public bool CrosshairOverlayEnabled { get; set; } = false;
        public OverlayCrosshair CrosshairOverlayType { get; set; } = OverlayCrosshair.RedGlowDot;
        public double CrosshairOverlayScale { get; set; } = 1.0;
    }
}
