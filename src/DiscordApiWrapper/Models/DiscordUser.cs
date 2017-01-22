using Newtonsoft.Json;

namespace BundtBot.Discord.Models
{
    public class DiscordUser
    {
		/// <summary>
		/// Required OAuth2 Scope: identify.
		/// </summary>
		[JsonProperty("id")]
		public ulong Id;

		/// <summary>
		/// The user's username, not unique across the platform.
		/// Required OAuth2 Scope: identify.
		/// </summary>
		[JsonProperty("username")]
		public string Username;

		/// <summary>
		/// The user's 4-digit discord-tag.
		/// Required OAuth2 Scope: identify.
		/// </summary>
		[JsonProperty("discriminator")]
		public string Discriminator;

		/// <summary>
		/// Required OAuth2 Scope: identify.
		/// </summary>
		[JsonProperty("avatar")]
		public string AvatarHash;

		/// <summary>
		/// Whether the user belongs to an OAuth2 application.
		/// Required OAuth2 Scope: identify.
		/// </summary>
		[JsonProperty("bot")]
		public bool IsBot;

		/// <summary>
		/// Required OAuth2 Scope: identify.
		/// </summary>
		[JsonProperty("mfa_enabled")]
		public bool IsTwoFactorAuthEnabled;

		/// <summary>
		/// Required OAuth2 Scope: email.
		/// </summary>
		[JsonProperty("verified")]
		public bool HasVerifiedEmail;

		/// <summary>
		/// Required OAuth2 Scope: email.
		/// </summary>
		[JsonProperty("email")]
		public string Email;
	}
}
