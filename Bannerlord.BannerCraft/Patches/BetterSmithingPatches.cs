using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;

namespace Bannerlord.BannerCraft.Patches
{
    internal static class BetterSmithingPatches
    {
        private static bool IsOtherCraftedItem(ItemObject item) => item.HasArmorComponent || item.HasSaddleComponent || (item.HasWeaponComponent && (item.PrimaryWeapon.IsShield || item.PrimaryWeapon.WeaponClass == WeaponClass.Bow || item.PrimaryWeapon.WeaponClass == WeaponClass.Crossbow || item.PrimaryWeapon.WeaponClass == WeaponClass.Arrow || item.PrimaryWeapon.WeaponClass == WeaponClass.Bolt));

        [HarmonyPatch]
        internal static class SmeltingItemRosterWrapperPatch
        {
            private static MethodBase TargetMethod() => AccessTools.Method(AccessTools.TypeByName("SmeltingItemRosterWrapper"), "GetCraftedItemList");

            // Check whether Better Smithing is loaded.
            private static bool Prepare() => TargetMethod() != null;

            private static void Postfix(ref IEnumerable<ItemRosterElement> __result, ItemRoster ___m_PartyItemRoster)
            {
                List<ItemRosterElement> craftedItemList = __result.ToList();

                foreach (ItemRosterElement itemRosterElement in ___m_PartyItemRoster.Where(e => IsOtherCraftedItem(e.EquipmentElement.Item)))
                {
                    craftedItemList.Add(itemRosterElement);
                }

                __result = craftedItemList;
            }
        }

        [HarmonyPatch]
        internal static class BetterSmeltingVMPatch
        {
            private static MethodBase TargetMethod() => AccessTools.Method(AccessTools.TypeByName("BetterSmeltingVM"), "ItemIsVisible");

            // Check whether Better Smithing is loaded.
            private static bool Prepare() => TargetMethod() != null;

            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
                List<CodeInstruction> codes = instructions.ToList(), codesToInsert = new();
                Label label = il.DefineLabel();
                int index = 0;

                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].operand is MethodInfo method && method == AccessTools.PropertyGetter(typeof(ItemObject), "IsCraftedWeapon"))
                    {
                        codes[i + 4].labels.Add(label);
                        index = i + 2;
                    }
                }

                codesToInsert.Add(new CodeInstruction(OpCodes.Ldloc_0));
                codesToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BetterSmithingPatches), "IsOtherCraftedItem")));
                codesToInsert.Add(new CodeInstruction(OpCodes.Brtrue_S, label));
                codes.InsertRange(index, codesToInsert);

                return codes;
            }
        }
    }
}