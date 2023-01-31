using System;
using System.Collections.Generic;
using MappingExtensionsAndExtraProperties.Models.TileProperties;
using MappingExtensionsAndExtraProperties.Models.TileProperties.FakeNpc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;

namespace MappingExtensionsAndExtraProperties.Utils;

public class Parsers
{
    public static bool TryParse(string property, out SpawnPlaceableObject parsedProperty)
    {
        // Implementation of this property is on hold.
        parsedProperty = new SpawnPlaceableObject();

        // This is very simple for now. Just two arguments The BigCraftable ID, and a 1/0 depending on whether it's breakable or not.
        string[] splitProperty = property.Split(" ");

        // If we have fewer than one arguments, we immediately return false.
        if (splitProperty.Length < 1)
            return false;

        // If we have more than two, we return false.
        if (splitProperty.Length > 2)
            return false;

        // We know we have one or two arguments, so we can grab our BigCraftable instance first.
        if (!int.TryParse(splitProperty[0], out int bigCraftableId))
            return false;

        try
        {
            parsedProperty.bigCraftable = ObjectFactory.getItemFromDescription(1, bigCraftableId, 1);
        }
        catch (Exception e)
        {
            parsedProperty.bigCraftable = null;
            return false;
        }

        // If we have two arguments, we parse the breakable aspect.
        if (!bool.TryParse(splitProperty[1], out bool breakable))
            return false;

        // Now we apply this to our parsed property.
        parsedProperty.Breakable = breakable;

        return true;
    }

    public static bool TryParse(string property, out SetMailFlag parsedProperty)
    {
        // There isn't really anything to parse here.
        parsedProperty = new SetMailFlag(property);

        return true;
    }

    public static bool TryParse(string property, out CloseupInteractionText parsedProperty)
    {
        // Not much to parse here, so we just set and return.
        parsedProperty = new CloseupInteractionText();
        parsedProperty.Text = property;

        return true;
    }

    public static bool TryParse(string property, out LetterText parsedProperty)
    {
        parsedProperty = new LetterText();
        parsedProperty.Text = property;

        return true;
    }

    public static bool TryParse(string property, out LetterType parsedProperty)
    {
        parsedProperty = new LetterType();
        parsedProperty.Type = 1;

        if (int.TryParse(property.ToString(), out int type))
        {
            parsedProperty.Type = type;

            return true;
        }

        return false;
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

    public static bool TryParse(string property, out DhFakeNpc parsedProperty)
    {
        parsedProperty = new DhFakeNpc();
        parsedProperty.HasSpriteSizes = false;
        string[] splitProperty = property.Split(" ");
        int npcSpriteWidth;
        int npcSpriteHeight;

        /*
        For this property, we expect:
        1) One parameter (NPC name)
        2) Three parameters (NPC name, sprite width, sprite height)
        */
        // if (splitProperty.Length < 1 || (splitProperty.Length > 1 && splitProperty.Length != 3))
        if (splitProperty.Length != 1)
            return false;

        parsedProperty.NpcName = splitProperty[0];

        // Now we want to confirm that our first argument is an NPC name that is NOT taken.
        Dictionary<string, string> dispositions = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
        foreach (string npc in dispositions.Keys)
        {
            if (npc.Equals(parsedProperty.NpcName, StringComparison.OrdinalIgnoreCase))
                return false; // If there's an NPC in the dispositions matching our fake NPC, we bail.
        }

        // // We've gotten this far, so we check to see if we only have the one argument.
        // if (splitProperty.Length == 1)
        //     return true; // If we only have an NPC name argument, we can simply return true here.
        //
        // // We know we're dealing with the full set of arguments, so we try to parse them all.
        // if (!int.TryParse(splitProperty[1], out npcSpriteWidth)) return false;
        // if (!int.TryParse(splitProperty[2], out npcSpriteHeight)) return false;
        //
        // parsedProperty.SpriteWidth = npcSpriteHeight;
        // parsedProperty.SpriteHeight = npcSpriteHeight;
        // parsedProperty.HasSpriteSizes = true;

        return true;
    }

    // public static bool TryParse(string property, out )
}
