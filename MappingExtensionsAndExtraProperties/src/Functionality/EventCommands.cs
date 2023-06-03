using DecidedlyShared.Logging;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace MappingExtensionsAndExtraProperties.Functionality;

public class EventCommands
{
    private static IModHelper helper;
    private static Logger logger;

    public EventCommands(IModHelper h, Logger l)
    {
        helper = h;
        logger = l;
    }

    public static void PlaySound(Event e, GameLocation loc, GameTime time, string[] args)
    {
        if (!DecidedlyShared.Utilities.Sound.TryPlaySound(args[1]))
            logger.Error($"Failed playing sound \"{args[1]}\" from event {e.id} in {loc.Name} at command index {e.currentCommand}.");

        ContinueEvent(e, loc, time);
    }

    private static void ContinueEvent(Event e, GameLocation loc, GameTime time)
    {
        e.currentCommand++;
        e.checkForNextCommand(loc, time);
    }
}
