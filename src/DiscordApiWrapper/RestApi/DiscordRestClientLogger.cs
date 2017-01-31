using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BundtBot;

namespace DiscordApiWrapper.RestApi
{
    class DiscordRestClientLogger : DelegatingHandler
    {
        static readonly MyLogger _logger = new MyLogger(nameof(DiscordRestClientLogger), ConsoleColor.DarkMagenta);

        public DiscordRestClientLogger(HttpMessageHandler innerHandler) : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            _logger.LogInfo(
                new LogMessage("Request: "),
                new LogMessage(request.RequestUri.ToString(), ConsoleColor.Magenta));
            _logger.LogTrace(request);
            if (request.Content != null)
            {
                _logger.LogTrace(await request.Content.ReadAsStringAsync());
            }

            var response = await base.SendAsync(request, cancellationToken);

            var logResponseMessage = "Response: " + response.StatusCode;
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInfo(logResponseMessage, ConsoleColor.DarkMagenta);
            }
            else
            {
                _logger.LogWarning(logResponseMessage);
            }
            _logger.LogTrace(response);
            if (response.Content != null)
            {
                _logger.LogTrace(await response.Content.ReadAsStringAsync());
            }

            return response;
        }
    }
}