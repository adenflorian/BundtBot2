using System;

namespace BundtBot
{
	class MyLogger
	{
		public const ConsoleColor DefaultColor = ConsoleColor.Gray;

		public bool EnableTimestamps = true;
		public LogLevel MaxLogLevel = LogLevel.Info;

		readonly string _prefix;

		public MyLogger(string prefix)
		{
			_prefix = prefix;
		}

		public void LogDebug(object message, ConsoleColor color = DefaultColor)
		{
			if (LogLevel.Debug > MaxLogLevel) return;
			Log(message, color);
		}

		public void LogInfo(object message, ConsoleColor color = DefaultColor)
		{
			if (LogLevel.Info > MaxLogLevel) return;
			Log(message, color);
		}

		public void LogWarning(object message)
		{
			if (LogLevel.Warning > MaxLogLevel) return;
			Log(message, ConsoleColor.Yellow);
		}

		public void LogError(Exception exception)
		{
			if (LogLevel.Error > MaxLogLevel) return;
			Log(exception, ConsoleColor.Red);
			Log(exception.StackTrace ?? "No stack trace available", ConsoleColor.Red);
		}

		void Log(object messageObject, ConsoleColor color)
		{
			var message = PrepareMessage(messageObject);

			Console.ForegroundColor = color;
			Console.WriteLine(message);
			Console.ForegroundColor = DefaultColor;
		}

		string PrepareMessage(object messageObject)
		{
			var message = messageObject.ToString();

			message = $"{_prefix}: {message}";

			if (EnableTimestamps) {
				message = $"{DateTime.Now} | {message}";
			}

			message = message.Replace("\n", "\n\t");

			return message;
		}
	}

	public enum LogLevel
	{
		Nothing = 0,
		Error = 25,
		Warning = 50,
		Info = 75,
		Debug = 100
	}
}
