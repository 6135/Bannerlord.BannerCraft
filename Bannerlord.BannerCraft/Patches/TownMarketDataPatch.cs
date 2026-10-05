using HarmonyLib;
using System.Reflection;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace Bannerlord.BannerCraft.Patches
{
    [HarmonyPatch]
    internal static class TownMarketDataPatch
    {
        private static MethodBase? TargetMethod() => AccessTools.Method(typeof(TownMarketData), nameof(TownMarketData.GetPrice),
            new[] { typeof(EquipmentElement), typeof(MobileParty), typeof(bool), typeof(PartyBase) });

        // Skip the patch instead of failing to load if the game changes the method.
        private static bool Prepare() => TargetMethod() != null;

        // Items that lost their category on a save/reload crash the inventory when priced, so give them one back.
        public static void Prefix(EquipmentElement __0)
        {
            var item = __0.Item;
            if (item != null && item.ItemCategory == null)
            {
                item.DetermineItemCategoryForItem();
            }
        }
    }
}
