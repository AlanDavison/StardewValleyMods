using HarmonyLib;

namespace MappingExtensionsAndExtraProperties.Features;

public class CloseupInteraction : IFeature
{
    public Harmony HarmonyPatcher { get; set; }
    public bool Enabled { get; set; }
    public string FeatureId { get; set; }

    public CloseupInteraction(Harmony harmony)
    {
        this.HarmonyPatcher = harmony;
    }

    public bool TryInitialise(out string failureMessage)
    {


        this.Enabled = true;
    }

    public void Disable()
    {
        throw new System.NotImplementedException();
    }

    public override int GetHashCode()
    {
        return this.FeatureId.GetHashCode();
    }
}
