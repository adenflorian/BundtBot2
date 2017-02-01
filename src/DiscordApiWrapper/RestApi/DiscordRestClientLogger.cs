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
                new LogMessage($"Requested "),
                new LogMessage($"{request.Method} ", ConsoleColor.Magenta),
                new LogMessage(request.RequestUri.PathAndQuery, ConsoleColor.DarkMagenta));
            _logger.LogTrace(request);
            if (request.Content != null)
            {
                _logger.LogTrace(await request.Content.ReadAsStringAsync());
            }

            var response = await base.SendAsync(request, cancellationToken);

            _logger.LogInfo(
                new LogMessage($"Received "),
                new LogMessage($"{(int)response.StatusCode} {response.StatusCode}", ConsoleColor.Magenta),
                new LogMessage($" in response to "),
                new LogMessage($"{response.RequestMessage.Method} ", ConsoleColor.Magenta),
                new LogMessage($"{response.RequestMessage.RequestUri.PathAndQuery}", ConsoleColor.DarkMagenta));
            _logger.LogTrace(response);
            if (response.Content != null)
            {
                _logger.LogTrace(await response.Content.ReadAsStringAsync());
            }

            return response;
        }
    }
}