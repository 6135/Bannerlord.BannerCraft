using System;
using System.Linq;
using System.Reflection;
using Bannerlord.BannerCraft.ViewModels;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting.Smelting;
using TaleWorlds.Core;

namespace Bannerlord.BannerCraft.Patches
{
    internal class SmeltingVMPatch
    {
        public static void RefreshListPostfix(SmeltingVM __instance)
        {
            if (!Settings.Instance.AllowSmeltingOtherItems)
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
                    && BannerCraftConfig.Instance.SmithingModel.GetSmeltingOutputForItem(elementCopyAtIndex.EquipmentElement.Item).Any((x) => x > 0))
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
}
