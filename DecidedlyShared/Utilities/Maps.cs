using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.FlowAnalysis;
using StardewValley;

namespace DecidedlyShared.Utilities
{
    public class Maps
    {
        private static Dictionary<GameLocation, string> nameMap;
        private static List<GameLocation> maps;

        public static Dictionary<GameLocation, string> NameMap
        {
            get => nameMap;
            set
            {
                nameMap = value;
            }
        }

        public static void PopulateMapList()
        {
            maps = Game1.locations.ToList();
        }

        public static GameLocation GetLocation(string locationName)
        {
            IEnumerable<GameLocation> mapQuery = from map in maps where map.Name.Contains(locationName) select map;

            return mapQuery.First(map => map.Name.Contains(locationName));
        }

        public static List<GameLocation> GetLocations(string searchTerm)
        {
            return Game1.locations.Where(map => map.Name.Contains(searchTerm)).ToList();
        }
    }
}