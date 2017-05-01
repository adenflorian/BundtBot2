using Newtonsoft.Json;

namespace DiscordApiWrapper.Models
{
    public class DiscordRole
    {
        [JsonProperty("id")]
        public ulong Id;

        [JsonProperty("name")]
        public string Name;

        /// <summary>
        /// integer representation of hexadecimal color code
        /// </summary>
        [JsonProperty("color")]
        public int Color;

        /// <summary>
        /// if this role is pinned in the user listing
        /// </summary>
        [JsonProperty("hoist")]
        public bool	PinnedToUserListing;

        /// <summary>
        /// position of this role
        /// </summary>
        [JsonProperty("position")]
        public int Position;

        /// <summary>
        /// permission bit set
        /// https://discordapp.com/developers/docs/topics/permissions#role-object
        /// </summary>
        [JsonProperty("permissions")]
        public int Permissions;

        [JsonProperty("managed")]
        public bool IsManagedByIntegration;

        [JsonProperty("mentionable")]
        public bool IsMentionable;

    }
}