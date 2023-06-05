using System.Collections.Generic;
using System.Linq;
using Force.DeepCloner;
using StardewValley;

namespace TerrainFeatureRefresh.Framework;

public class FeatureProcessor
{
    private TfrSettings settings;
    private GameLocation location;
    private GameLocation generatedLocation;

    public FeatureProcessor(TfrSettings settings)
    {
        this.settings = settings;
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

    #region SObjects

    private void DoFences()
    {
        if (this.settings.fences.actionToTake == TfrAction.Regenerate)
        {
            List<SObject> objectsToDestroy = new List<SObject>();

            foreach (SObject obj in this.location.Objects.Values)
            {
                if (obj is not Fence)
                    continue;

                // We know it's a fence, so we add it to our list of things to be destroyed.
                objectsToDestroy.Add(obj);
            }

            // Now, we destroy.
            foreach (SObject obj in objectsToDestroy)
            {
                this.location.Objects.Remove(obj.TileLocation);
            }

            // And there's no need to regenerate new fences, so we're done.
        }
    }

    private void DoWeeds()
    {
        if (this.settings.weeds.actionToTake == TfrAction.Regenerate)
        {
            List<SObject> objectsToDestroy = new List<SObject>();

            foreach (SObject obj in this.location.Objects.Values)
            {
                if (!obj.Type.Equals("Litter") && !obj.Name.Equals("Weeds"))
                    continue;

                // We know it's a fence, so we add it to our list of things to be destroyed.
                objectsToDestroy.Add(obj);
            }

            // Now, we destroy.
            foreach (SObject obj in objectsToDestroy)
            {
                this.location.Objects.Remove(obj.TileLocation);
            }

            // Now we copy over to the main location.
            foreach (SObject obj in this.generatedLocation.Objects.Values)
            {
                if (!obj.Type.Equals("Litter") && !obj.Name.Equals("Weeds"))
                    continue;

                this.location.Objects.Add(obj.TileLocation, obj);
            }
        }
    }

    private void DoTwigs()
    {
        if (this.settings.twigs.actionToTake == TfrAction.Regenerate)
        {
            List<SObject> objectsToDestroy = new List<SObject>();

            foreach (SObject obj in this.location.Objects.Values)
            {
                if (!obj.Type.Equals("Litter") && !obj.Name.Equals("Twig"))
                    continue;

                // We know it's a fence, so we add it to our list of things to be destroyed.
                objectsToDestroy.Add(obj);
            }

            // Now, we destroy.
            foreach (SObject obj in objectsToDestroy)
            {
                this.location.Objects.Remove(obj.TileLocation);
            }

            // Now we copy over to the main location.
            foreach (SObject obj in this.generatedLocation.Objects.Values)
            {
                if (!obj.Type.Equals("Litter") && !obj.Name.Equals("Twig"))
                    continue;

                this.location.Objects.Add(obj.TileLocation, obj);
            }
        }
    }

    private void DoStones()
    {
        if (this.settings.stones.actionToTake == TfrAction.Regenerate)
        {
            List<SObject> objectsToDestroy = new List<SObject>();

            foreach (SObject obj in this.location.Objects.Values)
            {
                if (!obj.Type.Equals("Litter") && !obj.Name.Equals("Stone"))
                    continue;

                // We know it's a fence, so we add it to our list of things to be destroyed.
                objectsToDestroy.Add(obj);
            }

            // Now, we destroy.
            foreach (SObject obj in objectsToDestroy)
            {
                this.location.Objects.Remove(obj.TileLocation);
            }

            // Now we copy over to the main location.
            foreach (SObject obj in this.generatedLocation.Objects.Values)
            {
                if (!obj.Type.Equals("Litter") && !obj.Name.Equals("Stone"))
                    continue;

                this.location.Objects.Add(obj.TileLocation, obj);
            }
        }
    }

    private void DoForage()
    {
        if (this.settings.fences.actionToTake == TfrAction.Regenerate)
        {
            List<SObject> objectsToDestroy = new List<SObject>();

            foreach (SObject obj in this.location.Objects.Values)
            {
                if (!obj.Type.Equals("Litter") && !obj.Name.Equals("Twig"))
                    continue;

                // We know it's a fence, so we add it to our list of things to be destroyed.
                objectsToDestroy.Add(obj);
            }

            // Now, we destroy.
            foreach (SObject obj in objectsToDestroy)
            {
                this.location.Objects.Remove(obj.TileLocation);
            }

            // Now we copy over to the main location.
            foreach (SObject obj in this.generatedLocation.Objects.Values)
            {
                if (!obj.Type.Equals("Litter") && !obj.Name.Equals("Twig"))
                    continue;

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
