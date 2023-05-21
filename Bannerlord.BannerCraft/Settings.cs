using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

namespace Bannerlord.BannerCraft
{
    public sealed class Settings : AttributeGlobalSettings<Settings>
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
}
