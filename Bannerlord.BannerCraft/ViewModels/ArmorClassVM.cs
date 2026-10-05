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

        private string _iconSprite;

        private bool _isSelected;

        private int _selectionIndex;

        public ArmorClassVM(int selectionIndex, TextObject templateTextObject, ItemType itemType, Action<int> onSelect)
        {
            _onSelect = onSelect;
            SelectionIndex = selectionIndex;
            _templateTextObject = templateTextObject;
            _iconSprite = GetIconSprite(itemType);

            RefreshValues();
        }

        [DataSourceProperty]
        public string TemplateName
        {
            get => _templateName;
            set => SetField(ref _templateName, value, nameof(TemplateName));
        }

        [DataSourceProperty]
        public string IconSprite
        {
            get => _iconSprite;
            set => SetField(ref _iconSprite, value, nameof(IconSprite));
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

        // Vanilla equipment type icons, they're in an always loaded sprite category.
        private static string GetIconSprite(ItemType itemType)
        {
            string icon = itemType switch
            {
                ItemType.HeadArmor => "head_armor",
                ItemType.ShoulderArmor => "cape",
                ItemType.BodyArmor => "body_armor",
                ItemType.ArmArmor => "hand_armor",
                ItemType.LegArmor => "leg_armor",
                ItemType.Barding => "mount",
                ItemType.Shield => "shield",
                ItemType.Bow => "bow",
                ItemType.Crossbow => "crossbow",
                ItemType.Arrows => "quiver",
                ItemType.Bolts => "quiver",
                ItemType.Banner => "banner",
                ItemType.OneHandedWeapon => "one_handed",
                ItemType.TwoHandedWeapon => "two_handed",
                ItemType.Polearm => "polearm",
                ItemType.Thrown => "throwing",
                _ => "default"
            };

            return "General\\EquipmentIcons\\equipment_type_" + icon;
        }
    }
}
