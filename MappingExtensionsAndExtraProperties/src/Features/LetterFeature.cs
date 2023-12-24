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

public class LetterFeature : Feature
{
    internal override FeatureManager ParentManager { get; init; }
    public override Harmony HarmonyPatcher { get; init; }
    public sealed override bool AffectsCursorIcon { get; init; }
    public sealed override int CursorId { get; init; }

    private static bool enabled;
    public override bool Enabled
    {
        get => enabled;
        internal set => enabled = value;
    }

    public override string FeatureId { get; init; }
    private static TilePropertyHandler tileProperties;
    private static Logger logger;

    public LetterFeature(Harmony harmony, string id, FeatureManager manager, Logger logger, TilePropertyHandler tilePropertyHandler)
    {
        this.Enabled = false;
        this.HarmonyPatcher = harmony;
        this.FeatureId = id;
        this.ParentManager = manager;
        LetterFeature.logger = logger;
        LetterFeature.tileProperties = tilePropertyHandler;
        this.AffectsCursorIcon = true;
        this.CursorId = 5;
    }

    public override bool Enable()
    {
        try
        {
            this.HarmonyPatcher.Patch(
                AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction)),
                postfix: new HarmonyMethod(typeof(LetterFeature),
                    nameof(LetterFeature.GameLocation_CheckAction_Postfix)));
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
        this.Enabled = false;
    }

    public override bool ShouldChangeCursor(Vector2 tile, out int cursorId)
    {
        throw new NotImplementedException();
    }

    public static void GameLocation_CheckAction_Postfix(GameLocation __instance, Location tileLocation,
        xTile.Dimensions.Rectangle viewport, Farmer who)
    {
        if (!enabled)
            return;

#if DEBUG
        Stopwatch timer = new Stopwatch();
        timer.Start();
#endif
        // Consider removing this try/catch.
        try
        {
            // First, pull our tile co-ordinates from the location.
            int tileX = tileLocation.X;
            int tileY = tileLocation.Y;

            if (tileProperties.TryGetBackProperty(tileX, tileY, __instance, LetterText.PropertyKey,
                         out PropertyValue letterProperty))
            {
                Letter.DoLetter(__instance, letterProperty, tileX, tileY, logger);
            }
        }
        catch (Exception e)
        {
            logger.Error("Caught exception handling GameLocation.checkAction in a postfix. Details follow:");
            logger.Exception(e);
        }
#if DEBUG
        timer.Stop();

        logger.Log($"Took {timer.ElapsedMilliseconds} to process in CheckAction patch.", LogLevel.Info);
#endif
    }
}
