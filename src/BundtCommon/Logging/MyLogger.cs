using System;
using Microsoft.Extensions.Logging;

namespace BundtBot
{
	public class MyLogger
	{
		public const ConsoleColor DefaultColor = ConsoleColor.Gray;

		public bool EnableTimestamps = true;
		public LogLevel CurrentLogLevel = LogLevel.Information;

		readonly string _prefix;

		public MyLogger(string prefix)
		{
			_prefix = prefix;
		}

		/// <summary>
		/// Logs that contain the most detailed messages.
		/// These messages may contain sensitive application data.
		/// These messages are disabled by default and should never be enabled in a production environment.
		/// </summary>
		public void LogTrace(object message, ConsoleColor color = DefaultColor)
		{
			if (LogLevel.Trace < CurrentLogLevel) return;
			Log("Trace: " + message, color);
		}

		/// <summary>
		/// Logs that are used for interactive investigation during development.
		/// These logs should primarily contain information useful for debugging and have no long-term value
		/// </summary>
		public void LogDebug(object message, ConsoleColor color = DefaultColor)
		{
			if (LogLevel.Debug < CurrentLogLevel) return;
			Log("Debug: " + message, color);
		}

		/// <summary>
		/// Logs that track the general flow of the application.
		/// These logs should have long-term value.
		/// </summary>
		public void LogInfo(object message, ConsoleColor color = DefaultColor)
		{
			if (LogLevel.Information < CurrentLogLevel) return;
			Log("Info: " + message, color);
		}

		/// <summary>
		/// Logs that highlight an abnormal or unexpected event in the application flow,
		/// but do not otherwise cause the application execution to stop.
		/// </summary>
		public void LogWarning(object message)
		{
			if (LogLevel.Warning < CurrentLogLevel) return;
			Log("Warning: " + message, ConsoleColor.Yellow);
		}

		/// <summary>
		/// Logs that highlight when the current flow of execution is stopped due to a failure.
		/// These should indicate a failure in the current activity, not an application-wide failure.
		/// </summary>
		public void LogError(Exception ex)
		{
			if (LogLevel.Error < CurrentLogLevel) return;
			Log("**ERROR** " + ex.GetType(), ConsoleColor.Red);
			Log(ex.Message, ConsoleColor.Red);
			Log(ex.StackTrace ?? "No stack trace available", ConsoleColor.Red);
			if (ex.InnerException != null)
			{
				Log($"InnerException: ${ex.InnerException}", ConsoleColor.Red);
			}
		}

		/// <summary>
		/// Logs that highlight when the current flow of execution is stopped due to a failure.
		/// These should indicate a failure in the current activity, not an application-wide failure.
		/// </summary>
		public void LogError(string message)
		{
			if (LogLevel.Error < CurrentLogLevel) return;
			Log("**ERROR** " + message, ConsoleColor.Red);
		}

		/// <summary>
		/// Logs that describe an unrecoverable application or system crash,
		/// or a catastrophic failure that requires immediate attention.
		/// </summary>
		public void LogCritical(Exception ex)
		{
			if (LogLevel.Critical < CurrentLogLevel) return;
			Log("************** ", ConsoleColor.Red);
			Log("***CRITICAL*** " + ex.GetType(), ConsoleColor.Red);
			Log("************** ", ConsoleColor.Red);
			Log(ex.Message, ConsoleColor.Red);
			Log(ex.StackTrace ?? "No stack trace available", ConsoleColor.Red);
			if (ex.InnerException != null)
			{
				Log($"InnerException: ${ex.InnerException}", ConsoleColor.Red);
			}
		}

		/// <summary>
		/// Logs that describe an unrecoverable application or system crash,
		/// or a catastrophic failure that requires immediate attention.
		/// </summary>
		public void LogCritical(string message)
		{
			if (LogLevel.Critical < CurrentLogLevel) return;
			Log("************** ", ConsoleColor.Red);
			Log("***CRITICAL*** " + message, ConsoleColor.Red);
			Log("************** ", ConsoleColor.Red);
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

			// TODO format datetime with timezone
			if (EnableTimestamps) {
				message = $"{DateTime.Now} | {message}";
			}

			message = message.Replace("\n", "\n\t");

			return message;
		}
	}
}
