using System;
using System.Collections.Generic;
using System.Linq;
using DecidedlyShared.Logging;
using SpreadingCrops.Framework.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace SpreadingCrops;

public class ModEntry : Mod
{
    private Logger logger;

    public override void Entry(IModHelper helper)
    {
        this.logger = new Logger(this.Monitor);
        helper.Events.GameLoop.DayStarted += this.GameLoopOnDayStarted;
    }

    private void GameLoopOnDayStarted(object? sender, DayStartedEventArgs e)
    {
        List<HoeDirt> dirtWithCrops = new List<HoeDirt>();

        foreach (GameLocation location in Game1.locations)
        {
            foreach (HoeDirt cropDirt in location.terrainFeatures.Values.Where(tf => tf is HoeDirt hd && hd.crop is not null))
            {
                // At this point, we should only have HoeDirt with crops in them.
                dirtWithCrops.Add(cropDirt);
            }
        }

        foreach (HoeDirt dirt in dirtWithCrops)
        {
            Crop crop = dirt.crop;

            if (!crop.GetData().CustomFields.TryGetValue("DH.SpreadingCropData", out string keyValue))
                continue;

            SpreadingCropInfo info;

            try
            {
                info = new SpreadingCropInfo(keyValue);
            }
            catch (ArgumentException ae)
            {
                this.logger.Exception(ae);
                continue;
            }

            double random = Utility.getRandomDouble(0, 1);

            if (info.SpreadChance <= random)
            {

            }
        }
    }
}
