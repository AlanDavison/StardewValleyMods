using DecidedlyShared.Logging;
using DecidedlyShared.Utilities;
using HarmonyLib;
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

    public CropSpawnFeature(string id, Logger logger, TilePropertyHandler tilePropertyHandler)
    {
        this.FeatureId = id;
        this.logger = logger;
        this.tilePropertyHandler = tilePropertyHandler;
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
        FeatureManager.OnDayStart();
    }

    public override bool ShouldChangeCursor(GameLocation location, int tileX, int tileY, out int cursorId)
    {
        throw new System.NotImplementedException();
    }
}
