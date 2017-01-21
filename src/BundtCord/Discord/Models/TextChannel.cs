using System.Threading.Tasks;

namespace BundtBot.Discord.Models
{
    public class TextChannel
    {
	    readonly GuildChannel _guildChannel;

	    internal DiscordClient Client {
		    get { return _guildChannel.Client; }
		    set { _guildChannel.Client = value; }
	    }
		public ulong Id => _guildChannel.Id;
		public ulong GuildID => _guildChannel.GuildID;
		public Guild Guild => _guildChannel.Guild;
		public string Name => _guildChannel.Name;
		public int Position => _guildChannel.Position;
		public Overwrite[] PermissionOverwrites => _guildChannel.PermissionOverwrites;
		public string Topic => _guildChannel.Topic;
		public ulong? LastMessageId => _guildChannel.LastMessageId;

		public TextChannel(GuildChannel guildChannel)
		{
			_guildChannel = guildChannel;
		}

		public async Task SendMessage(string message)
	    {
			await _guildChannel.Client.DiscordRestApiClient.CreateMessageAsync(_guildChannel.Id, new CreateMessage {
				Content = message
			});
	    }
	}
}
