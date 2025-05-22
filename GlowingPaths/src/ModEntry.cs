using HarmonyLib;
using StardewModdingAPI;

namespace GlowingPaths;

public class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        var harmony = new Harmony(this.ModManifest.UniqueID);

        harmony.Patch(
                AccessTools.Method(typeof(SObject), nameof(SObject.placementAction)),
                prefix: new HarmonyMethod(typeof(Patches), nameof(PlacementAction_Prefix)));
    }

    public static bool PlacementAction_Prefix(SObject __instance, GameLocation location, int x, int y, Farmer who)
    {


        return true;
    }
}
