using DecidedlyShared.Logging;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.TerrainFeatures;
using Utility = ExJojaFarm.Utilities.Utility;

namespace ExJojaFarm;

public class ModEntry : Mod
{
    private const string FARM_ID = "DH.ExJojaFarm.ExJojaFarmContent_Farm";
    private Logger logger;
    public static Logger StaticLogger;

    public override void Entry(IModHelper helper)
    {
        this.logger = new Logger(this.Monitor);
        ModEntry.StaticLogger = this.logger;
        Harmony harmony = new Harmony(this.ModManifest.UniqueID);

        harmony.Patch(
            original: AccessTools.Method(typeof(Farm), nameof(Farm.onNewGame)),
            postfix: new HarmonyMethod(typeof(ModEntry),
                nameof(ModEntry.Farm_OnNewGame_Postfix)));

        harmony.Patch(
            original: AccessTools.Method(typeof(Tree), nameof(Tree.dayUpdate)),
            postfix: new HarmonyMethod(typeof(ModEntry),
                nameof(ModEntry.Tree_DayUpdate_Postfix)));
    }

    public static void Tree_DayUpdate_Postfix(Tree __instance)
    {
        if (__instance.Location is not Farm)
            return;

        if (__instance.growthStage.Value < 5) // 5 is fully grown.
            return;

        Vector2 treeTile = __instance.Tile;
        string tileProperty =
            __instance.Location.doesTileHaveProperty(
                (int)treeTile.X,
                (int)treeTile.Y,
                "DH.EJF.SlimeGround",
                "Back3");

        if (tileProperty is not null)
            __instance.onGreenRainDay();
    }

    public static void Farm_OnNewGame_Postfix(Farm __instance)
    {
        if (!Utility.IsExJojaFarm())
            return;

        Farm farm = Game1.getFarm();

        string mapSiloTile = farm.getMapProperty("DefaultSiloTile");

        if (mapSiloTile is null || !Utility.TryParseVector2(mapSiloTile, out Vector2 defaultSiloTile))
        {
            ModEntry.StaticLogger.Error("Couldn't find or parse the default silo tile. The default silo will be missing. Please report this error.");
            return;
        }

        if (!Game1.buildingData.ContainsKey("DH-EJF_JojaSilo"))
        {
            ModEntry.StaticLogger.Error("It looks like the starter Joja silo didn't load correctly. Please report this!");
            return;
        }

        Building defaultSilo = new Building("DH-EJF_JojaSilo", defaultSiloTile);
        defaultSilo.FinishConstruction(true);
        farm.buildings.Add(defaultSilo);
    }
}
