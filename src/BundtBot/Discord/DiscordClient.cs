using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BundtBot.Discord.Gateway;
using BundtBot.Discord.Gateway.Models;
using BundtBot.Discord.Gateway.Models.Events;
using BundtBot.Discord.Gateway.Operation;
using BundtBot.Discord.Models;
using Newtonsoft.Json;

namespace BundtBot.Discord
{
    public class DiscordClient
	{
		const string Version = "0.0.1";
		public const string Name = "bundtbot";
		const string BotToken = "MjA5NDU2NjYyOTI1NDEwMzA1.CsjHmg.pyJbVPWaP4Pkdv8zQ55qLFUxFdM";

		static readonly MyLogger _logger = new MyLogger(nameof(DiscordClient));


		public delegate void GuildCreatedHandler(Guild guild);
		public event GuildCreatedHandler GuildCreated;
		public delegate void MessageCreatedHandler(Message message);
		public event MessageCreatedHandler MessageCreated;

		readonly DiscordGatewayClient _gatewayClient;
		internal readonly DiscordRestClient DiscordRestApiClient;

		public List<Guild> Guilds = new List<Guild>();
		public Dictionary<ulong, Channel> Channels = new Dictionary<ulong, Channel>();

		public DiscordClient()
		{
			DiscordRestApiClient = new DiscordRestClient(BotToken, Name, Version);
			_gatewayClient = new DiscordGatewayClient(BotToken);
		}

		public async Task Connect()
	    {
			var gatewayUrl = await DiscordRestApiClient.GetGatewayUrlAsync();
			
			//_gatewayClient.DispatchReceived += DispatchOperation.Instance.Execute;
		    _gatewayClient.DispatchReceived += (eventName, eventJsonData) => {
				_logger.LogInfo("Processing Gateway Event " + eventName);

				switch (eventName) {
					case "MESSAGE_CREATE":
						var message = JsonConvert.DeserializeObject<Message>(eventJsonData);
						message.client = this;
						_logger.LogInfo("Received Event: MESSAGE_CREATE " + message.Content);
						MessageCreated?.Invoke(message);
						break;
					// This event can be sent in three different scenarios:
					//   1. When a user is initially connecting, to lazily load and backfill
					//      information for all unavailable guilds sent in the ready event.
					//   2. When a Guild becomes available again to the client.
					//   3. When the current user joins a new Guild.
					// The inner payload is a guild object, with all the extra fields specified.
					case "GUILD_CREATE":
						var guild = JsonConvert.DeserializeObject<Guild>(eventJsonData);
						_logger.LogInfo("Received Event: GUILD_CREATE " + guild.Name);
						Guilds.Add(guild);
						foreach (var channel in guild.Channels) {
							if (Channels.ContainsKey(channel.ID)) {
								Channels[channel.ID] = channel;
							} else {
								Channels.Add(channel.ID, channel);
							}
							channel.client = this;
						}
						GuildCreated?.Invoke(guild);
						break;
					case "READY":
						var ready = JsonConvert.DeserializeObject<Ready>(eventJsonData);
						_logger.LogInfo("Received Event: READY " + ready.SessionId, ConsoleColor.Green);
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
			_gatewayClient.HeartbackAckReceived += HeartbackAckOperation.Instance.Execute;

			await _gatewayClient.ConnectAsync(gatewayUrl);
		}
    }
}
