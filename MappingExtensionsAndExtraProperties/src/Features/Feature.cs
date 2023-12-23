using System.Reflection;
using DecidedlyShared.Constants;
using DecidedlyShared.Logging;
using DecidedlyShared.Utilities;
using HarmonyLib;

namespace MappingExtensionsAndExtraProperties.Features;

public abstract class Feature
{
    /// <summary>
    /// The <see cref="Harmony">Harmony</see> reference used to apply this feature's patches.
    /// </summary>
    public abstract Harmony HarmonyPatcher { get; init; }

    /// <summary>
    /// Whether or not this feature has been enabled.
    /// </summary>
    public abstract bool Enabled { get; internal set; }

    /// <summary>
    /// The ID of this feature for pack loading purposes.
    /// </summary>
    public abstract string FeatureId { get; init; }

    /// <summary>
    /// Performs any actions this feature requires to be enabled. This often involves applying some Harmony patches.
    /// <returns>True if the feature was initialised successfully, and false if something failed.</returns>
    /// </summary>
    public abstract bool Enable();

    /// <summary>
    /// Disable this feature and all of its functionality.
    /// </summary>
    public abstract void Disable();
}
