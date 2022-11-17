using DecidedlyShared.Logging;
using DecidedlyShared.Ui;
using HarmonyLib;
using MappingExtensionsAndExtraProperties.Models.TileProperties;
using StardewModdingAPI;
using StardewValley;

namespace MappingExtensionsAndExtraProperties;

public class ModEntry : Mod
{
    private Logger logger;

    public override void Entry(IModHelper helper)
    {
        var harmony = new Harmony(this.ModManifest.UniqueID);
        this.logger = new Logger(this.Monitor);
        Patches.InitialisePatches(this.logger);

        // We need to ensure we can kill relevant UIs if the player is warping.
        helper.Events.Player.Warped += (sender, args) =>
        {
            if (Game1.activeClickableMenu is MenuBase menu)
            {
                // If it's one of our menus, we close it.
                if (menu.MenuName.Equals(CloseupInteractionImage.TileProperty))
                    Game1.exitActiveMenu();
            }
        };

        // Our patch for handling interactions.
        harmony.Patch(
            AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction)),
            postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.GameLocation_CheckAction_Postfix)));
    }
}
