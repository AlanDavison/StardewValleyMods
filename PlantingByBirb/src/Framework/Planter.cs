using System;
using System.Collections.Generic;
using System.Linq;
using DecidedlyShared.Logging;
using PlantingByBirb.Framework.Model;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.TerrainFeatures;

namespace PlantingByBirb.Framework;

public class Planter
{
    private Logger logger;
    private Dictionary<string, BirbSeed> seedData;
    private List<string> excludedSeedIds;
    private IModHelper helper;

    public void ProcessDayEnding(object? sender, DayEndingEventArgs e)
    {
        this.seedData =
            this.helper.GameContent.Load<Dictionary<string, BirbSeed>>("DecidedlyHuman/PlantingByBirb/SeedData");

        foreach (GameLocation location in Game1.locations)
        {
            if (location.GetData().CanPlantHere is null)
                continue;

            if (location.GetData().CanPlantHere!.Value)
                this.DoBirbs(location);
        }
    }

    private void DoBirbs(GameLocation location)
    {
        foreach (TerrainFeature tf in location.terrainFeatures.Values)
        {
            if (tf is HoeDirt hd)
            {
                if (hd.crop is null)
                {
                    this.DropSeed(hd);
                }
            }
        }
    }

    private void DropSeed(HoeDirt hd)
    {
        if (!Game1.random.NextBool(0.25f)) // TODO: Make this a config.
            return;

        this.logger.Log("We're trying to plant a seed.", LogLevel.Info);

        if (this.TryGetSeedToPlant(this.seedData, out BirbSeed? seed))
        {
            if (!hd.canPlantThisSeedHere(seed.SeedId))
                return;

            if (hd.plant(seed.SeedId, Game1.MasterPlayer, false))
            {
                this.logger.Log($"Seed {seed.SeedId} planted on tile X: {hd.Tile.X}, Y: {hd.Tile.Y} in {hd.Location.Name}.", LogLevel.Info);
            }
        }
    }

    public bool TryGetSeedToPlant(Dictionary<string, BirbSeed> seedData, out BirbSeed? seedToPlant)
    {
        int totalWeights = 0;
        seedToPlant = null;

        foreach (KeyValuePair<string, BirbSeed> seed in seedData)
        {
            totalWeights += seed.Value.PlantingWeight;
        }

        if (totalWeights <= 0)
            return false;

        int selection = Game1.random.Next(totalWeights);
        int cumulativeTotal = 0;

        foreach (KeyValuePair<string, BirbSeed> seed in seedData)
        {
            cumulativeTotal += seed.Value.PlantingWeight;

            if (cumulativeTotal > selection)
            {
                seedToPlant = seed.Value;
                seedToPlant.SetSeedId(seed.Key);

                if (this.IsSeedExcluded(seedToPlant))
                    continue;

                return true;
            }
        }

        return false;
    }

    private bool IsSeedExcluded(BirbSeed seed)
    {
        Item seedItem = ItemRegistry.Create(ItemRegistry.QualifyItemId(seed.SeedId), allowNull: true);

        if (seedItem is null)
        {
            this.logger.Warn($"Seed ID was invalid: {seed.SeedId}. Is it a correct item ID?");

            return true;
        }

        if (seedItem.modData is null)
            return true;

        if (seedItem.modData.ContainsKey("DH_BirbPlanting_ExcludeSeed"))
            return true;

        return false;
    }

    public Planter(Logger logger, IModHelper helper)
    {
        this.logger = logger;
        this.helper = helper;
    }
}
