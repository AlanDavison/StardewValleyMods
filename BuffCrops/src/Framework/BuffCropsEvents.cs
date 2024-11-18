using System;
using System.Collections.Generic;
using BuffCrops.Framework.Helpers;
using DecidedlyShared.Logging;
using DecidedlyShared.Utilities;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.GameData.Buffs;
using StardewValley.GameData.Crops;
using StardewValley.GameData.GiantCrops;
using StardewValley.TerrainFeatures;

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
        Dictionary<string, float> spaceCoreSkillContributions = new Dictionary<string, float>();

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

                if (customFields.ContainsKey("DH.BuffCrops.SpaceCoreSkillContribution"))
                {
                    string keyValue = crop.GetData().CustomFields["DH.BuffCrops.SpaceCoreSkillContribution"];
                    if (!this.parser.TryGetSpaceCoreSkillData(keyValue, out Dictionary<string, float> contributions))
                        return false;

                    spaceCoreSkillContributions =
                        DictionaryHelpers.AddDictionaries(spaceCoreSkillContributions, contributions);

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

                if (customFields.ContainsKey("DH.BuffCrops.SpaceCoreSkillContribution"))
                {
                    string keyValue = giantCrop.GetData().CustomFields["DH.BuffCrops.SpaceCoreSkillContribution"];
                    if (!this.parser.TryGetSpaceCoreSkillData(keyValue, out Dictionary<string, float> contributions))
                        return;

                    spaceCoreSkillContributions =
                        DictionaryHelpers.AddDictionaries(spaceCoreSkillContributions, contributions);

                }
            });

            Locations.ForEachPlantableInterior(location =>
            {
                foreach (TerrainFeature tf in location.terrainFeatures.Values)
                {
                    if (tf is not HoeDirt dirt)
                        continue;
                    if (dirt.crop is null)
                        continue;

                    Crop crop = dirt.crop;
                    CropData cropData = crop.GetData();

                    // TODO: For some reason, the customfields data contains the SpaceCore skill field, but *not*
                    // the field for the relevant normal skills. Very, very odd.

                    if (cropData is null)
                        return;

                    Dictionary<string, string> customFields = cropData.CustomFields;

                    if (customFields is null)
                        return;

                    if (customFields.ContainsKey("DH.BuffCrops.BuffContribution"))
                    {
                        string keyValue = crop.GetData().CustomFields["DH.BuffCrops.BuffContribution"];
                        this.parser.TryGetBuffAttributesData(keyValue, out BuffAttributesData? newData);

                        if (newData is null)
                            continue;

                        buffBuilder.AddBuff(newData);
                    }

                    if (customFields.ContainsKey("DH.BuffCrops.SpaceCoreSkillContribution"))
                    {
                        string keyValue = crop.GetData().CustomFields["DH.BuffCrops.SpaceCoreSkillContribution"];
                        if (!this.parser.TryGetSpaceCoreSkillData(keyValue,
                                out Dictionary<string, float> contributions))
                            continue;

                        spaceCoreSkillContributions =
                            DictionaryHelpers.AddDictionaries(spaceCoreSkillContributions, contributions);

                    }
                }

                foreach (TerrainFeature clump in location.resourceClumps)
                {
                    if (clump is not GiantCrop giantCrop)
                        continue;

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

                    if (customFields.ContainsKey("DH.BuffCrops.SpaceCoreSkillContribution"))
                    {
                        string keyValue = giantCrop.GetData().CustomFields["DH.BuffCrops.SpaceCoreSkillContribution"];
                        if (!this.parser.TryGetSpaceCoreSkillData(keyValue, out Dictionary<string, float> contributions))
                            return;

                        spaceCoreSkillContributions =
                            DictionaryHelpers.AddDictionaries(spaceCoreSkillContributions, contributions);

                    }
                }
            });

            Buff buff = buffBuilder.Build(
                I18n.Dh_BuffCrops_BuffName(),
                I18n.Dh_BuffCrops_BuffDescription(),
                Buff.ENDLESS,
                Game1.content.Load<Texture2D>("Mods/DecidedlyHuman/BuffCrops/CropBuffImage"),
                0
                );

            foreach (var kvp in spaceCoreSkillContributions)
            {

                buff.customFields.Add($"spacechase0.SpaceCore.SkillBuff.{kvp.Key}", MathF.Round(kvp.Value).ToString());

            }

            Game1.player.applyBuff(buff);
        }
        catch (Exception e)
        {
            this.logger.Exception(e);
        }
    }
}

