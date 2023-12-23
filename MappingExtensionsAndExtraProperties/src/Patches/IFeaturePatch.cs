using System;
using System.ComponentModel.DataAnnotations;
using DecidedlyShared.Logging;
using StardewModdingAPI;

namespace MappingExtensionsAndExtraProperties.Patches;

public interface IFeaturePatch
{
    internal string FeatureId { get; set; }
    internal static Func<bool> GetEnabled { get; set; }
    internal static Logger Logger { get; set; }
    internal static IModHelper Helper { get; set; }
}
