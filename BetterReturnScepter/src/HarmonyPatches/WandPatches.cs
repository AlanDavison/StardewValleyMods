using DecidedlyShared.Logging;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Tools;

namespace BetterReturnScepter.HarmonyPatches
{
    public class WandPatches
    {
        private static Logger logger;
        private static PerScreen<PreviousPoint> previousPoint;
        private static RodCooldown rodCooldown;

        /// <summary>
        /// </summary>
        /// <param name="logger">An instance of our main Logger class, used for trace logs of which location/tile was saved.</param>
        /// <param name="point">
        ///     The main instance of PreviousPoint the rest of the mod will warp the player to. Contains a
        ///     GameLocation and Vector2 tile location.
        /// </param>
        public WandPatches(Logger newLogger, PerScreen<PreviousPoint> point, RodCooldown cooldown)
        {
            logger = newLogger;
            previousPoint = point;
            rodCooldown = cooldown;
        }

        public static bool Wand_DoFunction_Prefix(Wand __instance, int x, int y, int power, Farmer who)
        {
            // If the player is in an instanceable structure (such as a shed, coop, barn, etc.), we want to warp to the front door.
            if (!who.currentLocation.isStructure.Value)
            {
                previousPoint.Value.Location = who.currentLocation.Name;
                previousPoint.Value.Tile = who.Tile;
            }
            else
            {
                Game1.showRedMessage("Can't warp back to this location. Scepter set to default location.");
                previousPoint.Value.Location = "Farm";
                previousPoint.Value.Tile = Utility.getHomeOfFarmer(who).getFrontDoorSpot().ToVector2();
            }

            // A warp has happened, so we reset our countdown.
            rodCooldown.ResetCountdown();

            logger.Log(
                $"[In {nameof(Wand_DoFunction_Prefix)}] Saved previous tile {previousPoint.Value.Tile} and location {previousPoint.Value.Location}",
                LogLevel.Trace);

            return true;
        }
    }
}
