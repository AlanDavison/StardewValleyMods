using System;
using System.Collections.Generic;
using DecidedlyShared.APIs;
using DecidedlyShared.Logging;
using DecidedlyShared.Utilities;
using HarmonyLib;
using MappingExtensionsAndExtraProperties.Api;
using MappingExtensionsAndExtraProperties.Features;
using MappingExtensionsAndExtraProperties.Functionality;
using MappingExtensionsAndExtraProperties.Models.EventCommands;
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

    private ISaveAnywhereApi saveAnywhereApi;
    private ISpaceCoreApi spaceCoreApi;
    private EventCommands eventCommands;

    public override void Entry(IModHelper helper)
    {
        var harmony = new Harmony(this.ModManifest.UniqueID);
        this.logger = new Logger(this.Monitor);
        this.tileProperties = new TilePropertyHandler(this.logger);
        this.propertyUtils = new Properties(this.logger);
        Parsers.InitialiseParsers(this.logger, helper);
        this.eventCommands = new EventCommands(this.Helper, this.logger);

        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

        this.LoadContentPacks();

        // This is where we kill all of our "fake" NPCs so they don't get serialised.
        helper.Events.GameLoop.DayEnding += this.OnDayEndingEarly;

        // Our asset loading.
        helper.Events.Content.AssetRequested += (sender, args) =>
        {
            if (args.NameWithoutLocale.IsDirectlyUnderPath("MEEP/FakeNPC/Dialogue"))
            {
                args.LoadFrom(() => { return new Dictionary<string, string>(); }, AssetLoadPriority.Low);
            }
        };

        CloseupInteractionFeature closeupInteractions = new CloseupInteractionFeature(
            harmony, "DH.CloseupInteractions", this.logger, this.tileProperties, this.propertyUtils);
        LetterFeature letter = new LetterFeature(
            harmony, "DH.Letter", this.logger, this.tileProperties);
        CursorIconFeature cursorIcons = new CursorIconFeature(
            harmony, "DH.Internal.CursorIconFeature", this.logger, this.tileProperties);
        FakeNpcFeature fakeNpc = new FakeNpcFeature(harmony, "DH.FakeNPC", this.logger, this.tileProperties,
            this.propertyUtils, this.Helper);
        CleanupFeature cleanup = new CleanupFeature("DH.Internal.CleanupFeature");
        FeatureManager.AddFeature(closeupInteractions);
        FeatureManager.AddFeature(letter);
        FeatureManager.AddFeature(cursorIcons);
        FeatureManager.AddFeature(fakeNpc);
        FeatureManager.AddFeature(cleanup);
        FeatureManager.EnableFeatures();

        helper.Events.Player.Warped += this.PlayerOnWarped;
    }

    private void LoadContentPacks()
    {
        foreach (var mod in this.Helper.ModRegistry.GetAll())
        {
            if (mod.Manifest.ExtraFields.ContainsKey("DH.MEEP"))
            {
                // Current thought: Have each feature (e.g., closeup interactions, mail flag alterations, backgrounds, etc.)
                // be a "Feature" in code that only becomes active if a pack that uses said feature is loaded.
                // This could be done via a FeatureManager that could have features added to its active list. When added
                // to the list, the Feature which inherits IFeature runs its patching logic where necessary, and exposes
                // the methods it uses to perform the functionality where required.
            }
        }
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
            !this.Helper.ModRegistry.Get("spacechase0.SpaceCore").Manifest.Version
                .IsOlderThan(new SemanticVersion(1, 13, 0)))
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
            this.logger.Warn(
                "SpaceCore was installed, but the minimum version for MEEP event commands to work is 1.13.0. Please update SpaceCore to enable custom event commands.");

        // Register our event commands through SpaceCore.
        if (this.spaceCoreApi is not null)
        {
            this.spaceCoreApi.AddEventCommand(PlaySound.Command,
                AccessTools.Method(this.eventCommands.GetType(), nameof(EventCommands.PlaySound)));
        }
    }

    private void AfterSaveAnywhereLoad(object? sender, EventArgs e)
    {
        // This is hacky and terrible, but this is also a hotfix. I'll live.
        this.logger.Log($"Save Anywhere fired its AfterLoad event. Processing our spawn map.", LogLevel.Info);

        FeatureManager.OnLocationChange(Game1.currentLocation, Game1.currentLocation, Game1.player);
    }

    private void BeforeSaveAnywhereSave(object? sender, EventArgs e)
    {
        this.logger.Log($"Save Anywhere fired its BeforeSave event. Killing NPCs early.", LogLevel.Info);

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

    [EventPriority((EventPriority)int.MinValue)]
    private void OnDayEndingLate(object? sender, DayEndingEventArgs e)
    {
        FeatureManager.LateOnDayEnding();
    }

    public override object? GetApi()
    {
        return this.api;
    }
}
