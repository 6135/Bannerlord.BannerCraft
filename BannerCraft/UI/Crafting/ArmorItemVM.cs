using System;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting.WeaponDesign;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerCraft
{
	using Config = BannerCraftConfig;

	public class ArmorItemVM : ViewModel
	{
		private readonly ArmorCraftingVM _armorCrafting;
		private ImageIdentifierVM _imageIdentifier;
		private ItemObject _item;

		private ArmorCraftingVM.ItemType _itemType;

		private bool _isSelected;

		private ItemObject.ItemTiers _tier;

		private string _tierText;

		private MBBindingList<CraftingItemFlagVM> _itemFlagIcons;

		private HintViewModel _hint;

		[DataSourceProperty]
		public ImageIdentifierVM ImageIdentifier
		{
			get => _imageIdentifier;
			set
			{
				if (value != _imageIdentifier)
				{
					_imageIdentifier = value;
					OnPropertyChangedWithValue(value, "ImageIdentifier");
				}
			}
		}

		[DataSourceProperty]
		public ItemObject Item
		{
			get => _item;
			set
			{
				if (value != _item)
				{
					_item = value;
					OnPropertyChangedWithValue(value, "Item");
				}
			}
		}

		[DataSourceProperty]
		public string ItemName => Item.Name.ToString();

		[DataSourceProperty]
		public ArmorCraftingVM.ItemType ItemType
		{
			get => _itemType;
			private set
			{
				if (value != _itemType)
				{
					_itemType = value;
					OnPropertyChangedWithValue(value, "ItemType");
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
		public ItemObject.ItemTiers Tier
		{
			get => _tier;
			set
			{
				if (value != _tier)
				{
					_tier = value;
					OnPropertyChangedWithValue(value, "Tier");
				}
			}
		}

		[DataSourceProperty]
		public string TierText
		{
			get => _tierText;
			set
			{
				if (value != _tierText)
				{
					_tierText = value;
					OnPropertyChangedWithValue(value, "TierText");
				}
			}
		}

		[DataSourceProperty]
		public MBBindingList<CraftingItemFlagVM> ItemFlagIcons
		{
			get => _itemFlagIcons;
			set
			{
				if (value != _itemFlagIcons)
				{
					_itemFlagIcons = value;
					OnPropertyChangedWithValue(value, "ItemFlagIcons");
				}
			}
		}

		[DataSourceProperty]
		public int Difficulty { get; }

		[DataSourceProperty]
		public HintViewModel Hint
		{
			get => _hint;
			set
            {
				if (value != _hint)
                {
					_hint = value;
					OnPropertyChangedWithValue(value, "Hint");
                }
            }
		}


		public ArmorItemVM(ArmorCraftingVM armorCrafting, ItemObject item, ArmorCraftingVM.ItemType type)
		{
			_armorCrafting = armorCrafting;
			ImageIdentifier = new ImageIdentifierVM(item);
			Item = item;
			ItemType = type;

			Tier = item.Tier;
			TierText = Common.ToRoman((int)Tier + 1);

			ItemFlagIcons = new MBBindingList<CraftingItemFlagVM>();

			foreach (Tuple<string, TextObject> itemFlagDetail in CampaignUIHelper.GetItemFlagDetails(Item.ItemFlags))
			{
				ItemFlagIcons.Add(new CraftingItemFlagVM(itemFlagDetail.Item1, itemFlagDetail.Item2, isDisplayed: true));
			}

			Difficulty = Config.Instance.SmithingModel.CalculateArmorDifficulty(Item);

			Hint = new HintViewModel(new TextObject(GenerateHintText()));
		}

		public void ExecuteSelect()
		{
			_armorCrafting.CurrentItem.IsSelected = false;
			_armorCrafting.CurrentItem = this;
			_armorCrafting.ItemVisualModel.StringId = Item?.StringId ?? "";
			IsSelected = true;

			_armorCrafting.RefreshStats(ItemType);
		}

		private string GenerateHintText()
        {
			string ret = "";
			TextObject weightDescriptionText = GameTexts.FindText("str_crafting_stat", "Weight");
			ret += string.Format("{0}{1}\n", weightDescriptionText.ToString(), Item.Weight);
			switch (ItemType)
            {
				case ArmorCraftingVM.ItemType.Invalid:
					/*
					 * Never reached, so let's use this to declare switch local variables
					 */
					
					break;
				case ArmorCraftingVM.ItemType.HeadArmor:
				case ArmorCraftingVM.ItemType.ShoulderArmor:
				case ArmorCraftingVM.ItemType.BodyArmor:
				case ArmorCraftingVM.ItemType.ArmArmor:
				case ArmorCraftingVM.ItemType.LegArmor:
					TextObject headDescriptionText = GameTexts.FindText("str_bannercraft_crafting_statdisplay", ArmorCraftingVM.ItemType.HeadArmor.ToString().ToLower());
					TextObject bodyDescriptionText = GameTexts.FindText("str_bannercraft_crafting_statdisplay", ArmorCraftingVM.ItemType.BodyArmor.ToString().ToLower());
					TextObject legDescriptionText = GameTexts.FindText("str_bannercraft_crafting_statdisplay", ArmorCraftingVM.ItemType.LegArmor.ToString().ToLower());
					TextObject armDescriptionText = GameTexts.FindText("str_bannercraft_crafting_statdisplay", ArmorCraftingVM.ItemType.ArmArmor.ToString().ToLower());
					
					ret += string.Format("{0}{1}\n", headDescriptionText.ToString(), Item.ArmorComponent.HeadArmor);
					ret += string.Format("{0}{1}\n", bodyDescriptionText.ToString(), Item.ArmorComponent.BodyArmor);
					ret += string.Format("{0}{1}\n", legDescriptionText.ToString(), Item.ArmorComponent.LegArmor);
					ret += string.Format("{0}{1}", armDescriptionText.ToString(), Item.ArmorComponent.ArmArmor);

					break;
				case ArmorCraftingVM.ItemType.Barding:
					TextObject horseDescriptionText = GameTexts.FindText("str_bannercraft_crafting_statdisplay", ArmorCraftingVM.ItemType.Barding.ToString().ToLower());

					ret += string.Format("{0}{1}", horseDescriptionText.ToString(), Item.ArmorComponent.BodyArmor);

					break;
				case ArmorCraftingVM.ItemType.Shield:
					TextObject handlingDescriptionText = GameTexts.FindText("str_bannercraft_crafting_statdisplay", "speed");
					TextObject shieldHitPointsDescriptionText = GameTexts.FindText("str_bannercraft_crafting_statdisplay", "shield_hitpoints");

					ret += string.Format("{0}{1}\n", handlingDescriptionText.ToString(), Item.PrimaryWeapon.Handling);
					ret += string.Format("{0}{1}", shieldHitPointsDescriptionText.ToString(), Item.PrimaryWeapon.MaxDataValue);

					break;
				case ArmorCraftingVM.ItemType.Bow:
				case ArmorCraftingVM.ItemType.Crossbow:
					TextObject rangedWeaponSpeedDescriptionText = GameTexts.FindText("str_bannercraft_crafting_statdisplay", "ranged_weapon_speed");
					TextObject damageDescriptionText = GameTexts.FindText("str_bannercraft_crafting_statdisplay", "missile_damage");
					TextObject accuracyDescriptionText = GameTexts.FindText("str_bannercraft_crafting_statdisplay", "accuracy");
					TextObject missileSpeedDescriptionText = GameTexts.FindText("str_bannercraft_crafting_statdisplay", "missile_speed");

					ret += string.Format("{0}{1}\n", rangedWeaponSpeedDescriptionText.ToString(), Item.PrimaryWeapon.SwingSpeed);
					ret += string.Format("{0}{1}\n", damageDescriptionText.ToString(), Item.PrimaryWeapon.MissileDamage);
					ret += string.Format("{0}{1}\n", accuracyDescriptionText.ToString(), Item.PrimaryWeapon.Accuracy);
					ret += string.Format("{0}{1}", missileSpeedDescriptionText.ToString(), Item.PrimaryWeapon.MissileSpeed);

					break;

				case ArmorCraftingVM.ItemType.Arrows:
				case ArmorCraftingVM.ItemType.Bolts:
					TextObject ammoDamageDescriptionText = GameTexts.FindText("str_bannercraft_crafting_statdisplay", "missile_damage");
					TextObject ammoStackAmountDescriptionText = GameTexts.FindText("str_bannercraft_crafting_statdisplay", "ammo_stack_amount");

					ret += string.Format("{0}{1}\n", ammoDamageDescriptionText.ToString(), Item.PrimaryWeapon.MissileDamage);
					ret += string.Format("{0}{1}", ammoStackAmountDescriptionText.ToString(), Item.PrimaryWeapon.MaxDataValue);

					break;
			}
			return ret;
        }
	}
}