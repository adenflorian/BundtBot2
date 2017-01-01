using System;
using System.Linq;
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

			var client = new DiscordClient();

			await client.Connect();

			client.GuildCreated += async (guild) => {
				await guild.Channels[0].SendMessage("yo");
			};

			client.MessageCreated += async (message) => {
				if (message.Author.IsBot == false) {
					await message.Channel.SendMessage("hiya");
				}
			};
		}
	}
}
