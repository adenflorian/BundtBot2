using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BundtBot.Discord.Gateway.Models
{
    public class EmbedThumbnail
	{
		/// <summary>
		/// Source url of thumbnail (only supports http(s) and attachments).
		/// </summary>
		[JsonProperty("url")]
		public string SourceUrl;

		[JsonProperty("proxy_icon_url")]
		public string ProxySourceUrl;

		[JsonProperty("height")]
		public int Height;

		[JsonProperty("width")]
		public int Width;
	}
}
