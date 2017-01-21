using System;
using Newtonsoft.Json;

namespace BundtBot.Discord.Models
{
	[JsonObject]
	public class GatewayUrl
	{
		[JsonRequired]
		public Uri Url;
	}
}
