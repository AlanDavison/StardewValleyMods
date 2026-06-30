using System;
using System.Collections.Generic;
using DecidedlyShared.Logging;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace MappingExtensionsAndExtraProperties.Features;

public class InvulnerableTreeFeature : Feature
{
    public override string FeatureId { get; init; }
    public override Harmony HarmonyPatcher { get; init; }
    public override bool AffectsCursorIcon { get; init; }
    public override int CursorId { get; init; }
    public override bool Enabled { get; internal set; }
    private static Logger logger;
    private static Harmony harmony;
    private static IModHelper helper;

    public InvulnerableTreeFeature(Harmony harmony, string featureId, Logger logger)
    {
        InvulnerableTreeFeature.harmony = harmony;
        InvulnerableTreeFeature.logger = logger;
        this.FeatureId = featureId;
    }

    public override void Enable()
    {
        try
        {
            harmony.Patch(
                AccessTools.DeclaredMethod(typeof(Tree), nameof(Tree.performToolAction)),
                prefix: new HarmonyMethod(typeof(InvulnerableTreeFeature),
                    nameof(InvulnerableTreeFeature.Tree_PerformToolAction_Prefix)));

            harmony.Patch(
                AccessTools.DeclaredMethod(typeof(FruitTree), nameof(FruitTree.performToolAction)),
                prefix: new HarmonyMethod(typeof(InvulnerableTreeFeature),
                    nameof(InvulnerableTreeFeature.FruitTree_PerformToolAction_Prefix)));
        }
        catch (Exception e)
        {
            logger.Exception(e);
        }

        this.Enabled = true;
    }

    public override void Disable()
    {
        this.Enabled = false;
    }

    public override void RegisterCallbacks() { }

    public override bool ShouldChangeCursor(GameLocation location, int tileX, int tileY, out int cursorId)
    {
        cursorId = default;
        return false;
    }

    public static bool FruitTree_PerformToolAction_Prefix(FruitTree __instance, Tool t, int explosion,
        Vector2 tileLocation)
    {
        try
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (__instance is null)
                return true;
            if (__instance.GetData() is null)
                return true;
            if (__instance.GetData().CustomFields is null)
                return true;

            return !DoCustomFieldsSuggestInvulnerability(__instance.GetData().CustomFields);
        }
        catch (Exception e)
        {
            logger.Exception(e);
            return true;
        }
    }

    public static bool Tree_PerformToolAction_Prefix(Tree __instance, Tool t, int explosion, Vector2 tileLocation)
    {
        try
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (__instance is null)
                return true;
            if (__instance.GetData() is null)
                return true;
            if (__instance.GetData().CustomFields is null)
                return true;

            return !DoCustomFieldsSuggestInvulnerability(__instance.GetData().CustomFields);
        }
        catch (Exception e)
        {
            logger.Exception(e);
            return true;
        }
    }

    private static bool DoCustomFieldsSuggestInvulnerability(Dictionary<string, string> data)
    {
        return data.ContainsKey("DH_MEEP_Invulnerable_Tree");
    }
}
