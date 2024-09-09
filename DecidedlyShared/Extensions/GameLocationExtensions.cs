using System.Collections.Generic;
using DecidedlyShared.Constants;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace DecidedlyShared.Extensions;

public static class GameLocationExtensions
{
    public static bool HasSObject(this GameLocation location, SObject obj, SearchType search)
    {
        return false;
    }

    public static bool HasTerrainFeature(this GameLocation location, TerrainFeature feature, SearchType search)
    {
        return false;
    }

    public static bool IsTileOccupied(this GameLocation location, Vector2 tile)
    {
        return false;
    }

    public static bool TryGetSObject(this GameLocation location, Vector2 tile, out SObject foundObject)
    {
        foundObject = null;

        if (location.Objects.ContainsKey(tile))
        {
            foundObject = location.Objects[tile];
            return true;
        }

        return false;
    }

    public static bool TryGetSObjectsInRadius(this GameLocation location, Vector2 tile, out List<SObject> foundObjects)
    {
        foundObjects = new List<SObject>();

        if (location.TryGetSObject(tile, out SObject? o))
        {
            foundObjects.Add(o);

            var directions = new List<Vector2>
            {
                tile + new Vector2(-1, 0), // Left
                tile + new Vector2(1, 0), // Right
                tile + new Vector2(0, -1), // Up
                tile + new Vector2(0, 1), // Down
                tile + new Vector2(-1, -1), // Up left
                tile + new Vector2(1, -1), // Up right
                tile + new Vector2(-1, 1), // Down left
                tile + new Vector2(1, 1) // Down right
            };

            foreach (var direction in directions)
            {
                if (location.TryGetSObject(direction, out SObject? adjacentObject))
                {
                    foundObjects.Add(adjacentObject);
                }
            }
        }

        return foundObjects.Count > 0;
    }

    public static SObject GetSObject(this GameLocation location, Vector2 tile)
    {
        return location.Objects[tile];
    }

    public static bool TryGetTerrainFeature(this GameLocation location, Vector2 tile, out TerrainFeature? foundFeature)
    {
        foundFeature = null;

        if (location.terrainFeatures.ContainsKey(tile))
        {
            foundFeature = location.terrainFeatures[tile];
            return true;
        }

        return false;
    }

    public static TerrainFeature GetTerrainFeature(this GameLocation location, Vector2 tile)
    {
        return location.terrainFeatures[tile];
    }
}
