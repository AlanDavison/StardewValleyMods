using System;
using System.Collections.Generic;
using DecidedlyShared.Logging;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.GameData.Buffs;

namespace InvisibleBuffs;

public class ModEntry : Mod
{
    private Logger logger;

    public override void Entry(IModHelper helper)
    {
        this.logger = new Logger(this.Monitor);
        BuffPatches.Logger = this.logger;
        Harmony harmony = new Harmony(this.ModManifest.UniqueID);

        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(Buff),
                [typeof(string),
                    typeof(string),
                    typeof(string),
                    typeof(int),
                    typeof(Texture2D),
                    typeof(int),
                    typeof(BuffEffects),
                    typeof(bool?),
                    typeof(string),
                    typeof(string)]),
            postfix: new HarmonyMethod(typeof(BuffPatches),
                nameof(BuffPatches.BuffConstructor_Prefix)));
    }
}
