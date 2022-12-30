using TaleWorlds.Library;

namespace BannerCraft
{
	public class BannerCraftConfig
	{
		public SmithingModel SmithingModel { get; } = new();

		static BannerCraftConfig()
		{
			ConfigHolder.CONFIG = new();
		}

		public static BannerCraftConfig Instance => ConfigHolder.CONFIG;

		private struct ConfigHolder
        {
			public static BannerCraftConfig CONFIG = new();
        }
	}
}
