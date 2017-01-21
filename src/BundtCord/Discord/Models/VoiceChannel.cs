namespace BundtBot.Discord.Models
{
    public class VoiceChannel
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
		public int? Bitrate => _guildChannel.Bitrate;
		public int? UserLimit => _guildChannel.UserLimit;

		public VoiceChannel(GuildChannel guildChannel)
		{
			_guildChannel = guildChannel;
		}
	}
}
