using System;
using MappingExtensionsAndExtraProperties.Models.TileProperties;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace MappingExtensionsAndExtraProperties.Utils;

public class Parsers
{
    public static bool TryParse(string property, out DhSetMailFlag parsedProperty)
    {
        // There isn't really anything to parse here.
        parsedProperty = new DhSetMailFlag(property);

        return true;
    }

    public static bool TryParse(string property, out CloseupInteractionText parsedProperty)
    {
        // Not much to parse here, so we just set and return.
        parsedProperty = new CloseupInteractionText();
        parsedProperty.Text = property;

        return true;
    }

    public static bool TryParse(string property, out CloseupInteractionImage parsedProperty)
    {
        parsedProperty = new CloseupInteractionImage();
        string[] splitProperty = property.Split(" ");

        /*
        For this property, we expect:
        1) One parameter (the texture asset name), or
        2) Five parameters (the texture asset name, plus an x, y, width, and height)
        */

        if (splitProperty.Length < 1 || (splitProperty.Length > 1 && splitProperty.Length < 5))
            return false;

        // First, we want to validate the asset points to a valid texture.
        try
        {
            parsedProperty.Texture = Game1.content.Load<Texture2D>(splitProperty[0]);
        }
        catch (Exception e)
        {
            return false;
        }

        // We've gotten this far, so we check to see if we only have the one argument.
        if (splitProperty.Length == 1)
        {
            // We're only worrying about texture asset, so we set our sourceRect.
            parsedProperty.SourceRect = parsedProperty.Texture.Bounds;

            return true;
        }

        // We know we're dealing with the full set of arguments.
        int rectX, rectY, rectWidth, rectHeight;

        if (!int.TryParse(splitProperty[1], out rectX)) return false;
        if (!int.TryParse(splitProperty[2], out rectY)) return false;
        if (!int.TryParse(splitProperty[3], out rectWidth)) return false;
        if (!int.TryParse(splitProperty[4], out rectHeight)) return false;

        // All of our ints were parsed successfully, so we assign them to a brand new sourceRect.
        parsedProperty.SourceRect = new Rectangle(rectX, rectY, rectWidth, rectHeight);

        return true;
    }
}
