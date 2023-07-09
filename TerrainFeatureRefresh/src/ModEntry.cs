using System;
using System.Collections.Generic;
using DecidedlyShared.Logging;
using DecidedlyShared.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using TerrainFeatureRefresh.Framework.Ui;

namespace TerrainFeatureRefresh;

public class ModEntry : Mod
{
    private Logger logger;
    private ModConfig config;

    public override void Entry(IModHelper helper)
    {
        this.logger = new Logger(this.Monitor);
        this.config = helper.ReadConfig<ModConfig>();

        helper.Events.Content.AssetRequested += this.ContentOnAssetRequested;
        helper.Events.Input.ButtonPressed += this.InputOnButtonPressed;
    }

    private void InputOnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        #if DEBUG
            if (e.IsDown(SButton.End))
            {
                Vector2 tile = Game1.currentCursorTile;
                GameLocation location = Game1.currentLocation;

                if (location.Objects.ContainsKey(tile))
                {
                    this.logger.Error($"Found object at {tile}.");

                    string objectId = location.Objects[tile].ItemId;

                    this.logger.Error("SObject ---------------------------------------------------------------------");
                    this.logger.Error($"Item ID for SObject: {ItemRegistry.QualifyItemId(objectId)}");
                    this.logger.Error($"SObject type: {ItemRegistry.GetData(objectId).ObjectType}");
                    this.logger.Error($"SObject category: {ItemRegistry.GetData(objectId).Category}");
                    this.logger.Error($"SObject name: {ItemRegistry.GetData(objectId).DisplayName}");
                    this.logger.Error($"SObject name: {ItemRegistry.GetData(objectId).DisplayName}");
                    this.logger.Error($"SObject is forage?: {location.Objects[tile].IsSpawnedObject}");
                }

                if (location.terrainFeatures.ContainsKey(tile))
                {
                    this.logger.Error($"Found TerrainFeature at {tile}.");

                    if (location.terrainFeatures[tile] is Flooring path)
                    {
                        string id = path.GetData().ItemId;

                        this.logger.Error("TerrainFeature --------------------------------------------------------------");
                        this.logger.Error($"TerrainFeature category: {ItemRegistry.GetData(id).Category}");
                        this.logger.Error($"Item ID for TerrainFeature: {ItemRegistry.QualifyItemId(id)}");
                        this.logger.Error($"TerrainFeature name: {ItemRegistry.GetData(id).DisplayName}");
                    }
                }
            }
            #endif

            if (e.IsDown(this.config.uiKey))
            {
                TfrMainMenu menu = new TfrMainMenu(this.logger);

                Game1.activeClickableMenu = menu;
            }
    }

    private void ContentOnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo("Mods/DecidedlyHuman/TFR/WindowSkin"))
            e.LoadFromModFile<Texture2D>("assets/window.png", AssetLoadPriority.Low);

        if (e.NameWithoutLocale.IsEquivalentTo("Mods/DecidedlyHuman/TFR/CloseButton"))
            e.LoadFromModFile<Texture2D>("assets/close-button.png", AssetLoadPriority.Low);

        if (e.NameWithoutLocale.IsEquivalentTo("Mods/DecidedlyHuman/TFR/Button"))
            e.LoadFromModFile<Texture2D>("assets/button.png", AssetLoadPriority.Low);

        if (e.NameWithoutLocale.IsEquivalentTo("Mods/DecidedlyHuman/TFR/Checkbox"))
            e.LoadFromModFile<Texture2D>("assets/checkbox.png", AssetLoadPriority.Low);
    }
}
