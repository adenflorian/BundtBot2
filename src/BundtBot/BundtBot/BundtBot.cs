using System.Threading.Tasks;
using BundtBot.Discord;

namespace BundtBot
{
    public class BundtBot
    {
	    public DiscordClient Client;

		static readonly MyLogger _logger = new MyLogger(nameof(BundtBot));

		//internal static Dictionary<Guild, TextChannel> TextChannelOverrides = new Dictionary<Guild, TextChannel>();

		public async Task Start()
	    {
			Client = new DiscordClient();

			/*Client.GuildCreated += async (guild) => {
				await guild.TextChannels.First().SendMessage("yo");
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
			
			Client.TextChannelCreated += async (textChannel) => {
				try {
					await textChannel.SendMessage("less is more");
					if (!textChannel.Name.ToLower().Contains("bundtbot")) return;
					TextChannelOverrides[textChannel.Guild] = textChannel;
				} catch (Exception ex) {
					_logger.LogError(ex);
				}
			};*/

			await Client.Connect();
		}
    }
}
