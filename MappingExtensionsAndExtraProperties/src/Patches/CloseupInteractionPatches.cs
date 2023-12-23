using System;
using System.Collections.Generic;
using System.Diagnostics;
using DecidedlyShared.Logging;
using DecidedlyShared.Ui;
using DecidedlyShared.Utilities;
using MappingExtensionsAndExtraProperties.Functionality;
using MappingExtensionsAndExtraProperties.Models.TileProperties;
using MappingExtensionsAndExtraProperties.Utils;
using StardewModdingAPI;
using StardewValley;
using xTile.Dimensions;
using xTile.ObjectModel;

namespace MappingExtensionsAndExtraProperties.Patches;

public class CloseupInteractionPatches : IFeaturePatch
{
    public string FeatureId { get; set; }
    public static Func<bool> GetEnabled { get; set; }
    public static Logger Logger { get; set; }
    public static IModHelper Helper { get; set; }
    private static TilePropertyHandler? TilePropertyHandler = null;
    private static Properties PropertyUtils;


    public CloseupInteractionPatches(Logger logger, IModHelper helper, Func<bool> getEnabled, TilePropertyHandler? tileProperties = null)
    {
        Logger = logger;
        Helper = helper;
        GetEnabled = getEnabled;
        TilePropertyHandler = tileProperties;
        PropertyUtils = new Properties(Logger);
    }

    public static void GameLocation_CheckAction_Postfix(GameLocation __instance, Location tileLocation,
        xTile.Dimensions.Rectangle viewport, Farmer who)
    {
        // If the feature isn't enabled, get out immediately.
        if (!GetEnabled.Invoke())
            return;

#if DEBUG
        Stopwatch timer = new Stopwatch();
        timer.Start();
#endif
        // Consider removing this try/catch.
        try
        {
            // First, pull our tile co-ordinates from the location.
            int tileX = tileLocation.X;
            int tileY = tileLocation.Y;

            // Check for a CloseupInteraction property on the given tile.
            if (TilePropertyHandler.TryGetBackProperty(tileX, tileY, __instance, CloseupInteractionImage.PropertyKey,
                    out PropertyValue closeupInteractionProperty))
            {
                CloseupInteraction.DoCloseupInteraction(__instance, tileX, tileY, closeupInteractionProperty, Logger);
            }
            // If there isn't a single interaction property, we want to look for the start of a reel.
            else if (PropertyUtils.TryGetInteractionReel(tileX, tileY, __instance,
                         CloseupInteractionImage.PropertyKey,
                         out List<MenuPage> pages))
            {
                string cueName = "bigSelect";

                // Now we check for a sound interaction property.
                if (TilePropertyHandler.TryGetBackProperty(tileX, tileY, __instance, CloseupInteractionSound.PropertyKey,
                        out PropertyValue closeupSoundProperty))
                {
                    if (Parsers.TryParse(closeupSoundProperty.ToString(),
                            out CloseupInteractionSound parsedSoundProperty))
                    {
                        cueName = parsedSoundProperty.CueName;
                    }
                }

                CloseupInteraction.DoCloseupReel(pages, Logger, cueName);
            }
        }
        catch (Exception e)
        {
            Logger.Error("Caught exception handling GameLocation.checkAction in a postfix. Details follow:");
            Logger.Exception(e);
        }
#if DEBUG
        timer.Stop();

        Logger.Log($"Took {timer.ElapsedMilliseconds} to process in CheckAction patch.", LogLevel.Info);
#endif
    }
}
