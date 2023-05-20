using System;
using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerCraft
{
	public class ArmorClassSelectionPopupVM : ViewModel
    {
        private MBBindingList<ArmorClassVM> _armorClasses;

        private Action<int> _onSelect;

        private string _popupHeader;

        private bool _isVisible;

        private List<TextObject> _templatesList;

        [DataSourceProperty]
        public string PopupHeader
        {
            get
            {
                return _popupHeader;
            }
            set
            {
                if (value != _popupHeader)
                {
                    _popupHeader = value;
                    OnPropertyChangedWithValue(value, "PopupHeader");
                }
            }
        }

        [DataSourceProperty]
        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                if (value != _isVisible)
                {
                    _isVisible = value;
                    OnPropertyChangedWithValue(value, "IsVisible");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<ArmorClassVM> ArmorClasses
        {
            get
            {
                return _armorClasses;
            }
            set
            {
                if (value != _armorClasses)
                {
                    _armorClasses = value;
                    OnPropertyChangedWithValue(value, "ArmorClasses");
                }
            }
        }

        public ArmorClassSelectionPopupVM(List<TextObject> templatesList, Action<int> onSelect)
        {
            ArmorClasses = new MBBindingList<ArmorClassVM>();
            _onSelect = onSelect;
            _templatesList = templatesList;

            foreach (TextObject templates in _templatesList)
            {
                ArmorClasses.Add(new ArmorClassVM(_templatesList.IndexOf(templates), templates, ExecuteSelectArmorClass));
            }

            RefreshList();
            RefreshValues();
        }

        private void RefreshList()
        {
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            PopupHeader = new TextObject("{=wZGj3qO1}Choose What to Craft").ToString();
        }

        public void ExecuteSelectArmorClass(int index)
        {
            if (ArmorClasses[index].IsSelected)
            {
                ExecuteClosePopup();
                return;
            }

            _onSelect?.Invoke(index);
            ExecuteClosePopup();
        }

        public void ExecuteClosePopup()
        {
            IsVisible = false;
        }

        public void ExecuteOpenPopup()
        {
            IsVisible = true;
            RefreshList();
        }
    }
}
