using HarmonyLib;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting;

namespace Bannerlord.BannerCraft.Patches
{
    [HarmonyPatch(typeof(CraftingVM), "ExecuteMainAction")]
    internal class CraftingVMPatch
    {
        // Prevent a vanilla weapon from being crafted in armor crafting mode when pressing space.
        public static bool Prefix(CraftingVM __instance) => __instance.IsInSmeltingMode || __instance.IsInCraftingMode || __instance.IsInRefinementMode;
    }
}
