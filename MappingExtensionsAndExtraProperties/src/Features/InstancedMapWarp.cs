using System;
using System.Diagnostics;
using DecidedlyShared.Logging;
using DecidedlyShared.Utilities;
using HarmonyLib;
using MappingExtensionsAndExtraProperties.Functionality;
using MappingExtensionsAndExtraProperties.Models.TileProperties;
using MappingExtensionsAndExtraProperties.Utils;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using xTile.Dimensions;
using xTile.ObjectModel;

namespace MappingExtensionsAndExtraProperties.Features;

public class InstancedMapWarp : Feature
{
    public override Harmony HarmonyPatcher { get; init; }
    public sealed override bool AffectsCursorIcon { get; init; }
    public sealed override int CursorId { get; init; }
    private string[] tilePropertiesControlled = [
        "MEEP_Instanced_Touch_Warp"];
    public sealed override bool Enabled
    {
        get => enabled;
        internal set => enabled = value;
    }
    private static bool enabled;

    public override string FeatureId { get; init; }
    private static TilePropertyHandler tileProperties;
    private static Logger logger;

    public InstancedMapWarp(Harmony harmony, string id, Logger logger, TilePropertyHandler tilePropertyHandler)
    {
        this.Enabled = false;
        this.HarmonyPatcher = harmony;
        this.FeatureId = id;
        InstancedMapWarp.logger = logger;
        InstancedMapWarp.tileProperties = tilePropertyHandler;
        this.AffectsCursorIcon = true;
        this.CursorId = 5;

        GameLocation.RegisterTouchAction("MEEP_Instanced_Touch_Warp", this.InstancedTouchWarp);
    }

    private void InstancedTouchWarp(GameLocation arg1, string[] arg2, Farmer arg3, Vector2 arg4)
    {
        // Naively assuming the args are correct for ease during testing.
        int x = int.Parse(arg2[1]);
        int y = int.Parse(arg2[2]);

        logger.Error($"x: {x}, y: {y}");
        logger.Error($"Is structure? {arg1.isStructure.Value}");
        logger.Error($"Map name and ID: {arg1.uniqueName.Value}");
        Game1.warpFarmer(arg1.uniqueName.Value, x, y, false);
    }

    public override void Enable()
    {
        this.Enabled = true;
    }

    public override void Disable()
    {
        this.Enabled = false;
    }

    public override void RegisterCallbacks() {}

    public override bool ShouldChangeCursor(GameLocation location, int tileX, int tileY, out int cursorId)
    {
        cursorId = default;

        if (!enabled)
            return false;

        for (int i = 0; i < this.tilePropertiesControlled.Length; i++)
        {
            if (tileProperties.TryGetBackProperty(tileX, tileY, Game1.currentLocation, this.tilePropertiesControlled[i],
                    out PropertyValue _))
            {
                cursorId = this.CursorId;
                return true;
            }
        }

        return false;
    }
}
