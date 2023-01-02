using System;
using System.Collections.Generic;
using DecidedlyShared.Logging;
using DecidedlyShared.Ui;
using DecidedlyShared.Utilities;
using HarmonyLib;
using MappingExtensionsAndExtraProperties.Models.TileProperties;
using MappingExtensionsAndExtraProperties.Models.TileProperties.FakeNpc;
using MappingExtensionsAndExtraProperties.Utils;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using xTile.ObjectModel;
using xTile.Tiles;

namespace MappingExtensionsAndExtraProperties;

public class ModEntry : Mod
{
    private Logger logger;
    private TilePropertyHandler tileProperties;

    public override void Entry(IModHelper helper)
    {
        var harmony = new Harmony(this.ModManifest.UniqueID);
        this.logger = new Logger(this.Monitor);
        this.tileProperties = new TilePropertyHandler(this.logger);
        Patches.InitialisePatches(this.logger,  this.tileProperties);

        helper.Events.Player.Warped += (sender, args) =>
        {
            // We need to ensure we can kill relevant UIs if the player is warping.
            if (Game1.activeClickableMenu is MenuBase menu)
            {
                // If it's one of our menus, we close it.
                // This should be refactored use an owned-menu system at some point.
                if (menu.MenuName.Equals(CloseupInteractionImage.PropertyKey))
                    Game1.exitActiveMenu();
            }

            // We want to add our fake NPCs to the new map at this point.


            // Then remove our fake NPCs from the previous map.
            Utils.Locations.RemoveFakeNpcs(args.OldLocation);
        };

        // This is where we kill all of our "fake" NPCs so they don't get serialised.
        helper.Events.GameLoop.Saving += (sender, args) =>
        {
            // We already do this manually whenever we leave a location, but this is something I want
            // extra security on.

            foreach (GameLocation location in Game1.locations)
            {
                Utils.Locations.RemoveFakeNpcs(location);
            }
        };

        // Our patch for handling interactions.
        harmony.Patch(
            AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction)),
            postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.GameLocation_CheckAction_Postfix)));

        // Our cursor draw patch for interaction highlights.
        harmony.Patch(
            AccessTools.Method(typeof(Game1), nameof(Game1.drawMouseCursor)),
            prefix: new HarmonyMethod(typeof(Patches), nameof(Patches.Game1_drawMouseCursor_Prefix)));

        // Super scary. Patching GameLocation's constructor.
        harmony.Patch(
            AccessTools.Constructor(typeof(GameLocation), new Type[] {typeof(string), typeof(string)}),
            postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.GameLocation_Ctor_Postfix)));

        // Our asset loading.
        helper.Events.Content.AssetRequested += (sender, args) =>
        {
            if (args.NameWithoutLocale.IsDirectlyUnderPath("MEEP/FakeNPC/Dialogue"))
            {
                args.LoadFrom(() => { return new Dictionary<string, string>();}, AssetLoadPriority.Low);
            }
        };

        // harmony.Patch(
        //     AccessTools.Method(typeof(ICollection<GameLocation>), nameof(Game1.locations.Add)),
        //     prefix: new HarmonyMethod(typeof(Patches), nameof(Patches.Game1_drawMouseCursor_Prefix)));

#if DEBUG
        helper.Events.Display.MenuChanged += (sender, args) =>
        {

        };

        helper.Events.Player.Warped += (sender, args) =>
        {
            int mapWidth = args.NewLocation.Map.DisplayWidth / Game1.tileSize;
            int mapHeight = args.NewLocation.Map.DisplayHeight / Game1.tileSize;

            for (int i = 0; i < mapWidth; i++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    Tile tile = args.NewLocation.Map.GetLayer("Back").Tiles.Array[i, y];

                    if (tile == null)
                        continue;

                    if (tile.Properties.TryGetValue(DhFakeNpc.PropertyKey, out PropertyValue property))
                    {
                        if (Parsers.TryParse(property.ToString(),
                                out DhFakeNpc fakeNpcProperty))
                        {
                            FakeNpc character = new FakeNpc(
                                new AnimatedSprite($"Characters\\{fakeNpcProperty.NpcName}",
                                    0,
                                    fakeNpcProperty.HasSpriteSizes ? fakeNpcProperty.SpriteWidth : 16,
                                    fakeNpcProperty.HasSpriteSizes ? fakeNpcProperty.SpriteHeight : 32),
                                new Vector2(i, y) * 64f,
                                2,
                                property.ToString()
                            );
                            // Dictionary<string, string> dialogue = Game1.content.Load<Dictionary<string, string>>($"MEEP/FakeNPC/Dialogue/{property.ToString()}");
                            Dictionary<string, string> dialogue =
                                helper.GameContent.Load<Dictionary<string, string>>(
                                    $"MEEP/FakeNPC/Dialogue/{fakeNpcProperty.NpcName}");

                            foreach (KeyValuePair<string, string> d in dialogue)
                            {
                                character.CurrentDialogue.Push(new Dialogue(d.Value, character));
                            }

                            args.NewLocation.characters.Add(character);
                        }
                    }
                }
            }
        };

        helper.Events.Input.ButtonPressed += (sender, args) =>
        {
            int cursorX = (int)Game1.currentCursorTile.X;
            int cursorY = (int)Game1.currentCursorTile.Y;
            GameLocation here = Game1.currentLocation;

            if (args.IsDown(SButton.OemSemicolon))
            {
                // Item furnace = ObjectFactory.getItemFromDescription(1, 13, 1);
                // Game1.currentLocation.Objects.Add(Game1.currentCursorTile, (SObject)furnace);
                FakeNpc character = new FakeNpc(
                    new AnimatedSprite("Characters\\NotAbigail", 0, 16, 32),
                    Game1.currentCursorTile * 64f,
                    2,
                    "NotAbigail"
                );

                string sheet = character.GetDialogueSheetName();

                // character.CurrentDialogue.Push(new Dialogue("Hello!", Game1.getCharacterFromName("Abigail")));
                // character.CurrentDialogue.Push(new Dialogue("Hello!", Game1.getCharacterFromName("Abigail")));
                // character.CurrentDialogue.Push(new Dialogue("How are you doing today?", Game1.getCharacterFromName("Abigail")));
                // character.CurrentDialogue.Push(new Dialogue("I'm glad to hear!", Game1.getCharacterFromName("Abigail")));
                // character.Dialogue.Add("Wed", "Thing");
                // character.Dialogue.Add();

                here.characters.Add(character);
            }
        };
#endif
    }
}
