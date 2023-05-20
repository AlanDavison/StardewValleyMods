using DecidedlyShared.Logging;
using DecidedlyShared.Utilities;
using MappingExtensionsAndExtraProperties.Models.TileProperties;
using MappingExtensionsAndExtraProperties.Utils;
using StardewValley;
using xTile.ObjectModel;

namespace MappingExtensionsAndExtraProperties.Patches;

public class Game1Patches
{
    private static Logger? logger = null;
    private static TilePropertyHandler? tileProperties = null;
    private static Properties propertyUtils;

    public static void InitialisePatches(Logger logger, TilePropertyHandler tileProperties)
    {
        Game1Patches.logger = logger;
        Game1Patches.tileProperties = tileProperties;
        propertyUtils = new Properties(logger);
    }

    public static void Game1_drawMouseCursor_Prefix(Game1 __instance)
    {
        int xTile = (int)Game1.currentCursorTile.X;
        int yTile = (int)Game1.currentCursorTile.Y;

        if (tileProperties.TryGetBackProperty(xTile, yTile, Game1.currentLocation, SetMailFlag.PropertyKey,
                out PropertyValue _) ||
            tileProperties.TryGetBackProperty(xTile, yTile, Game1.currentLocation, CloseupInteractionImage.PropertyKey,
                out PropertyValue _) ||
            tileProperties.TryGetBackProperty(xTile, yTile, Game1.currentLocation,
                $"{CloseupInteractionImage.PropertyKey}_1",
                out PropertyValue _) ||
            tileProperties.TryGetBackProperty(xTile, yTile, Game1.currentLocation, LetterType.PropertyKey,
                out PropertyValue _) ||
            tileProperties.TryGetBackProperty(xTile, yTile, Game1.currentLocation, LetterText.PropertyKey,
                out PropertyValue _) ||
            tileProperties.TryGetBackProperty(xTile, yTile, Game1.currentLocation, SetMailFlag.PropertyKey,
                out PropertyValue _))
        {
            Game1.mouseCursor = 5;
        }
    }
}
