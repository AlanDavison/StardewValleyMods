using System;
using DecidedlyShared.Logging;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;

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
            logger.Error($"Failed playing sound \"{args[1]}\" from event {e.id} in {loc.Name} at command index {e.CurrentCommand}.");

        ContinueEvent(e, loc, time);
    }

    private static void ContinueEvent(Event e, GameLocation loc, GameTime time)
    {
        e.CurrentCommand++;
        // e.InsertNextCommand(loc, time);
    }

    public static void AddColouredSlime(Event e, string[] args, EventContext context)
    {
        if (!ArgUtility.TryGetVector2(args, 1, out Vector2 tile, out string? error, integerOnly: false, "Vector2 tile") ||
            !ArgUtility.TryGetInt(args, 3, out int red, out error, "int red") ||
            !ArgUtility.TryGetInt(args, 4, out int green, out error, "int green") ||
            !ArgUtility.TryGetInt(args, 5, out int blue, out error, "int blue"))
        {
            context.LogErrorAndSkip(error);
            return;
        }

        red = Math.Clamp(red, 0, 255);
        green = Math.Clamp(green, 0, 255);
        blue = Math.Clamp(blue, 0, 255);

        string slimeTexture = "Characters\\Monsters\\Green Slime";
        int slimeSpriteWidth = 16;
        int slimeSpriteHeight = 24;

        GreenSlime slime = new GreenSlime(tile * 64f, new Color(red, green, blue));
        slime.timeSinceLastJump = 0;

        e.actors.Add(slime);
        e.currentCommand++;
    }
}
