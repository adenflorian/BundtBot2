using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using BundtBot.Youtube;
using BundtCommon;
using BundtCord.Discord;
using DiscordApiWrapper.Audio;
using Newtonsoft.Json;

namespace BundtBot
{
    public class BundtBot
    {
        static readonly MyLogger _logger = new MyLogger(nameof(BundtBot));

        DiscordClient _client;
        DJ _dj = new DJ();
        CommandManager _commandManager = new CommandManager();
        YoutubeDl _youtubeDl;

        public async Task StartAsync()
        {
            _client = SetupDiscordClient();
            _youtubeDl = SetupYoutubeDl();

            RegisterEventHandlers();
            RegisterCommands();
            RegisterAudioCommands();

            _dj.Start();
            await _client.ConnectAsync();
        }

        DiscordClient SetupDiscordClient()
        {
            return new DiscordClient(File.ReadAllText("bottoken"));
        }

        YoutubeDl SetupYoutubeDl()
        {
            var youtubeOutputFolder = new DirectoryInfo("audio-youtube");
            if (youtubeOutputFolder.Exists == false) youtubeOutputFolder.Create();

            var youtubeTempFolder = new DirectoryInfo("temp-youtube");
            if (youtubeTempFolder.Exists) youtubeTempFolder.Delete(true);
            youtubeTempFolder.Create();

            return new YoutubeDl(youtubeOutputFolder, youtubeTempFolder);
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
                catch (CommandException ce)
                {
                    await message.ReplyAsync(ce.Message);
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
                    await server.TextChannels.First().SendMessageAsync("bundtbot online");
                    await DoYoutubeCommandAsync(server.TextChannels.First(), BundtFig.GetValue("on-server-created-yt"));
                }
                catch (Exception ex)
                {
                    _logger.LogError("Exception thrown while handling event " + nameof(_client.ServerCreated));
                    _logger.LogError(ex);
                }
			};
            _client.Ready += async (ready) => {
                try
                {
                    _logger.LogInfo("Client is Ready/Connected! ໒( ͡ᵔ ▾ ͡ᵔ )७", ConsoleColor.Green);
                    _logger.LogInfo("Setting game...");
                    await _client.SetGameAsync(Assembly.GetEntryAssembly().GetName().Version.ToString());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex);
                }
			};
			_client.TextChannelCreated += async (textChannel) => {
				try {
					await textChannel.SendMessageAsync("less is more");
				} catch (Exception ex) {
					_logger.LogError(ex);
				}
			};
        }

        void RegisterCommands()
        {
            _commandManager.CommandPrefix = BundtFig.GetValue("command-prefix");

            _commandManager.AddCommand(new TextCommand("hi", async (message, receivedCommand) =>
            {
                await message.ReplyAsync(BundtFig.GetValue("hi-response"));
            }));
            _commandManager.AddCommand(new TextCommand("poke", async (message, receivedCommand) =>
            {
                await message.ReplyAsync(BundtFig.GetValue("poke-response"));
            }));
            _commandManager.AddCommand(new TextCommand("help", async (message, receivedCommand) =>
            {
                var helpMessage = "";
                _commandManager.GetCommands().ToList().ForEach(x => helpMessage += $"`{x.Name}` ");
                await message.ReplyAsync("help me help you: " + helpMessage + "\nhttps://github.com/AdenFlorian/BundtBot2");
            }));
            _commandManager.AddCommand(new TextCommand("echo", async (message, receivedCommand) =>
            {
                await message.ReplyAsync(receivedCommand.ArgumentsString);
            }, minimumArgCount: 1));
            _commandManager.AddCommand(new TextCommand("bugreport", async (message, receivedCommand) =>
            {
                await message.ReplyAsync("It's not a bug, it's a feature...");
                await Task.Delay(TimeEx._3seconds);
                await message.ReplyAsync("...but if it's really a bug, please create an issue here: https://github.com/AdenFlorian/BundtBot2");
            }));
            _commandManager.AddCommand(new TextCommand("github", async (message, receivedCommand) =>
            {
                await message.ReplyAsync(":octopus: :cat: https://github.com/AdenFlorian/BundtBot2");
            }));
            _commandManager.AddCommand(new TextCommand("dog", async (message, receivedCommand) =>
            {
                var randomDogBaseUrl = "https://random.dog/";
                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(2);
                    var dogId = await httpClient.GetStringAsync(randomDogBaseUrl + "woof");
                    await message.ReplyAsync("Dogs rule:\n" + randomDogBaseUrl + dogId);
                }
            }));
            _commandManager.AddCommand(new TextCommand("cat", async (message, receivedCommand) =>
            {
                if (new Random().NextDouble() >= 0.5)
                {
                    var randomCatBaseUrl = "http://random.cat/";
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.Timeout = TimeSpan.FromSeconds(2);
                        var meow = await httpClient.GetStringAsync(randomCatBaseUrl + "meow");
                        var pFrom = meow.IndexOf("\\/i\\/", StringComparison.Ordinal) + "\\/i\\/".Length;
                        var pTo = meow.LastIndexOf("\"}", StringComparison.Ordinal);
                        var cat = "http://random.cat/i/" + meow.Substring(pFrom, pTo - pFrom);
                        await message.ReplyAsync("Cats drool:\n" + cat);
                    }
                }
                else
                {
                    var randomDogBaseUrl = "https://random.dog/";
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.Timeout = TimeSpan.FromSeconds(2);
                        var dogId = await httpClient.GetStringAsync(randomDogBaseUrl + "woof");
                        await message.ReplyAsync("How about a dog instead:\n" + randomDogBaseUrl + dogId);
                    }
                }
            }));
        }

        void RegisterAudioCommands()
        {
            _commandManager.AddCommand(new TextCommand("yt", async (message, receivedCommand) =>
            {
                await DoYoutubeCommandAsync(message.TextChannel, receivedCommand.ArgumentsString);
            }, minimumArgCount: 1));
            _commandManager.AddCommand(new TextCommand("next", async (message, receivedCommand) =>
            {
                try
                {
                    _dj.Next();
                    await message.ReplyAsync("Yea, I wasn't a huge fan of that song either :track_next:");
                }
                catch (DJException dje) { await message.ReplyAsync(dje.Message); }
            }));
            _commandManager.AddCommand(new TextCommand("stop", async (message, receivedCommand) =>
            {
                try
                {
                    _dj.StopAudioAsync();
                    await message.ReplyAsync("Please don't :stop_button: the music :frowning:");
                }
                catch (DJException dje) { await message.ReplyAsync(dje.Message); }
            }));
            _commandManager.AddCommand(new TextCommand("resume", async (message, receivedCommand) =>
            {
                try
                {
                    _dj.ResumeAudio();
                    await message.ReplyAsync("Green light! :arrow_forward:");
                }
                catch (DJException dje) { await message.ReplyAsync(dje.Message); }
            }));
            _commandManager.AddCommand(new TextCommand("pause", async (message, receivedCommand) =>
            {
                try
                {
                    await _dj.PauseAudioAsync();
                    await message.ReplyAsync("Red Light! :rotating_light:");
                }
                catch (DJException dje) { await message.ReplyAsync(dje.Message); }
            }));
            _commandManager.AddCommand(new TextCommand("faster", async (message, receivedCommand) =>
            {
                try
                {
                    _dj.FastForward();
                    await message.ReplyAsync("Double time!");
                }
                catch (DJException dje) { await message.ReplyAsync(dje.Message); }
            }));
            _commandManager.AddCommand(new TextCommand("slower", async (message, receivedCommand) =>
            {
                try
                {
                    _dj.SloMo();
                    await message.ReplyAsync("Half time!");
                }
                catch (DJException dje) { await message.ReplyAsync(dje.Message); }
            }));
            _commandManager.AddCommand(new TextCommand("nofx", async (message, receivedCommand) =>
            {
                try
                {
                    _dj.StopEffects();
                    await message.ReplyAsync("Single time!...?");
                }
                catch (DJException dje) { await message.ReplyAsync(dje.Message); }
            }));
        }

        async Task DoYoutubeCommandAsync(TextChannel textchannel, string args)
        {
            if (textchannel.Server.VoiceChannels.Count() == 0)
            {
                await textchannel.SendMessageAsync("Y'all need a voice channel...");
                return;
            }

            // TODO Ensure requesting user is in audio channel
            try
            {
                YoutubeDlUrl youtubeDlUrl;

                if (Uri.IsWellFormedUriString(args, UriKind.Absolute))
                {
                    youtubeDlUrl = YoutubeDlUrl.FromUrl(new Uri(args));
                }
                else
                {
                    youtubeDlUrl = YoutubeDlUrl.FromSearchString(args);
                }

                var youtubeInfo = await _youtubeDl.DownloadInfoAsync(youtubeDlUrl);

                var audioFile = new FileInfo(_youtubeDl.OutputFolder.FullName + '/' + youtubeInfo.Id + ".wav");

                if (audioFile.Exists)
                {
                    _dj.EnqueueAudio(audioFile, textchannel.Server.VoiceChannels.First());
                    await textchannel.SendMessageAsync($"'{youtubeInfo.Title}' added to queue from cache");
                }
                else
                {
                    var youtubeResult = await _youtubeDl.DownloadAudioAsync(youtubeDlUrl, YoutubeDlAudioFormat.wav, 100);
                    _dj.EnqueueAudio(youtubeResult.DownloadedFile, textchannel.Server.VoiceChannels.First());
                    await textchannel.SendMessageAsync($"'{youtubeResult.Info.Title}' added to queue");
                }
            }
            catch (YoutubeException ye)
            {
                _logger.LogWarning(ye);
                await textchannel.SendMessageAsync(ye.Message);
            }
        }
    }
}
