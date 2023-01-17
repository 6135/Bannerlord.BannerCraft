using Bannerlord.UIExtenderEx;
using HarmonyLib;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using System;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting.Smelting;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;

namespace BannerCraft
{
	internal sealed class MCMUISettings : AttributeGlobalSettings<MCMUISettings>
	{
		private float _maximumBotchChance = 0.95f;

		private bool _defaultSmeltingModel = false;

		private bool _allowSmeltingOtherItems = true;

		private int _skillOverDifficultyBeforeNoPenalty = 25;

		private bool _allowCraftingNormalWeapons = false;

		public override string Id => "BannerCraft";
		public override string DisplayName => $"BannerCraft {ModuleHelper.GetModuleInfo("BannerCraft").Version}";
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

	public class Main : MBSubModuleBase
	{
		private static readonly UIExtender _extender = new(typeof(Main).Namespace!);

		private bool OnBeforeInitialModuleScreenSetAsRootCalled {  get; set; }

		private bool PatchedBSC;

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

			_extender.Register(typeof(Main).Assembly);

			_extender.Enable();

			Harmony harmony = new Harmony("BannerCraft");

            Assembly BSCMainFrameAssembly = null;
            Type BSCSmeltingItemRosterWrapperType = null;
            Type BSCBetterSmeltingVMType = null;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Count(); i++)
            {
                var assembly = assemblies[i];
                if (assembly.FullName.Contains("BetterSmithingContinued.MainFrame"))
                {
                    var types = assembly.GetTypes();
                    for (int j = 0; j < types.Count(); j++)
                    {
                        var type = types[j];
                        if (type.Name.Contains("Smelting"))
                        {
                            InformationManager.DisplayMessage(new InformationMessage(string.Format("Type {0}", type.Name)));
                        }

                        if (type.Name.Equals("SmeltingItemRosterWrapper"))
                        {
                            InformationManager.DisplayMessage(new InformationMessage(string.Format("Found type {0}", type.Name)));
                            BSCSmeltingItemRosterWrapperType = type;
                        }

                        if (type.Name.Equals("BetterSmeltingVM"))
                        {
                            InformationManager.DisplayMessage(new InformationMessage(string.Format("Found type {0}", type.Name)));
                            BSCBetterSmeltingVMType = type;
                        }
                    }

                    BSCMainFrameAssembly = assembly;
                }
            }

            if (BSCMainFrameAssembly != null && BSCSmeltingItemRosterWrapperType != null && BSCBetterSmeltingVMType != null)
            {
                BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                MethodInfo smeltingItemRosterWrapperOriginal = BSCSmeltingItemRosterWrapperType.GetMethod("PerformFullRefreshCheck", bindingFlags);
                MethodInfo smeltingItemRosterWrapperPostfix = typeof(BSCSmeltingItemRosterWrapperPatch).GetMethod(nameof(BSCSmeltingItemRosterWrapperPatch.PerformFullRefreshPostFix));

                MethodInfo betterSmeltingVMOriginal = BSCBetterSmeltingVMType.GetMethod("ItemIsVisible", bindingFlags);
                MethodInfo betterSmeltingVMPrefix = typeof(BSCBetterSmeltingVMPatch).GetMethod(nameof(BSCBetterSmeltingVMPatch.ItemIsVisiblePreFix));

                if (smeltingItemRosterWrapperOriginal != null && betterSmeltingVMOriginal != null)
                {
                    harmony.Patch(smeltingItemRosterWrapperOriginal, postfix: new HarmonyMethod(smeltingItemRosterWrapperPostfix));
                    harmony.Patch(betterSmeltingVMOriginal, prefix: new HarmonyMethod(betterSmeltingVMPrefix));
                    PatchedBSC = true;
                }
            }

            MethodInfo original = typeof(CraftingCampaignBehavior).GetMethod(nameof(CraftingCampaignBehavior.DoSmelting));
			MethodInfo prefix = typeof(CraftingCampaignBehaviorPatch).GetMethod(nameof(CraftingCampaignBehaviorPatch.DoSmeltingPrefix));
			harmony.Patch(original, prefix: new HarmonyMethod(prefix));

			if (!PatchedBSC)
			{
				MethodInfo SmeltingVMOriginal = typeof(SmeltingVM).GetMethod(nameof(SmeltingVM.RefreshList));
				MethodInfo SmeltingVMPostfix = typeof(SmeltingVMPatch).GetMethod(nameof(SmeltingVMPatch.RefreshListPostfix));
				harmony.Patch(SmeltingVMOriginal, postfix: new HarmonyMethod(SmeltingVMPostfix));
			}
		}

		protected override void OnBeforeInitialModuleScreenSetAsRoot()
		{
			base.OnBeforeInitialModuleScreenSetAsRoot();

			if (OnBeforeInitialModuleScreenSetAsRootCalled)
			{
				return;
			}

			OnBeforeInitialModuleScreenSetAsRootCalled = true;

			if (PatchedBSC)
			{
				InformationManager.DisplayMessage(new InformationMessage("BannerCraft with Better Smithing Continued patches loaded"));
			}
			else
			{
				InformationManager.DisplayMessage(new InformationMessage("BannerCraft loaded"));
			}
		}
	}
}
