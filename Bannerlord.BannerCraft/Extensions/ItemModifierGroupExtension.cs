using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerCraft
{
	public static class ItemModifierGroupExtension
	{
        public class ItemModifierPriceMultiplierComparer : IComparer<ItemModifier>
        {
            public int Compare(ItemModifier x, ItemModifier y)
            {
                return x.PriceMultiplier.CompareTo(y.PriceMultiplier);
            }
        }

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
	}
}
