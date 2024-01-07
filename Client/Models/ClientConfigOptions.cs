namespace SiNet
{
    namespace Client
    {
        /// <summary>
        /// ClientConfigOptions is a class that contains the options for creating a client.
        /// </summary>
        public struct ClientConfigOptions
        {
            public string ip { get; private set;}
            public int port { get; private set; }

            public ClientConfigOptions(string ip, int port)
            {
                this.ip = ip;
                this.port = port;
            }
        }
    }
}