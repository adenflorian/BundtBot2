using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BundtCord.Discord;
using DiscordApiWrapper.Audio;

namespace BundtBot
{
    public class BundtBot
    {
        DiscordClient _client;

        static readonly MyLogger _logger = new MyLogger(nameof(BundtBot));

        //internal static Dictionary<Guild, TextChannel> TextChannelOverrides = new Dictionary<Guild, TextChannel>();

        public async Task StartAsync()
        {
            _client = new DiscordClient(File.ReadAllText("bottoken"));

            RegisterEventHandlers();

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
                }
                catch (Exception ex)
                {
                    _logger.LogError("Exception thrown while handling event " + nameof(_client.ServerCreated));
                    _logger.LogError(ex);
                }
			};

            /*_client.Ready += (ready) => {
				_logger.LogInfo("Client is Ready/Connected! ໒( ͡ᵔ ▾ ͡ᵔ )७", ConsoleColor.Green);
				_logger.LogInfo("Setting game...");
				_client.SetGame(Assembly.GetEntryAssembly().GetName().Version.ToString());
			};*/
			
			/*_client.TextChannelCreated += async (textChannel) => {
				try {
					await textChannel.SendMessage("less is more");
					if (!textChannel.Name.ToLower().Contains("bundtbot")) return;
					TextChannelOverrides[textChannel.Guild] = textChannel;
				} catch (Exception ex) {
					_logger.LogError(ex);
				}
			};*/
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
                default: return;
            }
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

            await voiceChannel.JoinAsync();

            await Task.Delay(1000);

            var fullSongPcm = new WavFileReader().ReadFileBytes(new FileInfo("audio/bbhw.wav"));
            await voiceChannel.SendAudioAsync(fullSongPcm);

            await _client.LeaveVoiceChannelInServer(voiceChannel.Server);
        }

        async Task MarbleSodaCommand(TextChannelMessage message)
        {
            var voiceChannel = message.Author.VoiceChannel;
            if (voiceChannel == null)
            {
                await message.ReplyAsync("You're going to want to be in a voice channel for this...");
                return;
            }

            await voiceChannel.JoinAsync();

            await Task.Delay(1000);

            var fullSongPcm = new WavFileReader().ReadFileBytes(new FileInfo("audio/ms.wav"));
            await voiceChannel.SendAudioAsync(fullSongPcm);

            await _client.LeaveVoiceChannelInServer(voiceChannel.Server);
        }
    }
}
