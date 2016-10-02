namespace BundtBot.Discord.Gateway.Operation {
	public class HeartbackAckOperation : IGatewayCommand {
		public static readonly HeartbackAckOperation Instance = new HeartbackAckOperation();

		public void Execute(string eventName, object eventData) {
			MyLogger.LogInfo("Gateway: HeartbackAck Received ← ♥");
		}
	}
}
