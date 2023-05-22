using System;
using System.Collections.Generic;
using System.Linq;
using Bannerlord.BannerCraft.Models;
using Bannerlord.BannerCraft.ViewModels;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using HarmonyLib;
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
        private ArmorCraftingVM _armorCraftingVm;

        private readonly ICraftingCampaignBehavior _craftingBehavior;

        private bool _isInArmorMode;
        private string _armorText;

        private MBBindingList<ExtraMaterialItemVM> _craftingResourceItems;

        public CraftingMixin(CraftingVM vm) : base(vm)
        {
            _crafting = Traverse.Create(vm).Field("_crafting").GetValue<Crafting>();
            _craftingBehavior = Campaign.Current.GetCampaignBehavior<ICraftingCampaignBehavior>();
            _armorText = GameTexts.FindText("str_bannercraft_crafting_category_armor").ToString();
            _armorCraftingVm = new ArmorCraftingVM(this, vm, _crafting);

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
            get => _armorCraftingVm;
            set => SetField(ref _armorCraftingVm, value, nameof(ArmorCrafting));
        }

        [DataSourceProperty]
        public MBBindingList<ExtraMaterialItemVM> ExtraMaterials
        {
            get => _craftingResourceItems;
            set => SetField(ref _craftingResourceItems, value, nameof(ExtraMaterials));
        }

        [DataSourceMethod]
        public void ExecuteMainActionBannerCraft()
        {
            if (ViewModel.IsInRefinementMode || ViewModel.IsInSmeltingMode)
            {
                ViewModel.ExecuteMainAction();
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
                if (ViewModel.WeaponDesign.IsInOrderMode)
                {
                    botchChance = smithingModel.CalculateBotchingChance(ViewModel.CurrentCraftingHero.Hero, ViewModel.WeaponDesign.CurrentOrderDifficulty);
                }
                else
                {
                    botchChance = smithingModel.CalculateBotchingChance(ViewModel.CurrentCraftingHero.Hero, ViewModel.WeaponDesign.CurrentDifficulty);
                }

                if (MBRandom.RandomFloat < botchChance)
                {
                    SpendMaterials(_crafting.CurrentWeaponDesign);

                    /*
					 * Crafting is botched, materials spent, item not crafted
					 */
                    MBInformationManager.AddQuickInformation(new TextObject("{=A15k4LQS}{HERO} has botched {ITEM}!")
                            .SetTextVariable("HERO", ViewModel.CurrentCraftingHero.Hero.Name)
                            .SetTextVariable("ITEM", _crafting.CraftedWeaponName),
                        0, null, "event:/ui/notification/relation");

                    int energyCostForSmithing = smithingModel.GetEnergyCostForSmithing(_crafting.GetCurrentCraftedItemObject(), ViewModel.CurrentCraftingHero.Hero) / 2;
                    craftingBehavior.SetHeroCraftingStamina(ViewModel.CurrentCraftingHero.Hero, craftingBehavior.GetHeroCraftingStamina(ViewModel.CurrentCraftingHero.Hero) - energyCostForSmithing);
                }
                else
                {
                    ViewModel.ExecuteMainAction();
                }
            }
            else
            {
                float botchChance = smithingModel.CalculateBotchingChance(ViewModel.CurrentCraftingHero.Hero, ArmorCrafting.CurrentItem.Difficulty);

                SpendMaterials();

                int energyCostForCrafting = smithingModel.GetEnergyCostForArmor(ArmorCrafting.CurrentItem.Item, ViewModel.CurrentCraftingHero.Hero);

                craftingXp = smithingModel.GetSkillXpForSmithingInFreeBuildMode(ArmorCrafting.CurrentItem.Item);

                if (MBRandom.RandomFloat < botchChance)
                {
                    /*
					 * Crafting is botched, materials spent, item not crafted
					 */
                    MBInformationManager.AddQuickInformation(new TextObject("{=A15k4LQS}{HERO} has botched {ITEM}!")
                            .SetTextVariable("HERO", ViewModel.CurrentCraftingHero.Hero.Name)
                            .SetTextVariable("ITEM", ArmorCrafting.CurrentItem.Item.Name),
                        0, null, "event:/ui/notification/relation");

                    energyCostForCrafting /= 2;
                }
                else
                {
                    EquipmentElement element = new EquipmentElement(ArmorCrafting.CurrentItem.Item);

                    int modifierTier = smithingModel.GetModifierTierForItem(ArmorCrafting.CurrentItem.Item, ViewModel.CurrentCraftingHero.Hero);
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

                craftingBehavior.SetHeroCraftingStamina(ViewModel.CurrentCraftingHero.Hero, craftingBehavior.GetHeroCraftingStamina(ViewModel.CurrentCraftingHero.Hero) - energyCostForCrafting);
                ViewModel.CurrentCraftingHero.Hero.AddSkillXp(DefaultSkills.Crafting, craftingXp);

                ArmorCrafting.UpdateCraftingHero(ViewModel.CurrentCraftingHero);
            }

            UpdateAll();
        }

        [DataSourceMethod]
        public void ExecuteSwitchToArmor()
        {
            ViewModel.IsInSmeltingMode = false;
            ViewModel.IsInCraftingMode = false;
            ViewModel.IsInRefinementMode = false;
            IsInArmorMode = true;

            ViewModel?.OnItemRefreshed?.Invoke(isItemVisible: false);

            string t = GameTexts.FindText("str_bannercraft_crafting_category_armor").ToString();
            ViewModel.CurrentCategoryText = t;
            ViewModel.MainActionText = t;

            ArmorCrafting?.UpdateCraftingHero(ViewModel.CurrentCraftingHero);

            UpdateAll();
        }

        [DataSourceMethod]
        public void CloseWithWait()
        {
            ViewModel.ExecuteCancel();
        }

        public override void OnRefresh()
        {
            base.OnRefresh();

            if (ViewModel.IsInCraftingMode || ViewModel.IsInRefinementMode || ViewModel.IsInSmeltingMode)
            {
                IsInArmorMode = false;
                return;
            }

            ArmorCrafting?.UpdateCraftingHero(ViewModel.CurrentCraftingHero);

            UpdateAll();
        }

        private ItemModifier GetRandomModifierWithTarget(ItemModifierGroup modifierGroup, int modifierTier)
        {
            var results = modifierGroup.ItemModifiers.OrderByDescending(mod => mod.PriceMultiplier);

            return results.ElementAt(Math.Min(results.Count() - 1, modifierTier));
        }

        private int GetRequiredEnergy()
        {
            var smithingModel = Campaign.Current.Models.SmithingModel;

            if (ViewModel.CurrentCraftingHero?.Hero != null)
            {
                if (IsInArmorMode)
                {
                    if (ArmorCrafting.CurrentItem != null && smithingModel is BannerCraftSmithingModel bcSmithingModel)
                    {
                        return bcSmithingModel.GetEnergyCostForArmor(ArmorCrafting.CurrentItem.Item, ViewModel.CurrentCraftingHero.Hero);
                    }
                    else
                    {
                        return 0;
                    }
                }
                return smithingModel.GetEnergyCostForSmithing(_crafting.GetCurrentCraftedItemObject(), ViewModel.CurrentCraftingHero.Hero);
            }
            return 0;
        }

        private bool HaveEnergy()
        {
            var baseSmithingModel = Campaign.Current.Models.SmithingModel;

            if (ViewModel.CurrentCraftingHero?.Hero != null)
            {
                if (IsInArmorMode)
                {
                    if (ArmorCrafting.CurrentItem != null && baseSmithingModel is BannerCraftSmithingModel smithingModel)
                    {
                        return _craftingBehavior.GetHeroCraftingStamina(ViewModel.CurrentCraftingHero.Hero) >= smithingModel.GetEnergyCostForArmor(ArmorCrafting.CurrentItem.Item, ViewModel.CurrentCraftingHero.Hero);
                    }
                    return false;
                }
                return _craftingBehavior.GetHeroCraftingStamina(ViewModel.CurrentCraftingHero.Hero) >= baseSmithingModel.GetEnergyCostForSmithing(_crafting.GetCurrentCraftedItemObject(), ViewModel.CurrentCraftingHero.Hero);
            }

            return true;
        }

        private bool HaveMaterialsNeeded()
        {
            return !(ViewModel.PlayerCurrentMaterials.Any((m) => m.ResourceChangeAmount + m.ResourceAmount < 0)
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
                    ViewModel.PlayerCurrentMaterials[i].ResourceChangeAmount = 0;
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
                    ViewModel.PlayerCurrentMaterials[i].ResourceChangeAmount = craftingCostsForArmorCrafting[i];
                }

                for (int i = 0; i < (int)ExtraCraftingMaterials.NumExtraCraftingMats; i++)
                {
                    ExtraMaterials[i].ResourceChangeAmount = craftingCostsForArmorCrafting[(int)CraftingMaterials.NumCraftingMats + i];
                }
            }
        }

        private void UpdateCurrentMaterialsAvailable()
        {
            if (ViewModel.PlayerCurrentMaterials == null)
            {
                ViewModel.PlayerCurrentMaterials = new MBBindingList<CraftingResourceItemVM>();
                for (int i = 0; i < (int)CraftingMaterials.NumCraftingMats; i++)
                {
                    ViewModel.PlayerCurrentMaterials.Add(new CraftingResourceItemVM((CraftingMaterials)i, 0));
                }
            }

            for (int i = 0; i < (int)CraftingMaterials.NumCraftingMats; i++)
            {
                ItemObject craftingMaterialItem = Campaign.Current.Models.SmithingModel.GetCraftingMaterialItem((CraftingMaterials)i);
                ViewModel.PlayerCurrentMaterials[i].ResourceAmount = MobileParty.MainParty.ItemRoster.GetItemNumber(craftingMaterialItem);
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
            foreach (CraftingAvailableHeroItemVM item in ViewModel.AvailableCharactersForSmithing)
            {
                item.RefreshStamina();
            }
        }

        private void UpdateCraftingSkills()
        {
            foreach (CraftingAvailableHeroItemVM item in ViewModel.AvailableCharactersForSmithing)
            {
                item.RefreshSkills();
            }
        }

        private void RefreshEnableMainAction()
        {
            if (Campaign.Current.GameMode == CampaignGameMode.Tutorial)
            {
                ViewModel.IsMainActionEnabled = true;
                return;
            }

            if (!IsInArmorMode)
            {
                /*
				 * This is stupid as hell, but CraftingVM.RefreshEnableMainAction is private
				 * UpdateCraftingHero is the only public function that calls it
				 */
                ViewModel.UpdateCraftingHero(ViewModel.CurrentCraftingHero);
                return;
            }

            UpdateCurrentMaterialsAvailable();
            UpdateExtraMaterialsAvailable();

            ViewModel.IsMainActionEnabled = true;
            if (!HaveEnergy())
            {
                ViewModel.IsMainActionEnabled = false;
                if (ViewModel.MainActionHint != null)
                {
                    ViewModel.MainActionHint = new BasicTooltipViewModel(() =>
                        GameTexts.FindText("str_bannercraft_crafting_stamina_display")
                            .SetTextVariable("HERONAME", ViewModel.CurrentCraftingHero.Hero.Name.ToString())
                            .SetTextVariable("REQUIRED", GetRequiredEnergy())
                            .SetTextVariable("CURRENT", _craftingBehavior.GetHeroCraftingStamina(ViewModel.CurrentCraftingHero.Hero)).ToString());
                }
            }
            else if (!HaveMaterialsNeeded())
            {
                ViewModel.IsMainActionEnabled = false;
                if (ViewModel.MainActionHint != null)
                {
                    ViewModel.MainActionHint = new BasicTooltipViewModel(() => new TextObject("{=gduqxfck}You don't have all required materials!").ToString());
                }
            }
            else if (ArmorCrafting.CurrentItem == null)
            {
                ViewModel.IsMainActionEnabled = false;
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