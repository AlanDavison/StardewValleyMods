using System;
using System.IO;
using FontStashSharp;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace FontMaster;

public class ModEntry : Mod
{
    private static FontSystem fontSystem;
    private static SpriteFontBase font;
    private static IMonitor logger;

    public override void Entry(IModHelper helper)
    {
        fontSystem = new FontSystem();
        fontSystem.AddFont(
            File.ReadAllBytes(Path.Combine(this.Helper.DirectoryPath, "assets/StoryScript-Regular.ttf")));
        font = fontSystem.GetFont(12);
        logger = this.Monitor;
        Harmony harmony = new Harmony(this.ModManifest.UniqueID);
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(SpriteBatch), nameof(SpriteBatch.DrawString),
            new Type[]
            {
                typeof(SpriteFont), typeof(string), typeof(Vector2), typeof(Color), typeof(float), typeof(Vector2),
                typeof(float), typeof(SpriteEffects), typeof(float)

            }),
            prefix: new HarmonyMethod(typeof(ModEntry),
                nameof(ModEntry.SpriteBatch_DrawString_Prefix)));
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(SpriteBatch), nameof(SpriteBatch.DrawString),
                new Type[]
                {
                    typeof(SpriteFont), typeof(string), typeof(Vector2), typeof(Color)

                }),
            prefix: new HarmonyMethod(typeof(ModEntry),
                nameof(ModEntry.SpriteBatch_DrawString_Minimal_Prefix)));
    }

    private static bool SpriteBatch_DrawString_Minimal_Prefix(SpriteBatch __instance, SpriteFont spriteFont,
        string text, Vector2 position, Color color)
    {
        try
        {
            return false;
        }
        catch (Exception e)
        {
            logger.Log("DH did an oopsie.", LogLevel.Error);
            logger.Log(e.Message, LogLevel.Error);

            return true;
        }

        return false;
    }

    private static bool SpriteBatch_DrawString_Prefix(SpriteBatch __instance, SpriteFont spriteFont, string text,
        Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
    {
        try
        {
            return false;
        }
        catch (Exception e)
        {
            logger.Log("DH did an oopsie.", LogLevel.Error);
            logger.Log(e.Message, LogLevel.Error);

            return true;
        }

        return false;
    }
}
