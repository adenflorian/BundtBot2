using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BundtCord.Discord;

namespace BundtBot
{
    public class BundtBot
    {
        DiscordClient _client;

        static readonly MyLogger _logger = new MyLogger(nameof(BundtBot));

        //internal static Dictionary<Guild, TextChannel> TextChannelOverrides = new Dictionary<Guild, TextChannel>();

        public async Task Start()
        {
            _client = new DiscordClient(File.ReadAllText("bottoken"));

            RegisterEventHandlers();

            await _client.Connect();
        }

        void RegisterEventHandlers()
        {
            _client.MessageCreated += async (message) =>
            {
                try
                {
                    if (message.Author.Id == _client.Me.Id) return;
                    if (message.Content.StartsWith("echo ") == false) return;
                    await message.TextChannel.SendMessageAsync(message.Content.Substring(5));
                }
                catch (Exception ex)
                {
                    _logger.LogError("Exception thrown while handling event " + nameof(_client.MessageCreated));
                    _logger.LogError(ex);
                }
            };

            _client.ServerCreated += async (server) => {
                try
                {
                    await server.TextChannels.First().SendMessageAsync("yo");
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
			};
			
			_client.TextChannelCreated += async (textChannel) => {
				try {
					await textChannel.SendMessage("less is more");
					if (!textChannel.Name.ToLower().Contains("bundtbot")) return;
					TextChannelOverrides[textChannel.Guild] = textChannel;
				} catch (Exception ex) {
					_logger.LogError(ex);
				}
			};*/
        }
    }
}
