using System;
using Newtonsoft.Json;

namespace DiscordApiWrapper.Models
{
    /// <summary>NEVER cache this info??or do?</summary>
    public class VoiceServerInfo
    {
        [JsonProperty("token")]
        public string Token;

        [JsonProperty("guild_id")]
        public ulong GuildID;

        Uri _endpoint;
        [JsonProperty("endpoint")]
        public Uri Endpoint
        {
            get
            {
                return _endpoint;
            }
            set
            {
                var uriString = value.ToString();

                if (uriString.Contains(":"))
                {
                    var split = uriString.Split(':');
                    var uriStringWithoutPort = split[0];
                    var port = split[1];
                    
                    if (port == "80")
                    {
                        _endpoint = new Uri("wss://" + uriStringWithoutPort);
                    }
                    else
                    {
                        _endpoint = new Uri("wss://" + uriString);
                    }
                }
                else
                {
                    _endpoint = new Uri("wss://" + uriString);
                }
            }
        }
    }
}