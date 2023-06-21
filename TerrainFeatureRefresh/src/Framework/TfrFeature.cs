namespace TerrainFeatureRefresh.Framework;

public class TfrFeature
{
    public TfrAction actionToTake = TfrAction.Ignore;

    public override string ToString()
    {
        return this.actionToTake.ToString();
    }
}
