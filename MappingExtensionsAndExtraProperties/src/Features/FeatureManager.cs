using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MappingExtensionsAndExtraProperties.Models.EventArgs;
using Microsoft.Xna.Framework;
using StardewValley;

namespace MappingExtensionsAndExtraProperties.Features;

public static class FeatureManager
{
    private static HashSet<Feature> features = new HashSet<Feature>();
    private static Feature[] cursorAffectingFeatures;
    internal static event EventHandler GameTickCallback;
    internal static event EventHandler<OnLocationChangeEventArgs> OnLocationChangeCallback;

    public static void AddFeature(Feature f)
    {
        features.Add(f);

        // This is slower than a HashSet when adding, but we'll be iterating over this entire thing every frame.
        // I would rather this be an array for performance.
        cursorAffectingFeatures = features.Where((f) => (f.AffectsCursorIcon) == true).ToArray();
    }

    public static void EnableFeatures()
    {
        foreach (var feature in features)
        {
            feature.Enable();
        }
    }

    public static void DisableFeature(string featureId)
    {
        Feature feature = features.FirstOrDefault(f => f.FeatureId.Equals(featureId));

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
    public static bool IsFeatureEnabled(string featureId)
    {
        foreach (var feature in features)
        {
            if (feature.FeatureId.Equals(featureId))
                return feature.Enabled;
        }

        return false;
    }

    public static bool TryGetCursorIdForTile(GameLocation location, int tileX, int tileY, out int id)
    {
        id = default;
        bool shouldChangeCursor = false;

        for (int i = 0; i < cursorAffectingFeatures.Length; i++)
        {
            if (!shouldChangeCursor)
                shouldChangeCursor = cursorAffectingFeatures[i].ShouldChangeCursor(location, tileX, tileY, out id);
            else
                cursorAffectingFeatures[i].ShouldChangeCursor(location, tileX, tileY, out _);
        }

        return shouldChangeCursor;
    }

    public static void TickFeatures()
    {
        GameTickCallback.Invoke(null, null);

    }

    public static void OnLocationChange(GameLocation oldLocation, GameLocation newLocation, Farmer player)
    {
        OnLocationChangeCallback.Invoke(null,
            new OnLocationChangeEventArgs()
            {
                OldLocation =  oldLocation,
                NewLocation = newLocation,
                Player = player
            });
    }
}
