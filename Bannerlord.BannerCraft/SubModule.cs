using Bannerlord.BannerCraft.Mixins;
using Bannerlord.BannerCraft.Models;
using Bannerlord.UIExtenderEx;
using HarmonyLib;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.BannerCraft
{
    public class SubModule : MBSubModuleBase
    {
        private static readonly string Namespace = typeof(SubModule).Namespace;

        /*
         * Banner Kings and Banner Kings - Redux both keep these under the BannerKings namespace and
         * register their UIExtender as "BannerKings", so the same list covers both.
         */
        private static readonly string[] BannerKingsCraftingExtensions =
        {
            "BannerKings.UI.Extensions.CraftingMixin",
            "BannerKings.UI.Extensions.CraftingArmorLeftPanelExtension1",
            "BannerKings.UI.Extensions.CraftingArmorLeftPanelExtension2",
            "BannerKings.UI.Extensions.CraftingInsertArmorCategoryExtension",
            "BannerKings.UI.Extensions.CraftingInsertHoursExtension",
            "BannerKings.UI.Extensions.RefinementCategoryButtonPatch",
            "BannerKings.UI.Extensions.CraftingCategoryButtonPatch",
            "BannerKings.UI.Extensions.SmeltingCategoryButtonPatch",
            "BannerKings.UI.Extensions.MainActionButtonPatch",
            "BannerKings.UI.Extensions.CraftingCancelButtonPatch"
        };

        private readonly UIExtender _extender = UIExtender.Create(Namespace);
        private readonly Harmony _harmony = new(Namespace);

        private bool _bannerKingsCraftingDisabled;

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            if (gameStarterObject is not CampaignGameStarter)
            {
                return;
            }

            var smithingModel = GetGameModel<SmithingModel>(gameStarterObject) ?? throw new InvalidOperationException("Default SmithingModel was not found.");

            gameStarterObject.AddModel(new BannerCraftSmithingModel(smithingModel));
        }

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            CraftingMixin.ApplyPatches(_harmony);

            _extender.Register(typeof(SubModule).Assembly);
            _extender.Enable();
            DisableBannerKingsCrafting();
            _harmony.PatchAll();
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();

            /*
             * Banner Kings may not have registered its UIExtender yet during OnSubModuleLoad — that
             * depends on load order, and Banner Kings - Redux uses a different module id, so its
             * LoadBeforeThis metadata was not being matched. By this point every module has loaded,
             * and no crafting screen has been opened yet, so the prefab patches and mixin can still
             * be disabled in time.
             */
            DisableBannerKingsCrafting();
        }

        // Disable Banner Kings' armor crafting mixin and prefabs. Safe to call more than once.
        private void DisableBannerKingsCrafting()
        {
            if (_bannerKingsCraftingDisabled)
            {
                return;
            }

            var bannerKingsExtender = UIExtender.GetUIExtenderFor("BannerKings");

            if (bannerKingsExtender is null)
            {
                return;
            }

            foreach (var typeName in BannerKingsCraftingExtensions)
            {
                var type = AccessTools.TypeByName(typeName);

                if (type is not null)
                {
                    bannerKingsExtender.Disable(type);
                }
            }

            _bannerKingsCraftingDisabled = true;
        }

        private static T? GetGameModel<T>(IGameStarter gameStarterObject) where T : GameModel
        {
            var models = gameStarterObject.Models.ToArray();

            for (int index = models.Length - 1; index >= 0; --index)
            {
                if (models[index] is T gameModel1)
                    return gameModel1;
            }
            return default;
        }
    }
}