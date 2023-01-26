using System.Collections.Generic;
using DecidedlyShared.Logging;
using DecidedlyShared.Ui;
using DecidedlyShared.Utilities;
using MappingExtensionsAndExtraProperties.Models.TileProperties;
using Microsoft.Xna.Framework;
using StardewValley;
using xTile.Layers;
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

    public bool TryGetInteractionReel(int x, int y, GameLocation location, string key,
        out List<MenuPage> pages)
    {
        pages = new List<MenuPage>();
        int propertyNumber = 1;

        // We're trying to find multiple properties, and we know our syntax for multiple is PropertyName_1, etc.
        while (this.properties.TryGetTileProperty(x, y, location, "Back", $"{key}_{propertyNumber}", out PropertyValue property))
        {
            if (Parsers.TryParse(property.ToString(),
                    out CloseupInteractionImage parsed))
            {
                TextElement textElement = null;

                // We've successfully parsed an image reel element, so we want to check for a corresponding description.
                if (this.properties.TryGetBackProperty(x, y, location, $"{CloseupInteractionText.PropertyKey}_{propertyNumber}",
                        out PropertyValue closeupTextProperty))
                {
                    // We found a property, so we parse it.
                    if (Parsers.TryParse(closeupTextProperty.ToString(), out CloseupInteractionText parsedTextProperty))
                    {
                        textElement = new TextElement(
                            "Popup Text Box",
                            Microsoft.Xna.Framework.Rectangle.Empty,
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
                    new Microsoft.Xna.Framework.Rectangle(0, 0, parsed.SourceRect.Width * 4,
                        parsed.SourceRect.Height * 4),
                    DrawableType.Texture,
                    parsed.Texture,
                    parsed.SourceRect,
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
