using System;
using Newtonsoft.Json;

namespace BundtBot.Discord
{
	[JsonObject]
	public class GatewayUrl
	{
		[JsonRequired]
		public Uri Url;
	}
}
