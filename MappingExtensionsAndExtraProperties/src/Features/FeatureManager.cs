using System.Collections.Generic;

namespace MappingExtensionsAndExtraProperties.Features;

public class FeatureManager
{
    private HashSet<Feature> features;

    public FeatureManager()
    {
        this.features = new HashSet<Feature>();
    }

    public void AddFeature(Feature f)
    {
        this.features.Add(f);
    }

    public void EnableFeatures()
    {
        foreach (var feature in this.features)
        {
            feature.Enable();
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="featureId"></param>
    /// <returns>True if the feature is found and enabled, and false if the feature doesn't exist/hasn't been added.</returns>
    public bool IsFeatureEnabled(string featureId)
    {
        foreach (var feature in this.features)
        {
            if (feature.FeatureId.Equals(featureId))
                return feature.Enabled;
        }

        return false;
    }
}
