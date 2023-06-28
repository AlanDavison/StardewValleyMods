using System.Text;

namespace TerrainFeatureRefresh.Framework;

public struct TfrSettings
{
    // TODO for later: Store the predicate in each individual TfrFeature in order to tidy up FeatureProcessor.cs.

    // Objects
    public TfrFeature fences = new TfrFeature();
    public TfrFeature weeds = new TfrFeature();
    public TfrFeature twigs = new TfrFeature();
    public TfrFeature stones = new TfrFeature();
    public TfrFeature forage = new TfrFeature();
    public TfrFeature artifactSpots = new TfrFeature();

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

    public override string ToString()
    {
        StringBuilder returned = new StringBuilder();
        returned.AppendLine($"SObjects");
        returned.AppendLine($"=========================");
        returned.AppendLine($"Fences: {this.fences.ToString()}");
        returned.AppendLine($"Weeds: {this.weeds.ToString()}");
        returned.AppendLine($"Twigs: {this.twigs.ToString()}");
        returned.AppendLine($"Stones: {this.stones.ToString()}");
        returned.AppendLine($"Forage: {this.forage.ToString()}");
        returned.AppendLine($"Artifact Spots: {this.artifactSpots.ToString()}");
        returned.AppendLine("\n");

        returned.AppendLine($"TerrainFeatures");
        returned.AppendLine($"=========================");
        returned.AppendLine($"Grass: {this.grass.ToString()}");
        returned.AppendLine($"Wild trees: {this.wildTrees.ToString()}");
        returned.AppendLine($"Fruit trees: {this.fruitTrees.ToString()}");
        returned.AppendLine($"Paths: {this.paths.ToString()}");
        returned.AppendLine($"Hoe dirt: {this.hoeDirt.ToString()}");
        returned.AppendLine($"Crops: {this.crops.ToString()}");
        returned.AppendLine($"Bushes: {this.bushes.ToString()}");
        returned.AppendLine("\n");

        returned.AppendLine($"ResourceClumps");
        returned.AppendLine($"=========================");
        returned.AppendLine($"Stumps: {this.stumps.ToString()}");
        returned.AppendLine($"Logs: {this.logs.ToString()}");
        returned.AppendLine($"Boulders: {this.boulders.ToString()}");
        returned.AppendLine($"Meteorites: {this.meteorites.ToString()}");

        return returned.ToString();
    }
}
