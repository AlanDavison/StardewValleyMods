using System.Collections.Generic;
using DecidedlyShared.Logging;
using PlantingByBirb.Framework;
using PlantingByBirb.Framework.Model;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace PlantingByBirb;

public class ModEntry : Mod
{
    private Planter planter;

    public override void Entry(IModHelper helper)
    {
        this.planter = new Planter(new Logger(this.Monitor), this.Helper);

        helper.Events.Content.AssetRequested += this.ContentOnAssetRequested;
        helper.Events.GameLoop.DayEnding += this.planter.ProcessDayEnding;
    }

    private void ContentOnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo("DecidedlyHuman/PlantingByBirb/SeedData"))
        {
            e.LoadFrom(() => new Dictionary<string, BirbSeed>(), AssetLoadPriority.Low);
        }
    }
}
