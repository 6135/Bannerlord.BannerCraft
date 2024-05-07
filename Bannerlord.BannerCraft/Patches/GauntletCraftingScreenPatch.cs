using Bannerlord.BannerCraft.Mixins;
using HarmonyLib;
using SandBox.GauntletUI;
using TaleWorlds.CampaignSystem.GameState;

namespace Bannerlord.BannerCraft.Patches
{
    [HarmonyPatch(typeof(GauntletCraftingScreen), "OnCraftingLogicRefreshed")]
    internal static class GauntletCraftingScreenPatch
    {
        public static void Postfix(CraftingState ____craftingState)
        {
            if (CraftingMixin.Mixin != null && CraftingMixin.Mixin.TryGetTarget(out CraftingMixin mixin))
            {
                mixin.OnCraftingLogicRefreshed(____craftingState.CraftingLogic);
            }
        }
    }
}
