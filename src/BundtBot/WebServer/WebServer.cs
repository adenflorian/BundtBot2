using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace BundtBot
{
	public class WebServer
	{
		static readonly MyLogger _logger = new MyLogger(nameof(WebServer));

		public void Start()
		{
			var host = new WebHostBuilder()
				.UseKestrel()
				.UseStartup<Startup>()
				.Build();

			Task.Run(() => {
				try {
					host.Run();
				} catch (Exception ex) {
					_logger.LogError(ex);
					throw;
				}
			});
		}
	}

	public class Startup
	{
		static readonly MyLogger _logger = new MyLogger(nameof(Startup));

		public void Configure(IApplicationBuilder app)
		{
			app.Run(async (context) => {
				try {
					_logger.LogInfo("Received request to web server from " + context.Request.Host);

					var template = Handlebars.Compile(File.ReadAllText("WebServer/Templates/main.html"));

					var data = new {
						title = Program.Name,
						serverTime = DateTime.Now.ToString("HH:mm:ss tt zz"),
						assemblyVersion = Assembly.GetEntryAssembly().GetName().Name + " " + Assembly.GetEntryAssembly().GetName().Version,
						guilds = Program.BundtBot.Client.Guilds
					};

					var output = template(data);

					await context.Response.WriteAsync(output);
				} catch (Exception ex) {
					_logger.LogError(ex);
					throw;
				}
			});
		}
	}
}
