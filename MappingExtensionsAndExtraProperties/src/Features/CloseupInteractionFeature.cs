using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DecidedlyShared.Logging;
using DecidedlyShared.Ui;
using DecidedlyShared.Utilities;
using HarmonyLib;
using MappingExtensionsAndExtraProperties.Functionality;
using MappingExtensionsAndExtraProperties.Models.TileProperties;
using MappingExtensionsAndExtraProperties.Utils;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Triggers;
using xTile.ObjectModel;

namespace MappingExtensionsAndExtraProperties.Features;

public class CloseupInteractionFeature : Feature
{
    public sealed override Harmony HarmonyPatcher { get; init; }
    public sealed override bool AffectsCursorIcon { get; init; }
    public sealed override int CursorId { get; init; }
    private string[] tilePropertiesControlled = [
        "MEEP_CloseupInteraction_Image",
        "MEEP_CloseupInteraction_Image_1",
        "MEEP_CloseupInteraction_Text",
        "MEEP_CloseupInteraction_Sound"];

    public sealed override bool Enabled
    {
        get => enabled;
        internal set => enabled = value;
    }
    private static bool enabled;

    public sealed override string FeatureId { get; init; }
    private static TilePropertyHandler tileProperties;
    private static Properties propertyUtils;
    private static Logger logger;

    public CloseupInteractionFeature(Harmony harmony, string id, Logger logger, TilePropertyHandler tilePropertyHandler, Properties propertyUtils)
    {
        this.Enabled = false;
        this.HarmonyPatcher = harmony;
        this.FeatureId = id;
        CloseupInteractionFeature.logger = logger;
        CloseupInteractionFeature.tileProperties = tilePropertyHandler;
        CloseupInteractionFeature.propertyUtils = propertyUtils;
        this.AffectsCursorIcon = true;
        this.CursorId = 5;

        GameLocation.RegisterTileAction("MEEP_CloseupInteraction_Image", this.DoCloseupInteraction);
        GameLocation.RegisterTileAction("MEEP_CloseupInteractionReel", this.DoCloseupReel);
        GameLocation.RegisterTileAction("MEEP_CloseupInteractionReel_Furniture", this.DoCloseupReel);
        TriggerActionManager.RegisterAction("MEEP_CloseupInteraction_Action", this.CloseupInteractionAction);
        TriggerActionManager.RegisterAction("MEEP_CloseupInteractionReel_Action", this.CloseupInteractionReelAction);
    }

    private bool CloseupInteractionReelAction(string[] args, TriggerActionContext context, out string error)
    {
        error = null;

        if (!General.TryCombineDictionaryPairs(context.Data.CustomFields, out string[] properties))
        {
            string errorContext = TriggerActionUtils.GatherContext("Failed to merge properties for parsing. This should really never happen.",
                context);
            logger.Error(errorContext);
            error = errorContext;

            return false;
        }

        if (propertyUtils.TryGetInteractionReel(
                () => { return properties.Where(s => s.StartsWith(CloseupInteractionImage.PropertyKey)).ToList(); },
                () => { return properties.Where(s => s.StartsWith(CloseupInteractionText.PropertyKey)).ToList(); },
                out List<MenuPage> menuPages))
        {
            string? soundCue = properties.FirstOrDefault(s => s.StartsWith(CloseupInteractionSound.PropertyKey));
            if (Parsers.TryParseIncludingKey(soundCue, out CloseupInteractionSound parsedSoundProperty))
            {
                soundCue = parsedSoundProperty.CueName;
            }

            logger.Log($"Number of pages: {menuPages.Count}", LogLevel.Info);

            CloseupInteraction.DoCloseupReel(menuPages, logger, soundCue ?? "bigSelect");
        }
        else
        {
            logger.Error($"Problem parsing closeup interaction reel from trigger action {context.Data.Id}.");
        }

        return true;
    }


    private bool CloseupInteractionAction(string[] args, TriggerActionContext context, out string error)
    {
        // TODO: Refactor this to use the same data model method as the reels.

        error = null;
        CloseupInteractionText? textProperty = null;
        CloseupInteractionSound? soundProperty = null;

        if (!General.TryCombineDictionaryPairs(context.Data.CustomFields, out string[] properties))
        {
            GatherAndLogError(context, out error);

            return false;
        }

        if (!this.TryGetInteractionProperties(properties, logger, out string[] imageProperties, out string[] textProperties,
                out string? soundProp))
        {
            GatherAndLogError(context, out error);

            return false;
        }

        if (imageProperties.Length > 1) {}

        Parsers.TryParseIncludingKey(imageProperties[0], out CloseupInteractionImage imageProperty);
        if (textProperties is not null && Parsers.TryParseIncludingKey(textProperties[0], out CloseupInteractionText parsedTextProperty))
        {
            textProperty = parsedTextProperty;
        }

        if (soundProp is not null && Parsers.TryParseIncludingKey(soundProp, out CloseupInteractionSound parsedSoundProperty))
        {
            soundProperty = parsedSoundProperty;
        }

        CloseupInteraction.DoCloseupInteraction(imageProperty, textProperty, soundProperty, logger);

        return true;
    }

    private static void GatherAndLogError(TriggerActionContext context, out string error)
    {
        string errorContext = TriggerActionUtils.GatherContext("Failed to merge properties for parsing. This should really never happen.",
            context);
        logger.Error(errorContext);
        error = errorContext;
    }

    private bool TryGetInteractionProperties(string[] properties,
        Logger logger,
        out string[] imageProperties,
        out string[]? textProperties,
        out string? soundProperty)
    {
        imageProperties = [];
        textProperties = [];
        soundProperty = null;

        try
        {
            imageProperties = properties.Where(s => s.StartsWith("MEEP_CloseupInteraction_Image")).ToArray();
            textProperties = properties.Where(s => s.StartsWith("MEEP_CloseupInteraction_Text")).ToArray();
            soundProperty = properties.FirstOrDefault(s => s.StartsWith("MEEP_CloseupInteraction_Sound"));
        }
        catch (Exception e)
        {
            return false;
        }

        if (imageProperties.Length < 1)
        {
            logger.Error($"imageProperties length was less than 1: {imageProperties.Length}");
            foreach (string property in properties)
            {
                logger.Log($"Property: {property}", LogLevel.Info);
            }
            return false;
        }

        if (textProperties.Length < 1)
            textProperties = null;


        return true;
    }

    private bool DoCloseupReel(GameLocation location, string[] propertyArgs, Farmer player, Point tile)
    {
        if (!enabled)
            return false;

        foreach (string propertyArg in propertyArgs)
        {
            logger.Log($"Arg: {propertyArg}", LogLevel.Alert);
        }

        bool isOnFurniture = propertyArgs[0].Equals("MEEP_CloseupInteractionReel_Furniture");

        if (propertyUtils.TryGetInteractionReel(tile.X, tile.Y, location,
                CloseupInteractionImage.PropertyKey,
                false,
                out List<MenuPage> pages))
        {
            string cueName = "bigSelect";

            // Now we check for a sound interaction property.
            if (tileProperties.TryGetBackProperty(tile.X, tile.Y, location, CloseupInteractionSound.PropertyKey,
                    out PropertyValue closeupSoundProperty))
            {
                if (Parsers.TryParse(closeupSoundProperty.ToString(),
                        out CloseupInteractionSound parsedSoundProperty))
                {
                    cueName = parsedSoundProperty.CueName;
                }
            }

            CloseupInteraction.DoCloseupReel(pages, logger, cueName);

            return true;
        }

        return false;
    }

    private bool DoCloseupInteraction(GameLocation location, string[] propertyArgs, Farmer player, Point tile)
    {
        if (!enabled)
            return false;

        string joinedArgs = propertyArgs.Join(delimiter: " ");
        CloseupInteraction.DoCloseupInteraction(location, tile.X, tile.Y, joinedArgs, logger);

        return true;
    }

    public override void Enable()
    {
        this.Enabled = true;
    }

    public override void Disable()
    {
        this.Enabled = false;
    }

    public override void RegisterCallbacks() {}

    public override bool ShouldChangeCursor(GameLocation location, int tileX, int tileY, out int cursorId)
    {
        cursorId = default;

        if (!enabled)
            return false;

        for (int i = 0; i < this.tilePropertiesControlled.Length; i++)
        {
            if (tileProperties.TryGetBackProperty(tileX, tileY, Game1.currentLocation, this.tilePropertiesControlled[i],
                    out PropertyValue _))
            {
                cursorId = this.CursorId;
                return true;
            }
        }

        return false;
    }
}
