using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BundtBot.Discord;
using BundtBot.Discord.Gateway.Models;

namespace BundtBot
{
	public class Program
	{
		// TODO https://docs.asp.net/en/latest/fundamentals/configuration.html
		public const string Name = "bundtbot";

		public static void Main(string[] args)
		{
			SetupConsole();

			Task.Run(async () => {
				await Start();
			}).Wait();

			while (true) {
				Thread.Sleep(TimeSpan.FromMilliseconds(100));
			}
		}

		static void SetupConsole()
		{
			Console.OutputEncoding = Encoding.UTF8;
			if (Console.LargestWindowHeight > 0) {
				Console.WindowHeight = (int)(Console.LargestWindowHeight * 0.75);
			}
		}

		public static async Task Start()
		{
			new WebServer().Start();

			var discordClient = new DiscordClient();

			await discordClient.Connect();

			discordClient.GuildCreated += async (guild) => {
				await guild.Channels[0].SendMessage(discordClient, "yo");
			};

			discordClient.MessageCreated += async (message) => {
				if (message.Author.IsBot == false) {
					await discordClient.DiscordRestApiClient.CreateMessageAsync(message.ChannelId, new CreateMessage { Content = "yoyo" });
				}
			};
		}
	}
}
