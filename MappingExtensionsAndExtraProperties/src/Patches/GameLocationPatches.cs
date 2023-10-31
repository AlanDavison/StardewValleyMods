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

public class GameLocationPatches
{
    private static Logger? logger = null;
    private static TilePropertyHandler? tileProperties = null;
    private static Properties propertyUtils;

    public static void InitialisePatches(Logger logger, TilePropertyHandler tileProperties)
    {
        GameLocationPatches.logger = logger;
        GameLocationPatches.tileProperties = tileProperties;
        propertyUtils = new Properties(logger);
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
            // There isn't a reel either, so we check for a letter property.
            else if (tileProperties.TryGetBackProperty(tileX, tileY, __instance, LetterText.PropertyKey,
                         out PropertyValue letterProperty))
            {
                Letter.DoLetter(__instance, letterProperty, tileX, tileY, logger);
            }

            // Check for the DHSetMailFlag property on a given tile.
            if (tileProperties.TryGetBackProperty(tileX, tileY, __instance, SetMailFlag.PropertyKey,
                    out PropertyValue dhSetMailFlagProperty))
            {
                MailFlag.DoMailFlag(dhSetMailFlagProperty, logger);
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
