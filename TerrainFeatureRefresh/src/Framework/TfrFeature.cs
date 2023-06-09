namespace TerrainFeatureRefresh.Framework;

public class TfrFeature
{
    public TfrAction actionToTake = TfrAction.None;

    public override string ToString()
    {
        return this.actionToTake.ToString();
    }
}
