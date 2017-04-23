using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BundtCord.Discord;
using DiscordApiWrapper.Audio;

namespace BundtBot
{
    public class BundtBot
    {
        static readonly MyLogger _logger = new MyLogger(nameof(BundtBot));

        DiscordClient _client;
        DJ _dj = new DJ();

        public async Task StartAsync()
        {
            _client = new DiscordClient(File.ReadAllText("bottoken"));

            RegisterEventHandlers();
            _dj.Start();

            await _client.ConnectAsync();
        }

        void RegisterEventHandlers()
        {
            _client.TextChannelMessageReceived += async (message) =>
            {
                try
                {
                    await ProcessTextMessageAsync(message);
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
                    SayHello(server);
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

        async Task ProcessTextMessageAsync(TextChannelMessage message)
        {
            if (message.Author.User.Id == _client.Me.Id) return;

            var messageContent = message.Content;

            switch (message.Content)
            {
                case "!echo ": await EchoCommand(message); break;
                case "!hello": await HelloCommand(message); break;
                case "!ms": await MarbleSodaCommand(message); break;
                case "!pause": await PauseCommand(message); break;
                case "!resume": await ResumeCommandAsync(message); break;
                case "!stop": await StopCommandAsync(message); break;
                case "!next": await NextCommandAsync(message); break;
                //case "!yt": await NextCommandAsync(message); break;
                default: return;
            }
        }

        async Task NextCommandAsync(TextChannelMessage message)
        {
            try
            {
                _dj.Next();
                await message.ReplyAsync("Yea, I wasn't a huge fan of that song either :track_next:");
            }
            catch (DJException dje) { await message.ReplyAsync(dje.Message); }
            catch (Exception ex) { _logger.LogError(ex); }
        }

        async Task StopCommandAsync(TextChannelMessage message)
        {
            try
            {
                _dj.StopAudioAsync();
                await message.ReplyAsync("Please don't :stop_button: the music :frowning:");
            }
            catch (DJException dje) { await message.ReplyAsync(dje.Message); }
            catch (Exception ex) { _logger.LogError(ex); }
        }

        async Task ResumeCommandAsync(TextChannelMessage message)
        {
            try
            {
                _dj.ResumeAudio();
                await message.ReplyAsync("Green light! :arrow_forward:");
            }
            catch (DJException dje) { await message.ReplyAsync(dje.Message); }
            catch (Exception ex) { _logger.LogError(ex); }
        }

        async Task PauseCommand(TextChannelMessage message)
        {
            try
            {
                await _dj.PauseAudioAsync();
                await message.ReplyAsync("Red Light! :rotating_light:");
            }
            catch (DJException dje) { await message.ReplyAsync(dje.Message); }
            catch (Exception ex) { _logger.LogError(ex); }
        }

        async Task EchoCommand(TextChannelMessage message)
        {
            await message.ReplyAsync(message.Content.Substring(5));
        }

        async Task HelloCommand(TextChannelMessage message)
        {
            var voiceChannel = message.Author.VoiceChannel;
            if (voiceChannel == null)
            {
                await message.ReplyAsync("You're going to want to be in a voice channel for this...");
                return;
            }

            var fullSongPcm = new WavFileReader().ReadFileBytes(new FileInfo("audio/bbhw.wav"));
            _dj.EnqueueAudio(fullSongPcm, voiceChannel);
            await message.ReplyAsync("Hello added to queue");
        }

        async Task MarbleSodaCommand(TextChannelMessage message)
        {
            var voiceChannel = message.Author.VoiceChannel;
            if (voiceChannel == null)
            {
                await message.ReplyAsync("You're going to want to be in a voice channel for this...");
                return;
            }

            var fullSongPcm = new WavFileReader().ReadFileBytes(new FileInfo("audio/ms.wav"));
            _dj.EnqueueAudio(fullSongPcm, voiceChannel);
            await message.ReplyAsync("Marble Soda Best Soda added to queue");
        }

        void SayHello(Server server)
        {
            if (server.VoiceChannels.Count() == 0) return;

            var voiceChannel = server.VoiceChannels.First();

            var fullSongPcm = new WavFileReader().ReadFileBytes(new FileInfo("audio/bbhw.wav"));
            _dj.EnqueueAudio(fullSongPcm, voiceChannel);
        }
    }
}
