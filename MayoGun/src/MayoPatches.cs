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

    // public static void Character_Draw_SB_Postfix(Character __instance, SpriteBatch b)
    // {
    //     if (__instance is not Monster)
    //         return;
    //
    //     if (!MayoCore.IsMonsterGlobbed((Monster)__instance))
    //         return;
    //
    //     Vector2 position = new Vector2((__instance.Position.X * Game1.tileSize) - Game1.viewport.X, (__instance.Position.Y * Game1.tileSize) - Game1.viewport.Y);
    //     b.DrawString(Game1.smallFont, MayoCore.GetGlobTimeRemaining(__instance as Monster).Value.ToString(), position, Color.White);
    // }
    //
    // public static void Character_drawAboveAlwaysFrontLayer_Postfix(Character __instance, SpriteBatch b)
    // {
    //     if (__instance is not Monster)
    //         return;
    //
    //     if (!MayoCore.IsMonsterGlobbed((Monster)__instance))
    //         return;
    //
    //     Vector2 position = new Vector2((__instance.Position.X * Game1.tileSize) - Game1.viewport.X, (__instance.Position.Y * Game1.tileSize) - Game1.viewport.Y);
    //     b.DrawString(Game1.smallFont, MayoCore.GetGlobTimeRemaining(__instance as Monster).Value.ToString(), position, Color.White);
    // }
    //
    // public static void Character_Draw_SB_F_Postfix(Character __instance, SpriteBatch b, float alpha = 1f)
    // {
    //     if (__instance is not Monster)
    //         return;
    //
    //     if (!MayoCore.IsMonsterGlobbed((Monster)__instance))
    //         return;
    //
    //     Vector2 position = new Vector2((__instance.Position.X * Game1.tileSize) - Game1.viewport.X, (__instance.Position.Y * Game1.tileSize) - Game1.viewport.Y);
    //     b.DrawString(Game1.smallFont, MayoCore.GetGlobTimeRemaining(__instance as Monster).Value.ToString(), position, Color.White);
    // }
    //
    // public static void Character_Draw_SB_I_F_Postfix(Character __instance, SpriteBatch b, int ySourceRectOffset,
    //     float alpha = 1f)
    // {
    //     if (__instance is not Monster)
    //         return;
    //
    //     if (!MayoCore.IsMonsterGlobbed((Monster)__instance))
    //         return;
    //
    //     Vector2 position = new Vector2((__instance.Position.X * Game1.tileSize) - Game1.viewport.X, (__instance.Position.Y * Game1.tileSize) - Game1.viewport.Y);
    //     b.DrawString(Game1.smallFont, MayoCore.GetGlobTimeRemaining(__instance as Monster).Value.ToString(), position, Color.White);
    // }



    // public static void CharacterDraw_Postfix(Character __instance, SpriteBatch b, int ySourceRectOffset, float alpha)
    // public static void CharacterDraw_Postfix(Character __instance, SpriteBatch b)
    // {
    //     if (__instance is not Monster)
    //         return;
    //
    //     if (!MayoCore.IsMonsterGlobbed((Monster)__instance))
    //         return;
    //
    //     // Vector2 position = new Vector2((__instance.Position.X * Game1.tileSize) - Game1.viewport.X, (__instance.Position.Y * Game1.tileSize) - Game1.viewport.Y);
    //     // b.Draw(globTexture, position, sourceRect, Color.WhiteSmoke, 0f, Vector2.Zero, new Vector2(4, 4), SpriteEffects.None, 1f);
    //
    //     Vector2 position = new Vector2((__instance.Position.X * Game1.tileSize) - Game1.viewport.X, (__instance.Position.Y * Game1.tileSize) - Game1.viewport.Y);
    //     b.DrawString(Game1.smallFont, MayoCore.GetGlobTimeRemaining(__instance as Monster).Value.ToString(), position, Color.White);
    // }

    public static bool MonsterBehaviorAtGameTick_Prefix(Monster __instance, GameTime time)
    {
        return ShouldBlock(__instance);
    }

    public static bool MonsterUpdate_Prefix(Monster __instance, GameTime time, GameLocation location)
    {
        return ShouldBlock(__instance);
    }

    private static bool ShouldBlock(Monster __instance)
    {
        if (MayoCore.IsMonsterGlobbed(__instance))
            return false;

        return true;
    }

    public static bool CharacterUpdate_Prefix(Character __instance, GameTime time, GameLocation location, long id,
        bool move)
    {
        if (__instance is Monster)
            return MonsterUpdate_Prefix((Monster)__instance, time, location);

        return true;
    }
}
