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

Here's the data formatted as a Markdown table:

| S \ T | 6   | 5   | 4   | 3   | 2   | 1   |
|-------|-----|-----|-----|-----|-----|-----|
| 10    | 256 | 205 | 156 | 105 | 55  | 6   |
| 11    | 256 | 206 | 156 | 106 | 56  | 6   |
| 12    | 257 | 207 | 157 | 107 | 57  | 7   |
| 13    | 257 | 207 | 157 | 107 | 57  | 7   |
| 14    | 258 | 208 | 158 | 108 | 58  | 8   |
| 15    | 259 | 208 | 159 | 108 | 58  | 9   |
| 16    | 259 | 209 | 159 | 109 | 59  | 9   |
| 17    | 260 | 210 | 160 | 110 | 60  | 10  |
| 18    | 260 | 210 | 160 | 110 | 60  | 10  |
| 19    | 261 | 211 | 161 | 111 | 61  | 11  |
| 20    | 262 | 211 | 162 | 111 | 61  | 12  |
| 21    | 262 | 212 | 162 | 112 | 62  | 12  |
| 22    | 263 | 213 | 163 | 113 | 63  | 13  |
| 23    | 263 | 213 | 163 | 113 | 63  | 13  |
| 24    | 264 | 214 | 164 | 114 | 64  | 14  |
| 25    | 265 | 214 | 165 | 114 | 65  | 15  |
| 26    | 265 | 215 | 165 | 115 | 65  | 15  |
| 27    | 266 | 216 | 166 | 116 | 66  | 16  |
| 28    | 266 | 216 | 166 | 116 | 66  | 16  |
| 29    | 267 | 217 | 167 | 117 | 67  | 17  |
| 30    | 268 | 217 | 168 | 117 | 68  | 18  |
| 31    | 268 | 218 | 168 | 118 | 68  | 18  |
| 32    | 269 | 219 | 169 | 119 | 69  | 19  |
| 33    | 269 | 219 | 169 | 119 | 69  | 19  |
| 34    | 270 | 220 | 170 | 120 | 70  | 20  |
| 35    | 271 | 220 | 171 | 120 | 71  | 21  |
| 36    | 271 | 221 | 171 | 121 | 71  | 21  |
| 37    | 272 | 222 | 172 | 122 | 72  | 22  |
| 38    | 272 | 222 | 172 | 122 | 72  | 22  |
| 39    | 273 | 223 | 173 | 123 | 73  | 23  |
| 40    | 274 | 223 | 174 | 123 | 74  | 24  |
| 41    | 274 | 224 | 174 | 124 | 74  | 24  |
| 42    | 275 | 225 | 175 | 125 | 75  | 25  |
| 43    | 275 | 225 | 175 | 125 | 75  | 25  |
| 44    | 276 | 226 | 176 | 126 | 76  | 26  |
| 45    | 277 | 226 | 177 | 126 | 76  | 27  |
| 46    | 277 | 227 | 177 | 127 | 77  | 27  |
| 47    | 278 | 228 | 178 | 128 | 78  | 28  |
| 48    | 278 | 228 | 178 | 128 | 78  | 28  |
| 49    | 279 | 229 | 179 | 129 | 79  | 29  |
| 50    | 280 | 229 | 180 | 130 | 79  | 30  |
