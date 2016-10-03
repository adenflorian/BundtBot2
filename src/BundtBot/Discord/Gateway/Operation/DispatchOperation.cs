using System;
using BundtBot.Discord.Gateway.Models;
using Newtonsoft.Json;

namespace BundtBot.Discord.Gateway.Operation {
	public class DispatchOperation : IGatewayCommand {
		public static readonly DispatchOperation Instance = new DispatchOperation();

		public void Execute(string eventName, object eventData) {
			ProcessEvent(eventName, eventData.ToString());
		}

		static void ProcessEvent(string eventName, string eventJsonData) {
			MyLogger.LogInfo("Gateway: Processing Gateway Event " + eventName);
			switch (eventName) {
				case "GUILD_CREATE":
					var guild = JsonConvert.DeserializeObject<Guild>(eventJsonData);
					MyLogger.LogInfo("Gateway: Received Event: GUILD_CREATE " + guild.Name);
					break;
				case "READY":
					var ready = JsonConvert.DeserializeObject<Ready>(eventJsonData);
					MyLogger.LogInfo("Gateway: Received Event: READY " + ready.SessionId, ConsoleColor.Green);
					break;
				default:
					var ex = new ArgumentOutOfRangeException(nameof(eventName), eventName, "Unexpected Event Name");
					MyLogger.LogError(ex);
					throw ex;
			}
		}
	}
}
