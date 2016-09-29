using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BundtBot.Discord;
using BundtBot.Discord.Gateway;
using BundtBot.Discord.Gateway.Models;
using BundtBot.Discord.Gateway.Operation;
using Newtonsoft.Json;

namespace BundtBot {
	public class Program {
		static readonly string _version = "0.0.1";
		static readonly string _name = "bundtbot";
		static readonly string _botToken = "";

		static DiscordGatewayClient _gatewayClient;

		public static void Main(string[] args) {
			SetupConsole();

			Run().Wait();

			var x = 0;
			while (true) {
				Thread.Sleep(TimeSpan.FromSeconds(10));
				MyLogger.LogInfo("test " + x++);
			}
		}

		static void SetupConsole() {
			Console.OutputEncoding = Encoding.UTF8;
			if (Console.LargestWindowHeight > 0) {
				Console.WindowHeight = (int) (Console.LargestWindowHeight * 0.75);
			}
		}

		static async Task Run() {
			var discordRestApiClient = new DiscordRestApiHttpClient(_botToken, _name, _version);

			var gatewayUrl = discordRestApiClient.GetGatewayUrl();

			_gatewayClient = new DiscordGatewayClient(_botToken);

			_gatewayClient.MessageReceived += OnMessageReceived;

			await _gatewayClient.Connect(gatewayUrl);
			_gatewayClient.StartReceiveLoop();
		}

		static async void OnMessageReceived(GatewayPayload gatewayPayload) {
			var operation = GatewayOperationFactory.Create(gatewayPayload.GatewayOpCode);
			await operation.Execute(_gatewayClient, gatewayPayload.EventName, gatewayPayload.EventData);
		}
	}
}
