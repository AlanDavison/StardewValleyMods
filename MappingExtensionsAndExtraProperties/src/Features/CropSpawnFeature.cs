using System;
using DecidedlyShared.APIs;
using DecidedlyShared.Logging;
using DecidedlyShared.Utilities;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace MappingExtensionsAndExtraProperties.Features;

public class CropSpawnFeature : Feature
{
    public override string FeatureId { get; init; }
    public override Harmony HarmonyPatcher { get; init; }
    public override bool AffectsCursorIcon { get; init; }
    public override int CursorId { get; init; }
    public override bool Enabled { get; internal set; }
    private Logger logger;
    private TilePropertyHandler tilePropertyHandler;
    private IQuickSaveApi quickSaveApi;

    public CropSpawnFeature(string id, Logger logger, TilePropertyHandler tilePropertyHandler, IQuickSaveApi quickSaveApi)
    {
        this.FeatureId = id;
        this.logger = logger;
        this.tilePropertyHandler = tilePropertyHandler;
        this.quickSaveApi = quickSaveApi;
    }

    public override void Enable()
    {
        this.Enabled = true;
    }

    public override void Disable()
    {
        this.Enabled = false;
    }

    public override void RegisterCallbacks()
    {
        FeatureManager.OnDayStartCallback += this.OnDayStartCallback;
    }

    private void OnDayStartCallback(object? sender, EventArgs e)
    {
        if (this.quickSaveApi is not null)
        {
            this.logger.Log("[Crop Spawn Feature] Quick Save's API was loaded properly.", LogLevel.Trace);

            if (this.quickSaveApi.IsLoading)
            {
                this.logger.Log("[Crop Spawn Feature] Quick Save indicated it was loading. Skipping this DayStart.", LogLevel.Trace);

                return;
            }

            this.logger.Log("[Crop Spawn Feature] Quick Save did not indicate it was loading. Proceeding with this DayStart as normal.", LogLevel.Trace);
        }


    }

    public override bool ShouldChangeCursor(GameLocation location, int tileX, int tileY, out int cursorId)
    {
        throw new System.NotImplementedException();
    }
}
