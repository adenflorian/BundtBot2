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
		public void Start()
		{
			var host = new WebHostBuilder()
				.UseKestrel()
				.UseStartup<Startup>()
				.Build();

			Task.Run(() => host.Run());
		}
	}

	public class Startup
	{
		public void Configure(IApplicationBuilder app)
		{
			app.Run(async (context) => {
				var template = Handlebars.Compile(File.ReadAllText("WebServer/Templates/main.html"));

				var data = new {
					title = Program.Name,
					serverTime = DateTime.Now.ToString("HH:mm:ss tt zz"),
					assemblyVersion = Assembly.GetEntryAssembly().GetName().Version,
					guilds = Program.BundtBot.Client.Guilds
				};

				await context.Response.WriteAsync(template(data));
			});
		}
	}
}
