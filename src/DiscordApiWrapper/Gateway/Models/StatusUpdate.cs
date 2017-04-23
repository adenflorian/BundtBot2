using System;
using Newtonsoft.Json;

namespace BundtBot.Discord.Models.Gateway
{
    public class StatusUpdate
    {
		/// <summary>
		/// Unix time (in milliseconds) of when the client went idle,
		/// or null if the client is not idle.
		/// </summary>
		[JsonProperty("idle_since")]
		public string IdleSince;

		/// <summary>
		/// Either null, or an object with one key "name",
		/// representing the name of the game being played.
		/// </summary>
		[JsonProperty("game")]
		public Game Game;

	    public StatusUpdate(DateTime? idleSince, string currentlyPlaying)
	    {
		    IdleSince = idleSince.HasValue ? ((int)idleSince.Value.Ticks).ToString() : null;
			Game = new Game{Name = currentlyPlaying};
	    }
	}
}
