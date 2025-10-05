using System;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting.WeaponDesign;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Bannerlord.BannerCraft.ViewModels
{
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

        public ArmorItemVM(ArmorCraftingVM armorCrafting, ItemObject item, int difficulty, ItemType type)
        {
            _armorCrafting = armorCrafting;
            ImageIdentifier = new ItemImageIdentifierVM(item);
            Item = item;
            ItemType = type;

            Tier = item.Tier;
            TierText = Common.ToRoman((int)Tier + 1);

            ItemFlagIcons = new MBBindingList<CraftingItemFlagVM>();

            foreach (Tuple<string, TextObject> itemFlagDetail in CampaignUIHelper.GetItemFlagDetails(Item.ItemFlags))
            {
                ItemFlagIcons.Add(new CraftingItemFlagVM(itemFlagDetail.Item1, itemFlagDetail.Item2, isDisplayed: true));
            }

            Difficulty = difficulty;

            EquipmentElement = new EquipmentElement(item);
        }

        [DataSourceProperty]
        public ImageIdentifierVM ImageIdentifier
        {
            get => _imageIdentifier;
            set => SetField(ref _imageIdentifier, value, nameof(ImageIdentifier));
        }

        [DataSourceProperty]
        public ItemObject Item
        {
            get => _item;
            set => SetField(ref _item, value, nameof(Item));
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
            set => SetField(ref _isSelected, value, nameof(IsSelected));
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
            set => SetField(ref _tierText, value, nameof(TierText));
        }

        [DataSourceProperty]
        public MBBindingList<CraftingItemFlagVM> ItemFlagIcons
        {
            get => _itemFlagIcons;
            set => SetField(ref _itemFlagIcons, value, nameof(ItemFlagIcons));
        }

        [DataSourceProperty]
        public int Difficulty { get; }

        public EquipmentElement EquipmentElement { get; private set; }

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