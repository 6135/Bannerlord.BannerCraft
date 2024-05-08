using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace Bannerlord.BannerCraft.Patches
{
    [HarmonyPatch(typeof(CampaignGameStarter), "AddModel")]
    internal static class CampaignGameStarterPatch
    {
        // Skip Banner Kings' smithing model.
        public static bool Prefix(GameModel model) => model.GetType() != AccessTools.TypeByName("BKSmithingModel");
    }
}