using System;
using System.Net.Http.Headers;
using Xunit;

namespace BundtBot.Tests.Discord
{
	public class DiscordRestClient_ConstructorShould
	{
		readonly DiscordRestClientTestHelper _helper = new DiscordRestClientTestHelper();

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
			Assert.Throws<ArgumentException>(() => _helper.CreateDiscordRestClient(token, name, version));
		}

		[Fact]
		public void SetBaseAddressToDiscordAppDotComSlashApiAlways()
		{
			Assert.Equal(new Uri("https://discordapp.com/api/"),
				_helper.CreateDiscordRestClient().HttpClient.BaseAddress);
		}

		[Fact]
		public void SetTimeoutToOneSecondAlways()
		{
			Assert.Equal(TimeSpan.FromSeconds(1), _helper.CreateDiscordRestClient().HttpClient.Timeout);
		}

		[Fact]
		public void SetAcceptHeaderToJsonOnlyAlways()
		{
			var discordRestClient = _helper.CreateDiscordRestClient();
			Assert.Equal(1, discordRestClient.HttpClient.DefaultRequestHeaders.Accept.Count);
			Assert.Contains(new MediaTypeWithQualityHeaderValue("application/json"),
				_helper.CreateDiscordRestClient().HttpClient.DefaultRequestHeaders.Accept);
		}

		[Fact]
		public void SetAuthorizationHeaderSchemeToBotAlways()
		{
			Assert.Equal("Bot",
				_helper.CreateDiscordRestClient().HttpClient.DefaultRequestHeaders.Authorization.Scheme);
		}

		[Fact]
		public void SetAuthorizationHeaderUsingBotTokenAlways()
		{
			var discordRestClient = _helper.CreateDiscordRestClient("botToken");
			Assert.Equal("botToken", discordRestClient.HttpClient.DefaultRequestHeaders.Authorization.Parameter);
		}

		[Fact]
		public void SetUserAgentHeaderUsingNameAndVersionAlways()
		{
			var discordRestClient = _helper.CreateDiscordRestClient("token", "productName", "productVersion");
			var userAgentHeader = discordRestClient.HttpClient.DefaultRequestHeaders.UserAgent.ToString();
			Assert.Equal("productName/productVersion", userAgentHeader);
		}
	}
}
