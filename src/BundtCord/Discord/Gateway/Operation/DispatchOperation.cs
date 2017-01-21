using System;
using BundtBot.Discord.Models;
using BundtBot.Discord.Models.Events;
using Newtonsoft.Json;

namespace BundtBot.Discord.Gateway.Operation
{
	public class DispatchOperation : IGatewayCommand
	{
		public static readonly DispatchOperation Instance = new DispatchOperation();

		static readonly MyLogger _logger = new MyLogger(nameof(DispatchOperation));

		public void Execute(string eventName, object eventData)
		{
			ProcessEvent(eventName, eventData.ToString());
		}

		static void ProcessEvent(string eventName, string eventJsonData)
		{
			_logger.LogInfo("Processing Gateway Event " + eventName);

			switch (eventName) {
				case "MESSAGE_CREATE":
					var message = JsonConvert.DeserializeObject<Message>(eventJsonData);
					_logger.LogInfo("Received Event: MESSAGE_CREATE " + message.Content);
					break;
				case "GUILD_CREATE":
					var guild = JsonConvert.DeserializeObject<Guild>(eventJsonData);
					_logger.LogInfo("Received Event: GUILD_CREATE " + guild.Name);
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
		}
	}
}
