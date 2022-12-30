using System;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerCraft
{
	public class ArmorClassVM : ViewModel
	{
		private TextObject _templateTextObject;

		private Action<int> _onSelect;

		private string _templateName;

		private bool _isSelected;

		private int _selectionIndex;

		[DataSourceProperty]
		public string TemplateName
		{
			get => _templateName;
			set
			{
				if (value != _templateName)
				{
					_templateName = value;
					OnPropertyChangedWithValue(value, "TemplateName");
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
		public int SelectionIndex
		{
			get => _selectionIndex;
			set
			{
				if (value != _selectionIndex)
				{
					_selectionIndex = value;
					OnPropertyChangedWithValue(value, "SelectionIndex");
				}
			}
		}

		public ArmorTemplate Template { get; }

		public ArmorClassVM(int selectionIndex, TextObject templateTextObject, Action<int> onSelect)
		{
			_onSelect = onSelect;
			SelectionIndex = selectionIndex;
			_templateTextObject = templateTextObject;

			RefreshValues();
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