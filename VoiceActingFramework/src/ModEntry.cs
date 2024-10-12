using HarmonyLib;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley.Menus;

namespace VAF;

public class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        Patches patches = new Patches(helper, this.Monitor, this);
    }
}
