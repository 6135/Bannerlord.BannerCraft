using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bannerlord.BannerCraft.Models;
using Bannerlord.BannerCraft.ViewModels;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using HarmonyLib;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Bannerlord.BannerCraft.Mixins
{
    [ViewModelMixin("UpdateAll")]
    public class CraftingMixin : BaseViewModelMixin<CraftingVM>
    {
        private readonly Crafting _crafting;
        private readonly CraftingVM _craftingVM;
        private ArmorCraftingVM _armorCrafting;

        private readonly ICraftingCampaignBehavior _craftingBehavior;

        private bool _isInArmorMode;
        private string _armorText;

        private MBBindingList<ExtraMaterialItemVM> _craftingResourceItems;

        public CraftingMixin(CraftingVM vm) : base(vm)
        {
            _craftingVM = vm;

            _crafting = Traverse.Create(vm).Field("_crafting").GetValue<Crafting>();
            _craftingBehavior = Campaign.Current.GetCampaignBehavior<ICraftingCampaignBehavior>();
            _armorText = GameTexts.FindText("str_bannercraft_crafting_category_armor").ToString();
            _armorCrafting = new ArmorCraftingVM(this, vm, _crafting);

            var availableCharacters = _craftingVM.AvailableCharactersForSmithing;
            if (Campaign.Current.GameMode == CampaignGameMode.Campaign)
            {
                availableCharacters.Clear();
                foreach (Hero item in CraftingHelper.GetAvailableHeroesForCrafting())
                {
                    availableCharacters.Add(new CraftingAvailableHeroItemVM(item, UpdateCraftingHero));
                }
            }

            _craftingVM.CurrentCraftingHero = availableCharacters.FirstOrDefault();
            _craftingVM.ExecuteSwitchToCrafting();

            UpdateAll();
        }

        [DataSourceProperty]
        public bool IsInArmorMode
        {
            get => _isInArmorMode;
            set => SetField(ref _isInArmorMode, value, nameof(IsInArmorMode));
        }

        [DataSourceProperty]
        public string ArmorText
        {
            get => _armorText;
            set => SetField(ref _armorText, value, nameof(ArmorText));
        }

        [DataSourceProperty]
        public ArmorCraftingVM ArmorCrafting
        {
            get => _armorCrafting;
            set => SetField(ref _armorCrafting, value, nameof(ArmorCrafting));
        }

        [DataSourceProperty]
        public MBBindingList<ExtraMaterialItemVM> ExtraMaterials
        {
            get => _craftingResourceItems;
            set => SetField(ref _craftingResourceItems, value, nameof(ExtraMaterials));
        }

        public override void OnRefresh()
        {
            base.OnRefresh();

            if (_craftingVM.IsInCraftingMode || _craftingVM.IsInRefinementMode || _craftingVM.IsInSmeltingMode)
            {
                IsInArmorMode = false;
                return;
            }

            ArmorCrafting?.UpdateCraftingHero(_craftingVM.CurrentCraftingHero);

            UpdateAll();
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

            var baseSmithingModel = Campaign.Current.Models.SmithingModel;
            var smithingModel = baseSmithingModel as BannerCraftSmithingModel;

            if (smithingModel is null)
            {
                throw new InvalidOperationException("BannerCraft's SmithingModel is null.");
            }

            int craftingXp;
            if (!IsInArmorMode)
            {
                float botchChance;
                if (_craftingVM.WeaponDesign.IsInOrderMode)
                {
                    botchChance = smithingModel.CalculateBotchingChance(_craftingVM.CurrentCraftingHero.Hero, _craftingVM.WeaponDesign.CurrentOrderDifficulty);
                }
                else
                {
                    botchChance = smithingModel.CalculateBotchingChance(_craftingVM.CurrentCraftingHero.Hero, _craftingVM.WeaponDesign.CurrentDifficulty);
                }

                if (MBRandom.RandomFloat < botchChance)
                {
                    SpendMaterials(_crafting.CurrentWeaponDesign);

                    /*
					 * Crafting is botched, materials spent, item not crafted
					 */
                    MBInformationManager.AddQuickInformation(new TextObject("{=A15k4LQS}{HERO} has botched {ITEM}!")
                            .SetTextVariable("HERO", _craftingVM.CurrentCraftingHero.Hero.Name)
                            .SetTextVariable("ITEM", _crafting.CraftedWeaponName),
                        0, null, "event:/ui/notification/relation");

                    int energyCostForSmithing = smithingModel.GetEnergyCostForSmithing(_crafting.GetCurrentCraftedItemObject(), _craftingVM.CurrentCraftingHero.Hero) / 2;
                    craftingBehavior.SetHeroCraftingStamina(_craftingVM.CurrentCraftingHero.Hero, craftingBehavior.GetHeroCraftingStamina(_craftingVM.CurrentCraftingHero.Hero) - energyCostForSmithing);
                }
                else
                {
                    _craftingVM.ExecuteMainAction();
                }
            }
            else
            {
                float botchChance = smithingModel.CalculateBotchingChance(_craftingVM.CurrentCraftingHero.Hero, ArmorCrafting.CurrentItem.Difficulty);

                SpendMaterials();

                int energyCostForCrafting = smithingModel.GetEnergyCostForArmor(ArmorCrafting.CurrentItem.Item, _craftingVM.CurrentCraftingHero.Hero);

                craftingXp = smithingModel.GetSkillXpForSmithingInFreeBuildMode(ArmorCrafting.CurrentItem.Item);

                if (MBRandom.RandomFloat < botchChance)
                {
                    /*
					 * Crafting is botched, materials spent, item not crafted
					 */
                    MBInformationManager.AddQuickInformation(new TextObject("{=A15k4LQS}{HERO} has botched {ITEM}!")
                            .SetTextVariable("HERO", _craftingVM.CurrentCraftingHero.Hero.Name)
                            .SetTextVariable("ITEM", ArmorCrafting.CurrentItem.Item.Name),
                        0, null, "event:/ui/notification/relation");

                    energyCostForCrafting /= 2;
                }
                else
                {
                    EquipmentElement element = new EquipmentElement(ArmorCrafting.CurrentItem.Item);

                    int modifierTier = smithingModel.GetModifierTierForItem(ArmorCrafting.CurrentItem.Item, _craftingVM.CurrentCraftingHero.Hero);
                    if (modifierTier >= 0)
                    {
                        /*
						 * Non-negative modifier tiers are for the special ones
						 */
                        ItemModifierGroup modifierGroup = null;
                        if (ArmorCrafting.CurrentItem.Item.HasArmorComponent
                            && ArmorCrafting.CurrentItem.Item.ArmorComponent.ItemModifierGroup != null)
                        {
                            modifierGroup = ArmorCrafting.CurrentItem.Item.ArmorComponent.ItemModifierGroup;
                        }
                        else if (ArmorCrafting.CurrentItem.Item.HasArmorComponent
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
                            modifierGroup = Game.Current.ObjectManager.GetObjectTypeList<ItemModifierGroup>().FirstOrDefault((x) => x.GetName().ToString().ToLower() == lookup);
                        }
                        else if (ArmorCrafting.CurrentItem.Item.HasWeaponComponent
                                 && ArmorCrafting.CurrentItem.Item.WeaponComponent.ItemModifierGroup != null)
                        {
                            modifierGroup = ArmorCrafting.CurrentItem.Item.WeaponComponent.ItemModifierGroup;
                        }

                        ItemModifier modifier = GetRandomModifierWithTarget(modifierGroup, modifierTier);

                        if (modifier != null)
                        {
                            element.SetModifier(modifier);
                        }
                    }

                    ArmorCrafting.CreateCraftingResultPopup(element);
                    MobileParty.MainParty.ItemRoster.AddToCounts(element, 1);
                }

                craftingBehavior.SetHeroCraftingStamina(_craftingVM.CurrentCraftingHero.Hero, craftingBehavior.GetHeroCraftingStamina(_craftingVM.CurrentCraftingHero.Hero) - energyCostForCrafting);
                _craftingVM.CurrentCraftingHero.Hero.AddSkillXp(DefaultSkills.Crafting, craftingXp);

                ArmorCrafting.UpdateCraftingHero(_craftingVM.CurrentCraftingHero);
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

            ArmorCrafting?.UpdateCraftingHero(_craftingVM.CurrentCraftingHero);

            UpdateAll();
        }

        [DataSourceMethod]
        public void CloseWithWait()
        {
            _craftingVM.ExecuteCancel();
        }

        public void UpdateCraftingHero(CraftingAvailableHeroItemVM newHero)
        {
            _craftingVM.UpdateCraftingHero(newHero);

            ArmorCrafting.UpdateCraftingHero(newHero);

            UpdateCurrentMaterialCosts();

            RefreshEnableMainAction();
            UpdateCraftingSkills();
        }

        private ItemModifier GetRandomModifierWithTarget(ItemModifierGroup modifierGroup, int modifierTier)
        {
            var results = modifierGroup.ItemModifiers.OrderByDescending(mod => mod.PriceMultiplier);

            return results.ElementAt(Math.Min(results.Count() - 1, modifierTier));
        }

        private int GetRequiredEnergy()
        {
            var smithingModel = Campaign.Current.Models.SmithingModel;

            if (_craftingVM.CurrentCraftingHero?.Hero != null)
            {
                if (IsInArmorMode)
                {
                    if (ArmorCrafting.CurrentItem != null && smithingModel is BannerCraftSmithingModel bcSmithingModel)
                    {
                        return bcSmithingModel.GetEnergyCostForArmor(ArmorCrafting.CurrentItem.Item, _craftingVM.CurrentCraftingHero.Hero);
                    }
                    else
                    {
                        return 0;
                    }
                }
                return smithingModel.GetEnergyCostForSmithing(_crafting.GetCurrentCraftedItemObject(), _craftingVM.CurrentCraftingHero.Hero);
            }
            return 0;
        }

        private bool HaveEnergy()
        {
            var baseSmithingModel = Campaign.Current.Models.SmithingModel;

            if (_craftingVM.CurrentCraftingHero?.Hero != null)
            {
                if (IsInArmorMode)
                {
                    if (ArmorCrafting.CurrentItem != null && baseSmithingModel is BannerCraftSmithingModel smithingModel)
                    {
                        return _craftingBehavior.GetHeroCraftingStamina(_craftingVM.CurrentCraftingHero.Hero) >= smithingModel.GetEnergyCostForArmor(ArmorCrafting.CurrentItem.Item, _craftingVM.CurrentCraftingHero.Hero);
                    }
                    return false;
                }
                return _craftingBehavior.GetHeroCraftingStamina(_craftingVM.CurrentCraftingHero.Hero) >= baseSmithingModel.GetEnergyCostForSmithing(_crafting.GetCurrentCraftedItemObject(), _craftingVM.CurrentCraftingHero.Hero);
            }

            return true;
        }

        private bool HaveMaterialsNeeded()
        {
            return !(_craftingVM.PlayerCurrentMaterials.Any((m) => m.ResourceChangeAmount + m.ResourceAmount < 0)
                     || ExtraMaterials.Any((m) => m.ResourceChangeAmount + m.ResourceAmount < 0));
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

            var baseSmithingModel = Campaign.Current.Models.SmithingModel;
            if (baseSmithingModel is BannerCraftSmithingModel smithingModel)
            {
                int[] craftingCostsForArmorCrafting = smithingModel.GetCraftingInputForArmor(ArmorCrafting.CurrentItem.Item);

                for (int i = 0; i < (int)CraftingMaterials.NumCraftingMats; i++)
                {
                    _craftingVM.PlayerCurrentMaterials[i].ResourceChangeAmount = craftingCostsForArmorCrafting[i];
                }

                for (int i = 0; i < (int)ExtraCraftingMaterials.NumExtraCraftingMats; i++)
                {
                    ExtraMaterials[i].ResourceChangeAmount = craftingCostsForArmorCrafting[(int)CraftingMaterials.NumCraftingMats + i];
                }
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
            var baseSmithingModel = Campaign.Current.Models.SmithingModel;

            if (baseSmithingModel is BannerCraftSmithingModel smithingModel)
            {
                if (ExtraMaterials == null)
                {
                    ExtraMaterials = new MBBindingList<ExtraMaterialItemVM>();
                    for (int i = 0; i < (int)ExtraCraftingMaterials.NumExtraCraftingMats; ++i)
                    {
                        var material = (ExtraCraftingMaterials)i;
                        ExtraMaterials.Add(new ExtraMaterialItemVM(material, smithingModel.GetCraftingMaterialItem(material), 0));
                    }
                }

                for (int i = 0; i < (int)ExtraCraftingMaterials.NumExtraCraftingMats; ++i)
                {
                    ItemObject extraCraftingMaterialItem = smithingModel.GetCraftingMaterialItem((ExtraCraftingMaterials)i);
                    ExtraMaterials[i].ResourceAmount = MobileParty.MainParty.ItemRoster.GetItemNumber(extraCraftingMaterialItem);
                }
            }
        }

        private void UpdateCraftingStamina()
        {
            foreach (CraftingAvailableHeroItemVM item in _craftingVM.AvailableCharactersForSmithing)
            {
                item.RefreshStamina();
            }
        }

        private void UpdateCraftingSkills()
        {
            foreach (CraftingAvailableHeroItemVM item in _craftingVM.AvailableCharactersForSmithing)
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
                _craftingVM.UpdateCraftingHero(_craftingVM.CurrentCraftingHero);
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
                            .SetTextVariable("HERONAME", _craftingVM.CurrentCraftingHero.Hero.Name.ToString())
                            .SetTextVariable("REQUIRED", GetRequiredEnergy())
                            .SetTextVariable("CURRENT", _craftingBehavior.GetHeroCraftingStamina(_craftingVM.CurrentCraftingHero.Hero)).ToString());
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

        private void SpendMaterials(WeaponDesign weaponDesign)
        {
            ItemRoster itemRoster = MobileParty.MainParty.ItemRoster;
            var smithingModel = Campaign.Current.Models.SmithingModel;
            int[] smithingCostsForWeaponDesign = smithingModel.GetSmithingCostsForWeaponDesign(weaponDesign);
            for (int i = 0; i < smithingCostsForWeaponDesign.Length; i++)
            {
                if (smithingCostsForWeaponDesign[i] != 0)
                {
                    itemRoster.AddToCounts(smithingModel.GetCraftingMaterialItem((CraftingMaterials)i), smithingCostsForWeaponDesign[i]);
                }
            }
        }

        private void SpendMaterials()
        {
            ItemRoster itemRoster = MobileParty.MainParty.ItemRoster;
            var baseSmithingModel = Campaign.Current.Models.SmithingModel;
            if (baseSmithingModel is BannerCraftSmithingModel smithingModel)
            {
                int[] smithingCostsForArmorCrafting = smithingModel.GetCraftingInputForArmor(ArmorCrafting.CurrentItem.Item);
                for (int i = 0; i < (int)CraftingMaterials.NumCraftingMats; i++)
                {
                    if (smithingCostsForArmorCrafting[i] != 0)
                    {
                        itemRoster.AddToCounts(baseSmithingModel.GetCraftingMaterialItem((CraftingMaterials)i), smithingCostsForArmorCrafting[i]);
                    }
                }

                for (int i = 0; i < (int)ExtraCraftingMaterials.NumExtraCraftingMats; i++)
                {
                    if (smithingCostsForArmorCrafting[(int)CraftingMaterials.NumCraftingMats + i] != 0)
                    {
                        itemRoster.AddToCounts(smithingModel.GetCraftingMaterialItem((ExtraCraftingMaterials)i), smithingCostsForArmorCrafting[(int)CraftingMaterials.NumCraftingMats + i]);
                    }
                }
            }
        }
    }
}