using System;
using System.Collections.Generic;
using System.Reflection;
using DecidedlyShared.Constants;
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
    /// The field containing methods to be patched, and the methods to patch them with.
    /// </summary>
    internal List<(MethodInfo, HarmonyMethod, AffixType)> FeatureMethods { get; set; }

    /// <summary>
    /// Enable this feature after adding patches via <see cref="IFeature.TryAddPatch(MethodInfo, HarmonyMethod, AffixType, out string)"/>.
    /// <returns>True if the feature was initialised successfully, and false if something failed.</returns>
    /// </summary>
    public bool Enable();

    /// <summary>
    /// Adds a new patch to the feature.
    /// </summary>
    public void AddPatch(MethodInfo originalMethod, HarmonyMethod methodToUse, AffixType type);

    /// <summary>
    /// Disable this feature and all of its functionality.
    /// </summary>
    public void Disable();

    /// <summary>
    /// The ID of this feature for pack loading purposes.
    /// </summary>
    public string FeatureId { get; internal set; }
}
