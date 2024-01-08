namespace SiNet
{
    namespace Server
    {
        /// <summary>
        /// ServerConnectionSettings is a class that contains the options for creating a server.
        /// </summary>
        public struct ServerConnectionSettings
        {
            public string ip { get; private set; }
            public int port { get; private set; }
            public int maxClients { get; private set; }

            public ServerConnectionSettings(string ip, int port, int maxClients = ConfigDefaults.MAX_CLIENTS)
            {
                this.ip = ip;
                this.port = port;
                this.maxClients = maxClients;
            }
        }
    }
}