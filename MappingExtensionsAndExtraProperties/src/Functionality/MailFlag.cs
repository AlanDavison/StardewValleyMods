using DecidedlyShared.Logging;
using DecidedlyShared.Utilities;
using MappingExtensionsAndExtraProperties.Models.TileProperties;
using MappingExtensionsAndExtraProperties.Utils;
using StardewValley;
using xTile.ObjectModel;

namespace MappingExtensionsAndExtraProperties.Functionality;

public class MailFlag
{
    public static void SetMailFlag(PropertyValue dhSetMailFlagProperty, Logger logger)
    {
        if (Parsers.TryParse(dhSetMailFlagProperty.ToString(), out SetMailFlag parsedProperty))
        {
            Player.TryAddMailFlag(parsedProperty.MailFlag, Game1.player);
        }
        else
        {
            logger.Error($"Failed to parse property {dhSetMailFlagProperty.ToString()}");
        }
    }
}
