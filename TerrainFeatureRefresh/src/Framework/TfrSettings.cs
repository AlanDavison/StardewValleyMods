namespace TerrainFeatureRefresh.Framework;

public struct TfrSettings
{
    // Objects
    public TfrFeature fences = new TfrFeature();
    public TfrFeature weeds = new TfrFeature();
    public TfrFeature twigs = new TfrFeature();
    public TfrFeature stones = new TfrFeature();
    public TfrFeature forage = new TfrFeature();

    // Terrain Features
    public TfrFeature grass = new TfrFeature();
    public TfrFeature wildTrees = new TfrFeature();
    public TfrFeature fruitTrees = new TfrFeature();
    public TfrFeature paths = new TfrFeature();
    public TfrFeature hoeDirt = new TfrFeature();
    public TfrFeature crops = new TfrFeature();
    public TfrFeature bushes = new TfrFeature();

    // Resource Clumps
    public TfrFeature stumps = new TfrFeature();
    public TfrFeature logs = new TfrFeature();
    public TfrFeature boulders = new TfrFeature();
    public TfrFeature meteorites = new TfrFeature();

    public TfrSettings() { }
}
