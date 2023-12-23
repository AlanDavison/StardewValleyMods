using System;
using System.Collections.Generic;
using System.Diagnostics;
using DecidedlyShared.Logging;
using DecidedlyShared.Ui;
using DecidedlyShared.Utilities;
using HarmonyLib;
using MappingExtensionsAndExtraProperties.Functionality;
using MappingExtensionsAndExtraProperties.Models.TileProperties;
using MappingExtensionsAndExtraProperties.Utils;
using StardewModdingAPI;
using StardewValley;
using xTile.Dimensions;
using xTile.ObjectModel;

namespace MappingExtensionsAndExtraProperties.Features;

public class CloseupInteractionFeature : Feature
{
    public sealed override Harmony HarmonyPatcher { get; init; }
    public sealed override bool Enabled { get; internal set; }
    public sealed override string FeatureId { get; init; }
    private static TilePropertyHandler tileProperties;
    private static Properties propertyUtils;
    private static Logger logger;

    public CloseupInteractionFeature(Harmony harmony, string id, Logger logger, TilePropertyHandler tilePropertyHandler, Properties propertyUtils)
    {
        this.Enabled = false;
        this.HarmonyPatcher = harmony;
        this.FeatureId = id;
        CloseupInteractionFeature.logger = logger;
        CloseupInteractionFeature.tileProperties = tilePropertyHandler;
        CloseupInteractionFeature.propertyUtils = propertyUtils;
    }

    public override bool Enable()
    {
        try
        {
            this.HarmonyPatcher.Patch(
                AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction)),
                postfix: new HarmonyMethod(typeof(CloseupInteractionFeature),
                    nameof(CloseupInteractionFeature.GameLocation_CheckAction_Postfix)));
        }
        catch (Exception e)
        {
            logger.Exception(e);
        }

        this.Enabled = true;
        return true;
    }

    public override void Disable()
    {
        throw new NotImplementedException();
    }

    public override int GetHashCode()
    {
        return this.FeatureId.GetHashCode();
    }

    public static void GameLocation_CheckAction_Postfix(GameLocation __instance, Location tileLocation,
        xTile.Dimensions.Rectangle viewport, Farmer who)
    {
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
            if (tileProperties.TryGetBackProperty(tileX, tileY, __instance, CloseupInteractionImage.PropertyKey,
                    out PropertyValue closeupInteractionProperty))
            {
                CloseupInteraction.DoCloseupInteraction(__instance, tileX, tileY, closeupInteractionProperty, logger);
            }
            // If there isn't a single interaction property, we want to look for the start of a reel.
            else if (propertyUtils.TryGetInteractionReel(tileX, tileY, __instance,
                         CloseupInteractionImage.PropertyKey,
                         out List<MenuPage> pages))
            {
                string cueName = "bigSelect";

                // Now we check for a sound interaction property.
                if (tileProperties.TryGetBackProperty(tileX, tileY, __instance, CloseupInteractionSound.PropertyKey,
                        out PropertyValue closeupSoundProperty))
                {
                    if (Parsers.TryParse(closeupSoundProperty.ToString(),
                            out CloseupInteractionSound parsedSoundProperty))
                    {
                        cueName = parsedSoundProperty.CueName;
                    }
                }

                CloseupInteraction.DoCloseupReel(pages, logger, cueName);
            }
        }
        catch (Exception e)
        {
            logger.Error("Caught exception handling GameLocation.checkAction in a postfix. Details follow:");
            logger.Exception(e);
        }
#if DEBUG
        timer.Stop();

        logger.Log($"Took {timer.ElapsedMilliseconds} to process in CheckAction patch.", LogLevel.Info);
#endif
    }
}
