using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace Bannerlord.BannerCraft.PrefabExtensions
{
    [PrefabExtension("Crafting", "descendant::CraftingScreenWidget/Children")]
    internal class CraftingInsertArmorExtraMaterialsExtension : PrefabExtensionInsertPatch
    {
        /*
		 * Should be InsertType.Append but the item we're after doesn't have an Id
		 */
        [PrefabExtensionFileName] public string Id => "CraftingInsertArmorExtraMaterialsExtension";
        public override InsertType Type => InsertType.Child;
        public override int Index => 0;
    }
}