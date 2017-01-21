using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BundtBot.Discord;
using Xunit;

namespace BundtBot.Tests.Discord
{
	public class DiscordRestClient_GetGatewayUrlShould
	{
		readonly DiscordRestClientTestHelper _helper = new DiscordRestClientTestHelper();

		[Fact]
		public async Task ThrowHttpRequestExceptionWhenGetIsUnsuccessfull()
		{
			var stubHandler = new StubHtpMessageHandler("", HttpStatusCode.InternalServerError);
			var stubHttpClient = new HttpClient(stubHandler);
			var client = _helper.CreateDiscordRestClient("token", "name", "version", stubHttpClient);
			var ex = await Assert.ThrowsAsync<DiscordRestException>(() => client.GetGatewayUrlAsync());
			Assert.Contains("code", ex.Message);
		}

		[Fact]
		public async Task ThrowExceptionWhenItReceivesEmptyJson()
		{
			var stubHandler = new StubHtpMessageHandler("", HttpStatusCode.OK);
			var stubHttpClient = new HttpClient(stubHandler);
			var client = _helper.CreateDiscordRestClient("token", "name", "version", stubHttpClient);
			var ex = await Assert.ThrowsAsync<DiscordRestException>(() => client.GetGatewayUrlAsync());
			Assert.Contains("null", ex.Message);
		}

		[Fact]
		public async Task ThrowExceptionWhenItReceivesBadJson()
		{
			var stubHandler = new StubHtpMessageHandler("bad json", HttpStatusCode.OK);
			var stubHttpClient = new HttpClient(stubHandler);
			var client = _helper.CreateDiscordRestClient("token", "name", "version", stubHttpClient);
			var ex = await Assert.ThrowsAsync<DiscordRestException>(() => client.GetGatewayUrlAsync());
			Assert.Contains("json", ex.Message);
		}

		[Fact]
		public async Task ReturnGatewayUrlWhenItReceivesGoodJson()
		{
			var stubHandler = new StubHtpMessageHandler("{\"url\":\"wss://gateway.discord.gg/\"}",
				HttpStatusCode.OK);
			var stubHttpClient = new HttpClient(stubHandler);
			var client = _helper.CreateDiscordRestClient("token", "name", "version", stubHttpClient);
			var gatewayUrl = await client.GetGatewayUrlAsync();
			Assert.Equal(new Uri("wss://gateway.discord.gg/"), gatewayUrl);
		}

		class StubHtpMessageHandler : HttpMessageHandler
		{
			readonly string _responseContent;
			readonly HttpStatusCode _returnCode;

			public StubHtpMessageHandler(string responseContent, HttpStatusCode returnStatusCode)
			{
				_responseContent = responseContent;
				_returnCode = returnStatusCode;
			}

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
				CancellationToken cancellationToken)
			{
				var response = new HttpResponseMessage(_returnCode) {
					Content = new StringContent(_responseContent)
				};
				return Task.Factory.StartNew(() => response, cancellationToken);
			}
		}
	}
}
