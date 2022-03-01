using System;
using StardewModdingAPI;

namespace SmartBuilding.Utilities
{
	public class Logger
	{
		private IMonitor _monitor;
		private string _logPrefix = ":: ";

		public Logger(IMonitor m)
		{
			_monitor = m;
		}

		private void Log(string logMessage, string logPrefix, LogLevel logLevel)
		{
			_monitor.Log(logPrefix + logMessage, logLevel);
		}

		public void Log(string logMessage, LogLevel logLevel = LogLevel.Info)
		{
			this.Log(logMessage, _logPrefix, logLevel);
		}

		public void Trace(string logMessage)
		{
			this.Log(logMessage, _logPrefix, LogLevel.Warn);
		}

		public void Warn(string logMessage)
		{
			this.Log(logMessage, _logPrefix, LogLevel.Warn);
		}

		public void Error(string logMessage)
		{
			this.Log(logMessage, _logPrefix, LogLevel.Error);
		}

		public void Exception(Exception e)
		{
			_monitor.Log($"{_logPrefix} Exception: {e.Message}", LogLevel.Error);
			_monitor.Log($"{_logPrefix} Full exception data: \n{e.Data}", LogLevel.Error);
		}
	}
}