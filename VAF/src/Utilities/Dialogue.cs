using StardewModdingAPI;
using SDialogue = StardewValley.Dialogue;

namespace VAF.Utilities;

public class Dialogue
{
    public static string? GetDialogueKey(SDialogue dialogue)
    {
        string?[] key = null;

        key = dialogue.TranslationKey.Split(":");

        if (key.Length != 2)
            return null;

        return key[1];
    }

    public static string? GetDialoguePath(SDialogue dialogue)
    {
        string?[] path = null;

        path = dialogue.TranslationKey.Split(":");

        if (path.Length != 2)
            return null;

        return path[0];
    }
}
