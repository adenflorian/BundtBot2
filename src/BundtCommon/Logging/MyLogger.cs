using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace BundtBot
{
    // Coloring Guidelines
    // ===================
    // 
    // # Prefixes
    // Gateway stuff - Cyan/DarkCyan
    // Rest Api stuff - Magenta/DarkMagenta
    // Web Server stuff - Blue/DarkBlue
    public class MyLogger
    {
        public const ConsoleColor DefaultColor = ConsoleColor.Gray;

        public bool EnableTimestamps = true;
        public LogLevel CurrentLogLevel = LogLevel.Debug;

        readonly string _prefix;
        readonly ConsoleColor? _prefixColor;
        readonly bool _supportsAnsiColors;

        public MyLogger(string prefix, ConsoleColor? prefixColor = null)
        {
            _prefix = prefix;
            _prefixColor = prefixColor;
            _supportsAnsiColors = Console.LargestWindowHeight <= 0;
            LogDebug(nameof(_supportsAnsiColors) + ": " + _supportsAnsiColors);
        }

        /// <summary>
        /// Logs that contain the most detailed messages.
        /// These messages may contain sensitive application data.
        /// These messages are disabled by default and should never be enabled in a production environment.
        /// </summary>
        public void LogTrace(object message, ConsoleColor? color = null)
        {
            if (LogLevel.Trace < CurrentLogLevel) return;
            BuildAndLog("Trace", message, ConsoleColor.DarkMagenta, color);
        }

        /// <summary>
        /// Logs that are used for interactive investigation during development.
        /// These logs should primarily contain information useful for debugging and have no long-term value
        /// </summary>
        public void LogDebug(object message, ConsoleColor? color = null)
        {
            if (LogLevel.Debug < CurrentLogLevel) return;
            BuildAndLog("Debug", message, ConsoleColor.DarkCyan, color);
        }

        /// <summary>
        /// Logs that track the general flow of the application.
        /// These logs should have long-term value.
        /// </summary>
        public void LogInfo(object message, ConsoleColor? color = null)
        {
            if (LogLevel.Information < CurrentLogLevel) return;
            BuildAndLog("Info", message, ConsoleColor.Blue, color);
        }

        public void LogInfo(params LogMessage[] messages)
        {
            if (LogLevel.Information < CurrentLogLevel) return;

            var message = "";

            if (_supportsAnsiColors)
            {
                foreach (var msg in messages)
                {
                    var code = msg.Color == null
                        ? AnsiColorFg.Default
                        : GetAnsiColorFgEscSeq(msg.Color.GetValueOrDefault());
                    message += code + msg.Message;
                }
            }
            else
            {
                messages.ToList().ForEach(x => message += x.Message);
            }

            BuildAndLog("Info", message, ConsoleColor.Blue, null);
        }

        /// <summary>
        /// Logs that highlight an abnormal or unexpected event in the application flow,
        /// but do not otherwise cause the application execution to stop.
        /// </summary>
        public void LogWarning(object message)
        {
            if (LogLevel.Warning < CurrentLogLevel) return;
            BuildAndLog("Warning", message, ConsoleColor.Yellow, ConsoleColor.Yellow);
        }

        /// <summary>
        /// Logs that highlight when the current flow of execution is stopped due to a failure.
        /// These should indicate a failure in the current activity, not an application-wide failure.
        /// </summary>
        public void LogError(Exception ex)
        {
            if (LogLevel.Error < CurrentLogLevel) return;
            BuildAndLog("**ERROR**", $"{ex.GetType()}: {ex.Message}", ConsoleColor.Red, ConsoleColor.Red);
            BuildAndLog("**ERROR**", ex.StackTrace ?? "No stack trace available", ConsoleColor.Red);
            if (ex.InnerException != null)
            {
                BuildAndLog("**ERROR**", $"InnerException1: ${ex.InnerException}", ConsoleColor.Red);
                if (ex.InnerException.InnerException != null)
                {
                    BuildAndLog("**ERROR**", $"InnerException2: ${ex.InnerException.InnerException}", ConsoleColor.Red);
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
            BuildAndLog("**ERROR**", message, ConsoleColor.Red, ConsoleColor.Red);
        }

        /// <summary>
        /// Logs that describe an unrecoverable application or system crash,
        /// or a catastrophic failure that requires immediate attention.
        /// </summary>
        public void LogCritical(Exception ex)
        {
            if (LogLevel.Critical < CurrentLogLevel) return;
            BuildAndLog("❗❗❗CRITICAL❗❗❗ ", $"{ex.GetType()}: {ex.Message}", ConsoleColor.Red, ConsoleColor.Red);
            BuildAndLog("❗❗❗CRITICAL❗❗❗ ", ex.StackTrace ?? "No stack trace available", ConsoleColor.Red, ConsoleColor.Red);
            if (ex.InnerException != null)
            {
                BuildAndLog("❗❗❗CRITICAL❗❗❗ ", $"InnerException: ${ex.InnerException}", ConsoleColor.Red, ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Logs that describe an unrecoverable application or system crash,
        /// or a catastrophic failure that requires immediate attention.
        /// </summary>
        public void LogCritical(string message)
        {
            if (LogLevel.Critical < CurrentLogLevel) return;
            BuildAndLog("❗❗❗CRITICAL❗❗❗ ", message, ConsoleColor.Red);
        }

        void BuildAndLog(string logLevel, object messageObject, ConsoleColor? logLevelColor = null, ConsoleColor? messageColor = null)
        {
            var message = BuildMessage(messageObject, logLevel, logLevelColor, messageColor);

            message = AddTimeStampIfEnabled(message);

            message = message.Replace("\n", "\n\t");

            WriteStdout(message, messageColor);
        }

        string BuildMessage(object messageObject, string logLevel, ConsoleColor? logLevelColor, ConsoleColor? messageColor)
        {
            var message = messageObject.ToString();

            if (_supportsAnsiColors)
            {
                return AddAnsiColorsToMessage(message, logLevel, logLevelColor, messageColor);
            }
            else
            {
                return $"[{logLevel}]{GetLogLevelSpacing(logLevel)}{_prefix}:{GetPrefixSpacing()}{message}";
            }
        }

        string AddAnsiColorsToMessage(string message, string logLevel, ConsoleColor? logLevelColor, ConsoleColor? messageColor)
        {
            var logLevelColorCode = logLevelColor == null
                ? AnsiColorFg.Default
                : GetAnsiColorFgEscSeq(logLevelColor.GetValueOrDefault());

            var msgColorCode = "";
            if (messageColor == null)
            {
                msgColorCode = AnsiColorFg.Default;
            }
            else
            {
                msgColorCode = GetAnsiColorFgEscSeq(messageColor.GetValueOrDefault());
            }

            var prefixColorCode = _prefixColor == null
                ? AnsiColorFg.Default
                : GetAnsiColorFgEscSeq(_prefixColor.GetValueOrDefault());

            return $"{logLevelColorCode}[{logLevel}]{AnsiColorFg.Default}{GetLogLevelSpacing(logLevel)}{prefixColorCode}{_prefix}:{GetPrefixSpacing()}{msgColorCode}{message}{AnsiColorFg.Default}";
        }

        string GetLogLevelSpacing(string logLevel)
        {
            var spaces = "";

            var spacesCount = 9 - logLevel.Length;

            for (int i = 0; i < spacesCount; i++)
            {
                spaces += ' ';
            }

            return spaces;
        }

        string GetPrefixSpacing()
        {
            var spaces = "";

            var spacesCount = 26 - _prefix.Length;

            for (int i = 0; i < spacesCount; i++)
            {
                spaces += ' ';
            }

            return spaces;
        }

        string AddTimeStampIfEnabled(string message)
        {
            if (EnableTimestamps == false) return message;
            var dateTimeString = DateTime.Now.ToString("o");
            if (_supportsAnsiColors)
            {
                dateTimeString = AddAnsiColorsToDateTimeString(dateTimeString);
            }
            return $"{dateTimeString} {message}";
        }

        string AddAnsiColorsToDateTimeString(string dateTimeString)
        {
            var split = dateTimeString.Split('T');
            dateTimeString = AnsiColorFg.Green + split[0];
            dateTimeString += AnsiColorFg.DarkGray + 'T';
            var split2 = split[1].Split('.');
            dateTimeString += AnsiColorFg.DarkGreen + split2[0];
            dateTimeString += AnsiColorFg.DarkGray + '.';
            dateTimeString += AnsiColorFg.Green + split2[1];
            dateTimeString += AnsiColorFg.Default;
            return dateTimeString;
        }

        void WriteStdout(string message, ConsoleColor? messageColor)
        {
            if (_supportsAnsiColors)
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
            Console.WriteLine(message + AnsiColorFg.Default + AnsiColorBg.Default);
        }

        string GetAnsiColorFgEscSeq(ConsoleColor color)
        {
            switch (color)
            {
                case ConsoleColor.Black: return AnsiColorFg.Black;
                case ConsoleColor.DarkBlue: return AnsiColorFg.DarkBlue;
                case ConsoleColor.DarkGreen: return AnsiColorFg.DarkGreen;
                case ConsoleColor.DarkCyan: return AnsiColorFg.DarkCyan;
                case ConsoleColor.DarkRed: return AnsiColorFg.DarkRed;
                case ConsoleColor.DarkMagenta: return AnsiColorFg.DarkMagenta;
                case ConsoleColor.DarkYellow: return AnsiColorFg.DarkYellow;
                case ConsoleColor.Gray: return AnsiColorFg.Gray;
                case ConsoleColor.DarkGray: return AnsiColorFg.DarkGray;
                case ConsoleColor.Blue: return AnsiColorFg.Blue;
                case ConsoleColor.Green: return AnsiColorFg.Green;
                case ConsoleColor.Cyan: return AnsiColorFg.Cyan;
                case ConsoleColor.Red: return AnsiColorFg.Red;
                case ConsoleColor.Magenta: return AnsiColorFg.Magenta;
                case ConsoleColor.Yellow: return AnsiColorFg.Yellow;
                case ConsoleColor.White: return AnsiColorFg.White;
                default: return AnsiColorFg.Default;
            }
        }

        string GetAnsiColorBackgroundCode(ConsoleColor color)
        {
            switch (color)
            {
                case ConsoleColor.Black: return AnsiColorBg.Black;
                case ConsoleColor.DarkBlue: return AnsiColorBg.DarkBlue;
                case ConsoleColor.DarkGreen: return AnsiColorBg.DarkGreen;
                case ConsoleColor.DarkCyan: return AnsiColorBg.DarkCyan;
                case ConsoleColor.DarkRed: return AnsiColorBg.DarkRed;
                case ConsoleColor.DarkMagenta: return AnsiColorBg.DarkMagenta;
                case ConsoleColor.DarkYellow: return AnsiColorBg.DarkYellow;
                case ConsoleColor.Gray: return AnsiColorBg.Gray;
                case ConsoleColor.DarkGray: return AnsiColorBg.DarkGray;
                case ConsoleColor.Blue: return AnsiColorBg.Blue;
                case ConsoleColor.Green: return AnsiColorBg.Green;
                case ConsoleColor.Cyan: return AnsiColorBg.Cyan;
                case ConsoleColor.Red: return AnsiColorBg.Red;
                case ConsoleColor.Magenta: return AnsiColorBg.Magenta;
                case ConsoleColor.Yellow: return AnsiColorBg.Yellow;
                case ConsoleColor.White: return AnsiColorBg.White;
                default: return AnsiColorBg.Default;
            }
        }

        static class AnsiColorFg
        {
            const string prefix = "\x1b[";
            const string suffix = "m";
            public static string Black = prefix + 30 + suffix;
            public static string DarkBlue = prefix + 34 + suffix;
            public static string DarkGreen = prefix + 32 + suffix;
            public static string DarkCyan = prefix + 36 + suffix;
            public static string DarkRed = prefix + 31 + suffix;
            public static string DarkMagenta = prefix + 35 + suffix;
            public static string DarkYellow = prefix + 33 + suffix;
            public static string Gray = prefix + 37 + suffix;
            public static string DarkGray = prefix + 90 + suffix;
            public static string Blue = prefix + 94 + suffix;
            public static string Green = prefix + 92 + suffix;
            public static string Cyan = prefix + 96 + suffix;
            public static string Red = prefix + 91 + suffix;
            public static string Magenta = prefix + 95 + suffix;
            public static string Yellow = prefix + 93 + suffix;
            public static string White = prefix + 97 + suffix;
            public static string Default = prefix + 39 + suffix;
        }

        static class AnsiColorBg
        {
            const string prefix = "\x1b[";
            const string suffix = "m";
            public static string Black = prefix + 40 + suffix;
            public static string DarkBlue = prefix + 44 + suffix;
            public static string DarkGreen = prefix + 42 + suffix;
            public static string DarkCyan = prefix + 46 + suffix;
            public static string DarkRed = prefix + 41 + suffix;
            public static string DarkMagenta = prefix + 45 + suffix;
            public static string DarkYellow = prefix + 43 + suffix;
            public static string Gray = prefix + 47 + suffix;
            public static string DarkGray = prefix + 100 + suffix;
            public static string Blue = prefix + 104 + suffix;
            public static string Green = prefix + 102 + suffix;
            public static string Cyan = prefix + 106 + suffix;
            public static string Red = prefix + 101 + suffix;
            public static string Magenta = prefix + 105 + suffix;
            public static string Yellow = prefix + 103 + suffix;
            public static string White = prefix + 107 + suffix;
            public static string Default = prefix + 49 + suffix;
        }

        // Emoji confirmed to work in gitbash
        // ✂
        // ✈
        // ✉
        // ✏
        // ✒
        // ✔
        // ✖
        // ✳
        // ✴
        // ❄
        // ❇
        // ❗
        // ❤
        // ♥
        // ➡
        // ©
        // ®
        // ‼
        // 8⃣
        // 9⃣
        // 7⃣
        // 6⃣
        // 1⃣
        // 0⃣
        // 2⃣
        // 3⃣
        // 5⃣
        // 4⃣
        // #⃣
        // ™
        // ↔
        // ↕
        // ▪
        // ▫
        // ☀
        // ☁
        // ☎
        // ☑
        // ☺
        // ♈
        // ♉
        // ♊
        // ♋
        // ♌
        // ♍
        // ♎
        // ♏
        // ♐
        // ♑
        // ♒
        // ♓
        // ♠
        // ♣
        // ♥
        // ♦
        // ⭕
        // 〰
        // 〽
        // ㊗
        // ㊙
        // ♥
        // ☺
        // ☻
        // ♥
        // ♦
        // ♣
        // ♠
        // •
        // ◘
        // ○
        // ◙
        // ♂
        // ♀
        // ♪
        // ♫
        // ☼
        // ♂
        // ♀
        // ♪
        // ♫
        // ☼
        // ►
        // ◄
        // ↕
        // ‼
        // ¶
        // §
        // ▬
        // ↨
        // ↑
        // ↓
        // →
        // ←
        // ∟
        // ↔
        // ▲
        // ☺
        // ☻
        // ▼
    }
}
