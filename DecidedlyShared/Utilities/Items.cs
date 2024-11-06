using System;
using System.Linq;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace DecidedlyShared.Utilities
{
    public class Items
    {
        public static bool TryGetItemFromFlooring(Flooring floor, out Item floorItem)
        {
            floorItem = null;

            string? floorType = floor.whichFloor.Value;

            if (floorType is null)
                return false;

            if (!Game1.floorPathData.ContainsKey(floorType))
                return false;

            string? itemId = Game1.floorPathData[floorType].ItemId;

            if (itemId is null)
                return false;

            if (! ItemRegistry.Exists(itemId))
                return false;

            floorItem = ItemRegistry.Create(itemId, allowNull: true);

            if (floorItem is null)
                return false;

            return true;
        }

        public static bool TryGetFlooringFromItemId(string id, out Flooring floor)
        {
            floor = null;
            string? itemId;

            string? type = Game1.floorPathData.FirstOrDefault(f => f.Value.ItemId.Equals(id)).Value.Id;

            try
            {
                floor = new Flooring(type);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }

            return false;
        }
    }
}
