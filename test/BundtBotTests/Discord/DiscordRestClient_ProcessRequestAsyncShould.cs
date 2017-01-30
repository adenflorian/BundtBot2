using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BundtBot.Discord;
using DiscordApiWrapper.RestApi.RestApiRequests;
using Xunit;

namespace BundtBot.Tests.Discord
{
	public class DiscordRestClient_ProcessRequestAsyncShould
	{
		readonly TestHelper _helper = new TestHelper();

		[Fact]
		public async Task ThrowDiscordRestExceptionWhenGetIsUnsuccessfull()
		{
			var stubHandler = new StubHtpMessageHandler("", HttpStatusCode.InternalServerError);
			var stubHttpClient = new HttpClient(stubHandler);
			var client = _helper.CreateDiscordRestClient("token", "name", "version", stubHttpClient);
			var ex = await Assert.ThrowsAsync<DiscordRestException>(() => client.ProcessRequestAsync(new GetRequest("test")));
			Assert.Contains("InternalServerError", ex.Message);
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

			protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
				CancellationToken cancellationToken)
			{
				var response = new HttpResponseMessage(_returnCode) {
					Content = new StringContent(_responseContent)
				};
				await Task.Delay(1);
				return response;
			}
		}
	}
}
