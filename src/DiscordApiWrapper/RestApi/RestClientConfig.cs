using System;

namespace DiscordApiWrapper.RestApi
{
    public class RestClientConfig
    {
        public string BotToken;
        public string Name;
        public string Version;
        public Uri BaseAddress;

        public RestClientConfig()
        {
        }

        public RestClientConfig(string botToken, string name, string version, Uri baseAddress)
        {
            BotToken = botToken;
            Name = name;
            Version = version;
            BaseAddress = baseAddress;
        }
    }
}