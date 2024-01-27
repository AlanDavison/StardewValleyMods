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
    internal static event EventHandler GameTickCallback;
    internal static event EventHandler<OnLocationChangeEventArgs> OnLocationChangeCallback;

    public static void AddFeature(Feature f)
    {
        features.Add(f);
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

        Stopwatch watch = new Stopwatch();
        watch.Start();
        Feature[] f = features.Where((f) => (f.AffectsCursorIcon && f.Enabled) == true).ToArray();
        bool shouldChangeCursor = false;

        for (int i = 0; i < f.Length; i++)
        {
            if (!shouldChangeCursor)
                shouldChangeCursor = f[i].ShouldChangeCursor(location, tileX, tileY, out id);
            else
                f[i].ShouldChangeCursor(location, tileX, tileY, out _);
        }

        watch.Stop();
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
