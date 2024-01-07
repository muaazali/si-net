using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;

namespace SiNet
{
    namespace Server
    {
        public class Server
        {
            public static List<Server> Instances = new List<Server>();
            public ServerConnectionSettings connectionSettings { get; private set; }

            #region Events
            public System.Action<ConnectedClientInfo> OnClientConnected;
            #endregion

            private Dictionary<string, ConnectedClientInfo> connectedClients = new Dictionary<string, ConnectedClientInfo>();

            private Socket serverSocket;
            private bool shouldAcceptClients = true;

            public Server(ServerConnectionSettings connectionSettings)
            {
                if (Instances.Find(x => x.connectionSettings.ip == connectionSettings.ip && x.connectionSettings.port == connectionSettings.port) != null)
                {
                    DLog.LogError(string.Format("SERVER: A server is already running on {0}:{1}", this.connectionSettings.ip, this.connectionSettings.port));
                    return;
                }

                this.connectionSettings = connectionSettings;
                Instances.Add(this);

                int returnCode = StartListening();
                if (returnCode != 0)
                {
                    DLog.LogError(string.Format("SERVER: Failed to start server on {0}:{1}", this.connectionSettings.ip, this.connectionSettings.port));
                }
            }

            private int StartListening()
            {
                try {
                    serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    serverSocket.Bind(new IPEndPoint(IPAddress.Parse(connectionSettings.ip), connectionSettings.port));
                    serverSocket.Listen(0);
                    StartAcceptingClients();
                    DLog.Log(string.Format("SERVER: Server started on {0}:{1}", connectionSettings.ip, connectionSettings.port));
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
                while (shouldAcceptClients)
                {
                    Socket clientSocket = await serverSocket.AcceptAsync();

                    ThreadPool.QueueUserWorkItem((_) =>
                    {
                        ProcessClientConnection(clientSocket);
                    });
                }
            }

            private void StopAcceptingClients()
            {
                shouldAcceptClients = false;
                DLog.Log(string.Format("SERVER: Stopped accepting clients. Total Clients: {0}", connectedClients.Count));
            }

            private void ProcessClientConnection(Socket clientSocket)
            {
                string[] clientIP = clientSocket.RemoteEndPoint.ToString().Split(':');
                ConnectedClientInfo connectedClientInfo = new ConnectedClientInfo(clientIP[0], int.Parse(clientIP[1]), clientSocket, Guid.NewGuid().ToString());

                if (connectedClients.Count < ConfigDefaults.MAX_CLIENTS)
                {
                    connectedClients.Add(connectedClientInfo.clientId, connectedClientInfo);
                }
                else
                {
                    DLog.Log(string.Format("SERVER: Max clients reached. Not accepting any more clients."));
                    return;
                }

                if (connectedClients.Count >= ConfigDefaults.MAX_CLIENTS)
                {
                    StopAcceptingClients();
                }

                string sendClientIdMessage = MessageUtility.CreateMessage(
                    EventType.CLIENT_ID_SENT_TO_CLIENT,
                    new Client.ClientConnectionResponseData()
                    {
                        clientId = connectedClientInfo.clientId
                    }
                );
                clientSocket.Send(System.Text.Encoding.ASCII.GetBytes(sendClientIdMessage));

                clientSocket.Receive(connectedClientInfo.buffer, SocketFlags.None);
                Message clientReceivedMessage = MessageUtility.ParseMessage(System.Text.Encoding.ASCII.GetString(connectedClientInfo.buffer));
                if (clientReceivedMessage == null || clientReceivedMessage.eventType != EventType.CLIENT_ID_RECEIVED)
                {
                    DLog.LogError(string.Format("SERVER: Invalid message received: {0}", System.Text.Encoding.ASCII.GetString(connectedClientInfo.buffer)));
                    return;
                }

                string sendClientConnectedMessage = MessageUtility.CreateMessage(
                    EventType.CLIENT_CONNECTED
                );
                clientSocket.Send(System.Text.Encoding.ASCII.GetBytes(sendClientConnectedMessage));

                OnClientConnected?.Invoke(connectedClientInfo);
                DLog.Log(string.Format("SERVER: Client connected from {0}. Total Clients: {1}", clientSocket.RemoteEndPoint.ToString(), connectedClients.Count));
            }
        }

    }
}
