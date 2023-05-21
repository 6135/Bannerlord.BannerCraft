using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;

namespace Bannerlord.BannerCraft.Patches
{
    internal class CraftingCampaignBehaviorPatch
    {
        public static bool DoSmeltingPrefix(CraftingCampaignBehavior __instance, Hero hero, EquipmentElement equipmentElement)
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
            hero.AddSkillXp(DefaultSkills.Crafting, Campaign.Current.Models.SmithingModel.GetSkillXpForSmelting(item));
            int energyCostForSmelting = Campaign.Current.Models.SmithingModel.GetEnergyCostForSmelting(item, hero);
            __instance.SetHeroCraftingStamina(hero, __instance.GetHeroCraftingStamina(hero) - energyCostForSmelting);
            CampaignEventDispatcher.Instance.OnEquipmentSmeltedByHero(hero, equipmentElement);

            return false;
        }
    }
}