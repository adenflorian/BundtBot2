using System.Threading.Tasks;
using BundtBot.Discord.Gateway.Models;
using Newtonsoft.Json;

namespace BundtBot.Discord.Gateway.Operation {
	public class HelloOperation : IGatewayOperation {
		public async Task Execute(DiscordGatewayClient gatewayClient, string eventName, object eventData) {
			var hello = JsonConvert.DeserializeObject<GatewayHello>(eventData.ToString());
			await gatewayClient.SendHeartBeat();
			gatewayClient.StartHeartBeat(hello.HeartbeatInterval);
			await gatewayClient.SendGatewayIdentify();
		}
	}
}
