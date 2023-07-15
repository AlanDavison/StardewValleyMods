using DecidedlyShared.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;

namespace MayoGun;

public static class MayoPatches
{
    private static IModHelper helper;
    private static IMonitor monitor;
    private static Logger logger;
    private static Rectangle sourceRect = new Rectangle(0, 64, 16, 16);
    private static Texture2D globTexture;

    public static void InitialisePatches(IModHelper helper, IMonitor monitor, Logger logger)
    {
        MayoPatches.helper = helper;
        MayoPatches.monitor = monitor;
        MayoPatches.logger = logger;
        MayoPatches.globTexture = Game1.content.Load<Texture2D>("Characters/Monsters/Spider");
    }

    // public static void CharacterDraw_Postfix(Character __instance, SpriteBatch b, int ySourceRectOffset, float alpha)
    // {
    //     if (__instance is not Monster)
    //         return;
    //
    //     if (!MayoCore.IsMonsterGlobbed((Monster)__instance))
    //         return;
    //
    //     Vector2 position = new Vector2((__instance.Position.X * Game1.tileSize) - Game1.viewport.X, (__instance.Position.Y * Game1.tileSize) - Game1.viewport.Y);
    //     b.Draw(globTexture, position, sourceRect, Color.WhiteSmoke, 0f, Vector2.Zero, new Vector2(4, 4), SpriteEffects.None, 1f);
    // }

    public static bool MonsterUpdate_Prefix(Monster __instance, GameTime time, GameLocation location)
    {
        if (MayoCore.IsMonsterGlobbed(__instance))
            return false;

        return true;
    }

    public static bool CharacterUpdate_Prefix(Character __instance, GameTime time, GameLocation location, long id, bool move)
    {
        if (__instance is Monster)
            return MonsterUpdate_Prefix((Monster)__instance, time, location);

        return true;
    }
}
