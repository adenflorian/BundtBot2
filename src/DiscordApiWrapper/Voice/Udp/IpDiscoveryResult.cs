namespace DiscordApiWrapper.Voice.Udp
{
    public struct IpDiscoveryResult
    {
        readonly public string IpAddress;
        readonly public int Port;

        public IpDiscoveryResult(string ipAddress, int port)
        {
            IpAddress = ipAddress;
            Port = port;
        }
    }
}