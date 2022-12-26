using BannerKings.Models.Vanilla;

namespace BannerKings
{
    public class BannerKingsConfig
    {
        public BKSmithingModel SmithingModel { get; } = new();

        static BannerKingsConfig()
        {
            ConfigHolder.CONFIG = new();
        }

        public static BannerKingsConfig Instance => ConfigHolder.CONFIG;

        private struct ConfigHolder
        {
            public static BannerKingsConfig CONFIG = new();
        }
    }
}