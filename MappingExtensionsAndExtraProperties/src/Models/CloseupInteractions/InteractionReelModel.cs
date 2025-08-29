using System;
using System.Collections.Generic;
using System.Linq;
using DecidedlyShared.Logging;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using StardewValley.Extensions;

namespace MappingExtensionsAndExtraProperties.Models.CloseupInteractions;

public record InteractionReelModel(
    SortedList<int, InteractionImageModel> images,
    SortedList<int, InteractionTextModel> text,
    InteractionSoundModel soundCue)
{
    public static bool TryMakeFromProperties(
        string[] imageProperties, string[] textProperties,
        string soundCue, Logger logger, out InteractionReelModel? model)
    {
        model = null;
        SortedList<int, InteractionImageModel> imageList = new SortedList<int, InteractionImageModel>();
        SortedList<int, InteractionTextModel> textList = new SortedList<int, InteractionTextModel>();

        foreach (string prop in imageProperties)
        {
            string key = prop.Split(" ")[0];
            string property = string.Join(" ", prop.Split(" ")[Range.StartAt(1)]);

            if (!int.TryParse(key[Index.FromEnd(1)].ToString(), out int propertyNumber))
            {
                logger.Log($"Couldn't get property number from property {prop}. Skipping.");
                continue;
            }

            if (!imageList.TryAdd(propertyNumber, new InteractionImageModel(prop)))
            {
                logger.Error($"There's already an image property of number {propertyNumber} here. You should report this.");
                continue;
            }
        }

        foreach (string prop in textProperties)
        {
            string key = prop.Split(" ")[0];
            string property = string.Join(" ", prop.Split(" ")[Range.StartAt(1)]);

            if (!int.TryParse(key[Index.FromEnd(1)].ToString(), out int propertyNumber))
            {
                logger.Log($"Couldn't get property number from property {prop}. Skipping.");
                continue;
            }

            if (!textList.TryAdd(propertyNumber, new InteractionTextModel(prop)))
            {
                logger.Error($"There's already a text property of number {propertyNumber} here. You should report this.");
                continue;
            }
        }

        model = new InteractionReelModel(imageList, textList, new InteractionSoundModel(soundCue));

        return true;
    }
}
