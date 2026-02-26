using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using DecidedlyShared.Logging;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Machines;
using StardewValley.Objects;

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

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Crop), nameof(Crop.harvest)),
            transpiler: new HarmonyMethod(typeof(ModEntry),
                nameof(ModEntry.CropHarvest_Transpiler)));
    }

    public static IEnumerable<CodeInstruction> CropHarvest_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original, ILGenerator generator)
    {
        try
        {
            CodeMatcher matcher = new CodeMatcher(instructions, generator);

            matcher.MatchEndForward(
                // new CodeMatch(OpCodes.Newobj,
                //     AccessTools.Constructor(typeof(ColoredObject),
                //         new Type[] { typeof(string), typeof(int), typeof(Color) })),
                // new CodeMatch(OpCodes.Dup),
                // new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Callvirt, AccessTools.PropertySetter(typeof(Item), nameof(Item.Quality)))
                // new CodeMatch(OpCodes.Nop)
            );

            if (!matcher.IsValid)
            {
                StaticLogger.Error($"Transpiler match not found. Returning unmodified IL.");

                return instructions;
            }

            matcher
                .Advance(2)
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    CodeInstruction.Call(typeof(ModEntry), nameof(ModEntry.GetMagicalCropItem))
                );
        }
        catch (Exception e)
        {
            StaticLogger.Error($"Ran into exception handling transpiler for Crop.harvest. Details follow.");
            StaticLogger.Exception(e);
        }

        return instructions;
    }

    public static Item GetMagicalCropItem(Item originalItem, Crop cropInstance)
    {
        if (cropInstance.modData is null)
            return originalItem;

        if (!cropInstance.modData.ContainsKey("DH.MagicalCrops.ProduceItemId"))
        {
            StaticLogger.Log("This Crop instance didn't contain our item ID modData. Continuing with normal item.", LogLevel.Trace);

            return originalItem;
        }

        try
        {
            string produceId = cropInstance.modData["DH.MagicalCrops.ProduceItemId"];

            return ItemRegistry.Create(produceId);
        }
        catch (Exception e)
        {
            StaticLogger.Error(
                "Caught exception trying to get Magical Crop produce item for this crop. Returning the default item instead.");
            StaticLogger.Exception(e);
        }

        return originalItem;
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
