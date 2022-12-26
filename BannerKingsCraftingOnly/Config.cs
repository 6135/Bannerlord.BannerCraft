using TaleWorlds.Library;

namespace BannerCraft
{
	public class Config
	{
		public SmithingModel SmithingModel { get; } = new();

		static Config()
		{
			InformationManager.DisplayMessage(new InformationMessage("Initialising Config"));
			ConfigHolder.CONFIG = new();
		}

		public static Config Instance => ConfigHolder.CONFIG;

		private struct ConfigHolder
        {
			public static Config CONFIG = new();
        }
	}
}
