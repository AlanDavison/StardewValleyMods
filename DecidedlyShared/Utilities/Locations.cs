using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace DecidedlyShared.Utilities
{
    public class Locations
    {
        public static bool IsTileEmpty(GameLocation location, Vector2 tile)
        {
            return !(location.Objects.ContainsKey(tile) || location.terrainFeatures.ContainsKey(tile));
        }
    }
}
