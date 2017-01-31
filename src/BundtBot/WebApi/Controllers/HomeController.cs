using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace BundtBot.WebApi.Controllers
{
    public class HomeController : Controller
	{
		static readonly MyLogger _logger = new MyLogger(nameof(HomeController), ConsoleColor.DarkBlue);
		
		public IActionResult Index()
		{
			try {
				_logger.LogInfo("Received request to web server");

				ViewData["Title"] = Program.Name;
				ViewData["ServerTime"] = DateTime.Now.ToString("HH:mm:ss zz");
				ViewData["AssemblyVersion"] = Assembly.GetEntryAssembly().GetName().Name + " " + Assembly.GetEntryAssembly().GetName().Version;

				return View();
			} catch (Exception ex) {
				_logger.LogError(ex);
				throw;
			}
		}
	}
}
