using DecidedlyShared.Logging;
using DecidedlyShared.Ui;
using DecidedlyShared.Utilities;
using HarmonyLib;
using MappingExtensionsAndExtraProperties.Models.TileProperties;
using StardewModdingAPI;
using StardewValley;
using xTile.ObjectModel;

namespace MappingExtensionsAndExtraProperties;

public class ModEntry : Mod
{
    private Logger logger;
    private TilePropertyHandler tileProperties;

    public override void Entry(IModHelper helper)
    {
        var harmony = new Harmony(this.ModManifest.UniqueID);
        this.logger = new Logger(this.Monitor);
        this.tileProperties = new TilePropertyHandler(this.logger);
        Patches.InitialisePatches(this.logger,  this.tileProperties);

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

        // Our cursor draw patch for interaction highlights.
        harmony.Patch(
            AccessTools.Method(typeof(Game1), nameof(Game1.drawMouseCursor)),
            prefix: new HarmonyMethod(typeof(Patches), nameof(Patches.Game1_drawMouseCursor_Prefix)));
    }
}
