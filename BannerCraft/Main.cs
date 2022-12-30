using Bannerlord.UIExtenderEx;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace BannerCraft
{
    public class Main : MBSubModuleBase
    {
        private static readonly UIExtender _extender = new(typeof(Main).Namespace!);

        private bool OnBeforeInitialModuleScreenSetAsRootCalled {  get; set; }

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            base.OnGameStart(game, gameStarter);

            if (gameStarter is not CampaignGameStarter campaignStarter)
            {
                return;
            }

            campaignStarter.AddModel(BannerCraftConfig.Instance.SmithingModel);
        }

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            _extender.Register(typeof(Main).Assembly);

            _extender.Enable();
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();

            if (OnBeforeInitialModuleScreenSetAsRootCalled)
            {
                return;
            }

            OnBeforeInitialModuleScreenSetAsRootCalled = true;
        }
    }
}
