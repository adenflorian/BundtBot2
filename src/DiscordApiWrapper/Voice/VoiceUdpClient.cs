using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace DiscordApiWrapper.Voice
{
    class VoiceUdpClient
    {
        public VoiceUdpClient(Uri remoteUri, int remotePort)
        {
            var task = Dns.GetHostAddressesAsync(remoteUri.Host);
            task.Wait();
            Debug.Assert(task.Result.Length > 0);
            IPAddress hostIPAddress1 = task.Result[0];
            var x = new UdpClient();
            x.SendAsync(null, 0, new IPEndPoint(hostIPAddress1, remotePort));
        }

        void SendIpDiscoveryPacket()
        {
            
        }
    }
}