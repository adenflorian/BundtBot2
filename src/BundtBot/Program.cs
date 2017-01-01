using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BundtBot
{
	public class Program
	{
		// TODO https://docs.asp.net/en/latest/fundamentals/configuration.html
		public const string Name = "bundtbot";
		public static BundtBot BundtBot;

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

			BundtBot = new BundtBot();

			await BundtBot.Start();
		}
	}
}
