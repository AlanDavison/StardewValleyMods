using System;
using System.Collections.Generic;
using System.Linq;
using DecidedlyShared.Logging;
using DecidedlyShared.Utilities;
using HarmonyLib;
using MappingExtensionsAndExtraProperties.Functionality;
using MappingExtensionsAndExtraProperties.Models.FarmAnimals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Mods;

namespace MappingExtensionsAndExtraProperties.Features;

public class FarmAnimalSpawnsFeature : Feature
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
    private static Dictionary<string, Animal> animalData = new Dictionary<string, Animal>();
    private static Dictionary<FarmAnimal, Animal> spawnedAnimals = new Dictionary<FarmAnimal, Animal>();

    public FarmAnimalSpawnsFeature(Harmony harmony, string id, Logger logger, IModHelper helper)
    {
        this.Enabled = false;
        this.FeatureId = id;
        FarmAnimalSpawnsFeature.logger = logger;
        FarmAnimalSpawnsFeature.helper = helper;
        FarmAnimalSpawnsFeature.harmony = harmony;
    }

    public override void Enable()
    {
        try
        {
            FarmAnimalSpawnsFeature.harmony.Patch(
                AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.pet)),
                prefix: new HarmonyMethod(typeof(FarmAnimalSpawnsFeature),
                    nameof(FarmAnimalSpawnsFeature.FarmAnimalPetPrefix)));

            FarmAnimalSpawnsFeature.harmony.Patch(
                AccessTools.Method(typeof(AnimalPage), nameof(AnimalPage.FindAnimals)),
                postfix: new HarmonyMethod(typeof(FarmAnimalSpawnsFeature),
                    nameof(FarmAnimalSpawnsFeature.FindAnimals_Postfix)));

            FarmAnimalSpawnsFeature.harmony.Patch(
                AccessTools.Method(typeof(GameLocation), nameof(GameLocation.getAllFarmAnimals)),
                postfix: new HarmonyMethod(typeof(FarmAnimalSpawnsFeature),
                    nameof(FarmAnimalSpawnsFeature.GameLocationGetAllFarmAnimals_Postfix)));

            FarmAnimalSpawnsFeature.harmony.Patch(
                AccessTools.DeclaredMethod(typeof(FarmAnimal), nameof(FarmAnimal.draw)),
                prefix: new HarmonyMethod(typeof(FarmAnimalSpawnsFeature),
                    nameof(FarmAnimalSpawnsFeature.FarmAnimalDraw_Prefix)));
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

    public override void RegisterCallbacks()
    {
        FeatureManager.OnDayStartCallback += this.OnDayStart;
        FeatureManager.EarlyDayEndingCallback += this.OnEarlyDayEnding;
        FeatureManager.OnDisplayRenderedCallback += this.OnDisplayRenderedCallback;
    }

    private void OnDisplayRenderedCallback(object? sender, RenderedStepEventArgs e)
    {
        if (ModEntry.AnimalRemovalMode)
        {
            if (e.Step == RenderSteps.Overlays)
            {
                SpriteBatch sb = e.SpriteBatch;

                string warningMessage =
                    "IN MEEP EMERGENCY\nANIMAL REMOVAL MODE. IF\nYOU INTERACT WITH AN\nANIMAL IN THIS MODE,\nIT WILL BE REMOVED.\nRUN THE\nmeep_emergency_remove_animals\nCOMMAND AGAIN TO DISABLE IT.";
                Vector2 messageSize = Game1.dialogueFont.MeasureString(warningMessage);
                int centreX = Game1.uiViewport.Width / 2 - (int)messageSize.X / 2;
                int centreY = Game1.uiViewport.Height / 2 - (int)messageSize.Y / 2;
                sb.DrawString(Game1.dialogueFont, warningMessage, new Vector2(centreX + 2, centreY + 2), Color.Black * 0.75f);
                sb.DrawString(Game1.dialogueFont, warningMessage, new Vector2(centreX, centreY), Color.Blue);
            }
        }
    }

    private void OnEarlyDayEnding(object? sender, EventArgs e)
    {
        foreach (var animal in spawnedAnimals)
        {
            logger.Log($"Removing {animal.Key.displayName} of id {animal.Key.type} in {animal.Key.currentLocation.Name}.", LogLevel.Trace);
            animal.Key.currentLocation.animals.Remove(animal.Key.myID.Value);
        }
    }

    private void OnDayStart(object? sender, EventArgs e)
    {
        if (!Context.IsWorldReady || !Context.IsMainPlayer || !this.Enabled)
            return;

        // We technically only need to run this once, but this will be a super fast operation because it's cached.
        animalData = helper.GameContent.Load<Dictionary<string, Animal>>("MEEP/FarmAnimals/SpawnData");

        spawnedAnimals.Clear();

        // We need access to Game1.multiplayer. This is critical.
        Multiplayer multiplayer = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

        if (multiplayer is null)
        {
            // This is a catastrophic failure.
            logger.Log("Reflecting to get Game1.Multiplayer failed. As a result, we can't spawn any animals. This should never happen.", LogLevel.Error);

            return;
        }

        foreach (KeyValuePair<string, Animal> animal in animalData)
        {
            try
            {
                GameLocation targetLocation = Game1.getLocationFromName(animal.Value.LocationId);

                if (!GameStateQuery.CheckConditions(animal.Value.Condition, location: targetLocation))
                {
                    logger.Log($"Condition to spawn {animal.Value.DisplayName} was false. Skipping!", LogLevel.Trace);

                    continue;
                }

                if (targetLocation is null)
                {
                    logger.Log($"Couldn't parse location name \"{animal.Value.LocationId}\". Animal not spawned.",
                        LogLevel.Error);
                    continue;
                }

                // Sanity check time.
                if (animal.Value.SkinId is null)
                    animal.Value.SkinId = "";

                FarmAnimal babbyAnimal = new FarmAnimal(animal.Value.AnimalId, multiplayer.getNewID(), -1L)
                {
                    skinID = { animal.Value.SkinId },
                    age = { animal.Value.Age }
                };

                babbyAnimal.modData.Add("MEEP_Farm_Animal", "true");
                babbyAnimal.modData.Add("MEEP_Farm_Animal_ID", animal.Key);
                babbyAnimal.modData.Add("MEEP_Farm_Animal_Name", animal.Value.DisplayName);

                if (animal.Value.PortraitTexture is not null)
                    babbyAnimal.modData.Add("MEEP_Farm_Animal_Portrait", animal.Value.PortraitTexture);

                babbyAnimal.Position =
                    new Vector2(animal.Value.HomeTileX * Game1.tileSize, animal.Value.HomeTileY * Game1.tileSize);
                babbyAnimal.Name = animal.Value.DisplayName is null ? "No Name Boi" : animal.Value.DisplayName;

                // We got a location, so we're good to check our GameStateQuery condition.

                targetLocation.animals.Add(babbyAnimal.myID.Value, babbyAnimal);
                babbyAnimal.update(Game1.currentGameTime, targetLocation);
                babbyAnimal.ReloadTextureIfNeeded();
                babbyAnimal.allowReproduction.Value = false;
                babbyAnimal.wasPet.Value = true;
                spawnedAnimals.Add(babbyAnimal, animal.Value);

                logger.Log($"Animal {animal.Value.AnimalId} spawned in {targetLocation.Name}.", LogLevel.Info);
            }
            catch (Exception ex)
            {
                logger.Log($"Caught an exception spawning {animal.Value.AnimalId} spawned in {animal.Value.LocationId}. Skipping!");
            }
        }
    }

    public override bool ShouldChangeCursor(GameLocation location, int tileX, int tileY, out int cursorId)
    {
        cursorId = default;
        return false;
    }

    public static bool FarmAnimalDraw_Prefix(FarmAnimal __instance, SpriteBatch b)
    {
        try
        {
            if (Game1.CurrentEvent is not null)
            {
                if (__instance.modData.ContainsKey("MEEP_Farm_Animal"))
                {
                    return false;
                }
            }
        }
        catch (Exception e)
        {
            logger.Warn("Caught exception in FarmAnimal.draw() prefix:");
            logger.Exception(e);

            return true;
        }

        return true;
    }

    public static bool FarmAnimalPetPrefix(FarmAnimal __instance, Farmer who, bool is_auto_pet)
    {
        if (!enabled)
            return true;

        try
        {
            if (ModEntry.AnimalRemovalMode)
            {
                // I'm going absolutely bonkers with safety checks here.
                if (__instance.currentLocation is null || __instance.currentLocation.Animals is null)
                    return true;

                if (!__instance.currentLocation.Animals.TryGetValue(__instance.myID.Value, out FarmAnimal farmAnimal))
                    return true;

                if (farmAnimal.currentLocation is null)
                    return true;

                if (farmAnimal.currentLocation != Game1.player.currentLocation)
                    return true;

                __instance.currentLocation.Animals.Remove(__instance.myID.Value);

                string removalMessage =
                    $"REMOVED FARM ANIMAL \"{__instance.myID.Value}\" in {__instance.currentLocation.Name} because we were in animal removal mode.";
                logger.Log(removalMessage, LogLevel.Info);
                Game1.addHUDMessage(new HUDMessage(removalMessage));

                return false;
            }
        }
        catch (Exception e)
        {
            logger.Exception(e);
        }

        try
        {
            if (is_auto_pet)
                return true;

            if (who is null || who.currentLocation is null || __instance.currentLocation is null || __instance.currentLocation.Name is null)
                return true;

            if (who.currentLocation.Name != __instance.currentLocation.Name)
                return true;

            if (__instance.modData is null)
                return true;

            if (!__instance.modData.ContainsKey("MEEP_Farm_Animal"))
                return true;

            // In case we're a multiplayer client, we load the animal spawn data.
            if (!Context.IsMainPlayer)
                animalData = helper.GameContent.Load<Dictionary<string, Animal>>("MEEP/FarmAnimals/SpawnData");

            KeyValuePair<string, Animal> data = animalData.First(pair =>
                pair.Key == __instance.modData?["MEEP_Farm_Animal_ID"]);

            if ((bool)__instance.modData?.ContainsKey("MEEP_Farm_Animal_Portrait"))
            {
                if (data.Value is null)
                {
                    logger.Error("That animal was somehow not found in the spawn data.");

                    // It's important that we return false here, because we don't want the default
                    // animal interaction UI to appear regardless of this failure.
                    return false;
                }

                try
                {
                    Vector2 messageSize =
                        Geometry.GetLargestString(data.Value.PetMessage, Game1.dialogueFont);
                    NPC npc = new NPC();

                    npc.Portrait =
                        Game1.content.Load<Texture2D>(__instance.modData?["MEEP_Farm_Animal_Portrait"]);

                    npc.Name = data.Value.DisplayName;
                    npc.displayName = data.Value.DisplayName;

                    AnimalDialogueBox dialogueBoxWithPortrait = new AnimalDialogueBox(
                        new Dialogue(npc, "", string.Join(" ", data.Value.PetMessage.ToList())),
                        npc);

                    Game1.activeClickableMenu = dialogueBoxWithPortrait;
                }
                catch (Exception e)
                {
                    logger.Warn(
                        $"Portrait key for farm animal {data.Value.DisplayName} was present, but invalid.");
                }
            }
            else
            {
                DialogueBox dialogue = new DialogueBox(data.Value.PetMessage.ToList());
                Game1.activeClickableMenu = dialogue;
            }

            return false;

        }
        catch (Exception e)
        {
            logger.Error("Caught exception handling pet interaction for farm animal with MEEP's modData.");
            logger.Exception(e);
        }

        return false;
    }

    public static void GameLocationGetAllFarmAnimals_Postfix(GameLocation __instance, List<FarmAnimal> __result)
    {
        try
        {
            List<FarmAnimal> toRemove = new List<FarmAnimal>();

            foreach (FarmAnimal entry in __result)
            {
                if (entry.modData.ContainsKey("MEEP_Farm_Animal"))
                {
                    toRemove.Add(entry);
                }
            }

            foreach (FarmAnimal removing in toRemove)
            {
                if (__result.Contains(removing))
                {
                    __result.Remove(removing);
                }
            }
        }
        catch (Exception e)
        {
            logger.Exception(e);
        }
    }

    public static void FindAnimals_Postfix(AnimalPage __instance, List<AnimalPage.AnimalEntry> __result)
    {
        try
        {
            List<AnimalPage.AnimalEntry> toRemove = new List<AnimalPage.AnimalEntry>();

            foreach (AnimalPage.AnimalEntry entry in __result)
            {
                if (entry.Animal.modData.ContainsKey("MEEP_Farm_Animal"))
                {
                    toRemove.Add(entry);
                }
            }

            foreach (AnimalPage.AnimalEntry removing in toRemove)
            {
                if (__result.Contains(removing))
                {
                    __result.Remove(removing);
                }
            }
        }
        catch (Exception e)
        {
            logger.Exception(e);
        }
    }
}
