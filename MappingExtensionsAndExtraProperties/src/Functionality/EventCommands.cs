using DecidedlyShared.Logging;
using StardewModdingAPI;

namespace MappingExtensionsAndExtraProperties.Functionality;

public class EventCommands
{
    private IModHelper helper;
    private Logger logger;

    public EventCommands(IModHelper h, Logger l)
    {
        this.helper = h;
        this.logger = l;
    }
}
