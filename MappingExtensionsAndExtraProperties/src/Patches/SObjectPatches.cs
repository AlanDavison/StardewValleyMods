using System.Collections.Generic;
using DecidedlyShared.Logging;
using DecidedlyShared.Utilities;
using MappingExtensionsAndExtraProperties.Functionality;
using MappingExtensionsAndExtraProperties.Models.TileProperties;
using MappingExtensionsAndExtraProperties.Utils;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace MappingExtensionsAndExtraProperties.Patches;

public class SObjectPatches
{
    private static Logger? logger = null;
    private static TilePropertyHandler? tileProperties = null;
    private static Properties propertyUtils;

    public static void InitialisePatches(Logger logger, TilePropertyHandler tileProperties)
    {
        SObjectPatches.logger = logger;
        SObjectPatches.tileProperties = tileProperties;
        propertyUtils = new Properties(logger);
    }

    // NOTE: COMPLETELY INACTIVE CURRENTLY.
    public static void SObject_PerformUseAction(SObject __instance, GameLocation location)
    {
        HashSet<string> tags = __instance.GetContextTags();
        bool hasInteractionProperty = false;
        bool hasText = false;
        bool hasSound = false;
        bool hasLetter = false;
        bool hasLetterType = false;
        CloseupInteractionImage parsedProperty = new CloseupInteractionImage();
        CloseupInteractionText parsedTextProperty = new CloseupInteractionText();
        CloseupInteractionSound parsedSoundProperty = new CloseupInteractionSound();
        LetterText parsedLetterProperty = new LetterText();
        LetterType parsedLetterTypeProperty = new LetterType();

        foreach (string tag in tags)
        {
            if (tag.Contains("MEEP"))
            {
                logger.Log($"Working with tag {tag}.", LogLevel.Info);

                // We're dealing with an abomination of a MEEP in-item property.
                if (tag.Contains(CloseupInteractionImage.PropertyKey))
                    hasInteractionProperty = Parsers.TryParseIncludingKey(tag, out parsedProperty);
                if (tag.Contains(CloseupInteractionText.PropertyKey))
                    hasText = Parsers.TryParseIncludingKey(tag, out parsedTextProperty);
                if (tag.Contains(CloseupInteractionSound.PropertyKey))
                    hasSound = Parsers.TryParseIncludingKey(tag, out parsedSoundProperty);
                // if (tag.Contains(LetterText.PropertyKey))
                //     hasLetter = Parsers.TryParseIncludingKey(tag, out parsedLetterProperty);
                // if (tag.Contains(LetterType.PropertyKey))
                //     hasLetterType = Parsers.TryParseIncludingKey(tag, out parsedLetterTypeProperty);

            }
        }

        logger.Log($"hasProperty: {hasInteractionProperty}.", LogLevel.Info);
        logger.Log($"hasText: {hasText}.", LogLevel.Info);
        logger.Log($"hasSound: {hasSound}.", LogLevel.Info);

        if (hasInteractionProperty)
        {
            CloseupInteraction.CreateInteractionUi(parsedProperty,
                logger,
                hasText ? parsedTextProperty : null,
                hasSound ? parsedSoundProperty : null
            );
        }
        else if (hasLetter)
        {
            LetterViewerMenu letterViewer = new LetterViewerMenu(parsedLetterProperty.Text, "Test");

            if (hasLetterType)
                letterViewer.whichBG = parsedLetterTypeProperty.BgType;

            Game1.activeClickableMenu = letterViewer;
        }
    }
}
