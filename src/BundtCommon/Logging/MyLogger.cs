using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace BundtBot
{
	public class MyLogger
	{
		public const ConsoleColor DefaultColor = ConsoleColor.Gray;

		public bool EnableTimestamps = true;
		public LogLevel CurrentLogLevel = LogLevel.Information;

		const string defaultFgColorEscSeq = "\x1b[39m";
		const string defaultBgColorEscSeq = "\x1b[49m";

		readonly string _prefix;
		readonly bool supportsAnsiColors;

		public MyLogger(string prefix)
		{
			_prefix = prefix;
			supportsAnsiColors = Console.LargestWindowHeight <= 0;
			LogInfo(nameof(supportsAnsiColors) + ": " + supportsAnsiColors);
		}

		public void PrintTestLogColors()
		{
			Log("Test", "Black Fg", ConsoleColor.Black);
			Log("Test", "DarkBlue Fg", ConsoleColor.DarkBlue);
			Log("Test", "DarkGreen Fg", ConsoleColor.DarkGreen);
			Log("Test", "DarkCyan Fg", ConsoleColor.DarkCyan);
			Log("Test", "DarkRed Fg", ConsoleColor.DarkRed);
			Log("Test", "DarkMagenta Fg", ConsoleColor.DarkMagenta);
			Log("Test", "DarkYellow Fg", ConsoleColor.DarkYellow);
			Log("Test", "Gray Fg", ConsoleColor.Gray);
			Log("Test", "DarkGray Fg", ConsoleColor.DarkGray);
			Log("Test", "Blue Fg", ConsoleColor.Blue);
			Log("Test", "Green Fg", ConsoleColor.Green);
			Log("Test", "Cyan Fg", ConsoleColor.Cyan);
			Log("Test", "Red Fg", ConsoleColor.Red);
			Log("Test", "Magenta Fg", ConsoleColor.Magenta);
			Log("Test", "Yellow Fg", ConsoleColor.Yellow);
			Log("Test", "White Fg", ConsoleColor.White);
		}

		/// <summary>
		/// Logs that contain the most detailed messages.
		/// These messages may contain sensitive application data.
		/// These messages are disabled by default and should never be enabled in a production environment.
		/// </summary>
		public void LogTrace(object message, ConsoleColor? color = null)
		{
			if (LogLevel.Trace < CurrentLogLevel) return;
			Log("Trace", message, ConsoleColor.DarkBlue, color);
		}

		/// <summary>
		/// Logs that are used for interactive investigation during development.
		/// These logs should primarily contain information useful for debugging and have no long-term value
		/// </summary>
		public void LogDebug(object message, ConsoleColor? color = null)
		{
			if (LogLevel.Debug < CurrentLogLevel) return;
			Log("Debug", message, ConsoleColor.DarkCyan, color);
		}

		/// <summary>
		/// Logs that track the general flow of the application.
		/// These logs should have long-term value.
		/// </summary>
		public void LogInfo(object message, ConsoleColor? color = null)
		{
			if (LogLevel.Information < CurrentLogLevel) return;
			Log("Info", message, ConsoleColor.Blue, color);
		}

		public void LogInfo(params LogMessage[] messages)
		{
			if (LogLevel.Information < CurrentLogLevel) return;

			var message = "";

			if (supportsAnsiColors)
			{
				foreach (var msg in messages)
				{
					var code = msg.Color == null
						? defaultFgColorEscSeq
						: GetAnsiColorForegroundEscapeSequence(msg.Color.GetValueOrDefault());
					message += code + msg.Message;
				}
			}
			else
			{
				messages.ToList().ForEach(x => message += x.Message);
			}

			Log("Info", message, ConsoleColor.Blue, null);
		}

		/// <summary>
		/// Logs that highlight an abnormal or unexpected event in the application flow,
		/// but do not otherwise cause the application execution to stop.
		/// </summary>
		public void LogWarning(object message)
		{
			if (LogLevel.Warning < CurrentLogLevel) return;
			Log("Warning", message, ConsoleColor.Yellow, ConsoleColor.Yellow);
		}

		/// <summary>
		/// Logs that highlight when the current flow of execution is stopped due to a failure.
		/// These should indicate a failure in the current activity, not an application-wide failure.
		/// </summary>
		public void LogError(Exception ex)
		{
			if (LogLevel.Error < CurrentLogLevel) return;
			Log("**ERROR**", ex.GetType(), ConsoleColor.Red, ConsoleColor.Red);
			Log("**ERROR**", ex.Message, ConsoleColor.Red);
			Log("**ERROR**", ex.StackTrace ?? "No stack trace available", ConsoleColor.Red);
			if (ex.InnerException != null)
			{
				Log("**ERROR**", $"InnerException1: ${ex.InnerException}", ConsoleColor.Red);
				if (ex.InnerException.InnerException != null)
				{
					Log("**ERROR**", $"InnerException2: ${ex.InnerException.InnerException}", ConsoleColor.Red);
				}
			}
		}

		/// <summary>
		/// Logs that highlight when the current flow of execution is stopped due to a failure.
		/// These should indicate a failure in the current activity, not an application-wide failure.
		/// </summary>
		public void LogError(string message)
		{
			if (LogLevel.Error < CurrentLogLevel) return;
			Log("**ERROR**", message, ConsoleColor.Red, ConsoleColor.Red);
		}

		/// <summary>
		/// Logs that describe an unrecoverable application or system crash,
		/// or a catastrophic failure that requires immediate attention.
		/// </summary>
		public void LogCritical(Exception ex)
		{
			if (LogLevel.Critical < CurrentLogLevel) return;
			Log("***CRITICAL*** ", ex.GetType(), ConsoleColor.Red, ConsoleColor.Red);
			Log("***CRITICAL*** ", ex.Message, ConsoleColor.Red, ConsoleColor.Red);
			Log("***CRITICAL*** ", ex.StackTrace ?? "No stack trace available", ConsoleColor.Red, ConsoleColor.Red);
			if (ex.InnerException != null)
			{
				Log("***CRITICAL*** ", $"InnerException: ${ex.InnerException}", ConsoleColor.Red, ConsoleColor.Red);
			}
		}

		/// <summary>
		/// Logs that describe an unrecoverable application or system crash,
		/// or a catastrophic failure that requires immediate attention.
		/// </summary>
		public void LogCritical(string message)
		{
			if (LogLevel.Critical < CurrentLogLevel) return;
			Log("***CRITICAL*** ", message, ConsoleColor.Red);
		}

		void Log(string logLevel, object messageObject, ConsoleColor? logLevelColor = null, ConsoleColor? messageColor = null)
		{
			var message = messageObject.ToString();

			if (supportsAnsiColors)
			{
				var logLevelColorCode = logLevelColor == null
					? defaultFgColorEscSeq
					: GetAnsiColorForegroundEscapeSequence(logLevelColor.GetValueOrDefault());
				var msgColorCode = "";
				if (messageColor == null)
				{
					msgColorCode = defaultFgColorEscSeq;
				}
				else
				{
					msgColorCode = GetAnsiColorForegroundEscapeSequence(messageColor.GetValueOrDefault());
				}
				message = $"{logLevelColorCode}[{logLevel}]{defaultFgColorEscSeq} {_prefix}: {msgColorCode}{message}{defaultFgColorEscSeq}";
			}
			else
			{
				message = $"[{logLevel}] {_prefix}: {message}";
			}

			if (EnableTimestamps) {
				message = $"{DateTime.Now.ToString("o")} {message}";
			}

			message = message.Replace("\n", "\n\t");

			if (supportsAnsiColors)
			{
				WriteStdoutUnix(message);
			}
			else
			{
				WriteStdoutWindows(message, messageColor ?? DefaultColor);
			}

		}

		void WriteStdoutWindows(string message, ConsoleColor color)
		{
			Console.ForegroundColor = color;
			Console.WriteLine(message);
			Console.ForegroundColor = DefaultColor;
		}

		void WriteStdoutUnix(string message)
		{
			Console.WriteLine(message + defaultFgColorEscSeq + defaultBgColorEscSeq);
		}

		string GetAnsiColorForegroundEscapeSequence(ConsoleColor color)
		{
			var code = "";
			switch (color)
			{
				case ConsoleColor.Black: code = "30"; break;
				case ConsoleColor.DarkBlue: code = "34"; break;
				case ConsoleColor.DarkGreen: code = "32"; break;
				case ConsoleColor.DarkCyan: code = "36"; break;
				case ConsoleColor.DarkRed: code = "31"; break;
				case ConsoleColor.DarkMagenta: code = "35"; break;
				case ConsoleColor.DarkYellow: code = "33"; break;
				case ConsoleColor.Gray: code = "37"; break;
				case ConsoleColor.DarkGray: code = "90"; break;
				case ConsoleColor.Blue: code = "94"; break;
				case ConsoleColor.Green: code = "92"; break;
				case ConsoleColor.Cyan: code = "96"; break;
				case ConsoleColor.Red: code = "91"; break;
				case ConsoleColor.Magenta: code = "95"; break;
				case ConsoleColor.Yellow: code = "93"; break;
				case ConsoleColor.White: code = "97"; break;
				default: code = "39"; break;
			}
			return "\x1b[" + code + "m";
		}

		string GetAnsiColorBackgroundCode(ConsoleColor color)
		{
			var code = "";
			switch (color)
			{
				case ConsoleColor.Black: code = "40"; break;
				case ConsoleColor.DarkBlue: code = "44"; break;
				case ConsoleColor.DarkGreen: code = "42"; break;
				case ConsoleColor.DarkCyan: code = "46"; break;
				case ConsoleColor.DarkRed: code = "41"; break;
				case ConsoleColor.DarkMagenta: code = "45"; break;
				case ConsoleColor.DarkYellow: code = "43"; break;
				case ConsoleColor.Gray: code = "47"; break;
				case ConsoleColor.DarkGray: code = "100"; break;
				case ConsoleColor.Blue: code = "104"; break;
				case ConsoleColor.Green: code = "102"; break;
				case ConsoleColor.Cyan: code = "106"; break;
				case ConsoleColor.Red: code = "101"; break;
				case ConsoleColor.Magenta: code = "105"; break;
				case ConsoleColor.Yellow: code = "103"; break;
				case ConsoleColor.White: code = "107"; break;
				default: code = "49"; break;
			}
			return "\x1b[" + code + "m";
		}
	}
}
