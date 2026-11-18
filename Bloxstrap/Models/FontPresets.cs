namespace Bloxstrap.Models
{
    public sealed record FontPreset(string DisplayName, string ResourceName, string FileName);

    public static class FontPresets
    {
        public const string NoneName = "Default (no custom font)";

        public static IReadOnlyList<FontPreset> All { get; } = new List<FontPreset>
        {
            new("Hey Comic", "Fonts.Hey_Comic.ttf", "Hey_Comic.ttf"),
            new("Anime Ace 2", "Fonts.animeace2_reg.ttf", "animeace2_reg.ttf"),
            new("Minecraft", "Fonts.Minecraft.ttf", "Minecraft.ttf")
        };

        public static IReadOnlyList<string> Names { get; } =
            new[] { NoneName }.Concat(All.Select(x => x.DisplayName)).ToList();

        public static FontPreset? Find(string displayName) => All.FirstOrDefault(x => x.DisplayName == displayName);

        public static string GetResourceHash(FontPreset preset)
        {
            using var stream = Resource.GetStream(preset.ResourceName);
            return MD5Hash.Stringify(App.MD5Provider.ComputeHash(stream));
        }

        // Writes the embedded font next to DeadStrap's data (outside Modifications, so it is never copied into Roblox by itself)
        // and returns its path, or null if extraction failed.
        public static string? Extract(FontPreset preset)
        {
            const string LOG_IDENT = "FontPresets::Extract";

            try
            {
                string directory = Path.Combine(Paths.Base, "Presets", "Fonts");
                Directory.CreateDirectory(directory);

                string path = Path.Combine(directory, preset.FileName);

                using var resource = Resource.GetStream(preset.ResourceName);
                using var file = File.Create(path);
                resource.CopyTo(file);

                return path;
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Failed to extract font preset '{preset.DisplayName}'");
                App.Logger.WriteException(LOG_IDENT, ex);
                return null;
            }
        }
    }
}
