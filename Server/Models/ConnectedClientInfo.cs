using System.Net.Sockets;

namespace SiNet
{
    namespace Server
    {
        public struct ConnectedClientInfo
        {
            public string ip { get; private set; }
            public int port { get; private set; }
            public Socket socket { get; private set; }
            public string clientId { get; private set; }
            public byte[] buffer;
            public bool isBeingListenedTo;

            public ConnectedClientInfo(string ip, int port, Socket socket, string id)
            {
                this.ip = ip;
                this.port = port;
                this.socket = socket;
                this.clientId = id;
                buffer = new byte[ConfigDefaults.MAX_PACKET_SIZE];
                isBeingListenedTo = false;
            }
        }

    }
}