using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bannerlord.BannerCraft.Models;
using Bannerlord.BannerCraft.ViewModels;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Bannerlord.BannerCraft.Mixins
{
    [ViewModelMixin("RefreshValues")]
    public class CraftingMixin : BaseViewModelMixin<CraftingVM>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0090:Use 'new(...)'", Justification = "<Pending>")]
        private static readonly Dictionary<ArmorComponent.ArmorMaterialTypes, string> MaterialTypeMap = new Dictionary<ArmorComponent.ArmorMaterialTypes, string>
        {
            { ArmorComponent.ArmorMaterialTypes.Plate, "plate" },
            { ArmorComponent.ArmorMaterialTypes.Chainmail, "chain" },
            { ArmorComponent.ArmorMaterialTypes.Leather, "leather" },
            { ArmorComponent.ArmorMaterialTypes.Cloth, "cloth" },
            { ArmorComponent.ArmorMaterialTypes.None, "cloth_unarmored" }
        };

        public static void ApplyPatches(Harmony harmony)
        {
            harmony.Patch(
                AccessTools.PropertySetter(typeof(CraftingVM), nameof(CraftingVM.IsInCraftingMode)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(CraftingMixin), nameof(ModePropertiesPostfix))));
            harmony.Patch(
                AccessTools.PropertySetter(typeof(CraftingVM), nameof(CraftingVM.IsInRefinementMode)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(CraftingMixin), nameof(ModePropertiesPostfix))));
            harmony.Patch(
                AccessTools.PropertySetter(typeof(CraftingVM), nameof(CraftingVM.IsInSmeltingMode)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(CraftingMixin), nameof(ModePropertiesPostfix))));
        }

        private static void ModePropertiesPostfix(ref CraftingVM __instance)
        {
            if (__instance.GetPropertyValue("Mixin") is WeakReference<CraftingMixin> weakReference
                && weakReference.TryGetTarget(out var mixin))
            {
                if (__instance.IsInCraftingMode || __instance.IsInRefinementMode || __instance.IsInSmeltingMode)
                {
                    mixin.IsInArmorMode = false;
                }
            }
        }

        private readonly Crafting _crafting;
        private ArmorCraftingVM _armorCraftingVm;

        private bool _isInArmorMode;
        private string _armorText;

        private MBBindingList<ExtraMaterialItemVM> _craftingResourceItems;

        private readonly MethodInfo _updateAllBase;
        private readonly MethodInfo _refreshEnableMainActionBase;

        public CraftingMixin(CraftingVM vm) : base(vm)
        {
            _crafting = Traverse.Create(vm).Field("_crafting").GetValue<Crafting>();
            _armorText = GameTexts.FindText("str_bannercraft_crafting_category_armor").ToString();
            _armorCraftingVm = new ArmorCraftingVM(this, vm, _crafting);

             //TODO: Optimize this, somehow
            _updateAllBase = AccessTools.Method(typeof(CraftingVM), "UpdateAll");
            _refreshEnableMainActionBase = AccessTools.Method(typeof(CraftingVM), "RefreshEnableMainAction");

            UpdateAll();
        }

        [DataSourceProperty]
        public WeakReference<CraftingMixin> Mixin => new(this);

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
            if (ViewModel is null)
            {
                return;
            }

            if (ViewModel.IsInRefinementMode || ViewModel.IsInSmeltingMode)
            {
                ViewModel.ExecuteMainAction();
                return;
            }

            var craftingBehavior = Campaign.Current.GetCampaignBehavior<ICraftingCampaignBehavior>();

            if (Campaign.Current.Models.SmithingModel is not BannerCraftSmithingModel smithingModel)
            {
                throw new InvalidOperationException("BannerCraft's SmithingModel is null.");
            }

            var hero = ViewModel.CurrentCraftingHero.Hero;

            if (!HaveMaterialsNeeded() || !HaveEnergy(hero))
            {
                //print to chat that we don't have enough materials
                InformationManager.DisplayMessage(new InformationMessage("Not enough materials"));
                return;
            }

            var difficulty = ArmorCrafting.CurrentItem.Difficulty;
            float botchChance = smithingModel.CalculateBotchingChance(hero, difficulty);

            SpendMaterials();

            var item = ArmorCrafting.CurrentItem.Item;
            int energyCost = smithingModel.GetEnergyCostForArmor(item, hero);

            if (MBRandom.RandomFloat < botchChance)
            {
                /*
                 * Crafting is botched, materials spent, item not crafted
                 */
                MBInformationManager.AddQuickInformation(new TextObject("{=A15k4LQS}{HERO} has botched {ITEM}!")
                        .SetTextVariable("HERO", hero.Name)
                        .SetTextVariable("ITEM", item.Name),
                    0, null, "event:/ui/notification/relation");

                energyCost /= 2;
            }
            else
            {
                CraftItem(smithingModel, hero, item);
            }

            UpdateStamina(craftingBehavior, hero, energyCost);
            UpdateXp(smithingModel, hero, item);

            ArmorCrafting.UpdateCraftingHero(hero);

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

            ArmorCrafting?.UpdateCraftingHero(ViewModel.CurrentCraftingHero.Hero);

            UpdateAll();
        }

        public override void OnRefresh()
        {
            base.OnRefresh();
            ArmorCrafting?.UpdateCraftingHero(ViewModel.CurrentCraftingHero.Hero);

            UpdateAll();
        }

        private void UpdateAll()
        {

            _updateAllBase.Invoke(ViewModel, Array.Empty<object>());

            UpdateExtraMaterialsAvailable();
            UpdateCurrentMaterialCosts();
            RefreshEnableMainAction();
        }

        private void CraftItem(BannerCraftSmithingModel smithingModel, Hero hero, ItemObject item)
        {
            EquipmentElement element = new EquipmentElement(ArmorCrafting.CurrentItem.Item);

            int modifierTier = smithingModel.GetModifierTierForItem(item, hero);
            if (modifierTier >= 0)
            {
                /*
                 * Non-negative modifier tiers are for the special ones
                 */
                ItemModifierGroup? modifierGroup = null;
                if (item.HasArmorComponent)
                {
                    if (item.ArmorComponent.ItemModifierGroup is null)
                    {
                        var lookup = MaterialTypeMap[item.ArmorComponent.MaterialType];
                        modifierGroup = Game.Current.ObjectManager.GetObjectTypeList<ItemModifierGroup>()
                            .Find((x) => x.GetName().ToString().ToLower() == lookup);
                    }
                    else
                    {
                        modifierGroup = item.ArmorComponent.ItemModifierGroup;
                    }
                }
                else if (item.HasWeaponComponent && item.WeaponComponent.ItemModifierGroup != null)
                {
                    modifierGroup = item.WeaponComponent.ItemModifierGroup;
                }

                if (modifierGroup is not null)
                {
                    ItemModifier modifier = GetRandomModifierWithTarget(modifierGroup, modifierTier);
                    element.SetModifier(modifier);
                }
            }

            ArmorCrafting.CreateCraftingResultPopup(element);
            MobileParty.MainParty.ItemRoster.AddToCounts(element, 1);
        }

        private ItemModifier GetRandomModifierWithTarget(ItemModifierGroup modifierGroup, int modifierTier)
        {
            var results = modifierGroup.ItemModifiers.OrderByDescending(mod => mod.PriceMultiplier);

            return results.ElementAt(Math.Min(results.Count() - 1, modifierTier));
        }

        private int GetRequiredEnergy(Hero hero)
        {
            var baseSmithingModel = Campaign.Current.Models.SmithingModel;
            int result;

            if (IsInArmorMode && baseSmithingModel is BannerCraftSmithingModel smithingModel)
            {
                var item = ArmorCrafting.CurrentItem.Item;
                result = smithingModel.GetEnergyCostForArmor(item, hero);
            }
            else
            {
                var item = _crafting.GetCurrentCraftedItemObject();
                result = baseSmithingModel.GetEnergyCostForSmithing(item, hero);
            }

            return result;
        }

        private bool HaveEnergy(Hero hero)
        {
            var craftingBehavior = Campaign.Current.GetCampaignBehavior<ICraftingCampaignBehavior>();
            var stamina = craftingBehavior.GetHeroCraftingStamina(hero);
            int energyCost = GetRequiredEnergy(hero);

            return stamina >= energyCost;
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
            var item = ArmorCrafting.CurrentItem.Item;
            if (baseSmithingModel is BannerCraftSmithingModel smithingModel)
            {
                int[] craftingCostsForArmorCrafting = smithingModel.GetCraftingInputForArmor(item);

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
                        var item = smithingModel.GetCraftingMaterialItem(material);
                        ExtraMaterials.Add(new ExtraMaterialItemVM(material, item, 0));
                    }
                }

                for (int i = 0; i < (int)ExtraCraftingMaterials.NumExtraCraftingMats; ++i)
                {
                    var extraCraftingMaterialItem = smithingModel.GetCraftingMaterialItem((ExtraCraftingMaterials)i);
                    ExtraMaterials[i].ResourceAmount = MobileParty.MainParty.ItemRoster.GetItemNumber(extraCraftingMaterialItem);
                }
            }
        }

        private void RefreshEnableMainAction()
        {
            _refreshEnableMainActionBase.Invoke(ViewModel, Array.Empty<object>());

            var craftingBehavior = Campaign.Current.GetCampaignBehavior<ICraftingCampaignBehavior>();
            var hero = ViewModel.CurrentCraftingHero.Hero;

            ViewModel.IsMainActionEnabled = true;
            if (!HaveEnergy(hero))
            {
                var stamina = craftingBehavior.GetHeroCraftingStamina(hero);
                var requiredStamina = GetRequiredEnergy(hero);
                ViewModel.IsMainActionEnabled = false;
                ViewModel.MainActionHint = new BasicTooltipViewModel(() =>
                    GameTexts.FindText("str_bannercraft_crafting_stamina_display")
                        .SetTextVariable("HERONAME", hero.Name.ToString())
                        .SetTextVariable("REQUIRED", requiredStamina)
                        .SetTextVariable("CURRENT", stamina).ToString());
            }
            else if (!HaveMaterialsNeeded())
            {
                ViewModel.IsMainActionEnabled = false;
                ViewModel.MainActionHint = new BasicTooltipViewModel(() =>
                {
                    return new TextObject("{=gduqxfck}You don't have all required materials!").ToString();
                });
            }
            else if (ArmorCrafting.CurrentItem == null)
            {
                ViewModel.IsMainActionEnabled = false;
            }
        }

        private void SpendMaterials()
        {
            ItemRoster itemRoster = MobileParty.MainParty.ItemRoster;
            var baseSmithingModel = Campaign.Current.Models.SmithingModel;
            if (baseSmithingModel is BannerCraftSmithingModel smithingModel)
            {
                var item = ArmorCrafting.CurrentItem.Item;
                int[] smithingCostsForArmorCrafting = smithingModel.GetCraftingInputForArmor(item);
                for (int i = 0; i < (int)CraftingMaterials.NumCraftingMats; i++)
                {
                    if (smithingCostsForArmorCrafting[i] != 0)
                    {
                        var materialItem = baseSmithingModel.GetCraftingMaterialItem((CraftingMaterials)i);
                        itemRoster.AddToCounts(materialItem, smithingCostsForArmorCrafting[i]);
                    }
                }

                for (int i = 0; i < (int)ExtraCraftingMaterials.NumExtraCraftingMats; i++)
                {
                    if (smithingCostsForArmorCrafting[(int)CraftingMaterials.NumCraftingMats + i] != 0)
                    {
                        var materialItem = smithingModel.GetCraftingMaterialItem((ExtraCraftingMaterials)i);
                        var cost = smithingCostsForArmorCrafting[(int)CraftingMaterials.NumCraftingMats + i];
                        itemRoster.AddToCounts(materialItem, cost);
                    }
                }
            }
        }

        private void UpdateStamina(ICraftingCampaignBehavior craftingBehavior, Hero hero, int energyCost)
        {
            var currentStamina = craftingBehavior.GetHeroCraftingStamina(hero);
            craftingBehavior.SetHeroCraftingStamina(hero, currentStamina - energyCost);
        }

        private void UpdateXp(SmithingModel smithingModel, Hero hero, ItemObject item)
        {
            var craftingXp = smithingModel.GetSkillXpForSmithingInFreeBuildMode(item);
            hero.AddSkillXp(DefaultSkills.Crafting, craftingXp);
        }
    }
}