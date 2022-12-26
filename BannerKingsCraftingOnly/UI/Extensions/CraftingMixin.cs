using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerCraft
{
	[ViewModelMixin("UpdateCraftingStamina")]
	public class CraftingMixin : BaseViewModelMixin<CraftingVM>
	{
		private readonly CraftingVM _crafting;

		private bool _isInArmorMode;

		private string _armorText;

		private ArmorCraftingVM _armorCrafting;

		private MBBindingList<ExtraCraftingResourceItemVM> _craftingResourceItems;

		[DataSourceProperty]
		public bool IsInArmorMode
		{
			get => _isInArmorMode;
			set
			{
				if (value != _isInArmorMode)
				{
					_isInArmorMode = value;
					ViewModel!.OnPropertyChangedWithValue(value, "IsInArmorMode");
				}
			}
		}

		[DataSourceProperty]
		public string ArmorText
		{
			get => _armorText;
			set
			{
				if (value != _armorText)
				{
					_armorText = value;
					ViewModel!.OnPropertyChangedWithValue(value, "ArmorText");
				}
			}
		}

        [DataSourceProperty]
		public ArmorCraftingVM ArmorCrafting
        {
			get => _armorCrafting;
			set
            {
				if (value != _armorCrafting)
                {
					_armorCrafting = value;
					ViewModel!.OnPropertyChangedWithValue(value, "ArmorCrafting");
                }
            }
        }

		[DataSourceProperty]
		public MBBindingList<ExtraCraftingResourceItemVM> ExtraMaterials
        {
			get => _craftingResourceItems;
			set
            {
				if (value != _craftingResourceItems)
                {
					_craftingResourceItems = value;
					ViewModel!.OnPropertyChangedWithValue(value, "ExtraMaterials");
                }
            }
        }

		public CraftingMixin(CraftingVM crafting) : base(crafting)
		{
			_crafting = crafting;

			ArmorText = GameTexts.FindText("str_bannercraft_crafting_category_armor").ToString();

			ArmorCrafting = new ArmorCraftingVM(this);

			UpdateExtraMaterialsAvailable();
		}

		private void UpdateExtraMaterialsAvailable()
        {
			if (ExtraMaterials == null)
            {
				ExtraMaterials = new MBBindingList<ExtraCraftingResourceItemVM>();
				for (int i = 0; i < (int)ExtraCraftingMaterials.NumExtraCraftingMats; ++i)
                {
					ExtraMaterials.Add(new ExtraCraftingResourceItemVM((ExtraCraftingMaterials)i, 0));
                }
            }

			for (int i = 0; i < (int)ExtraCraftingMaterials.NumExtraCraftingMats; ++i)
			{
				ItemObject extraCraftingMaterialItem = Config.Instance.SmithingModel.GetCraftingMaterialItem((ExtraCraftingMaterials)i);
				ExtraMaterials[i].ResourceAmount = MobileParty.MainParty.ItemRoster.GetItemNumber(extraCraftingMaterialItem);
			}
        }

		public override void OnRefresh()
		{
			base.OnRefresh();

			if (_crafting.IsInCraftingMode || _crafting.IsInRefinementMode || _crafting.IsInSmeltingMode)
			{
				IsInArmorMode = false;
			}
		}

		[DataSourceMethod]
		public void ExecuteMainActionBannerCraft()
		{
			if (!IsInArmorMode)
			{
				if (_crafting.IsInRefinementMode || _crafting.IsInSmeltingMode)
                {
					_crafting.ExecuteMainAction();
                }

				float botchChance = Config.Instance.SmithingModel.CalculateBotchingChance(_crafting.CurrentCraftingHero.Hero, _crafting.WeaponDesign.CurrentOrderDifficulty);

				if (MBRandom.RandomFloat < botchChance)
                {
					/*
					 * Crafting is botched, materials spent, item not crafted, minimal XP gained
					 */

                }
				else
                {
					_crafting.ExecuteMainAction();
                }
			}
			else
			{
				if (Config.Instance.SmithingModel.CalculateArmorDifficulty(_armorCrafting.CurrentItem.Item) > 0)
                {

                }
			}
		}

		[DataSourceMethod]
		public void ExecuteSwitchToArmor()
		{
			_crafting.IsInSmeltingMode = false;
			_crafting.IsInCraftingMode = false;
			_crafting.IsInRefinementMode = false;
			IsInArmorMode = true;

			ViewModel?.OnItemRefreshed?.Invoke(isItemVisible: false);

			string t = GameTexts.FindText("str_bannercraft_crafting_category_armor").ToString();
			_crafting.CurrentCategoryText = t;
			_crafting.MainActionText = t;
		}

		[DataSourceMethod]
		public void CloseWithWait()
		{
			_crafting.ExecuteCancel();
		}
	}
}