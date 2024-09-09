using System;
using System.Collections.Generic;
using DecidedlyShared.Logging;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley.GameData.Tools;

namespace DHVarietyTools.Utilities;

public class PatchToolMethods(Harmony harmony, Patches patchesClass, Logger logger, List<ToolData> toolData)
{
    private readonly Type patchesClass = patchesClass.GetType();

    public void DoPatches()
    {
        logger.Log("Starting patches...", LogLevel.Info);

        foreach (ToolData data in toolData)
        {
            if (data.CustomFields is not null)
            {
                foreach (KeyValuePair<string, string> pair in data.CustomFields)
                {
                    this.TryDoPatchForField(pair.Key, pair.Value);
                }
            }
        }
    }

    private void TryDoPatchForField(string key, string value)
    {
        switch (key)
        {
            case "DH_VarietyTools_ToolDoFunction_Prefix":
                logger.Log($"Found tool with method prefix ID. Method to patch: {value}", LogLevel.Info);

                this.PrintPatchingResponse(HarmonyHelper.TryPrefixPatchMethod(harmony, this.patchesClass, value));
                break;
            case "DH_VarietyTools_ToolDoFunction_Postfix":
                logger.Log($"Found tool with method postfix ID. Method to patch: {value}", LogLevel.Info);

                this.PrintPatchingResponse(HarmonyHelper.TryPostfixPatchMethod(harmony, this.patchesClass, value));
                break;
        }
    }

    private void PrintPatchingResponse(DescriptiveBool result)
    {
        if (!result)
        {
            logger.Error($"Error patching method. Context follows.");
            logger.Error($"Context: {result.Context}");

            if (result.ExceptionExists())
            {
                logger.Exception(result.Exception!);
            }
        }
        else
        {
            logger.Log($"{result.Context}", LogLevel.Info);
        }
    }
}
