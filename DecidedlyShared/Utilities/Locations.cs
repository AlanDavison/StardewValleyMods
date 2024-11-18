using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Internal;
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

        public static void ForEachPlantableInterior(Action<GameLocation> action)
        {
            Func<Building, bool> searchAction = building =>
            {
                if (!building.HasIndoors() || building.GetIndoors() is null)
                    return true;

                action.Invoke(building.GetIndoors());

                return true;
            };

            Utility.ForEachBuilding(searchAction);
        }
    }
}
