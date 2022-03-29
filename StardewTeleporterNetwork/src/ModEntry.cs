using StardewModdingAPI;
using StardewTeleporterNetwork.Utilities;

namespace StardewTeleporterNetwork
{
	public class ModEntry : Mod
	{
		private IModHelper _helper;
		private IMonitor _monitor;
		private Logger _logger;

		public override void Entry(IModHelper helper)
		{
			_helper = helper;
			_monitor = Monitor;
			_logger = new Logger(_monitor);
		}
	}
}