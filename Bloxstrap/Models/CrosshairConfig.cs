namespace Bloxstrap.Models
{
    public static class CrosshairConfig
    {
        // SOURCE: assumption (my own recollection of how Roblox lays out its content folder).
        // NOT taken from this repo and NOT checked against the Roblox build you run.
        // KIND: path relative to Paths.Modifications, same style as the cursor mod paths in ModsViewModel.
        // If the crosshair never changes in game, this is the one value to fix.
        public const string LockedCursorPath = @"content\textures\MouseLockedCursor.png";

        public const string RedGlowDotResource = "Crosshair.RedGlowDot.png";
        public const string BlackDotResource = "Crosshair.BlackDot.png";

        public static string ResourceFor(OverlayCrosshair type) => type switch
        {
            OverlayCrosshair.BlackDot => BlackDotResource,
            _ => RedGlowDotResource
        };
    }
}
