using System;
using System.Net.Http.Headers;
using DecidedlyShared.Logging;
using DecidedlyShared.APIs;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Object = StardewValley.Object;

namespace BetterCrystalariums
{
    public class BetterCrystalariumEntry : Mod
    {
        private ModConfig config;
        private IModHelper helper;
        private Logger logger;
        private IMonitor monitor;

        public override void Entry(IModHelper helper)
        {
            this.helper = helper;
            this.monitor = this.Monitor;
            this.logger = new Logger(this.monitor);
            this.config = this.helper.ReadConfig<ModConfig>();
            Patches.Initialise(this.monitor, this.helper, this.logger, this.config);

            Harmony harmony = new(this.ModManifest.UniqueID);

            harmony.Patch(
                AccessTools.Method(typeof(Object),
                    nameof(Object.performObjectDropInAction),
                    new[] { typeof(Item), typeof(bool), typeof(Farmer) }),
                new HarmonyMethod(typeof(Patches),
                    nameof(Patches.ObjectDropIn_Prefix))
            );

            this.helper.Events.GameLoop.GameLaunched += this.GameLaunched;
        }

        private void GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            try
            {
                this.RegisterWithGmcm();
            }
            catch (Exception ex)
            {
                this.logger.Log(this.helper.Translation.Get("bettercrystalariums.no-gmcm"));
            }
        }

        private void RegisterWithGmcm()
        {
            var configMenuApi =
                this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

            configMenuApi.Register(
                mod: this.ModManifest,
                reset: () => this.config = new ModConfig(),
                save: () => this.Helper.WriteConfig(this.config));

            configMenuApi.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.helper.Translation.Get("bettercrystalariums.debug-setting-title"),
                tooltip: () => this.helper.Translation.Get("bettercrystalariums.debug-setting-description"),
                getValue: () => this.config.DebugMode,
                setValue: value => this.config.DebugMode = value);
        }
    }
}
