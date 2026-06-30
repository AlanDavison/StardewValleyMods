using System;
using DecidedlyShared.Logging;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buffs;

namespace InvisibleBuffs;

public class ModEntry : Mod
{
    private Logger logger;
    private static Logger StaticLogger;
    private static ModConfig config;

    public override void Entry(IModHelper helper)
    {
        this.logger = new Logger(this.Monitor);
        config = helper.ReadConfig<ModConfig>();
        StaticLogger = this.logger;
        Harmony harmony = new Harmony(this.ModManifest.UniqueID);

        if (config.DebugMode)
        {
            this.logger.Log(
                "Debug mode is enabled. You'll see a log line every time a buff detected as invisible is instantiated.",
                LogLevel.Info);
            this.logger.Log("If you don't want this, set \"DebugMode\" to false in \"config.json\".", LogLevel.Info);
        }

        try
        {
            this.PatchMethods(harmony);
        }
        catch (Exception e)
        {
            this.logger.Exception(
                e,
                $"Caught exception patching \"StardewValley.Buff\" constructor with method \"{nameof(ModEntry.BuffConstructor_Prefix)}\"");
        }
    }

    private void PatchMethods(Harmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(Buff),
            [
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(int),
                typeof(Texture2D),
                typeof(int),
                typeof(BuffEffects),
                typeof(bool?),
                typeof(string),
                typeof(string)
            ]),
            postfix: new HarmonyMethod(typeof(ModEntry),
                nameof(ModEntry.BuffConstructor_Prefix)));
    }

    public static void BuffConstructor_Prefix(Buff __instance, string id, string source = null,
        string displaySource = null, int duration = -1, Texture2D iconTexture = null, int iconSheetIndex = -1,
        BuffEffects effects = null, bool? isDebuff = null, string displayName = null, string description = null)
    {
        try
        {
            if (__instance.customFields.ContainsKey("DH.Buffs.Invisible"))
            {
                __instance.visible = false;

                if (config.DebugMode)
                {
                    StaticLogger?.Log("Invisible buff instantiated. Details (note that some of these can be blank): ", LogLevel.Info);

                    try
                    {
                        StaticLogger?.Log($"ID: {__instance.id}", LogLevel.Info);
                        StaticLogger?.Log($"Display name: {__instance.displayName}", LogLevel.Info);
                        StaticLogger?.Log($"Description: {__instance.description}", LogLevel.Info);

                        foreach (INetSerializable effect in __instance.effects.NetFields.GetFields())
                        {
                            StaticLogger?.Log($"{effect.Name}: {effect}", LogLevel.Info);
                        }
                    }
                    catch (Exception e)
                    {
                        StaticLogger?.Exception(e, $"Caught an exception getting buff {__instance.id}'s details.");
                    }

                    StaticLogger?.Log("If you don't want this debug logging, set \"DebugMode\" to false in \"config.json\".", LogLevel.Info);
                }
            }
        }
        catch (Exception e)
        {
            StaticLogger?.Exception(e, "Caught exception handling invisible buff.");
        }
    }
}
