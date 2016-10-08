using System;
using BundtBot.Discord;
using Xunit;

namespace BundtBot.Tests {
    public class DiscordRestApiHttpClient_ConstructorShould {
		[Fact]
        public void ThrowArgumentExceptionWhenPassedNullToken() {
			Assert.Throws<ArgumentException>(() => new DiscordRestClient(null, "name", "version"));
		}

		[Fact]
		public void ThrowArgumentExceptionWhenPassedEmptyToken() {
			Assert.Throws<ArgumentException>(() => new DiscordRestClient(string.Empty, "name", "version"));
		}
	}
}
