using System;
using DiscordApiWrapper.RestApi;

namespace BundtBot.Discord
{
	public class RateLimitExceededException : Exception
	{
		public RateLimitExceeded RateLimitExceeded;

		public RateLimitExceededException(RateLimitExceeded rateLimitExceeded)
		{
			RateLimitExceeded = rateLimitExceeded;
		}

		public RateLimitExceededException(RateLimitExceeded rateLimitExceeded, string message) : base(message)
		{
			RateLimitExceeded = rateLimitExceeded;
		}

		public RateLimitExceededException(RateLimitExceeded rateLimitExceeded, string message, Exception innerException) : base(message, innerException)
		{
			RateLimitExceeded = rateLimitExceeded;
		}
	}
}
