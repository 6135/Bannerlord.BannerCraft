using System;
using System.Collections.Generic;
using TaleWorlds.Core;

namespace Bannerlord.BannerCraft.Extensions
{
    public static class ItemModifierGroupExtension
    {
        public static ItemModifier GetRandomModifierWithTarget(this ItemModifierGroup instance, int modifierTier)
        {
            List<ItemModifier> list = new();

            foreach (ItemModifier modifier in instance.ItemModifiers)
            {
                list.Add(modifier);
            }

            list.Sort(new ItemModifierPriceMultiplierComparer());

            return list[Math.Min(list.Count - 1, modifierTier)];
        }

        public class ItemModifierPriceMultiplierComparer : IComparer<ItemModifier>
        {
            public int Compare(ItemModifier x, ItemModifier y)
            {
                return x.PriceMultiplier.CompareTo(y.PriceMultiplier);
            }
        }
    }
}