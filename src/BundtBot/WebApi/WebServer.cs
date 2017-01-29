using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BundtBot.WebApi
{
	public class WebServer
	{
		static readonly MyLogger _logger = new MyLogger(nameof(WebServer));

		public void Start()
		{
			var host = new WebHostBuilder()
				.UseKestrel()
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseStartup<Startup>()
				.Build();

			Task.Run(() => {
				while (true)
				{
					try {
						host.Run();
					} catch (Exception ex) {
						_logger.LogWarning("WebServer threw an exception...");
						_logger.LogError(ex);
						_logger.LogWarning("Restarting WebServer...");
					}
				}
			});
		}
	}

	public class Startup
	{
		static readonly MyLogger _logger = new MyLogger(nameof(Startup));

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc();
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			try {
				loggerFactory
					.AddConsole()
					.AddDebug();
				app.UseMvcWithDefaultRoute();
			} catch (Exception ex) {
				_logger.LogError(ex);
				throw;
			}
		}
	}
}
