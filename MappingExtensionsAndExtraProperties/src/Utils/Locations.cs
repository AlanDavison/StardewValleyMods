using System;
using System.Collections.Generic;
using DecidedlyShared.Logging;
using StardewModdingAPI;
using StardewValley;

namespace MappingExtensionsAndExtraProperties.Utils;

public class Locations
{
    public static void RemoveFakeNpcs(GameLocation location, Logger logger)
    {
        List<Tuple<GameLocation, NPC>> charactersToRemove = new();

        foreach (NPC character in location.characters)
        {
            if (character is FakeNpc)
                charactersToRemove.Add(Tuple.Create(location, character));
        }

        foreach (var character in charactersToRemove)
        {
            character.Item1.characters.Remove(character.Item2);
            logger.Log($"Fake NPC {character.Item2.Name} removed from {character.Item1.Name}.", LogLevel.Trace);
        }
    }

    public static void PlaceFakeNpcs(GameLocation location)
    {

    }
}
