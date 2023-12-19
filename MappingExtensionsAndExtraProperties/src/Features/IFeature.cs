using HarmonyLib;

namespace MappingExtensionsAndExtraProperties.Features;

public interface IFeature
{
    /// <summary>
    /// The <see cref="Harmony">Harmony</see> reference used to apply this feature's patches.
    /// </summary>
    internal Harmony HarmonyPatcher { get; set; }

    /// <summary>
    /// Whether or not this feature has been enabled (its patches applied).
    /// </summary>
    public bool Enabled { get; internal set; }

    /// <summary>
    /// Enable this feature, and patch any required methods.
    /// <returns>True if the feature was initialised successfully, and false if something failed.</returns>
    /// </summary>
    public bool TryInitialise(out string failureMessage);

    /// <summary>
    /// Disable this feature and all of its functionality.
    /// </summary>
    public void Disable();

    /// <summary>
    /// The ID of this feature for pack loading purposes.
    /// </summary>
    public string FeatureId { get; internal set; }
}
