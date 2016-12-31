using Newtonsoft.Json;

namespace BundtBot.Discord.Models
{
	public enum OverwriteType
	{
		Role,
		Member
	}

    public class Overwrite
	{
		/// <summary>
		/// Role or user id.
		/// </summary>
		[JsonProperty("id")]
		public ulong Id;
		
		[JsonProperty("type")]
		public string Type;

		/// <summary>
		/// Permission bit set.
		/// </summary>
		[JsonProperty("allow")]
		public int Allow;

		/// <summary>
		/// Permission bit set.
		/// </summary>
		[JsonProperty("deny")]
		public int Deny;
	}
}
