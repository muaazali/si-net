namespace SiNet
{
    namespace Server
    {
        /// <summary>
        /// ServerConnectionSettings is a class that contains the options for creating a server.
        /// </summary>
        public struct ServerConnectionSettings
        {
            public int port { get; private set; }
            public int maxClients { get; private set; }
            public int maxPacketSize { get; private set; }

            public ServerConnectionSettings(int port = 5000, int maxClients = 8, int maxPacketSize = 4096)
            {
                this.port = port;
                this.maxClients = maxClients;
                this.maxPacketSize = maxPacketSize;
            }
        }
    }
}