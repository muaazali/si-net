using System.Net.Sockets;

namespace SiNet
{
    namespace Client
    {
        public class Client
        {
            public ClientConfigOptions configOptions { get; private set; }

            public Client(ClientConfigOptions configOptions)
            {
                this.configOptions = configOptions;
                ConnectToServer();
            }

            private async void ConnectToServer()
            {
                Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await clientSocket.ConnectAsync(configOptions.ip, configOptions.port);
                DLog.Log(string.Format("Connected to server at {0}:{1}", configOptions.ip, configOptions.port));
            }
        }
    }
}