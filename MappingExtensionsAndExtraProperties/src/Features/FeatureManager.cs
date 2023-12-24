using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;

namespace MappingExtensionsAndExtraProperties.Features;

public class FeatureManager
{
    private HashSet<Feature> features;
    internal event EventHandler GameTickCallback;

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

    public void DisableFeature(string featureId)
    {
        Feature feature = this.features.FirstOrDefault(f => f.FeatureId.Equals(featureId));

        if (feature is not null)
        {
            feature.Disable();
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

    public bool TryGetCursorIdForTile(GameLocation location, Vector2 tile, out int id)
    {
        id = default;

        Feature f = this.features.FirstOrDefault((f) => (f.AffectsCursorIcon && f.Enabled) == true);

        if (f is not null)
        {
            id = f.CursorId;

            return true;
        }

        return false;
    }

    public void TickFeatures()
    {
        this.GameTickCallback.Invoke(this, null);
    }
}
