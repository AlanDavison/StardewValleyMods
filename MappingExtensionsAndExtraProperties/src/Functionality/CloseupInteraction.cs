using System.Collections.Generic;
using DecidedlyShared.Logging;
using DecidedlyShared.Ui;
using DecidedlyShared.Utilities;
using MappingExtensionsAndExtraProperties.Models.TileProperties;
using MappingExtensionsAndExtraProperties.Utils;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using StardewValley;
using xTile.ObjectModel;

namespace MappingExtensionsAndExtraProperties.Functionality;

public class CloseupInteraction
{
    public static void DoCloseupReel(List<MenuPage> pages, Logger logger, string soundCue = "bigSelect")
    {
        TilePropertyHandler handler = new TilePropertyHandler(logger);

        PaginatedMenu pagedMenu = new PaginatedMenu(
            "Interaction Reel",
            pages,
            Geometry.RectToRect(Game1.uiViewport),
            logger,
            DrawableType.None,
            soundCue);

        // Finally, we create our menu, and set it to be the current, active menu.
        MenuBase menu = new MenuBase(pagedMenu, $"Reel", logger, soundCue);

        // And set our container's parent.
        pagedMenu.SetParent(menu);

        Game1.activeClickableMenu = menu;
        menu.MenuOpened();
    }

    public static void GetCloseupInteractionProperties()
    {

    }

    public static void DoCloseupInteraction(
        CloseupInteractionImage closeupInteraction,
        CloseupInteractionText? closeupInteractionText,
        CloseupInteractionSound? closeupInteractionSound,
        Logger logger)
    {
        // At this point, we have our correctly-parsed tile property, so we create our image container.
        VBoxElement vBox = new VBoxElement(
            "Picture Box",
            new Microsoft.Xna.Framework.Rectangle(
                0,
                0,
                closeupInteraction.SourceRect.Width * 2, closeupInteraction.SourceRect.Height),
            logger,
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
            new Microsoft.Xna.Framework.Rectangle(0, 0, closeupInteraction.SourceRect.Width * 4,
                closeupInteraction.SourceRect.Height * 4),
            logger,
            DrawableType.Texture,
            closeupInteraction.Texture,
            closeupInteraction.SourceRect,
            Color.White));

        if (closeupInteractionText.HasValue)
        {
            // It parsed successfully, so we create a text element, and add it to our image container.
            vBox.AddChild(new TextElement(
                "Popup Text Box",
                Microsoft.Xna.Framework.Rectangle.Empty,
                logger,
                600,
                closeupInteractionText.Value.Text));
        }

        // Finally, we create our menu.
        MenuBase menu = new MenuBase(vBox, $"{CloseupInteractionImage.PropertyKey}", logger);

        // And set our container's parent.
        vBox.SetParent(menu);

        if (closeupInteractionSound.HasValue)
        {
            logger.Log($"Setting sound cue: {closeupInteractionSound.Value.CueName}");
            menu.SetOpenSound(closeupInteractionSound.Value.CueName);
        }

        Game1.activeClickableMenu = menu;
        menu.MenuOpened();
    }

    public static void DoCloseupInteraction(GameLocation location, int tileX, int tileY,
        PropertyValue closeupInteractionProperty, Logger logger)
    {
        TilePropertyHandler handler = new TilePropertyHandler(logger);
        CloseupInteractionImage closeupInteractionParsed;
        CloseupInteractionText? textProperty = null;
        CloseupInteractionSound? soundProperty = null;

        // Next, we try to parse our tile property.
        if (!Parsers.TryParseIncludingKey(closeupInteractionProperty.ToString(),
                out closeupInteractionParsed))
        {
            // If the parsing failed, we want to nope out.

            return;
        }

        if (handler.TryGetBuildingProperty(tileX, tileY, location, CloseupInteractionText.PropertyKey,
                out PropertyValue closeupTextProperty))
        {
            if (Parsers.TryParse(closeupTextProperty.ToString(), out CloseupInteractionText parsedTextProperty))
            {
                textProperty = parsedTextProperty;
            }
        }

        // Now we check for a sound interaction property.
        if (handler.TryGetBuildingProperty(tileX, tileY, location, CloseupInteractionSound.PropertyKey,
                out PropertyValue closeupSoundProperty))
        {
            if (Parsers.TryParse(closeupSoundProperty.ToString(), out CloseupInteractionSound parsedSoundProperty))
            {
                soundProperty = parsedSoundProperty;
            }
        }

        DoCloseupInteraction(closeupInteractionParsed, textProperty, soundProperty, logger);
    }

    public static void CreateInteractionUi(CloseupInteractionImage closeupInteractionParsed, Logger logger,
        CloseupInteractionText? closeupInteractionText = null, CloseupInteractionSound? closeupInteractionSound = null)
    {
        string soundCue = "bigSelect";

        // At this point, we have our correctly-parsed tile property, so we create our image container.
        VBoxElement vBox = new VBoxElement(
            "Picture Box",
            new Microsoft.Xna.Framework.Rectangle(
                0,
                0,
                closeupInteractionParsed.SourceRect.Width * 2, closeupInteractionParsed.SourceRect.Height),
            logger,
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
            logger,
            DrawableType.Texture,
            closeupInteractionParsed.Texture,
            closeupInteractionParsed.SourceRect,
            Color.White));

        if (closeupInteractionText.HasValue)
        {
            // And our text element.
            vBox.AddChild(new TextElement(
                "Popup Text Box",
                Microsoft.Xna.Framework.Rectangle.Empty,
                logger,
                600,
                closeupInteractionText.Value.Text));
        }

        if (closeupInteractionSound.HasValue)
        {
            soundCue = closeupInteractionSound.Value.CueName;
        }

        // Finally, we create our menu, and set it to be the current, active menu.
        MenuBase menu = new MenuBase(vBox, $"{CloseupInteractionImage.PropertyKey}", logger, soundCue);

        // And set our container's parent.
        vBox.SetParent(menu);

        Game1.activeClickableMenu = menu;
        menu.MenuOpened();
    }
}
