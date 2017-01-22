using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BundtBot.Discord.Gateway;
using BundtBot.Discord.Models;
using BundtBot.Discord.Models.Gateway;

namespace BundtBot.Discord
{
    public class DiscordClient
	{
		public const string Name = "bundtbot";
		const string Version = "0.0.1";
		readonly string BotToken;

		public IUser Me { get; internal set; }

		public delegate void MessageCreatedHandler(IMessage message);
		public event MessageCreatedHandler MessageCreated;

		internal readonly DiscordRestClient DiscordRestApiClient;

		static readonly MyLogger _logger = new MyLogger(nameof(DiscordClient));

		readonly DiscordGatewayClient _gatewayClient;

		internal Dictionary<ulong, ITextChannel> TextChannels = new Dictionary<ulong, ITextChannel>();
		internal Dictionary<ulong, IUser> Users = new Dictionary<ulong, IUser>();

		public DiscordClient(string botToken)
		{
			BotToken = botToken;
			DiscordRestApiClient = new DiscordRestClient(BotToken, Name, Version);
			_gatewayClient = new DiscordGatewayClient(BotToken);
		}

		public async Task Connect()
	    {
			var gatewayUrl = await DiscordRestApiClient.GetGatewayUrlAsync();

			await _gatewayClient.ConnectAsync(gatewayUrl);

			_gatewayClient.GuildCreated += (discordGuild) => {
				discordGuild.AllChannels
					.Where(x => x.Type == GuildChannelType.Text)
					.Select(x => new TextChannel(x, this))
					.ToList().ForEach(x => {TextChannels[x.Id] = x;});
				discordGuild.Members
					.Select(x => new User(x.User))
					.ToList().ForEach(x => {Users[x.Id] = x;});
			};

			_gatewayClient.MessageCreated += (discordMessage) => {
				var textChannel = TextChannels[discordMessage.ChannelId];
				var message = new Message(discordMessage, this);
				MessageCreated?.Invoke(message);
			};

			_gatewayClient.Ready += (readyInfo) => {
				Me = new User(readyInfo.User);
			};
		}

		public async void SetGame(string gameName)
		{
			await _gatewayClient.SendStatusUpdate(new StatusUpdate(null, gameName));
		}
    }
}
