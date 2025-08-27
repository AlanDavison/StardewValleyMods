using System;
using DecidedlyShared.Logging;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.TerrainFeatures;
using xTile.Tiles;
using Utility = ExJojaFarm.Utilities.Utility;

namespace ExJojaFarm;

public class ModEntry : Mod
{
    public const string FARM_ID = "DH.ExJojaFarm.ExJojaFarmContent_Farm";
    private Logger logger;
    public static Logger StaticLogger;
    private static IModHelper Helper;

    public override void Entry(IModHelper helper)
    {
        this.logger = new Logger(this.Monitor);
        ModEntry.Helper = helper;
        ModEntry.StaticLogger = this.logger;
        Harmony harmony = new Harmony(this.ModManifest.UniqueID);
        helper.Events.Input.ButtonPressed += this.InputOnButtonPressed;

        harmony.Patch(
            original: AccessTools.Method(typeof(Farm), nameof(Farm.onNewGame)),
            postfix: new HarmonyMethod(typeof(ModEntry),
                nameof(ModEntry.Farm_OnNewGame_Postfix)));

        harmony.Patch(
            original: AccessTools.Method(typeof(Farm), nameof(Farm.DayUpdate)),
            postfix: new HarmonyMethod(typeof(ModEntry),
                nameof(ModEntry.Farm_DayUpdate_Postfix)));

        harmony.Patch(
            original: AccessTools.Method(typeof(Tree), nameof(Tree.dayUpdate)),
            postfix: new HarmonyMethod(typeof(ModEntry),
                nameof(ModEntry.Tree_DayUpdate_Postfix)));
    }

    private void InputOnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (e.Button == SButton.F7)
        {
            GameLocation location = new GameLocation("Maps/Farm", "Joja Dungeon: Floor 3");
            location.uniqueName.Value = "Maps/DH/ExJojaFarm/Floor3";
            location.loadMap("Maps/Farm");
            // Game1.locations.Add(location);
            LocationRequest request = new LocationRequest("Maps/DH/ExJojaFarm/Floor3", false, location);
            Game1.warpFarmer(request, 10, 10, 10);
        }
    }

    public static void Farm_DayUpdate_Postfix(Farm __instance, int dayOfMonth)
    {
        if (!Utility.IsExJojaFarm())
            return;

        int mapWidth = __instance.Map.GetLayer("Paths").Tiles.Array.GetLength(0);
        int mapHeight = __instance.Map.GetLayer("Paths").Tiles.Array.GetLength(1);

        if (mapWidth == 0 || mapHeight == 0)
            return;

        StaticLogger.Log("Doing DayUpdate for farm.", LogLevel.Info);

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Tile tile;

                try
                {
                    tile = __instance.Map.GetLayer("Paths").Tiles.Array[x, y];
                }
                catch (Exception e)
                {
                    StaticLogger.Warn($"Caught exception getting tile at {x},{y} on Paths layer.");

                    continue;
                }

                StaticLogger.Log("Doing safety check before accessing tile properties.", LogLevel.Info);

                if (tile is null)
                    continue;

                if (tile.Properties is null || !tile.Properties.ContainsKey("DH_Regrow_Log"))
                    continue;

                StaticLogger.Log("Trying to spawn log!", LogLevel.Info);
                Vector2 logTile = new Vector2(x, y);

                if (__instance.CanItemBePlacedHere(logTile) &&
                    __instance.CanItemBePlacedHere(logTile + new Vector2(1, 0)) &&
                    __instance.CanItemBePlacedHere(logTile + new Vector2(1, 1)) &&
                    __instance.CanItemBePlacedHere(logTile + new Vector2(0, 1)))
                {
                    __instance.resourceClumps.Add(new ResourceClump(602, 2, 2, logTile));
                }
            }
        }
    }

    public static void Tree_DayUpdate_Postfix(Tree __instance)
    {
        if (__instance.Location is not Farm)
            return;

        if (!Utility.IsExJojaFarm())
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
