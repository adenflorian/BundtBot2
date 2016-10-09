using System;

namespace BundtBot.Discord
{
	public class DiscordRestException : Exception
	{
		public DiscordRestException()
		{
		}

		public DiscordRestException(string message) : base(message)
		{
		}

		public DiscordRestException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
