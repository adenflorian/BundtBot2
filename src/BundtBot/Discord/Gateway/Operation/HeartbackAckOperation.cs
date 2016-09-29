using System.Threading.Tasks;

namespace BundtBot.Discord.Gateway.Operation {
	public class HeartbackAckOperation : IGatewayOperation {
		public Task Execute(DiscordGatewayClient gatewayClient, string eventName, object eventData) {
			MyLogger.LogInfo("Gateway: HeartbackAck Received ← ♥");
			return Task.CompletedTask;
		}
	}
}
