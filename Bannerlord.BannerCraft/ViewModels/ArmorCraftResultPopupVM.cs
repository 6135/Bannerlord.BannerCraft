using System;
using TaleWorlds.CampaignSystem.ViewModelCollection.Input;
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

        private InputKeyItemVM? _doneInputKey;

        public ArmorCraftResultPopupVM(Action onFinalize, Crafting crafting, MBBindingList<ItemFlagVM> itemFlagIconsList, ItemObject craftedItem, string itemName, MBBindingList<WeaponDesignResultPropertyItemVM> designResultPropertyList, ItemCollectionElementViewModel itemVisualModel, InputKeyItemVM? doneInputKey)
        {
            _onFinalize = onFinalize;
            _crafting = crafting;
            ItemFlagIconsList = itemFlagIconsList;
            DesignResultPropertyList = designResultPropertyList;
            _craftedItem = craftedItem;
            _itemVisualModel = itemVisualModel;

            ItemName = itemName;

            DoneLbl = GameTexts.FindText("str_done").ToString();
            DoneInputKey = doneInputKey;

            /*
             * The base ViewModel constructor does not call RefreshValues, so ArmorCraftedText would
             * otherwise stay null until something else refreshed the popup.
             */
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

            /*
             * Every ItemType has a matching str_bannercraft_crafting_result variation, including
             * Invalid, so banners and normal weapons no longer fall through to a generic message.
             */
            ItemType itemType = ArmorCraftingVM.GetItemType(_craftedItem);
            ArmorCraftedText = GameTexts.FindText("str_bannercraft_crafting_result", itemType.ToString().ToLower()).ToString();
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