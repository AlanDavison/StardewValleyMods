namespace UITestbed;

using System;
using StardewModdingAPI;

public class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        helper.Events.Input.ButtonPressed += (sender, args) => { };
    }
}
