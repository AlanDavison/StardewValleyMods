using System;
using DecidedlyShared.Logging;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley.Menus;

namespace VAF;

public class Patches
{
    private static Logger logger;

    public Patches(IModHelper helper, IMonitor monitor, IMod mod)
    {
        logger = new Logger(monitor);
        Harmony harmony = new Harmony(mod.ModManifest.UniqueID);

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(DialogueBox), nameof(DialogueBox.draw)),
            postfix: new HarmonyMethod(typeof(Patches),
                nameof(Patches.DialogueBox_draw_Postfix)));
    }

    public static void DialogueBox_draw_Postfix(DialogueBox __instance, SpriteBatch b)
    {
        try
        {
            int currentIndex = __instance.characterDialogue.currentDialogueIndex + 1;
            int dialogueTotal = __instance.characterDialogue.dialogues.Count;
            logger.Log($"dialogueContinuesOnNextPage: {__instance.dialogueContinuedOnNextPage}");

            // THIS IS ALMOST PERFECT. The current index doesn't seem to increment when using the $e dialogue split.
            // It does when using $b. If I can figure this one thing out, I think VAF can actually happen.
            // WAIT! I think this is because of the zero-indexing, and the ID always goes to dialogues.Count + 1 even if
            // the dialogue ends at dialogues.Count.

            if (currentIndex > dialogueTotal)
                logger.Log($"Current dialogue index was above the total dialogue count. Last dialogue was just displayed.", LogLevel.Info);
            else
            {
                logger.Log(
                    $"On dialogue {currentIndex} of {dialogueTotal} for {__instance.characterDialogue.TranslationKey}.",
                    LogLevel.Info);
            }
        }
        catch (Exception e)
        {
            logger.Exception(e);
        }
    }
}
