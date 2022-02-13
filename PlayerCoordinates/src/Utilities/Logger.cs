using System;
using StardewModdingAPI;

namespace PlayerCoordinates.Utilities
{
	public static class Logger
	{
		public static void LogMessage(IMonitor m, LogLevel level, string message = "No message specified. Bad developer.")
		{
			m.Log(message, level);
		}

		public static void LogException(IMonitor m, Exception e)
		{
			m.Log($"Exception: {e.Message}.", LogLevel.Error);
			m.Log($"{e.Data}.", LogLevel.Error);
		}
	}
}