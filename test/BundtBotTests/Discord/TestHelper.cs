using System;
using System.Net.Http;
using BundtBot.Discord;
using DiscordApiWrapper.RestApi;

namespace BundtBot.Tests.Discord
{
    class TestHelper
	{
		public DiscordRestClient CreateDiscordRestClient(string token = "token", string name = "name",
			string version = "version", HttpClient httpClient = null)
		{
			var config = new RestClientConfig
			{
				BotToken = token,
				Name = name,
				Version = version,
				BaseAddress = new Uri("https://discordapp.com/api/")
			};
			return new DiscordRestClient(config, httpClient);
		}

		public HttpClientWrapper CreateHttpClientWrapper(string token = "token", string name = "name",
			string version = "version", HttpClient httpClient = null)
		{
			var config = new RestClientConfig
			{
				BotToken = token,
				Name = name,
				Version = version,
				BaseAddress = new Uri("https://discordapp.com/api/")
			};
			return new HttpClientWrapper(config, httpClient);
		}
	}
}
