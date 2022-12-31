using Bannerlord.UIExtenderEx;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace BannerCraft
{
    internal sealed class MCMUISettings : AttributeGlobalSettings<MCMUISettings>
    {
        private float _maximumBotchChance = 0.95f;

        private bool _defaultSmeltingModel = false;

        private int _skillOverDifficultyBeforeNoPenalty = 25;

        public override string Id => "BannerCraft";
        public override string DisplayName => $"BannerCraft {typeof(MCMUISettings).Assembly.GetName().Version.ToString(3)}";
        public override string FolderName => "BannerCraft";
        public override string FormatType => "json";

        [SettingPropertyFloatingInteger("Maximum botch chance", 0f, 1f, "#0%", Order = 2, HintText = "Maximum chance to botch crafting when Crafting skill level is lower than difficulty.", RequireRestart = false)]
        [SettingPropertyGroup("General")]
        public float MaximumBotchChance
        {
            get => _maximumBotchChance;
            set
            {
                if (value != _maximumBotchChance)
                {
                    _maximumBotchChance = value;
                    OnPropertyChanged();
                }
            }
        }

        [SettingPropertyBool("Use Vanilla smelting calculations", HintText = "Use vanilla smelting calculations that turn 0.8 weight pugios into 2.5 weight of materials", RequireRestart = false)]
        [SettingPropertyGroup("General")]
        public bool DefaultSmeltingModel
        {
            get => _defaultSmeltingModel;
            set
            {
                if (value != _defaultSmeltingModel)
                {
                    _defaultSmeltingModel = value;
                    OnPropertyChanged();
                }
            }
        }

        [SettingPropertyInteger("Crafting penalty threshold", 0, 100, HintText = "How much higher your skill has to be than the item difficulty before you have no chance of making a bad item.", RequireRestart = false)]
        [SettingPropertyGroup("General")]
        public int SkillOverDifficultyBeforeNoPenalty
        {
            get => _skillOverDifficultyBeforeNoPenalty;
            set
            {
                if (value != _skillOverDifficultyBeforeNoPenalty)
                {
                    _skillOverDifficultyBeforeNoPenalty = value;
                    OnPropertyChanged();
                }
            }
        }
    }

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

            if (BannerCraftConfig.Instance.SmithingModel == null)
            {
                BannerCraftConfig.Instance.SmithingModel = new SmithingModelBC((SmithingModel)campaignStarter.Models.ToList().FindLast(model => model is SmithingModel));
            }

            /*campaignStarter.AddBehavior(new BannerCraftingBehavior());*/

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
