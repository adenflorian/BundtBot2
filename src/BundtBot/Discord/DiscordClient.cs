using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BundtBot.Discord.Gateway;
using BundtBot.Discord.Gateway.Models;
using BundtBot.Discord.Gateway.Models.Events;
using BundtBot.Discord.Gateway.Operation;
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
						_logger.LogInfo("Received Event: MESSAGE_CREATE " + message.Content);
						MessageCreated?.Invoke(message);
						break;
					case "GUILD_CREATE":
						var guild = JsonConvert.DeserializeObject<Guild>(eventJsonData);
						_logger.LogInfo("Received Event: GUILD_CREATE " + guild.Name);
						Guilds.Add(guild);
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
