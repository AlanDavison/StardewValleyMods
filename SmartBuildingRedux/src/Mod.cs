using DecidedlyShared.Logging;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace SmartBuildingRedux;

public class Mod : StardewModdingAPI.Mod
{
    private Logger logger;

    public override void Entry(IModHelper helper)
    {
        this.logger = new Logger(this.Monitor, helper.Translation);

        helper.Events.Input.ButtonPressed += this.InputOnButtonPressed;
    }

    private void InputOnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {

    }
}
