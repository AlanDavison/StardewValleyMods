using System.Collections.Generic;
using DecidedlyShared.Constants;
using Microsoft.Xna.Framework;
using StardewValley;

namespace DecidedlyShared.Utilities
{
    public class Locations
    {
        public static bool IsTileEmpty(GameLocation location, Vector2 tile)
        {
            return !(location.Objects.ContainsKey(tile) || location.terrainFeatures.ContainsKey(tile));
        }

        public static List<Vector2> GetSurroundingTileCoords(Vector2 tile)
        {
            List<Vector2> tiles = new List<Vector2>();

            foreach (Vector2 direction in Directions.All)
            {
                tiles.Add(tile + direction);
            }

            return tiles;
        }
    }
}
