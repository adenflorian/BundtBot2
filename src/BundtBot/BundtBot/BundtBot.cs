using System.Threading.Tasks;
using BundtBot.Discord;

namespace BundtBot
{
    public class BundtBot
    {
	    public DiscordClient Client;

	    public async Task Start()
	    {
			Client = new DiscordClient();

			/*Client.GuildCreated += async (guild) => {
				await guild.Channels[0].SendMessage("yo");
			};*/

			Client.MessageCreated += async (message) => {
				if (message.Author.IsBot == false) {
					await message.Channel.SendMessage("hiya");
				}
			};

			await Client.Connect();
		}
    }
}
