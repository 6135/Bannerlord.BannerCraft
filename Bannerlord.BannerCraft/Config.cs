using System;

namespace BannerCraft
{
	public class BannerCraftConfig
	{
		private SmithingModelBC _smithingModel;
		public SmithingModelBC SmithingModel { 
			get => _smithingModel;
			set
            {
				if (_smithingModel != null)
                {
					throw new InvalidOperationException("SmithingModel is already set");
                }
				_smithingModel = value;
            }
		}

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
