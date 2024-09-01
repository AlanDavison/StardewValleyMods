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
        DescriptiveBool result;

        switch (key)
        {
            case "DH_VarietyTools_ToolDoFunction_Prefix":
                logger.Log($"Found tool with method prefix ID.", LogLevel.Info);
                result = HarmonyHelper.TryPrefixPatchMethod(harmony, this.patchesClass, value);

                this.PrintPatchingResponse(result);
                break;
            case "DH_VarietyTools_ToolDoFunction_Postfix":
                logger.Log($"Found tool with method postfix ID.", LogLevel.Info);
                result = HarmonyHelper.TryPostfixPatchMethod(harmony, this.patchesClass, value);

                this.PrintPatchingResponse(result);
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
            logger.Log($"Successfully patched method {result.Context}", LogLevel.Info);
        }
    }
}
