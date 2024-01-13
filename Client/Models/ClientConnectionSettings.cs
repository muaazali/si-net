namespace SiNet
{
    namespace Client
    {
        /// <summary>
        /// ClientConnectionSettings is a class that contains the options for creating a client.
        /// </summary>
        public struct ClientConnectionSettings
        {
            public string ip { get; private set;}
            public int port { get; private set; }
            public int maxPacketSize { get; private set; }

            public ClientConnectionSettings(string ip, int port, int maxPacketSize = 4096)
            {
                this.ip = ip;
                this.port = port;
                this.maxPacketSize = maxPacketSize;
            }
        }
    }
}