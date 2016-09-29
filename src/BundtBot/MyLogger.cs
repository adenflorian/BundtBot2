using System;

namespace BundtBot {
	public class MyLogger {
		public static bool EnableTimestamps = true;
		public const ConsoleColor DefaultColor = ConsoleColor.Gray;
		public static LogLevel MaxLogLevel = LogLevel.Info;

		public static void LogDebug(object message, ConsoleColor color = DefaultColor) {
			if (LogLevel.Debug > MaxLogLevel) return;
			Log(message, color);
		}

		public static void LogInfo(object message, ConsoleColor color = DefaultColor) {
			if (LogLevel.Info > MaxLogLevel) return;
			Log(message, color);
		}

		public static void LogWarning(object message) {
			if (LogLevel.Warning > MaxLogLevel) return;
			Log(message, ConsoleColor.Yellow);
		}

		public static void LogError(Exception exception) {
			if (LogLevel.Error > MaxLogLevel) return;
			Log(exception, ConsoleColor.Red);
			Log(exception.StackTrace ?? "No stack trace available", ConsoleColor.Red);
		}

		static void Log(object message, ConsoleColor color) {
			if (EnableTimestamps) {
				message = DateTime.Now + " | " + message;
			}

			message = message.ToString().Replace("\n", "\n\t");

			Console.ForegroundColor = color;
			Console.WriteLine(message);
			Console.ForegroundColor = DefaultColor;
		}
	}

	public enum LogLevel {
		Nothing = 0,
		Error = 25,
		Warning = 50,
		Info = 75,
		Debug = 100
	}
}
