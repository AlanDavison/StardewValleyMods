using System;
using System.Collections.Generic;
using DecidedlyShared.APIs;
using DecidedlyShared.Logging;
using DecidedlyShared.Ui;
using DecidedlyShared.Utilities;
using HarmonyLib;
using MappingExtensionsAndExtraProperties.Api;
using MappingExtensionsAndExtraProperties.Functionality;
using MappingExtensionsAndExtraProperties.Models.EventCommands;
using MappingExtensionsAndExtraProperties.Models.TileProperties;
using MappingExtensionsAndExtraProperties.Patches;
using MappingExtensionsAndExtraProperties.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    private MeepApi api;
    private List<FakeNpc> allNpcs;
    private ISaveAnywhereApi saveAnywhereApi;
    private ISpaceCoreApi spaceCoreApi;
    private EventCommands eventCommands;

    public override void Entry(IModHelper helper)
    {
        var harmony = new Harmony(this.ModManifest.UniqueID);
        this.logger = new Logger(this.Monitor);
        this.tileProperties = new TilePropertyHandler(this.logger);
        this.allNpcs = new List<FakeNpc>();
        EventPatches.InitialisePatches(this.logger, this.tileProperties);
        Game1Patches.InitialisePatches(this.logger, this.tileProperties);
        GameLocationPatches.InitialisePatches(this.logger, this.tileProperties);
        SObjectPatches.InitialisePatches(this.logger, this.tileProperties);
        Parsers.InitialiseParsers(this.logger, helper);
        this.eventCommands = new EventCommands(this.Helper, this.logger);

        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

        // This is where we kill all of our "fake" NPCs so they don't get serialised.
        helper.Events.GameLoop.DayEnding += this.OnDayEnding;

        // Our asset loading.
        helper.Events.Content.AssetRequested += (sender, args) =>
        {
            if (args.NameWithoutLocale.IsDirectlyUnderPath("MEEP/FakeNPC/Dialogue"))
            {
                args.LoadFrom(() => { return new Dictionary<string, string>(); }, AssetLoadPriority.Low);
            }
        };

        // Our patch for handling interactions.
        harmony.Patch(
            AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction)),
            postfix: new HarmonyMethod(typeof(GameLocationPatches), nameof(GameLocationPatches.GameLocation_CheckAction_Postfix)));

        // harmony.Patch(
        //     AccessTools.Method(typeof(Event), nameof(Event.checkAction)),
        //     postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.Event_CheckAction_Postfix)));
        //
        // harmony.Patch(
        //     AccessTools.Method(typeof(Event), nameof(Event.receiveActionPress)),
        //     postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.Event_ReceiveActionPress_Postfix)));

        // Our cursor draw patch for interaction highlights.
        harmony.Patch(
            AccessTools.Method(typeof(Game1), nameof(Game1.drawMouseCursor)),
            prefix: new HarmonyMethod(typeof(Game1Patches), nameof(Game1Patches.Game1_drawMouseCursor_Prefix)));

        // We need this to handle items with integrated closeup interactions. Disabled for now.
        // harmony.Patch(
        //     AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.performUseAction)),
        //     prefix: new HarmonyMethod(typeof(Patches), nameof(Patches.SObject_PerformUseAction)));

        helper.Events.Player.Warped += this.PlayerOnWarped;

#if DEBUG
        helper.Events.Display.RenderingWorld += (sender, args) =>
        {
            // args.SpriteBatch.DrawString(Game1.dialogueFont, "AAAAAAAAAAAAAAAAAAA", Vector2.Zero,
            //     Color.Red, 0f, Vector2.Zero, new Vector2(10, 10), SpriteEffects.None, 0f);
        };

        helper.Events.Player.Warped += (sender, args) =>
        {
            // TODO: Finish MEEP background functionality.
            string image = args.NewLocation.getMapProperty("MEEP_Background_Image");
            string tile = args.NewLocation.getMapProperty("MEEP_Background_Tile_Size");
            string variation = args.NewLocation.getMapProperty("MEEP_Background_Tile_Variation");

            int mapWidth = args.NewLocation.map.DisplayWidth;
            int mapHeight = args.NewLocation.map.DisplayHeight;

            // I want all of these for this test.
            if (image is null || tile is null || variation is null)
                return;

            bool imageParsed = Parsers.TryParse(image, out MapBackgroundImage backgroundImage);
            bool sizeParsed = Parsers.TryParse(tile, out MapBackgroundTileSize tileSize);
            bool variationParsed = Parsers.TryParse(variation, out MapBackgroundTileVariation tileVariation);

            if (!imageParsed || !sizeParsed || !variationParsed)
                return;

            Texture2D texture = Game1.content.Load<Texture2D>(backgroundImage.ImagePath);
            int numTiles = (texture.Width / tileSize.Width) * (texture.Height / tileSize.Height);
            Game1.background = new Background(texture, 1, mapWidth / tileSize.Width, mapHeight / tileSize.Height,
                tileSize.Width, tileSize.Height, 4f, 1, numTiles, 1d, Color.White);
        };

        helper.Events.Input.ButtonPressed += (sender, args) =>
        {
            // Event testing!

            if (args.Button == SButton.Home)
            {
                Game1.currentLocation.startEvent(new Event(
                    "continue/-100 -100/farmer 39 14 0 golem 37 15 0 Abigail -100 23 0/MEEP_PlaySound cat/MEEP_PlaySound invalidboobsound/MEEP_PlaySound cat/MEEP_PlaySound dog_bark/addWorldState golemGrave/makeInvisible 37 11 5 7/skippable/pause 1000/viewport 39 11 clamp true/move farmer 0 -3 0/doAction 39 10/pause 500/doAction 39 10/pause 1000/move farmer 1 0 0/playMusic none/pause 100/doAction 39 10/playSound rockGolemSpawn/move Golem 0 -4 0/doAction 39 10/move Golem 2 0 1/pause 500/faceDirection farmer 3/pause 300/playSound rockGolemHit/pause 100/faceDirection farmer 2 true/animate farmer false true 100 94/jump farmer/pause 700/showFrame Golem 5/specificTemporarySprite swordswipe 39 11/playSound swordswipe/pause 250/playSound hitEnemy/startJittering/positionOffset farmer 8 0/pause 20/positionOffset farmer 8 0/pause 20/positionOffset farmer 8 0/pause 20/positionOffset farmer 8 0/pause 20/positionOffset farmer 8 0/pause 20/positionOffset farmer 8 0/pause 20/positionOffset farmer 8 0/pause 20/positionOffset farmer 8 0/playSound clubhit/stopJittering/pause 20/showFrame Golem 4/showFrame farmer 5/pause 1000/move Golem 1 0 1/warp Abigail 49 14/animate Golem false true 150 4 5 6 7/move Abigail -5 0 3/jump Abigail/animate Abigail false true 100 32/speak Abigail \"*gasp*...!$7\"/stopAnimation Abigail/speed Abigail 8/move Abigail -5 0 0/speed Abigail 8/move Abigail 0 -3 1/pause 250/animate Abigail false false 200 50 51/specificTemporarySprite swordswipe 39 11/playSound swordswipe/pause 250/playSound rockGolemHit/stopAnimation Golem/shake Golem 250/faceDirection Golem 3 true/pause 500/animate Abigail false false 200 50 51/specificTemporarySprite swordswipe 39 11/playSound swordswipe/pause 250/playSound rockGolemHit/stopAnimation Golem/shake Golem 250/pause 250/animate Abigail false false 200 50 51/specificTemporarySprite swordswipe 39 11/playSound swordswipe/pause 250/playSound rockGolemDie/specificTemporarySprite golemDie/warp Golem -100 -100/pause 750/playMusic echos/speed Abigail 4/move Abigail 1 0 1/pause 700/animate Abigail false true 100 39/pause 100/emote Abigail 28/pause 400/speak Abigail \"@...?$6\"/pause 1000/speak Abigail \"@... are you okay?$6\"/pause 1500/startJittering/pause 500/stopJittering/pause 1000/startJittering/pause 500/stopJittering/pause 1000/stopAnimation farmer/showFrame farmer 0/jump farmer/pause 250/stopAnimation Abigail/showFrame Abigail 4/pause 500/faceDirection farmer 3/speak Abigail \"It looks like you're just scraped up a little... you had me worried!\"/pause 450/animate Abigail true true 100 33/positionOffset Abigail 8 0/positionOffset farmer -8 0/animate farmer true true 100 101/pause 1500/positionOffset farmer 8 0/positionOffset Abigail -8 0/stopAnimation farmer/stopAnimation Abigail/showFrame Abigail 4/pause 1300/faceDirection Abigail 2/pause 800/move Abigail 0 1 2/faceDirection farmer 2/pause 500/move farmer 0 1 3/emote Abigail 28/pause 1200/MEEP_PlaySound dog_bark/speak Abigail \"I... I've never taken a life before...$s\"/pause 700/faceDirection Abigail 1/pause 550/quickQuestion #It's sad, but there was no other option.#Monsters don't deserve our sympathy!#It's a harsh world, kid.#Did you have to kill him?(break)speak Abigail \"Yeah... I did what I had to do.$s#$b#I guess the world's a pretty tough place. It was either you or him, right?$6\"(break)speak Abigail \"...Aren't they just trying to survive, like us?$s#$b#They may be our enemies, but I still think they deserve sympathy.$6#$b#Still... I did what had to be done.$6\"(break)speak Abigail \"Huh? That's pretty funny coming from someone who just got rescued...$h#$b#But... yeah... Sometimes reality forces us to do things we'd rather not... Guess I'm learning that the hard way.$6\"(break)speak Abigail \"Hey! I saved your life, didn't I? Maybe you should think about that instead of putting me on a guilt trip.$a\"\\pause 600/pause 500/emote Abigail 40/pause 500/speak Abigail \"Look... You've gotta be more careful from now on. I don't want to lose you.\"/pause 500/animate Abigail true true 100 33/positionOffset Abigail 8 0/positionOffset farmer -8 0/animate farmer true true 100 101/specificTemporarySprite heart 40 11/pause 2000/positionOffset farmer 8 0/positionOffset Abigail -8 0/stopAnimation farmer/stopAnimation Abigail/showFrame Abigail 4/pause 1300/faceDirection Abigail 2/faceDirection farmer 2/pause 500/animate Abigail false true 100 0 1 2 3/positionOffset Abigail 0 2/pause 50/positionOffset Abigail 0 2/pause 50/positionOffset Abigail 0 2/pause 50/positionOffset Abigail 0 2/pause 50/positionOffset Abigail 0 2/pause 50/positionOffset Abigail 0 2/pause 50/positionOffset Abigail 0 2/pause 50/positionOffset Abigail 0 2/pause 50/positionOffset Abigail 0 2/pause 50/positionOffset Abigail 0 2/pause 50/positionOffset Abigail 0 2/pause 50/positionOffset Abigail 0 2/pause 50/positionOffset Abigail 0 2/pause 50/stopAnimation Abigail/pause 500/showFrame Abigail 26/pause 100/showFrame Abigail 27/pause 600/speak Abigail \"Let's put him to rest nearby... okay?$6\"/pause 600/fade/viewport -1000 -1000/pause 1000/playSound dirtyHit/pause 600/playSound dirtyHit/pause 600/playSound dirtyHit/pause 600/playSound dirtyHit/pause 2000/end dialogue Abigail \"Good thing I brought my sword today!$6\""
                ));
            }

            // int cursorX = (int)Game1.currentCursorTile.X;
            // int cursorY = (int)Game1.currentCursorTile.Y;
            // GameLocation here = Game1.currentLocation;
            //
            // if (args.IsDown(SButton.OemSemicolon))
            // {
            //     // Item furnace = ObjectFactory.getItemFromDescription(1, 13, 1);
            //     // Game1.currentLocation.Objects.Add(Game1.currentCursorTile, (SObject)furnace);
            //     FakeNpc character = new FakeNpc(
            //         new AnimatedSprite("Characters\\NotAbigail", 0, 16, 32),
            //         Game1.currentCursorTile * 64f,
            //         2,
            //         "NotAbigail",
            //         this.logger
            //     );
            //
            //     string sheet = character.GetDialogueSheetName();
            //
            //     // character.CurrentDialogue.Push(new Dialogue("Hello!", Game1.getCharacterFromName("Abigail")));
            //     // character.CurrentDialogue.Push(new Dialogue("Hello!", Game1.getCharacterFromName("Abigail")));
            //     // character.CurrentDialogue.Push(new Dialogue("How are you doing today?", Game1.getCharacterFromName("Abigail")));
            //     // character.CurrentDialogue.Push(new Dialogue("I'm glad to hear!", Game1.getCharacterFromName("Abigail")));
            //     // character.Dialogue.Add("Wed", "Thing");
            //     // character.Dialogue.Add();
            //
            //     here.characters.Add(character);
            // }
        };
#endif
    }

    private void InitialiseModIntegrations()
    {

    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs args)
    {
        if (this.Helper.ModRegistry.IsLoaded("Omegasis.SaveAnywhere"))
        {
            // Grab the Save Anywhere API so we can safely destroy our NPCs before it saves.
            try
            {
                this.saveAnywhereApi = this.Helper.ModRegistry.GetApi<ISaveAnywhereApi>("Omegasis.SaveAnywhere");
                this.saveAnywhereApi.BeforeSave += this.BeforeSaveAnywhereSave;
                this.saveAnywhereApi.AfterLoad += this.AfterSaveAnywhereLoad;
            }
            catch (Exception e)
            {
                this.logger.Exception(e);
            }
        }

        if (this.Helper.ModRegistry.IsLoaded("spacechase0.SpaceCore") &&
            !this.Helper.ModRegistry.Get("spacechase0.SpaceCore").Manifest.Version.IsOlderThan(new SemanticVersion(1, 13, 0)))
        {
            // Get SpaceCore's API.
            try
            {
                this.spaceCoreApi = this.Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        else
            this.logger.Warn("SpaceCore was installed, but the minimum version for MEEP event commands to work is 1.13.0. Please update SpaceCore to enable custom event commands.");

        // Register our event commands through SpaceCore.
        if (this.spaceCoreApi is not null)
        {
            this.spaceCoreApi.AddEventCommand(PlaySound.Command, AccessTools.Method(this.eventCommands.GetType(), nameof(EventCommands.PlaySound)));
        }
    }

    private void AfterSaveAnywhereLoad(object? sender, EventArgs e)
    {
        // This is hacky and terrible, but this is also a hotfix. I'll live.
        this.logger.Log($"Save Anywhere fired its AfterLoad event. Processing our spawn map.", LogLevel.Info);

        this.ProcessNewLocation(Game1.currentLocation, Game1.currentLocation, Game1.player);
    }

    private void BeforeSaveAnywhereSave(object? sender, EventArgs e)
    {
        this.logger.Log($"Save Anywhere fired its BeforeSave event. Killing NPCs early.", LogLevel.Info);

        this.OnDayEnding(null, null);
    }

    private void PlayerOnWarped(object? sender, WarpedEventArgs args)
    {
        // We need to ensure we can kill relevant UIs if the player is warping.
        if (Game1.activeClickableMenu is MenuBase menu)
        {
            // If it's one of our menus, we close it.
            // This should be refactored use an owned-menu system at some point.
            if (menu.MenuName.Equals(CloseupInteractionImage.PropertyKey))
                Game1.exitActiveMenu();
        }

        // And process our new location.
        this.ProcessNewLocation(args.NewLocation, args.OldLocation, args.Player);
    }

    private void ProcessNewLocation(GameLocation newLocation, GameLocation oldLocation, Farmer player)
    {
        int mapWidth = newLocation.Map.GetLayer("Back").Tiles.Array.GetLength(0);
        int mapHeight = newLocation.Map.GetLayer("Back").Tiles.Array.GetLength(1);

        if (mapWidth == 0 || mapHeight == 0)
            return;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Tile tile;

                try
                {
                    tile = newLocation.Map.GetLayer("Back").Tiles.Array[x, y];
                }
                catch (Exception e)
                {
                    this.logger.Error(
                        $"Couldn't get tile {x}, {y} from map {newLocation.Name}. Exception follows.");
                    this.logger.Exception(e);

                    continue;
                }

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
                            new Vector2(x, y) * 64f,
                            2,
                            fakeNpcProperty.NpcName,
                            this.logger,
                            newLocation
                        );
                        if (fakeNpcProperty.HasSpriteSizes)
                        {
                            if (fakeNpcProperty.SpriteWidth > 16)
                                character.HideShadow = true;

                            character.Breather = false;
                        }

                        Dictionary<string, string> dialogue =
                            this.Helper.GameContent.Load<Dictionary<string, string>>(
                                $"MEEP/FakeNPC/Dialogue/{fakeNpcProperty.NpcName}");

                        foreach (KeyValuePair<string, string> d in dialogue)
                        {
                            character.CurrentDialogue.Push(new Dialogue(d.Value, character));
                        }

                        // A safeguard for multiplayer.
                        if (!newLocation.isTileOccupied(new Vector2(x, y)))
                        {
                            newLocation.characters.Add(character);
                            this.allNpcs.Add(character);
                            this.logger.Log(
                                $"Fake NPC {character.Name} spawned in {newLocation.Name} at X:{x}, Y:{y}.",
                                LogLevel.Trace);
                        }
                    }
                    else
                    {
                        this.logger.Error($"Failed to parse property {property.ToString()}");
                    }
                }
            }
        }
    }

    // This is just to ensure we come before as many other DayEnding events as possible.
    [EventPriority((EventPriority)int.MaxValue)]
    private void OnDayEnding(object? sender, DayEndingEventArgs e)
    {
        foreach (FakeNpc npc in this.allNpcs)
        {
            npc.KillNpc();
        }

        // foreach (GameLocation location in Game1.locations)
        // {
        //     Utils.Locations.RemoveFakeNpcs(location, this.logger);
        // }
        //
        // foreach (Building building in Game1.getFarm().buildings)
        // {
        //     if (building.indoors.Value is GameLocation indoors)
        //         Utils.Locations.RemoveFakeNpcs(indoors, this.logger);
        //
        // }
    }

    public override object? GetApi()
    {
        return this.api;
    }
}
