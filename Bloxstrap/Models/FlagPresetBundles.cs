namespace Bloxstrap.Models
{
    public sealed record FlagPresetBundle(string Name, string Description, IReadOnlyDictionary<string, string?> Flags)
    {
        public bool HasFlags => Flags.Count > 0;
    }

    // SOURCE for every flag name and value below: the flag presets that already exist in this repo
    // (FastFlagManager.PresetFlags, TextureQuality levels 0 = lowest and 3 = highest, MSAA modes 1/2/4).
    // A null value means "remove this flag".
    // Nothing here comes from a Roblox flag list. "Lowest ping" is empty on purpose: this repo has no network flags,
    // and I did not invent any. Add real ones to that dictionary and the button turns on by itself.
    public static class FlagPresetBundles
    {
        public static IReadOnlyList<FlagPresetBundle> All { get; } = new List<FlagPresetBundle>
        {
            new("Best graphics",
                "4x MSAA, highest texture quality, display scaling fix on.",
                new Dictionary<string, string?>
                {
                    { "FIntDebugForceMSAASamples", "4" },
                    { "DFFlagTextureQualityOverrideEnabled", "True" },
                    { "DFIntTextureQualityOverride", "3" },
                    { "DFFlagDisableDPIScale", "True" }
                }),

            new("Highest FPS",
                "MSAA off, lowest texture quality, display scaling fix off. Only uses the graphics flags DeadStrap already knows.",
                new Dictionary<string, string?>
                {
                    { "FIntDebugForceMSAASamples", "1" },
                    { "DFFlagTextureQualityOverrideEnabled", "True" },
                    { "DFIntTextureQualityOverride", "0" },
                    { "DFFlagDisableDPIScale", null }
                }),

            new("Lowest ping",
                "No flags set yet. DeadStrap has no network flags to use, so this stays off until some are added.",
                new Dictionary<string, string?>())
        };
    }
}
