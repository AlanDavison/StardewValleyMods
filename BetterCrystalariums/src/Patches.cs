using System;
using DecidedlyShared.Logging;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;

namespace BetterCrystalariums
{
    public class Patches
    {
        private static IMonitor monitor;
        private static IModHelper helper;
        private static Logger logger;
        private static ModConfig config;

        public static void Initialise(IMonitor m, IModHelper h, Logger l, ModConfig c)
        {
            monitor = m;
            helper = h;
            logger = l;
            config = c;
        }

        public static bool ObjectDropIn_Prefix(Object __instance, Item dropInItem, bool probe, Farmer who)
        {
            if (config.DebugMode)
            {
                // We're debugging, so we want to spit out as much information as possible.
                Item objectInMachine = __instance.heldObject;

                if (objectInMachine != null)
                {
                    logger.Log("Debug output:\tVariable\t\t\t\tDetails");
                    logger.Log($"\t\tFarmer.Name: \t\t\t\t{who.Name}");
                    logger.Log($"\t\t__instance.Name \t\t\t{__instance.Name}");
                    logger.Log($"\t\tdropInItem.Name \t\t\t{dropInItem.Name}");
                    logger.Log($"\t\tdropInItem.Category \t\t\t{dropInItem.Category}");

                    logger.Log($"\t\tName of object in machine \t\t{objectInMachine.Name}");
                    logger.Log($"\t\tCategory of object in machine \t\t{objectInMachine.Category}");
                    logger.Log($"{Environment.NewLine}");
                }
            }

            // Firstly, if the object isn't a crystalarium, we do nothing.
            if (!__instance.Name.Equals("Crystalarium"))
                return true;

            // Secondly, if the item the player is holding isn't a mineral, we don't want to do anything.
            if (dropInItem.Category != -2 && dropInItem.Category != -12)
                return true;

            // At this point, we know the player is holding a crystalarium-able item, and is interacting with a crystalarium.

            // We get the object held in the crystalarium, cast to an Item.
            Item heldObject = __instance.heldObject;

            if (heldObject != null)
                // Then, if the object in the crystalarium doesn't match what the playe's holding, we display our warning, and stop the replacement.
                if (!heldObject.Name.Equals(dropInItem.Name))
                {
                    Game1.showRedMessage($"{helper.Translation.Get("bettercrystalariums.wrong-mineral")}");
                    return false;
                }

            return true;
        }
    }
}
