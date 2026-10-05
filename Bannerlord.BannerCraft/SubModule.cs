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
            "BannerKings.UI.Extensions.CraftingCancelButtonPatch",
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
            _harmony.PatchAll();
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();

            // Done here rather than in OnSubModuleLoad so it also works when Banner Kings (or Banner Kings Redux)
            // loads after BannerCraft. Otherwise its crafting UI stays half enabled and leaves a popup that can't be closed.
            DisableBannerKingsCrafting();
        }

        private void DisableBannerKingsCrafting()
        {
            if (_bannerKingsCraftingDisabled)
            {
                return;
            }

            var bannerKingsExtender = UIExtender.GetUIExtenderFor("BannerKings");
            if (bannerKingsExtender == null)
            {
                return;
            }

            foreach (var typeName in BannerKingsCraftingExtensions)
            {
                var type = AccessTools.TypeByName(typeName);
                if (type != null)
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