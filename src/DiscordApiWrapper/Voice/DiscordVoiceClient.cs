using BundtBot.Discord.Gateway;

namespace DiscordApiWrapper.Voice
{
    public class DiscordVoiceClient
    {
        public void ConnectAsync(DiscordGatewayClient gatewayClient)
        {
            gatewayClient.VoiceStateUpdate += (voiceState) =>
            {

            };

            // gatewayClient.VoiceServerUpdate += (voiceServerInfo) =>
            // {

            // };

            //await gatewayClient.SendVoiceStateUpdateAsync();
        }
    }
}