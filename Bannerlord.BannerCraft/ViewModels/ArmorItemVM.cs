using System;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting.WeaponDesign;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Bannerlord.BannerCraft.ViewModels
{
    using Config = BannerCraftConfig;

    public class ArmorItemVM : ViewModel
    {
        private readonly ArmorCraftingVM _armorCrafting;
        private ImageIdentifierVM _imageIdentifier;
        private ItemObject _item;

        private ItemType _itemType;

        private bool _isSelected;

        private ItemObject.ItemTiers _tier;

        private string _tierText;

        private MBBindingList<CraftingItemFlagVM> _itemFlagIcons;

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

        [DataSourceProperty]
        public ItemObject Item
        {
            get => _item;
            set
            {
                if (value != _item)
                {
                    _item = value;
                    OnPropertyChangedWithValue(value, "Item");
                }
            }
        }

        [DataSourceProperty]
        public string ItemName => Item.Name.ToString();

        [DataSourceProperty]
        public ItemType ItemType
        {
            get => _itemType;
            private set
            {
                if (value != _itemType)
                {
                    _itemType = value;
                    OnPropertyChangedWithValue((int)value, "ItemType");
                }
            }
        }

        [DataSourceProperty]
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    OnPropertyChangedWithValue(value, "IsSelected");
                }
            }
        }

        [DataSourceProperty]
        public ItemObject.ItemTiers Tier
        {
            get => _tier;
            set
            {
                if (value != _tier)
                {
                    _tier = value;
                    OnPropertyChangedWithValue((int)value, "Tier");
                }
            }
        }

        [DataSourceProperty]
        public string TierText
        {
            get => _tierText;
            set
            {
                if (value != _tierText)
                {
                    _tierText = value;
                    OnPropertyChangedWithValue(value, "TierText");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<CraftingItemFlagVM> ItemFlagIcons
        {
            get => _itemFlagIcons;
            set
            {
                if (value != _itemFlagIcons)
                {
                    _itemFlagIcons = value;
                    OnPropertyChangedWithValue(value, "ItemFlagIcons");
                }
            }
        }

        [DataSourceProperty]
        public int Difficulty { get; }

        public EquipmentElement EquipmentElement { get; private set; }

        public ArmorItemVM(ArmorCraftingVM armorCrafting, ItemObject item, ItemType type)
        {
            _armorCrafting = armorCrafting;
            ImageIdentifier = new ImageIdentifierVM(item);
            Item = item;
            ItemType = type;

            Tier = item.Tier;
            TierText = Common.ToRoman((int)Tier + 1);

            ItemFlagIcons = new MBBindingList<CraftingItemFlagVM>();

            foreach (Tuple<string, TextObject> itemFlagDetail in CampaignUIHelper.GetItemFlagDetails(Item.ItemFlags))
            {
                ItemFlagIcons.Add(new CraftingItemFlagVM(itemFlagDetail.Item1, itemFlagDetail.Item2, isDisplayed: true));
            }

            Difficulty = Config.Instance.SmithingModel.CalculateArmorDifficulty(Item);

            EquipmentElement = new EquipmentElement(item);
        }

        public void ExecuteSelect()
        {
            _armorCrafting.CurrentItem.IsSelected = false;
            _armorCrafting.CurrentItem = this;
            _armorCrafting.ItemVisualModel.StringId = Item?.StringId ?? "";
            IsSelected = true;

            _armorCrafting.RefreshStats(ItemType);
        }

        public void ExecuteShowItemTooltip()
        {
            InformationManager.ShowTooltip(typeof(ItemObject), EquipmentElement);
        }

        public void ExecuteHideItemTooltip()
        {
            MBInformationManager.HideInformations();
        }
    }
}