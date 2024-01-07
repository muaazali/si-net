namespace SiNet
{
    namespace Server
    {
        /// <summary>
        /// ServerConfigOptions is a class that contains the options for creating a server.
        /// </summary>
        public struct ServerConfigOptions
        {
            public string ip { get; private set; }
            public int port { get; private set; }

            public ServerConfigOptions(string ip, int port)
            {
                this.ip = ip;
                this.port = port;
            }
        }
    }
}