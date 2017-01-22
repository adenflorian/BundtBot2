using System;
using System.IO;
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
		readonly string BotToken;

		public delegate void GuildCreatedHandler(Guild guild);
		/// <summary>
		/// This event can be sent in three different scenarios:
		///   1. When a user is initially connecting, to lazily load and backfill
		///      information for all unavailable guilds sent in the ready event.
		///   2. When a Guild becomes available again to the client.
		///   3. When the current user joins a new Guild.
		/// The inner payload is a guild object, with all the extra fields specified.
		/// </summary>
		public delegate void ReadyHandler(Ready readyInfo);
		public event ReadyHandler Ready;

		internal readonly DiscordRestClient DiscordRestApiClient;

		static readonly MyLogger _logger = new MyLogger(nameof(DiscordClient));

		readonly DiscordGatewayClient _gatewayClient;

		public DiscordClient()
		{
			BotToken = File.ReadAllText("bottoken");
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
						break;
					case "MESSAGE_CREATE":
						var message = JsonConvert.DeserializeObject<Message>(eventJsonData);
						_logger.LogInfo("Received Event: MESSAGE_CREATE " + message.Content, ConsoleColor.Green);
						break;
					case "GUILD_CREATE":
						var guild = JsonConvert.DeserializeObject<Guild>(eventJsonData);
						_logger.LogInfo("Received Event: GUILD_CREATE " + guild.Name, ConsoleColor.Green);
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
    }
}
