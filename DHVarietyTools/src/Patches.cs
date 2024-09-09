using DecidedlyShared.Logging;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace DHVarietyTools;

public class Patches
{
    private static IMonitor monitor;
    private static IModHelper helper;
    private static Logger logger;

    public Patches(IMonitor monitor, IModHelper helper, Logger logger)
    {
        Patches.monitor = monitor;
        Patches.helper = helper;
        Patches.logger = logger;
    }

    public static void FenceFixer1_Postfix(Tool __instance, GameLocation location, int x, int y, int power, Farmer who)
    {
        if (__instance is null)
            return;
        if (__instance.ItemId is null)
            return;
        if (__instance.ItemId != "DecidedlyHuman_FenceFixer1_Tool.")
            return;

        Vector2 toolHitLocation = new Vector2(x / 64, y / 64);

        if (location.Objects.ContainsKey(toolHitLocation))
        {
            SObject o = location.Objects[toolHitLocation];
            if (o is Fence fence)
            {
                fence.health.Value = fence.maxHealth.Value;

            }
        }
    }

    public static void KaboomHammer1_Postfix(Tool __instance, GameLocation location, int x, int y, int power, Farmer who)
    {
        if (__instance is null)
            return;
        if (__instance.ItemId is null)
            return;
        if (__instance.ItemId != "DecidedlyHuman_KaboomHammer1_Tool")
            return;

        Vector2 toolHitLocation = new Vector2(x / 64, y / 64);

        if (location.Objects.ContainsKey(toolHitLocation))
        {
            bool hitAtLeastOneTile = false;
            location.TryGetSObjectsInRadius(toolHitLocation, out List<SObject> surroundingTiles);

            foreach (SObject o in surroundingTiles)
            {
                if (o.IsBreakableStone())
                {
                    hitAtLeastOneTile = true;
                    int tileX, tileY;
                    tileX = (int)o.TileLocation.X;
                    tileY = (int)o.TileLocation.Y;

                    location.OnStoneDestroyed(o.ItemId, tileX, tileY, who);
                    location.removeObject(o.TileLocation, false);
                    Game1.createRadialDebris(location, 4, tileX, tileY, 12, false);
                }
            }

            if (hitAtLeastOneTile)
                DecidedlyShared.Utilities.Sound.TryPlaySound("cowboy_explosion");
        }
    }
}
