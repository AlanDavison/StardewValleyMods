using System.Collections.Generic;
using BuffCrops.Framework;
using BuffCrops.Framework.Helpers;
using DecidedlyShared.Utilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Buffs;
using StardewValley.GameData.Crops;
using StardewValley.GameData.GiantCrops;

namespace BuffCrops;

public class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        helper.Events.GameLoop.DayStarted += this.GameLoopOnDayStarted;
    }

    private void GameLoopOnDayStarted(object? sender, DayStartedEventArgs e)
    {
        BuffBuilder buffBuilder = new BuffBuilder();

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
                string keyValue = crop.GetData()?.CustomFields["DH.BuffCrops.BuffContribution"];
                Parser.TryGetBuffAttributesData(keyValue, out BuffAttributesData? newData);

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
                string keyValue = giantCrop.GetData()?.CustomFields["DH.BuffCrops.BuffContribution"];
                Parser.TryGetBuffAttributesData(keyValue, out BuffAttributesData? newData);

                if (newData is not null)
                    buffBuilder.AddBuff(newData);
            }
        });

        Buff buff = buffBuilder.Build();
        buff.millisecondsDuration = Buff.ENDLESS;
        buff.description = "Your crops have blessed you!";
        buff.displaySource = "Your Crops";
        Game1.player.applyBuff(buff);
    }
}
