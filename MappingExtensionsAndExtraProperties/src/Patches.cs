using System;
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

namespace MappingExtensionsAndExtraProperties;

public static class Patches
{
    private static Logger? logger = null;
    private static TilePropertyHandler? tileProperties = null;

    public static void InitialisePatches(Logger logger, TilePropertyHandler tileProperties)
    {
        Patches.logger = logger;
        Patches.tileProperties = tileProperties;
    }

    public static void GameLocation_CheckAction_Postfix(GameLocation __instance, Location tileLocation,
        xTile.Dimensions.Rectangle viewport, Farmer who)
    {
        // Consider removing this try/catch.
        try
        {
            // First, pull our tile co-ordinates from the Location.
            int tileX = tileLocation.X;
            int tileY = tileLocation.Y;

            // Check for a CloseupInteraction property on the given tile.
            if (tileProperties.TryGetBackProperty(tileX, tileY, __instance, CloseupInteractionImage.TileProperty,
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

                // At this point, we have our correctly-parsed tile property, so we create our image container.
                VBoxElement vBox = new VBoxElement(
                    "Picture Box",
                    new Microsoft.Xna.Framework.Rectangle(
                        0,
                        0,
                        closeupInteractionParsed.SourceRect.Width * 2, closeupInteractionParsed.SourceRect.Height),
                    false,
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
                    closeupInteractionParsed.Texture,
                    closeupInteractionParsed.SourceRect,
                    Color.White));

                // Next, we want to see if there's a text tile property to display.
                if (tileProperties.TryGetBackProperty(tileX, tileY, __instance, CloseupInteractionText.TileProperty,
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
                }

                // Finally, we create our menu, and set it to be the current, active menu.
                MenuBase menu = new MenuBase(vBox, $"{CloseupInteractionImage.TileProperty}");
                Game1.activeClickableMenu = menu;
                menu.MenuOpened();
            }

            // Check for the DHSetMailFlag property on a given tile.
            if (tileProperties.TryGetBackProperty(tileX, tileY, __instance, DhSetMailFlag.TileProperty,
                    out PropertyValue dhSetMailFlagProperty))
            {Game1.isInspectionAtCurrentCursorTile = true;
                // It exists, so parse it.
                if (Parsers.TryParse(dhSetMailFlagProperty.ToString(), out DhSetMailFlag parsedProperty))
                {
                    // We've parsed it, so we try setting the mail flag appropriately.
                    Player.TryAddMailFlag(parsedProperty.MailFlag, Game1.player);
                }
            }
        }
        catch (Exception e)
        {
            logger.Error("Caught exception handling GameLocation.checkAction in a postfix. Details follow:");
            logger.Exception(e);
        }
    }

    public static void Game1_drawMouseCursor_Prefix(Game1 __instance)
    {
        int xTile = (int)Game1.currentCursorTile.X;
        int yTile = (int)Game1.currentCursorTile.Y;

        if (tileProperties.TryGetBackProperty(xTile, yTile, Game1.currentLocation, DhSetMailFlag.TileProperty,
                out PropertyValue _) ||
            tileProperties.TryGetBackProperty(xTile, yTile, Game1.currentLocation, CloseupInteractionImage.TileProperty,
                out PropertyValue _)

           )
        {
            // Game1.isInspectionAtCurrentCursorTile = true;
            Game1.mouseCursor = 5;
        }
    }
}
