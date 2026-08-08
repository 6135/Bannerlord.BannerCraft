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

        /*
         * Classify straight from the item rather than through ArmorCraftingVM.GetItemType: that
         * collapses normal weapons to Invalid whenever AllowCraftingNormalWeapons is off (the
         * default), and ItemTypeIsWeapon(Invalid) is false — so weapons vanilla had already listed
         * were being appended a second time.
         */
        private static bool IsWeapon(ItemObject item) => item.ItemType is ItemObject.ItemTypeEnum.OneHandedWeapon
            or ItemObject.ItemTypeEnum.TwoHandedWeapon
            or ItemObject.ItemTypeEnum.Polearm
            or ItemObject.ItemTypeEnum.Thrown;

        public static void Postfix(ref SmeltingVM __instance)
        {
            bool allowCraftingOtherItems = Settings.Instance?.AllowSmeltingOtherItems ?? false;
            if (allowCraftingOtherItems)
            {
                var smithingModel = Campaign.Current.Models.SmithingModel;

                var playerItemRoster = GetPlayerItemRoster(__instance);
                var onItemSelection = GetOnItemSelectionAction(__instance);
                var processLockItem = GetProcessLockItemAction(__instance);

                for (int i = 0; i < playerItemRoster.Count; i++)
                {
                    var elementCopyAtIndex = playerItemRoster.GetElementCopyAtIndex(i);
                    var equipmentElement = elementCopyAtIndex.EquipmentElement;
                    var item = equipmentElement.Item;
                    var smeltingOutputs = smithingModel.GetSmeltingOutputForItem(item);
                    var givesOutput = smeltingOutputs.Any(output => output > 0);
                    if (!IsWeapon(item) && givesOutput)
                    {
                        bool isLocked = IsItemLocked(__instance, equipmentElement);

                        SmeltingItemVM smeltingItem = new SmeltingItemVM(
                            equipmentElement,
                            onItemSelection,
                            processLockItem,
                            isLocked,
                            elementCopyAtIndex.Amount);
                        /*
                         * Compare the whole equipment element, not just the item: two roster stacks
                         * of the same item with different modifiers are distinct entries, and
                         * matching on the item alone silently dropped the second one.
                         */
                        if (!__instance.SmeltableItemList.Any(smeltableItem => smeltableItem.EquipmentElement.Equals(equipmentElement)))
                            __instance.SmeltableItemList.Add(smeltingItem);
                    }
                }

                if (__instance.SmeltableItemList.Count == 0)
                {
                    __instance.CurrentSelectedItem = null;
                } /* if has values and current value is set to null, then get first or default on updated list*/
                else if (__instance.CurrentSelectedItem is null)
                {
                    var newItem = __instance.SmeltableItemList.FirstOrDefault();
                    onItemSelection(newItem);
                }
            }
        }
    }
}