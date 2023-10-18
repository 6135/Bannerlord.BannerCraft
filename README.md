# BannerCraft

![build](https://github.com/6135/Bannerlord.BannerCraft/actions/workflows/build.yml/badge.svg?event=push)


Crafting extension for Mount and Blade 2: Bannerlord

# Download

Visit [nexus mods](https://www.nexusmods.com/mountandblade2bannerlord/mods/5932) to download or download [latest release](https://github.com/6135/Bannerlord.BannerCraft/releases/latest) and install manually. 

# Depencies 


| Dependency                   | Version  | Required |
|------------------------------|----------|----------|
| Harmony                      | >= 2.2.2 | ✅        |
| ButterLib                    | >= 2.6.3 | ✅        |
| UIExtenderEx                 | >= 2.8.0 | ✅        |
| Mod Configuration Menu (MCM) | >= 5.9.1 | ✅        |


# License
Do whatever you want, just provide a link back here if you use it in your own project.

# Credits
Special thanks to the BannerKings mod team (https://github.com/R-Vaccari/bannerlord-banner-kings) for showing how to edit the crafting system.
Forked from @adwitkow updated version of the mod.
Original author: https://github.com/Evangel63/BannerCraft

# Changelog

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
