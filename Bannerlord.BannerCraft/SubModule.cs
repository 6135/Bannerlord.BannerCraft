using Bannerlord.BannerCraft.Models.Vanilla;
using Bannerlord.BannerCraft.Patches;
using Bannerlord.UIExtenderEx;
using HarmonyLib;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
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

            if (gameStarter is not CampaignGameStarter campaignStarter)
            {
                return;
            }

            if (BannerCraftConfig.Instance.SmithingModel == null)
            {
                BannerCraftConfig.Instance.SmithingModel = new SmithingModelBC((SmithingModel)campaignStarter.Models.ToList().FindLast(model => model is SmithingModel));
            }

            campaignStarter.AddModel(BannerCraftConfig.Instance.SmithingModel);
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
    }
}
