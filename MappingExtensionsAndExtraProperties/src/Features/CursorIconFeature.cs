using System;
using DecidedlyShared.Logging;
using DecidedlyShared.Utilities;
using HarmonyLib;
using MappingExtensionsAndExtraProperties.Models.TileProperties;
using Microsoft.Xna.Framework;
using StardewValley;
using xTile.ObjectModel;

namespace MappingExtensionsAndExtraProperties.Features;

public class CursorIconFeature : Feature
{
    public override string FeatureId { get; init; }
    public override Harmony HarmonyPatcher { get; init; }
    public sealed override bool AffectsCursorIcon { get; init; }
    public sealed override int CursorId { get; init; }
    private static string[] tilePropertiesControlled;
    public override bool Enabled { get; internal set; }
    private static TilePropertyHandler tileProperties;
    private static Logger logger;

    public CursorIconFeature(Harmony harmony, string id, Logger logger, TilePropertyHandler tilePropertyHandler)
    {
        this.Enabled = false;
        this.HarmonyPatcher = harmony;
        this.FeatureId = id;
        CursorIconFeature.logger = logger;
        CursorIconFeature.tileProperties = tilePropertyHandler;
        this.AffectsCursorIcon = false;
        this.CursorId = default;
    }

    public override bool Enable()
    {
        try
        {
            this.HarmonyPatcher.Patch(
                AccessTools.Method(typeof(Game1), nameof(Game1.drawMouseCursor)),
                prefix: new HarmonyMethod(typeof(CursorIconFeature), nameof(CursorIconFeature.Game1_drawMouseCursor_Prefix)));
        }
        catch (Exception e)
        {
            logger.Exception(e);
        }

        this.Enabled = true;
        return true;
    }

    public override void Disable()
    {
        throw new System.NotImplementedException();
    }

    public override bool ShouldChangeCursor(GameLocation location, int tileX, int tileY, out int cursorId)
    {
        throw new System.NotImplementedException();
    }

    public static void Game1_drawMouseCursor_Prefix(Game1 __instance)
    {
        // int tileX = (int)Game1.currentCursorTile.X;
        // int tileY = (int)Game1.currentCursorTile.Y;
        //
        // if (FeatureManager.TryGetCursorIdForTile(Game1.currentLocation, tileX, tileY, out int id))
        // {
        //     Game1.mouseCursor = id;
        // }
    }
}