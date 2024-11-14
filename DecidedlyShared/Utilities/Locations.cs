using System;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace DecidedlyShared.Utilities
{
    public class Locations
    {
        public static bool IsTileEmpty(GameLocation location, Vector2 tile)
        {
            return !(location.Objects.ContainsKey(tile) || location.terrainFeatures.ContainsKey(tile));
        }

        public static void ForEachGiantCrop(Action<GiantCrop> action)
        {
            Func<GameLocation, bool> searchAction = location =>
            {
                foreach (TerrainFeature tf in location.resourceClumps)
                {
                    if (tf is GiantCrop crop)
                        action.Invoke(crop);
                }

                return true;
            };

            Utility.ForEachLocation(searchAction);
        }
    }
}
