using System;
using DecidedlyShared.Logging;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buffs;

namespace InvisibleBuffs;

public class BuffPatches
{
    private static Logger? logger;

    public static Logger? Logger
    {
        get => logger;
        set => logger = value;
    }

    public static void BuffConstructor_Prefix(Buff __instance, string id, string source = null,
        string displaySource = null, int duration = -1, Texture2D iconTexture = null, int iconSheetIndex = -1,
        BuffEffects effects = null, bool? isDebuff = null, string displayName = null, string description = null)
    {
        try
        {
            if (__instance.customFields.ContainsKey("DH.Buffs.Invisible"))
                __instance.visible = false;
        }
        catch (Exception e)
        {
            logger?.Exception(e);
        }
    }
}
