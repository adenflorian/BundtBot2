using System.Net.Http;
using BundtBot.Discord;

namespace BundtBot.Tests.Discord
{
    class DiscordRestClientTestHelper
	{
		public DiscordRestClient CreateDiscordRestClient(string token = "token", string name = "name",
			string version = "version", HttpClient httpClient = null)
		{
			if (httpClient == null) {
				return new DiscordRestClient(token, name, version);
			} else {
				return new DiscordRestClient(token, name, version, httpClient);
			}
		}
	}
}
