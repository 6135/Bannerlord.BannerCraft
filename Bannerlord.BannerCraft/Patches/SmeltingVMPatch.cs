using Bannerlord.BannerCraft.ViewModels;
using HarmonyLib;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting.Smelting;
using TaleWorlds.Core;

namespace Bannerlord.BannerCraft.Patches
{
    [HarmonyPatch(typeof(SmeltingVM), "RefreshList")]
    internal static class SmeltingVMPatch
    {
        static SmeltingVMPatch()
        {
            var isItemLockedMethod = AccessTools.Method(typeof(SmeltingVM), "IsItemLocked");
            IsItemLocked = (vm, elem) => (bool)isItemLockedMethod.Invoke(vm, new object[] { elem });

            var itemRosterField = AccessTools.Field(typeof(SmeltingVM), "_playerItemRoster");
            GetPlayerItemRoster = vm => (ItemRoster)itemRosterField.GetValue(vm);

            var onItemSelectionMethod = AccessTools.Method(typeof(SmeltingVM), "OnItemSelection");
            GetOnItemSelectionAction = vm => AccessTools.MethodDelegate<Action<SmeltingItemVM>>(onItemSelectionMethod, vm);

            var processLockItemMethod = AccessTools.Method(typeof(SmeltingVM), "ProcessLockItem");
            GetProcessLockItemAction = vm => AccessTools.MethodDelegate<Action<SmeltingItemVM, bool>>(processLockItemMethod, vm);
        }

        private static Func<SmeltingVM, EquipmentElement, bool> IsItemLocked { get; }

        private static Func<SmeltingVM, ItemRoster> GetPlayerItemRoster { get; }

        private static Func<SmeltingVM, Action<SmeltingItemVM>> GetOnItemSelectionAction { get; }

        private static Func<SmeltingVM, Action<SmeltingItemVM, bool>> GetProcessLockItemAction { get; }

        /// <summary>
        /// Whether a non crafted weapon item (armor, shields, banners, etc.) can be smelted.
        /// </summary>
        public static bool CanSmeltOtherItem(ItemObject? item)
        {
            if (item == null || item.IsCraftedWeapon || !(Settings.Instance?.AllowSmeltingOtherItems ?? false))
            {
                return false;
            }

            if (ArmorCraftingVM.ItemTypeIsWeapon(ArmorCraftingVM.GetItemType(item)))
            {
                return false;
            }

            return Campaign.Current.Models.SmithingModel.GetSmeltingOutputForItem(item).Any(output => output > 0);
        }

        public static void Postfix(SmeltingVM __instance)
        {
            // Better Smithing builds its own list, which BetterSmithingPatches already extends.
            // Adding items here as well duplicated them and bypassed its locked items filter.
            if (BetterSmithingPatches.IsBetterSmithingLoaded)
            {
                return;
            }

            if (!(Settings.Instance?.AllowSmeltingOtherItems ?? false))
            {
                return;
            }

            var playerItemRoster = GetPlayerItemRoster(__instance);
            var onItemSelection = GetOnItemSelectionAction(__instance);
            var processLockItem = GetProcessLockItemAction(__instance);
            var currentSelectedItem = __instance.CurrentSelectedItem;

            for (int i = 0; i < playerItemRoster.Count; i++)
            {
                var elementCopyAtIndex = playerItemRoster.GetElementCopyAtIndex(i);
                var equipmentElement = elementCopyAtIndex.EquipmentElement;
                if (!CanSmeltOtherItem(equipmentElement.Item))
                {
                    continue;
                }

                // The same item with a different modifier is a separate entry, just like in vanilla.
                if (__instance.SmeltableItemList.Any(smeltableItem => smeltableItem.EquipmentElement.IsEqualTo(equipmentElement)))
                {
                    continue;
                }

                bool isLocked = IsItemLocked(__instance, equipmentElement);
                SmeltingItemVM smeltingItem = new SmeltingItemVM(
                    equipmentElement,
                    onItemSelection,
                    processLockItem,
                    isLocked,
                    elementCopyAtIndex.Amount);

                if (currentSelectedItem != null && currentSelectedItem.EquipmentElement.IsEqualTo(equipmentElement))
                {
                    onItemSelection(smeltingItem);
                }

                __instance.SmeltableItemList.Add(smeltingItem);
            }

            if (__instance.SmeltableItemList.Count == 0)
            {
                __instance.CurrentSelectedItem = null;
            }
            else if (__instance.CurrentSelectedItem is null)
            {
                onItemSelection(__instance.SmeltableItemList.First());
            }
        }
    }
}
