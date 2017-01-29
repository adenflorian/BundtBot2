using System;

namespace BundtBot
{
    public struct LogMessage
    {
        public string Message;
		public ConsoleColor? Color;
		public LogMessage(string message, ConsoleColor? color = null)
		{
			Message = message;
			Color = color;
		}
    }
}