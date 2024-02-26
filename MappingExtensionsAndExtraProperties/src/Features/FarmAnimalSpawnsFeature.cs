using System;
using DecidedlyShared.Logging;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace MappingExtensionsAndExtraProperties.Features;

public class FarmAnimalSpawns : Feature
{
    public override string FeatureId { get; init; }
    public override Harmony HarmonyPatcher { get; init; }
    public override bool AffectsCursorIcon { get; init; }
    public override int CursorId { get; init; }
    private static bool enabled;
    public override bool Enabled
    {
        get => enabled;
        internal set { enabled = value; }
    }

    private static Logger logger;
    private static Harmony harmony;
    private static IModHelper helper;

    public FarmAnimalSpawns(Harmony harmony, Logger logger, IModHelper helper)
    {
        FarmAnimalSpawns.logger = logger;
        FarmAnimalSpawns.helper = helper;
        FarmAnimalSpawns.harmony = harmony;
    }

    public override void Enable()
    {
        try
        {
            this.HarmonyPatcher.Patch(
                AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction)),
                postfix: new HarmonyMethod(typeof(LetterFeature),
                    nameof(SetMailFlagFeature.GameLocation_CheckAction_Postfix)));
        }
        catch (Exception e)
        {
            logger.Exception(e);
        }

        this.Enabled = true;
    }

    public override void Disable()
    {
        this.Enabled = false;
    }

    public override void RegisterCallbacks() {}

    public override bool ShouldChangeCursor(GameLocation location, int tileX, int tileY, out int cursorId)
    {
        cursorId = default;
        return false;
    }

    public static bool FarmAnimalPetPrefix(FarmAnimal __instance, Farmer who, bool is_auto_pet)
    {
        // If we're dealing with one of our spawned animals, we display a nice message.
        if (spawnedAnimals.ContainsKey(__instance))
        {
            string hudMessage;

            if (spawnedAnimals[__instance].PetMessage is not null &&
                !spawnedAnimals[__instance].PetMessage.Equals(""))
                hudMessage = translation.Get(spawnedAnimals[__instance].PetMessage);
            else
                hudMessage = $"{__instance.displayName} looks very happy today!";

            if (!Game1.doesHUDMessageExist(hudMessage))
            {
                Game1.addHUDMessage(new HUDMessage(hudMessage) {noIcon = true });
            }

            return false;
        }

        return true;
    }
}
