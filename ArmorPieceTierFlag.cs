using System;

namespace Bannerlord.BannerCraft
{
    [Flags]
    public enum ArmorPieceTierFlag
    {
        None = 0x0,
        Tier0 = 0x1,
        Tier1 = 0x2,
        Tier2 = 0x4,
        Tier3 = 0x8,
        Tier4 = 0x10,
        Tier5 = 0x20,
        Tier6 = 0x40,
        All = 0x7F
    }
}