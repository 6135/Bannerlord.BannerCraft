using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;

namespace BannerCraft
{
	using Config = BannerCraftConfig;

	public enum ExtraCraftingMaterials
    {
		Fur,
		Leather,
		Linen,
		Velvet,
		NumExtraCraftingMats
    }

	public class ExtraMaterialItemVM : ViewModel
	{
		private string _resourceName;
		private string _resourceItemStringId;
		private int _resourceUsageAmount;
		private int _resourceChangeAmount;
		private string _resourceMaterialTypeAsStr;
		private HintViewModel _resourceHint;

		private ImageIdentifierVM _imageIdentifier;

		public ItemObject ResourceItem { get; private set; }
		public ItemObject Material { get => ResourceItem; }
		
		public ExtraCraftingMaterials ResourceMaterial { get; private set; }

        [DataSourceProperty]
		public string ResourceName
        {
			get => _resourceName;
			set
            {
				if (value != _resourceName)
                {
					_resourceName = value;
					OnPropertyChangedWithValue(value, "ResourceName");
                }
            }
        }

        [DataSourceProperty]
		public HintViewModel ResourceHint
        {
			get => _resourceHint;
			set
            {
				if (value != _resourceHint)
                {
					_resourceHint = value;
					OnPropertyChangedWithValue(value, "ResourceHint");
                }
            }
        }

        [DataSourceProperty]
		public string ResourceMaterialTypeAsStr
        {
			get => _resourceMaterialTypeAsStr;
			set
            {
				if (value != _resourceMaterialTypeAsStr)
                {
					_resourceMaterialTypeAsStr = value;
					OnPropertyChangedWithValue(value, "ResourceMaterialTypeAsStr");
                }
            }
        }

        [DataSourceProperty]
		public int ResourceAmount
        {
			get => _resourceUsageAmount;
			set
            {
				if (value != _resourceUsageAmount)
                {
					_resourceUsageAmount = value;
					OnPropertyChangedWithValue(value, "ResourceAmount");
                }
            }
        }

        [DataSourceProperty]
		public int ResourceChangeAmount
        {
			get => _resourceChangeAmount;
			set
            {
				if (value != _resourceChangeAmount)
                {
					_resourceChangeAmount = value;
					OnPropertyChangedWithValue(value, "ResourceChangeAmount");
                }
            }
        }

        [DataSourceProperty]
		public string ResourceItemStringId
		{
			get => _resourceItemStringId;
			set
            {
				if (value != _resourceItemStringId)
				{
					_resourceItemStringId = value;
					OnPropertyChangedWithValue(value, "ResourceItemStringId");
				}
            }
		}

        [DataSourceProperty]
		public ImageIdentifierVM ImageIdentifier
		{
			get => _imageIdentifier;
			set
            {
				if (value != _imageIdentifier)
                {
					_imageIdentifier = value;
					OnPropertyChangedWithValue(value, "ImageIdentifier");
                }
            }
		}

        [DataSourceProperty] public ImageIdentifierVM Visual { get => ImageIdentifier; }

		public ExtraMaterialItemVM(ExtraCraftingMaterials material, int amount, int changeAmount = 0)
		{
			ResourceMaterial = material;
			ResourceItem = Config.Instance.SmithingModel.GetCraftingMaterialItem(material);
			ResourceName = ResourceItem?.Name?.ToString() ?? "none";
			ResourceHint = new HintViewModel(new TaleWorlds.Localization.TextObject("{=!}" + ResourceName));
			ResourceAmount = amount;
			ResourceItemStringId = ResourceItem?.StringId ?? "none";
			ResourceMaterialTypeAsStr = ResourceMaterial.ToString();
			ResourceChangeAmount = changeAmount;

			ImageIdentifier = new ImageIdentifierVM(ResourceItem);
		}

		public static ExtraCraftingMaterials FromItem(ItemObject item) => item.Name.ToString().ToLower() switch
		{
			"linen" => ExtraCraftingMaterials.Linen,
			"fur" => ExtraCraftingMaterials.Fur,
			"leather" => ExtraCraftingMaterials.Leather,
			"velvet" => ExtraCraftingMaterials.Velvet,
			_ => ExtraCraftingMaterials.NumExtraCraftingMats
		};

		public ExtraMaterialItemVM(ItemObject material) : this(FromItem(material), 0)
        {
        }
	}
}
