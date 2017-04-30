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
                    await server.TextChannels.First().SendMessageAsync("!yt mac 420");
                    await server.TextChannels.First().SendMessageAsync("!yt mac 420");
                    await Task.Delay(TimeEx._10seconds);
                    await server.TextChannels.First().SendMessageAsync("!pause");
                    await Task.Delay(TimeEx._1second);
                    await server.TextChannels.First().SendMessageAsync("!next");
                    await server.TextChannels.First().SendMessageAsync("!yt mac 420");
                    await Task.Delay(TimeEx._1second);
                    await server.TextChannels.First().SendMessageAsync("!yt heyayaya");
                    await Task.Delay(TimeEx._1second);
                    await server.TextChannels.First().SendMessageAsync("!yt x gonna give it to ya");
                    await Task.Delay(TimeEx._5seconds);
                    await server.TextChannels.First().SendMessageAsync("!faster");
                    await Task.Delay(TimeEx._5seconds);
                    await server.TextChannels.First().SendMessageAsync("!nofx");
                    await Task.Delay(TimeEx._2seconds);
                    await server.TextChannels.First().SendMessageAsync("!slower");
                    await Task.Delay(TimeEx._2seconds);
                    await server.TextChannels.First().SendMessageAsync("!slower");
                    await Task.Delay(TimeEx._2seconds);
                    await server.TextChannels.First().SendMessageAsync("!faster");
                    await Task.Delay(TimeEx._2seconds);
                    await server.TextChannels.First().SendMessageAsync("!faster");
                    await Task.Delay(TimeEx._2seconds);
                    await server.TextChannels.First().SendMessageAsync("!faster");
                    await Task.Delay(TimeEx._2seconds);
                    await server.TextChannels.First().SendMessageAsync("!faster");
                    await Task.Delay(TimeEx._2seconds);
                    await server.TextChannels.First().SendMessageAsync("!slower");
                    await Task.Delay(TimeEx._2seconds);
                    await server.TextChannels.First().SendMessageAsync("!slower");
                    await Task.Delay(TimeEx._2seconds);
                    await server.TextChannels.First().SendMessageAsync("!next");
                    await Task.Delay(TimeEx._2seconds);
                    await server.TextChannels.First().SendMessageAsync("!pause");
                    await Task.Delay(TimeEx._3seconds);
                    await server.TextChannels.First().SendMessageAsync("!resume");
                    await Task.Delay(TimeEx._3seconds);
                    await server.TextChannels.First().SendMessageAsync("!faster");
                    await Task.Delay(TimeEx._5seconds);
                    await server.TextChannels.First().SendMessageAsync("!next");
                    await Task.Delay(TimeEx._3seconds);
                    await server.TextChannels.First().SendMessageAsync("!stop");
                    await Task.Delay(TimeEx._2seconds);
                    await server.TextChannels.First().SendMessageAsync("!pause");
                    await server.TextChannels.First().SendMessageAsync("!resume");
                    await server.TextChannels.First().SendMessageAsync("!faster");
                    await server.TextChannels.First().SendMessageAsync("!nofx");
                    await server.TextChannels.First().SendMessageAsync("!next");
                    await server.TextChannels.First().SendMessageAsync("!stop");
                    await Task.Delay(TimeEx._1seconds);
                    await server.TextChannels.First().SendMessageAsync("Tests complete! Good job @bundtbot");
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
