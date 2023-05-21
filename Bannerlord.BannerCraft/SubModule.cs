using Bannerlord.BannerCraft.Models.Vanilla;
using Bannerlord.BannerCraft.Patches;
using Bannerlord.UIExtenderEx;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting.Smelting;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.BannerCraft
{
    public class SubModule : MBSubModuleBase
    {
		private static readonly string Namespace = typeof(SubModule).Namespace;

        private readonly UIExtender _extender = new UIExtender(Namespace);
        private readonly Harmony _harmony = new Harmony(Namespace);

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            base.OnGameStart(game, gameStarter);

            if (gameStarter is not CampaignGameStarter)
            {
                return;
            }

            var smithingModel = GetGameModel<SmithingModel>(gameStarter);

            if (smithingModel is null)
            {
                throw new InvalidOperationException("Default SmithingModel was not found.");
            }
            
            gameStarter.AddModel(new SmithingModelBC(smithingModel));
        }

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            _extender.Register(typeof(SubModule).Assembly);
            _extender.Enable();

            MethodInfo original = typeof(CraftingCampaignBehavior).GetMethod(nameof(CraftingCampaignBehavior.DoSmelting));
            MethodInfo prefix = typeof(CraftingCampaignBehaviorPatch).GetMethod(nameof(CraftingCampaignBehaviorPatch.DoSmeltingPrefix));
            _harmony.Patch(original, prefix: new HarmonyMethod(prefix));

            MethodInfo SmeltingVMOriginal = typeof(SmeltingVM).GetMethod(nameof(SmeltingVM.RefreshList));
            MethodInfo SmeltingVMPostfix = typeof(SmeltingVMPatch).GetMethod(nameof(SmeltingVMPatch.RefreshListPostfix));
            _harmony.Patch(SmeltingVMOriginal, postfix: new HarmonyMethod(SmeltingVMPostfix));
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
