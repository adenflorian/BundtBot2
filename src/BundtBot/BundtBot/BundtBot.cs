using System;
using System.Reflection;
using System.Threading.Tasks;
using BundtBot.Discord;

namespace BundtBot
{
    public class BundtBot
    {
	    public DiscordClient Client;

		static readonly MyLogger _logger = new MyLogger(nameof(BundtBot));

		public async Task Start()
	    {
			Client = new DiscordClient();

			Client.GuildCreated += async (guild) => {
				await guild.Channels[0].SendMessage("yo");
			};

			Client.MessageCreated += async (message) => {
				if (message.Author.IsBot == false) {
					await message.TextChannel.SendMessage("hiya");
				}
			};

			Client.Ready += (ready) => {
				_logger.LogInfo("Client is Ready/Connected! ໒( ͡ᵔ ▾ ͡ᵔ )७", ConsoleColor.Green);
				_logger.LogInfo("Setting game...");
				Client.SetGame(Assembly.GetEntryAssembly().GetName().Version.ToString());
			};

			// TODO
			/*Client.ChannelCreated += async (sender, e) => {
				try {
					await e.Channel.SendMessageEx("less is more");
					if (e.Channel.Name.ToLower().Contains("bundtbot")) {
						if (BundtBot.TextChannelOverrides.ContainsKey(e.Server)) {
							BundtBot.TextChannelOverrides[e.Server] = e.Channel;
						} else {
							BundtBot.TextChannelOverrides.Add(e.Server, e.Channel);
						}
					}
				} catch (Exception ex) {
					MyLogger.WriteException(ex);
				}

			};*/

			await Client.Connect();
		}
    }
}
