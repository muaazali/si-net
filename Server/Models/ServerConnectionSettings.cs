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
            public bool shouldPingClients { get; private set; }
            public float pingInterval { get; private set; }

            public ServerConnectionSettings(string ip, int port, int maxClients = ConfigDefaults.MAX_CLIENTS, bool shouldPingClients = ConfigDefaults.SHOULD_PING_CLIENTS, float pingInterval = ConfigDefaults.PING_INTERVAL)
            {
                this.ip = ip;
                this.port = port;
                this.maxClients = maxClients;
                this.shouldPingClients = shouldPingClients;
                this.pingInterval = pingInterval;
            }
        }
    }
}