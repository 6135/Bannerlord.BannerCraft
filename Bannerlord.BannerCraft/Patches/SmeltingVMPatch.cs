using System;
using System.Linq;
using Bannerlord.BannerCraft.ViewModels;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting.Smelting;
using TaleWorlds.Core;

namespace Bannerlord.BannerCraft.Patches
{
    internal class SmeltingVMPatch
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

        public static void RefreshListPostfix(ref SmeltingVM __instance)
        {
            if (!Settings.Instance.AllowSmeltingOtherItems)
            {
                return;
            }

            var smithingModel = Campaign.Current.Models.SmithingModel;

            var playerItemRoster = GetPlayerItemRoster(__instance);
            var onItemSelection = GetOnItemSelectionAction(__instance);
            var processLockItem = GetProcessLockItemAction(__instance);

            for (int i = 0; i < playerItemRoster.Count; i++)
            {
                var elementCopyAtIndex = playerItemRoster.GetElementCopyAtIndex(i);
                var item = elementCopyAtIndex.EquipmentElement.Item;
                var itemType = ArmorCraftingVM.GetItemType(item);
                var smeltingOutputs = smithingModel.GetSmeltingOutputForItem(item);
                var givesOutput = smeltingOutputs.Any(output => output > 0);
                if (!ArmorCraftingVM.ItemTypeIsWeapon(itemType) && givesOutput)
                {
                    bool isLocked = IsItemLocked(__instance, elementCopyAtIndex.EquipmentElement);

                    SmeltingItemVM smeltingItem = new SmeltingItemVM(
                        elementCopyAtIndex.EquipmentElement,
                        onItemSelection,
                        processLockItem,
                        isLocked,
                        elementCopyAtIndex.Amount);

                    __instance.SmeltableItemList.Add(smeltingItem);
                }
            }

            if (__instance.SmeltableItemList.Count == 0)
            {
                __instance.CurrentSelectedItem = null;
            }
        }
    }
}