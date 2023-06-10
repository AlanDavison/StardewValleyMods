using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using DecidedlyShared.Logging;
using Force.DeepCloner;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace TerrainFeatureRefresh.Framework;

public class FeatureProcessor
{
    private TfrSettings settings;
    private GameLocation location;
    private GameLocation generatedLocation;
    private Logger logger;

    public FeatureProcessor(TfrSettings settings, Logger logger)
    {
        this.settings = settings;
        this.logger = logger;
    }

    public void Execute()
    {
        this.location = Game1.currentLocation;
        this.generatedLocation =
            new GameLocation(Game1.currentLocation.mapPath.Value, Game1.currentLocation.Name);

        this.DoFences();
        this.DoWeeds();
        this.DoTwigs();
        this.DoStones();
        this.DoForage();
        this.DoGrass();
        this.DoWildTrees();
        this.DoFruitTrees();
        this.DoPaths();
        this.DoHoeDirt();
        this.DoCrops();
        this.DoBushes();
        this.DoStumps();
        this.DoLogs();
        this.DoBoulders();
        this.DoMeteorites();
    }

    private void LogRemoval(SObject obj)
    {
        this.logger.Log($"Removed {obj.Name}:{obj.DisplayName} in current map.", LogLevel.Info);
    }

    private void LogAddition(SObject obj, Vector2 tile)
    {
        this.logger.Log($"Adding {obj.Name}:{obj.DisplayName} to {tile} in current map.", LogLevel.Info);
    }

    private void LogRemoval(TerrainFeature tf)
    {

    }

    private List<SObject> GetSObjects(GameLocation location, Func<SObject, bool> predicate)
    {
        List<SObject> objects = new List<SObject>();

        foreach (SObject obj in location.Objects.Values.Where(predicate))
            objects.Add(obj);

        return objects;
    }

    #region SObjects

    private void DoFences()
    {
        if (this.settings.fences.actionToTake == TfrAction.Regenerate)
        {
            List<SObject> objectsToDestroy = this.GetSObjects(this.location, (SObject o) => o is Fence);;

            // Now, we destroy.
            foreach (SObject obj in objectsToDestroy)
            {
                this.LogRemoval(obj);
                this.location.Objects.Remove(obj.TileLocation);
            }

            // And there's no need to regenerate new fences, so we're done.
        }
    }

    private void DoWeeds()
    {
        if (this.settings.weeds.actionToTake == TfrAction.Regenerate)
        {
            List<SObject> objectsToDestroy = this.GetSObjects(this.location,
                (SObject o) => o.Type.Equals("Litter") && o.Name.Equals("Weeds"));

            // Now, we destroy.
            foreach (SObject obj in objectsToDestroy)
            {
                this.LogRemoval(obj);
                this.location.Objects.Remove(obj.TileLocation);
            }

            // Now we copy over to the main location.
            foreach (SObject obj in this.generatedLocation.Objects.Values)
            {
                if (!obj.Type.Equals("Litter") || !obj.Name.Equals("Weeds"))
                    continue;

                if (this.location.Objects.ContainsKey(obj.TileLocation))
                    continue;

                this.LogAddition(obj, obj.TileLocation);
                this.location.Objects.Add(obj.TileLocation, obj);
            }
        }
    }

    private void DoTwigs()
    {
        if (this.settings.twigs.actionToTake == TfrAction.Regenerate)
        {
            List<SObject> objectsToDestroy = this.GetSObjects(this.location,
                (SObject o) => o.Type.Equals("Litter") && o.Name.Equals("Twig"));

            // Now, we destroy.
            foreach (SObject obj in objectsToDestroy)
            {
                this.LogRemoval(obj);
                this.location.Objects.Remove(obj.TileLocation);
            }

            // Now we copy over to the main location.
            foreach (SObject obj in this.generatedLocation.Objects.Values)
            {
                if (!obj.Type.Equals("Litter") || !obj.Name.Equals("Twig"))
                    continue;

                if (this.location.Objects.ContainsKey(obj.TileLocation))
                    continue;

                this.LogAddition(obj, obj.TileLocation);
                this.location.Objects.Add(obj.TileLocation, obj);
            }
        }
    }

    private void DoStones()
    {
        if (this.settings.stones.actionToTake == TfrAction.Regenerate)
        {
            List<SObject> objectsToDestroy = this.GetSObjects(this.location,
                (SObject o) => o.Type.Equals("Litter") && o.Name.Equals("Stone"));

            // Now, we destroy.
            foreach (SObject obj in objectsToDestroy)
            {
                this.LogRemoval(obj);
                this.location.Objects.Remove(obj.TileLocation);
            }

            // Now we copy over to the main location.
            foreach (SObject obj in this.generatedLocation.Objects.Values)
            {
                if (!obj.Type.Equals("Litter") || !obj.Name.Equals("Stone"))
                    continue;

                if (this.location.Objects.ContainsKey(obj.TileLocation))
                    continue;

                this.LogAddition(obj, obj.TileLocation);
                this.location.Objects.Add(obj.TileLocation, obj);
            }
        }
    }

    private void DoForage()
    {
        if (this.settings.forage.actionToTake == TfrAction.Regenerate)
        {
            List<SObject> objectsToDestroy = this.GetSObjects(this.location,
                (SObject o) => o.IsSpawnedObject);

            // Now, we destroy.
            foreach (SObject obj in objectsToDestroy)
            {
                this.LogRemoval(obj);
                this.location.Objects.Remove(obj.TileLocation);
            }

            // Now we copy over to the main location.
            foreach (SObject obj in this.generatedLocation.Objects.Values)
            {
                if (!obj.IsSpawnedObject)
                    continue;

                if (this.location.Objects.ContainsKey(obj.TileLocation))
                    continue;

                this.LogAddition(obj, obj.TileLocation);
                this.location.Objects.Add(obj.TileLocation, obj);
            }
        }
    }

    #endregion

    #region TerrainFeatures

    private void DoGrass()
    {
        if (this.settings.grass.actionToTake == TfrAction.Regenerate)
        {
            // Do the thing.
        }
    }

    private void DoWildTrees()
    {
        if (this.settings.wildTrees.actionToTake == TfrAction.Regenerate)
        {
            // Do the thing.
        }
    }

    private void DoFruitTrees()
    {
        if (this.settings.fruitTrees.actionToTake == TfrAction.Regenerate)
        {
            // Do the thing.
        }
    }

    private void DoPaths()
    {
        if (this.settings.paths.actionToTake == TfrAction.Regenerate)
        {
            // Do the thing.
        }
    }

    private void DoHoeDirt()
    {
        if (this.settings.hoeDirt.actionToTake == TfrAction.Regenerate)
        {
            // Do the thing.
        }
    }

    private void DoCrops()
    {
        // For crops, I think I want to give the player back any seeds/fertiliser that was in the soil?
        if (this.settings.crops.actionToTake == TfrAction.Regenerate)
        {
            // Do the thing.
        }
    }

    private void DoBushes()
    {
        if (this.settings.bushes.actionToTake == TfrAction.Regenerate)
        {
            // Do the thing.
        }
    }

    #endregion

    #region ResourceClumps

    private void DoStumps()
    {
        if (this.settings.stumps.actionToTake == TfrAction.Regenerate)
        {
            // Do the thing.
        }
    }

    private void DoLogs()
    {
        if (this.settings.logs.actionToTake == TfrAction.Regenerate)
        {
            // Do the thing.
        }
    }

    private void DoBoulders()
    {
        if (this.settings.boulders.actionToTake == TfrAction.Regenerate)
        {
            // Do the thing.
        }
    }

    private void DoMeteorites()
    {
        if (this.settings.meteorites.actionToTake == TfrAction.Regenerate)
        {
            // Do the thing.
        }
    }

    #endregion
}
