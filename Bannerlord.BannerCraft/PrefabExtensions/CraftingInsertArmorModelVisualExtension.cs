using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace Bannerlord.BannerCraft.PrefabExtensions
{
    [PrefabExtension("Crafting", "descendant::CraftingScreenWidget/Children")]
    internal class CraftingInsertArmorModelVisualExtension : PrefabExtensionInsertPatch
    {
        [PrefabExtensionFileName] public string Id => "CraftingInsertArmorModelVisualExtension";
        public override InsertType Type => InsertType.Child;
        public override int Index => 0;
    }
}