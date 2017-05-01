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
        CommandManager _commandManager = new CommandManager();

        public async Task StartAsync()
        {
            _client = new DiscordClient(File.ReadAllText("bottoken"));

            RegisterEventHandlers();
            RegisterCommands();

            await _client.ConnectAsync();
        }

        void RegisterEventHandlers()
        {
            _client.TextChannelMessageReceived += async (message) =>
            {
                try
                {
                    if (message.Author.User.Id == _client.Me.Id) return;
                    await _commandManager.ProcessTextMessageAsync(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Exception thrown while handling event " + nameof(_client.TextChannelMessageReceived));
                    _logger.LogError(ex);
                }
            };
            _client.ServerCreated += async (server) =>
            {
                try
                {
                    await server.TextChannels.First().SendMessageAsync("testbot online :robot:");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Exception thrown while handling event " + nameof(_client.ServerCreated));
                    _logger.LogError(ex);
                }
            };
        }

        void RegisterCommands()
        {
            _commandManager.CommandPrefix = "~";

            _commandManager.AddCommand(new TextCommand("hi", async (message, receivedCommand) =>
            {
                await message.ReplyAsync("sorry, busy, no time to talk.");
            }));
            _commandManager.AddCommand(new TextCommand("testdev", async (message, receivedCommand) =>
            {
                await RunVarietyTestAsync(message.Server, "$");
            }));
            _commandManager.AddCommand(new TextCommand("testtest", async (message, receivedCommand) =>
            {
                await RunVarietyTestAsync(message.Server, "!");
            }));
        }

        async Task RunVarietyTestAsync(Server server, string commandPrefix)
        {
            await server.TextChannels.First().SendMessageAsync("testerbot online");
            await server.TextChannels.First().SendMessageAsync(commandPrefix + "yt mac 420");
            await server.TextChannels.First().SendMessageAsync(commandPrefix + "yt mac 420");
            await Task.Delay(TimeEx._10seconds);
            await server.TextChannels.First().SendMessageAsync(commandPrefix + "pause");
            await Task.Delay(TimeEx._1second);
            await server.TextChannels.First().SendMessageAsync(commandPrefix + "next");
            await server.TextChannels.First().SendMessageAsync(commandPrefix + "yt mac 420");
            await Task.Delay(TimeEx._1second);
            await server.TextChannels.First().SendMessageAsync(commandPrefix + "yt heyayaya");
            await Task.Delay(TimeEx._1second);
            await server.TextChannels.First().SendMessageAsync(commandPrefix + "yt x gonna give it to ya");
            await Task.Delay(TimeEx._5seconds);
            await server.TextChannels.First().SendMessageAsync(commandPrefix + "faster");
            await Task.Delay(TimeEx._5seconds);
            await server.TextChannels.First().SendMessageAsync(commandPrefix + "nofx");
            await Task.Delay(TimeEx._2seconds);
            await server.TextChannels.First().SendMessageAsync(commandPrefix + "slower");
            await Task.Delay(TimeEx._2seconds);
            await server.TextChannels.First().SendMessageAsync(commandPrefix + "slower");
            await Task.Delay(TimeEx._2seconds);
            await server.TextChannels.First().SendMessageAsync(commandPrefix + "faster");
            await Task.Delay(TimeEx._2seconds);
            await server.TextChannels.First().SendMessageAsync(commandPrefix + "faster");
            await Task.Delay(TimeEx._2seconds);
            await server.TextChannels.First().SendMessageAsync(commandPrefix + "faster");
            await Task.Delay(TimeEx._2seconds);
            await server.TextChannels.First().SendMessageAsync(commandPrefix + "faster");
            await Task.Delay(TimeEx._2seconds);
            await server.TextChannels.First().SendMessageAsync(commandPrefix + "slower");
            await Task.Delay(TimeEx._2seconds);
            await server.TextChannels.First().SendMessageAsync(commandPrefix + "slower");
            await Task.Delay(TimeEx._2seconds);
            await server.TextChannels.First().SendMessageAsync(commandPrefix + "next");
            await Task.Delay(TimeEx._2seconds);
            await server.TextChannels.First().SendMessageAsync(commandPrefix + "pause");
            await Task.Delay(TimeEx._3seconds);
            await server.TextChannels.First().SendMessageAsync(commandPrefix + "resume");
            await Task.Delay(TimeEx._3seconds);
            await server.TextChannels.First().SendMessageAsync(commandPrefix + "faster");
            await Task.Delay(TimeEx._5seconds);
            await server.TextChannels.First().SendMessageAsync(commandPrefix + "next");
            await Task.Delay(TimeEx._3seconds);
            await server.TextChannels.First().SendMessageAsync(commandPrefix + "stop");
            await Task.Delay(TimeEx._2seconds);
            await server.TextChannels.First().SendMessageAsync("Time for the lightning round!");
            await Task.Delay(TimeEx._1second);
            await server.TextChannels.First().SendMessageAsync(commandPrefix + "pause");
            await server.TextChannels.First().SendMessageAsync(commandPrefix + "resume");
            await server.TextChannels.First().SendMessageAsync(commandPrefix + "faster");
            await server.TextChannels.First().SendMessageAsync(commandPrefix + "nofx");
            await server.TextChannels.First().SendMessageAsync(commandPrefix + "next");
            await server.TextChannels.First().SendMessageAsync(commandPrefix + "stop");
            await Task.Delay(TimeEx._1second);
            await server.TextChannels.First().SendMessageAsync("Tests complete! Good job @bundtbot");
        }
    }
}
