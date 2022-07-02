using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SmartBuilding.APIs;
using SmartBuilding.UI;
using SmartBuilding.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.SDKs;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace SmartBuilding
{
    public class ModEntry : Mod
    {
        // SMAPI gubbins.
        private static IModHelper helper = null!;
        private static IMonitor monitor = null!;
        private static Logger logger = null!;
        private static ModConfig config = null!;

        private Dictionary<Vector2, ItemInfo> tilesSelected = new Dictionary<Vector2, ItemInfo>();
        private Texture2D itemBox = null!;
        private DrawingUtils drawingUtils;
        private IdentificationUtils identificationUtils;
        private PlacementUtils placementUtils;

        private string betaVersion = "This is a prerelease beta version of Smart Building 1.7.x.";

        // Rectangle drawing.
        private Vector2? startTile = null;
        private Vector2? endTile = null;
        private List<Vector2> rectTiles = new List<Vector2>();

        private Item rectangleItem;

        private bool currentlyDrawing = false;
        private bool currentlyErasing = false;
        private bool currentlyPlacing = false;
        private bool buildingMode = false;

        // UI gubbins
        private Texture2D toolButtonsTexture;
        private ButtonActions buttonActions;
        private Toolbar gameToolbar;
        private int currentMouseX;
        private int currentMouseY;
        private ToolMenu toolMenuUi;
        
        // State stuff
        private Options.ItemStowingModes previousStowingMode;

        // Debug stuff to make my life less painful when going through my pre-release checklist.
        private ConsoleCommand command = null!;

        // Mod integrations.
        private IMoreFertilizersAPI? moreFertilizersApi;
        private IDynamicGameAssetsApi? dgaApi;
        private bool moreFertilisersInstalled;
        private bool dgaInstalled;
        private Type dgaCustomObjectType;

        #region Properties
        
        private bool BuildingMode
        {
            get { return buildingMode; }
            set
            {
                buildingMode = value;
                HarmonyPatches.Patches.CurrentlyInBuildMode = value;

                if (!buildingMode) // If this is now false, we want to clear the tiles list, refund everything, and kill our UI.
                {
                    if (toolMenuUi != null)
                        toolMenuUi.Enabled = false;

                    ClearPaintedTiles();

                    // And, if our UI exists, we kill it.
                    if (Game1.onScreenMenus.Contains(toolMenuUi))
                        Game1.onScreenMenus.Remove(toolMenuUi);

                    // And set our active tool and layer to none.
                    ModState.ActiveTool = null;
                    ModState.SelectedLayer = null;
                    
                    // Then, finally, set the stowing mode back to what it used to be.
                    Game1.options.stowingMode = previousStowingMode;
                }
                else
                {
                    // First, we create our list of buttons.
                    List<ToolButton> toolButtons = new List<ToolButton>()
                    {
                        new ToolButton(ButtonId.Draw, ButtonType.Tool, buttonActions.DrawClicked, 
                            I18n.SmartBuilding_Buttons_Draw_Tooltip(), toolButtonsTexture),
                        new ToolButton(ButtonId.Erase, ButtonType.Tool, buttonActions.EraseClicked, 
                            I18n.SmartBuilding_Buttons_Erase_Tooltip(), toolButtonsTexture),
                        new ToolButton(ButtonId.FilledRectangle, ButtonType.Tool, buttonActions.FilledRectangleClicked, 
                            I18n.SmartBuilding_Buttons_FilledRectangle_Tooltip(), toolButtonsTexture),
                        new ToolButton(ButtonId.Insert, ButtonType.Tool, buttonActions.InsertClicked, 
                            I18n.SmartBuilding_Buttons_Insert_Tooltip(), toolButtonsTexture), 
                        new ToolButton(ButtonId.ConfirmBuild, ButtonType.Function, buttonActions.ConfirmBuildClicked, 
                            I18n.SmartBuilding_Buttons_ConfirmBuild_Tooltip(), toolButtonsTexture), 
                        new ToolButton(ButtonId.ClearBuild, ButtonType.Function, buttonActions.ClearBuildClicked, 
                            I18n.SmartBuilding_Buttons_ClearBuild_Tooltip(), toolButtonsTexture), 
                        new ToolButton(ButtonId.DrawnLayer, ButtonType.Layer, buttonActions.DrawnLayerClicked, 
                            I18n.SmartBuilding_Buttons_LayerDrawn_Tooltip(), toolButtonsTexture, TileFeature.Drawn), 
                        new ToolButton(ButtonId.ObjectLayer, ButtonType.Layer, buttonActions.ObjectLayerClicked, 
                            I18n.SmartBuilding_Buttons_LayerObject_Tooltip(), toolButtonsTexture, TileFeature.Object), 
                        new ToolButton(ButtonId.TerrainFeatureLayer, ButtonType.Layer, buttonActions.TerrainFeatureLayerClicked, 
                            I18n.SmartBuilding_Buttons_LayerTerrainfeature_Tooltip(), toolButtonsTexture, TileFeature.TerrainFeature), 
                        new ToolButton(ButtonId.FurnitureLayer, ButtonType.Layer, buttonActions.FurnitureLayerClicked, 
                            I18n.SmartBuilding_Buttons_LayerFurniture_Tooltip(), toolButtonsTexture, TileFeature.Furniture), 
                    };

                    // If we're enabling building mode, we create our UI, and set it to enabled.
                    toolMenuUi = new ToolMenu(logger, toolButtonsTexture, toolButtons);
                    toolMenuUi.Enabled = true;

                    // Then, if it isn't already in onScreenMenus, we add it.
                    if (!Game1.onScreenMenus.Contains(toolMenuUi))
                    {
                        Game1.onScreenMenus.Add(toolMenuUi);
                    }
                    
                    // First we save the current item stowing mode
                    previousStowingMode = Game1.options.stowingMode;
                    
                    // Then we set it to off to avoid a strange stuttery drawing issue.
                    Game1.options.stowingMode = Options.ItemStowingModes.Off;
                }
            }
        }

        private bool CurrentlyDrawing
        {
            get { return currentlyDrawing; }
            set { currentlyDrawing = value; }
        }

        private bool CurrentlyErasing
        {
            get { return currentlyErasing; }
            set { currentlyErasing = value; }
        }

        private bool CurrentlyPlacing
        {
            get { return currentlyPlacing; }
            set { currentlyPlacing = value; }
        }
        
        #endregion

        #region Asset Loading Gubbins

        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.Name.IsEquivalentTo("Mods/SmartBuilding/ToolButtons"))
                e.LoadFromModFile<Texture2D>("assets/Buttons.png", AssetLoadPriority.Medium);

            // if (e.Name.IsEquivalentTo("Mods/SmartBuilding/WindowSkin"))
            //     e.LoadFromModFile<Texture2D>("assets/WindowSkin.png", AssetLoadPriority.Medium);
        }

        #endregion

        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
            ModEntry.helper = helper;
            monitor = Monitor;
            logger = new Logger(monitor);
            config = ModEntry.helper.ReadConfig<ModConfig>();
            buttonActions = new ButtonActions(this); // Ew, no. Fix this ugly nonsense later.
            drawingUtils = new DrawingUtils();
            identificationUtils = new IdentificationUtils(ModEntry.helper, logger, config, dgaApi, moreFertilizersApi, placementUtils);
            placementUtils = new PlacementUtils(config);

            // This is where we'll register with GMCM.
            ModEntry.helper.Events.GameLoop.GameLaunched += GameLaunched;

            // This is fired whenever input is changed, so we check for input here.
            ModEntry.helper.Events.Input.ButtonsChanged += OnInput;

            // This is used to have the queued builds draw themselves in the world.
            ModEntry.helper.Events.Display.RenderedWorld += RenderedWorld;

            // This is a huge mess, and is used to draw the building mode HUD, and build queue if enabled.
            ModEntry.helper.Events.Display.RenderedHud += RenderedHud;

            // This is purely for our rectangle quantity drawing.
            ModEntry.helper.Events.Display.Rendered += Rendered;

            ModEntry.helper.Events.GameLoop.UpdateTicking += OnUpdateTicking;

            // If the screen is changed, clear our painted tiles, because currently, placing objects is done on the current screen.
            ModEntry.helper.Events.Player.Warped += (sender, args) =>
            {
                ClearPaintedTiles();
                BuildingMode = false;
                currentlyDrawing = false;
                HarmonyPatches.Patches.CurrentlyInBuildMode = false;
                HarmonyPatches.Patches.AllowPlacement = false;
            };

            // ModEntry.helper.Events.Content.AssetRequested += OnAssetRequested;

            toolButtonsTexture = ModEntry.helper.ModContent.Load<Texture2D>(Path.Combine("assets", "Buttons.png"));
            itemBox = ModEntry.helper.GameContent.Load<Texture2D>("LooseSprites/tailoring");

            Harmony harmony = new Harmony(ModManifest.UniqueID);

            // I'll need more patches to ensure you can't interact with chests, etc., while building. Should be simple. 
            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.placementAction)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches.Patches), nameof(HarmonyPatches.Patches.PlacementAction_Prefix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.checkForAction)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches.Patches), nameof(HarmonyPatches.Patches.Chest_CheckForAction_Prefix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(FishPond), nameof(FishPond.doAction)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches.Patches), nameof(HarmonyPatches.Patches.FishPond_DoAction_Prefix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(StorageFurniture), nameof(StorageFurniture.checkForAction)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches.Patches), nameof(HarmonyPatches.Patches.StorageFurniture_DoAction_Prefix)));
            
            harmony.Patch(
                original: AccessTools.Method(typeof(StorageFurniture), nameof(StorageFurniture.checkForAction)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches.Patches), nameof(HarmonyPatches.Patches.StorageFurniture_DoAction_Prefix)));

#if !DEBUG
            ModEntry.helper.ConsoleCommands.Add("sb_binding_ui", "This will open up Smart Building's binding UI.", command.BindingUI);
#endif
        }
        
        private void GameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            // We need a reference to the toolbar in order to detect if the cursor is over it or not.
            foreach (IClickableMenu menu in Game1.onScreenMenus)
            {
                if (menu is Toolbar)
                    gameToolbar = (Toolbar)menu;
            }
            
            // Then we register with GMCM.
            RegisterWithGmcm();
            
            // Set up our mod integrations.
            SetupModIntegrations();
            
            // Set up our console commands.
            command = new ConsoleCommand(logger, this, dgaApi, identificationUtils);
            this.Helper.ConsoleCommands.Add("sb_test", I18n.SmartBuilding_Commands_Debug_SbTest(), command.TestCommand);
            this.Helper.ConsoleCommands.Add("sb_identify_all_items", I18n.SmartBuilding_Commands_Debug_SbIdentifyItems(), command.IdentifyItemsCommand);
            
            // Then get the initial state of the item stowing mode setting.
            previousStowingMode = Game1.options.stowingMode;
        }

        /// <summary>
        /// SMAPI's <see cref="IGameLoopEvents.UpdateTicking"/> event.
        /// </summary>
        private void OnUpdateTicking(object? sender, UpdateTickingEventArgs e)
        {
            //logger.Log($"Should clicks be blocked: {ModState.BlockMouseInteractions}");

            if (toolMenuUi != null)
            {
                if (toolMenuUi.Enabled)
                {
                    MouseState mouseState = Game1.input.GetMouseState();
                    currentMouseX = mouseState.X;
                    currentMouseY = mouseState.Y;

                    currentMouseX = (int)MathF.Floor(currentMouseX / Game1.options.uiScale);
                    currentMouseY = (int)MathF.Floor(currentMouseY / Game1.options.uiScale);

                    // We need to process our custom middle click held event.
                    if (mouseState.MiddleButton == ButtonState.Pressed && Game1.oldMouseState.MiddleButton == ButtonState.Pressed)
                        toolMenuUi.MiddleMouseHeld(currentMouseX, currentMouseY);
                    if (mouseState.MiddleButton == ButtonState.Released)
                        toolMenuUi.MiddleMouseReleased(currentMouseX, currentMouseY);

                    // Do our hover event.
                    toolMenuUi.DoHover(currentMouseX, currentMouseY);

                    // toolMenuUi.middleMouseHeld((int)MathF.Round(mouseState.X * Game1.options.uiScale), (int)MathF.Round(mouseState.X * Game1.options.uiScale));

                    toolMenuUi.SetCursorHoverState(currentMouseX, currentMouseY);

                    // We also need to manually call the click event, because by default, it'll only work if the bounds of the IClickableMenu contain the cursor.
                    // We specifically do not want the bounds to be expanded to include the side layer buttons, however, because that will be far too large a boundary.

                    // TODO: Refactor this to only use config binding. This will require my own previousInputState thing, but that's not a big deal.
                    if (mouseState.LeftButton == ButtonState.Pressed || config.HoldToDraw.IsDown())
                    {
                        toolMenuUi.ReceiveLeftClick(currentMouseX, currentMouseY);
                    }
                }
            }
        }

        /// <summary>
        /// SMAPI's <see cref="IInputEvents.ButtonsChanged"> event.
        /// </summary>
        private void OnInput(object? sender, ButtonsChangedEventArgs e)
        {
            // If the world isn't ready, we definitely don't want to do anything.
            if (!Context.IsWorldReady)
                return;

            // If a menu is up, we don't want any of our controls to do anything.
            if (Game1.activeClickableMenu != null)
                return;

            if (config.EnableDebugControls)
            {
                if (config.IdentifyItem.JustPressed())
                {
                    // We now want to identify the currently held item.
                    Farmer player = Game1.player;

                    if (player.CurrentItem != null)
                    {
                        if (player.CurrentItem is not Tool)
                        {
                            ItemType type = identificationUtils.IdentifyItemType((SObject)player.CurrentItem);
                            Item item = player.CurrentItem;

                            logger.Log($"{I18n.SmartBuilding_Message_ItemName()}");
                            logger.Log($"\t{item.Name}");
                            logger.Log($"{I18n.SmartBuilding_Message_ItemParentSheetIndex()}");
                            logger.Log($"\t{item.ParentSheetIndex}");
                            logger.Log($"{I18n.SmartBuilding_Message_ItemCategory()}");
                            logger.Log($"\t{item.Category}");
                            logger.Log($"{I18n.SmartBuilding_Message_ItemType()}");
                            logger.Log($"\t{(item as SObject).Type}");
                            logger.Log($"{I18n.SmartBuilding_Message_ItemSmartBuildingType()}");
                            logger.Log($"\t{type}.");
                            logger.Log("");
                        }
                    }
                }

                if (config.IdentifyProducer.JustPressed())
                {
                    // We're trying to identify the type of producer under the cursor.
                    GameLocation here = Game1.currentLocation;
                    Vector2 targetTile = Game1.currentCursorTile;

                    if (here.objects.ContainsKey(targetTile))
                    {
                        SObject producer = here.objects[targetTile];
                        ProducerType type = identificationUtils.IdentifyProducer(producer);

                        logger.Log($"Identified producer {producer.Name} as {type}.");
                        logger.Log($"{I18n.SmartBuilding_Message_ProducerBeingIdentified()} {producer.Name}");
                        logger.Log($"{I18n.SmartBuilding_Message_IdentifiedProducerType()}: {type}");
                    }
                }
            }

            // If the player presses to engage build mode, we flip the bool.
            if (config.EngageBuildMode.JustPressed())
            {
                // The BuildingMode property takes care of clearing the build queue.
                BuildingMode = !BuildingMode;
            }

            // If the player is drawing placeables in the world. 
            if (config.HoldToDraw.IsDown())
            {
                if (buildingMode)
                {
                    // We don't want to do anything here if we're hovering over the menu, or the toolbar.
                    if (!ModState.BlockMouseInteractions && !gameToolbar.isWithinBounds(currentMouseX, currentMouseY))
                    {
                        // First, we need to make sure there even is a tool active.
                        if (ModState.ActiveTool.HasValue)
                        {
                            // There is, so we want to determine exactly which tool we're working with.
                            switch (ModState.ActiveTool)
                            {
                                case ButtonId.Draw:
                                    // We don't want to draw if the cursor is in the negative.
                                    if (Game1.currentCursorTile.X < 0 || Game1.currentCursorTile.Y < 0)
                                        return;

                                    AddTile(Game1.player.CurrentItem, Game1.currentCursorTile);
                                    break;
                                case ButtonId.Erase:
                                    if (ModState.SelectedLayer.HasValue)
                                    {
                                        DemolishOnTile(Game1.currentCursorTile, ModState.SelectedLayer.Value);
                                        EraseTile(Game1.currentCursorTile);
                                    }
                                    break;
                                case ButtonId.FilledRectangle:
                                    // This is a split method and is hideous, but this is the best I can think of for now.

                                    CurrentlyDrawing = true;
                                    rectangleItem = Game1.player.CurrentItem;

                                    if (startTile == null)
                                    {
                                        // If the start tile hasn't yet been set, then we want to set that.
                                        startTile = Game1.currentCursorTile;
                                    }

                                    endTile = Game1.currentCursorTile;

                                    rectTiles = CalculateRectangle(startTile.Value, endTile.Value, rectangleItem);

                                    break;
                                case ButtonId.Insert:
                                    AddItem(Game1.player.CurrentItem, Game1.currentCursorTile);
                                    break;
                            }
                        }
                    }
                }
            }
            else if (config.HoldToDraw.GetState() == SButtonState.Released)
            {
                if (ModState.ActiveTool.HasValue)
                {
                    if (ModState.ActiveTool == ButtonId.FilledRectangle)
                    {
                        // We need to process the key up stuff for the filled rectangle.

                        // The rectangle drawing key was released, so we want to calculate the tiles within, and set CurrentlyDrawing to false.

                        if (startTile.HasValue && endTile.HasValue)
                        {
                            List<Vector2> tiles = CalculateRectangle(startTile.Value, endTile.Value, rectangleItem);

                            foreach (Vector2 tile in tiles)
                            {
                                AddTile(rectangleItem, tile);
                            }

                            startTile = null;
                            endTile = null;
                            rectTiles.Clear();
                        }
                    }
                }

                // Otherwise, the key is up, meaning we want to indicate we're not currently drawing.
                CurrentlyDrawing = false;
            }

            // if (config.HoldToDrawRectangle.IsDown())
            // {
            //     // If we're holding our rectangle modifier, we do things a little differently.
            //
            //     if (buildingMode)
            //     {
            //         CurrentlyDrawing = true;
            //         rectangleItem = Game1.player.CurrentItem;
            //
            //         if (startTile == null)
            //         {
            //             // If the start tile hasn't yet been set, then we want to set that.
            //             startTile = Game1.currentCursorTile;
            //         }
            //
            //         endTile = Game1.currentCursorTile;
            //
            //         rectTiles = CalculateRectangle(startTile.Value, endTile.Value, rectangleItem);
            //     }
            // }
            // else
            // {
            //
            // }

            // if (config.HoldToErase.IsDown())
            // {
            //     if (buildingMode)
            //     {
            //         // We update this to set both our mod state, and patch bool.
            //         CurrentlyErasing = true;
            //
            //         EraseTile(Game1.currentCursorTile);
            //     }
            // }
            // else
            // {
            //     // The key is no longer held, so we set this to false.
            //     CurrentlyErasing = false;
            // }

            // if (config.HoldToInsert.IsDown())
            // {
            //     if (buildingMode)
            //     {
            //         // We're in building mode, but we also want to ensure the setting to enable this is on.
            //         if (config.EnableInsertingItemsIntoMachines)
            //         {
            //             // If it is, we proceed to flag that we're placing items.
            //             CurrentlyPlacing = true;
            //
            //             AddItem(Game1.player.CurrentItem, Game1.currentCursorTile);
            //         }
            //     }
            // }
            // else
            // {
            //     CurrentlyPlacing = false;
            // }

            // if (config.ConfirmBuild.JustPressed())
            // {
            //     ConfirmBuild();
            // }

            // if (config.PickUpObject.IsDown())
            // {
            //     if (buildingMode) // If we're in building mode, we demolish the tile, indicating we're dealing with an SObject.
            //         DemolishOnTile(Game1.currentCursorTile, TileFeature.Object);
            // }
            //
            // if (config.PickUpFloor.IsDown())
            // {
            //     if (buildingMode) // If we're in building mode, we demolish the tile, indicating we're dealing with TerrainFeature.
            //         DemolishOnTile(Game1.currentCursorTile, TileFeature.TerrainFeature);
            // }
            //
            // if (config.PickUpFurniture.IsDown())
            // {
            //     if (buildingMode) // If we're in building mode, we demolish the tile, indicating we're dealing with Furniture.
            //         DemolishOnTile(Game1.currentCursorTile, TileFeature.Furniture);
            // }
        }
        
        /// <summary>
        /// SMAPI's <see cref="IDisplayEvents.RenderedWorld"/> event.
        /// </summary>
        private void Rendered(object? sender, RenderedEventArgs e)
        {
            if (buildingMode)
            {
                // Now, we render our rectangle quantity amount.
                if (rectTiles != null)
                {
                    foreach (Vector2 tile in rectTiles)
                    {
                        // IClickableMenu.drawTextureBox(
                        //     b,
                        //     Game1.getMouseX() - 144,
                        //     Game1.getMouseY() - 32 - 16,
                        //     64 + 32,
                        //     128 + 16,
                        //     Color.White
                        // );

                        // Utility.drawTextWithShadow(
                        //     e.SpriteBatch,
                        //     "rectTiles.Count.ToString()",
                        //     Game1.smallFont, 
                        //     new Vector2(Game1.getMouseX(), Game1.getMouseY()),
                        //     Color.Black,
                        //     1f);
                        
                        Utility.drawTinyDigits(
                            rectTiles.Count,
                            e.SpriteBatch,
                            // new Vector2(100, 100),
                            new Vector2(Game1.getMouseX() + 38, Game1.getMouseY() + 86),
                            3f,
                            -10f,
                            Color.White);

                        // Utility.drawTextWithShadow(
                        //     e.SpriteBatch,
                        //     rectTiles.Count.ToString(),
                        //     Game1.dialogueFont,
                        //     new Vector2(Game1.getMouseX(), Game1.getMouseY()),
                        //     Color.Black);

                        // Utility.drawTextWithShadow(
                        //     e.SpriteBatch,
                        //     "—",
                        //     Game1.dialogueFont,
                        //     new Vector2(Game1.getMouseX(), Game1.getMouseY()),
                        //     Color.Black);
                        //
                        // Utility.drawTextWithShadow(
                        //     e.SpriteBatch,
                        //     rectangleItem.Stack.ToString(),
                        //     Game1.dialogueFont,
                        //     new Vector2(Game1.getMouseX(), Game1.getMouseY()),
                        //     Color.Black);

                        // e.SpriteBatch.DrawString(Game1.dialogueFont, rectTiles.Count.ToString(), new Vector2(Game1.getMouseX() - 64, Game1.getMouseY() - 32), Color.White);
                        // e.SpriteBatch.DrawString(Game1.dialogueFont, "—", new Vector2(Game1.getMouseX() - 64, Game1.getMouseY()), Color.White);
                        // e.SpriteBatch.DrawString(Game1.dialogueFont, rectangleItem.Stack.ToString(), new Vector2(Game1.getMouseX() - 64, Game1.getMouseY() + 32), Color.White);
                    }
                }
            }
        }

        /// <summary>
        /// SMAPI's <see cref="IDisplayEvents.RenderedHud"/> event. 
        /// </summary>
        private void RenderedHud(object? sender, RenderedHudEventArgs e)
        {
            // There's absolutely no need to run this while we're not in building mode.
            if (buildingMode)
            {
                if (config.ShowBuildQueue)
                {
                    Dictionary<Item, int> itemAmounts = new Dictionary<Item, int>();

                    foreach (var item in tilesSelected.Values.GroupBy(x => x))
                    {
                        itemAmounts.Add(item.Key.Item, item.Count());
                    }

                    float screenWidth, screenHeight;

                    screenWidth = Game1.uiViewport.Width;
                    screenHeight = Game1.uiViewport.Height;
                    Vector2 startingPoint = new Vector2();

                    #region Shameless decompile copy

                    Point playerGlobalPosition = Game1.player.GetBoundingBox().Center;
                    Vector2 playerLocalVector = Game1.GlobalToLocal(globalPosition: new Vector2(playerGlobalPosition.X, playerGlobalPosition.Y), viewport: Game1.viewport);
                    bool toolbarAtTop = playerLocalVector.Y > (float)(Game1.viewport.Height / 2 + 64) ? true : false;

                    #endregion


                    if (toolbarAtTop)
                    {
                        startingPoint = new Vector2(screenWidth / 2 - 398, 130);
                    }
                    else
                        startingPoint = new Vector2(screenWidth / 2 - 398, screenHeight - 230);

                    foreach (var item in itemAmounts)
                    {
                        e.SpriteBatch.Draw(
                            texture: itemBox,
                            position: startingPoint,
                            sourceRectangle: new Rectangle(0, 128, 24, 24),
                            color: Color.White,
                            rotation: 0f,
                            origin: Vector2.Zero,
                            scale: Game1.pixelZoom,
                            effects: SpriteEffects.None,
                            layerDepth: 1f
                        );

                        item.Key.drawInMenu(
                            e.SpriteBatch,
                            startingPoint + new Vector2(17, 16),
                            0.75f, 1f, 4f, StackDrawType.Hide);

                        drawingUtils.DrawStringWithShadow(
                            spriteBatch: e.SpriteBatch,
                            font: Game1.smallFont,
                            text: item.Value.ToString(),
                            position: startingPoint + new Vector2(10, 14) * Game1.pixelZoom,
                            textColour: Color.White,
                            shadowColour: Color.Black
                        );

                        startingPoint += new Vector2(24 * Game1.pixelZoom + 4, 0);
                    }
                }

                // Now, we render our rectangle quantity amount.
                if (rectTiles != null)
                {
                    foreach (Vector2 tile in rectTiles)
                    {
                        // IClickableMenu.drawTextureBox(
                        //     b,
                        //     Game1.getMouseX() - 144,
                        //     Game1.getMouseY() - 32 - 16,
                        //     64 + 32,
                        //     128 + 16,
                        //     Color.White
                        // );

                        // Utility.drawTextWithShadow(
                        //     e.SpriteBatch,
                        //     rectTiles.Count.ToString(),
                        //     Game1.dialogueFont,
                        //     new Vector2(Game1.getMouseX(), Game1.getMouseY()),
                        //     Color.Black,
                        //     1f,
                        //     100f
                        //     
                        //     );
                        
                        // Utility.drawTextWithShadow(
                        //     e.SpriteBatch,
                        //     rectTiles.Count.ToString(),
                        //     Game1.dialogueFont,
                        //     new Vector2(Game1.getMouseX(), Game1.getMouseY()),
                        //     Color.Black);

                        // Utility.drawTextWithShadow(
                        //     e.SpriteBatch,
                        //     "—",
                        //     Game1.dialogueFont,
                        //     new Vector2(Game1.getMouseX(), Game1.getMouseY()),
                        //     Color.Black);
                        //
                        // Utility.drawTextWithShadow(
                        //     e.SpriteBatch,
                        //     rectangleItem.Stack.ToString(),
                        //     Game1.dialogueFont,
                        //     new Vector2(Game1.getMouseX(), Game1.getMouseY()),
                        //     Color.Black);

                        // e.SpriteBatch.DrawString(Game1.dialogueFont, rectTiles.Count.ToString(), new Vector2(Game1.getMouseX() - 64, Game1.getMouseY() - 32), Color.White);
                        // e.SpriteBatch.DrawString(Game1.dialogueFont, "—", new Vector2(Game1.getMouseX() - 64, Game1.getMouseY()), Color.White);
                        // e.SpriteBatch.DrawString(Game1.dialogueFont, rectangleItem.Stack.ToString(), new Vector2(Game1.getMouseX() - 64, Game1.getMouseY() + 32), Color.White);
                    }
                }
            }
        }

        

        private void SetupModIntegrations()
        {
            // First, check whether More Fertilizers is installed.
            if (moreFertilisersInstalled = this.Helper.ModRegistry.IsLoaded("atravita.MoreFertilizers"))
            {
                // I don't feel like I need a version check here, because this is v1.0 API stuff.
                
                // And then grab the API for atravita's More Fertilizers mod.
                try
                {
                    this.moreFertilizersApi = this.Helper.ModRegistry.GetApi<IMoreFertilizersAPI>("atravita.MoreFertilizers");
                }
                catch (Exception e)
                {
                    logger.Log($"Exception {e} getting More Fertilizers API.");
                }
            }
            
            // First, check whether DGA is installed.
            if (dgaInstalled = this.Helper.ModRegistry.IsLoaded("spacechase0.DynamicGameAssets"))
            {
                if (this.Helper.ModRegistry.Get("spacechase0.DynamicGameAssets") is IModInfo modInfo)
                {
                    if (modInfo.Manifest.Version.IsOlderThan("1.4.3"))
                    {
                        logger.Log("Installed version of DGA is too low. Please update to DGA v1.4.3.");
                        dgaApi = null;
                    }
                    
                    // And then grab the API for Casey's DGA mod.
                    try
                    {
                        this.dgaApi = this.Helper.ModRegistry.GetApi<IDynamicGameAssetsApi>("spacechase0.DynamicGameAssets");
                    }
                    catch (Exception e)
                    {
                        logger.Log($"Exception {e} getting Dynamic Game Assets API.");
                    }
                }
            }
        }

        private void RegisterWithGmcm()
        {
            IGenericModConfigMenuApi configMenuApi =
                Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

            if (configMenuApi == null)
            {
                logger.Log(I18n.SmartBuilding_Warning_GmcmNotInstalled(), LogLevel.Info);

                return;
            }

            configMenuApi.Register(ModManifest,
                () => config = new ModConfig(),
                () => Helper.WriteConfig(config));

            configMenuApi.AddSectionTitle(
                mod: ModManifest,
                text: () => I18n.SmartBuilding_Settings_Keybinds_Title()
            );

            configMenuApi.AddParagraph(
                mod: ModManifest,
                text: () => I18n.SmartBuilding_Settings_Keybinds_Paragraph_GmcmWarning()
            );

            configMenuApi.AddKeybindList(
                mod: ModManifest,
                name: () => I18n.SmartBuilding_Settings_Keybinds_Binds_EnterBuildMode(),
                getValue: () => config.EngageBuildMode,
                setValue: value => config.EngageBuildMode = value);

            configMenuApi.AddKeybindList(
                mod: ModManifest,
                name: () => I18n.SmartBuilding_Settings_Keybinds_Binds_HoldToDraw(),
                getValue: () => config.HoldToDraw,
                setValue: value => config.HoldToDraw = value);

            // configMenuApi.AddKeybindList(
            //     mod: ModManifest,
            //     name: () => I18n.SmartBuilding_Settings_Keybinds_Binds_HoldToDrawRectangle(),
            //     getValue: () => config.HoldToDrawRectangle,
            //     setValue: value => config.HoldToDrawRectangle = value
            // );
            //
            // configMenuApi.AddKeybindList(
            //     mod: ModManifest,
            //     name: () => I18n.SmartBuilding_Settings_Keybinds_Binds_HoldToErase(),
            //     getValue: () => config.HoldToErase,
            //     setValue: value => config.HoldToErase = value);
            //
            // configMenuApi.AddKeybindList(
            //     mod: ModManifest,
            //     name: () => I18n.SmartBuilding_Settings_Keybinds_Binds_HoldToInsert(),
            //     getValue: () => config.HoldToInsert,
            //     setValue: value => config.HoldToInsert = value);
            //
            // configMenuApi.AddKeybindList(
            //     mod: ModManifest,
            //     name: () => I18n.SmartBuilding_Settings_Keybinds_Binds_ConfirmBuild(),
            //     getValue: () => config.ConfirmBuild,
            //     setValue: value => config.ConfirmBuild = value);
            //
            // configMenuApi.AddKeybindList(
            //     mod: ModManifest,
            //     name: () => I18n.SmartBuilding_Settings_Keybinds_Binds_PickUpObject(),
            //     getValue: () => config.PickUpObject,
            //     setValue: value => config.PickUpObject = value);
            //
            // configMenuApi.AddKeybindList(
            //     mod: ModManifest,
            //     name: () => I18n.SmartBuilding_Settings_Keybinds_Binds_PickUpFloor(),
            //     getValue: () => config.PickUpFloor,
            //     setValue: value => config.PickUpFloor = value);
            //
            // configMenuApi.AddKeybindList(
            //     mod: ModManifest,
            //     name: () => I18n.SmartBuilding_Settings_Keybinds_Binds_PickUpFurniture(),
            //     getValue: () => config.PickUpFurniture,
            //     setValue: value => config.PickUpFurniture = value);

            configMenuApi.AddParagraph(
                mod: ModManifest,
                text: () => "" // This is purely for spacing.
            );

            configMenuApi.AddSectionTitle(
                mod: ModManifest,
                text: () => I18n.SmartBuilding_Settings_OptionalToggles_Title()
            );

            configMenuApi.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.SmartBuilding_Settings_OptionalToggles_ShowBuildQueue(),
                getValue: () => config.ShowBuildQueue,
                setValue: value => config.ShowBuildQueue = value
            );

            configMenuApi.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.SmartBuilding_Settings_OptionalToggles_CanDestroyChests(),
                tooltip: () => I18n.SmartBuilding_Settings_OptionalToggles_CanDestroyChests_Tooltip(),
                getValue: () => config.CanDestroyChests,
                setValue: value => config.CanDestroyChests = value
            );

            configMenuApi.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.SmartBuilding_Settings_OptionalTogglesMoreLaxObjectPlacement(),
                tooltip: () => I18n.SmartBuilding_Settings_OptionalTogglesMoreLaxObjectPlacement_Tooltip(),
                getValue: () => config.LessRestrictiveObjectPlacement,
                setValue: value => config.LessRestrictiveObjectPlacement = value
            );

            configMenuApi.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.SmartBuilding_Settings_OptionalToggles_MoreLaxFloorPlacement(),
                tooltip: () => I18n.SmartBuilding_Settings_OptionalToggles_MoreLaxFloorPlacement_Tooltip(),
                getValue: () => config.LessRestrictiveFloorPlacement,
                setValue: value => config.LessRestrictiveFloorPlacement = value
            );

            configMenuApi.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.SmartBuilding_Settings_OptionalToggles_MoreLaxFurniturePlacement(),
                tooltip: () => I18n.SmartBuilding_Settings_OptionalToggles_MoreLaxFurniturePlacement_Tooltip(),
                getValue: () => config.LessRestrictiveFurniturePlacement,
                setValue: value => config.LessRestrictiveFurniturePlacement = value
            );

            configMenuApi.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.SmartBuilding_Settings_OptionalToggles_MoreLaxBedPlacement(),
                tooltip: () => I18n.SmartBuilding_Settings_OptionalToggles_MoreLaxBedPlacement_Tooltip(),
                getValue: () => config.LessRestrictiveBedPlacement,
                setValue: value => config.LessRestrictiveBedPlacement = value
            );

            configMenuApi.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.SmartBuilding_Settings_OptionalToggles_EnableReplacingFloors(),
                getValue: () => config.EnableReplacingFloors,
                setValue: value => config.EnableReplacingFloors = value
            );

            configMenuApi.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.SmartBuilding_Settings_OptionalToggles_EnableReplacingFences(),
                tooltip: () => I18n.SmartBuilding_Settings_OptionalToggles_EnableReplacingFences_Tooltip(),
                getValue: () => config.EnableReplacingFences,
                setValue: value => config.EnableReplacingFences = value
            );

            configMenuApi.AddParagraph(
                mod: ModManifest,
                text: () => "" // This is purely for spacing.
            );

            configMenuApi.AddSectionTitle(
                mod: ModManifest,
                text: () => I18n.SmartBuilding_Settings_CheatyOptions_Title()
            );

            configMenuApi.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.SmartBuilding_Settings_CheatyOptions_CrabPotsInAnyWaterTile(),
                getValue: () => config.CrabPotsInAnyWaterTile,
                setValue: value => config.CrabPotsInAnyWaterTile = value
            );

            configMenuApi.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.SmartBuilding_Settings_CheatyOptions_EnablePlantingCrops(),
                getValue: () => config.EnablePlantingCrops,
                setValue: value => config.EnablePlantingCrops = value
            );

            configMenuApi.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.SmartBuilding_Settings_CheatyOptions_EnableCropFertilisers(),
                getValue: () => config.EnableCropFertilizers,
                setValue: value => config.EnableCropFertilizers = value
            );

            configMenuApi.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.SmartBuilding_Settings_CheatyOptions_EnableTreeFertilisers(),
                getValue: () => config.EnableTreeFertilizers,
                setValue: value => config.EnableTreeFertilizers = value
            );

            configMenuApi.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.SmartBuilding_Settings_CheatyOptions_EnableTreeTappers(),
                getValue: () => config.EnableTreeTappers,
                setValue: value => config.EnableTreeTappers = value
            );

            configMenuApi.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.SmartBuilding_Settings_CheatyOptions_EnableInsertingItemsIntoMachines(),
                getValue: () => config.EnableInsertingItemsIntoMachines,
                setValue: value => config.EnableInsertingItemsIntoMachines = value
            );

            configMenuApi.AddParagraph(
                mod: ModManifest,
                text: () => "" // This is purely for spacing.
            );

            configMenuApi.AddSectionTitle(
                mod: ModManifest,
                text: () => I18n.SmartBuilding_Settings_Debug_Title()
            );

            configMenuApi.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.SmartBuilding_Settings_Debug_EnableDebugCommand(),
                getValue: () => config.EnableDebugCommand,
                setValue: value => config.EnableDebugCommand = value
            );

            configMenuApi.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.SmartBuilding_Settings_Debug_EnableDebugKeybinds(),
                getValue: () => config.EnableDebugControls,
                setValue: value => config.EnableDebugControls = value
            );

            configMenuApi.AddKeybindList(
                mod: ModManifest,
                name: () => I18n.SmartBuilding_Settings_Debug_IdentifyProducerToConsole(),
                getValue: () => config.IdentifyProducer,
                setValue: value => config.IdentifyProducer = value);

            configMenuApi.AddKeybindList(
                mod: ModManifest,
                name: () => I18n.SmartBuilding_Settings_Debug_IdentifyHeldItemToConsole(),
                getValue: () => config.IdentifyItem,
                setValue: value => config.IdentifyItem = value);

            configMenuApi.AddParagraph(
                mod: ModManifest,
                text: () => "" // This is purely for spacing.
            );

            configMenuApi.AddSectionTitle(
                mod: ModManifest,
                text: () => I18n.SmartBuilding_Settings_PotentiallyDangerous_Title()
            );

            configMenuApi.AddParagraph(
                mod: ModManifest,
                text: () => I18n.SmartBuilding_Settings_PotentiallyDangerous_Paragraph()
            );

            configMenuApi.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.SmartBuilding_Settings_PotentiallyDangerous_EnablePlacingStorageFurniture(),
                tooltip: () => I18n.SmartBuilding_Settings_PotentiallyDangerous_EnablePlacingStorageFurniture_Tooltip(),
                getValue: () => config.EnablePlacingStorageFurniture,
                setValue: value => config.EnablePlacingStorageFurniture = value
            );

            configMenuApi.AddPageLink(
                mod: ModManifest,
                pageId: "JsonGuide",
                text: () => I18n.SmartBuilding_Settings_JsonGuide_PageLink()
            );

            configMenuApi.AddPage(
                mod: ModManifest,
                pageId: "JsonGuide",
                pageTitle: () => I18n.SmartBuilding_Settings_JsonGuide_PageTitle()
            );

            configMenuApi.AddParagraph(
                mod: ModManifest,
                text: () => I18n.SmartBuilding_Settings_JsonGuide_Guide1()
            );

            configMenuApi.AddParagraph(
                mod: ModManifest,
                text: () => I18n.SmartBuilding_Settings_JsonGuide_Guide2()
            );

            configMenuApi.AddParagraph(
                mod: ModManifest,
                text: () => I18n.SmartBuilding_Settings_JsonGuide_Guide3()
            );

            configMenuApi.AddParagraph(
                mod: ModManifest,
                text: () => I18n.SmartBuilding_Settings_JsonGuide_Guide4()
            );

            configMenuApi.AddParagraph(
                mod: ModManifest,
                text: () => I18n.SmartBuilding_Settings_JsonGuide_Guide5()
            );
        }

        private List<Vector2> CalculateRectangle(Vector2 cornerOne, Vector2 cornerTwo, Item item)
        {
            Vector2 topLeft;
            Vector2 bottomRight;
            List<Vector2> tiles = new List<Vector2>();
            int itemsRemainingInStack = 0;

            if (item != null)
                itemsRemainingInStack = item.Stack;
            else
                itemsRemainingInStack = 0;

            topLeft = new Vector2(MathF.Min(cornerOne.X, cornerTwo.X), MathF.Min(cornerOne.Y, cornerTwo.Y));
            bottomRight = new Vector2(MathF.Max(cornerOne.X, cornerTwo.X), MathF.Max(cornerOne.Y, cornerTwo.Y));

            int rectWidth = (int)bottomRight.X - (int)topLeft.X + 1;
            int rectHeight = (int)bottomRight.Y - (int)topLeft.Y + 1;

            for (int x = (int)topLeft.X; x < rectWidth + topLeft.X; x++)
            {
                for (int y = (int)topLeft.Y; y < rectHeight + topLeft.Y; y++)
                {
                    if (itemsRemainingInStack > 0)
                    {
                        if (identificationUtils.CanBePlacedHere(new Vector2(x, y), item))
                        {
                            tiles.Add(new Vector2(x, y));
                            itemsRemainingInStack--;
                        }
                    }
                }
            }

            return tiles;
        }
        
        public void ResetVolatileTiles()
        {
            rectTiles.Clear();
            startTile = null;
            endTile = null;
        }
        
        private void EraseTile(Vector2 tile)
        {
            Vector2 flaggedForRemoval = new Vector2();

            foreach (var item in tilesSelected)
            {
                if (item.Key == tile)
                {
                    // If we're over a tile in _tilesSelected, remove it and refund the item to the player.
                    Game1.player.addItemToInventoryBool(item.Value.Item.getOne(), false);
                    monitor.Log($"{item.Value.Item.Name} {I18n.SmartBuilding_Info_RefundedIntoPlayerInventory()}");

                    // And flag it for removal from the queue, since we can't remove from within the foreach.
                    flaggedForRemoval = tile;
                }
            }

            tilesSelected.Remove(flaggedForRemoval);
        }
        
        /// <summary>
        /// There is no queue for item insertion, as the only method available to determine whether an item can be inserted is to insert it.
        /// </summary>
        /// <param name="targetTile"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool TryToInsertHere(Vector2 targetTile, Item item)
        {
            // First, we need to ensure there's an SObject here.
            if (Game1.currentLocation.objects.ContainsKey(targetTile))
            {
                // There is one, so we grab a reference to it.
                SObject o = Game1.currentLocation.objects[targetTile];

                // We also need to know what type of producer we're looking at, if any.
                ProducerType type = identificationUtils.IdentifyProducer(o);

                // Whether or not we need to manually deduct the item.
                bool needToDeduct = false;

                // If this isn't a producer, we return immediately.
                if (type == ProducerType.NotAProducer)
                {
                    return false;
                }
                else if (type == ProducerType.ManualRemoval)
                {
                    // If this requires manual removal, we mark that we do need to manually deduct the item.
                    needToDeduct = true;
                }
                else if (type == ProducerType.AutomaticRemoval)
                {
                    // The producer in question removes automatically, so we don't need to manually deduct the item.
                    needToDeduct = false;
                }

                InsertItem(item, o, needToDeduct);
            }

            // There was no object here, so we return false.
            return false;
        }

        private void InsertItem(Item item, SObject o, bool shouldManuallyDeduct)
        {
            // For some reason, apparently, we always need to deduct the held item by one, even if we're working with a producer which does it by itself.

            //if (shouldManuallyDeduct)
            //{
            //    // This is marked as needing to be deducted manually, so we do just that.
            //    Game1.player.reduceActiveItemByOne();
            //}

            // First, we perform the drop in action.
            bool successfullyInserted = o.performObjectDropInAction(item, false, Game1.player);

            // Then, perplexingly, we still need to manually deduct the item by one, or we can end up with an item that has a stack size of zero.
            if (successfullyInserted)
            {
                Game1.player.reduceActiveItemByOne();
            }
        }

        private void AddItem(Item item, Vector2 v)
        {
            // If we're not in building mode, we do nothing.
            if (!buildingMode)
                return;

            // If the player isn't holding an item, we do nothing.
            if (Game1.player.CurrentItem == null)
                return;

            // There is no queue for item insertion, so we simply try to insert.
            TryToInsertHere(v, item);
        }

        private void AddTile(Item item, Vector2 v)
        {
            // If we're not in building mode, we do nothing.
            if (!buildingMode)
                return;

            // If the player isn't holding an item, we do nothing.
            if (Game1.player.CurrentItem == null)
                return;

            // If the item cannot be placed here according to our own rules, we do nothing. This is to allow for slightly custom placement logic.
            if (!identificationUtils.CanBePlacedHere(v, item))
                return;

            ItemInfo itemInfo = identificationUtils.GetItemInfo((SObject)item);

            // We only want to add the tile if the Dictionary doesn't already contain it. 
            if (!tilesSelected.ContainsKey(v))
            {
                // We then want to check if the item can even be placed in this spot.
                if (identificationUtils.CanBePlacedHere(v, item))
                {
                    tilesSelected.Add(v, itemInfo);
                    Game1.player.reduceActiveItemByOne();
                }
            }
        }


        /// <summary>
        /// Render the drawn queue in the world.
        /// </summary>
        private void RenderedWorld(object? sender, RenderedWorldEventArgs e)
        {
            SpriteBatch b = e.SpriteBatch;

            foreach (KeyValuePair<Vector2, ItemInfo> item in tilesSelected)
            {
                // Here, we simply have the Item draw itself in the world.
                item.Value.Item.drawInMenu
                (e.SpriteBatch,
                    Game1.GlobalToLocal(
                        Game1.viewport,
                        item.Key * Game1.tileSize),
                    1f, 1f, 4f, StackDrawType.Hide);
            }

            if (rectTiles != null)
            {
                foreach (Vector2 tile in rectTiles)
                {
                    // Here, we simply have the Item draw itself in the world.
                    rectangleItem.drawInMenu
                    (e.SpriteBatch,
                        Game1.GlobalToLocal(
                            Game1.viewport,
                            tile * Game1.tileSize),
                        1f, 1f, 4f, StackDrawType.Hide);

                    // IClickableMenu.drawTextureBox(
                    //     b,
                    //     Game1.getMouseX() - 144,
                    //     Game1.getMouseY() - 32 - 16,
                    //     64 + 32,
                    //     128 + 16,
                    //     Color.White
                    // );
                    //
                    // Utility.drawTextWithShadow(
                    //     b,
                    //     rectTiles.Count.ToString(),
                    //     Game1.dialogueFont,
                    //     new Vector2(Game1.getMouseX() - 128, Game1.getMouseY() - 32),
                    //     Color.Black);
                    //
                    // Utility.drawTextWithShadow(
                    //     b,
                    //     "—",
                    //     Game1.dialogueFont,
                    //     new Vector2(Game1.getMouseX() - 128, Game1.getMouseY()),
                    //     Color.Black);
                    //
                    // Utility.drawTextWithShadow(
                    //     b,
                    //     rectangleItem.Stack.ToString(),
                    //     Game1.dialogueFont,
                    //     new Vector2(Game1.getMouseX() - 128, Game1.getMouseY() + 32),
                    //     Color.Black);

                    // e.SpriteBatch.DrawString(Game1.dialogueFont, rectTiles.Count.ToString(), new Vector2(Game1.getMouseX() - 64, Game1.getMouseY() - 32), Color.White);
                    // e.SpriteBatch.DrawString(Game1.dialogueFont, "—", new Vector2(Game1.getMouseX() - 64, Game1.getMouseY()), Color.White);
                    // e.SpriteBatch.DrawString(Game1.dialogueFont, rectangleItem.Stack.ToString(), new Vector2(Game1.getMouseX() - 64, Game1.getMouseY() + 32), Color.White);
                }
            }
        }

        /// <summary>
        /// Clear the tiles in the drawn queue.
        /// </summary>
        private void ClearPaintedTiles()
        {
            // To clear the painted tiles, we want to iterate through our Dictionary, and refund every item contained therein.
            foreach (var t in tilesSelected)
            {
                RefundItem(t.Value.Item, I18n.SmartBuilding_Info_BuildCancelled(), LogLevel.Trace, false);
            }

            // And, finally, clear it.
            tilesSelected.Clear();

            // We also want to clear the rect tiles. No refunding necessary here, however, as items are only deducted when added to tilesSelected.
            rectTiles.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tile">The tile we want to demolish the <see cref="StardewValley.Object"/> or <see cref="StardewValley.TerrainFeature"/> on.</param>
        /// <param name="feature">Which type of <see cref="TileFeature"/> we're dealing with.</param>
        private void DemolishOnTile(Vector2 tile, TileFeature feature)
        {
            GameLocation here = Game1.currentLocation;
            Vector2 playerTile = Game1.player.getTileLocation();
            Item itemToDestroy;
            ItemType type;

            // We're working with an SObject in this specific instance.
            if (feature == TileFeature.Object)
            {
                if (here.objects.ContainsKey(tile))
                {
                    // We have an object in this tile, so we want to try to figure out what it is.
                    SObject o = here.objects[tile];
                    itemToDestroy = Utility.fuzzyItemSearch(o.Name);

                    type = identificationUtils.IdentifyItemType((SObject)itemToDestroy);

                    // Chests need special handling because they can store items.
                    if (type == ItemType.Chest)
                    {
                        // We're double checking at this point for safety. I want to be extra careful with chests.
                        if (here.objects.ContainsKey(tile))
                        {
                            // If the setting to disable chest pickup is enabled, we pick up the chest. If not, we do nothing.
                            if (config.CanDestroyChests)
                            {
                                // This is fairly fragile, but it's fine with vanilla chests, at least.
                                Chest chest = new Chest(o.ParentSheetIndex, tile, 0, 1);

                                (o as Chest).destroyAndDropContents(tile * 64, here);
                                Game1.player.addItemByMenuIfNecessary(chest.getOne());
                                here.objects.Remove(tile);
                            }
                        }
                    }
                    else if (o is Chest)
                    {
                        // We're double checking at this point for safety. I want to be extra careful with chests.
                        if (here.objects.ContainsKey(tile))
                        {
                            // If the setting to disable chest pickup is enabled, we pick up the chest. If not, we do nothing.
                            if (config.CanDestroyChests)
                            {
                                // This is fairly fragile, but it's fine with vanilla chests, at least.
                                Chest chest = new Chest(o.ParentSheetIndex, tile, 0, 1);

                                (o as Chest).destroyAndDropContents(tile * 64, here);
                                Game1.player.addItemByMenuIfNecessary(chest.getOne());
                                here.objects.Remove(tile);
                            }
                        }
                    }
                    else if (type == ItemType.Fence)
                    {
                        // We need special handling for fences, since we don't want to pick them up if their health has deteriorated too much.
                        Fence fenceToRemove = (Fence)o;

                        // We also need to check to see if the fence has a torch on it so we can remove the light source.
                        if (o.heldObject.Value != null)
                        {
                            // There's an item there, so we can relatively safely assume it's a torch.

                            // We remove its light source from the location, and refund the torch.
                            here.removeLightSource(o.heldObject.Value.lightSource.identifier);

                            RefundItem(o.heldObject, "No error. Do not log.", LogLevel.Trace, false);
                        }

                        fenceToRemove.performRemoveAction(tile * 64, here);
                        here.objects.Remove(tile);

                        // And, if the fence had enough health remaining, we refund it.
                        if (fenceToRemove.maxHealth.Value - fenceToRemove.health.Value < 0.5f)
                            Game1.player.addItemByMenuIfNecessary(fenceToRemove.getOne());
                    }
                    else if (type == ItemType.Tapper)
                    {
                        // Tappers need special handling to mark the tree as untapped, otherwise they can't be chopped down with an axe.
                        if (here.terrainFeatures.ContainsKey(tile))
                        {
                            // We've confirmed there's a TerrainFeature here, so next we grab a reference to it if it is a tree.
                            if (here.terrainFeatures[tile] is Tree treeToUntap)
                            {
                                // After double checking there's a tree here, we grab a reference to it.
                                treeToUntap.tapped.Value = false;
                            }

                            o.performRemoveAction(tile * 64, here);
                            Game1.player.addItemByMenuIfNecessary(o.getOne());

                            here.objects.Remove(tile);
                        }
                    }
                    else
                    {
                        if (o.Fragility == 2)
                        {
                            // A fragility of 2 means the item should not be broken, or able to be picked up, like incubators in coops, so we return.

                            return;
                        }

                        // Now we need to figure out whether the object has a heldItem within it.
                        if (o.heldObject != null)
                        {
                            // There's an item inside here, so we need to determine whether to refund the item, or discard it if it's a chest.
                            if (o.heldObject.Value is Chest)
                            {
                                // It's a chest, so we want to force it to drop all of its items.
                                if ((o.heldObject.Value as Chest).items.Count > 0)
                                {
                                    (o.heldObject.Value as Chest).destroyAndDropContents(tile * 64, here);
                                }
                            }
                        }

                        o.performRemoveAction(tile * 64, here);
                        Game1.player.addItemByMenuIfNecessary(o.getOne());

                        here.objects.Remove(tile);
                    }
                    
                    return;
                }
            }

            // We're working with a TerrainFeature.
            if (feature == TileFeature.TerrainFeature)
            {
                if (here.terrainFeatures.ContainsKey(tile))
                {
                    TerrainFeature tf = here.terrainFeatures[tile];

                    // We only really want to be handling flooring when removing TerrainFeatures.
                    if (tf is Flooring)
                    {
                        Flooring floor = (Flooring)tf;

                        int? floorType = floor.whichFloor.Value;
                        string? floorName = identificationUtils.GetFlooringNameFromId(floorType.Value);
                        SObject finalFloor;

                        if (floorType.HasValue)
                        {
                            floorName = identificationUtils.GetFlooringNameFromId(floorType.Value);
                            finalFloor = (SObject)Utility.fuzzyItemSearch(floorName, 1);
                        }
                        else
                        {
                            finalFloor = null;
                        }

                        if (finalFloor != null)
                            Game1.player.addItemByMenuIfNecessary(finalFloor);
                        // Game1.createItemDebris(finalFloor, playerTile * 64, 1, here);

                        here.terrainFeatures.Remove(tile);
                    }
                }
            }

            if (feature == TileFeature.Furniture)
            {
                Furniture furnitureToGrab = null;

                foreach (Furniture f in here.furniture)
                {
                    if (f.boundingBox.Value.Intersects(new Rectangle((int)tile.X * 64, (int)tile.Y * 64, 1, 1)))
                    {
                        furnitureToGrab = f;
                    }
                }

                if (furnitureToGrab != null)
                {
                    // If it's a StorageFurniture, and the setting to allow working with it is false, do nothing.
                    if (furnitureToGrab is StorageFurniture && !config.EnablePlacingStorageFurniture)
                        return;

                    // Otherwise, we can continue.
                    logger.Log($"{I18n.SmartBuikding_Message_TryingToGrab()} {furnitureToGrab.Name}");
                    Game1.player.addItemToInventory(furnitureToGrab);
                    here.furniture.Remove(furnitureToGrab);
                }
            }
        }

        /// <summary>
        /// Determine how to correctly place an item in the world, and place it.
        /// </summary>
        /// <param name="item">The <see cref="KeyValuePair"/> containing the <see cref="Vector2"/> tile, and <see cref="ItemInfo"/> information about the item to be placed.</param>
        private void PlaceObject(KeyValuePair<Vector2, ItemInfo> item)
        {
            SObject itemToPlace = (SObject)item.Value.Item;
            Vector2 targetTile = item.Key;
            ItemInfo itemInfo = item.Value;
            GameLocation here = Game1.currentLocation;

            if (itemToPlace != null && identificationUtils.CanBePlacedHere(targetTile, itemInfo.Item))
            { // The item can be placed here.
                if (itemInfo.ItemType == ItemType.Floor)
                {
                    // We're specifically dealing with a floor/path.

                    int? floorType = identificationUtils.GetFlooringIdFromName(itemToPlace.Name);
                    Flooring floor;

                    if (floorType.HasValue)
                        floor = new Flooring(floorType.Value);
                    else
                    {
                        // At this point, something is very wrong, so we want to refund the item to the player's inventory, and print an error.
                        RefundItem(itemToPlace, I18n.SmartBuilding_Error_TerrainFeature_Flooring_CouldNotIdentifyFloorType(), LogLevel.Error, true);

                        return;
                    }

                    // At this point, we *need* there to be no TerrainFeature present.
                    if (!here.terrainFeatures.ContainsKey(targetTile))
                        here.terrainFeatures.Add(targetTile, floor);
                    else
                    {
                        // At this point, we know there's a terrain feature here.
                        if (config.EnableReplacingFloors)
                        {
                            TerrainFeature tf = here.terrainFeatures[targetTile];

                            if (tf != null && tf is Flooring)
                            {
                                // At this point, we know it's Flooring, so we remove the existing terrain feature, and add our new one.
                                DemolishOnTile(targetTile, TileFeature.TerrainFeature);
                                here.terrainFeatures.Add(targetTile, floor);
                            }
                            else
                            {
                                // At this point, there IS a terrain feature here, but it isn't flooring, so we want to refund the item, and return.
                                RefundItem(item.Value.Item, I18n.SmartBuilding_Error_TerrainFeature_Generic_AlreadyPresent(), LogLevel.Error);

                                // We now want to jump straight out of this method, because this will flow through to the below if, and bad things will happen.
                                return;
                            }
                        }
                    }

                    // By this point, we'll have returned false if this could be anything but our freshly placed floor.
                    if (!(here.terrainFeatures.ContainsKey(item.Key) && here.terrainFeatures[item.Key] is Flooring))
                        RefundItem(item.Value.Item, I18n.SmartBuilding_Error_TerrainFeature_Generic_UnknownError(), LogLevel.Error);
                }
                else if (itemInfo.ItemType == ItemType.Chest)
                {
                    // We're dealing with a chest.
                    int? chestType = identificationUtils.GetChestType(itemToPlace.Name);
                    Chest chest;

                    if (chestType.HasValue)
                    {
                        chest = new Chest(true, chestType.Value);
                    }
                    else
                    { // At this point, something is very wrong, so we want to refund the item to the player's inventory, and print an error.
                        RefundItem(itemToPlace, I18n.SmartBuilding_Error_Chest_CouldNotIdentifyChest(), LogLevel.Error, true);

                        return;
                    }

                    // We do our second placement possibility check, just in case something was placed in the meantime.
                    if (identificationUtils.CanBePlacedHere(targetTile, itemToPlace))
                    {
                        bool placed = chest.placementAction(here, (int)targetTile.X * 64, (int)targetTile.Y * 64, Game1.player);

                        // Apparently, chests placed in the world are hardcoded with the name "Chest".
                        if (!here.objects.ContainsKey(targetTile) || !here.objects[targetTile].Name.Equals("Chest"))
                            RefundItem(itemToPlace, I18n.SmartBuilding_Error_Object_PlacementFailed(), LogLevel.Error);
                    }
                }
                else if (itemInfo.ItemType == ItemType.Fence)
                {
                    // We want to check to see if the target tile contains an object.
                    if (here.objects.ContainsKey(targetTile))
                    {
                        SObject o = here.objects[targetTile];

                        if (o != null)
                        {
                            // We try to identify what kind of object is placed here.
                            if (identificationUtils.IsTypeOfObject(o, ItemType.Fence))
                            {
                                if (config.EnableReplacingFences)
                                {
                                    // We have a fence, so we want to remove it before placing our new one.
                                    DemolishOnTile(targetTile, TileFeature.Object);
                                }
                            }
                            else
                            {
                                // If it isn't a fence, we want to refund the item, and return to avoid placing the fence.
                                RefundItem(item.Value.Item, I18n.SmartBuilding_Error_Object_PlacementFailed(), LogLevel.Error);
                                return;
                            }
                        }
                    }

                    if (!itemToPlace.placementAction(Game1.currentLocation, (int)item.Key.X * 64, (int)item.Key.Y * 64, Game1.player))
                        RefundItem(item.Value.Item, I18n.SmartBuilding_Error_Object_PlacementFailed(), LogLevel.Error);
                }
                else if (itemInfo.ItemType == ItemType.GrassStarter)
                {
                    Grass grassStarter = new Grass(1, 4);

                    // At this point, we *need* there to be no TerrainFeature present.
                    if (!here.terrainFeatures.ContainsKey(targetTile))
                        here.terrainFeatures.Add(targetTile, grassStarter);
                    else
                    {
                        RefundItem(item.Value.Item, I18n.SmartBuilding_Error_TerrainFeature_Generic_AlreadyPresent(), LogLevel.Error);

                        // We now want to jump straight out of this method, because this will flow through to the below if, and bad things may happen.
                        return;
                    }

                    if (!(here.terrainFeatures.ContainsKey(item.Key) && here.terrainFeatures[targetTile] is Grass))
                        RefundItem(item.Value.Item, I18n.SmartBuilding_Error_TerrainFeature_Generic_AlreadyPresent(), LogLevel.Error);
                }
                else if (itemInfo.ItemType == ItemType.CrabPot)
                {
                    CrabPot pot = new CrabPot(targetTile);

                    if (identificationUtils.CanBePlacedHere(targetTile, itemToPlace))
                    {
                        itemToPlace.placementAction(Game1.currentLocation, (int)targetTile.X * 64, (int)targetTile.Y * 64, Game1.player);
                    }
                }
                else if (itemInfo.ItemType == ItemType.Seed)
                {
                    // Here, we're dealing with a seed, so we need very special logic for this.
                    // Item.placementAction for seeds is semi-broken, unless the player is currently
                    // holding the specific seed being planted.

                    bool successfullyPlaced = false;

                    // First, we check for a TerrainFeature.
                    if (Game1.currentLocation.terrainFeatures.ContainsKey(targetTile))
                    {
                        // Then, we check to see if it's a HoeDirt.
                        if (Game1.currentLocation.terrainFeatures[targetTile] is HoeDirt)
                        {
                            // If it is, we grab a reference to it.
                            HoeDirt hd = (HoeDirt)Game1.currentLocation.terrainFeatures[targetTile];

                           // We check to see if it can be planted, and act appropriately.
                            if (identificationUtils.CanBePlacedHere(targetTile, itemToPlace))
                            {
                                if (itemInfo.IsDgaItem)
                                    successfullyPlaced = itemToPlace.placementAction(here, (int)targetTile.X * 64, (int)targetTile.Y * 64, Game1.player);
                                else
                                    successfullyPlaced = hd.plant(itemToPlace.ParentSheetIndex, (int)targetTile.X, (int)targetTile.Y, Game1.player, false, Game1.currentLocation);
                            }
                        }
                    }

                    // If the planting failed, we refund the seed.
                    if (!successfullyPlaced)
                        RefundItem(item.Value.Item, I18n.SmartBuilding_Error_Seeds_PlacementFailed(), LogLevel.Error);
                }
                else if (itemInfo.ItemType == ItemType.Fertilizer)
                {
                    // First, we get whether or not More Fertilizers can place this fertiliser.
                    if (this.moreFertilizersApi?.CanPlaceFertilizer(itemToPlace, here, targetTile) == true)
                    {
                        // If it can, we try to place it.
                        if (this.moreFertilizersApi.TryPlaceFertilizer(itemToPlace, here, targetTile))
                        {
                            // If the placement is successful, we do the fancy animation thing.
                            this.moreFertilizersApi.AnimateFertilizer(itemToPlace, here, targetTile);
                        }
                        else
                        {
                            // Otherwise, the fertiliser gets refunded.
                            RefundItem(itemToPlace, $"{I18n.SmartBuilding_Integrations_MoreFertilizers_InvalidFertiliserPosition()}: {itemToPlace.Name} @ {targetTile}", LogLevel.Debug);
                        }
                    }

                    if (here.terrainFeatures.ContainsKey(targetTile))
                    {
                        // We know there's a TerrainFeature here, so next we want to check if it's HoeDirt.
                        if (here.terrainFeatures[targetTile] is HoeDirt)
                        {
                            // If it is, we want to grab the HoeDirt, check if it's already got a fertiliser, and fertilise if not.
                            HoeDirt hd = (HoeDirt)here.terrainFeatures[targetTile];

                            // 0 here means no fertilizer. This is a known change in 1.6.
                            if (hd.fertilizer.Value == 0)
                            {
                                // Next, we want to check if there's already a crop here.
                                if (hd.crop != null)
                                {
                                    Crop cropToCheck = hd.crop;

                                    if (cropToCheck.currentPhase.Value == 0)
                                    {
                                        // If the current crop phase is zero, we can plant the fertilizer here.

                                        hd.plant(itemToPlace.ParentSheetIndex, (int)targetTile.X, (int)targetTile.Y, Game1.player, true, Game1.currentLocation);
                                    }
                                }
                                else
                                {
                                    // If there is no crop here, we can plant the fertilizer with reckless abandon.
                                    hd.plant(itemToPlace.ParentSheetIndex, (int)targetTile.X, (int)targetTile.Y, Game1.player, true, Game1.currentLocation);
                                }
                            }
                            else
                            {
                                // If there is already a fertilizer here, we want to refund the item.
                                RefundItem(itemToPlace, I18n.SmartBuilding_Error_Fertiliser_AlreadyFertilised(), LogLevel.Warn);
                            }

                            // Now, we want to run the final check to see if the fertilization was successful.
                            if (hd.fertilizer.Value == 0)
                            {
                                // If there's still no fertilizer here, we need to refund the item.
                                RefundItem(itemToPlace, I18n.SmartBuilding_Error_Fertiliser_IneligibleForFertilisation(), LogLevel.Warn);
                            }
                        }
                    }

                }
                else if (itemInfo.ItemType == ItemType.TreeFertilizer)
                {
                    if (here.terrainFeatures.ContainsKey(targetTile))
                    {
                        // If there's a TerrainFeature here, we check if it's a tree.
                        if (here.terrainFeatures[targetTile] is Tree)
                        {
                            // It is a tree, so now we check to see if the tree is fertilised.
                            Tree tree = (Tree)here.terrainFeatures[targetTile];

                            // If it's already fertilised, there's no need for us to want to place tree fertiliser on it.
                            if (!tree.fertilized.Value)
                                tree.fertilize(here);
                        }
                    }
                }
                else if (itemInfo.ItemType == ItemType.Tapper)
                {
                    if (identificationUtils.CanBePlacedHere(targetTile, itemToPlace))
                    {
                        // If there's a TerrainFeature here, we need to know if it's a tree.
                        if (here.terrainFeatures[targetTile] is Tree)
                        {
                            // If it is, we grab a reference, and check for a tapper on it already.
                            Tree tree = (Tree)here.terrainFeatures[targetTile];

                            if (!tree.tapped.Value)
                            {
                                if (!itemToPlace.placementAction(here, (int)targetTile.X * 64, (int)targetTile.Y * 64, Game1.player))
                                {
                                    // If the placement action didn't succeed, we refund the item.
                                    RefundItem(itemToPlace, I18n.SmartBuilding_Error_TreeTapper_PlacementFailed(), LogLevel.Error);
                                }
                            }
                        }
                    }
                }
                else if (itemInfo.ItemType == ItemType.FishTankFurniture)
                {
                    // This cannot be reached, because placement of fish tanks is blocked for now.
                    
                    // // We're dealing with a fish tank. This has dangerous consequences.
                    // if (_config.LessRestrictiveFurniturePlacement)
                    // {
                    //  FishTankFurniture tank = new FishTankFurniture(itemToPlace.ParentSheetIndex, targetTile);
                    //
                    //  foreach (var fish in (itemToPlace as FishTankFurniture).tankFish)
                    //  {
                    //      tank.tankFish.Add(fish);
                    //  }
                    //
                    //  foreach (var fish in tank.tankFish)
                    //  {
                    //      fish.ConstrainToTank();
                    //  }
                    //  
                    //  here.furniture.Add(tank);
                    // }
                    // else
                    // {
                    //  (itemToPlace as FishTankFurniture).placementAction(here, (int)targetTile.X, (int)targetTile.Y, Game1.player);
                    // }
                }
                else if (itemInfo.ItemType == ItemType.StorageFurniture)
                {
                    if (config.EnablePlacingStorageFurniture && !itemInfo.IsDgaItem)
                    {
                        bool placedSuccessfully = false;

                        // We need to create a new instance of StorageFurniture.
                        StorageFurniture storage = new StorageFurniture(itemToPlace.ParentSheetIndex, targetTile);

                        // A quick bool to avoid an unnecessary log to console later.
                        bool anyItemsAdded = false;

                        // Then, we iterate through all of the items in the existing StorageFurniture, and add them to the new one.
                        foreach (var itemInStorage in (itemToPlace as StorageFurniture).heldItems)
                        {
                            logger.Log($"{I18n.SmartBuilding_Message_StorageFurniture_AddingItem()} {itemInStorage.Name} ({itemInStorage.ParentSheetIndex}).", LogLevel.Info);
                            storage.AddItem(itemInStorage);

                            anyItemsAdded = true;
                        }

                        // If any items were added, inform the user of the purpose of logging them.
                        if (anyItemsAdded)
                            logger.Log(I18n.SmartBuilding_Message_StorageFurniture_RetrievalTip(), LogLevel.Info);

                        // If we have less restrictive furniture placement enabled, we simply try to place it. Otherwise, we use the vanilla placementAction.
                        if (config.LessRestrictiveFurniturePlacement)
                            here.furniture.Add(storage as StorageFurniture);
                        else
                            placedSuccessfully = storage.placementAction(here, (int)targetTile.X * 64, (int)targetTile.Y * 64, Game1.player);

                        // Here, we check to see if the placement was successful. If not, we refund the item.
                        if (!here.furniture.Contains(storage) && !placedSuccessfully)
                            RefundItem(storage, I18n.SmartBuilding_Error_StorageFurniture_PlacementFailed(), LogLevel.Info);
                    }
                    else
                        RefundItem(itemToPlace, I18n.SmartBuilding_Error_StorageFurniture_SettingIsOff(), LogLevel.Info, true);
                }
                else if (itemInfo.ItemType == ItemType.TvFurniture)
                {
                    bool placedSuccessfully = false;
                    TV tv = null;

                    // We need to determine which we we're placing this TV based upon the furniture placement restriction option.
                    if (config.LessRestrictiveFurniturePlacement && !itemInfo.IsDgaItem)
                    {
                        tv = new TV(itemToPlace.ParentSheetIndex, targetTile);
                        here.furniture.Add(tv);
                    }
                    else
                    {
                        placedSuccessfully = (itemToPlace as TV).placementAction(here, (int)targetTile.X * 64, (int)targetTile.Y * 64, Game1.player);
                    }

                    // If both of these are false, the furniture was not successfully placed, so we need to refund the item.
                    if (tv != null && !here.furniture.Contains(tv as TV) && !placedSuccessfully)
                        RefundItem(itemToPlace, I18n.SmartBuilding_Error_TvFurniture_PlacementFailed(), LogLevel.Error);
                }
                else if (itemInfo.ItemType == ItemType.BedFurniture)
                {
                    bool placedSuccessfully = false;
                    BedFurniture bed = null;

                    // We decide exactly how we're placing the furniture based upon the less restrictive setting.
                    if (config.LessRestrictiveBedPlacement && !itemInfo.IsDgaItem)
                    {
                        bed = new BedFurniture(itemToPlace.ParentSheetIndex, targetTile);
                        here.furniture.Add(bed);
                    }
                    else
                        placedSuccessfully = (itemToPlace as BedFurniture).placementAction(here, (int)targetTile.X * 64, (int)targetTile.Y * 64, Game1.player);

                    // If both of these are false, the furniture was not successfully placed, so we need to refund the item.
                    if (bed != null && !here.furniture.Contains(bed as BedFurniture) && !placedSuccessfully)
                        RefundItem(itemToPlace, I18n.SmartBuilding_Error_BedFurniture_PlacementFailed(), LogLevel.Error);

                }
                else if (itemInfo.ItemType == ItemType.GenericFurniture)
                {
                    bool placedSuccessfully = false;
                    Furniture furniture = null;

                    // Determine exactly how we're placing this furniture.
                    if (config.LessRestrictiveFurniturePlacement && !itemInfo.IsDgaItem)
                    {
                        furniture = new Furniture(itemToPlace.ParentSheetIndex, targetTile);
                        here.furniture.Add(furniture);
                    }
                    else
                        placedSuccessfully = (itemToPlace as Furniture).placementAction(here, (int)targetTile.X * 64, (int)targetTile.Y * 64, Game1.player);

                    // If both of these are false, the furniture was not successfully placed, so we need to refund the item.
                    if (furniture != null && !here.furniture.Contains(furniture as Furniture) && !placedSuccessfully)
                        RefundItem(itemToPlace, I18n.SmartBuilding_Error_Furniture_PlacementFailed(), LogLevel.Error);
                }
                else if (itemInfo.ItemType == ItemType.Torch)
                {
                    // We need to figure out whether there's a fence in the placement tile.
                    if (here.objects.ContainsKey(targetTile))
                    {
                        // We know there's an object at these coordinates, so we grab a reference.
                        SObject o = here.objects[targetTile];

                        if (identificationUtils.IsTypeOfObject(o, ItemType.Fence))
                        {
                            // If the object in this tile is a fence, we add the torch to it.
                            //itemToPlace.placementAction(Game1.currentLocation, (int)item.Key.X * 64, (int)item.Key.Y * 64, Game1.player);

                            // We know it's a fence by type, but we need to make sure it isn't a gate, and to ensure it isn't already "holding" anything.
                            if (!o.Name.Equals("Gate") && o.heldObject != null)
                            {
                                // There's something in there, so we need to refund the torch.
                                RefundItem(item.Value.Item, I18n.SmartBuilding_Error_Torch_PlacementInFenceFailed(), LogLevel.Error);
                            }

                            o.performObjectDropInAction(itemToPlace, false, Game1.player);

                            if (identificationUtils.IdentifyItemType(o.heldObject) != ItemType.Torch)
                            {
                                // If the fence isn't "holding" a torch, there was a problem, so we should refund.
                                RefundItem(item.Value.Item, I18n.SmartBuilding_Error_Torch_PlacementInFenceFailed(), LogLevel.Error);
                            }

                            return;
                        }
                        else
                        {
                            // If it's not a fence, we want to refund the item.
                            RefundItem(item.Value.Item, I18n.SmartBuilding_Error_Object_PlacementFailed(), LogLevel.Error);

                            return;
                        }
                    }

                    // There is no object here, so we treat it like a generic placeable.
                    if (!itemToPlace.placementAction(Game1.currentLocation, (int)item.Key.X * 64, (int)item.Key.Y * 64, Game1.player))
                        RefundItem(item.Value.Item, I18n.SmartBuilding_Error_Object_PlacementFailed(), LogLevel.Error);
                }
                else
                { // We're dealing with a generic placeable.
                    bool successfullyPlaced = itemToPlace.placementAction(Game1.currentLocation, (int)item.Key.X * 64, (int)item.Key.Y * 64, Game1.player);

                    // if (Game1.currentLocation.objects.ContainsKey(item.Key) && Game1.currentLocation.objects[item.Key].Name.Equals(itemToPlace.Name))
                    if (!successfullyPlaced)
                        RefundItem(item.Value.Item, I18n.SmartBuilding_Error_Object_PlacementFailed(), LogLevel.Error);
                }
            }
            else
            {
                RefundItem(item.Value.Item);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item">The item to be refunded to the player's inventory.</param>
        /// <param name="reason">The reason for the refund. This could be an error, or simply the player cancelling the build.</param>
        /// <param name="logLevel">The <see cref="StardewModdingAPI.LogLevel"/> to log with.</param>
        /// <param name="shouldLog">Whether or not to log. This is overridden by <see cref="StardewModdingAPI.LogLevel.Alert"/>, <see cref="StardewModdingAPI.LogLevel.Error"/>, and <see cref="StardewModdingAPI.LogLevel.Warn"/>.</param>
        private void RefundItem(Item item, string reason = "Something went wrong", LogLevel logLevel = LogLevel.Trace, bool shouldLog = false)
        {
            Game1.player.addItemByMenuIfNecessary(item.getOne());

            if (shouldLog || logLevel == LogLevel.Debug || logLevel == LogLevel.Error || logLevel == LogLevel.Warn || logLevel == LogLevel.Alert)
                monitor.Log($"{reason} {I18n.SmartBuilding_Error_Refunding_RefundingItemToPlayerInventory()} {item.Name}", logLevel);
        }

        /// <summary>
        /// Confirm the drawn build, and pass tiles and items into <see cref="PlaceObject"/>.
        /// </summary>
        public void ConfirmBuild()
        {
            // The build has been confirmed, so we iterate through our Dictionary, and pass each tile into PlaceObject.
            foreach (KeyValuePair<Vector2, ItemInfo> v in tilesSelected)
            {
                // We want to allow placement for the duration of this method.
                HarmonyPatches.Patches.AllowPlacement = true;

                PlaceObject(v);

                // And disallow it afterwards.
                HarmonyPatches.Patches.AllowPlacement = false;
            }

            // Then, we clear the list, because building is done, and all errors are handled internally.
            tilesSelected.Clear();
        }

        /// <summary>
        /// Clear all painted tiles.
        /// </summary>
        public void ClearBuild()
        {
            ClearPaintedTiles();
        }
    }
}