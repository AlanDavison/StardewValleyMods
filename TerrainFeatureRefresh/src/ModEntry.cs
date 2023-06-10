using System.Collections.Generic;
using DecidedlyShared.Logging;
using DecidedlyShared.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace TerrainFeatureRefresh;

public class ModEntry : Mod
{
    private Logger logger;

    public override void Entry(IModHelper helper)
    {
        this.logger = new Logger(this.Monitor);

        helper.Events.Content.AssetRequested += this.ContentOnAssetRequested;
        helper.Events.Input.ButtonPressed += (sender, args) =>
        {
            if (args.IsDown(SButton.OemSemicolon))
            {
                // No good. Gets the same, active instance, not a new one.
                // GameLocation location = Game1.getLocationFromName(Game1.currentLocation.Name);
                // GameLocation curLoc = Game1.currentLocation;

                // TODO: Change this to use GameLocation.CreateGameLocation(string id) instead.
                GameLocation location =
                    new GameLocation(Game1.currentLocation.mapPath.Value, Game1.currentLocation.Name);
                // GameLocation curLoc = Game1.currentLocation;

                List<Vector2> objectsToRemove = new();
                foreach (Vector2 tile in Game1.currentLocation.Objects.Keys)
                {
                    objectsToRemove.Add(tile);
                }

                foreach (Vector2 tile in objectsToRemove)
                {
                    Game1.currentLocation.Objects.Remove(tile);
                }

                List<Vector2> terrainFeaturesToRemove = new();
                foreach (Vector2 tile in Game1.currentLocation.terrainFeatures.Keys)
                {
                    terrainFeaturesToRemove.Add(tile);
                }

                List<LargeTerrainFeature> largeTerrainFeaturesToRemove = new();
                foreach (LargeTerrainFeature feature in Game1.currentLocation.largeTerrainFeatures)
                {
                    largeTerrainFeaturesToRemove.Add(feature);
                }

                foreach (LargeTerrainFeature tile in largeTerrainFeaturesToRemove)
                {
                    Game1.currentLocation.largeTerrainFeatures.Remove(tile);
                }

                foreach (Vector2 tile in terrainFeaturesToRemove)
                {
                    Game1.currentLocation.terrainFeatures.Remove(tile);
                }

                foreach (Vector2 tile in location.Objects.Keys)
                {
                    if (location.Objects.TryGetValue(tile, out SObject obj))
                    {
                        Game1.currentLocation.Objects.Add(tile, obj);
                    }
                }

                foreach (Vector2 tile in location.terrainFeatures.Keys)
                {
                    if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature tf))
                        Game1.currentLocation.terrainFeatures.Add(tile, tf);
                }

                foreach (LargeTerrainFeature feature in location.largeTerrainFeatures)
                {
                    Game1.currentLocation.largeTerrainFeatures.Add(feature);
                }
            }

            if (args.IsDown(SButton.End))
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

            if (args.IsDown(SButton.Home))
            {
                // HBoxElement mainHBox =
                //     new HBoxElement("Main Box", Rectangle.Empty, this.logger, DrawableType.SlicedBox);
                // VBoxElement leftColumn = new VBoxElement("Left Column", new Rectangle(0, 0, 100, 400), this.logger,
                //     DrawableType.None);
                // VBoxElement rightColumn = new VBoxElement("Right Column", new Rectangle(0, 0, 100, 600), this.logger,
                //     DrawableType.None);
                //
                // HBoxElement checkbox = new HBoxElement("Checkbox", Rectangle.Empty, this.logger, DrawableType.None);
                // UiElement checkboxImage = new UiElement(
                //     "Checkbox One Image",
                //     Rectangle.Empty,
                //     this.logger,
                //     DrawableType.Texture,
                //     Game1.mouseCursors,
                //     new Rectangle(227, 425, 9, 9));
                // TextElement checkboxLabel = new TextElement(
                //     "Checkbox One Label",
                //     Rectangle.Empty,
                //     this.logger,
                //     400,
                //     "Checkbox One",
                //     Game1.dialogueFont,
                //     DrawableType.None
                // );
                // checkbox.AddChild(checkboxImage);
                // checkbox.AddChild(checkboxLabel);
                //
                // HBoxElement checkboxTwo = new HBoxElement("Checkbox Two", Rectangle.Empty, this.logger, DrawableType.None);
                // UiElement checkboxImageTwo = new UiElement(
                //     "Checkbox Two Image",
                //     Rectangle.Empty,
                //     this.logger,
                //     DrawableType.Texture,
                //     Game1.mouseCursors,
                //     new Rectangle(227, 425, 9, 9));
                // TextElement checkboxLabelTwo = new TextElement(
                //     "Checkbox Two Label",
                //     Rectangle.Empty,
                //     this.logger,
                //     400,
                //     "Checkbox Two",
                //     Game1.dialogueFont,
                //     DrawableType.None
                // );
                // checkboxTwo.AddChild(checkboxImageTwo);
                // checkboxTwo.AddChild(checkboxLabelTwo);
                //
                // leftColumn.AddChild(checkbox);
                // rightColumn.AddChild(checkboxTwo);
                //
                // mainHBox.AddChild(leftColumn);
                // mainHBox.AddChild(rightColumn);

                Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(640, 480);
                TfrMainMenu menu = new TfrMainMenu((int)topLeft.X, (int)topLeft.Y, 640, 440, this.logger);

                Game1.activeClickableMenu = menu;
            }
        };
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
