using System;
using DecidedlyShared.APIs;
using DecidedlyShared.Logging;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace ZoomPerMap;

public class ModEntry : Mod
{
    private Logger logger;
    private static Logger StaticLogger;
    private ModConfig config;
    private static ModConfig StaticConfig;

    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        this.config = helper.ReadConfig<ModConfig>();
        StaticConfig = this.config;
        this.logger = new Logger(this.Monitor);
        helper.Events.Display.MenuChanged += this.DisplayOnMenuChanged;
        helper.Events.Player.Warped += this.PlayerOnWarped;
        helper.Events.GameLoop.GameLaunched += this.GameLoopOnGameLaunched;
        helper.Events.GameLoop.SaveLoaded += this.GameLoopOnSaveLoaded;

        Harmony harmony = new Harmony(this.ModManifest.UniqueID);
        harmony.Patch(
            AccessTools.Method(typeof(Event), nameof(Event.exitEvent)),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.ExitEvent_Postfix)));
    }

    private void GameLoopOnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        Game1.options.desiredBaseZoomLevel = GetZoomForLocation(Game1.currentLocation);
    }

    private void GameLoopOnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        IGenericModConfigMenuApi? gmcm;

        if (this.Helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu"))
        {
            gmcm = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

            if (gmcm is null)
                return;

            gmcm.Register(
                this.ModManifest,
                () => this.config = new ModConfig(),
                () => this.Helper.WriteConfig(this.config));

            gmcm.AddNumberOption(
                this.ModManifest,
                () => this.config.defaultOutdoorZoomLevel,
                (val) => this.config.defaultOutdoorZoomLevel = val,
                () => I18n.Settings_DefaultOutdoorZoom());

            gmcm.AddNumberOption(
                this.ModManifest,
                () => this.config.defaultIndoorZoomLevel,
                (val) => this.config.defaultIndoorZoomLevel = val,
                () => I18n.Settings_DefaultIndoorZoom());

            gmcm.AddNumberOption(
                this.ModManifest,
                () => this.config.defaultMineZoomLevel,
                (val) => this.config.defaultMineZoomLevel = val,
                () => I18n.Settings_DefaultMineZoom());
        }
    }

    public static void ExitEvent_Postfix(Event __instance)
    {
        try
        {
            Game1.options.desiredBaseZoomLevel = GetZoomForLocation(Game1.player.currentLocation);
        }
        catch (Exception e)
        {
            StaticLogger.Error("Caught an exception in Event.exitMethod postfix.");

            StaticLogger.Exception(e);
        }
    }

    private void PlayerOnWarped(object? sender, WarpedEventArgs e)
    {
        if (Game1.eventUp)
        {
            Game1.options.desiredBaseZoomLevel = StaticConfig.eventZoomLevel;
            return;
        }

        Game1.options.desiredBaseZoomLevel = GetZoomForLocation(e.NewLocation);
    }

    private void DisplayOnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.OldMenu is GameMenu)
        {
            bool zoomLevelsDirty = false;

            if (this.config.zoomLevels.TryGetValue(Game1.currentLocation.Name, out float zoomLevel))
            {
                if (zoomLevel != Game1.options.desiredBaseZoomLevel)
                {
                    zoomLevelsDirty = true;
                }
            }
            else
            {
                zoomLevelsDirty = true;
            }

            if (zoomLevelsDirty)
            {
                this.config.zoomLevels[Game1.currentLocation.Name] = Game1.options.zoomLevel;
                this.Helper.WriteConfig(this.config);
            }
        }
    }

    private static float GetZoomForLocation(GameLocation location)
    {
        if (StaticConfig.zoomLevels.TryGetValue(location.Name, out float zoomLevel))
        {
            return zoomLevel;
        }

        if (location is MineShaft || location is VolcanoDungeon)
            return StaticConfig.defaultMineZoomLevel;

        return location.IsOutdoors ? StaticConfig.defaultOutdoorZoomLevel : StaticConfig.defaultIndoorZoomLevel;
    }
}
