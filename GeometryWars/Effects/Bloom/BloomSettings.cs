namespace GeometryWars.Effects.Bloom
{
    public class BloomSettings
    {
        public BloomSettings(string name, float bloomThreshold, float blurAmount,
            float bloomIntensity, float baseIntensity,
            float bloomSaturation, float baseSaturation)
        {
            Name = name;
            BloomThreshold = bloomThreshold;
            BlurAmount = blurAmount;
            BloomIntensity = bloomIntensity;
            BaseIntensity = baseIntensity;
            BloomSaturation = bloomSaturation;
            BaseSaturation = baseSaturation;
        }

        public string Name { get; }

        public float BloomThreshold { get; }

        public float BlurAmount { get; }

        public float BloomIntensity { get; }

        public float BaseIntensity { get; }

        public float BloomSaturation { get; }

        public float BaseSaturation { get; }

        public static BloomSettings[] PresetSettings { get; set; } = {
            //                Name           Thresh  Blur Bloom  Base  BloomSat BaseSat
            new BloomSettings("Default", 0.25f, 4, 1.25f, 1, 1, 1),
            new BloomSettings("Soft", 0, 3, 1, 1, 1, 1),
            new BloomSettings("Desaturated", 0.5f, 8, 2, 1, 0, 1),
            new BloomSettings("Saturated", 0.25f, 4, 2, 1, 2, 0),
            new BloomSettings("Blurry", 0, 2, 1, 0.1f, 1, 1),
            new BloomSettings("Subtle", 0.5f, 2, 1, 1, 1, 1)
        };
    }
}