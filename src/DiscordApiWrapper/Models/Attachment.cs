using Newtonsoft.Json;

namespace BundtBot.Discord.Models
{
    public class Attachment
    {
		[JsonProperty("id")]
		public ulong Id;
		
		[JsonProperty("filename")]
		public string Filename;
		
		[JsonProperty("size")]
		public int SizeInBytes;
		
		[JsonProperty("url")]
		public string SourceUrlOfFile;
		
		[JsonProperty("proxy_url")]
		public string ProxyUrlOfFile;

		/// <summary>
		/// Height of file (if image).
		/// </summary>
		[JsonProperty("height")]
		public int? Height;

		/// <summary>
		/// Width of file (if image).
		/// </summary>
		[JsonProperty("width")]
		public int? Width;
    }
}
