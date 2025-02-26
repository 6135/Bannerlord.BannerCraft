# BannerCraft

![build](https://github.com/6135/Bannerlord.BannerCraft/actions/workflows/build.yml/badge.svg?event=push)
[![CodeFactor](https://www.codefactor.io/repository/github/6135/bannerlord.bannercraft/badge)](https://www.codefactor.io/repository/github/6135/bannerlord.bannercraft)

Crafting extension for Mount and Blade 2: Bannerlord

# Download

Visit [nexus mods](https://www.nexusmods.com/mountandblade2bannerlord/mods/5932) to download or download [latest release](https://github.com/6135/Bannerlord.BannerCraft/releases/latest) and install manually. 

# Depencies 


| Dependency                   | Version  | Required |
|------------------------------|----------|----------|
| [Harmony](https://www.nexusmods.com/mountandblade2bannerlord/mods/2006)                      | >= 2.2.2 | ✅        |
| [ButterLib](https://www.nexusmods.com/mountandblade2bannerlord/mods/2018)                    | >= 2.6.3 | ✅        |
| [UIExtenderEx](https://www.nexusmods.com/mountandblade2bannerlord/mods/2102)                 | >= 2.8.0 | ✅        |
| [Mod Configuration Menu (MCM)](https://www.nexusmods.com/mountandblade2bannerlord/mods/612) | >= 5.9.1 | ✅        |


# License

[See license](https://github.com/6135/Bannerlord.BannerCraft/blob/master/LICENSE)

# Credits
Special thanks to the BannerKings mod team (https://github.com/R-Vaccari/bannerlord-banner-kings) for showing how to edit the crafting system.
Forked from @adwitkow updated version of the mod.
Original author: https://github.com/Evangel63/BannerCraft

# Changelog

**v1.0.28**

Fixed 3 bugs

1) Duplicate items in the smelting list
2) When smelting list only has items added by the mod, the selection would reset to none, now it resets to top of the list, like in vanilla
3) The difficulty cheat wasnt properly applying to chance calculation

**v1.0.27.0**

Added setting to change crafting costs, it works as a multiplier, by default it is x1.00, a value of 0 will effectively set the costs to x0.00 and a value of x5.00 will increase costs by 5 times.

**v1.0.26.0**

Added 3 cheats. 

1) sets material costs to 0. 
2) sets stamina cost to 0.
3) sets item difficulty to 0 (removing botch chances and increasing chances at better results)

**v1.0.25.0**

Updated to 1.2.4 and made it easier to keep old version compatible code when updating by reversing the conditions on the preprocessor directives. Fixed a visual and functional but where the previous extra crafting materials where shown when switching to normal crafting. Possible fix for a nullRefExeption on updating materials (temporary fix).

**v1.0.22**

Refactored modifier behaviour using vanilla formulas, and added the option to switch between behaviours. Added options to increase an individual modifier's chance of showing up. Fixed a major bug when crafting vanilla items.

v1.0.21

Updated to 1.2.3

**v1.0.20**

Updated to 1.1.6

**1.0.5**

Fixed mismatched ordering between what's shown on armor crafting result popup and the actual result.

**1.0.4**

Changed ordering of armor stats to match vanilla hints
Vanilla hints are stupid and go Head Body Leg Arm which doesn't match inventory total values
Added negative item prefix chance based on skill level. Positive prefixes require Experienced/Master/Legendary Smith perks.
Added mouseover tooltips to items. They look bad but the vanilla ones are done via more hidden magic code.

**Version 1.0.3**

Added botching chance and smelting to MCM menu

**Version 1.0.2**

Fixed issue with refinement selection being reset. Repacked to be single folder instead of loose files.

**Version 1.0.1**

Fixed exception when no items are shown

Version 1.0.0
Initial release

### Information

| S \ T | 6 | 5 | 4 | 3 | 2 | 1 | 0 |
|--|-|-|-|-|-|-|
| 0 | 305.0 | 253.76 | 202.51999999999998 | 152.5 | 101.25999999999999 | 50.019999999999996 | 0.0 |
| 1 | 305.0 | 253.76 | 203.74 | 152.5 | 101.25999999999999 | 51.24 | 0.0 |
| 2 | 306.21999999999997 | 254.98 | 203.74 | 153.72 | 102.48 | 51.24 | 1.22 |
| 3 | 306.21999999999997 | 254.98 | 204.96 | 153.72 | 102.48 | 52.46 | 1.22 |
| 4 | 307.44 | 256.2 | 204.96 | 154.94 | 103.7 | 52.46 | 2.44 |
| 5 | 307.44 | 256.2 | 206.18 | 154.94 | 103.7 | 53.68 | 2.44 |
| 6 | 308.65999999999997 | 257.42 | 206.18 | 156.16 | 104.92 | 53.68 | 3.66 |
| 7 | 308.65999999999997 | 257.42 | 207.4 | 156.16 | 104.92 | 54.9 | 3.66 |
| 8 | 309.88 | 258.64 | 207.4 | 157.38 | 106.14 | 54.9 | 4.88 |
| 9 | 309.88 | 258.64 | 208.62 | 157.38 | 106.14 | 56.12 | 4.88 |
| 10 | 311.09999999999997 | 259.86 | 208.62 | 158.6 | 107.36 | 56.12 | 6.1 |
| 11 | 311.09999999999997 | 259.86 | 209.84 | 158.6 | 107.36 | 57.339999999999996 | 6.1 |
| 12 | 312.32 | 261.08 | 209.84 | 159.82 | 108.58 | 57.339999999999996 | 7.32 |
| 13 | 312.32 | 261.08 | 211.06 | 159.82 | 108.58 | 58.56 | 7.32 |
| 14 | 313.54 | 262.3 | 211.06 | 161.04 | 109.8 | 58.56 | 8.54 |
| 15 | 313.54 | 262.3 | 212.28 | 161.04 | 109.8 | 59.78 | 8.54 |
| 16 | 314.76 | 263.52 | 212.28 | 162.26 | 111.02 | 59.78 | 9.76 |
| 17 | 314.76 | 263.52 | 213.5 | 162.26 | 111.02 | 61.0 | 9.76 |
| 18 | 315.98 | 264.74 | 213.5 | 163.48 | 112.24 | 61.0 | 10.98 |
| 19 | 315.98 | 264.74 | 214.72 | 163.48 | 112.24 | 62.22 | 10.98 |
| 20 | 317.2 | 265.96 | 214.72 | 164.7 | 113.46 | 62.22 | 12.2 |
| 21 | 317.2 | 265.96 | 215.94 | 164.7 | 113.46 | 63.44 | 12.2 |
| 22 | 318.42 | 267.18 | 215.94 | 165.92 | 114.67999999999999 | 63.44 | 13.42 |
| 23 | 318.42 | 267.18 | 217.16 | 165.92 | 114.67999999999999 | 64.66 | 13.42 |
| 24 | 319.64 | 268.4 | 217.16 | 167.14 | 115.89999999999999 | 64.66 | 14.64 |
| 25 | 319.64 | 268.4 | 218.38 | 167.14 | 115.89999999999999 | 65.88 | 14.64 |
| 26 | 320.86 | 269.62 | 218.38 | 168.35999999999999 | 117.12 | 65.88 | 15.86 |
| 27 | 320.86 | 269.62 | 219.6 | 168.35999999999999 | 117.12 | 67.1 | 15.86 |
| 28 | 322.08 | 270.84 | 219.6 | 169.57999999999998 | 118.34 | 67.1 | 17.08 |
| 29 | 322.08 | 270.84 | 220.82 | 169.57999999999998 | 118.34 | 68.32 | 17.08 |
| 30 | 323.3 | 272.06 | 220.82 | 170.79999999999998 | 119.56 | 68.32 | 18.3 |
| 31 | 323.3 | 272.06 | 222.04 | 170.79999999999998 | 119.56 | 69.53999999999999 | 18.3 |
| 32 | 324.52 | 273.28 | 222.04 | 172.02 | 120.78 | 69.53999999999999 | 19.52 |
| 33 | 324.52 | 273.28 | 223.26 | 172.02 | 120.78 | 70.76 | 19.52 |
| 34 | 325.74 | 274.5 | 223.26 | 173.24 | 122.0 | 70.76 | 20.74 |
| 35 | 325.74 | 274.5 | 224.48 | 173.24 | 122.0 | 71.98 | 20.74 |
| 36 | 326.96 | 275.71999999999997 | 224.48 | 174.46 | 123.22 | 71.98 | 21.96 |
| 37 | 326.96 | 275.71999999999997 | 225.7 | 174.46 | 123.22 | 73.2 | 21.96 |
| 38 | 328.18 | 276.94 | 225.7 | 175.68 | 124.44 | 73.2 | 23.18 |
| 39 | 328.18 | 276.94 | 226.92 | 175.68 | 124.44 | 74.42 | 23.18 |
| 40 | 329.4 | 278.15999999999997 | 226.92 | 176.9 | 125.66 | 74.42 | 24.4 |
| 41 | 329.4 | 278.15999999999997 | 228.14 | 176.9 | 125.66 | 75.64 | 24.4 |
| 42 | 330.62 | 279.38 | 228.14 | 178.12 | 126.88 | 75.64 | 25.62 |
| 43 | 330.62 | 279.38 | 229.35999999999999 | 178.12 | 126.88 | 76.86 | 25.62 |
| 44 | 331.84 | 280.59999999999997 | 229.35999999999999 | 179.34 | 128.1 | 76.86 | 26.84 |
| 45 | 331.84 | 280.59999999999997 | 230.57999999999998 | 179.34 | 128.1 | 78.08 | 26.84 |
| 46 | 333.06 | 281.82 | 230.57999999999998 | 180.56 | 129.32 | 78.08 | 28.06 |
| 47 | 333.06 | 281.82 | 231.79999999999998 | 180.56 | 129.32 | 79.3 | 28.06 |
| 48 | 334.28 | 283.04 | 231.79999999999998 | 181.78 | 130.54 | 79.3 | 29.28 |
| 49 | 334.28 | 283.04 | 233.01999999999998 | 181.78 | 130.54 | 80.52 | 29.28 |
| 50 | 335.5 | 284.26 | 233.01999999999998 | 183.0 | 131.76 | 80.52 | 30.5 |
