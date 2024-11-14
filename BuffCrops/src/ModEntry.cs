using BuffCrops.Framework;
using DecidedlyShared.Logging;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace BuffCrops;

public class ModEntry : Mod
{
    private BuffCropsEvents buffCrops;
    private Logger logger;

    public override void Entry(IModHelper helper)
    {
        helper.Events.GameLoop.DayStarted += this.GameLoopOnDayStarted;
        I18n.Init(this.Helper.Translation);
        this.logger = new Logger(this.Monitor);
        this.buffCrops = new BuffCropsEvents(this.logger);
    }

    private void GameLoopOnDayStarted(object? sender, DayStartedEventArgs e)
    {
        this.buffCrops.DoDayStart();
    }
}
