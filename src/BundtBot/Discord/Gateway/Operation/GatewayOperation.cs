using System.Threading.Tasks;

namespace BundtBot.Discord.Gateway.Operation {
	public interface IGatewayOperation {
		Task Execute(DiscordGatewayClient gatewayClient, string eventName, object eventData);
	}
}
