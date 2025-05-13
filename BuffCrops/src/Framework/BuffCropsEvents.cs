using System;
using System.Collections.Generic;
using BuffCrops.Framework.Helpers;
using BuffCrops.src.Framework.Helpers;
using DecidedlyShared.Logging;
using DecidedlyShared.Utilities;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Buffs;
using StardewValley.GameData.Crops;
using StardewValley.GameData.GiantCrops;

namespace BuffCrops.Framework;

public class BuffCropsEvents
{
    private Logger logger;
    private Parser parser;
    private bool debugMode = false;

    public BuffCropsEvents(Logger logger, bool debugMode)
    {
        this.logger = logger;
        this.parser = new Parser(logger);
        this.debugMode = debugMode;
    }

    public void DoDayStart()
    {
        BuffBuilder buffBuilder = new BuffBuilder();

        try
        {
            Utility.ForEachCrop(crop =>
            {
                if (crop.currentPhase.Value < crop.phaseDays.Count - 1)
                {
                    if (this.debugMode)
                        this.logger.Log($"Crop in {crop.currentLocation.Name} not contributing to buffs because it's not fully grown.", LogLevel.Info);

                    return true;
                }

                CropData cropData = crop.GetData();

                if (cropData is null)
                    return true;

                Dictionary<string, string> customFields = cropData.CustomFields;

                if (customFields is null)
                    return true;

                if (customFields.ContainsKey("DH.BuffCrops.BuffContribution"))
                {
                    string keyValue = crop.GetData().CustomFields["DH.BuffCrops.BuffContribution"];
                    this.parser.TryGetBuffAttributesData(keyValue, out BuffAttributesData? newData);

                    if (newData is null)
                        return true;

                    if (this.debugMode)
                        this.logger.Log($"Crop in {crop.currentLocation.Name} contributing {BuffUtils.GetBuffInformation(newData)} to buffs.", LogLevel.Info);

                    buffBuilder.AddBuff(newData);
                }

                return true;
            });

            Locations.ForEachGiantCrop(giantCrop =>
            {
                GiantCropData cropData = giantCrop.GetData();

                if (cropData is null)
                    return;

                Dictionary<string, string> customFields = cropData.CustomFields;

                if (customFields is null)
                    return;

                if (customFields.ContainsKey("DH.BuffCrops.BuffContribution"))
                {
                    string keyValue = giantCrop.GetData().CustomFields["DH.BuffCrops.BuffContribution"];
                    this.parser.TryGetBuffAttributesData(keyValue, out BuffAttributesData? newData);

                    if (newData is null)
                        return;

                    if (this.debugMode)
                        this.logger.Log($"Crop in {giantCrop.Location.Name} contributing {BuffUtils.GetBuffInformation(newData)} to buffs.", LogLevel.Info);

                    buffBuilder.AddBuff(newData);

                }
            });

            if (buffBuilder.Empty)
                return;

            Buff buff = buffBuilder.Build(
                I18n.Dh_BuffCrops_BuffName(),
                I18n.Dh_BuffCrops_BuffDescription(),
                Buff.ENDLESS,
                Game1.content.Load<Texture2D>("Mods/DecidedlyHuman/BuffCrops/CropBuffImage"),
                0
                );

            Game1.player.applyBuff(buff);
        }
        catch (Exception e)
        {
            this.logger.Exception(e);
        }
    }
}

