﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SmartBuilding.Helpers;
using SmartBuilding.Patches;
using SmartBuilding.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

/* 
TODO: Comment this more heavily.
TODO: Implement correct spacing restrictions for fruit trees, etc. Should be relatively simple with a change to our adjacent tile detection method.
TODO: Split things into separate classes where it would make things neater.
TODO: Lots of minor optimisations. Move ItemType detection prior to CanBePlacedHere called.
*/

namespace SmartBuilding
{
	public class ModEntry : Mod, IAssetLoader
	{
		// SMAPI gubbins.
		private static IModHelper _helper;
		private static IMonitor _monitor;
		private static Logger _logger;
		private static ModConfig _config;

		private Dictionary<Vector2, ItemInfo> _tilesSelected = new Dictionary<Vector2, ItemInfo>();
		private Vector2 _currentTile = Vector2.Zero;
		private Vector2 _hudPosition;
		private Texture2D _buildingHud;
		private Texture2D _itemBox;
		private bool _currentlyDrawing = false;
		private bool _currentlyErasing = false;
		private bool _buildingMode = false;
		private bool _toolbarFlipped = false;
		private int _itemBarWidth = 800; // This is the default.

		/// <summary>
		/// Basic Item metadata.
		/// </summary>
		private struct ItemInfo
		{
			/// <summary>
			/// The item to be placed.
			/// </summary>
			public Item item;
			/// <summary>
			/// The correct spritesheet for the image.
			/// </summary>
			public Texture2D spriteSheet;
			/// <summary>
			/// The basic type of item that it is, determined by <see cref="ModEntry.IdentifyItemType"/>
			/// </summary>
			public ItemType itemType;
			public int sheetId;
			public int inventoryIndex;
		}

		/// <summary>
		/// The type of item to be placed.
		/// </summary>
		private enum ItemType
		{
			/// <summary>
			/// A Stardew Valley Fence. This is a special case, so we need to be able to identify a fence specifically.
			/// </summary>
			Fence,
			/// <summary>
			/// A Stardew Valley floor. A TerrainFeature.
			/// </summary>
			Floor,
			/// <summary>
			/// A Stardew Valley chest. This needs somewhat special handling.
			/// </summary>
			Chest,
			/// <summary>
			/// A Stardew Valley grass starter.
			/// </summary>
			GrassStarter,
			/// <summary>
			/// A Stardew Valley crab pot.
			/// </summary>
			CrabPot,
			/// <summary>
			/// Since seeds need very special treatment, this is important.
			/// </summary>
			Seed,
			/// <summary>
			/// Fertilizers also require special treatment.
			/// </summary>
			Fertilizer,
			/// <summary>
			/// Tree fertilizers also require special treatment.
			/// </summary>
			TreeFertilizer,
			/// <summary>
			/// Tappers need slightly special logic.
			/// </summary>
			Tapper,
			/// <summary>
			/// A generic placeable object.
			/// </summary>
			Generic
		}

		#region Asset Loading Gubbins

		public bool CanLoad<T>(IAssetInfo asset)
		{
			return asset.AssetNameEquals("Mods/DecidedlyHuman/BuildingHUD");
		}

		public T Load<T>(IAssetInfo asset)
		{ // We can just return this, because this mod can load only a single asset.
			return this.Helper.Content.Load<T>(Path.Combine("assets", "HUD.png"));
		}

		#endregion

		public override void Entry(IModHelper helper)
		{
			_helper = helper;
			_monitor = Monitor;
			_logger = new Logger(_monitor);
			_config = _helper.ReadConfig<ModConfig>();
			_hudPosition = new Vector2(50, 0);
			_buildingHud = _helper.Content.Load<Texture2D>("Mods/DecidedlyHuman/BuildingHUD", ContentSource.GameContent);
			 _itemBox = _helper.Content.Load<Texture2D>("LooseSprites/tailoring", ContentSource.GameContent);
			
			//_itemBox = _helper.Content.Load<Texture2D>("LooseSprites/Cursors", ContentSource.GameContent);
			
			Harmony harmony = new Harmony(ModManifest.UniqueID);

			harmony.Patch(
				original: AccessTools.Method(typeof(Object), nameof(Object.placementAction)),
				prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(Patches.ObjectPatches.PlacementAction_Prefix)));

			// TODO: Consider refactoring input events to simply use ButtonsChanged, since we're using KeybindLists and not SButtons.
			_helper.Events.GameLoop.GameLaunched += GameLaunched;
			_helper.Events.Input.ButtonPressed += ButtonPressed;
			_helper.Events.Input.ButtonReleased += ButtonReleased;
			_helper.Events.Input.CursorMoved += CursorMoved;
			_helper.Events.Display.RenderedWorld += RenderedWorld;
			_helper.Events.Display.RenderedHud += RenderedHud;

			// If the screen is changed, clear our painted tiles, because currently, placing objects is done on the current screen.
			_helper.Events.Player.Warped += (sender, args) =>
			{
				ClearPaintedTiles();
				_buildingMode = false;
				_currentlyDrawing = false;
				ObjectPatches.CurrentlyDrawing = false;
			};
		}

		private void GameLaunched(object sender, GameLaunchedEventArgs e)
		{
			RegisterWithGmcm();
		}

		private void RegisterWithGmcm()
		{
			GenericModConfigMenuApi configMenuApi =
				Helper.ModRegistry.GetApi<GenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

			if (configMenuApi == null)
			{
				_logger.Log("The user doesn't have GMCM installed. This is not an error.", LogLevel.Info);

				return;
			}

			configMenuApi.Register(ModManifest,
				() => _config = new ModConfig(),
				() => Helper.WriteConfig(_config));

			configMenuApi.AddSectionTitle(
				mod: ModManifest,
				text: () => "Keybinds"
			);

			configMenuApi.AddParagraph(
				mod: ModManifest,
				text: () => "GMCM currently doesn't support adding mouse keybinds in its config menus. In the meantime, refer to the second page for advice on editing the config.json file to add them manually."
			);

			configMenuApi.AddKeybindList(
				mod: ModManifest,
				name: () => "Engage build mode",
				getValue: () => _config.EngageBuildMode,
				setValue: value => _config.EngageBuildMode = value);

			configMenuApi.AddKeybindList(
				mod: ModManifest,
				name: () => "Hold to draw",
				getValue: () => _config.HoldToDraw,
				setValue: value => _config.HoldToDraw = value);

			configMenuApi.AddKeybindList(
				mod: ModManifest,
				name: () => "Hold to erase",
				getValue: () => _config.HoldToErase,
				setValue: value => _config.HoldToErase = value);

			configMenuApi.AddKeybindList(
				mod: ModManifest,
				name: () => "Confirm build",
				getValue: () => _config.ConfirmBuild,
				setValue: value => _config.ConfirmBuild = value);
			
			configMenuApi.AddBoolOption(
				mod: ModManifest,
				name: () => "Show Build Queue",
				getValue: () => _config.ShowBuildQueue,
				setValue: value => _config.ShowBuildQueue = value
				);

			configMenuApi.AddSectionTitle(
				mod: ModManifest,
				text: () => "The Slightly Cheaty Zone"
			);

			configMenuApi.AddBoolOption(
				mod: ModManifest,
				name: () => "Place crab pots in any water tile",
				getValue: () => _config.CrabPotsInAnyWaterTile,
				setValue: b => _config.CrabPotsInAnyWaterTile = b
			);

			configMenuApi.AddBoolOption(
				mod: ModManifest,
				name: () => "Allow planting crops",
				getValue: () => _config.EnablePlantingCrops,
				setValue: b => _config.EnablePlantingCrops = b
			);

			configMenuApi.AddBoolOption(
				mod: ModManifest,
				name: () => "Allow fertilizing crops",
				getValue: () => _config.EnableCropFertilizers,
				setValue: b => _config.EnableCropFertilizers = b
			);

			configMenuApi.AddBoolOption(
				mod: ModManifest,
				name: () => "Allow fertilizing trees",
				getValue: () => _config.EnableTreeFertilizers,
				setValue: b => _config.EnableTreeFertilizers = b
			);
			
			configMenuApi.AddBoolOption(
				mod: ModManifest,
				name: () => "Allow tree tappers",
				getValue: () => _config.EnableTreeTappers,
				setValue: b => _config.EnableTreeTappers = b
			);

			configMenuApi.AddPageLink(
				mod: ModManifest,
				pageId: "JsonGuide",
				text: () => "(Click me!) A short guide on adding mouse bindings."
			);

			configMenuApi.AddPage(
				mod: ModManifest,
				pageId: "JsonGuide",
				pageTitle: () => "Mouse Key Bindings"
			);

			configMenuApi.AddParagraph(
				mod: ModManifest,
				text: () => "From: https://stardewvalleywiki.com/Modding:Player_Guide/Key_Bindings#Multi-key_bindings"
			);

			configMenuApi.AddParagraph(
				mod: ModManifest,
				text: () => "Mods using SMAPI 3.9+ features can support multi-key bindings. That lets you combine multiple button codes into a combo keybind, and list alternate keybinds. For example, \"LeftShoulder, LeftControl + S\" will apply if LeftShoulder is pressed, or if both LeftControl and S are pressed."
			);

			configMenuApi.AddParagraph(
				mod: ModManifest,
				text: () => "Some things to keep in mind:"
			);

			configMenuApi.AddParagraph(
				mod: ModManifest,
				text: () => "The order doesn't matter, so \"LeftControl + S\" and \"S + LeftControl\" are equivalent."
			);

			configMenuApi.AddParagraph(
				mod: ModManifest,
				text: () => "SMAPI doesn't prevent mods from using overlapping hotkeys. For example, if one mod uses \"S\" and the other mod uses \"LeftControl + S\", pressing LeftControl and S will activate both."
			);
		}

		// TODO: Actually comment things in this method.
		private void RenderedHud(object? sender, RenderedHudEventArgs e)
		{
			if (_buildingMode)
			{ // There's absolutely no need to run this while we're not in building mode.
				int windowWidth = Game1.game1.Window.ClientBounds.Width;

				// TODO: Use the newer logic I have to get the toolbar position for this.
				_hudPosition = new Vector2(
					(windowWidth / 2) - (_itemBarWidth / 2) - _buildingHud.Width * 4,
					0);

				e.SpriteBatch.Draw(
					texture: _buildingHud,
					position: _hudPosition,
					sourceRectangle: _buildingHud.Bounds,
					color: Color.White,
					rotation: 0f,
					origin: Vector2.Zero,
					scale: Game1.pixelZoom,
					effects: SpriteEffects.None,
					layerDepth: 1f
				);

				if (_config.ShowBuildQueue)
				{
					Dictionary<Item, int> itemAmounts = new Dictionary<Item, int>();
					
					foreach (var item in _tilesSelected.Values.GroupBy(x => x))
					{
						itemAmounts.Add(item.Key.item, item.Count());
					}

					float screenWidth, screenHeight;
					screenWidth = Game1.viewport.Width * Game1.options.zoomLevel;
					screenHeight = Game1.viewport.Height * Game1.options.zoomLevel;
					Vector2 startingPoint = new Vector2();
					
					#region Shameless decompile copy
					
					Point playerGlobalPosition = Game1.player.GetBoundingBox().Center;
					Vector2 playerLocalVector = Game1.GlobalToLocal(globalPosition: new Vector2(playerGlobalPosition.X, playerGlobalPosition.Y), viewport: Game1.viewport);
					bool toolbarAtTop = ((playerLocalVector.Y > (float)(Game1.viewport.Height / 2 + 64)) ? true : false);
					
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
							texture: _itemBox,
							position: startingPoint,
							sourceRectangle: new Rectangle(0, 128, 24, 24),
							color: Color.White,
							rotation: 0f,
							origin: Vector2.Zero,
							scale: Game1.pixelZoom,
							effects: SpriteEffects.None,
							layerDepth: 1f
						);
						
						// e.SpriteBatch.Draw(
						// 	texture: _itemBox,
						// 	position: startingPoint,
						// 	sourceRectangle: new Rectangle(293, 360, 36, 24),
						// 	color: Color.White,
						// 	rotation: 0f,
						// 	origin: Vector2.Zero,
						// 	scale: Game1.pixelZoom,
						// 	effects: SpriteEffects.None,
						// 	layerDepth: 1f
						// );
						
						item.Key.drawInMenu(
							e.SpriteBatch,
							startingPoint + new Vector2(17, 16),
							0.75f, 1f, 4f, StackDrawType.Hide);

						// Utility.drawTextWithShadow(
						// 	b: e.SpriteBatch,
						// 	text: item.Value.ToString(),
						// 	font: Game1.smallFont,
						// 	position: startingPoint + new Vector2(4, 24 * Game1.pixelZoom),
						// 	color: Color.White
						// 	);
						
						DrawStringWithShadow(
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
			}
		}

		private void DrawStringWithShadow(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color textColour, Color shadowColour)
		{
			spriteBatch.DrawString(
				spriteFont: font,
				text: text,
				position: position + new Vector2(2, 2),
				shadowColour
				);
						
			spriteBatch.DrawString(
				spriteFont: font,
				text: text,
				position: position,
				textColour
				);
		}

		private void CursorMoved(object sender, CursorMovedEventArgs e)
		{
			// If the player is holding down the draw keybind, we want to call AddTile to see if we can
			// add the selected item to the tile under the cursor.
			if (_currentlyDrawing)
			{
				int inventoryIndex = Game1.player.getIndexOfInventoryItem(Game1.player.CurrentItem);

				AddTile(Game1.player.CurrentItem, Game1.currentCursorTile, inventoryIndex);
			}

			// If the player is holding the erase keybind, we want to see if we can remove a registered tile
			// under the cursor, and refund the item where applicable.
			if (_currentlyErasing)
			{
				Vector2 v = Game1.currentCursorTile;
				Vector2 flaggedForRemoval = new Vector2();

				foreach (var item in _tilesSelected)
				{
					if (item.Key == v)
					{ // If we're over a tile in _tilesSelected, remove it and refund the item to the player.
						Game1.player.addItemToInventoryBool(item.Value.item.getOne(), false);
						_monitor.Log($"Refunding {item.Value.item.Name} back into player's inventory.");

						// And flag it for removal from the queue, since we can't remove from within the foreach.
						flaggedForRemoval = v;
					}
				}

				_tilesSelected.Remove(flaggedForRemoval);
			}
		}

		/// <summary>
		/// Will return whether or not a tile can be placed 
		/// </summary>
		/// <param name="v">The world-space Tile in which the check is to be performed.</param>
		/// <param name="i">The placeable type.</param>
		/// <returns></returns>
		private bool CanBePlacedHere(Vector2 v, Item i)
		{
			// TODO: Ensure all logic in here flows the same way. Some things are slightly different because of being written at different times.
			ItemType itemType = IdentifyItemType((Object)i);
			GameLocation here = Game1.currentLocation;

			// TODO: Add logic to allow for placing floors underneath fences.
			switch (itemType)
			{
				case ItemType.CrabPot: // We need to determine if the crab pot is being placed in an appropriate water tile.
					return CrabPot.IsValidCrabPotLocationTile(here, (int)v.X, (int)v.Y) && HasAdjacentNonWaterTile(v);
				case ItemType.GrassStarter:

					// If there's a terrain feature here, we can't possibly place a grass starter.
					if (here.terrainFeatures.ContainsKey(v))
						return false;
					else
						return true;

				case ItemType.Floor:
					// In this case, we need to know whether there's a TerrainFeature in the tile.
					// isTileLocationTotallyClearAndPlaceable ignores TerrainFeatures, it seems.
					bool isFloorPlaceable = false;

					isFloorPlaceable = !here.terrainFeatures.ContainsKey(v);

					if (!here.isTileLocationTotallyClearAndPlaceable(v))
					{
						if (here.Objects.ContainsKey(v))
						{
							if (here.Objects[v].Name.Contains("Fence") ||
								here.Objects[v].Name.Contains("Wall"))
								return true;
						}

						return false;
					}

					return isFloorPlaceable;
				case ItemType.Chest:
					return !i.Name.Equals("Junimo Chest"); // This is very hackish. TODO: Move this Junimo Chest blocking logic further up the chain.
				case ItemType.Fertilizer:
					// If the setting to enable fertilizers is off, return false to ensure they can't be added to the queue.
					if (!_config.EnableCropFertilizers)
						return false;

					// If there's a fence, etc., here, we don't want to place fertiliser.
					// It is technically valid, but there's no reason someone would want to.
					if (here.Objects.ContainsKey(v))
						return false;

					if (here.terrainFeatures.ContainsKey(v))
					{
						// We know there's a TerrainFeature here, so next we want to check if it's HoeDirt.
						if (here.terrainFeatures[v] is HoeDirt)
						{
							// If it is, we want to grab the HoeDirt, and check for the possibility of planting.
							HoeDirt hd = (HoeDirt)here.terrainFeatures[v];

							if (hd.crop != null)
							{
								// If the HoeDirt has a crop, we want to grab it and check for growth phase and fertilization status.
								Crop cropToCheck = hd.crop;

								if (cropToCheck.currentPhase.Value != 0)
								{
									// If the crop's current phase is not zero, we return false.

									return false;
								}
							}
							
							return hd.canPlantThisSeedHere(i.ParentSheetIndex, (int)v.X, (int)v.Y, true);
						}
					}

					return false;
				case ItemType.TreeFertilizer:
					// If the setting to enable tree fertilizers is off, return false to ensure they can't be added to the queue.
					if (!_config.EnableTreeFertilizers)
						return false;

					if (here.terrainFeatures.ContainsKey(v))
					{
						// If there's a TerrainFeature here, we check if it's a tree.
						if (here.terrainFeatures[v] is Tree)
						{
							// It is a tree, so now we check to see if the tree is fertilised.
							Tree tree = (Tree)here.terrainFeatures[v];

							// If it's already fertilised, there's no need for us to want to place tree fertiliser on it, so we return false.
							if (tree.fertilized.Value)
								return false;
							else
								return true;
						}
					}

					return false;
				case ItemType.Seed:
					// If the setting to enable crops is off, return false to ensure they can't be added to the queue.
					if (!_config.EnablePlantingCrops)
						return false;

					if (here.terrainFeatures.ContainsKey(v))
					{
						if (here.terrainFeatures[v] is HoeDirt)
						{
							HoeDirt hd = (HoeDirt)here.terrainFeatures[v];

							return hd.canPlantThisSeedHere(i.ParentSheetIndex, (int)v.X, (int)v.Y);
						}
					}

					return false;
				case ItemType.Tapper:
					// If the setting to enable tree tappers is off, we return false here to ensure nothing further happens.
					if (!_config.EnableTreeTappers)
						return false;

					if (here.terrainFeatures.ContainsKey(v))
					{
						if (here.terrainFeatures[v] is Tree)
						{
							Tree tree = (Tree)here.terrainFeatures[v];

							// If the tree isn't tapped, we confirm that a tapper can be placed here.
							if (!tree.tapped)
							{
								if (tree.growthStage >= 5)
								{
									// If the tree is fully grown, we *can* place a tapper.
									return true;
								}

								return false;
							}
						}
					}

					return false;
				case ItemType.Fence:
				case ItemType.Generic:
					return Game1.currentLocation.isTileLocationTotallyClearAndPlaceableIgnoreFloors(v);
			}

			// If the PlaceableType is somehow neither of these, we want to be safe and return false.
			return false;
		}

		private ItemType IdentifyItemType(Object item)
		{
			// TODO: Make this detection more robust. If possible, don't depend upon it at all.
			string itemName = item.Name;

			// The whole point of this is to determine whether the object being placed requires
			// special treatment at all, and assist us in determining whether it's a TerrainFeature, or an Object.
			if (itemName.Contains("Floor") || itemName.Contains("Path"))
				return ItemType.Floor;
			else if (itemName.Contains("Chest") && !itemName.Contains("Junimo"))
				return ItemType.Chest;
			else if (itemName.Contains("Fence"))
				return ItemType.Fence;
			else if (itemName.Equals("Grass Starter"))
				return ItemType.GrassStarter;
			else if (itemName.Equals("Crab Pot"))
				return ItemType.CrabPot;
			else if (item.Type == "Seeds")
				return ItemType.Seed;
			else if (item.Name.Equals("Tree Fertilizer"))
				return ItemType.TreeFertilizer;
			else if (item.Category == -19)
				return ItemType.Fertilizer;
			else if (item.Name.Equals("Tapper") || item.Name.Equals("Heavy Tapper"))
				return ItemType.Tapper;

			return ItemType.Generic;
		}

		private ItemInfo GetItemInfo(Object item, int itemInventoryIndex)
		{
			// Here, we pull the correct sprite sheet out of the Item, based upon whether
			// it's a BigCraftable or not.
			Texture2D itemSpriteSheet;
			int itemSheetId;
			ItemType itemType = IdentifyItemType(item);

			if (item.bigCraftable.Value)
			{
				itemSpriteSheet = Game1.bigCraftableSpriteSheet;
			}
			else
			{
				itemSpriteSheet = Game1.objectSpriteSheet;
			}

			itemSheetId = item.ParentSheetIndex;

			return new ItemInfo()
			{
				item = item,
				spriteSheet = itemSpriteSheet,
				sheetId = itemSheetId,
				inventoryIndex = itemInventoryIndex,
				itemType = itemType
			};
		}

		private void AddTile(Item item, Vector2 v, int itemInventoryIndex)
		{
			// We're not in building mode, so we do nothing.
			if (!_buildingMode)
				return;

			// If the player isn't holding an item, we do nothing.
			if (Game1.player.CurrentItem == null)
				return;

			// If the item isn't placeable, we do nothing.
			if (!item.isPlaceable())
				return;

			// If the item cannot be placed here according to our own rules, we do nothing. This is to allow for slightly custom placement logic.
			if (!CanBePlacedHere(v, item))
				return;

			ItemInfo itemInfo = GetItemInfo((Object)item, itemInventoryIndex);

			// We only want to add the tile if the Dictionary doesn't already contain it. 
			if (!_tilesSelected.ContainsKey(v))
			{
				//Next, we want to check if the item is flooring intended to be placed on a tile that already has a TerrainFeature.
				if (itemInfo.itemType == ItemType.Floor)
				{
					// We're dealing with a floor, so we need to check if the target tile has a terrain feature on it.
					if (Game1.currentLocation.terrainFeatures.ContainsKey(v))
					{
						// The tile has a terrain feature on it, so we want to simply return. Otherwise, we can fall through and continue as normal.
						return;
					}
				}

				_tilesSelected.Add(v, itemInfo);
				Game1.player.reduceActiveItemByOne();
			}
		}

		private bool HasAdjacentNonWaterTile(Vector2 v)
		{
			// Although crab pots are the only currently tested object that
			// go in water, I do want to modularise this later.
			// TODO: Modularise for not only crab pots.

			if (_config.CrabPotsInAnyWaterTile)
				return true;

			List<Vector2> directions = new List<Vector2>()
			{
				v + new Vector2(-1, 0), // Left
				v + new Vector2(1, 0), // Right
				v + new Vector2(0, -1), // Up
				v + new Vector2(0, 1), // Down
				v + new Vector2(-1, -1), // Up left
				v + new Vector2(1, -1), // Up right
				v + new Vector2(-1, 1), // Down left
				v + new Vector2(1, 1) // Down right
			};

			foreach (Vector2 vector in directions)
			{
				if (!Game1.currentLocation.isWaterTile((int)vector.X, (int)vector.Y))
					return true;
			}

			return false;
		}

		private void RenderedWorld(object sender, RenderedWorldEventArgs e)
		{
			foreach (KeyValuePair<Vector2, ItemInfo> item in _tilesSelected)
			{
				// Here, we simply have the Item draw itself in the world.
				item.Value.item.drawInMenu
				(e.SpriteBatch,
					Game1.GlobalToLocal(
						Game1.viewport,
						item.Key * Game1.tileSize),
					1f, 1f, 4f, StackDrawType.Hide);
			}
		}

		private void ButtonReleased(object sender, ButtonReleasedEventArgs e)
		{
			if (_config.HoldToDraw.GetState() == SButtonState.Released)
			{
				_currentlyDrawing = false;
				ObjectPatches.CurrentlyDrawing = false;
			}

			if (_config.HoldToErase.GetState() == SButtonState.Released)
			{
				_currentlyErasing = false;
			}
		}

		private void ClearPaintedTiles()
		{
			// To clear the painted tiles, we want to iterate through our Dictionary, and refund every item contained therein.
			foreach (var t in _tilesSelected)
			{
				RefundItem(t.Value.item, "User left build mode. Refunding items.", LogLevel.Trace, false);
			}

			// And, finally, clear it.
			_tilesSelected.Clear();
		}

		private void ButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			// If the world isn't ready, we definitely don't want to do anything.
			if (!Context.IsWorldReady)
				return;

			// If a menu is up, we don't want any of our controls to do anything.
			if (Game1.activeClickableMenu != null)
				return;

			// This is not a hold situation, so we want JustPressed here.
			if (_config.EngageBuildMode.JustPressed())
			{
				_buildingMode = !_buildingMode;

				if (!_buildingMode) // If this is now false, we want to clear the tiles list, and refund everything.
				{
					ClearPaintedTiles();
				}
			}

			if (_config.HoldToDraw.JustPressed())
			{
				if (_buildingMode)
				{
					_currentlyDrawing = true;
					ObjectPatches.CurrentlyDrawing = _currentlyDrawing;

					int inventoryIndex = Game1.player.getIndexOfInventoryItem(Game1.player.CurrentItem);
					AddTile(Game1.player.CurrentItem, Game1.currentCursorTile, inventoryIndex);
				}
			}

			if (_config.HoldToErase.JustPressed())
			{
				_currentlyErasing = true;

				Vector2 v = Game1.currentCursorTile;
				Vector2 flaggedForRemoval = new Vector2();

				foreach (var item in _tilesSelected)
				{
					if (item.Key == v)
					{ // If we're over a tile in _tilesSelected, remove it and refund the item to the player.
						Game1.player.addItemToInventoryBool(item.Value.item.getOne(), false);
						_monitor.Log($"Refunding {item.Value.item.Name} back into player's inventory.");

						// And flag it for removal from the queue, since we can't remove from within the foreach.
						flaggedForRemoval = v;
					}
				}

				_tilesSelected.Remove(flaggedForRemoval);
			}

			if (_config.ConfirmBuild.JustPressed())
			{
				// The build has been confirmed, so we iterate through our Dictionary, and pass each tile into PlaceObject.
				foreach (KeyValuePair<Vector2, ItemInfo> v in _tilesSelected)
				{
					PlaceObject(v);
				}

				// Then, we clear the list, because building is done.
				_tilesSelected.Clear();
			}
		}

		// TODO for this method: Refactor detecting placement success to use the bool returned from the placement method.
		private void PlaceObject(KeyValuePair<Vector2, ItemInfo> item)
		{
			Object itemToPlace = (Object)item.Value.item;
			Vector2 targetTile = item.Key;
			ItemInfo itemInfo = item.Value;
			GameLocation here = Game1.currentLocation;

			if (itemToPlace != null && CanBePlacedHere(targetTile, itemInfo.item))
			{ // The item can be placed here.
				if (itemInfo.itemType == ItemType.Floor)
				{
					// We're specifically dealing with a floor/path.

					int? floorType = GetFlooringType(itemToPlace.Name);
					Flooring floor;

					if (floorType.HasValue)
						floor = new Flooring(floorType.Value);
					else
					{ // At this point, something is very wrong, so we want to refund the item to the player's inventory, and print an error.
						RefundItem(itemToPlace);

						return;
					}

					// At this point, we *need* there to be no TerrainFeature present.
					if (!Game1.currentLocation.terrainFeatures.ContainsKey(item.Key))
						Game1.currentLocation.terrainFeatures.Add(item.Key, floor);
					else
					{
						RefundItem(item.Value.item, "There was already a TerrainFeature present. Maybe you hoed the ground before confirming the build");

						// We now want to jump straight out of this method, because this will flow through to the below if, and bad things will happen.
						return;
					}

					if (Game1.currentLocation.terrainFeatures.ContainsKey(item.Key))
					{
						Flooring flooring = (Flooring)Game1.currentLocation.terrainFeatures[item.Key];
					}
					else
						RefundItem(item.Value.item);
				}
				else if (itemInfo.itemType == ItemType.Chest)
				{
					// We're dealing with a chest.
					int? chestType = GetChestType(itemToPlace.Name);
					Chest chest;

					if (chestType.HasValue)
					{
						chest = new Chest(true, chestType.Value);
					}
					else
					{ // At this point, something is very wrong, so we want to refund the item to the player's inventory, and print an error.
						RefundItem(itemToPlace);

						return;
					}

					bool placed = chest.placementAction(Game1.currentLocation, (int)item.Key.X * 64, (int)item.Key.Y * 64, Game1.player);

					if (Game1.currentLocation.objects.ContainsKey(item.Key) && Game1.currentLocation.objects[item.Key].Name.Equals(itemToPlace.Name))
					{
						
					}
					else
						RefundItem(item.Value.item);
				}
				else if (itemInfo.itemType == ItemType.Fence)
				{
					// We're dealing with a fence.

					itemToPlace.placementAction(Game1.currentLocation, (int)item.Key.X * 64, (int)item.Key.Y * 64, Game1.player);

					if (Game1.currentLocation.objects.ContainsKey(item.Key) && Game1.currentLocation.objects[item.Key].Name.Equals(itemToPlace.Name))
					{
						
					}
					else
						RefundItem(item.Value.item);
				}
				else if (itemInfo.itemType == ItemType.GrassStarter)
				{
					Grass grassStarter = new Grass(1, 4);

					// At this point, we *need* there to be no TerrainFeature present.
					if (!Game1.currentLocation.terrainFeatures.ContainsKey(item.Key))
						Game1.currentLocation.terrainFeatures.Add(item.Key, grassStarter);
					else
					{
						RefundItem(item.Value.item, "There was already a TerrainFeature present. Maybe you hoed the ground before confirming the build");

						// We now want to jump straight out of this method, because this will flow through to the below if, and bad things will happen.
						return;
					}

					if (Game1.currentLocation.terrainFeatures.ContainsKey(item.Key))
					{

					}
					else
						RefundItem(item.Value.item);
				}
				else if (itemInfo.itemType == ItemType.CrabPot)
				{
					CrabPot pot = new CrabPot(item.Key);

					if (CanBePlacedHere(item.Key, itemToPlace))
					{
						itemToPlace.placementAction(Game1.currentLocation, (int)item.Key.X * 64, (int)item.Key.Y * 64, Game1.player);
					}
				}
				else if (itemInfo.itemType == ItemType.Seed)
				{
					// Here, we're dealing with a seed, so we need very special logic for this.
					// Item.placementAction for seeds is broken, unless the player is currently
					// holding the specific seed being planted.

					bool successfullyPlaced = false;

					if (Game1.currentLocation.terrainFeatures.ContainsKey(targetTile))
					{
						if (Game1.currentLocation.terrainFeatures[targetTile] is HoeDirt)
						{
							HoeDirt hd = (HoeDirt)Game1.currentLocation.terrainFeatures[targetTile];

							if (hd.canPlantThisSeedHere(itemToPlace.ParentSheetIndex, (int)item.Key.X, (int)item.Key.Y))
							{
								successfullyPlaced = hd.plant(itemToPlace.ParentSheetIndex, (int)item.Key.X, (int)item.Key.Y, Game1.player, false, Game1.currentLocation);
							}
						}
					}

					if (!successfullyPlaced)
						RefundItem(item.Value.item);
				}
				else if (itemInfo.itemType == ItemType.Fertilizer)
				{
					if (Game1.currentLocation.terrainFeatures.ContainsKey(targetTile))
					{
						// We know there's a TerrainFeature here, so next we want to check if it's HoeDirt.
						if (Game1.currentLocation.terrainFeatures[targetTile] is HoeDirt)
						{
							// If it is, we want to grab the HoeDirt, check if it's already got a fertiliser, and fertilise if not.
							HoeDirt hd = (HoeDirt)Game1.currentLocation.terrainFeatures[targetTile];

							// 0 here means no fertilizer.
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
								RefundItem(itemToPlace, "There was already fertilizer placed here", LogLevel.Warn);
							}
							
							// Now, we want to run the final check to see if the fertilization was successful.
							if (hd.fertilizer.Value == 0)
							{
								// If there's still no fertilizer here, we need to refund the item.
								RefundItem(itemToPlace, "There was either fertilizer already here, or the crop is too grown to accept fertilizer", LogLevel.Warn);
							}
						}
					}

				}
				else if (itemInfo.itemType == ItemType.TreeFertilizer)
				{
					if (Game1.currentLocation.terrainFeatures.ContainsKey(targetTile))
					{
						// If there's a TerrainFeature here, we check if it's a tree.
						if (Game1.currentLocation.terrainFeatures[targetTile] is Tree)
						{
							// It is a tree, so now we check to see if the tree is fertilised.
							Tree tree = (Tree)Game1.currentLocation.terrainFeatures[targetTile];

							// If it's already fertilised, there's no need for us to want to place tree fertiliser on it.
							if (!tree.fertilized.Value)
								tree.fertilize(Game1.currentLocation);
						}
					}
				}
				else if (itemInfo.itemType == ItemType.Tapper)
				{
					if (here.terrainFeatures.ContainsKey(targetTile))
					{
						if (here.terrainFeatures[targetTile] is Tree)
						{
							Tree tree = (Tree)here.terrainFeatures[targetTile];

							if (!tree.tapped.Value)
							{
								if (!itemToPlace.placementAction(here, (int)targetTile.X * 64, (int)targetTile.Y * 64, Game1.player))
								{
									// If the placement action didn't succeed, we refund the item.
									RefundItem(itemToPlace);
								}
							}
						}
					}
				}
				else
				{ // We're dealing with a generic placeable.
					bool successfullyPlaced = itemToPlace.placementAction(Game1.currentLocation, (int)item.Key.X * 64, (int)item.Key.Y * 64, Game1.player);

					// if (Game1.currentLocation.objects.ContainsKey(item.Key) && Game1.currentLocation.objects[item.Key].Name.Equals(itemToPlace.Name))
					if (successfullyPlaced)
					{
						
					}
					else
						RefundItem(item.Value.item);
				}
			}
			else
			{
				RefundItem(item.Value.item);
			}
		}

		private void RefundItem(Item item, string reason = "Something went wrong", LogLevel logLevel = LogLevel.Error, bool shouldLog = false)
		{
			Game1.player.addItemToInventoryBool(item.getOne(), false);
			
			if (shouldLog)
				_monitor.Log($"{reason}. Refunding {item.Name} back into player's inventory.", logLevel);
		}

		private int? GetFlooringType(string itemName)
		{
			// TODO: Investigate whether or not there's a less terrible way to do this.
			switch (itemName)
			{
				case "Wood Floor":
					return 0; // Correct.
				case "Rustic Plank Floor":
					return 11; // Correct.
				case "Straw Floor":
					return 4; // Correct
				case "Weathered Floor":
					return 2; // Correct.
				case "Crystal Floor":
					return 3; // Correct.
				case "Stone Floor":
					return 1; // Correct.
				case "Stone Walkway Floor":
					return 12; // Correct.
				case "Brick Floor":
					return 10; // Correct
				case "Wood Path":
					return 6; // Correct.
				case "Gravel Path":
					return 5; // Correct.
				case "Cobblestone Path":
					return 8; // Correct.
				case "Stepping Stone Path":
					return 9; // Correct.
				case "Crystal Path":
					return 7; // Correct.
				default:
					return null;
			}
		}

		private int? GetChestType(string itemName)
		{
			// TODO: Investigate whether or not there's a less terrible way to do this.
			switch (itemName)
			{
				case "Chest":
					return 130;
				case "Stone Chest":
					return 232;
				default:
					return null;
			}
		}
	}
}