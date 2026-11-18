namespace Bloxstrap.Models
{
    // Both crosshair images are 80x80 with the dot centred on the middle pixel (same canvas size as the built-in cursor images).
    public static class CrosshairConfig
    {
        public const string RedGlowDotResource = "Crosshair.RedGlowDot.png";
        public const string BlackDotResource = "Crosshair.BlackDot.png";

        public static string ResourceFor(OverlayCrosshair type) => type switch
        {
            OverlayCrosshair.BlackDot => BlackDotResource,
            _ => RedGlowDotResource
        };
    }
}
