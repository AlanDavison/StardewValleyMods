using StardewModdingAPI;
using StardewRituals.Utilities;

namespace StardewRituals
{
    public class ModEntry : Mod
    {
        private IModHelper _helper;
        private Logger _logger;
        private IMonitor _monitor;

        public override void Entry(IModHelper helper)
        {
            this._monitor = this.Monitor;
            this._helper = helper;
            this._logger = new Logger(this._monitor);

            // WHY THE FUCK DOES DGA SAY THERE ARE DUPLICATE ITEMS?
        }
    }
}
