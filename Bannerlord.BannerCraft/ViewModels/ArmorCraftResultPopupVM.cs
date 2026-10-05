using System;
using TaleWorlds.CampaignSystem.ViewModelCollection.Input;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting.WeaponDesign;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
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

        private InputKeyItemVM? _doneInputKey;

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

            var confirmHotKey = HotKeyManager.GetCategory("GenericPanelGameKeyCategory")?.GetHotKey("Confirm");
            if (confirmHotKey != null)
            {
                // Like vanilla, only show the key hint when using a gamepad.
                DoneInputKey = InputKeyItemVM.CreateFromHotKey(confirmHotKey, isConsoleOnly: true);
            }

            RefreshValues();
        }

        [DataSourceProperty]
        public InputKeyItemVM? DoneInputKey
        {
            get => _doneInputKey;
            set => SetField(ref _doneInputKey, value, nameof(DoneInputKey));
        }

        [DataSourceProperty]
        public ItemCollectionElementViewModel ItemVisualModel
        {
            get => _itemVisualModel;
            set => SetField(ref _itemVisualModel, value, nameof(ItemVisualModel));
        }

        [DataSourceProperty]
        public MBBindingList<ItemFlagVM> ItemFlagIconsList
        {
            get => _itemFlagIconsList;
            set => SetField(ref _itemFlagIconsList, value, nameof(ItemFlagIconsList));
        }

        [DataSourceProperty]
        public string ArmorCraftedText
        {
            get => _armorCraftedText;
            set => SetField(ref _armorCraftedText, value, nameof(ArmorCraftedText));
        }

        [DataSourceProperty]
        public string DoneLbl
        {
            get => _doneLbl;
            set => SetField(ref _doneLbl, value, nameof(DoneLbl));
        }

        [DataSourceProperty]
        public bool CanConfirm
        {
            get => _canConfirm;
            set => SetField(ref _canConfirm, value, nameof(CanConfirm));
        }

        [DataSourceProperty]
        public HintViewModel ConfirmDisabledReasonHint
        {
            get => _confirmDisabledReasonHint;
            set => SetField(ref _confirmDisabledReasonHint, value, nameof(ConfirmDisabledReasonHint));
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
            set => SetField(ref _designResultPropertyList, value, nameof(DesignResultPropertyList));
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            ItemType itemType = ArmorCraftingVM.GetItemType(_craftedItem);
            TextObject craftedText = itemType switch
            {
                ItemType.Barding => new TextObject("{=bannercraft_crafted_barding}Horse Armor Crafted!"),
                ItemType.HeadArmor => new TextObject("{=bannercraft_crafted_headarmor}Head Armor Crafted!"),
                ItemType.ShoulderArmor => new TextObject("{=bannercraft_crafted_shoulderarmor}Shoulder Armor Crafted!"),
                ItemType.BodyArmor => new TextObject("{=bannercraft_crafted_bodyarmor}Body Armor Crafted!"),
                ItemType.ArmArmor => new TextObject("{=bannercraft_crafted_armarmor}Arm Armor Crafted!"),
                ItemType.LegArmor => new TextObject("{=bannercraft_crafted_legarmor}Leg Armor Crafted!"),
                ItemType.Shield => new TextObject("{=bannercraft_crafted_shield}Shield Crafted!"),
                ItemType.Bow => new TextObject("{=bannercraft_crafted_bow}Bow Crafted!"),
                ItemType.Crossbow => new TextObject("{=bannercraft_crafted_crossbow}Crossbow Crafted!"),
                ItemType.Arrows => new TextObject("{=bannercraft_crafted_arrows}Arrows Crafted!"),
                ItemType.Bolts => new TextObject("{=bannercraft_crafted_bolts}Bolts Crafted!"),
                ItemType.Banner => new TextObject("{=bannercraft_crafted_banner}Banner Crafted!"),
                ItemType.OneHandedWeapon or ItemType.TwoHandedWeapon or ItemType.Polearm or ItemType.Thrown
                    => new TextObject("{=bannercraft_crafted_weapon}Weapon Crafted!"),
                _ => new TextObject("{=bannercraft_crafted_other}Something Crafted!")
            };
            ArmorCraftedText = craftedText.ToString();
        }

        public void ExecuteFinalizeCrafting()
        {
            _onFinalize?.Invoke();
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
    }
}