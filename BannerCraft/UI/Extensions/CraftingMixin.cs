using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting.Smelting;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerCraft
{
	using Config = BannerCraftConfig;

	[ViewModelMixin("UpdateAll")]
	public class CraftingMixin : BaseViewModelMixin<CraftingVM>
	{
		private readonly Crafting _crafting;
		private readonly CraftingVM _craftingVM;

		private readonly ICraftingCampaignBehavior _craftingBehavior;

		private bool _isInArmorMode;

		private string _armorText;

		private ArmorCraftingVM _armorCrafting;

		private MBBindingList<ExtraMaterialItemVM> _craftingResourceItems;

		[DataSourceProperty]
		public bool IsInArmorMode
		{
			get => _isInArmorMode;
			set
			{
				if (value != _isInArmorMode)
				{
					_isInArmorMode = value;
					ViewModel!.OnPropertyChangedWithValue(value, "IsInArmorMode");
				}
			}
		}

		[DataSourceProperty]
		public string ArmorText
		{
			get => _armorText;
			set
			{
				if (value != _armorText)
				{
					_armorText = value;
					ViewModel!.OnPropertyChangedWithValue(value, "ArmorText");
				}
			}
		}

		[DataSourceProperty]
		public ArmorCraftingVM ArmorCrafting
		{
			get => _armorCrafting;
			set
			{
				if (value != _armorCrafting)
				{
					_armorCrafting = value;
					ViewModel!.OnPropertyChangedWithValue(value, "ArmorCraftingBC");
				}
			}
		}

		[DataSourceProperty]
		public MBBindingList<ExtraMaterialItemVM> ExtraMaterials
		{
			get => _craftingResourceItems;
			set
			{
				if (value != _craftingResourceItems)
				{
					_craftingResourceItems = value;
					ViewModel!.OnPropertyChangedWithValue(value, "ExtraMaterials");
				}
			}
		}

		public CraftingMixin(CraftingVM craftingVM) : base(craftingVM)
		{
			_craftingVM = craftingVM;

			Type type = typeof(CraftingVM);
			BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
			_crafting = (Crafting)ViewModel!.GetType().GetField("_crafting", bindingFlags).GetValue(ViewModel!);

			_craftingBehavior = Campaign.Current.GetCampaignBehavior<ICraftingCampaignBehavior>();

			ArmorText = GameTexts.FindText("str_bannercraft_crafting_category_armor").ToString();

			ArmorCrafting = new ArmorCraftingVM(this, _crafting);

			if (Campaign.Current.GameMode == CampaignGameMode.Campaign)
			{
				_craftingVM.AvailableCharactersForSmithing.Clear();
				foreach (Hero item in CraftingHelper.GetAvailableHeroesForCrafting())
				{
					AvailableCharactersForSmithing.Add(new CraftingAvailableHeroItemVM(item, UpdateCraftingHero));
				}

				CurrentCraftingHero = AvailableCharactersForSmithing.FirstOrDefault();
			}
			else
			{
				CurrentCraftingHero = new CraftingAvailableHeroItemVM(Hero.MainHero, UpdateCraftingHero);
			}

			CurrentCraftingHero = AvailableCharactersForSmithing.FirstOrDefault();

			_craftingVM.ExecuteSwitchToCrafting();

			UpdateAll();
		}

		private int GetRequiredEnergy()
		{
			if (CurrentCraftingHero?.Hero != null)
			{
				if (IsInArmorMode)
				{
					if (ArmorCrafting.CurrentItem == null)
					{
						return 0;
					}
					return Config.Instance.SmithingModel.GetEnergyCostForArmor(ArmorCrafting.CurrentItem.Item, CurrentCraftingHero.Hero);
				}
				return Config.Instance.SmithingModel.GetEnergyCostForSmithing(_crafting.GetCurrentCraftedItemObject(), CurrentCraftingHero.Hero);
			}
			return 0;
		}

		private bool HaveEnergy()
		{
			if (CurrentCraftingHero?.Hero != null)
			{
				if (IsInArmorMode)
				{
					if (ArmorCrafting.CurrentItem == null)
					{
						return false;
					}
					return _craftingBehavior.GetHeroCraftingStamina(CurrentCraftingHero.Hero) >= Config.Instance.SmithingModel.GetEnergyCostForArmor(ArmorCrafting.CurrentItem.Item, CurrentCraftingHero.Hero);
				}
				return _craftingBehavior.GetHeroCraftingStamina(CurrentCraftingHero.Hero) >= Config.Instance.SmithingModel.GetEnergyCostForSmithing(_crafting.GetCurrentCraftedItemObject(), CurrentCraftingHero.Hero);
			}

			return true;
		}

		private bool HaveMaterialsNeeded()
		{
			return !(   _craftingVM.PlayerCurrentMaterials.Any((CraftingResourceItemVM m) => m.ResourceChangeAmount + m.ResourceAmount < 0)
					 || ExtraMaterials.Any((ExtraMaterialItemVM m) => m.ResourceChangeAmount + m.ResourceAmount < 0));
		}

		private void UpdateCurrentMaterialCosts()
		{
			if (!IsInArmorMode)
			{
				for (int i = 0; i < (int)ExtraCraftingMaterials.NumExtraCraftingMats; i++)
				{
					ExtraMaterials[i].ResourceChangeAmount = 0;
				}
				return;
			}

			if (ArmorCrafting.CurrentItem == null)
			{
				for (int i = 0; i < (int)CraftingMaterials.NumCraftingMats; i++)
				{
					_craftingVM.PlayerCurrentMaterials[i].ResourceChangeAmount = 0;
				}

				for (int i = 0; i < (int)ExtraCraftingMaterials.NumExtraCraftingMats; i++)
				{
					ExtraMaterials[i].ResourceChangeAmount = 0;
				}

				return;
			}

			int[] craftingCostsForArmorCrafting = Config.Instance.SmithingModel.GetCraftingInputForArmor(ArmorCrafting.CurrentItem.Item);

			for (int i = 0; i < (int)CraftingMaterials.NumCraftingMats; i++)
			{
				_craftingVM.PlayerCurrentMaterials[i].ResourceChangeAmount = craftingCostsForArmorCrafting[i];
			}

			for (int i = 0; i < (int)ExtraCraftingMaterials.NumExtraCraftingMats; i++)
			{
				ExtraMaterials[i].ResourceChangeAmount = craftingCostsForArmorCrafting[(int)CraftingMaterials.NumCraftingMats + i];
			}
		}

		private void UpdateCurrentMaterialsAvailable()
		{
			if (_craftingVM.PlayerCurrentMaterials == null)
			{
				_craftingVM.PlayerCurrentMaterials = new MBBindingList<CraftingResourceItemVM>();
				for (int i = 0; i < (int)CraftingMaterials.NumCraftingMats; i++)
				{
					_craftingVM.PlayerCurrentMaterials.Add(new CraftingResourceItemVM((CraftingMaterials)i, 0));
				}
			}

			for (int i = 0; i < (int)CraftingMaterials.NumCraftingMats; i++)
			{
				ItemObject craftingMaterialItem = Campaign.Current.Models.SmithingModel.GetCraftingMaterialItem((CraftingMaterials)i);
				_craftingVM.PlayerCurrentMaterials[i].ResourceAmount = MobileParty.MainParty.ItemRoster.GetItemNumber(craftingMaterialItem);
			}
		}

		private void UpdateExtraMaterialsAvailable()
		{
			if (ExtraMaterials == null)
			{
				ExtraMaterials = new MBBindingList<ExtraMaterialItemVM>();
				for (int i = 0; i < (int)ExtraCraftingMaterials.NumExtraCraftingMats; ++i)
				{
					ExtraMaterials.Add(new ExtraMaterialItemVM((ExtraCraftingMaterials)i, 0));
				}
			}

			for (int i = 0; i < (int)ExtraCraftingMaterials.NumExtraCraftingMats; ++i)
			{
				ItemObject extraCraftingMaterialItem = Config.Instance.SmithingModel.GetCraftingMaterialItem((ExtraCraftingMaterials)i);
				ExtraMaterials[i].ResourceAmount = MobileParty.MainParty.ItemRoster.GetItemNumber(extraCraftingMaterialItem);
			}
		}

		private void UpdateCraftingStamina()
		{
			foreach (CraftingAvailableHeroItemVM item in AvailableCharactersForSmithing)
			{
				item.RefreshStamina();
			}
		}

		private void UpdateCraftingSkills()
		{
			foreach (CraftingAvailableHeroItemVM item in AvailableCharactersForSmithing)
			{
				item.RefreshSkills();
			}
		}

		private void RefreshEnableMainAction()
		{
			if (Campaign.Current.GameMode == CampaignGameMode.Tutorial)
			{
				_craftingVM.IsMainActionEnabled = true;
				return;
			}

			if (!IsInArmorMode)
			{
				/*
				 * This is stupid as hell, but CraftingVM.RefreshEnableMainAction is private
				 * UpdateCraftingHero is the only public function that calls it
				 */
				_craftingVM.UpdateCraftingHero(CurrentCraftingHero);
				return;
			}

			UpdateCurrentMaterialsAvailable();
			UpdateExtraMaterialsAvailable();

			_craftingVM.IsMainActionEnabled = true;
			if (!HaveEnergy())
			{
				_craftingVM.IsMainActionEnabled = false;
				if (_craftingVM.MainActionHint != null)
				{
					_craftingVM.MainActionHint = new BasicTooltipViewModel(() => 
						GameTexts.FindText("str_bannercraft_crafting_stamina_display")
							.SetTextVariable("HERONAME", CurrentCraftingHero.Hero.Name.ToString())
							.SetTextVariable("REQUIRED", GetRequiredEnergy())
							.SetTextVariable("CURRENT", _craftingBehavior.GetHeroCraftingStamina(CurrentCraftingHero.Hero)).ToString());
				}
			}
			else if (!HaveMaterialsNeeded())
			{
				_craftingVM.IsMainActionEnabled = false;
				if (_craftingVM.MainActionHint != null)
				{
					_craftingVM.MainActionHint = new BasicTooltipViewModel(() => new TextObject("{=gduqxfck}You don't have all required materials!").ToString());
				}
			}
			else if (ArmorCrafting.CurrentItem == null)
			{
				_craftingVM.IsMainActionEnabled = false;
			}
		}

		private void UpdateAll()
		{
			/*
			 * Copy of CraftingVM.UpdateAll because it's private for some stupid reason
			 */
			UpdateCurrentMaterialsAvailable();
			UpdateExtraMaterialsAvailable();
			UpdateCurrentMaterialCosts();

			RefreshEnableMainAction();
			UpdateCraftingStamina();
			UpdateCraftingSkills();
		}

		public override void OnRefresh()
		{
			base.OnRefresh();

			if (_craftingVM.IsInCraftingMode || _craftingVM.IsInRefinementMode || _craftingVM.IsInSmeltingMode)
			{
				IsInArmorMode = false;
				return;
			}

			ArmorCrafting?.UpdateCraftingHero(CurrentCraftingHero);

			UpdateAll();
		}

		private void SpendMaterials(WeaponDesign weaponDesign)
		{
			ItemRoster itemRoster = MobileParty.MainParty.ItemRoster;
			int[] smithingCostsForWeaponDesign = Config.Instance.SmithingModel.GetSmithingCostsForWeaponDesign(weaponDesign);
			for (int i = 0; i < smithingCostsForWeaponDesign.Length; i++)
			{
				if (smithingCostsForWeaponDesign[i] != 0)
				{
					itemRoster.AddToCounts(Config.Instance.SmithingModel.GetCraftingMaterialItem((CraftingMaterials)i), smithingCostsForWeaponDesign[i]);
				}
			}
		}

		private void SpendMaterials()
		{
			ItemRoster itemRoster = MobileParty.MainParty.ItemRoster;
			int[] smithingCostsForArmorCrafting = Config.Instance.SmithingModel.GetCraftingInputForArmor(ArmorCrafting.CurrentItem.Item);
			for (int i = 0; i < (int)CraftingMaterials.NumCraftingMats; i++)
			{
				if (smithingCostsForArmorCrafting[i] != 0)
				{
					itemRoster.AddToCounts(Config.Instance.SmithingModel.GetCraftingMaterialItem((CraftingMaterials)i), smithingCostsForArmorCrafting[i]);
				}
			}

			for (int i = 0; i < (int)ExtraCraftingMaterials.NumExtraCraftingMats; i++)
			{
				if (smithingCostsForArmorCrafting[(int)CraftingMaterials.NumCraftingMats + i] != 0)
				{
					itemRoster.AddToCounts(Config.Instance.SmithingModel.GetCraftingMaterialItem((ExtraCraftingMaterials)i), smithingCostsForArmorCrafting[(int)CraftingMaterials.NumCraftingMats + i]);
				}
			}
		}

		[DataSourceMethod]
		public void ExecuteMainActionBannerCraft()
		{
			if (_craftingVM.IsInRefinementMode || _craftingVM.IsInSmeltingMode)
			{
				_craftingVM.ExecuteMainAction();
				return;
			}

			ICraftingCampaignBehavior craftingBehavior = Campaign.Current.GetCampaignBehavior<ICraftingCampaignBehavior>();

			if (!HaveMaterialsNeeded() || !HaveEnergy())
			{
				return;
			}

			int craftingXp;
			if (!IsInArmorMode)
			{
				float botchChance;
				if (_craftingVM.WeaponDesign.IsInOrderMode)
				{
					botchChance = Config.Instance.SmithingModel.CalculateBotchingChance(_craftingVM.CurrentCraftingHero.Hero, _craftingVM.WeaponDesign.CurrentOrderDifficulty);
				}
				else
				{
					botchChance = Config.Instance.SmithingModel.CalculateBotchingChance(_craftingVM.CurrentCraftingHero.Hero, _craftingVM.WeaponDesign.CurrentDifficulty);
				}

				if (MBRandom.RandomFloat < botchChance)
				{
					SpendMaterials(_crafting.CurrentWeaponDesign);
					
					/*
					 * Crafting is botched, materials spent, item not crafted
					 */
					MBInformationManager.AddQuickInformation(new TextObject("{=A15k4LQS}{HERO} has botched {ITEM}!")
							.SetTextVariable("HERO", CurrentCraftingHero.Hero.Name)
							.SetTextVariable("ITEM", _crafting.CraftedWeaponName),
						0, null, "event:/ui/notification/relation");


					int energyCostForSmithing = Config.Instance.SmithingModel.GetEnergyCostForSmithing(_crafting.GetCurrentCraftedItemObject(), CurrentCraftingHero.Hero) / 2;
					craftingBehavior.SetHeroCraftingStamina(CurrentCraftingHero.Hero, craftingBehavior.GetHeroCraftingStamina(CurrentCraftingHero.Hero) - energyCostForSmithing);
				}
				else
				{
					_craftingVM.ExecuteMainAction();
				}
			}
			else
			{
				float botchChance = Config.Instance.SmithingModel.CalculateBotchingChance(_craftingVM.CurrentCraftingHero.Hero, ArmorCrafting.CurrentItem.Difficulty);

				SpendMaterials();

				int energyCostForCrafting = Config.Instance.SmithingModel.GetEnergyCostForArmor(ArmorCrafting.CurrentItem.Item, CurrentCraftingHero.Hero);

				craftingXp = Config.Instance.SmithingModel.GetSkillXpForSmithingInFreeBuildMode(ArmorCrafting.CurrentItem.Item);

				if (MBRandom.RandomFloat < botchChance)
				{
					/*
					 * Crafting is botched, materials spent, item not crafted
					 */
					MBInformationManager.AddQuickInformation(new TextObject("{=A15k4LQS}{HERO} has botched {ITEM}!")
							.SetTextVariable("HERO", CurrentCraftingHero.Hero.Name)
							.SetTextVariable("ITEM", ArmorCrafting.CurrentItem.Item.Name),
						0, null, "event:/ui/notification/relation");

					energyCostForCrafting /= 2;
				}
				else
				{
					EquipmentElement element = new EquipmentElement(ArmorCrafting.CurrentItem.Item);

					int modifierTier = Config.Instance.SmithingModel.GetModifierTierForItem(ArmorCrafting.CurrentItem.Item, CurrentCraftingHero.Hero);
					if (modifierTier >= 0)
					{
						/*
						 * Non-negative modifier tiers are for the special ones
						 */
						ItemModifierGroup modifierGroup = null;
						if (   ArmorCrafting.CurrentItem.Item.HasArmorComponent
							&& ArmorCrafting.CurrentItem.Item.ArmorComponent.ItemModifierGroup != null)
						{
							modifierGroup = ArmorCrafting.CurrentItem.Item.ArmorComponent.ItemModifierGroup;
						}
						else if (   ArmorCrafting.CurrentItem.Item.HasArmorComponent
							     && ArmorCrafting.CurrentItem.Item.ArmorComponent.ItemModifierGroup == null)
                        {
							var dict = new Dictionary<ArmorComponent.ArmorMaterialTypes, string>
							{
								{ ArmorComponent.ArmorMaterialTypes.Plate, "plate" },
								{ ArmorComponent.ArmorMaterialTypes.Chainmail, "chain" },
								{ ArmorComponent.ArmorMaterialTypes.Leather, "leather" },
								{ ArmorComponent.ArmorMaterialTypes.Cloth, "cloth" },
								{ ArmorComponent.ArmorMaterialTypes.None, "cloth_unarmored" }
							};

							var lookup = dict[ArmorCrafting.CurrentItem.Item.ArmorComponent.MaterialType];
							modifierGroup = Game.Current.ObjectManager.GetObjectTypeList<ItemModifierGroup>().FirstOrDefault((ItemModifierGroup x) => x.GetName().ToString().ToLower() == lookup);
                        }
						else if (   ArmorCrafting.CurrentItem.Item.HasWeaponComponent
								 && ArmorCrafting.CurrentItem.Item.WeaponComponent.ItemModifierGroup != null)
						{
							modifierGroup = ArmorCrafting.CurrentItem.Item.WeaponComponent.ItemModifierGroup;
						}
						ItemModifier modifier = modifierGroup?.GetRandomModifierWithTarget(modifierTier) ?? null;

						if (modifier != null)
						{
							element.SetModifier(modifier);
						}
					}

					ArmorCrafting.CreateCraftingResultPopup(element);
					MobileParty.MainParty.ItemRoster.AddToCounts(element, 1);
				}

				craftingBehavior.SetHeroCraftingStamina(CurrentCraftingHero.Hero, craftingBehavior.GetHeroCraftingStamina(CurrentCraftingHero.Hero) - energyCostForCrafting);
				CurrentCraftingHero.Hero.AddSkillXp(DefaultSkills.Crafting, craftingXp);

				ArmorCrafting.UpdateCraftingHero(CurrentCraftingHero);
			}

			UpdateAll();
		}

		[DataSourceMethod]
		public void ExecuteSwitchToArmor()
		{
			_craftingVM.IsInSmeltingMode = false;
			_craftingVM.IsInCraftingMode = false;
			_craftingVM.IsInRefinementMode = false;
			IsInArmorMode = true;

			ViewModel?.OnItemRefreshed?.Invoke(isItemVisible: false);

			string t = GameTexts.FindText("str_bannercraft_crafting_category_armor").ToString();
			_craftingVM.CurrentCategoryText = t;
			_craftingVM.MainActionText = t;

			ArmorCrafting?.UpdateCraftingHero(CurrentCraftingHero);

			UpdateAll();
		}

		[DataSourceMethod]
		public void CloseWithWait()
		{
			_craftingVM.ExecuteCancel();
		}

		[DataSourceProperty]
		public MBBindingList<CraftingAvailableHeroItemVM> AvailableCharactersForSmithing
		{
			get => _craftingVM.AvailableCharactersForSmithing;
			set
			{
				if (value != _craftingVM.AvailableCharactersForSmithing)
				{
					_craftingVM.AvailableCharactersForSmithing = value;
					ViewModel!.OnPropertyChangedWithValue(value, "AvailableCharactersForSmithing");
				}
			}
		}

		[DataSourceProperty]
		public CraftingAvailableHeroItemVM CurrentCraftingHero
		{
			get => _craftingVM.CurrentCraftingHero;
			set
			{
				if (value != _craftingVM.CurrentCraftingHero)
				{
					_craftingVM.CurrentCraftingHero = value;
					ViewModel!.OnPropertyChangedWithValue(value, "CurrentCraftingHero");
				}
			}
		}

		public void UpdateCraftingHero(CraftingAvailableHeroItemVM newHero)
		{
			_craftingVM.UpdateCraftingHero(newHero);

			ArmorCrafting.UpdateCraftingHero(newHero);

			UpdateCurrentMaterialCosts();

			RefreshEnableMainAction();
			UpdateCraftingSkills();
		}
	}
}