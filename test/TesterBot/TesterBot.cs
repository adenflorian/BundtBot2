using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BundtBot;
using BundtCommon;
using BundtCord.Discord;
using Newtonsoft.Json;

namespace TesterBot
{
    public class TesterBot
    {
        static readonly MyLogger _logger = new MyLogger(nameof(TesterBot));

        DiscordClient _client;

        public async Task StartAsync()
        {
            _client = new DiscordClient(File.ReadAllText("bottoken"));

            RegisterEventHandlers();

            await _client.ConnectAsync();
        }

        void RegisterEventHandlers()
        {
            _client.TextChannelMessageReceived += (message) =>
            {
                try
                {
                    if (message.Author.User.Id == _client.Me.Id) return;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Exception thrown while handling event " + nameof(_client.TextChannelMessageReceived));
                    _logger.LogError(ex);
                }
            };
            _client.ServerCreated += async (server) => {
                try
                {
                    await server.TextChannels.First().SendMessageAsync("testerbot online");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Exception thrown while handling event " + nameof(_client.ServerCreated));
                    _logger.LogError(ex);
                }
			};
            _client.Ready += (ready) => {
                try
                {
                    _logger.LogInfo("Client is Ready/Connected! ໒( ͡ᵔ ▾ ͡ᵔ )७", ConsoleColor.Green);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex);
                }
			};
        }
    }
}
