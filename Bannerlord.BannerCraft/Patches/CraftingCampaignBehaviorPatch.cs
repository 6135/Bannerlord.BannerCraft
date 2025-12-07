using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;

namespace Bannerlord.BannerCraft.Patches
{
    [HarmonyPatch(typeof(CraftingCampaignBehavior), "DoSmelting")]
    internal static class CraftingCampaignBehaviorPatch
    {
        public static bool Prefix(CraftingCampaignBehavior __instance, Hero currentCraftingHero, EquipmentElement equipmentElement)
        {
            ItemObject item = equipmentElement.Item;
            if (item.WeaponDesign != null && item.WeaponDesign.Template != null)
            {
                return true;
            }

            ItemRoster itemRoster = MobileParty.MainParty.ItemRoster;
            int[] smeltingOutputForItem = Campaign.Current.Models.SmithingModel.GetSmeltingOutputForItem(item);
            for (int num = 8; num >= 0; num--)
            {
                if (smeltingOutputForItem[num] != 0)
                {
                    itemRoster.AddToCounts(Campaign.Current.Models.SmithingModel.GetCraftingMaterialItem((CraftingMaterials)num), smeltingOutputForItem[num]);
                }
            }

            itemRoster.AddToCounts(equipmentElement, -1);
            currentCraftingHero.AddSkillXp(DefaultSkills.Crafting, Campaign.Current.Models.SmithingModel.GetSkillXpForSmelting(item));
            int energyCostForSmelting = Campaign.Current.Models.SmithingModel.GetEnergyCostForSmelting(item, currentCraftingHero);
            __instance.SetHeroCraftingStamina(currentCraftingHero, __instance.GetHeroCraftingStamina(currentCraftingHero) - energyCostForSmelting);
            CampaignEventDispatcher.Instance.OnEquipmentSmeltedByHero(currentCraftingHero, equipmentElement);

            return false;
        }
    }
}