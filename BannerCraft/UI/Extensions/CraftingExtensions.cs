using System.Collections.Generic;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace BannerCraft
{
	[PrefabExtension("Crafting", "descendant::CraftingScreenWidget/Children", "Crafting")]
	internal class CraftingInsertArmorModelVisualExtension : PrefabExtensionInsertPatch
	{
		[PrefabExtensionFileName] public string Id => "CraftingInsertArmorModelVisualExtension";
		public override InsertType Type => InsertType.Child;
		public override int Index => 0;
	}

	[PrefabExtension("Crafting", "descendant::CraftingScreenWidget/Children", "Children")]
	internal class CraftingInsertArmorDifficultyExtension : PrefabExtensionInsertPatch
	{
		[PrefabExtensionFileName] public string Id => "CraftingInsertArmorDifficultyExtension";
		public override InsertType Type => InsertType.Child;
		public override int Index => 0;
	}

	[PrefabExtension("Crafting", "descendant::CraftingScreenWidget/Children", "Children")]
	internal class CraftingInsertArmorExtraMaterialsExtension : PrefabExtensionInsertPatch
	{
		/*
		 * Should be InsertType.Append but the item we're after doesn't have an Id
		 */
		[PrefabExtensionFileName] public string Id => "CraftingInsertArmorExtraMaterialsExtension";
		public override InsertType Type => InsertType.Child;
		public override int Index => 0;
	}

    [PrefabExtension("Crafting", "descendant::ListPanel[@Id='CategoryParent']/Children", "Crafting")]
    internal class CraftingInsertArmorCategoryExtension : PrefabExtensionInsertPatch
    {
        [PrefabExtensionFileName] public string Id => "CraftingInsertArmorCategoryExtension";
        public override InsertType Type => InsertType.Child;
        public override int Index => 2;
    }

    [PrefabExtension("Crafting", "descendant::Widget[@Id='RightPanel']/Children", "Crafting")]
    internal class CraftingInsertArmorCategoryPanelExtension : PrefabExtensionInsertPatch
    {
        [PrefabExtensionFileName] public string Id => "CraftingInsertArmorCategoryPanelExtension";
        public override InsertType Type => InsertType.Child;
        public override int Index => 3;
    }

    [PrefabExtension("Crafting", "descendant::CraftingTemplateSelectionPopup[@Id='WeaponClassSelectionPopup']", "Crafting")]
	internal class CraftingInsertArmorClassSelectionPopupExtension : PrefabExtensionInsertPatch
	{
		[PrefabExtensionFileName] public string Id => "CraftingInsertArmorClassSelectionPopupExtension";
		public override InsertType Type => InsertType.Append;
	}

	[PrefabExtension("Crafting", "descendant::NewCraftedWeaponPopup[@Id='NewCraftedWeaponPopupWidget']", "Crafting")]
	internal class CraftingInsertNewCraftedArmorPopupExtension : PrefabExtensionInsertPatch
	{
		[PrefabExtensionFileName] public string Id => "CraftingInsertNewCraftedArmorPopupExtension";
		public override InsertType Type => InsertType.Append;
	}

	[PrefabExtension("Crafting", "descendant::ButtonWidget[@Id='RefinementCategoryButton']", "Crafting")]
	internal class RefinementCategoryButtonPatch : PrefabExtensionSetAttributePatch
	{
		public override List<Attribute> Attributes => new()
		{
			new Attribute("SuggestedWidth", "320")
		};
	}

	[PrefabExtension("Crafting", "descendant::ButtonWidget[@Id='CraftingCategoryButton']", "Crafting")]
	internal class CraftingCategoryButtonPatch : PrefabExtensionSetAttributePatch
	{
		public override List<Attribute> Attributes => new()
		{
			new Attribute("SuggestedWidth", "320")
		};
	}

	[PrefabExtension("Crafting", "descendant::ButtonWidget[@Id='SmeltingCategoryButton']", "Crafting")]
	internal class SmeltingCategoryButtonPatch : PrefabExtensionSetAttributePatch
	{
		public override List<Attribute> Attributes => new()
		{
			new Attribute("SuggestedWidth", "320")
		};
	}

	[PrefabExtension("Crafting", "descendant::ButtonWidget[@Id='MainActionButtonWidget']", "Crafting")]
	internal class MainActionButtonPatch : PrefabExtensionSetAttributePatch
	{
		public override List<Attribute> Attributes => new()
		{
			new Attribute("Command.Click", "ExecuteMainActionBannerCraft")
		};
	}
}