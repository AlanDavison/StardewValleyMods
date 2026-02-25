using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using DecidedlyShared.Logging;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Machines;

namespace MagicalCrops;

public class ModEntry : Mod
{
    private Logger logger;
    private static Logger StaticLogger;

    public override void Entry(IModHelper helper)
    {
        Harmony harmony = new Harmony(this.ModManifest.UniqueID);
        this.logger = new Logger(this.Monitor);
        StaticLogger = this.logger;

        harmony.Patch(
            AccessTools.Method(typeof(Item), nameof(Item.canStackWith)),
            prefix: new HarmonyMethod(typeof(ModEntry),
                nameof(ModEntry.canStackWith_Postfix)));

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(SObject), nameof(SObject.maximumStackSize)),
            prefix: new HarmonyMethod(typeof(ModEntry),
                nameof(ModEntry.maximumStackSize_Postfix)));
    }

    public static IEnumerable<CodeInstruction> CropHarvest_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original, ILGenerator generator)
    {
        CodeMatcher matcher = new CodeMatcher(instructions, generator);


        matcher.MatchStartForward([OpCodes.ca]);

        return instructions;
    }

    public static bool maximumStackSize_Postfix(SObject __instance, int __result)
    {
        try
        {
            if (__instance.modData.ContainsKey("DH.MagicalSeed.ProduceID"))
            {
                __result = 1;

                return false;
            }
        }
        catch (Exception e)
        {
            StaticLogger.Exception(e);
        }

        return true;
    }

    public static bool canStackWith_Postfix(Item __instance, ISalable other, bool __result)
    {
        try
        {
            if (__instance.modData is null)
                return true;

            if (__instance.modData.ContainsKey("DH.MagicalSeed.ProduceID"))
            {
                __result = false;

                return false;
            }
        }
        catch (Exception e)
        {
            StaticLogger.Exception(e);
        }

        return true;
    }

    public static Item GetMagicalSeed(SObject machine, Item inputItem, bool probe, MachineItemOutput outputData,
        Farmer player, out int? overrideMinutesUntilReady)
    {
        overrideMinutesUntilReady = 10;

        Item magicalSeed = ItemRegistry.Create("770");
        magicalSeed.modData.Add("DH.MagicalSeed.ProduceID", inputItem.ItemId);

        return magicalSeed;
    }
}
