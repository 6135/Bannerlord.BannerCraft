using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace Bannerlord.BannerCraft.PrefabExtensions
{
    [PrefabExtension("Crafting", "descendant::CraftingScreenWidget/Children")]
    internal class CraftingInsertArmorDifficultyExtension : PrefabExtensionInsertPatch
    {
        [PrefabExtensionFileName] public string Id => "CraftingInsertArmorDifficultyExtension";
        public override InsertType Type => InsertType.Child;
        public override int Index => 0;
    }
}