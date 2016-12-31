using System;
using Newtonsoft.Json;

namespace BundtBot.Discord.Gateway.Models
{
    public class Embed
    {
		[JsonProperty("title")]
		public string Title;

		/// <summary>
		/// Type of embed (always "rich" for webhook embeds).
		/// </summary>
		[JsonProperty("type")]
		public string Type;
		
		[JsonProperty("description")]
		public string Description;
		
		[JsonProperty("url")]
		public string Url;
		
		[JsonProperty("timestamp")]
		public DateTime Timestamp;

		/// <summary>
		/// Color code of the embed.
		/// </summary>
		[JsonProperty("color")]
		public int Color;
		
		[JsonProperty("footer")]
		public EmbedFooter Footer;
		
		[JsonProperty("image")]
		public EmbedImage Image;
		
		[JsonProperty("thumbnail")]
		public EmbedThumbnail Thumbnail;
		
		[JsonProperty("video")]
		public EmbedVideo Video;
		
		[JsonProperty("provider")]
		public EmbedProvider Provider;
		
		[JsonProperty("author")]
		public EmbedAuthor Author;
		
		[JsonProperty("fields")]
		public EmbedField[] Fields;
	}
}
