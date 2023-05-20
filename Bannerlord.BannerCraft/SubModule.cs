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
    internal sealed class MCMUISettings : AttributeGlobalSettings<MCMUISettings>
    {
        private float _maximumBotchChance = 0.95f;

        private bool _defaultSmeltingModel = false;

        private bool _allowSmeltingOtherItems = true;

        private int _skillOverDifficultyBeforeNoPenalty = 25;

        private bool _allowCraftingNormalWeapons = false;

        public override string Id => "Bannerlord.BannerCraft";
        public override string DisplayName => "BannerCraft";
        public override string FolderName => "BannerCraft";
        public override string FormatType => "json";

        [SettingPropertyFloatingInteger("{=bannercraft_mcm_maximum_botch_chance}Maximum botch chance", 0f, 1f, "#0%", Order = 2, HintText = "{=bannercraft_mcm_maximum_botch_chance_description}Maximum chance to botch crafting when Crafting skill level is lower than difficulty.", RequireRestart = false)]
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

        [SettingPropertyBool("{=bannercraft_mcm_use_vanilla_smelting_calculations}Use Vanilla smelting calculations", HintText = "{=bannercraft_mcm_use_vanilla_smelting_calculations_description}Use vanilla smelting calculations that turn 0.8 weight pugios into 2.5 weight of materials.", RequireRestart = false)]
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

        [SettingPropertyBool("{=bannercraft_mcm_allow_smelting_other_items}Allow smelting other items", HintText = "{=bannercraft_mcm_allow_smelting_other_items_description}Allow smelting other items such as armor, shields, etc as long as they return at least one material", RequireRestart = false)]
        [SettingPropertyGroup("General")]
        public bool AllowSmeltingOtherItems
        {
            get => _allowSmeltingOtherItems;
            set
            {
                if (value != _allowSmeltingOtherItems)
                {
                    _allowSmeltingOtherItems = value;
                    OnPropertyChanged();
                }
            }
        }

        [SettingPropertyInteger("{=bannercraft_mcm_crafting_penalty_threshold}Crafting penalty threshold", 0, 100, HintText = "{=bannercraft_mcm_crafting_penalty_threshold_description}How much higher your skill has to be than the item difficulty before you have no chance of making a bad item.", RequireRestart = false)]
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

        [SettingPropertyBool("{=bannercraft_mcm_allow_crafting_normal_weapons}Allow crafting normal weapons", HintText = "{bannercraft_mcm_allow_crafting_normal_weapons_description}Allow crafting normal weapons without", RequireRestart = false)]
        [SettingPropertyGroup("General")]
        public bool AllowCraftingNormalWeapons
        {
            get => _allowCraftingNormalWeapons;
            set
            {
                if (value != _allowCraftingNormalWeapons)
                {
                    _allowCraftingNormalWeapons = value;
                    OnPropertyChanged();
                }
            }
        }
    }

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
