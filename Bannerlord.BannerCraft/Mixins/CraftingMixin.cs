using Bannerlord.BannerCraft.Models;
using Bannerlord.BannerCraft.ViewModels;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting.WeaponDesign;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Bannerlord.BannerCraft.Mixins
{
    [ViewModelMixin("RefreshValues")]
    public class CraftingMixin : BaseViewModelMixin<CraftingVM>
    {
        private static readonly Dictionary<ArmorComponent.ArmorMaterialTypes, string> MaterialTypeMap = new()
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
                && weakReference.TryGetTarget(out var mixin) && (__instance.IsInCraftingMode || __instance.IsInRefinementMode || __instance.IsInSmeltingMode))
            {
                mixin.IsInArmorMode = false;
                mixin.UpdateCurrentMaterialCosts();
            }
        }

        private readonly CraftingVM _craftingVm;
        private Crafting _crafting;
        private ArmorCraftingVM _armorCraftingVm;

        private bool _isInArmorMode;
        private string _armorText;

        private MBBindingList<ExtraMaterialItemVM>? _craftingResourceItems;

        private readonly MethodInfo _updateAllBase;
        private readonly MethodInfo _refreshEnableMainActionBase;

        private delegate T GetItemFieldDelegate<out T>(ItemModifier item, string _fieldName);

#if v116 || v115 || v114 || v113 || v112 || v111 || v110 || v103 || v102 || v101 || v100

        private string MountHitPoints = "_mountHitPoints";
        private string ChargeDamage = "_chargeDamage";
        private string Maneuver = "_maneuver";
        private string MountSpeed = "_mountSpeed";
        private string StackCount = "_stackCount";
        private string HitPoints = "_hitPoints";
        private string Armor = "_armor";
        private string MissileSpeed = "_missileSpeed";
        private string Speed = "_speed";
        private string Damage = "_damage";
        private string PriceMultiplier = "_priceMultiplier";

        private int GetItemFieldInt(ItemModifier item, string _fieldName)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            var value = item.GetType()?.GetField(_fieldName, bindingFlags)?.GetValue(item);
            if (value is not null)
                return (int)value;
            else return 0;
        }

        private short GetItemFieldShort(ItemModifier item, string _fieldName)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            var value = item.GetType()?.GetField(_fieldName, bindingFlags)?.GetValue(item);
            if (value is not null)
                return (short)value;
            else return 0;
        }

        //float
        private float GetItemFieldFloat(ItemModifier item, string _fieldName)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            var value = item.GetType()?.GetField(_fieldName, bindingFlags)?.GetValue(item);
            if (value is not null)
                return (float)value;
            else return 0;
        }

#else
        private readonly string MountHitPoints = "MountHitPoints";
        private readonly string ChargeDamage = "ChargeDamage";
        private readonly string Maneuver = "Maneuver";
        private readonly string MountSpeed = "MountSpeed";
        private readonly string StackCount = "StackCount";
        private readonly string HitPoints = "HitPoints";
        private readonly string Armor = "Armor";
        private readonly string MissileSpeed = "MissileSpeed";
        private readonly string Speed = "Speed";
        private readonly string Damage = "Damage";
        private readonly string PriceMultiplier = "PriceMultiplier";

        //they were into properties.
        private int GetItemFieldInt(ItemModifier item, string _fieldName)
        {
            var value = item.GetType()?.GetProperty(_fieldName)?.GetValue(item);
            if (value is not null)
                return (int)value;
            else return 0;
        }

        private short GetItemFieldShort(ItemModifier item, string _fieldName)
        {
            var value = item.GetType()?.GetProperty(_fieldName)?.GetValue(item);
            if (value is not null)
                return (short)value;
            else return 0;
        }

        //float
        private float GetItemFieldFloat(ItemModifier item, string _fieldName)
        {
            var value = item.GetType()?.GetProperty(_fieldName)?.GetValue(item);
            if (value is not null)
                return (float)value;
            else return 0;
        }

#endif

        private float GetModifierSum(ItemModifier im)
        {
            /*
             *           string MountHitPoints; //float
                        string ChargeDamage; //float
                        string Maneuver; //float
                        string MountSpeed; //float
                        string StackCount; //short
                        string HitPoints; //short
                        string Armor; //int
                        string MissileSpeed; //int
                        string Speed;   //int
                        string Damage; //int
                        string PriceMultiplier; //float*/
            //sorts by the sum of all the modifiers, then by price multiplier

            GetItemFieldDelegate<int>? getItemFieldDelegateInstanceInt = GetItemFieldInt;
            GetItemFieldDelegate<short>? getItemFieldDelegateInstanceShort = GetItemFieldShort;
            GetItemFieldDelegate<float>? getItemFieldDelegateInstanceFloat = GetItemFieldFloat;
            //sum of all the modifiers and multiply by the price multiplier, using /getItemFieldDelegateInstanceInt /and getItemFieldDelegateInstanceShort
            float sum =
                getItemFieldDelegateInstanceFloat(im, MountHitPoints) +
                getItemFieldDelegateInstanceFloat(im, ChargeDamage) +
                getItemFieldDelegateInstanceFloat(im, Maneuver) +
                getItemFieldDelegateInstanceFloat(im, MountSpeed) +
                getItemFieldDelegateInstanceShort(im, StackCount) +
                getItemFieldDelegateInstanceShort(im, HitPoints) +
                getItemFieldDelegateInstanceInt(im, Armor) +
                getItemFieldDelegateInstanceInt(im, MissileSpeed) +
                getItemFieldDelegateInstanceInt(im, Speed) +
                getItemFieldDelegateInstanceInt(im, Damage);
            //if sum is negative, abs it and multiply by the price multiplier and return it negative using branchless code

            if (sum < 0)
                return sum / getItemFieldDelegateInstanceFloat(im, PriceMultiplier);
            else
                return sum * getItemFieldDelegateInstanceFloat(im, PriceMultiplier);
        }

        public CraftingMixin(CraftingVM vm) : base(vm)
        {
            _craftingVm = vm;
            _crafting = Traverse.Create(vm).Field("_crafting").GetValue<Crafting>();
            _armorText = GameTexts.FindText("str_bannercraft_crafting_category_armor").ToString();
            _armorCraftingVm = new ArmorCraftingVM(this, vm, _crafting);

            //TODO: Optimize this, somehow
            _updateAllBase = AccessTools.Method(typeof(CraftingVM), "UpdateAll");
            _refreshEnableMainActionBase = AccessTools.Method(typeof(CraftingVM), "RefreshEnableMainAction");

            Mixin = new(this);

            UpdateAll();
        }

        [DataSourceProperty]
        public static WeakReference<CraftingMixin>? Mixin { get; set; }

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
        public MBBindingList<ExtraMaterialItemVM>? ExtraMaterials
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
            var smithingModel = Campaign.Current.Models.SmithingModel as BannerCraftSmithingModel;
            var hero = ViewModel.CurrentCraftingHero.Hero;
            bool noMaterialsRequired = Settings.Instance?.NoMaterialsRequired ?? false;
            bool noStaminaRequired = Settings.Instance?.NoStaminaRequired ?? false;
            bool noSkillRequired = Settings.Instance?.NoSkillRequired ?? false;

            if (smithingModel is null)
            {
                throw new InvalidOperationException("BannerCraft's SmithingModel is null.");
            }

            int energyCostForSmithing = 0;
            if (!IsInArmorMode)
            {
                float botchChance;
                float randomFloat = MBRandom.RandomFloat;
                int difficulty;
                if (_craftingVm.WeaponDesign.IsInOrderMode)
                {
                    difficulty = _craftingVm.WeaponDesign.CurrentOrderDifficulty;
                }
                else
                {
                    difficulty = _craftingVm.WeaponDesign.CurrentDifficulty;
                }
                botchChance = smithingModel.CalculateBotchingChance(_craftingVm.CurrentCraftingHero.Hero, difficulty);
                if (randomFloat < botchChance)
                {
                    SpendMaterials(_crafting.CurrentWeaponDesign);
                    MBInformationManager.AddQuickInformation(new TextObject("{=A15k4LQS}{HERO} has botched {ITEM}!")
                            .SetTextVariable("HERO", hero.Name)
                            .SetTextVariable("ITEM", _crafting.CraftedWeaponName),
                        2000, null, null, "event:/ui/notification/relation");

                    energyCostForSmithing = smithingModel.GetEnergyCostForSmithing(_crafting.GetCurrentCraftedItemObject(), hero) / 2;
                    UpdateStamina(craftingBehavior, hero, energyCostForSmithing);
                }
                else
                {
                    _craftingVm.ExecuteMainAction();
                }
            }
            else
            {
                if (!HaveMaterialsNeeded() || (!HaveEnergy(hero) && !noStaminaRequired))
                {
                    return;
                }
                var difficulty = noSkillRequired ? 0 : ArmorCrafting.CurrentItem?.Difficulty ?? 0;
                float botchChance = smithingModel.CalculateBotchingChance(hero, difficulty);
                var item = ArmorCrafting.CurrentItem.Item;
                energyCostForSmithing = noStaminaRequired ? 0 : smithingModel.GetEnergyCostForArmor(item, hero);

                if (!noMaterialsRequired)
                    SpendMaterials();

                if (MBRandom.RandomFloat < botchChance)
                {
                    /*
                     * Crafting is botched, materials spent, item not crafted
                     */
                    MBInformationManager.AddQuickInformation(new TextObject("{=A15k4LQS}{HERO} has botched {ITEM}!")
                            .SetTextVariable("HERO", hero.Name)
                            .SetTextVariable("ITEM", item.Name),
                        0, null, null, "event:/ui/notification/relation");

                    energyCostForSmithing /= 2;
                }
                else
                {
                    CraftItem(smithingModel, hero, item);
                }

                UpdateXp(smithingModel, hero, item);
                ArmorCrafting.UpdateCraftingHero(hero);
            }

            UpdateStamina(craftingBehavior, hero, energyCostForSmithing);
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

        public void OnCraftingLogicRefreshed(Crafting newCraftingLogic)
        {
            _crafting = newCraftingLogic;

            AccessTools.Method(typeof(WeaponDesignVM), "OnCraftingLogicRefreshed").Invoke(_craftingVm.WeaponDesign, new object[] { newCraftingLogic });
        }

        public override void OnRefresh()
        {
            base.OnRefresh();

            ArmorCrafting?.UpdateCraftingHero(ViewModel.CurrentCraftingHero.Hero);

            UpdateAll();
        }

        private void UpdateAll()
        {
            _updateAllBase?.Invoke(ViewModel, Array.Empty<object>());

            UpdateExtraMaterialsAvailable();
            UpdateCurrentMaterialCosts();
            RefreshEnableMainAction();
        }

        private void CraftItem(BannerCraftSmithingModel smithingModel, Hero hero, ItemObject item)
        {
            EquipmentElement element = new(ArmorCrafting.CurrentItem.Item);

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
                            .FirstOrDefault((x) => x.GetName().ToString().ToLower() == lookup);
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
                    ItemModifier? modifier = GetRandomModifierWithTarget(modifierGroup, modifierTier);
                    element.SetModifier(modifier);
                }
            }

            ArmorCrafting.CreateCraftingResultPopup(element);
            MobileParty.MainParty.ItemRoster.AddToCounts(element, 1);
        }

        private ItemModifier? GetRandomModifierWithTarget(ItemModifierGroup modifierGroup, int modifierTier)
        {
            var results = modifierGroup.ItemModifiers.OrderBy(mod => mod.PriceMultiplier);

            try
            {
                //check if there are more than 6 modifiers
                if (modifierGroup.ItemModifiers.Count() > 6)
                {
                    var modifiers = modifierGroup.ItemModifiers;
                    //remove duplicate itemModifiers and order them by the sum of the modifiers
                    var filteredOrdered = modifiers.Distinct().OrderBy(mod => GetModifierSum(mod));
                    //create a new list with the same order, but with the sums of the modifiers as the key, there can be duplicate k
                    var resultsWithSum = filteredOrdered.Select(mod => new KeyValuePair<float, ItemModifier>(GetModifierSum(mod), mod));

                    //get the 3 highest sums
                    var highest = resultsWithSum.Skip(resultsWithSum.Count() - 3).Take(3);
                    //get all the negative sums
                    var negative = resultsWithSum.Where(mod => mod.Key < 0);
                    //order the negative sums by the lowest first
                    var negativeOrdered = negative.OrderBy(mod => mod.Key);
                    //get the 2 highest sums of the negative sums without TakeLast
                    var negativeHighest = negativeOrdered.Skip(negativeOrdered.Count() - 2).Take(2);
                    //join the 3 highest sums and the 3 highest negative sums
                    var joined = negativeHighest.Concat(highest);
                    //return the modifier at modifierTier
                    return joined.ElementAt(Math.Min(joined.Count() - 1, modifierTier)).Value;
                }

                return results.ElementAt(Math.Min(results.Count() - 1, modifierTier));
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage(ex.ToString()));

                return null;
            }
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
            var noMaterialsRequired = Settings.Instance?.NoMaterialsRequired ?? false;
            /*
             * This part sets the resource change amount to 0 for all materials if the player has the option to craft without materials
             */
            if (noMaterialsRequired)
            {
                for (int i = 0; i < (int)ExtraCraftingMaterials.NumExtraCraftingMats; i++)
                {
                    ExtraMaterials[i].ResourceChangeAmount = 0;
                }
                for (int i = 0; i < (int)CraftingMaterials.NumCraftingMats; i++)
                {
                    ViewModel.PlayerCurrentMaterials[i].ResourceChangeAmount = 0;
                }
                return;
            }
            //TODO: Remove at a later date if issue is recreatable
            if (!IsInArmorMode)
            {
                try
                {
                    for (int i = 0; i < (int)ExtraCraftingMaterials.NumExtraCraftingMats; i++)
                    {
                        ExtraMaterials[i].ResourceChangeAmount = 0;
                    }

                    return;
                }
                catch /*Catch NullReferenceException */ (NullReferenceException)
                {
                    BannerCraftSmithingModel tempSmithingModel = (BannerCraftSmithingModel)Campaign.Current.Models.SmithingModel;

                    MBBindingList<ExtraMaterialItemVM> TempExtraMaterials = new();
                    //go throw enum and add all extra materials to the list
                    for (int i = 0; i < (int)ExtraCraftingMaterials.NumExtraCraftingMats; ++i)
                    {
                        var material = (ExtraCraftingMaterials)i;
                        TempExtraMaterials.Add(new ExtraMaterialItemVM(material, tempSmithingModel.GetCraftingMaterialItem(material), 0));
                    }
                    //using setter method to avoid null reference exception
                    ExtraMaterials = TempExtraMaterials;
                    return;
                }
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
            _refreshEnableMainActionBase?.Invoke(ViewModel, Array.Empty<object>());

            var craftingBehavior = Campaign.Current.GetCampaignBehavior<ICraftingCampaignBehavior>();
            var hero = ViewModel.CurrentCraftingHero.Hero;
            bool noStaminaRequired = Settings.Instance?.NoStaminaRequired ?? false;
            ViewModel.IsMainActionEnabled = true;
            if (!HaveEnergy(hero) && !noStaminaRequired)
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
            //dont need to add this check for armor crafting cheat because the cheat already sets the required materials to 0
            else if (!HaveMaterialsNeeded())
            {
                ViewModel.IsMainActionEnabled = false;
                ViewModel.MainActionHint = new BasicTooltipViewModel(() =>
                {
                    return new TextObject("{=gduqxfck}You don't have all required materials!").ToString();
                });
            }
            else if (ArmorCrafting?.CurrentItem == null)
            {
                ViewModel.IsMainActionEnabled = false;
            }
        }

        private void SpendMaterials(WeaponDesign weaponDesign)
        {
            var smithingModel = Campaign.Current.Models.SmithingModel as BannerCraftSmithingModel;
            ItemRoster itemRoster = MobileParty.MainParty.ItemRoster;
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