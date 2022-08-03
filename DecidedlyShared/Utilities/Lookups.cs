using DecidedlyShared.Constants;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace DecidedlyShared.Utilities
{
    public class Lookups
    {
        public static BreakableType BreakableType(SObject obj)
        {
            if (obj.Name.Equals("Stone"))
                return Constants.BreakableType.Pickaxe;

            if (obj.Name.Equals("Twig"))
                return Constants.BreakableType.Axe;

            if (obj.Name.Equals("Artifact Spot"))
                return Constants.BreakableType.Hoe;

            return Constants.BreakableType.NotAllowed;
        }

        public static BreakableType BreakableType(TerrainFeature tf)
        {
            // For now, we're only dealing with trees, so we simply return appropriately.

            return Constants.BreakableType.Axe;
        }
    }
}