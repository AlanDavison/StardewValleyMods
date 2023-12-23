using System;
using System.Collections.Generic;
using System.Reflection;
using DecidedlyShared.Constants;
using HarmonyLib;
using MappingExtensionsAndExtraProperties.Patches;
using StardewValley;

namespace MappingExtensionsAndExtraProperties.Features;

public class CloseupInteraction : IFeature
{
    public Harmony HarmonyPatcher { get; set; }
    public bool Enabled { get; set; }
    public List<(MethodInfo, HarmonyMethod, AffixType)> FeatureMethods { get; set; }
    public string FeatureId { get; set; }

    public CloseupInteraction(Harmony harmony)
    {
        this.HarmonyPatcher = harmony;
    }

    public bool Enable()
    {
        try
        {
            foreach (var patch in this.FeatureMethods)
            {
                if (patch.Item3 == AffixType.Postfix)
                {
                    this.HarmonyPatcher.Patch(
                        original: originalMethod,
                        postfix: methodToUse);
                }
                else if (patch.Item3 == AffixType.Prefix)
                {
                    this.HarmonyPatcher.Patch(
                        original: originalMethod,
                        prefix: methodToUse);
                }
            }

            this.HarmonyPatcher.Patch(
                AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction)),
                postfix: new HarmonyMethod(typeof(GameLocationPatches),
                    nameof(GameLocationPatches.GameLocation_CheckAction_Postfix)));
        }

        this.Enabled = true;
        return true;
    }

    public void AddPatch(MethodInfo originalMethod, HarmonyMethod methodToUse, AffixType type)
    {
        this.FeatureMethods.Add((originalMethod, methodToUse, type));
    }

    public void Disable()
    {
        throw new System.NotImplementedException();
    }

    public override int GetHashCode()
    {
        return this.FeatureId.GetHashCode();
    }
}
