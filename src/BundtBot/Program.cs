using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BundtBot.WebApi;

namespace BundtBot
{
	public class Program
	{
		// TODO https://docs.asp.net/en/latest/fundamentals/configuration.html
		public const string Name = "bundtbot";

		static readonly MyLogger _logger = new MyLogger(nameof(Program));

		public static void Main(string[] args)
		{
			SetupConsole();
			_logger.LogInfo("Current working directory: " + Directory.GetCurrentDirectory());

			try {
				Start().Wait();
			} catch (Exception ex) {
				_logger.LogError(ex);
				throw;
			}

			var exit = false;
			Console.CancelKeyPress += (s, e) => {
				exit = true;
			};

			while (exit == false)
			{
				// To not suck up CPU
				Thread.Sleep(TimeSpan.FromMilliseconds(200));
			}

			_logger.LogWarning("Exiting because of Cancel Key Press Event, goodbye!");

			// Giving WebServer a chancve to cleanup
			Thread.Sleep(TimeSpan.FromMilliseconds(200));
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

			await new BundtBot().Start();
		}
	}
}
