using System;
using DecidedlyShared.Ui;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Minigames;

namespace ATropicalStart;

public class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        var harmony = new Harmony(this.ModManifest.UniqueID);

        // All of our Harmony patches to disable interactions while in build mode.
        harmony.Patch(
            AccessTools.Method(typeof(Intro), nameof(Intro.tick)),
            new HarmonyMethod(typeof(Patches), nameof(Patches.Intro_tick_prefix)));
    }
}
