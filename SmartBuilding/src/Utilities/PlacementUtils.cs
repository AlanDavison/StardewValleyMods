using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;

namespace SmartBuilding.Utilities
{
    public class PlacementUtils
    {
        private ModConfig config;
        
        public PlacementUtils(ModConfig c)
        {
            config = c;
        }

        public bool HasAdjacentNonWaterTile(Vector2 v)
        {
            // Right now, this is only applicable for crab pots.
            if (config.CrabPotsInAnyWaterTile)
                return true;

            // We create our list of cardinal and ordinal directions.
            List<Vector2> directions = new List<Vector2>()
            {
                v + new Vector2(-1, 0), // Left
                v + new Vector2(1, 0), // Right
                v + new Vector2(0, -1), // Up
                v + new Vector2(0, 1), // Down
                v + new Vector2(-1, -1), // Up left
                v + new Vector2(1, -1), // Up right
                v + new Vector2(-1, 1), // Down left
                v + new Vector2(1, 1) // Down right
            };

            // Then loop through in each of those directions relative to the passed in tile to determine if a water tile is adjacent.
            foreach (Vector2 vector in directions)
            {
                if (!Game1.currentLocation.isWaterTile((int)vector.X, (int)vector.Y))
                    return true;
            }

            return false;
        }
    }
}