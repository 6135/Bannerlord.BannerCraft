using Bannerlord.UIExtenderEx.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting.WeaponDesign;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerCraft
{
	public class ArmorCraftingVM : ViewModel
	{
		[Flags]
		public enum ArmorPieceTierFilter
		{
			None = 0x0,
			Tier0 = 0x1,
			Tier1 = 0x2,
			Tier2 = 0x4,
			Tier3 = 0x8,
			Tier4 = 0x10,
			Tier5 = 0x20,
			Tier6 = 0x40,
			All = 0x7F
		}

		public enum ItemType
		{
			HeadArmor,
			ShoulderArmor,
			BodyArmor,
			ArmArmor,
			LegArmor,

			Barding,

			Shield,

			Bow,
			Crossbow,

			Arrows,
			Bolts,

			Invalid
		}

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
		private ArmorPieceTierFilter _currentTierFilter;

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

		[DataSourceProperty]
		public ArmorClassSelectionPopupVM ArmorClassSelectionPopup
		{
			get => _armorClassSelectionPopup;
			set
			{
				if (value != _armorClassSelectionPopup)
				{
					_armorClassSelectionPopup = value;
					OnPropertyChangedWithValue(value, "ArmorClassSelectionPopup");
				}
			}
		}

		[DataSourceProperty]
		public string ChooseArmorTypeText
		{
			get => _chooseArmorTypeText;
			set
			{
				if (value != _chooseArmorTypeText)
				{
					_chooseArmorTypeText = value;
					OnPropertyChangedWithValue(value, "ChooseArmorTypeText");
				}
			}
		}
		
		[DataSourceProperty]
		public string CurrentCraftedArmorTypeText
		{
			get => _currentCraftedArmorTypeText;
			set
			{
				if (value != _currentCraftedArmorTypeText)
				{
					_currentCraftedArmorTypeText = value;
					OnPropertyChangedWithValue(value, "CurrentCraftedArmorTypeText");
				}
			}
		}

		[DataSourceProperty]
		public MBBindingList<ArmorTierFilterTypeVM> TierFilters
		{
			get => _tierFilters;
			set
			{
				if (value != _tierFilters)
				{
					_tierFilters = value;
					OnPropertyChangedWithValue(value, "TierFilters");
				}
			}
		}

		[DataSourceProperty]
		public MBBindingList<ArmorItemVM> Armors
		{
			get => _armors;
			set
			{
				if (value != _armors)
				{
					_armors = value;
					OnPropertyChangedWithValue(value, "Armors");
				}
			}
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
					_mixin.OnRefresh();
				}
			}
		}

		[DataSourceProperty]
		public int SelectedPieceTypeIndex
		{
			get => _selectedPieceTypeIndex;
			set
			{
				if (value != _selectedPieceTypeIndex)
				{
					_selectedPieceTypeIndex = value;
					OnPropertyChangedWithValue(value, "SelectedPieceTypeIndex");
				}
			}
		}

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
		public MBBindingList<CraftingListPropertyItem> ItemProperties
		{
			get => _itemProperties;
			set
			{
				if (value != _itemProperties)
				{
					_itemProperties = value;
					OnPropertyChangedWithValue(value, "ItemProperties");
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
					OnPropertyChangedWithValue(value, "ItemFlagIconsList");
				}
			}
		}

		[DataSourceProperty]
		public string DifficultyText
		{
			get => _difficultyText;
			set
			{
				if (value != _difficultyText)
				{
					_difficultyText = value;
					OnPropertyChangedWithValue(value, "DifficultyText");
				}
			}
		}

		[DataSourceProperty]
		public string CurrentDifficultyText
		{
			get => _currentDifficultyText;
			set
			{
				if (value != _currentDifficultyText)
				{
					_currentDifficultyText = value;
					OnPropertyChangedWithValue(value, "CurrentDifficultyText");
				}
			}
		}

		[DataSourceProperty]
		public string CurrentCraftingSkillValueText
		{
			get => _currentCraftingSkillValueText;
			set
			{
				if (value != _currentCraftingSkillValueText)
				{
					_currentCraftingSkillValueText = value;
					OnPropertyChangedWithValue(value, "CurrentCraftingSkillValueText");
				}
			}
		}

		[DataSourceProperty]
		public int CurrentHeroCraftingSkill
		{
			get => _currentHeroCraftingSkill;
			set
			{
				if (value != _currentHeroCraftingSkill)
				{
					_currentHeroCraftingSkill = value;
					OnPropertyChangedWithValue(value, "CurrentHeroCraftingSkill");
				}
			}
		}

		[DataSourceProperty]
		public int MaxDifficulty
		{
			get => _maxDifficulty;
			set
			{
				if (value != _maxDifficulty)
				{
					_maxDifficulty = value;
					OnPropertyChangedWithValue(value, "MaxDifficulty");
				}
			}
		}

		[DataSourceProperty]
		public int CurrentDifficulty
		{
			get => _currentDifficulty;
			set
			{
				if (value != _currentDifficulty)
				{
					_currentDifficulty = value;
					OnPropertyChangedWithValue(value, "CurrentDifficulty");
				}
			}
		}

		[DataSourceProperty]
		public bool IsCurrentHeroAtMaxCraftingSkill
		{
			get => _isCurrentHeroAtMaxCraftingSkill;
			set
			{
				if (value != _isCurrentHeroAtMaxCraftingSkill)
				{
					_isCurrentHeroAtMaxCraftingSkill = value;
					OnPropertyChangedWithValue(value, "IsCurrentHeroAtMaxCraftingSkill");
				}
			}
		}

		public ArmorCraftingVM(CraftingMixin mixin, Crafting crafting)
		{
			_mixin = mixin;
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
				armorClasses.Add(GameTexts.FindText("str_bannercraft_crafting_itemtype", value.ToString().ToLower()));
			}

			ArmorClassSelectionPopup = new ArmorClassSelectionPopupVM(armorClasses, delegate (int x)
			{
				RefreshArmorDesignMode(x);
			});

			ChooseArmorTypeText = new TextObject("{=Gd6zuUwh}Free Build").ToString();

			TierFilters = new MBBindingList<ArmorTierFilterTypeVM>
			{
				new ArmorTierFilterTypeVM(ArmorPieceTierFilter.All, OnSelectPieceTierFilter, GameTexts.FindText("str_crafting_tier_filter_all").ToString()),
				new ArmorTierFilterTypeVM(ArmorPieceTierFilter.Tier0, OnSelectPieceTierFilter, 0.ToString()),
				new ArmorTierFilterTypeVM(ArmorPieceTierFilter.Tier1, OnSelectPieceTierFilter, GameTexts.FindText("str_tier_one").ToString()),
				new ArmorTierFilterTypeVM(ArmorPieceTierFilter.Tier2, OnSelectPieceTierFilter, GameTexts.FindText("str_tier_two").ToString()),
				new ArmorTierFilterTypeVM(ArmorPieceTierFilter.Tier3, OnSelectPieceTierFilter, GameTexts.FindText("str_tier_three").ToString()),
				new ArmorTierFilterTypeVM(ArmorPieceTierFilter.Tier4, OnSelectPieceTierFilter, GameTexts.FindText("str_tier_four").ToString()),
				new ArmorTierFilterTypeVM(ArmorPieceTierFilter.Tier5, OnSelectPieceTierFilter, GameTexts.FindText("str_tier_five").ToString()),
				new ArmorTierFilterTypeVM(ArmorPieceTierFilter.Tier6, OnSelectPieceTierFilter, Common.ToRoman(6))
			};

			Armors = new MBBindingList<ArmorItemVM>();

			ItemVisualModel = new ItemCollectionElementViewModel();

			ItemProperties = new MBBindingList<CraftingListPropertyItem>();
			DesignResultPropertyList = new MBBindingList<WeaponDesignResultPropertyItemVM>();

			ItemFlagIconsList = new MBBindingList<ItemFlagVM>();

			_currentCraftingSkillValueTextObj = new TextObject("{=LEiZWuZm}{SKILL_NAME}: {SKILL_VALUE}");
			_currentDifficultyTextObj = new TextObject("{=cbbUzYX3}Difficulty: {DIFFICULTY}");

			MaxDifficulty = 300;

			UpdateTierFilterFlags(ArmorPieceTierFilter.All);

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

		private void RefreshArmorDesignMode(int classIndex)
		{
			_selectedItemType = GetItemType(classIndex);

			CurrentCraftedArmorTypeText = GameTexts.FindText("str_bannercraft_crafting_itemtype", _selectedItemType.ToString().ToLower()).ToString();

			RefreshValues();
			RefreshCurrentHeroSkillLevel();

			RefreshDifficulty();
		}

		public override void RefreshValues()
		{
			base.RefreshValues();

			Armors.Clear();

			foreach (var item in Game.Current.ObjectManager.GetObjectTypeList<ItemObject>())
			{
				ItemType itemType = GetItemType(item);
				if (   itemType == ItemType.Invalid
					|| itemType != _selectedItemType)
				{
					continue;
				}

				if (((int)_currentTierFilter & (1 << ((int)item.Tier + 1))) == 0)
				{
					continue;
				}

				Armors.Add(new ArmorItemVM(this, item, itemType));
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
			CurrentHeroCraftingSkill = _mixin.CurrentCraftingHero.Hero.CharacterObject.GetSkillValue(DefaultSkills.Crafting);
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

		private List<int> GenerateModifierValues(ItemType itemType, EquipmentElement element)
        {
			/*
			 * This is a very fragile function that should be refactored alongside RefreshStats
			 * But not right now
			 */
			if (itemType == ItemType.Invalid)
            {
				return null;
            }

			List<int> ret = new List<int>();
			/*
			 * Weight is always the first element in the stats list and it can't be changed from the modifier
			 */
			ret.Add(0);

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
						armor = (int)element.ItemModifier?.GetType().GetField("_armor", bindingFlags)?.GetValue(element.ItemModifier);
					}

					ret.Add(CurrentItem.Item.ArmorComponent.HeadArmor > 0 ? armor : 0);
					ret.Add(CurrentItem.Item.ArmorComponent.BodyArmor > 0 ? armor : 0);
					ret.Add(CurrentItem.Item.ArmorComponent.LegArmor > 0 ? armor : 0);
					ret.Add(CurrentItem.Item.ArmorComponent.ArmArmor > 0 ? armor : 0);

					break;
				case ItemType.Barding:
					armor = 0;
					if (element.ItemModifier != null)
					{
						armor = (int)element.ItemModifier.GetType().GetField("_armor", bindingFlags).GetValue(element.ItemModifier);
					}

					ret.Add(armor);

					break;
				case ItemType.Shield:
					shieldSpeed = 0;
					shieldHitPoints = 0;
					if (element.ItemModifier != null)
                    {
						shieldSpeed = (int)element.ItemModifier.GetType().GetField("_speed", bindingFlags).GetValue(element.ItemModifier);
						shieldHitPoints = (short)element.ItemModifier.GetType().GetField("_hitPoints", bindingFlags).GetValue(element.ItemModifier);
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
						speed = (int)element.ItemModifier.GetType().GetField("_speed", bindingFlags).GetValue(element.ItemModifier);
						missileSpeed = (int)element.ItemModifier.GetType().GetField("_missileSpeed", bindingFlags).GetValue(element.ItemModifier);
						missileDamage = (int)element.ItemModifier.GetType().GetField("_damage", bindingFlags).GetValue(element.ItemModifier);
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
						missileDamage = (int)element.ItemModifier.GetType().GetField("_damage", bindingFlags).GetValue(element.ItemModifier);
						stackCount = (short)element.ItemModifier.GetType().GetField("_stackCount", bindingFlags).GetValue(element.ItemModifier);
					}

					ret.Add(missileDamage);
					ret.Add(stackCount);

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
					TextObject handlingDescriptionText = GameTexts.FindText("str_bannercraft_crafting_statdisplay", "speed");
					TextObject shieldHitPointsDescriptionText = GameTexts.FindText("str_bannercraft_crafting_statdisplay", "shield_hitpoints");

					itemProperty = new CraftingListPropertyItem(weightDescriptionText, 10f, CurrentItem.Item.Weight, 0f, CraftingTemplate.CraftingStatTypes.Weight)
					{
						IsValidForUsage = true
					};
					ItemProperties.Add(itemProperty);

					itemProperty = new CraftingListPropertyItem(handlingDescriptionText, 150f, CurrentItem.Item.PrimaryWeapon.Handling, 0f, CraftingTemplate.CraftingStatTypes.Handling)
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
					TextObject damageDescriptionText = GameTexts.FindText("str_bannercraft_crafting_statdisplay", "missile_damage");
					TextObject accuracyDescriptionText = GameTexts.FindText("str_bannercraft_crafting_statdisplay", "accuracy");
					TextObject missileSpeedDescriptionText = GameTexts.FindText("str_bannercraft_crafting_statdisplay", "missile_speed");

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

					itemProperty = new CraftingListPropertyItem(damageDescriptionText, 150f, CurrentItem.Item.PrimaryWeapon.MissileDamage, 0f, CraftingTemplate.CraftingStatTypes.MissileDamage)
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
					TextObject ammoDamageDescriptionText = GameTexts.FindText("str_bannercraft_crafting_statdisplay", "missile_damage");
					TextObject ammoStackAmountDescriptionText = GameTexts.FindText("str_bannercraft_crafting_statdisplay", "ammo_stack_amount");

					itemProperty = new CraftingListPropertyItem(weightDescriptionText, 100f, CurrentItem.Item.Weight, 0f, CraftingTemplate.CraftingStatTypes.Weight)
					{
						IsValidForUsage = true
					};
					ItemProperties.Add(itemProperty);

					itemProperty = new CraftingListPropertyItem(ammoDamageDescriptionText, 10f, CurrentItem.Item.WeaponComponent.PrimaryWeapon.MissileDamage, 0f, CraftingTemplate.CraftingStatTypes.MissileDamage)
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
			}

			foreach (Tuple<string, TextObject> itemFlagDetail in CampaignUIHelper.GetItemFlagDetails(CurrentItem.Item.ItemFlags))
			{
				ItemFlagIconsList.Add(new CraftingItemFlagVM(itemFlagDetail.Item1, itemFlagDetail.Item2, isDisplayed: true));
			}

			if (CurrentItem.Item.HasWeaponComponent)
			{
				ItemObject.ItemUsageSetFlags itemUsageFlags = TaleWorlds.MountAndBlade.MBItem.GetItemUsageSetFlags(CurrentItem.Item.WeaponComponent.PrimaryWeapon.ItemUsage);
				foreach ((string, TextObject) flagDetail in CampaignUIHelper.GetFlagDetailsForWeapon(CurrentItem.Item.WeaponComponent.PrimaryWeapon, itemUsageFlags))
				{
					ItemFlagIconsList.Add(new CraftingItemFlagVM(flagDetail.Item1, flagDetail.Item2, isDisplayed: true));
				}
			}
		}

		public void UpdateCraftingHero(CraftingAvailableHeroItemVM currentHero)
		{
			CurrentHeroCraftingSkill = currentHero.Hero.CharacterObject.GetSkillValue(DefaultSkills.Crafting);

			IsCurrentHeroAtMaxCraftingSkill = CurrentHeroCraftingSkill >= 300;

			RefreshDifficulty();
		}

		private void UpdateTierFilterFlags(ArmorPieceTierFilter filter)
		{
			foreach (ArmorTierFilterTypeVM tierFilter in TierFilters)
			{
				tierFilter.IsSelected = filter.HasAllFlags(tierFilter.FilterType);
			}

			_currentTierFilter = filter;
		}

		private void OnSelectPieceTierFilter(ArmorPieceTierFilter filter)
		{
			if (_currentTierFilter != filter)
			{
				UpdateTierFilterFlags(filter);

				RefreshValues();
			}
		}

		public static ItemType GetItemType(ItemObject item) => item.ItemType switch
		{
			ItemObject.ItemTypeEnum.HeadArmor => ItemType.HeadArmor,
			ItemObject.ItemTypeEnum.Cape => ItemType.ShoulderArmor,
			ItemObject.ItemTypeEnum.BodyArmor => ItemType.BodyArmor,
			ItemObject.ItemTypeEnum.HandArmor => ItemType.ArmArmor,
			ItemObject.ItemTypeEnum.LegArmor => ItemType.LegArmor,

			ItemObject.ItemTypeEnum.HorseHarness => ItemType.Barding,

			ItemObject.ItemTypeEnum.Shield => ItemType.Shield,

			ItemObject.ItemTypeEnum.Bow => ItemType.Bow,
			ItemObject.ItemTypeEnum.Crossbow => ItemType.Crossbow,

			ItemObject.ItemTypeEnum.Arrows => ItemType.Arrows,
			ItemObject.ItemTypeEnum.Bolts => ItemType.Bolts,

			_ => ItemType.Invalid
		};

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
			set
			{
				if (value != _armorCraftResultPopup)
				{
					_armorCraftResultPopup = value;
					OnPropertyChangedWithValue(value, "ArmorCraftResultPopup");
				}
			}
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
			set
			{
				if (value != _designResultPropertyList)
				{
					_designResultPropertyList = value;
					OnPropertyChangedWithValue(value, "DesignResultPropertyList");
				}
			}
		}

		private void UpdateResultPropertyList()
		{
			DesignResultPropertyList.Clear();

			var modifiedValues = GenerateModifierValues(GetItemType(CurrentItem.Item), _equipmentElement);

			foreach (Tuple<CraftingListPropertyItem, int> propertyItem in ItemProperties.Zip(modifiedValues, Tuple.Create))
			//foreach (CraftingListPropertyItem propertyItem in ItemProperties)
			{
				//DesignResultPropertyList.Add(new WeaponDesignResultPropertyItemVM(propertyItem.Description, propertyItem.PropertyValue, 0f));
				DesignResultPropertyList.Add(new WeaponDesignResultPropertyItemVM(propertyItem.Item1.Description, propertyItem.Item1.PropertyValue, propertyItem.Item2));
			}
		}

		private EquipmentElement _equipmentElement;
	}
}