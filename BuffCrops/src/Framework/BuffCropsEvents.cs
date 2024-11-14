using System;
using System.Collections.Generic;
using BuffCrops.Framework.Helpers;
using DecidedlyShared.Logging;
using DecidedlyShared.Utilities;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.GameData.Buffs;
using StardewValley.GameData.Crops;
using StardewValley.GameData.GiantCrops;

namespace BuffCrops.Framework;

public class BuffCropsEvents
{
    private Logger logger;
    private Parser parser;

    public BuffCropsEvents(Logger logger)
    {
        this.logger = logger;
        this.parser = new Parser(logger);
    }

    public void DoDayStart()
    {
        BuffBuilder buffBuilder = new BuffBuilder();

        try
        {
            Utility.ForEachCrop(crop =>
            {
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

                    if (newData is not null)
                        buffBuilder.AddBuff(newData);
                }
            });

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

