using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bannerlord.BannerCraft.Mixins;
using Bannerlord.BannerCraft.Models;
using Bannerlord.UIExtenderEx.Attributes;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting.WeaponDesign;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Bannerlord.BannerCraft.ViewModels
{
    public class ArmorCraftingVM : ViewModel
    {
        public class TierComparer : IComparer<ArmorItemVM>
        {
            public int Compare(ArmorItemVM x, ArmorItemVM y)
            {
                if (x.Tier != y.Tier)
                {
                    return x.Tier.CompareTo(y.Tier);
                }

                return x.ItemName.CompareTo(y.ItemName);
            }
        }

        private readonly CraftingMixin _mixin;
        private readonly CraftingVM _craftingVm;
        private readonly Crafting _crafting;

        private readonly TierComparer _tierComparer;

        private ItemType _selectedItemType;

        private ArmorClassSelectionPopupVM _armorClassSelectionPopup;

        private string _chooseArmorTypeText;
        private string _currentCraftedArmorTypeText;

        private MBBindingList<ArmorTierFilterTypeVM> _tierFilters;

        private MBBindingList<ArmorItemVM> _armors;
        private ArmorItemVM _currentItem;
        private int _selectedPieceTypeIndex;
        private ArmorPieceTierFlag _currentTierFilter;

        private ItemCollectionElementViewModel _itemVisualModel;
        private MBBindingList<CraftingListPropertyItem> _itemProperties;

        private MBBindingList<ItemFlagVM> _itemFlagIconsList;

        private string _difficultyText;

        private string _currentDifficultyText;

        private string _currentCraftingSkillValueText;

        private int _currentHeroCraftingSkill;

        private int _maxDifficulty;

        private int _currentDifficulty;

        private bool _isCurrentHeroAtMaxCraftingSkill;

        private TextObject _currentCraftingSkillValueTextObj;

        private TextObject _currentDifficultyTextObj;

        private string _itemSearchText;

        [DataSourceProperty]
        public ArmorClassSelectionPopupVM ArmorClassSelectionPopup
        {
            get => _armorClassSelectionPopup;
            set => SetField(ref _armorClassSelectionPopup, value, nameof(ArmorClassSelectionPopup));
        }

        [DataSourceProperty]
        public string ChooseArmorTypeText
        {
            get => _chooseArmorTypeText;
            set => SetField(ref _chooseArmorTypeText, value, nameof(ChooseArmorTypeText));
        }

        [DataSourceProperty]
        public string CurrentCraftedArmorTypeText
        {
            get => _currentCraftedArmorTypeText;
            set => SetField(ref _currentCraftedArmorTypeText, value, nameof(CurrentCraftedArmorTypeText));
        }

        [DataSourceProperty]
        public MBBindingList<ArmorTierFilterTypeVM> TierFilters
        {
            get => _tierFilters;
            set => SetField(ref _tierFilters, value, nameof(TierFilters));
        }

        [DataSourceProperty]
        public MBBindingList<ArmorItemVM> Armors
        {
            get => _armors;
            set => SetField(ref _armors, value, nameof(Armors));
        }

        [DataSourceProperty]
        public int SelectedPieceTypeIndex
        {
            get => _selectedPieceTypeIndex;
            set => SetField(ref _selectedPieceTypeIndex, value, nameof(SelectedPieceTypeIndex));
        }

        [DataSourceProperty]
        public ItemCollectionElementViewModel ItemVisualModel
        {
            get => _itemVisualModel;
            set => SetField(ref _itemVisualModel, value, nameof(ItemVisualModel));
        }

        [DataSourceProperty]
        public MBBindingList<CraftingListPropertyItem> ItemProperties
        {
            get => _itemProperties;
            set => SetField(ref _itemProperties, value, nameof(ItemProperties));
        }

        [DataSourceProperty]
        public MBBindingList<ItemFlagVM> ItemFlagIconsList
        {
            get => _itemFlagIconsList;
            set => SetField(ref _itemFlagIconsList, value, nameof(ItemFlagIconsList));
        }

        [DataSourceProperty]
        public string DifficultyText
        {
            get => _difficultyText;
            set => SetField(ref _difficultyText, value, nameof(DifficultyText));
        }

        [DataSourceProperty]
        public string CurrentDifficultyText
        {
            get => _currentDifficultyText;
            set => SetField(ref _currentDifficultyText, value, nameof(CurrentDifficultyText));
        }

        [DataSourceProperty]
        public string CurrentCraftingSkillValueText
        {
            get => _currentCraftingSkillValueText;
            set => SetField(ref _currentCraftingSkillValueText, value, nameof(CurrentCraftingSkillValueText));
        }

        [DataSourceProperty]
        public int CurrentHeroCraftingSkill
        {
            get => _currentHeroCraftingSkill;
            set => SetField(ref _currentHeroCraftingSkill, value, nameof(CurrentHeroCraftingSkill));
        }

        [DataSourceProperty]
        public int MaxDifficulty
        {
            get => _maxDifficulty;
            set => SetField(ref _maxDifficulty, value, nameof(MaxDifficulty));
        }

        [DataSourceProperty]
        public int CurrentDifficulty
        {
            get => _currentDifficulty;
            set => SetField(ref _currentDifficulty, value, nameof(CurrentDifficulty));
        }

        [DataSourceProperty]
        public bool IsCurrentHeroAtMaxCraftingSkill
        {
            get => _isCurrentHeroAtMaxCraftingSkill;
            set => SetField(ref _isCurrentHeroAtMaxCraftingSkill, value, nameof(IsCurrentHeroAtMaxCraftingSkill));
        }

        [DataSourceProperty]
        public ArmorItemVM CurrentItem
        {
            get => _currentItem;
            set
            {
                if (value != _currentItem)
                {
                    _currentItem = value;
                    OnPropertyChangedWithValue(value, "CurrentItem");
                    RefreshSecondaryUsages();
                    _mixin.OnRefresh();
                }
            }
        }

        [DataSourceProperty]
        public string ItemSearchText
        {
            get => _itemSearchText;
            set
            {
                if (value != _itemSearchText)
                {
                    _itemSearchText = value;
                    OnPropertyChangedWithValue(value, "ItemSearchText");
                    RefreshValues();
                }
            }
        }

        public static bool ItemTypeIsWeapon(ItemType itemType)
        {
            if (itemType == ItemType.OneHandedWeapon
                || itemType == ItemType.TwoHandedWeapon
                || itemType == ItemType.Polearm
                || itemType == ItemType.Thrown)
            {
                return true;
            }

            return false;
        }

        public static bool AllowItemType(ItemType itemType)
        {
            if (!Settings.Instance.AllowCraftingNormalWeapons)
            {
                if (ItemTypeIsWeapon(itemType))
                {
                    return false;
                }
            }
            return true;
        }

        public ArmorCraftingVM(CraftingMixin mixin, CraftingVM craftingVm, Crafting crafting)
        {
            _mixin = mixin;
            _craftingVm = craftingVm;
            _crafting = crafting;

            _tierComparer = new TierComparer();

            _selectedItemType = ItemType.HeadArmor;

            var armorClasses = new List<TextObject>();
            foreach (ItemType value in Enum.GetValues(typeof(ItemType)))
            {
                if (value == ItemType.Invalid)
                {
                    continue;
                }

                if (!AllowItemType(value))
                {
                    continue;
                }

                armorClasses.Add(GameTexts.FindText("str_bannercraft_crafting_itemtype", value.ToString().ToLower()));
            }

            ArmorClassSelectionPopup = new ArmorClassSelectionPopupVM(armorClasses, delegate (int x)
            {
                RefreshArmorDesignMode(x);
            });

            ChooseArmorTypeText = new TextObject("{=Gd6zuUwh}Free Build").ToString();

            TierFilters = new MBBindingList<ArmorTierFilterTypeVM>
            {
                new ArmorTierFilterTypeVM(ArmorPieceTierFlag.All, OnSelectPieceTierFilter, GameTexts.FindText("str_crafting_tier_filter_all").ToString()),
                new ArmorTierFilterTypeVM(ArmorPieceTierFlag.Tier0, OnSelectPieceTierFilter, 0.ToString()),
                new ArmorTierFilterTypeVM(ArmorPieceTierFlag.Tier1, OnSelectPieceTierFilter, GameTexts.FindText("str_tier_one").ToString()),
                new ArmorTierFilterTypeVM(ArmorPieceTierFlag.Tier2, OnSelectPieceTierFilter, GameTexts.FindText("str_tier_two").ToString()),
                new ArmorTierFilterTypeVM(ArmorPieceTierFlag.Tier3, OnSelectPieceTierFilter, GameTexts.FindText("str_tier_three").ToString()),
                new ArmorTierFilterTypeVM(ArmorPieceTierFlag.Tier4, OnSelectPieceTierFilter, GameTexts.FindText("str_tier_four").ToString()),
                new ArmorTierFilterTypeVM(ArmorPieceTierFlag.Tier5, OnSelectPieceTierFilter, GameTexts.FindText("str_tier_five").ToString()),
                new ArmorTierFilterTypeVM(ArmorPieceTierFlag.Tier6, OnSelectPieceTierFilter, Common.ToRoman(6))
            };

            Armors = new MBBindingList<ArmorItemVM>();

            ItemVisualModel = new ItemCollectionElementViewModel();

            ItemProperties = new MBBindingList<CraftingListPropertyItem>();
            DesignResultPropertyList = new MBBindingList<WeaponDesignResultPropertyItemVM>();

            ItemFlagIconsList = new MBBindingList<ItemFlagVM>();

            _currentCraftingSkillValueTextObj = new TextObject("{=LEiZWuZm}{SKILL_NAME}: {SKILL_VALUE}");
            _currentDifficultyTextObj = new TextObject("{=cbbUzYX3}Difficulty: {DIFFICULTY}");

            MaxDifficulty = 300;

            SecondaryUsageSelector = new SelectorVM<CraftingSecondaryUsageItemVM>(new List<string>(), 0, null);

            BannerDescriptionText = "";

            UpdateTierFilterFlags(ArmorPieceTierFlag.All);

            RefreshArmorDesignMode(0);
        }

        [DataSourceMethod]
        public void ExecuteOpenArmorClassSelectionPopup()
        {
            ArmorClassSelectionPopup.ExecuteOpenPopup();
        }

        public void OnCraftingHeroChanged(CraftingAvailableHeroItemVM newHero)
        {
            RefreshCurrentHeroSkillLevel();
            RefreshDifficulty();
        }

        private bool _inBannerMode;

        private string _bannerDescriptionText;

        [DataSourceProperty]
        public bool InBannerMode
        {
            get => _inBannerMode;
            private set
            {
                if (value != _inBannerMode)
                {
                    _inBannerMode = value;
                    OnPropertyChangedWithValue(value, "InBannerMode");
                }
            }
        }

        [DataSourceProperty]
        public string BannerDescriptionText
        {
            get => _bannerDescriptionText;
            set => SetField(ref _bannerDescriptionText, value, nameof(BannerDescriptionText));
        }

        private void RefreshArmorDesignMode(int classIndex)
        {
            _selectedItemType = GetItemType(classIndex);

            CurrentCraftedArmorTypeText = GameTexts.FindText("str_bannercraft_crafting_itemtype", _selectedItemType.ToString().ToLower()).ToString();

            RefreshValues();
            RefreshCurrentHeroSkillLevel();

            RefreshDifficulty();

            RefreshSecondaryUsages();

            InBannerMode = false;
            if (GetItemType(classIndex) == ItemType.Banner)
            {
                InBannerMode = true;
            }
        }

        private void RefreshSecondaryUsages()
        {
            int usageIndex = SecondaryUsageSelector?.SelectedIndex ?? 0;
            SecondaryUsageSelector.Refresh(new List<string>(), 0, UpdateSecondaryUsageIndex);

            if (CurrentItem != null && CurrentItem.Item.Weapons != null)
            {
                int num = 0;
                for (int i = 0; i < CurrentItem.Item.Weapons.Count; i++)
                {
                    if (CampaignUIHelper.IsItemUsageApplicable(CurrentItem.Item.Weapons[i]))
                    {
                        TextObject name = GameTexts.FindText("str_weapon_usage", CurrentItem.Item.Weapons[i].WeaponDescriptionId);
                        SecondaryUsageSelector.AddItem(new CraftingSecondaryUsageItemVM(name, num, i, SecondaryUsageSelector));

                        num++;
                    }
                }
                TrySetSecondaryUsageIndex(usageIndex);
            }
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            var baseSmithingModel = Campaign.Current.Models.SmithingModel;

            Armors.Clear();

            if (baseSmithingModel is BannerCraftSmithingModel smithingModel)
                foreach (var item in Game.Current.ObjectManager.GetObjectTypeList<ItemObject>())
                {
                    ItemType itemType = GetItemType(item);
                    if (itemType == ItemType.Invalid
                        || itemType != _selectedItemType
                        || item.IsCraftedByPlayer)
                    {
                        continue;
                    }

                    if (((int)_currentTierFilter & 1 << (int)item.Tier + 1) == 0)
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(ItemSearchText)
                        && item.Name.ToString().IndexOf(ItemSearchText, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }

                    Armors.Add(new ArmorItemVM(this, item, smithingModel.CalculateArmorDifficulty(item), itemType));
                }

            if (!AllowItemType(_selectedItemType))
            {
                Armors.Clear();
            }

            Armors.Sort(_tierComparer);

            if (Armors.Count > 0)
            {
                CurrentItem = Armors[0];
                CurrentItem.ExecuteSelect();
            }
            else
            {
                CurrentItem = null;
                RefreshStats(ItemType.Invalid);
            }

            ItemProperties.ApplyActionOnAllItems(delegate (CraftingListPropertyItem x)
            {
                x.RefreshValues();
            });
        }

        private void RefreshCurrentHeroSkillLevel()
        {
            CurrentHeroCraftingSkill = _craftingVm.CurrentCraftingHero.Hero.CharacterObject.GetSkillValue(DefaultSkills.Crafting);
            IsCurrentHeroAtMaxCraftingSkill = CurrentHeroCraftingSkill >= 300;
            _currentCraftingSkillValueTextObj.SetTextVariable("SKILL_VALUE", CurrentHeroCraftingSkill);
            CurrentCraftingSkillValueText = _currentCraftingSkillValueTextObj.ToString();
        }

        private void RefreshDifficulty()
        {
            CurrentDifficulty = CurrentItem?.Difficulty ?? 0;

            _currentCraftingSkillValueTextObj.SetTextVariable("SKILL_VALUE", CurrentHeroCraftingSkill);
            _currentCraftingSkillValueTextObj.SetTextVariable("SKILL_NAME", DefaultSkills.Crafting.Name);
            CurrentCraftingSkillValueText = _currentCraftingSkillValueTextObj.ToString();

            _currentDifficultyTextObj.SetTextVariable("DIFFICULTY", CurrentDifficulty);
            CurrentDifficultyText = _currentDifficultyTextObj.ToString();

            DifficultyText = GameTexts.FindText("str_difficulty").ToString();
        }

        //delegate to decide how to access the fields of the item due to the fact that the fields are private in versions prior to 1.2.0
        private delegate T GetItemFieldDelegate<T>(EquipmentElement item, string _fieldName);
        GetItemFieldDelegate<int> getItemFieldDelegateInstanceInt;
        GetItemFieldDelegate<short> getItemFieldDelegateInstanceShort;
        //If original value is higher than modifier value, then the result will be the modifier value
        //If the original value is lower or equal to the modifier value, then the result will be original value minus 1
        
#if v120 || v121 || v122 || v123
        //they were made public in these verions
        private int GetItemFieldInt(EquipmentElement item, string _fieldName)
        {
            var value = item.GetType()?.GetField(_fieldName)?.GetValue(item,null);
            if (value is not null)
                return (int) value;
            else return 0;
        }
        private short GetItemFieldShort(EquipmentElement item, string _fieldName)
        {
            var value = item.GetType()?.GetField(_fieldName)?.GetValue(item,null);
            if (value is not null)
                return (short) value;
            else return 0;
        }
#else
        private int GetItemFieldInt(EquipmentElement item, string _fieldName)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            var value = item.ItemModifier.GetType()?.GetField(_fieldName, bindingFlags)?.GetValue(item.ItemModifier);
            if (value is not null)
                return (int) value;
            else return 0;
        }
        private short GetItemFieldShort(EquipmentElement item, string _fieldName)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            var value = item.ItemModifier.GetType()?.GetField(_fieldName, bindingFlags)?.GetValue(item.ItemModifier);
            if (value is not null)
                return (short)value;
            else return 0;
        }
#endif
        private List<int> GenerateModifierValues(ItemType itemType, EquipmentElement element)
        {

            getItemFieldDelegateInstanceInt = GetItemFieldInt;
            getItemFieldDelegateInstanceShort = GetItemFieldShort;
            string _armor;
            string _hitPoints;
            string _speed;
            string _damage;
            string _missileSpeed;
            string _stackCount;


#if v120 || v121 || v122 || v123
            _armor = "Armor";

#else
            _armor = "_armor";
            _hitPoints = "_hitPoints";
            _speed = "_speed";
            _damage = "_damage";
            _missileSpeed = "_missileSpeed";
            _stackCount = "_stackCount";
#endif
            /*
			 * This is a very fragile function that should be refactored alongside RefreshStats
			 * But not right now
			 */
            if (itemType == ItemType.Invalid)
            {
                return null;
            }

            List<int> ret = new List<int>
            {
				/*
				 * Weight is always the first element in the stats list and it can't be changed from the modifier
				*/
				0
            };

            /*
			 * Once again we need to get private values
			 */
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            switch (itemType)
            {
                case ItemType.Invalid:
                    /*
					 * Never reached, so let's use this to declare switch local variables
					 */
                    int armor;

                    int shieldSpeed;
                    short shieldHitPoints;
                    break;

                case ItemType.HeadArmor:
                case ItemType.ShoulderArmor:
                case ItemType.BodyArmor:
                case ItemType.ArmArmor:
                case ItemType.LegArmor:
                    armor = 0;
                    if (element.ItemModifier != null)
                    {
                        armor = getItemFieldDelegateInstanceInt(element, _armor);
                        //armor = (int)element.ItemModifier?.GetType().GetField(_armor, bindingFlags)?.GetValue(element.ItemModifier);
                    }
                    //Subtract armor from this because we dont wan't negative values...
                    ret.Add(CurrentItem.Item.ArmorComponent.HeadArmor > 0 ? armor : 0);
                    ret.Add(CurrentItem.Item.ArmorComponent.BodyArmor > 0 ? armor : 0);
                    ret.Add(CurrentItem.Item.ArmorComponent.LegArmor > 0 ? armor : 0);
                    ret.Add(CurrentItem.Item.ArmorComponent.ArmArmor > 0 ? armor : 0);

                    break;

                case ItemType.Barding:
                    armor = 0;
                    if (element.ItemModifier != null)
                    {
                        armor = (int)element.ItemModifier.GetType().GetField(_armor, bindingFlags).GetValue(element.ItemModifier);
                    }

                    ret.Add(armor);

                    break;

                case ItemType.Shield:
                    shieldSpeed = 0;
                    shieldHitPoints = 0;
                    if (element.ItemModifier != null)
                    {
                        shieldSpeed = (int)element.ItemModifier.GetType().GetField(_speed, bindingFlags).GetValue(element.ItemModifier);
                        shieldHitPoints = (short)element.ItemModifier.GetType().GetField(_hitPoints, bindingFlags).GetValue(element.ItemModifier);
                    }

                    ret.Add(shieldSpeed);
                    ret.Add(shieldHitPoints);

                    break;

                case ItemType.Bow:
                case ItemType.Crossbow:
                    int speed = 0;
                    int missileSpeed = 0;
                    int missileDamage = 0;
                    if (element.ItemModifier != null)
                    {
                        speed = (int)element.ItemModifier.GetType().GetField(_speed, bindingFlags).GetValue(element.ItemModifier);
                        missileSpeed = (int)element.ItemModifier.GetType().GetField(_missileSpeed, bindingFlags).GetValue(element.ItemModifier);
                        missileDamage = (int)element.ItemModifier.GetType().GetField(_damage, bindingFlags).GetValue(element.ItemModifier);
                    }

                    ret.Add(speed);
                    ret.Add(missileDamage);
                    ret.Add(0); // Accuracy can't be changed here for some stupid reason
                    ret.Add(missileSpeed);

                    if (itemType == ItemType.Crossbow)
                    {
                        ret.Add(0); // Ammo limit can't be changed
                    }

                    break;

                case ItemType.Arrows:
                case ItemType.Bolts:
                    missileDamage = 0;
                    short stackCount = 0;
                    if (element.ItemModifier != null)
                    {
                        missileDamage = (int)element.ItemModifier.GetType().GetField(_damage, bindingFlags).GetValue(element.ItemModifier);
                        stackCount = (short)element.ItemModifier.GetType().GetField(_stackCount, bindingFlags).GetValue(element.ItemModifier);
                    }

                    ret.Add(missileDamage);
                    ret.Add(stackCount);

                    break;

                case ItemType.Banner:

                    break;

                case ItemType.OneHandedWeapon:
                case ItemType.TwoHandedWeapon:
                case ItemType.Polearm:
                case ItemType.Thrown:
                    WeaponComponentData weaponData = element.Item.PrimaryWeapon;
                    ret.Add(0); // Can't change weapon reach
                    if (weaponData.IsMeleeWeapon)
                    {
                        if (weaponData.ThrustDamageType != DamageTypes.Invalid
                            && weaponData.ThrustDamage > 0)
                        {
                            speed = 0;
                            if (element.ItemModifier != null)
                            {
                                speed = (int)element.ItemModifier.GetType().GetField(_speed, bindingFlags).GetValue(element.ItemModifier);
                            }
                            ret.Add(speed);
                        }

                        if (weaponData.SwingDamageType != DamageTypes.Invalid
                            && weaponData.SwingDamage > 0)
                        {
                            speed = 0;
                            if (element.ItemModifier != null)
                            {
                                speed = (int)element.ItemModifier.GetType().GetField(_speed, bindingFlags).GetValue(element.ItemModifier);
                            }
                            ret.Add(speed);
                        }

                        if (weaponData.ThrustDamageType != DamageTypes.Invalid
                            && weaponData.ThrustDamage > 0)
                        {
                            int damage = 0;
                            if (element.ItemModifier != null)
                            {
                                damage = (int)element.ItemModifier.GetType().GetField(_damage, bindingFlags).GetValue(element.ItemModifier);
                            }
                            ret.Add(damage);
                        }

                        if (weaponData.SwingDamageType != DamageTypes.Invalid
                            && weaponData.SwingDamage > 0)
                        {
                            int damage = 0;
                            if (element.ItemModifier != null)
                            {
                                damage = (int)element.ItemModifier.GetType().GetField(_damage, bindingFlags).GetValue(element.ItemModifier);
                            }
                            ret.Add(damage);
                        }

                        ret.Add(0); // Can't change handling
                    }
                    else if (weaponData.IsRangedWeapon)
                    {
                        int damage = 0;
                        missileSpeed = 0;
                        stackCount = 0;
                        if (element.ItemModifier != null)
                        {
                            damage = (int)element.ItemModifier.GetType().GetField(_damage, bindingFlags).GetValue(element.ItemModifier);
                            missileSpeed = (int)element.ItemModifier.GetType().GetField(_missileSpeed, bindingFlags).GetValue(element.ItemModifier);
                            stackCount = (short)element.ItemModifier.GetType().GetField(_stackCount, bindingFlags).GetValue(element.ItemModifier);
                        }
                        ret.Add(damage);
                        ret.Add(missileSpeed);
                        ret.Add(0); // Can't change accuracy
                        ret.Add(stackCount);
                    }

                    break;
            }

            return ret;
        }

        public void RefreshStats(ItemType itemType)
        {
            ItemProperties.Clear();
            ItemFlagIconsList.Clear();

            RefreshDifficulty();

            if (itemType == ItemType.Invalid)
            {
                return;
            }

            TextObject weightDescriptionText = GameTexts.FindText("str_crafting_stat", "Weight"); ;
            switch (itemType)
            {
                case ItemType.Invalid:
                    /*
					 * Never reached, so let's use this to declare switch local variables
					 */

                    CraftingListPropertyItem itemProperty;
                    break;

                case ItemType.HeadArmor:
                case ItemType.ShoulderArmor:
                case ItemType.BodyArmor:
                case ItemType.ArmArmor:
                case ItemType.LegArmor:
                    TextObject headDescriptionText = GameTexts.FindText("str_bannercraft_crafting_statdisplay", ItemType.HeadArmor.ToString().ToLower());
                    TextObject bodyDescriptionText = GameTexts.FindText("str_bannercraft_crafting_statdisplay", ItemType.BodyArmor.ToString().ToLower());
                    TextObject legDescriptionText = GameTexts.FindText("str_bannercraft_crafting_statdisplay", ItemType.LegArmor.ToString().ToLower());
                    TextObject armDescriptionText = GameTexts.FindText("str_bannercraft_crafting_statdisplay", ItemType.ArmArmor.ToString().ToLower());

                    itemProperty = new CraftingListPropertyItem(weightDescriptionText, 50f, CurrentItem.Item.Weight, 0f, CraftingTemplate.CraftingStatTypes.Weight)
                    {
                        IsValidForUsage = true
                    };
                    ItemProperties.Add(itemProperty);

                    /*
					 * Use CraftingTemplate.CraftingStatTypes.StackAmount since it's the only one that is always displayed as an integer
					 */
                    itemProperty = new CraftingListPropertyItem(headDescriptionText, 100f, CurrentItem.Item.ArmorComponent.HeadArmor, 0f, CraftingTemplate.CraftingStatTypes.StackAmount)
                    {
                        IsValidForUsage = true
                    };
                    ItemProperties.Add(itemProperty);

                    itemProperty = new CraftingListPropertyItem(bodyDescriptionText, 100f, CurrentItem.Item.ArmorComponent.BodyArmor, 0f, CraftingTemplate.CraftingStatTypes.StackAmount)
                    {
                        IsValidForUsage = true
                    };
                    ItemProperties.Add(itemProperty);

                    itemProperty = new CraftingListPropertyItem(legDescriptionText, 100f, CurrentItem.Item.ArmorComponent.LegArmor, 0f, CraftingTemplate.CraftingStatTypes.StackAmount)
                    {
                        IsValidForUsage = true
                    };
                    ItemProperties.Add(itemProperty);

                    /*
					 * Armor is shown Head Body Leg Arm in item hints in the vanilla UI
					 * It's ordered Head Body Arm Leg in the inventory totals, but who needs consistency
					 */
                    itemProperty = new CraftingListPropertyItem(armDescriptionText, 100f, CurrentItem.Item.ArmorComponent.ArmArmor, 0f, CraftingTemplate.CraftingStatTypes.StackAmount)
                    {
                        IsValidForUsage = true
                    };
                    ItemProperties.Add(itemProperty);

                    break;

                case ItemType.Barding:
                    TextObject horseDescriptionText = GameTexts.FindText("str_bannercraft_crafting_statdisplay", ItemType.Barding.ToString().ToLower());

                    itemProperty = new CraftingListPropertyItem(weightDescriptionText, 150f, CurrentItem.Item.Weight, 0f, CraftingTemplate.CraftingStatTypes.Weight)
                    {
                        IsValidForUsage = true
                    };
                    ItemProperties.Add(itemProperty);

                    itemProperty = new CraftingListPropertyItem(horseDescriptionText, 100f, CurrentItem.Item.ArmorComponent.BodyArmor, 0f, CraftingTemplate.CraftingStatTypes.Weight)
                    {
                        IsValidForUsage = true
                    };
                    ItemProperties.Add(itemProperty);

                    break;

                case ItemType.Shield:
                    TextObject shieldSpeedDescriptionText = GameTexts.FindText("str_bannercraft_crafting_statdisplay", "speed");
                    TextObject shieldHitPointsDescriptionText = GameTexts.FindText("str_bannercraft_crafting_statdisplay", "shield_hitpoints");

                    itemProperty = new CraftingListPropertyItem(weightDescriptionText, 10f, CurrentItem.Item.Weight, 0f, CraftingTemplate.CraftingStatTypes.Weight)
                    {
                        IsValidForUsage = true
                    };
                    ItemProperties.Add(itemProperty);

                    itemProperty = new CraftingListPropertyItem(shieldSpeedDescriptionText, 150f, CurrentItem.Item.PrimaryWeapon.Handling, 0f, CraftingTemplate.CraftingStatTypes.Handling)
                    {
                        IsValidForUsage = true
                    };
                    ItemProperties.Add(itemProperty);

                    itemProperty = new CraftingListPropertyItem(shieldHitPointsDescriptionText, 600f, CurrentItem.Item.PrimaryWeapon.MaxDataValue, 0f, CraftingTemplate.CraftingStatTypes.StackAmount)
                    {
                        IsValidForUsage = true
                    };
                    ItemProperties.Add(itemProperty);

                    break;

                case ItemType.Bow:
                case ItemType.Crossbow:
                    TextObject rangedWeaponSpeedDescriptionText = GameTexts.FindText("str_bannercraft_crafting_statdisplay", "ranged_weapon_speed");
                    TextObject missileDamageDescriptionText = GameTexts.FindText("str_crafting_stat", "MissileDamage");
                    TextObject accuracyDescriptionText = GameTexts.FindText("str_crafting_stat", "Accuracy");
                    TextObject missileSpeedDescriptionText = GameTexts.FindText("str_crafting_stat", "MissileSpeed");

                    itemProperty = new CraftingListPropertyItem(weightDescriptionText, 10f, CurrentItem.Item.Weight, 0f, CraftingTemplate.CraftingStatTypes.Weight)
                    {
                        IsValidForUsage = true
                    };
                    ItemProperties.Add(itemProperty);

                    itemProperty = new CraftingListPropertyItem(rangedWeaponSpeedDescriptionText, 150f, CurrentItem.Item.PrimaryWeapon.SwingSpeed, 0f, CraftingTemplate.CraftingStatTypes.SwingSpeed)
                    {
                        IsValidForUsage = true
                    };
                    ItemProperties.Add(itemProperty);

                    itemProperty = new CraftingListPropertyItem(missileDamageDescriptionText, 150f, CurrentItem.Item.PrimaryWeapon.MissileDamage, 0f, CraftingTemplate.CraftingStatTypes.MissileDamage)
                    {
                        IsValidForUsage = true
                    };
                    ItemProperties.Add(itemProperty);

                    itemProperty = new CraftingListPropertyItem(accuracyDescriptionText, 150f, CurrentItem.Item.PrimaryWeapon.Accuracy, 0f, CraftingTemplate.CraftingStatTypes.Accuracy)
                    {
                        IsValidForUsage = true
                    };
                    ItemProperties.Add(itemProperty);

                    itemProperty = new CraftingListPropertyItem(missileSpeedDescriptionText, 150f, CurrentItem.Item.PrimaryWeapon.MissileSpeed, 0f, CraftingTemplate.CraftingStatTypes.MissileSpeed)
                    {
                        IsValidForUsage = true
                    };
                    ItemProperties.Add(itemProperty);

                    if (itemType == ItemType.Crossbow)
                    {
                        TextObject ammoLimitDescriptionText = GameTexts.FindText("str_bannercraft_crafting_statdisplay", "ammo_limit");

                        itemProperty = new CraftingListPropertyItem(ammoLimitDescriptionText, 3f, CurrentItem.Item.PrimaryWeapon.MaxDataValue, 0f, CraftingTemplate.CraftingStatTypes.StackAmount)
                        {
                            IsValidForUsage = true
                        };
                        ItemProperties.Add(itemProperty);
                    }

                    break;

                case ItemType.Arrows:
                case ItemType.Bolts:
                    missileDamageDescriptionText = GameTexts.FindText("str_crafting_stat", "MissileDamage");
                    TextObject ammoStackAmountDescriptionText = GameTexts.FindText("str_crafting_stat", "StackAmount");

                    itemProperty = new CraftingListPropertyItem(weightDescriptionText, 100f, CurrentItem.Item.Weight, 0f, CraftingTemplate.CraftingStatTypes.Weight)
                    {
                        IsValidForUsage = true
                    };
                    ItemProperties.Add(itemProperty);

                    itemProperty = new CraftingListPropertyItem(missileDamageDescriptionText, 10f, CurrentItem.Item.WeaponComponent.PrimaryWeapon.MissileDamage, 0f, CraftingTemplate.CraftingStatTypes.MissileDamage)
                    {
                        IsValidForUsage = true
                    };
                    ItemProperties.Add(itemProperty);

                    itemProperty = new CraftingListPropertyItem(ammoStackAmountDescriptionText, 50f, CurrentItem.Item.PrimaryWeapon.MaxDataValue, 0f, CraftingTemplate.CraftingStatTypes.StackAmount)
                    {
                        IsValidForUsage = true
                    };
                    ItemProperties.Add(itemProperty);

                    break;

                case ItemType.Banner:
                    itemProperty = new CraftingListPropertyItem(weightDescriptionText, 2f, CurrentItem.Item.Weight, 0f, CraftingTemplate.CraftingStatTypes.Weight)
                    {
                        IsValidForUsage = true
                    };
                    ItemProperties.Add(itemProperty);

                    if (CurrentItem.Item.BannerComponent != null && CurrentItem.Item.BannerComponent.BannerEffect != null)
                    {
                        BannerDescriptionText = CurrentItem.Item.BannerComponent.BannerEffect.GetDescription(CurrentItem.Item.BannerComponent.BannerLevel).ToString();
                    }
                    else
                    {
                        BannerDescriptionText = "";
                    }

                    break;

                case ItemType.OneHandedWeapon:
                case ItemType.TwoHandedWeapon:
                case ItemType.Polearm:
                case ItemType.Thrown:
                    TextObject weaponReachDescriptionText = GameTexts.FindText("str_crafting_stat", "WeaponReach");
                    TextObject thrustDamageDescriptionText = GameTexts.FindText("str_crafting_stat", "ThrustDamage");
                    TextObject swingDamageDescriptionText = GameTexts.FindText("str_crafting_stat", "SwingDamage");
                    TextObject thrustSpeedDescriptionText = GameTexts.FindText("str_crafting_stat", "ThrustSpeed");
                    TextObject swingSpeedDescriptionText = GameTexts.FindText("str_crafting_stat", "SwingSpeed");
                    TextObject handlingDescriptionText = GameTexts.FindText("str_crafting_stat", "Handling");

                    missileDamageDescriptionText = GameTexts.FindText("str_crafting_stat", "MissileDamage");
                    missileSpeedDescriptionText = GameTexts.FindText("str_crafting_stat", "MissileSpeed");
                    accuracyDescriptionText = GameTexts.FindText("str_crafting_stat", "Accuracy");
                    ammoStackAmountDescriptionText = GameTexts.FindText("str_crafting_stat", "StackAmount");

                    WeaponComponentData weaponData = CurrentItem.Item.GetWeaponWithUsageIndex(SecondaryUsageSelector.SelectedIndex);

                    itemProperty = new CraftingListPropertyItem(weightDescriptionText, 15f, CurrentItem.Item.Weight, 0f, CraftingTemplate.CraftingStatTypes.Weight)
                    {
                        IsValidForUsage = true
                    };
                    ItemProperties.Add(itemProperty);

                    itemProperty = new CraftingListPropertyItem(weaponReachDescriptionText, 400f, weaponData.WeaponLength, 0f, CraftingTemplate.CraftingStatTypes.WeaponReach)
                    {
                        IsValidForUsage = true
                    };
                    ItemProperties.Add(itemProperty);

                    if (weaponData.IsMeleeWeapon)
                    {
                        if (weaponData.ThrustDamageType != DamageTypes.Invalid
                            && weaponData.ThrustDamage > 0)
                        {
                            itemProperty = new CraftingListPropertyItem(thrustSpeedDescriptionText, 150f, weaponData.ThrustSpeed, 0f, CraftingTemplate.CraftingStatTypes.ThrustSpeed)
                            {
                                IsValidForUsage = true
                            };
                            ItemProperties.Add(itemProperty);
                        }

                        if (weaponData.SwingDamageType != DamageTypes.Invalid
                            && weaponData.SwingDamage > 0)
                        {
                            itemProperty = new CraftingListPropertyItem(swingSpeedDescriptionText, 150f, weaponData.SwingSpeed, 0f, CraftingTemplate.CraftingStatTypes.SwingSpeed)
                            {
                                IsValidForUsage = true
                            };
                            ItemProperties.Add(itemProperty);
                        }

                        if (weaponData.ThrustDamageType != DamageTypes.Invalid
                            && weaponData.ThrustDamage > 0)
                        {
                            thrustDamageDescriptionText = thrustDamageDescriptionText.SetTextVariable("THRUST_DAMAGE_TYPE", GameTexts.FindText("str_inventory_dmg_type", ((int)CurrentItem.Item.PrimaryWeapon.ThrustDamageType).ToString()));
                            itemProperty = new CraftingListPropertyItem(thrustDamageDescriptionText, 200f, weaponData.ThrustDamage, 0f, CraftingTemplate.CraftingStatTypes.ThrustDamage)
                            {
                                IsValidForUsage = true
                            };
                            ItemProperties.Add(itemProperty);
                        }

                        if (weaponData.SwingDamageType != DamageTypes.Invalid
                            && weaponData.SwingDamage > 0)
                        {
                            swingDamageDescriptionText = swingDamageDescriptionText.SetTextVariable("SWING_DAMAGE_TYPE", GameTexts.FindText("str_inventory_dmg_type", ((int)CurrentItem.Item.PrimaryWeapon.SwingDamageType).ToString()));
                            itemProperty = new CraftingListPropertyItem(swingDamageDescriptionText, 200f, weaponData.SwingDamage, 0f, CraftingTemplate.CraftingStatTypes.SwingDamage)
                            {
                                IsValidForUsage = true
                            };
                            ItemProperties.Add(itemProperty);
                        }

                        itemProperty = new CraftingListPropertyItem(handlingDescriptionText, 150f, weaponData.Handling, 0f, CraftingTemplate.CraftingStatTypes.Handling)
                        {
                            IsValidForUsage = true
                        };
                        ItemProperties.Add(itemProperty);
                    }
                    else if (weaponData.IsRangedWeapon)
                    {
                        if (weaponData.ThrustDamageType != DamageTypes.Invalid)
                        {
                            missileDamageDescriptionText = missileDamageDescriptionText.SetTextVariable("THRUST_DAMAGE_TYPE", GameTexts.FindText("str_inventory_dmg_type", ((int)weaponData.ThrustDamageType).ToString()));
                        }
                        else if (weaponData.SwingDamageType != DamageTypes.Invalid)
                        {
                            missileDamageDescriptionText = missileDamageDescriptionText.SetTextVariable("SWING_DAMAGE_TYPE", GameTexts.FindText("str_inventory_dmg_type", ((int)weaponData.SwingDamageType).ToString()));
                        }

                        itemProperty = new CraftingListPropertyItem(missileDamageDescriptionText, 200f, weaponData.MissileDamage, 0f, CraftingTemplate.CraftingStatTypes.MissileDamage)
                        {
                            IsValidForUsage = true
                        };
                        ItemProperties.Add(itemProperty);

                        itemProperty = new CraftingListPropertyItem(missileSpeedDescriptionText, 150f, weaponData.MissileSpeed, 0f, CraftingTemplate.CraftingStatTypes.MissileSpeed)
                        {
                            IsValidForUsage = true
                        };
                        ItemProperties.Add(itemProperty);

                        itemProperty = new CraftingListPropertyItem(accuracyDescriptionText, 150f, weaponData.Accuracy, 0f, CraftingTemplate.CraftingStatTypes.Accuracy)
                        {
                            IsValidForUsage = true
                        };
                        ItemProperties.Add(itemProperty);

                        itemProperty = new CraftingListPropertyItem(ammoStackAmountDescriptionText, 10f, weaponData.MaxDataValue, 0f, CraftingTemplate.CraftingStatTypes.StackAmount)
                        {
                            IsValidForUsage = true
                        };
                        ItemProperties.Add(itemProperty);
                    }

                    break;
            }

            foreach (Tuple<string, TextObject> itemFlagDetail in CampaignUIHelper.GetItemFlagDetails(CurrentItem.Item.ItemFlags))
            {
                ItemFlagIconsList.Add(new CraftingItemFlagVM(itemFlagDetail.Item1, itemFlagDetail.Item2, isDisplayed: true));
            }

            if (CurrentItem.Item.HasWeaponComponent)
            {
                WeaponComponentData weaponData = CurrentItem.Item.GetWeaponWithUsageIndex(SecondaryUsageSelector.SelectedIndex);
                ItemObject.ItemUsageSetFlags itemUsageFlags = TaleWorlds.MountAndBlade.MBItem.GetItemUsageSetFlags(weaponData.ItemUsage);
                foreach ((string, TextObject) flagDetail in CampaignUIHelper.GetFlagDetailsForWeapon(weaponData, itemUsageFlags))
                {
                    ItemFlagIconsList.Add(new CraftingItemFlagVM(flagDetail.Item1, flagDetail.Item2, isDisplayed: true));
                }
            }
        }

        public void UpdateCraftingHero(Hero currentHero)
        {
            CurrentHeroCraftingSkill = currentHero.CharacterObject.GetSkillValue(DefaultSkills.Crafting);

            IsCurrentHeroAtMaxCraftingSkill = CurrentHeroCraftingSkill >= 300;

            RefreshDifficulty();
        }

        private void UpdateTierFilterFlags(ArmorPieceTierFlag filter)
        {
            foreach (ArmorTierFilterTypeVM tierFilter in TierFilters)
            {
                tierFilter.IsSelected = filter.HasAllFlags(tierFilter.FilterType);
            }

            _currentTierFilter = filter;
        }

        private void OnSelectPieceTierFilter(ArmorPieceTierFlag filter)
        {
            if (_currentTierFilter != filter)
            {
                UpdateTierFilterFlags(filter);

                RefreshValues();
            }
        }

        public static ItemType GetItemType(ItemObject item)
        {
            switch (item.ItemType)
            {
                case ItemObject.ItemTypeEnum.HeadArmor: return ItemType.HeadArmor;
                case ItemObject.ItemTypeEnum.Cape: return ItemType.ShoulderArmor;
                case ItemObject.ItemTypeEnum.BodyArmor: return ItemType.BodyArmor;
                case ItemObject.ItemTypeEnum.HandArmor: return ItemType.ArmArmor;
                case ItemObject.ItemTypeEnum.LegArmor: return ItemType.LegArmor;

                case ItemObject.ItemTypeEnum.HorseHarness: return ItemType.Barding;

                case ItemObject.ItemTypeEnum.Shield: return ItemType.Shield;

                case ItemObject.ItemTypeEnum.Bow: return ItemType.Bow;
                case ItemObject.ItemTypeEnum.Crossbow: return ItemType.Crossbow;

                case ItemObject.ItemTypeEnum.Arrows: return ItemType.Arrows;
                case ItemObject.ItemTypeEnum.Bolts: return ItemType.Bolts;

                case ItemObject.ItemTypeEnum.Banner: return ItemType.Banner;

                case ItemObject.ItemTypeEnum.OneHandedWeapon:
                case ItemObject.ItemTypeEnum.TwoHandedWeapon:
                case ItemObject.ItemTypeEnum.Polearm:
                case ItemObject.ItemTypeEnum.Thrown:
                    if (Settings.Instance.AllowCraftingNormalWeapons)
                    {
                        switch (item.ItemType)
                        {
                            case ItemObject.ItemTypeEnum.OneHandedWeapon: return ItemType.OneHandedWeapon;
                            case ItemObject.ItemTypeEnum.TwoHandedWeapon: return ItemType.TwoHandedWeapon;
                            case ItemObject.ItemTypeEnum.Polearm: return ItemType.Polearm;
                            case ItemObject.ItemTypeEnum.Thrown: return ItemType.Thrown;
                            default: return ItemType.Invalid;
                        }
                    }
                    return ItemType.Invalid;

                default: return ItemType.Invalid;
            }
        }

        private static ItemType GetItemType(int itemType) => itemType switch
        {
            0 => ItemType.HeadArmor,
            1 => ItemType.ShoulderArmor,
            2 => ItemType.BodyArmor,
            3 => ItemType.ArmArmor,
            4 => ItemType.LegArmor,

            5 => ItemType.Barding,

            6 => ItemType.Shield,

            7 => ItemType.Bow,
            8 => ItemType.Crossbow,

            9 => ItemType.Arrows,
            10 => ItemType.Bolts,

            11 => ItemType.Banner,

            12 => ItemType.OneHandedWeapon,
            13 => ItemType.TwoHandedWeapon,
            14 => ItemType.Polearm,
            15 => ItemType.Thrown,

            _ => ItemType.Invalid
        };

        private ArmorCraftResultPopupVM _armorCraftResultPopup;

        private bool _armorCraftResultPopupVisible;

        private MBBindingList<WeaponDesignResultPropertyItemVM> _designResultPropertyList;

        public void CreateCraftingResultPopup(EquipmentElement equipmentElement)
        {
            _equipmentElement = equipmentElement;

            string itemName = "";
            if (_equipmentElement.ItemModifier != null)
            {
                itemName += _equipmentElement.ItemModifier.Name.ToString();
            };
            itemName += CurrentItem.Item.Name.ToString();

            ArmorCraftResultPopup = new ArmorCraftResultPopupVM(ExecuteFinalizeCrafting, _crafting, ItemFlagIconsList, CurrentItem.Item, itemName, DesignResultPropertyList, ItemVisualModel);
            ArmorCraftResultPopupVisible = true;
        }

        public void ExecuteFinalizeCrafting()
        {
            ArmorCraftResultPopupVisible = false;
        }

        [DataSourceProperty]
        public ArmorCraftResultPopupVM ArmorCraftResultPopup
        {
            get => _armorCraftResultPopup;
            set => SetField(ref _armorCraftResultPopup, value, nameof(ArmorCraftResultPopup));
        }

        [DataSourceProperty]
        public bool ArmorCraftResultPopupVisible
        {
            get => _armorCraftResultPopupVisible;
            set
            {
                if (value != _armorCraftResultPopupVisible)
                {
                    _armorCraftResultPopupVisible = value;
                    UpdateResultPropertyList();
                    OnPropertyChangedWithValue(value, "ArmorCraftResultPopupVisible");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<WeaponDesignResultPropertyItemVM> DesignResultPropertyList
        {
            get => _designResultPropertyList;
            set => SetField(ref _designResultPropertyList, value, nameof(DesignResultPropertyList));
        }

        private void UpdateResultPropertyList()
        {
            DesignResultPropertyList.Clear();

            var modifiedValues = GenerateModifierValues(GetItemType(CurrentItem.Item), _equipmentElement);

            foreach (Tuple<CraftingListPropertyItem, int> propertyItem in ItemProperties.Zip(modifiedValues, Tuple.Create))
            {
#if v100 || v101 || v102 || v103
                DesignResultPropertyList.Add(new WeaponDesignResultPropertyItemVM(propertyItem.Item1.Description, propertyItem.Item1.PropertyValue, propertyItem.Item2));
#else
                DesignResultPropertyList.Add(new WeaponDesignResultPropertyItemVM(propertyItem.Item1.Description, propertyItem.Item1.PropertyValue, propertyItem.Item2, true));
#endif
            }
        }

        private EquipmentElement _equipmentElement;

        private SelectorVM<CraftingSecondaryUsageItemVM> _secondaryUsageSelector;

        public SelectorVM<CraftingSecondaryUsageItemVM> SecondaryUsageSelector
        {
            get => _secondaryUsageSelector;
            set
            {
                if (value != _secondaryUsageSelector)
                {
                    _secondaryUsageSelector = value;
                    OnPropertyChangedWithValue(value, "SecondaryUsageSelector");
                }
            }
        }

        private void UpdateSecondaryUsageIndex(SelectorVM<CraftingSecondaryUsageItemVM> selector)
        {
            if (selector.SelectedIndex != -1 && CurrentItem != null)
            {
                RefreshStats(CurrentItem.ItemType);
            }
            else
            {
                RefreshStats(ItemType.Invalid);
            }
        }

        private void TrySetSecondaryUsageIndex(int usageIndex)
        {
            int num = 0;
            if (DoesCurrentItemHaveSecondaryUsage(usageIndex))
            {
                CraftingSecondaryUsageItemVM craftingSecondaryUsageItemVM = SecondaryUsageSelector.ItemList.FirstOrDefault((x) => x.UsageIndex == usageIndex);
                num = craftingSecondaryUsageItemVM?.SelectorIndex ?? 0;
            }

            if (num >= 0 && num < SecondaryUsageSelector.ItemList.Count)
            {
                SecondaryUsageSelector.SelectedIndex = num;
                SecondaryUsageSelector.ItemList[num].IsSelected = true;
            }
        }

        private bool DoesCurrentItemHaveSecondaryUsage(int usageIndex)
        {
            if (usageIndex >= 0)
            {
                return usageIndex < CurrentItem.Item.Weapons.Count;
            }

            return false;
        }
    }
}