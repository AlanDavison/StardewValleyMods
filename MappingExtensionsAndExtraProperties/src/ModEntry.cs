using System;
using System.Collections.Generic;
using DecidedlyShared.APIs;
using DecidedlyShared.Logging;
using DecidedlyShared.Utilities;
using HarmonyLib;
using MappingExtensionsAndExtraProperties.Api;
using MappingExtensionsAndExtraProperties.Commands;
using MappingExtensionsAndExtraProperties.Features;
using MappingExtensionsAndExtraProperties.Functionality;
using MappingExtensionsAndExtraProperties.Models.FarmAnimals;
using MappingExtensionsAndExtraProperties.Utils;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace MappingExtensionsAndExtraProperties;

public class ModEntry : Mod
{
    private Logger logger;
    private TilePropertyHandler tileProperties;
    private Properties propertyUtils;
    private MeepApi api;
    private Harmony harmony;

    // Debug/emergency command static mess
    public static bool AnimalRemovalMode = false;

    private IQuickSaveApi quickSaveApi;
    private ISpaceCoreApi spaceCoreApi;
    private EventCommands eventCommands;

    public override void Entry(IModHelper helper)
    {
        this.harmony = new Harmony(this.ModManifest.UniqueID);
        this.logger = new Logger(this.Monitor);
        this.tileProperties = new TilePropertyHandler(this.logger);
        this.propertyUtils = new Properties(this.logger);
        Parsers.InitialiseParsers(this.logger, helper);
        this.eventCommands = new EventCommands(this.Helper, this.logger);
        ConsoleCommands commands = new ConsoleCommands(this.logger);

        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        helper.Events.Display.RenderedStep += this.DisplayOnRendered;
        helper.ConsoleCommands.Add(
            "meep_emergency_remove_animals", "Enter MEEP's animal removal mode. PAY ATTENTION TO THE WARNINGS.",
            commands.MeepAnimalWipingMode);

        // This is where we kill all of our "fake" NPCs so they don't get serialised.
        helper.Events.GameLoop.DayEnding += this.OnDayEndingEarly;

        // Our asset loading.
        helper.Events.Content.AssetRequested += (sender, args) =>
        {
            if (args.NameWithoutLocale.IsDirectlyUnderPath("MEEP/FakeNPC/Dialogue"))
            {
                args.LoadFrom(() => new Dictionary<string, string>(), AssetLoadPriority.Low);
            }

            if (args.NameWithoutLocale.IsEquivalentTo("MEEP/FarmAnimals/SpawnData"))
            {
                args.LoadFrom(() => new Dictionary<string, Animal>(), AssetLoadPriority.Low);
            }
        };

        helper.Events.Player.Warped += this.PlayerOnWarped;
        helper.Events.GameLoop.DayStarted += this.OnDayStarted;
    }

    private void LoadContentPacks()
    {
        bool interactionsUsed = false;
        bool fakeNpcsUsed = false;
        bool vanillaLettersUsed = false;
        bool setMailFlagUsed = false;
        bool farmAnimalSpawningUsed = false;
        bool addConversationTopicUsed = false;
        bool invulnerableTreeUsed = false;


        foreach (var mod in this.Helper.ModRegistry.GetAll())
        {
            if (mod.Manifest.ExtraFields.ContainsKey("DH.MEEP"))
            {
                // This mod uses MEEP, so we want to check for the features.

                if (mod.Manifest.ExtraFields.ContainsKey("DH.MEEP.CloseupInteractions"))
                    interactionsUsed = true;
                if (mod.Manifest.ExtraFields.ContainsKey("DH.MEEP.FakeNPCs"))
                    fakeNpcsUsed = true;
                if (mod.Manifest.ExtraFields.ContainsKey("DH.MEEP.VanillaLetters"))
                    vanillaLettersUsed = true;
                if (mod.Manifest.ExtraFields.ContainsKey("DH.MEEP.SetMailFlag"))
                    setMailFlagUsed = true;
                if (mod.Manifest.ExtraFields.ContainsKey("DH.MEEP.FarmAnimalSpawns"))
                    farmAnimalSpawningUsed = true;
                if (mod.Manifest.ExtraFields.ContainsKey("DH.MEEP.AddConversationTopic"))
                    addConversationTopicUsed = true;
                if (mod.Manifest.ExtraFields.ContainsKey("DH.MEEP.InvulnerableTrees"))
                    invulnerableTreeUsed = true;
            }
        }

        // We've figured out all of our feature usage, so now we create and enable the appropriate features.
        // TODO: Make this more elegant in future. For now, this is 100% functionally fine.

        if (interactionsUsed)
        {
            CloseupInteractionFeature closeupInteractions = new CloseupInteractionFeature(
                this.harmony, "DH.CloseupInteractions", this.logger, this.tileProperties, this.propertyUtils);
            FeatureManager.AddFeature(closeupInteractions);
        }

        if (fakeNpcsUsed)
        {
            FakeNpcFeature fakeNpc = new FakeNpcFeature(this.harmony, "DH.FakeNPC", this.logger, this.tileProperties,
                this.propertyUtils, this.Helper);
            FeatureManager.AddFeature(fakeNpc);
        }

        if (vanillaLettersUsed)
        {
            LetterFeature letter = new LetterFeature(
                this.harmony, "DH.Letter", this.logger, this.tileProperties);
            FeatureManager.AddFeature(letter);
        }

        if (setMailFlagUsed)
        {
            SetMailFlagFeature setMailFlag =
                new SetMailFlagFeature(this.harmony, "DH.SetMailFlag", this.logger, this.tileProperties);
            FeatureManager.AddFeature(setMailFlag);
        }

        if (farmAnimalSpawningUsed)
        {
            FarmAnimalSpawnsFeature farmAnimals =
                new FarmAnimalSpawnsFeature(this.harmony, "DH.FarmAnimalSpawns", this.quickSaveApi, this.logger, this.Helper);
            FeatureManager.AddFeature(farmAnimals);
        }

        if (addConversationTopicUsed)
        {
            AddConversationTopicFeature conversationTopics =
                new AddConversationTopicFeature(this.harmony, "DH.AddConversationTopic", this.logger,
                    this.tileProperties);
            FeatureManager.AddFeature(conversationTopics);
        }

        if (invulnerableTreeUsed)
        {
            InvulnerableTreeFeature invulnerableTrees =
                new InvulnerableTreeFeature(this.harmony, "DH.InvulnerableTrees", this.logger);
            FeatureManager.AddFeature(invulnerableTrees);
        }

        if (FeatureManager.FeatureCount > 0)
        {
            CleanupFeature cleanup = new CleanupFeature("DH.Internal.CleanupFeature");
            FeatureManager.AddFeature(cleanup);
        }

        FeatureManager.EnableFeatures();
        this.RegisterEventCommands();
    }

    private void RegisterEventCommands()
    {
        Event.RegisterCommand("addColouredSlime", EventCommands.AddColouredSlime);
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs args)
    {
        if (this.Helper.ModRegistry.IsLoaded("DLX.QuickSave"))
        {
            // Grab the Save Anywhere API so we can safely destroy our NPCs before it saves.
            try
            {
                this.quickSaveApi = this.Helper.ModRegistry.GetApi<IQuickSaveApi>("DLX.QuickSave");
                this.quickSaveApi.SavingEvent += this.BeforeQuickSaveSave;
                this.quickSaveApi.LoadedEvent += this.AfterQuickSaveLoad;
            }
            catch (Exception e)
            {
                this.logger.Log($"Quick Save was loaded, but we couldn't get its API for some reason. Exception follows:");
                this.logger.Exception(e);
            }
        }

        this.LoadContentPacks();
    }

    private void AfterQuickSaveLoad(object? sender, ILoadedEventArgs e)
    {
        this.logger.Log($"Quick Save fired its LoadedEvent. Processing our spawn map.", LogLevel.Info);

        FeatureManager.OnLocationChange(Game1.currentLocation, Game1.currentLocation, Game1.player);
        FeatureManager.OnDayStart();
    }

    private void BeforeQuickSaveSave(object? sender, ISavingEventArgs e)
    {
        this.logger.Log($"Quick Save fired its SavingEvent. Treating this as an early day end.", LogLevel.Info);

        FeatureManager.EarlyOnDayEnding();
    }

    private void PlayerOnWarped(object? sender, WarpedEventArgs args)
    {
        FeatureManager.OnLocationChange(args.OldLocation, args.NewLocation, args.Player);
    }

    // This is just to ensure we come before as many other DayEnding events as possible.
    [EventPriority((EventPriority)int.MaxValue)]
    private void OnDayEndingEarly(object? sender, DayEndingEventArgs e)
    {
        FeatureManager.EarlyOnDayEnding();
    }

    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        FeatureManager.OnDayStart();
    }

    private void DisplayOnRendered(object? sender, RenderedStepEventArgs e)
    {
        FeatureManager.OnRenderedStep(e);
    }

    public override object? GetApi()
    {
        return this.api;
    }
}
