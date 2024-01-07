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

            public ClientConnectionSettings(string ip, int port)
            {
                this.ip = ip;
                this.port = port;
            }
        }
    }
}