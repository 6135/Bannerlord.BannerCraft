using BetterSmithingContinued.MainFrame;
using BetterSmithingContinued.MainFrame.Persistence;
using BetterSmithingContinued.MainFrame.UI.ViewModels;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting.Smelting;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerCraft
{
	internal class SmeltingVMPatch
	{
		public static void RefreshListPostfix(SmeltingVM __instance)
		{
			if (!MCMUISettings.Instance.AllowSmeltingOtherItems)
			{
				return;
			}

			BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
			Func<EquipmentElement, bool> isItemLocked = (Func<EquipmentElement, bool>)typeof(SmeltingVM).GetMethod("IsItemLocked", bindingFlags).CreateDelegate(typeof(Func<EquipmentElement, bool>), __instance); ;

			ItemRoster playerItemRoster = (ItemRoster)typeof(SmeltingVM).GetField("_playerItemRoster", bindingFlags).GetValue(__instance);

			Action<SmeltingItemVM> onItemSelection = (Action<SmeltingItemVM>)typeof(SmeltingVM).GetMethod("OnItemSelection", bindingFlags).CreateDelegate(typeof(Action<SmeltingItemVM>), __instance);
			Action<SmeltingItemVM, bool> processLockItem = (Action<SmeltingItemVM, bool>)typeof(SmeltingVM).GetMethod("ProcessLockItem", bindingFlags).CreateDelegate(typeof(Action<SmeltingItemVM, bool>), __instance);

			for (int i = 0; i < playerItemRoster.Count; i++)
			{
				ItemRosterElement elementCopyAtIndex = playerItemRoster.GetElementCopyAtIndex(i);
				if (!ArmorCraftingVM.ItemTypeIsWeapon(ArmorCraftingVM.GetItemType(elementCopyAtIndex.EquipmentElement.Item))
					&& BannerCraftConfig.Instance.SmithingModel.GetSmeltingOutputForItem(elementCopyAtIndex.EquipmentElement.Item).Any((int x) => x > 0))
				{
					bool isLocked = isItemLocked(elementCopyAtIndex.EquipmentElement);
					SmeltingItemVM item = new SmeltingItemVM(elementCopyAtIndex.EquipmentElement, onItemSelection, processLockItem, isLocked, elementCopyAtIndex.Amount);
					__instance.SmeltableItemList.Add(item);
				}
			}

			if (__instance.SmeltableItemList.Count == 0)
			{
				__instance.CurrentSelectedItem = null;
			}
		}
	}

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

    internal class BSCSmeltingItemRosterWrapperPatch
    {
        public static void PerformFullRefreshPostFix(ref bool __result, ItemRosterElement x)
        {
            __result = __result || ArmorCraftingVM.GetItemType(x.EquipmentElement.Item) != ArmorCraftingVM.ItemType.Invalid;
        }
    }

    internal class BSCBetterSmeltingVMPatch
    {
        public static bool ItemIsVisiblePreFix(SmeltingSettings ___m_SmeltingSettings, SmithingManager ___m_SmithingManager, ref bool __result, EquipmentElement _equipmentElement)
        {
            ItemObject item = _equipmentElement.Item;
            if (ArmorCraftingVM.GetItemType(item) == ArmorCraftingVM.ItemType.Invalid)
            {
                return true;
            }

            if (___m_SmeltingSettings.DisplayLockedWeapons && ___m_SmithingManager.SmeltingItemRoster.IsItemLocked(_equipmentElement))
            {
                __result = false;
                return false;
            }

            SmithingModel smithingModel = Campaign.Current.Models.SmithingModel;
            int[] array = (smithingModel != null) ? smithingModel.GetSmeltingOutputForItem(item) : null;
            if (array == null)
            {
                __result = false;
                return false;
            }

            for (int i = 0; i < (int)CraftingMaterials.NumCraftingMats; i++)
            {
                if (array[i] > 0 && ___m_SmeltingSettings.GetMaterialIsDisplayed((CraftingMaterials)i))
                {
                    __result = true;
                    return false;
                }
            }

            __result = false;
            return false;
        }
    }
}
