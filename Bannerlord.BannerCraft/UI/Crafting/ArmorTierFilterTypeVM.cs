using System;
using TaleWorlds.Library;

namespace BannerCraft
{
    public class ArmorTierFilterTypeVM : ViewModel
    {
        private readonly Action<ArmorCraftingVM.ArmorPieceTierFilter> _onSelect;

        private bool _isSelected;

        private string _tierName;

        public ArmorCraftingVM.ArmorPieceTierFilter FilterType { get; }

        [DataSourceProperty]
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
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
        public string TierName
        {
            get
            {
                return _tierName;
            }
            set
            {
                if (value != _tierName)
                {
                    _tierName = value;
                    OnPropertyChangedWithValue(value, "TierName");
                }
            }
        }

        public ArmorTierFilterTypeVM(ArmorCraftingVM.ArmorPieceTierFilter filterType, Action<ArmorCraftingVM.ArmorPieceTierFilter> onSelect, string tierName)
        {
            FilterType = filterType;
            _onSelect = onSelect;
            TierName = tierName;
        }

        public void ExecuteSelectTier()
        {
            _onSelect?.Invoke(FilterType);
        }
    }
}