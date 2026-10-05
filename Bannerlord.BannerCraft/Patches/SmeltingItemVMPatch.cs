using HarmonyLib;
using System.Reflection;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting.Smelting;

namespace Bannerlord.BannerCraft.Patches
{
    [HarmonyPatch]
    internal static class SmeltingItemVMPatch
    {
        private static MethodBase? TargetMethod() => AccessTools.Method(typeof(SmeltingItemVM), nameof(SmeltingItemVM.RefreshValues));

        // Skip the patch instead of failing to load if the game changes the method.
        private static bool Prepare() => TargetMethod() != null;

        // Vanilla only shows the base item name, so items with different modifiers (Rusty, Fine, Lordly...) looked identical.
        public static void Postfix(SmeltingItemVM __instance)
        {
            var equipmentElement = __instance.EquipmentElement;
            if (equipmentElement.Item != null && equipmentElement.ItemModifier != null)
            {
                __instance.Name = equipmentElement.GetModifiedItemName().ToString();
            }
        }
    }
}
