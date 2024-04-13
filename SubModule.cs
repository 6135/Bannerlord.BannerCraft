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

        private readonly UIExtender _extender = UIExtender.Create(Namespace);
        private readonly UIExtender? _bannerKingsExtender = UIExtender.GetUIExtenderFor("BannerKings");
        private readonly Harmony _harmony = new(Namespace);

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
            // Disable Banner Kings' armor crafting prefabs.
            _bannerKingsExtender?.Disable(AccessTools.TypeByName("BannerKings.UI.Extensions.CraftingArmorLeftPanelExtension1"));
            _bannerKingsExtender?.Disable(AccessTools.TypeByName("BannerKings.UI.Extensions.CraftingArmorLeftPanelExtension2"));
            _bannerKingsExtender?.Disable(AccessTools.TypeByName("BannerKings.UI.Extensions.CraftingInsertArmorCategoryExtension"));
            _bannerKingsExtender?.Disable(AccessTools.TypeByName("BannerKings.UI.Extensions.CraftingInsertHoursExtension"));
            _bannerKingsExtender?.Disable(AccessTools.TypeByName("BannerKings.UI.Extensions.RefinementCategoryButtonPatch"));
            _bannerKingsExtender?.Disable(AccessTools.TypeByName("BannerKings.UI.Extensions.CraftingCategoryButtonPatch"));
            _bannerKingsExtender?.Disable(AccessTools.TypeByName("BannerKings.UI.Extensions.SmeltingCategoryButtonPatch"));
            _bannerKingsExtender?.Disable(AccessTools.TypeByName("BannerKings.UI.Extensions.MainActionButtonPatch"));
            _bannerKingsExtender?.Disable(AccessTools.TypeByName("BannerKings.UI.Extensions.CraftingCancelButtonPatch"));
            _harmony.PatchAll();
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