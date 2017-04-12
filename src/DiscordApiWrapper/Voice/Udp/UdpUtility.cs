using System;
using System.Net;

namespace DiscordApiWrapper.Voice.Udp
{
    internal class UdpUtility
    {
        public static IPEndPoint GetEndpointFromInfo(Uri remoteUri, int remotePort)
        {
            var task = Dns.GetHostAddressesAsync(remoteUri.Host);
            task.Wait();
            var voiceUdpServerAddress = task.Result[0];

            return new IPEndPoint(voiceUdpServerAddress, remotePort);
        }

        public static Tuple<string, int> GetIpAddressAndPortFromIpDiscoveryResponse(byte[] ipDiscoveryResponse)
        {
            var ipString = "";
            var i = 4;
            while (true)
            {
                if (ipDiscoveryResponse[i] == 0x00) break;
                ipString += (char)ipDiscoveryResponse[i];
                i++;
            }

            var port = 0;

            if (BitConverter.IsLittleEndian)
            {
                var portBytesLittleEndian = new byte[] { ipDiscoveryResponse[ipDiscoveryResponse.Length - 2], ipDiscoveryResponse[ipDiscoveryResponse.Length - 1] };
                port = BitConverter.ToUInt16(portBytesLittleEndian, 0);
            }
            else
            {
                var portBytesBigEndian = new byte[] { ipDiscoveryResponse[ipDiscoveryResponse.Length - 1], ipDiscoveryResponse[ipDiscoveryResponse.Length - 2] };
                port = BitConverter.ToUInt16(portBytesBigEndian, 0);
            }

            return Tuple.Create(ipString, port);
        }
    }
}