using DecidedlyShared.Logging;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

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

    public void PlaySound(Event e, GameLocation loc, GameTime time, string[] args)
    {

    }
}
