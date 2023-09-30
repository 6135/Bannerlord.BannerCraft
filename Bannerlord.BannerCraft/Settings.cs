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

        /**
         * 
         * For testing porpuses
         */
        private bool _useOldModifierBehaviour = false;
        [SettingPropertyBool("Use old modifier behaviour", HintText = "Use old modifier behaviour", RequireRestart = false)]
        [SettingPropertyGroup("BannerCraft/Behaviours")]
        public bool UseOldModifierBehaviour
        {
            get => _useOldModifierBehaviour;
            set
            {
                if (value != _useOldModifierBehaviour)
                {
                    _useOldModifierBehaviour = value;
                    OnPropertyChanged();
                }
            }
        }
        private float _legendaryChanceIncrease = 0f;

        [SettingPropertyFloatingInteger("{=bannercraft_mcm_crafting_legendary_added_chance}Legendary added chance", 0f, 1f, "#0%", HintText = "{=bannercraft_mcm_crafting_legendary_added_chance_description}Added chance to get a Legendary crafting result.", RequireRestart = false)]
        [SettingPropertyGroup("BannerCraft/Chances")]
        public float LegendaryChanceIncrease
        {
            get => _legendaryChanceIncrease;
            set
            {
                if (value != _legendaryChanceIncrease)
                {
                    _legendaryChanceIncrease = value;
                    OnPropertyChanged();
                }
            }
        }   
        private float _masterworkChanceIncrease = 0f;
        [SettingPropertyFloatingInteger("{=bannercraft_mcm_crafting_masterwork_added_chance}Masterwork added chance", 0f, 1f, "#0%", HintText = "{=bannercraft_mcm_crafting_masterwork_added_chance_description}Added chance to get a Masterwork crafting result.", RequireRestart = false)]
        [SettingPropertyGroup("BannerCraft/Chances")]
        public float MasterworkChanceIncrease
        {
            get => _masterworkChanceIncrease;
            set
            {
                if (value != _masterworkChanceIncrease)
                {
                    _masterworkChanceIncrease = value;
                    OnPropertyChanged();
                }
            }
        }
        private float _fineChanceIncrease = 0f;
        [SettingPropertyFloatingInteger("{=bannercraft_mcm_crafting_fine_added_chance}Fine added chance", 0f, 1f, "#0%", HintText = "{=bannercraft_mcm_crafting_fine_added_chance_description}Added chance to get a Fine crafting result.", RequireRestart = false)]
        [SettingPropertyGroup("BannerCraft/Chances")]
        public float FineChanceIncrease
        {
            get => _fineChanceIncrease;
            set
            {
                if (value != _fineChanceIncrease)
                {
                    _fineChanceIncrease = value;
                    OnPropertyChanged();
                }
            }
        }
        private float _commonChanceIncrease = 0f;
        [SettingPropertyFloatingInteger("{=bannercraft_mcm_crafting_common_added_chance}Common added chance", 0f, 1f, "#0%", HintText = "{=bannercraft_mcm_crafting_common_added_chance_description}Added chance to get a Common crafting result.", RequireRestart = false)]
        [SettingPropertyGroup("BannerCraft/Chances")]
        public float CommonChanceIncrease
        {
            get => _commonChanceIncrease;
            set
            {
                if (value != _commonChanceIncrease)
                {
                    _commonChanceIncrease = value;
                    OnPropertyChanged();
                }
            }
        }
        private float _inferiorChanceIncrease = 0f;
        [SettingPropertyFloatingInteger("{=bannercraft_mcm_crafting_inferior_added_chance}Inferior added chance", 0f, 1f, "#0%", HintText = "{=bannercraft_mcm_crafting_inferior_added_chance_description}Added chance to get a Inferior crafting result.", RequireRestart = false)]
        [SettingPropertyGroup("BannerCraft/Chances")]
        public float InferiorChanceIncrease
        {
            get => _inferiorChanceIncrease;
            set
            {
                if (value != _inferiorChanceIncrease)
                {
                    _inferiorChanceIncrease = value;
                    OnPropertyChanged();
                }
            }
        }
        private float _poorChanceIncrease = 0f;
        [SettingPropertyFloatingInteger("{=bannercraft_mcm_crafting_poor_added_chance}Poor added chance", 0f, 1f, "#0%", HintText = "{=bannercraft_mcm_crafting_poor_added_chance_description}Added chance to get a Poor crafting result.", RequireRestart = false)]
        [SettingPropertyGroup("BannerCraft/Chances")]
        public float PoorChanceIncrease
        {
            get => _poorChanceIncrease;
            set
            {
                if (value != _poorChanceIncrease)
                {
                    _poorChanceIncrease = value;
                    OnPropertyChanged();
                }
            }
        }
        /**
         * 
         * For testing porpuses
         */
        public override string Id => "Bannerlord.BannerCraft";
        public override string DisplayName => "BannerCraft";
        public override string FolderName => "BannerCraft";
        public override string FormatType => "json";

        [SettingPropertyFloatingInteger("{=bannercraft_mcm_maximum_botch_chance}Maximum botch chance", 0f, 1f, "#0%", Order = 2, HintText = "{=bannercraft_mcm_maximum_botch_chance_description}Maximum chance to botch crafting when Crafting skill level is lower than difficulty.", RequireRestart = false)]
        [SettingPropertyGroup("BannerCraft/Behaviours")]
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
        [SettingPropertyGroup("BannerCraft/Behaviours")]
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
        [SettingPropertyGroup("BannerCraft/Behaviours")]
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
        [SettingPropertyGroup("BannerCraft/Behaviours")]
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
        [SettingPropertyGroup("BannerCraft/Behaviours")]
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