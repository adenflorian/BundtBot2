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
				try {
					host.Run();
				} catch (Exception ex) {
					_logger.LogWarning("WebServer threw an exception...");
					_logger.LogError(ex);
				} finally {
					_logger.LogWarning("WebServer exiting...");
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
					.AddMyWebServerLogger();
				app.UseMvcWithDefaultRoute();
			} catch (Exception ex) {
				_logger.LogError(ex);
				throw;
			}
		}
	}
}
