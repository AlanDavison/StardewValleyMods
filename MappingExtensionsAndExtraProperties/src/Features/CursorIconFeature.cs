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
    internal override FeatureManager ParentManager { get; init; }
    public override Harmony HarmonyPatcher { get; init; }
    public sealed override bool AffectsCursorIcon { get; init; }
    public sealed override int CursorId { get; init; }
    public override bool Enabled { get; internal set; }
    private static TilePropertyHandler tileProperties;
    private static Logger logger;

    public CursorIconFeature(Harmony harmony, string id, FeatureManager manager, Logger logger, TilePropertyHandler tilePropertyHandler)
    {
        this.Enabled = false;
        this.HarmonyPatcher = harmony;
        this.FeatureId = id;
        this.ParentManager = manager;
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

    public override bool ShouldChangeCursor(Vector2 tile, out int cursorId)
    {
        throw new System.NotImplementedException();
    }

    public static void Game1_drawMouseCursor_Prefix(Game1 __instance)
    {
        int xTile = (int)Game1.currentCursorTile.X;
        int yTile = (int)Game1.currentCursorTile.Y;



        if (tileProperties.TryGetBackProperty(xTile, yTile, Game1.currentLocation, SetMailFlag.PropertyKey,
                out PropertyValue _) ||
            tileProperties.TryGetBackProperty(xTile, yTile, Game1.currentLocation, CloseupInteractionImage.PropertyKey,
                out PropertyValue _) ||
            tileProperties.TryGetBackProperty(xTile, yTile, Game1.currentLocation,
                $"{CloseupInteractionImage.PropertyKey}_1",
                out PropertyValue _) ||
            tileProperties.TryGetBackProperty(xTile, yTile, Game1.currentLocation, LetterType.PropertyKey,
                out PropertyValue _) ||
            tileProperties.TryGetBackProperty(xTile, yTile, Game1.currentLocation, LetterText.PropertyKey,
                out PropertyValue _) ||
            tileProperties.TryGetBackProperty(xTile, yTile, Game1.currentLocation, SetMailFlag.PropertyKey,
                out PropertyValue _))
        {
            Game1.mouseCursor = 5;
        }
    }
}
