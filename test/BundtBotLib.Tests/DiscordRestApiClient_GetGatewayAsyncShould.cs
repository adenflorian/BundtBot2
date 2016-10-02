using Xunit;

namespace BundtBotLib.Tests {
	public class DiscordRestApiClient_GetGatewayAsyncShould {
		readonly DiscordRestApiClient _discordRestApiClient;

		public DiscordRestApiClient_GetGatewayAsyncShould() {
			_discordRestApiClient = new DiscordRestApiClient();
		}

		[Fact]
		public async void ReturnAbsoluteUri() {
			var result = await _discordRestApiClient.GetGatewayAsync();
			Assert.True(result.IsAbsoluteUri);
		}
	}
}