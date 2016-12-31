using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace BundtBot
{
	public class WebServer
	{
		public async Task Start()
		{
			var host = new WebHostBuilder()
				.UseKestrel()
				.UseStartup<Startup>()
				.Build();

			await Task.Run(() => host.Run());
		}
	}

	public class Startup
	{
		public void Configure(IApplicationBuilder app)
		{
			app.Run(async (context) => {
				await context.Response.WriteAsync(
					"Hello World. The Time is: " +
					DateTime.Now.ToString("hh:mm:ss tt") +
					"\n\n Name: " + Program.Name +
					"\n\n Assembly Version: " + Assembly.GetEntryAssembly().GetName().Version);
			});
		}
	}
}
