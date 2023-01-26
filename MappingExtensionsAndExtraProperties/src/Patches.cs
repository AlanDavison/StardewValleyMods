using System;
using System.Collections.Generic;
using System.Diagnostics;
using MappingExtensionsAndExtraProperties.Models.TileProperties;
using MappingExtensionsAndExtraProperties.Utils;
using DecidedlyShared.Logging;
using DecidedlyShared.Ui;
using DecidedlyShared.Utilities;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using xTile.Dimensions;
using xTile.ObjectModel;
using xTile.Tiles;

namespace MappingExtensionsAndExtraProperties;

public static class Patches
{
    private static Logger? logger = null;
    private static TilePropertyHandler? tileProperties = null;
    private static Properties propertyUtils;

    public static void InitialisePatches(Logger logger, TilePropertyHandler tileProperties)
    {
        Patches.logger = logger;
        Patches.tileProperties = tileProperties;
        Patches.propertyUtils = new Properties(logger);
    }

    // This is complete and utter nesting hell. TODO: Clean this up at some point. After release is fine.
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
                // We have our tile property. We need to check for the presence of an existing Action tile property.
                if (tileProperties.TryGetBuildingProperty(tileX, tileY, __instance, "Action",
                        out PropertyValue _))
                {
                    // We want to return so we don't conflict with opening a shop, going through a door, etc.

                    Patches.logger.Warn(
                        $"Found CloseupInteraction_Image tile property on {tileX}, {tileY} in {__instance.Name}. Interaction with it was blocked due to there being an Action tile property on the same tile.");
                    Patches.logger.Warn(
                        $"It's recommended that you contact the author of the mod that added the tile property to let them know.");
                    return;
                }

                // Next, we try to parse our tile property.
                if (!Parsers.TryParse(closeupInteractionProperty.ToString(),
                        out CloseupInteractionImage closeupInteractionParsed))
                {
                    // If the parsing failed, we want to nope out and log appropriately.

                    logger.Error(
                        $"Parsing tile property CloseupInteraction_Image on layer \"Back\" at {tileX}, {tileY} in {__instance.Name} failed. Is it formatted correctly?");
                    logger.Error($"Property value: {closeupInteractionProperty.ToString()}");
                    return;
                }
                else
                {
                    logger.Error($"Failed to parse property {closeupInteractionProperty.ToString()}");
                }

                // At this point, we have our correctly-parsed tile property, so we create our image container.
                VBoxElement vBox = new VBoxElement(
                    "Picture Box",
                    new Microsoft.Xna.Framework.Rectangle(
                        0,
                        0,
                        closeupInteractionParsed.SourceRect.Width * 2, closeupInteractionParsed.SourceRect.Height),
                    DrawableType.None,
                    Game1.menuTexture,
                    new Microsoft.Xna.Framework.Rectangle(0, 256, 60, 60),
                    Color.White,
                    16,
                    16,
                    16,
                    16);

                // And the image element itself.
                vBox.AddChild(new UiElement(
                    "Picture",
                    new Microsoft.Xna.Framework.Rectangle(0, 0, closeupInteractionParsed.SourceRect.Width * 4,
                        closeupInteractionParsed.SourceRect.Height * 4),
                    DrawableType.Texture,
                    closeupInteractionParsed.Texture,
                    closeupInteractionParsed.SourceRect,
                    Color.White));

                // Next, we want to see if there's a text tile property to display.
                if (tileProperties.TryGetBackProperty(tileX, tileY, __instance, CloseupInteractionText.PropertyKey,
                        out PropertyValue closeupTextProperty))
                {
                    // There is, so we try to parse it.
                    if (Parsers.TryParse(closeupTextProperty.ToString(), out CloseupInteractionText parsedTextProperty))
                    {
                        // It parsed successfully, so we create a text element, and add it to our image container.
                        vBox.AddChild(new TextElement(
                            "Popup Text Box",
                            Microsoft.Xna.Framework.Rectangle.Empty,
                            600,
                            parsedTextProperty.Text));
                    }
                    else
                    {
                        logger.Error($"Failed to parse property {closeupTextProperty.ToString()}");
                    }
                }

                // Finally, we create our menu, and set it to be the current, active menu.
                MenuBase menu = new MenuBase(vBox, $"{CloseupInteractionImage.PropertyKey}");
                Game1.activeClickableMenu = menu;
                menu.MenuOpened();
            } // If there isn't a single interaction property, we want to look for the start of a reel.
            else if (Patches.propertyUtils.TryGetInteractionReel(tileX, tileY, __instance, CloseupInteractionImage.PropertyKey,
                         out List<MenuPage> pages))
            {
                // List<MenuPage> pages = new List<MenuPage>();
                //
                // foreach (PropertyValue value in properties)
                // {
                //     if (Parsers.TryParse(value.ToString(),
                //             out CloseupInteractionImage parsed))
                //     {
                //         MenuPage menuPage = new MenuPage();
                //         UiElement picture = new UiElement(
                //             "Picture",
                //             new Microsoft.Xna.Framework.Rectangle(0, 0, parsed.SourceRect.Width * 4,
                //                 parsed.SourceRect.Height * 4),
                //             DrawableType.Texture,
                //             parsed.Texture,
                //             parsed.SourceRect,
                //             Color.White);
                //
                //         menuPage.page = picture;
                //
                //         pages.Add(menuPage);
                //     }
                // }

                PaginatedMenu pagedMenu = new PaginatedMenu(
                    "Interaction Reel",
                    pages,
                    Utility.xTileToMicrosoftRectangle(Game1.uiViewport),
                    DrawableType.None);

                // Finally, we create our menu, and set it to be the current, active menu.
                MenuBase menu = new MenuBase(pagedMenu, $"Reel");
                Game1.activeClickableMenu = menu;
                menu.MenuOpened();
            }

            // Check for the DHSetMailFlag property on a given tile.
            if (tileProperties.TryGetBackProperty(tileX, tileY, __instance, SetMailFlag.PropertyKey,
                    out PropertyValue dhSetMailFlagProperty))
            {Game1.isInspectionAtCurrentCursorTile = true;
                // It exists, so parse it.
                if (Parsers.TryParse(dhSetMailFlagProperty.ToString(), out SetMailFlag parsedProperty))
                {
                    // We've parsed it, so we try setting the mail flag appropriately.
                    Player.TryAddMailFlag(parsedProperty.MailFlag, Game1.player);
                }
                else
                {
                    logger.Error($"Failed to parse property {dhSetMailFlagProperty.ToString()}");
                }
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

    public static void Game1_drawMouseCursor_Prefix(Game1 __instance)
    {
        int xTile = (int)Game1.currentCursorTile.X;
        int yTile = (int)Game1.currentCursorTile.Y;

        if (tileProperties.TryGetBackProperty(xTile, yTile, Game1.currentLocation, SetMailFlag.PropertyKey,
                out PropertyValue _) ||
            tileProperties.TryGetBackProperty(xTile, yTile, Game1.currentLocation, CloseupInteractionImage.PropertyKey,
                out PropertyValue _) ||
            tileProperties.TryGetBackProperty(xTile, yTile, Game1.currentLocation, $"{CloseupInteractionImage.PropertyKey}_1",
                out PropertyValue _))
        {
            Game1.mouseCursor = 5;
        }
    }
}
