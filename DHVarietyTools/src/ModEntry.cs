using System.Linq;
using DecidedlyShared.Logging;
using DHVarietyTools.Utilities;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace DHVarietyTools;

public class ModEntry : Mod
{
    private Harmony harmony;
    private Patches patchesClass;
    private Logger logger;

    public override void Entry(IModHelper helper)
    {
        this.harmony = new Harmony(this.ModManifest.UniqueID);
        this.logger = new Logger(this.Monitor);
        this.patchesClass = new Patches(this.Monitor, helper, this.logger);
        helper.Events.GameLoop.SaveLoaded += this.GameLoopOnSaveLoaded;
    }

    private void GameLoopOnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        PatchToolMethods patcher = new PatchToolMethods(this.harmony, this.patchesClass, this.logger, Game1.toolData.Values.ToList());
        patcher.DoPatches();
    }
}
