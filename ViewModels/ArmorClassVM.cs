using System;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Bannerlord.BannerCraft.ViewModels
{
    public class ArmorClassVM : ViewModel
    {
        private TextObject _templateTextObject;

        private Action<int> _onSelect;

        private string _templateName;

        private bool _isSelected;

        private int _selectionIndex;

        public ArmorClassVM(int selectionIndex, TextObject templateTextObject, Action<int> onSelect)
        {
            _onSelect = onSelect;
            SelectionIndex = selectionIndex;
            _templateTextObject = templateTextObject;

            RefreshValues();
        }

        [DataSourceProperty]
        public string TemplateName
        {
            get => _templateName;
            set => SetField(ref _templateName, value, nameof(TemplateName));
        }

        [DataSourceProperty]
        public bool IsSelected
        {
            get => _isSelected;
            set => SetField(ref _isSelected, value, nameof(IsSelected));
        }

        [DataSourceProperty]
        public int SelectionIndex
        {
            get => _selectionIndex;
            set => SetField(ref _selectionIndex, value, nameof(SelectionIndex));
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            TemplateName = _templateTextObject.ToString();
        }

        public void ExecuteSelect()
        {
            _onSelect?.Invoke(SelectionIndex);
        }
    }
}