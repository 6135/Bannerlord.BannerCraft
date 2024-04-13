using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace Bannerlord.BannerCraft.PrefabExtensions
{
    [PrefabExtension("Crafting", "descendant::ListPanel[@Id='CategoryParent']/Children")]
    internal class CraftingInsertArmorCategoryExtension : PrefabExtensionInsertPatch
    {
        [PrefabExtensionFileName] public string Id => "CraftingInsertArmorCategoryExtension";
        public override InsertType Type => InsertType.Child;
        public override int Index => 2;
    }
}