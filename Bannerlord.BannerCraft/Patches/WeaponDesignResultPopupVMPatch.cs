using HarmonyLib;
using System;
using System.Reflection;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting.WeaponDesign;

namespace Bannerlord.BannerCraft.Patches
{
    [HarmonyPatch]
    internal static class WeaponDesignResultPopupVMPatch
    {
        private static readonly MethodInfo? IsStringApplicableForItemName =
            AccessTools.Method(AccessTools.TypeByName("TaleWorlds.CampaignSystem.ViewModelCollection.CampaignUIHelper"), "IsStringApplicableForItemName", new[] { typeof(string) });

        private static bool _isShortening;

        private static MethodBase? TargetMethod() => AccessTools.Method(typeof(WeaponDesignResultPopupVM), "UpdateConfirmAvailability");

        // Skip the patch instead of failing to load if the game changes the methods.
        private static bool Prepare() => TargetMethod() is MethodInfo method && method.GetParameters().Length == 0 && IsStringApplicableForItemName != null;

        // In some languages (e.g. Russian) the generated name of an order weapon is longer than the game allows.
        // The Done button then stays disabled and the result popup can't be closed, so shorten the name to the longest valid one.
        public static void Postfix(WeaponDesignResultPopupVM __instance, MethodBase __originalMethod)
        {
            if (_isShortening || __instance.CanConfirm)
            {
                return;
            }

            string? itemName = __instance.ItemName;
            if (string.IsNullOrEmpty(itemName) || IsApplicable(itemName!))
            {
                return;
            }

            for (int length = itemName!.Length - 1; length > 0; length--)
            {
                string shortName = itemName.Substring(0, length).TrimEnd();
                if (shortName.Length == 0 || !IsApplicable(shortName))
                {
                    continue;
                }

                _isShortening = true;
                try
                {
                    __instance.ItemName = shortName;
                    __originalMethod.Invoke(__instance, Array.Empty<object>());
                }
                finally
                {
                    _isShortening = false;
                }

                return;
            }
        }

        private static bool IsApplicable(string name)
        {
            try
            {
                return IsStringApplicableForItemName!.Invoke(null, new object[] { name }) is Tuple<bool, TaleWorlds.Localization.TextObject> result && result.Item1;
            }
            catch (Exception)
            {
                // Don't change the name if the game check fails.
                return true;
            }
        }
    }
}
