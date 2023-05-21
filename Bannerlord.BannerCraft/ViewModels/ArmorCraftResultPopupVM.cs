using System;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting.WeaponDesign;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Bannerlord.BannerCraft.ViewModels
{
    public class ArmorCraftResultPopupVM : ViewModel
    {
        private Action _onFinalize;

        private Crafting _crafting;

        private MBBindingList<ItemFlagVM> _itemFlagIconsList;

        private ItemObject _craftedItem;

        private ItemCollectionElementViewModel _itemVisualModel;

        private string _armorCraftedText;

        private string _doneLbl;

        private bool _canConfirm;

        private HintViewModel _confirmDisabledReasonHint;

        private string _itemName;

        private MBBindingList<WeaponDesignResultPropertyItemVM> _designResultPropertyList;

        [DataSourceProperty]
        public ItemCollectionElementViewModel ItemVisualModel
        {
            get => _itemVisualModel;
            set
            {
                if (value != _itemVisualModel)
                {
                    _itemVisualModel = value;
                    OnPropertyChangedWithValue(value, "ItemVisualModel");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<ItemFlagVM> ItemFlagIconsList
        {
            get => _itemFlagIconsList;
            set
            {
                if (value != _itemFlagIconsList)
                {
                    _itemFlagIconsList = value;
                    OnPropertyChangedWithValue(value, "WeaponFlagIconsList");
                }
            }
        }

        [DataSourceProperty]
        public string ArmorCraftedText
        {
            get => _armorCraftedText;
            set
            {
                if (value != _armorCraftedText)
                {
                    _armorCraftedText = value;
                    OnPropertyChangedWithValue(value, "ArmorCraftedText");
                }
            }
        }

        [DataSourceProperty]
        public string DoneLbl
        {
            get => _doneLbl;
            set
            {
                if (value != _doneLbl)
                {
                    _doneLbl = value;
                    OnPropertyChangedWithValue(value, "DoneLbl");
                }
            }
        }

        [DataSourceProperty]
        public bool CanConfirm
        {
            get => _canConfirm;
            set
            {
                if (value != _canConfirm)
                {
                    _canConfirm = value;
                    OnPropertyChangedWithValue(value, "CanConfirm");
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel ConfirmDisabledReasonHint
        {
            get => _confirmDisabledReasonHint;
            set
            {
                if (value != _confirmDisabledReasonHint)
                {
                    _confirmDisabledReasonHint = value;
                    OnPropertyChangedWithValue(value, "ConfirmDisabledReasonHint");
                }
            }
        }

        [DataSourceProperty]
        public string ItemName
        {
            get => _itemName;
            set
            {
                if (value != _itemName)
                {
                    _itemName = value;
                    UpdateCanConfirmAvailability();
                    OnPropertyChangedWithValue(value, "ItemName");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<WeaponDesignResultPropertyItemVM> DesignResultPropertyList
        {
            get => _designResultPropertyList;
            set
            {
                if (value != _designResultPropertyList)
                {
                    _designResultPropertyList = value;
                    OnPropertyChangedWithValue(value, "DesignResultPropertyList");
                }
            }
        }

        public ArmorCraftResultPopupVM(Action onFinalize, Crafting crafting, MBBindingList<ItemFlagVM> itemFlagIconsList, ItemObject craftedItem, string itemName, MBBindingList<WeaponDesignResultPropertyItemVM> designResultPropertyList, ItemCollectionElementViewModel itemVisualModel)
        {
            _onFinalize = onFinalize;
            _crafting = crafting;
            ItemFlagIconsList = itemFlagIconsList;
            DesignResultPropertyList = designResultPropertyList;
            _craftedItem = craftedItem;
            _itemVisualModel = itemVisualModel;

            ItemName = itemName;

            DoneLbl = GameTexts.FindText("str_done").ToString();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            ItemType itemType = ArmorCraftingVM.GetItemType(_craftedItem);
            ArmorCraftedText = itemType switch
            {
                ItemType.Barding => "Horse Armor Crafted!",
                ItemType.HeadArmor => "Head Armor Crafted!",
                ItemType.ShoulderArmor => "Shoulder Armor Crafted!",
                ItemType.BodyArmor => "Body Armor Crafted!",
                ItemType.ArmArmor => "Arm Armor Crafted!",
                ItemType.LegArmor => "Leg Armor Crafted!",
                ItemType.Shield => "Shield Crafted!",
                ItemType.Bow => "Bow Crafted!",
                ItemType.Crossbow => "Crossbow Crafted!",
                ItemType.Arrows => "Arrows Crafted!",
                ItemType.Bolts => "Bolts Crafted!",
                _ => "Something Crafted!"
            };

        }

        private void UpdateCanConfirmAvailability()
        {
            CanConfirm = true;
            if (string.IsNullOrEmpty(ItemName))
            {
                CanConfirm = false;
                ConfirmDisabledReasonHint = new HintViewModel(new TextObject("{=QQ03J6sf}Item name can not be empty."));
            }
        }

        public void ExecuteFinalizeCrafting()
        {
            _onFinalize?.Invoke();
        }
    }
}
