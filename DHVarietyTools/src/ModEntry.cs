using System;
using System.Linq;
using DecidedlyShared.Logging;
using DHVarietyTools.Utilities;
using HarmonyLib;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;

namespace DHVarietyTools;

public class ModEntry : Mod
{
    private static IMonitor monitor;

    public override void Entry(IModHelper helper)
    {
        ModEntry.monitor = this.Monitor;
        Harmony harmony = new Harmony(this.ModManifest.UniqueID);
        Logger logger = new Logger(this.Monitor);
        Patches patches = new Patches(this.Monitor, helper, logger);


        // TODO: For full release, use the CustomFields field to scan for methods to patch in Patches.cs.
        // Example:
        /* For FenceFixer:
         * "CustomFields": {
               "DH_Variety_Tool_Class": "FenceFixer1"
           }
           Scan for method named FenceFixer1(args...) in Patches.cs.
         */

        helper.Events.GameLoop.SaveLoaded += (sender, args) =>
        {
            PatchToolMethods patcher = new PatchToolMethods(harmony, patches, logger, Game1.toolData.Values.ToList());
            patcher.DoPatches();
        };
    }
}
