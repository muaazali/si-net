using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace SiNet
{
    namespace Server
    {
        public class Server
        {
            public static List<Server> Instances = new List<Server>();
            public ServerConfigOptions configOptions { get; private set; }

            private TcpListener tcpListener;

            public Server(ServerConfigOptions configOptions)
            {
                if (Instances.Find(x => x.configOptions.ip == configOptions.ip && x.configOptions.port == configOptions.port) != null)
                {
                    DLog.LogError(string.Format("A server is already running on {0}:{1}", this.configOptions.ip, this.configOptions.port));
                    return;
                }

                this.configOptions = configOptions;
                Instances.Add(this);

                int returnCode = StartListening();
                if (returnCode != 0)
                {
                    DLog.LogError(string.Format("Failed to start server on {0}:{1}", this.configOptions.ip, this.configOptions.port));
                }
            }

            private int StartListening()
            {
                try {
                    tcpListener = new TcpListener(IPAddress.Parse(configOptions.ip), configOptions.port);
                    tcpListener.Start();
                    StartAcceptingClients();
                    DLog.Log(string.Format("Server started on {0}:{1}", configOptions.ip, configOptions.port));
                    return 0;
                }
                catch (Exception e)
                {
                    DLog.LogError(e.ToString());
                    return 1;
                }
            }

            private async void StartAcceptingClients()
            {
                while (true)
                {
                    Socket clientSocket = await tcpListener.AcceptSocketAsync();
                    DLog.Log(string.Format("Client connected from {0}", clientSocket.RemoteEndPoint.ToString()));
                }
            }
        }

    }
}
