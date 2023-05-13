namespace SmartBuildingRedux.Framework.PlacementRestrictions;

public struct Vanilla : IPlacementRestrictions
{
    public bool ClearTile { get; private set; }

    public Vanilla(bool requireClearTile)
    {
        this.ClearTile = requireClearTile;
    }
}
