using DecidedlyShared.Logging;
using DecidedlyShared.Utilities;
using MappingExtensionsAndExtraProperties.Utils;

namespace MappingExtensionsAndExtraProperties.Patches;

public class EventPatches
{
    private static Logger? logger = null;
    private static TilePropertyHandler? tileProperties = null;
    private static Properties propertyUtils;

    public static void InitialisePatches(Logger logger, TilePropertyHandler tileProperties)
    {
        EventPatches.logger = logger;
        EventPatches.tileProperties = tileProperties;
        propertyUtils = new Properties(logger);
    }

}
