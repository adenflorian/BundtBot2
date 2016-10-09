using System;
using BundtBot.Discord;
using Xunit;

namespace BundtBot.Tests
{
	public class DiscordRestClient_ConstructorShould
	{
		[Theory]
		[InlineData(null, "name", "version")]
		[InlineData(" ", "name", "version")]
		[InlineData("token", null, "version")]
		[InlineData("token", " ", "version")]
		[InlineData("token", "name", null)]
		[InlineData("token", "name", " ")]
		public void ThrowArgumentExceptionWhenPassedNullOrWhitespaceArguments(
			string token, string name, string version)
		{
			Assert.Throws<ArgumentException>(() => CreateDiscordRestClient(token, name, version));
		}

		[Fact]
		public void SetTimeoutToOneSecondAlways()
		{
			Assert.Equal(TimeSpan.FromSeconds(1), CreateDiscordRestClient().Timeout);
		}

		[Fact]
		public void SetAuthorizationHeaderSchemeToBotAlways()
		{
			Assert.Equal("Bot", CreateDiscordRestClient().DefaultRequestHeaders.Authorization.Scheme);
		}

		[Fact]
		public void SetAuthorizationHeaderUsingBotTokenAlways()
		{
			var discordRestClient = CreateDiscordRestClient("botToken");
			Assert.Equal("botToken", discordRestClient.DefaultRequestHeaders.Authorization.Parameter);
		}

		[Fact]
		public void SetUserAgentHeaderUsingNameAndVersionAlways()
		{
			var discordRestClient = CreateDiscordRestClient("token", "productName", "productVersion");
			var userAgentHeader = discordRestClient.DefaultRequestHeaders.UserAgent.ToString();
			Assert.Equal("productName/productVersion", userAgentHeader);
		}

		static DiscordRestClient CreateDiscordRestClient(string token = "token", string name = "name",
			string version = "version")
		{
			return new DiscordRestClient(token, name, version);
		}
	}
}
