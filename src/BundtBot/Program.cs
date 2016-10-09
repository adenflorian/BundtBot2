using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BundtBot.Discord;
using BundtBot.Discord.Gateway;
using BundtBot.Discord.Gateway.Operation;

namespace BundtBot
{
	public class Program
	{
		// TODO https://docs.asp.net/en/latest/fundamentals/configuration.html
		const string Version = "0.0.1";
		const string Name = "bundtbot";
		const string BotToken = "MjA5NDU2NjYyOTI1NDEwMzA1.CsjHmg.pyJbVPWaP4Pkdv8zQ55qLFUxFdM";

		static DiscordGatewayClient _gatewayClient;

		public static void Main(string[] args)
		{
			SetupConsole();

			Run().Wait();

			var x = 0;
			var myLogger = new MyLogger(nameof(Program));
			while (true) {
				Thread.Sleep(TimeSpan.FromSeconds(10));
				myLogger.LogInfo("test " + x++);
			}
		}

		static void SetupConsole()
		{
			Console.OutputEncoding = Encoding.UTF8;
			if (Console.LargestWindowHeight > 0) {
				Console.WindowHeight = (int)(Console.LargestWindowHeight * 0.75);
			}
		}

		static async Task Run()
		{
			var discordRestApiClient = new DiscordRestClient(BotToken, Name, Version);

			var gatewayUrl = discordRestApiClient.GetGatewayUrl();

			_gatewayClient = new DiscordGatewayClient(BotToken);

			_gatewayClient.DispatchReceived += DispatchOperation.Instance.Execute;
			_gatewayClient.HeartbackAckReceived += HeartbackAckOperation.Instance.Execute;

			await _gatewayClient.ConnectAsync(gatewayUrl);
			_gatewayClient.StartReceiveLoop();
		}
	}
}
