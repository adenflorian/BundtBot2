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

			Start();

			var notCanceled = true;
			Console.CancelKeyPress += (s, e) => notCanceled = false;

			while (notCanceled) Thread.Sleep(TimeSpan.FromMilliseconds(200));

			_logger.LogInfo("Goodbye!");
		}

		static void SetupConsole()
		{
			Console.OutputEncoding = Encoding.UTF8;
			if (Console.LargestWindowHeight > 0) {
				Console.WindowHeight = (int)(Console.LargestWindowHeight * 0.75);
			}
		}

		static void Start()
		{
			_logger.LogInfo("Current working directory: " + Directory.GetCurrentDirectory());

			try {
				StartAsync().Wait();
			} catch (Exception ex) {
				_logger.LogError(ex);
				throw;
			}
		}

		static async Task StartAsync()
		{
			new WebServer().Start();
			await new BundtBot().Start();
		}
	}
}
