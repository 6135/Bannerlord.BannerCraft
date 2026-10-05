using Bannerlord.BannerCraft.Mixins;
using HarmonyLib;
using System.Reflection;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting;

namespace Bannerlord.BannerCraft.Patches
{
    [HarmonyPatch(typeof(CraftingVM), "ExecuteMainAction")]
    internal class CraftingVMPatch
    {
        // Prevent a vanilla weapon from being crafted in armor crafting mode when pressing space.
        public static bool Prefix(CraftingVM __instance) => __instance.IsInSmeltingMode || __instance.IsInCraftingMode || __instance.IsInRefinementMode;
    }

    [HarmonyPatch]
    internal class CraftingVMExecuteConfirmPatch
    {
        private static MethodBase? TargetMethod() => AccessTools.Method(typeof(CraftingVM), "ExecuteConfirm");

        // Skip the patch instead of failing to load if the game changes the method.
        private static bool Prepare() => TargetMethod() is MethodInfo method && method.ReturnType == typeof((bool, bool));

        // Vanilla only knows about weapon crafting, so the confirm key (Enter) did nothing in armor crafting mode.
        public static bool Prefix(CraftingVM __instance, ref (bool isConfirmSuccessful, bool isMainActionExecuted) __result)
        {
            if (CraftingMixin.Mixin == null
                || !CraftingMixin.Mixin.TryGetTarget(out var mixin)
                || !mixin.IsAttachedTo(__instance)
                || !mixin.IsInArmorMode)
            {
                return true;
            }

            __result = (false, false);

            var armorCrafting = mixin.ArmorCrafting;
            if (armorCrafting.ArmorClassSelectionPopup?.IsVisible ?? false)
            {
                return false;
            }

            if (armorCrafting.ArmorCraftResultPopupVisible)
            {
                if (armorCrafting.ArmorCraftResultPopup?.CanConfirm ?? true)
                {
                    armorCrafting.ExecuteFinalizeCrafting();
                    __result = (true, false);
                }
            }
            else if (__instance.IsMainActionEnabled)
            {
                mixin.ExecuteMainActionBannerCraft();
                __result = (true, false);
            }

            return false;
        }
    }
}
