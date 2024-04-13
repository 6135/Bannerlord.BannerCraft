using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;
using System.Collections.Generic;

namespace Bannerlord.BannerCraft.PrefabExtensions
{
    [PrefabExtension("Crafting", "descendant::ButtonWidget[@Id='MainActionButtonWidget']")]
    internal class MainActionButtonPatch : PrefabExtensionSetAttributePatch
    {
        public override List<Attribute> Attributes => new()
        {
            new Attribute("Command.Click", "ExecuteMainActionBannerCraft")
        };
    }
}