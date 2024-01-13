using System.Net.Sockets;

namespace SiNet
{
    namespace Server
    {
        public class ConnectedClientInfo
        {
            public string ip { get; private set; }
            public int port { get; private set; }
            public Socket socket { get; private set; }
            public string clientId { get; private set; }
            public byte[] buffer;

            public ConnectedClientInfo(string ip, int port, Socket socket, string id, int maxPacketSize)
            {
                this.ip = ip;
                this.port = port;
                this.socket = socket;
                this.clientId = id;
                buffer = new byte[maxPacketSize];
            }
        }

    }
}