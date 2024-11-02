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
            !ArgUtility.TryGetDirection(args, 3, out int facingDirection, out error, "int facingDirection") ||
            !ArgUtility.TryGetInt(args, 4, out int red, out error, "int facingDirection") ||
            !ArgUtility.TryGetInt(args, 5, out int green, out error, "int facingDirection") ||
            !ArgUtility.TryGetInt(args, 6, out int blue, out error, "int facingDirection"))
        {
            context.LogErrorAndSkip(error);
            return;
        }

        string slimeTexture = "Characters\\Monsters\\Green Slime";
        int slimeSpriteWidth = 16;
        int slimeSpriteHeight = 24;

        AnimatedSprite slimeSprite = new AnimatedSprite(slimeTexture, 0, slimeSpriteWidth, slimeSpriteHeight);
        NPC slime = new NPC(slimeSprite, tile * 64f, facingDirection, "Slime", Game1.content);

        // GreenSlime slime = new GreenSlime(tile * 64f, new Color(red, green, blue));
        // slime.timeSinceLastJump = -1000000;

        e.actors.Add(slime);
        e.currentCommand++;
    }
}
