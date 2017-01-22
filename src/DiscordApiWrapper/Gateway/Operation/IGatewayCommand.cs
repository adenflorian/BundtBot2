namespace BundtBot.Discord.Gateway.Operation {
	interface IGatewayCommand {
		void Execute(string eventName, object eventData);
	}
}
