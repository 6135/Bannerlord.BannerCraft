using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Bannerlord.BannerCraft.ViewModels
{
    public class ExtraMaterialItemVM : ViewModel
    {
        private string _resourceName;
        private string _resourceItemStringId;
        private int _resourceUsageAmount;
        private int _resourceChangeAmount;
        private string _resourceMaterialTypeAsStr;
        private HintViewModel _resourceHint;

        private ImageIdentifierVM _imageIdentifier;

        public ExtraMaterialItemVM(ExtraCraftingMaterials material, ItemObject resourceItem, int amount, int changeAmount = 0)
        {
            ResourceMaterial = material;
            ResourceItem = resourceItem;
            ResourceName = ResourceItem?.Name?.ToString() ?? "none";
            ResourceHint = new HintViewModel(new TextObject("{=!}" + ResourceName));
            ResourceAmount = amount;
            ResourceItemStringId = ResourceItem?.StringId ?? "none";
            ResourceMaterialTypeAsStr = ResourceMaterial.ToString();
            ResourceChangeAmount = changeAmount;

            ImageIdentifier = new ImageIdentifierVM(ResourceItem);
        }

        public ItemObject ResourceItem { get; private set; }
        public ItemObject Material { get => ResourceItem; }

        public ExtraCraftingMaterials ResourceMaterial { get; private set; }

        [DataSourceProperty]
        public string ResourceName
        {
            get => _resourceName;
            set => SetField(ref _resourceName, value, nameof(ResourceName));
        }

        [DataSourceProperty]
        public HintViewModel ResourceHint
        {
            get => _resourceHint;
            set => SetField(ref _resourceHint, value, nameof(ResourceHint));
        }

        [DataSourceProperty]
        public string ResourceMaterialTypeAsStr
        {
            get => _resourceMaterialTypeAsStr;
            set => SetField(ref _resourceMaterialTypeAsStr, value, nameof(ResourceMaterialTypeAsStr));
        }

        [DataSourceProperty]
        public int ResourceAmount
        {
            get => _resourceUsageAmount;
            set => SetField(ref _resourceUsageAmount, value, nameof(ResourceAmount));
        }

        [DataSourceProperty]
        public int ResourceChangeAmount
        {
            get => _resourceChangeAmount;
            set => SetField(ref _resourceChangeAmount, value, nameof(ResourceChangeAmount));
        }

        [DataSourceProperty]
        public string ResourceItemStringId
        {
            get => _resourceItemStringId;
            set => SetField(ref _resourceItemStringId, value, nameof(ResourceItemStringId));
        }

        [DataSourceProperty]
        public ImageIdentifierVM ImageIdentifier
        {
            get => _imageIdentifier;
            set => SetField(ref _imageIdentifier, value, nameof(ImageIdentifier));
        }

        [DataSourceProperty] public ImageIdentifierVM Visual { get => ImageIdentifier; }
    }
}