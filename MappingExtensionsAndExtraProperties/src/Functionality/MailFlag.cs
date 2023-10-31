using DecidedlyShared.Logging;
using DecidedlyShared.Utilities;
using MappingExtensionsAndExtraProperties.Models.TileProperties;
using MappingExtensionsAndExtraProperties.Utils;
using StardewValley;
using xTile.ObjectModel;

namespace MappingExtensionsAndExtraProperties.Functionality;

public class MailFlag
{
    public static void DoMailFlag(PropertyValue dhSetMailFlagProperty, Logger logger)
    {
        // It exists, so parse it.
        if (Parsers.TryParse(dhSetMailFlagProperty.ToString(), out SetMailFlag parsedProperty))
        {
            // We've parsed it, so we try setting the mail flag appropriately.
            Player.TryAddMailFlag(parsedProperty.MailFlag, Game1.player);
        }
        else
        {
            logger.Error($"Failed to parse property {dhSetMailFlagProperty.ToString()}");
        }
    }
}
