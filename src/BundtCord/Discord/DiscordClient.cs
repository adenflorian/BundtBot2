using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BundtBot.Discord.Gateway;
using BundtBot.Discord.Gateway.Operation;
using BundtBot.Discord.Models;
using BundtBot.Discord.Models.Events;
using BundtBot.Discord.Models.Gateway;
using Newtonsoft.Json;

namespace BundtBot.Discord
{
    public class DiscordClient
	{
		public const string Name = "bundtbot";
		const string Version = "0.0.1";
		const string BotToken = "MjA5NDU2NjYyOTI1NDEwMzA1.CsjHmg.pyJbVPWaP4Pkdv8zQ55qLFUxFdM";

		public delegate void GuildCreatedHandler(Guild guild);
		/// <summary>
		/// This event can be sent in three different scenarios:
		///   1. When a user is initially connecting, to lazily load and backfill
		///      information for all unavailable guilds sent in the ready event.
		///   2. When a Guild becomes available again to the client.
		///   3. When the current user joins a new Guild.
		/// The inner payload is a guild object, with all the extra fields specified.
		/// </summary>
		public event GuildCreatedHandler GuildCreated;
		public delegate void MessageCreatedHandler(Message message);
		public event MessageCreatedHandler MessageCreated;
		public delegate void ReadyHandler(Ready readyInfo);
		public event ReadyHandler Ready;
		public delegate void TextChannelCreatedHandler(TextChannel newTextChannel);
		public event TextChannelCreatedHandler TextChannelCreated;
		public delegate void VoiceChannelCreatedHandler(VoiceChannel newVoiceChannel);
		public event VoiceChannelCreatedHandler VoiceChannelCreated;
		public delegate void DmChannelCreatedHandler(DmChannel newDmChannel);
		public event DmChannelCreatedHandler DmChannelCreated;

		public Dictionary<ulong, Guild> Guilds = new Dictionary<ulong, Guild>();
		public Dictionary<ulong, TextChannel> TextChannels = new Dictionary<ulong, TextChannel>();
		public Dictionary<ulong, VoiceChannel> VoiceChannels = new Dictionary<ulong, VoiceChannel>();
		public Dictionary<ulong, DmChannel> DmChannels = new Dictionary<ulong, DmChannel>();

		internal readonly DiscordRestClient DiscordRestApiClient;

		static readonly MyLogger _logger = new MyLogger(nameof(DiscordClient));

		readonly DiscordGatewayClient _gatewayClient;

		public DiscordClient()
		{
			DiscordRestApiClient = new DiscordRestClient(BotToken, Name, Version);
			_gatewayClient = new DiscordGatewayClient(BotToken);
		}

		public async Task Connect()
	    {
			var gatewayUrl = await DiscordRestApiClient.GetGatewayUrlAsync();

			_gatewayClient.HeartbackAckReceived += HeartbackAckOperation.Instance.Execute;
			_gatewayClient.DispatchReceived += (eventName, eventJsonData) => {
				_logger.LogInfo("Processing Gateway Event " + eventName);
				switch (eventName) {
					case "CHANNEL_CREATE":
						var channel = JsonConvert.DeserializeObject<Channel>(eventJsonData);
						_logger.LogInfo("Received Event: CHANNEL_CREATE " + channel.Id, ConsoleColor.Green);
						OnChannelCreated(channel, eventJsonData);
						break;
					case "MESSAGE_CREATE":
						var message = JsonConvert.DeserializeObject<Message>(eventJsonData);
						_logger.LogInfo("Received Event: MESSAGE_CREATE " + message.Content, ConsoleColor.Green);
						OnMessageCreated(message);
						break;
					case "GUILD_CREATE":
						var guild = JsonConvert.DeserializeObject<Guild>(eventJsonData);
						_logger.LogInfo("Received Event: GUILD_CREATE " + guild.Name, ConsoleColor.Green);
						OnGuildCreated(guild);
						break;
					case "READY":
						var ready = JsonConvert.DeserializeObject<Ready>(eventJsonData);
						_logger.LogInfo("Received Event: READY Our username is " + ready.User.Username, ConsoleColor.Green);
						Ready?.Invoke(ready);
						break;
					case "TYPING_START":
						var typingStart = JsonConvert.DeserializeObject<TypingStart>(eventJsonData);
						_logger.LogInfo("Received Event: TYPING_START " + typingStart.UserId, ConsoleColor.Green);
						break;
					default:
						var ex = new ArgumentOutOfRangeException(nameof(eventName), eventName, "Unexpected Event Name");
						_logger.LogError(ex);
						throw ex;
				}
			};

			await _gatewayClient.ConnectAsync(gatewayUrl);
		}

		public async void SetGame(string gameName)
		{
			await _gatewayClient.SendStatusUpdate(new StatusUpdate(null, gameName));
		}

		void OnChannelCreated(Channel channel, string eventJsonData)
		{
			if (channel.IsPrivate) {
				var dmChannel = JsonConvert.DeserializeObject<DmChannel>(eventJsonData);
				dmChannel.Client = this;
				DmChannels[dmChannel.Id] = dmChannel;
				DmChannelCreated?.Invoke(dmChannel);
			}

			var guildChannel = JsonConvert.DeserializeObject<GuildChannel>(eventJsonData);
			guildChannel.Client = this;

			if (guildChannel.Type == GuildChannelType.Text) {
				var textChannel = new TextChannel(guildChannel);
				TextChannels[textChannel.Id] = textChannel;
				TextChannelCreated?.Invoke(textChannel);
			} else {
				var voiceChannel = new VoiceChannel(guildChannel);
				VoiceChannels[voiceChannel.Id] = voiceChannel;
				VoiceChannelCreated?.Invoke(new VoiceChannel(guildChannel));
			}
		}

		void OnMessageCreated(Message message)
		{
			message.Client = this;
			MessageCreated?.Invoke(message);
		}

		void OnGuildCreated(Guild guild)
		{
			guild.Client = this;
			guild.AllChannels.ForEach(x => x.Client = this);
			guild.Members.ForEach(x => x.Client = this);

			Guilds[guild.Id] = guild;
			guild.TextChannels.ForEach(x => TextChannels[x.Id] = x);
			guild.VoiceChannels.ForEach(x => VoiceChannels[x.Id] = x);

			GuildCreated?.Invoke(guild);
		}
    }
}
