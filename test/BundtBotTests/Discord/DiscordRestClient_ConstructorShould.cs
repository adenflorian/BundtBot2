using System;
using System.Net.Http.Headers;
using BundtBot.Discord;
using DiscordApiWrapper.RestApi;
using Xunit;

namespace BundtBot.Tests.Discord
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
        public void ThrowArgumentExceptionWhenPassedNullOrWhitespaceArgumentsAsync(
            string token, string name, string version)
        {
            Assert.Throws<ArgumentException>(() => CreateDiscordRestClient(token, name, version));
        }

        [Fact]
		public void SetBaseAddressToDiscordAppDotComSlashApiAlways()
		{
			Assert.Equal(new Uri("https://discordapp.com/api/"),
				CreateDiscordRestClient().HttpClient.BaseAddress);
		}

		[Fact]
		public void SetTimeoutToOneSecondAlways()
		{
			Assert.Equal(TimeSpan.FromSeconds(1), CreateDiscordRestClient().HttpClient.Timeout);
		}

		[Fact]
		public void SetAcceptHeaderToJsonOnlyAlways()
		{
			var discordRestClient = CreateDiscordRestClient();
			Assert.Equal(1, discordRestClient.HttpClient.DefaultRequestHeaders.Accept.Count);
			Assert.Contains(new MediaTypeWithQualityHeaderValue("application/json"),
				CreateDiscordRestClient().HttpClient.DefaultRequestHeaders.Accept);
		}

		[Fact]
		public void SetAuthorizationHeaderSchemeToBotAlways()
		{
			Assert.Equal("Bot",
				CreateDiscordRestClient().HttpClient.DefaultRequestHeaders.Authorization.Scheme);
		}

		[Fact]
		public void SetAuthorizationHeaderUsingBotTokenAlways()
		{
			var discordRestClient = CreateDiscordRestClient("botToken");
			Assert.Equal("botToken", discordRestClient.HttpClient.DefaultRequestHeaders.Authorization.Parameter);
		}

		[Fact]
		public void SetUserAgentHeaderUsingNameAndVersionAlways()
		{
			var discordRestClient = CreateDiscordRestClient("token", "productName", "productVersion");
			var userAgentHeader = discordRestClient.HttpClient.DefaultRequestHeaders.UserAgent.ToString();
			Assert.Equal("productName/productVersion", userAgentHeader);
		}

		DiscordRestClient CreateDiscordRestClient(string token = "token", string name = "name",
			string version = "version")
		{
			var config = new RestClientConfig
			{
				BotToken = token,
				Name = name,
				Version = version,
				BaseAddress = new Uri("https://discordapp.com/api/")
			};
			return new DiscordRestClient(config);
		}
	}
}
