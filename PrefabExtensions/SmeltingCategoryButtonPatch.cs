using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;
using System.Collections.Generic;

namespace Bannerlord.BannerCraft.PrefabExtensions
{
    [PrefabExtension("Crafting", "descendant::ButtonWidget[@Id='SmeltingCategoryButton']")]
    internal class SmeltingCategoryButtonPatch : PrefabExtensionSetAttributePatch
    {
        public override List<Attribute> Attributes => new()
        {
            new Attribute("SuggestedWidth", "320")
        };
    }
}