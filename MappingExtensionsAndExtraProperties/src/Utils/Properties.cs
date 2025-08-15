using System;
using System.Collections.Generic;
using System.Linq;
using DecidedlyShared.Logging;
using DecidedlyShared.Ui;
using DecidedlyShared.Utilities;
using MappingExtensionsAndExtraProperties.Models.TileProperties;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using xTile.ObjectModel;

namespace MappingExtensionsAndExtraProperties.Utils;

public class Properties
{
    private Logger logger;
    private TilePropertyHandler properties;

    public Properties(Logger logger)
    {
        this.logger = logger;
        this.properties = new TilePropertyHandler(this.logger);
    }

    public bool TryGetItemInteractionReel(string key, SObject obj, out List<MenuPage> pages)
    {
        pages = new List<MenuPage>();



        return true;
    }

    public bool TryGetInteractionReel(
        Func<List<string>> imagePropertiesProvider,
        Func<List<string>> textPropertiesProvider,
        out List<MenuPage> pages)
    {
        string[] imageProperties = imagePropertiesProvider.Invoke().ToArray();
        string[] textProperties = textPropertiesProvider.Invoke().ToArray();

        this.logger.Log($"imageProperties count: {imageProperties.Length}");
        this.logger.Log($"textProperties count: {textProperties.Length}");
        foreach (string textProperty in textProperties)
        {
            this.logger.Log(textProperty, LogLevel.Info);
        }
        pages = new List<MenuPage>();
        int propertyNumber = 1;

        foreach (string imageProperty in imageProperties)
        {
            if (Parsers.TryParseIncludingKey(imageProperty, out CloseupInteractionImage parsedImageProperty))
            {
                TextElement textElement = null;
                string currentTextPropertyKey = $"{CloseupInteractionText.PropertyKey}_{propertyNumber}";
                string? currentTextProperty = textProperties.FirstOrDefault(s => s.Contains($"{CloseupInteractionText.PropertyKey}_{propertyNumber.ToString()}"));

                if (currentTextProperty is not null)
                {
                    if (Parsers.TryParseIncludingKey(textProperties[propertyNumber - 1],
                            out CloseupInteractionText parsedTextProperty))
                    {
                        textElement = new TextElement(
                            "Popup Text Box",
                            Rectangle.Empty,
                            this.logger,
                            600,
                            parsedTextProperty.Text);
                    }
                    else
                    {
                        this.logger.Error($"Failed to parse property {currentTextPropertyKey}.");
                    }
                }

                MenuPage page = new MenuPage();
                UiElement picture = new UiElement(
                    "Picture",
                    new Rectangle(0, 0, parsedImageProperty.SourceRect.Width * 4,
                        parsedImageProperty.SourceRect.Height * 4),
                    this.logger,
                    DrawableType.Texture,
                    parsedImageProperty.Texture,
                    parsedImageProperty.SourceRect,
                    Color.White);

                page.page = picture;
                if (textElement != null) page.pageText = textElement;

                pages.Add(page);
            }

            propertyNumber++;
        }

        return true;
    }

    // This relies on external things, and is stinky. TODO: Combine this and the item reel property into one later.
    public bool TryGetInteractionReel(int x, int y, GameLocation location, string key,
        out List<MenuPage> pages)
    {
        pages = new List<MenuPage>();
        int propertyNumber = 1;

        // We're trying to find multiple properties, and we know our syntax for multiple is PropertyName_1, etc.
        while (this.properties.TryGetTileProperty(x, y, location, "Buildings", $"{key}_{propertyNumber}", out PropertyValue property))
        {
            if (Parsers.TryParse(property.ToString(),
                    out CloseupInteractionImage parsedImageProperty))
            {
                TextElement textElement = null;

                // We've successfully parsed an image reel element, so we want to check for a corresponding description.
                if (this.properties.TryGetBuildingProperty(x, y, location, $"{CloseupInteractionText.PropertyKey}_{propertyNumber}",
                        out PropertyValue closeupTextProperty))
                {
                    // We found a property, so we parse it.
                    if (Parsers.TryParse(closeupTextProperty.ToString(), out CloseupInteractionText parsedTextProperty))
                    {
                        textElement = new TextElement(
                            "Popup Text Box",
                            Microsoft.Xna.Framework.Rectangle.Empty,
                            this.logger,
                            600,
                            parsedTextProperty.Text);
                    }
                    else
                    {
                        this.logger.Error($"Failed to parse property {closeupTextProperty.ToString()}");
                    }
                }

                MenuPage menuPage = new MenuPage();
                UiElement picture = new UiElement(
                    "Picture",
                    new Microsoft.Xna.Framework.Rectangle(0, 0, parsedImageProperty.SourceRect.Width * 4,
                        parsedImageProperty.SourceRect.Height * 4),
                    this.logger,
                    DrawableType.Texture,
                    parsedImageProperty.Texture,
                    parsedImageProperty.SourceRect,
                    Color.White);

                menuPage.page = picture;
                menuPage.pageText = textElement;

                pages.Add(menuPage);
            }
            else
            {
                this.logger.Error($"Failed to parse property {property.ToString()}");
            }

            propertyNumber++;
        }

        return pages.Count > 0;
    }
}
