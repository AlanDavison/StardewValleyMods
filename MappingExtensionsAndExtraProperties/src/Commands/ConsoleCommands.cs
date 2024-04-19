using DecidedlyShared.Logging;
using StardewModdingAPI;

namespace MappingExtensionsAndExtraProperties.Commands;

public class ConsoleCommands
{
    private Logger logger;

    public ConsoleCommands(Logger logger)
    {
        this.logger = logger;
    }

    public void MeepAnimalWipingMode(string command, string[] args)
    {
        ModEntry.AnimalRemovalMode = !ModEntry.AnimalRemovalMode;

        if (ModEntry.AnimalRemovalMode)
        {
            this.logger.Log("MEEP ANIMAL REMOVAL MODE HAS BEEN ENABLED.", LogLevel.Alert);
            this.logger.Log(
                "IF YOU INTERACT WITH A FARM ANIMAL NOW, IT WILL BE PERMANENTLY SENT TO A FARM IN ANOTHER DIMENSION.",
                LogLevel.Alert);
            this.logger.Log("READ: GONE FOREVER.", LogLevel.Alert);
            this.logger.Log("TO DISABLE THIS MODE, RE-RUN THE COMMAND.", LogLevel.Alert);
        }
    }
}
