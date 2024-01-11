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
            public System.Action<ConnectedClientInfo> OnClientDisconnected;
            #endregion

            private Dictionary<string, ConnectedClientInfo> connectedClients = new Dictionary<string, ConnectedClientInfo>();
            private Dictionary<string, System.Action<ConnectedClientInfo, Message>> eventHandlers = new Dictionary<string, System.Action<ConnectedClientInfo, Message>>();

            private Socket serverSocket;
            private bool shouldAcceptClients = true;

#pragma warning disable CS8618

            public Server(ServerConnectionSettings connectionSettings)
#pragma warning restore CS8618

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

                OnClientConnected += StartListeningToClient;

            }

            private int StartListening()
            {
                try
                {
                    serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    serverSocket.Bind(new IPEndPoint(IPAddress.Parse(connectionSettings.ip), connectionSettings.port));
                    serverSocket.Listen(0);
                    StartAcceptingClients();
                    DLog.Log(string.Format("SERVER: Server started on {0}:{1}", connectionSettings.ip, connectionSettings.port));
                    return 0;
                }
                catch (System.Exception e)
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
                if (clientSocket == null || !clientSocket.Connected) return;
                if (clientSocket.RemoteEndPoint == null) return;

                string[]? clientIP = clientSocket.RemoteEndPoint.ToString()?.Split(':');
                if (clientIP == null || clientIP.Length != 2) return;

                ConnectedClientInfo connectedClientInfo = new ConnectedClientInfo(clientIP[0], int.Parse(clientIP[1]), clientSocket, System.Guid.NewGuid().ToString());

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

                string sendClientIdMessage = MessageUtility.CreateMessageJson(
                    EventType.CLIENT_ID_SENT_TO_CLIENT,
                    JsonConvert.SerializeObject(
                        new Client.ClientConnectionResponseData()
                        {
                            clientId = connectedClientInfo.clientId
                        }
                    )
                );
                clientSocket.Send(System.Text.Encoding.ASCII.GetBytes(sendClientIdMessage));

                OnClientConnected?.Invoke(connectedClientInfo);
                DLog.Log(string.Format("SERVER: Client connected from {0}. Total Clients: {1}", clientSocket.RemoteEndPoint.ToString(), connectedClients.Count));
            }

            private async void StartListeningToClient(ConnectedClientInfo clientInfo)
            {
                while (clientInfo.socket.Connected)
                {
                    int bytesRead = await clientInfo.socket.ReceiveAsync(clientInfo.buffer, SocketFlags.None);
                    if (bytesRead == 0)
                    {
                        DisconnectClient(clientInfo);
                        break;
                    }
                    Message? message = MessageUtility.ParseMessage(System.Text.Encoding.ASCII.GetString(clientInfo.buffer, 0, bytesRead));
                    if (message == null)
                    {
                        DLog.LogError(string.Format("SERVER: Invalid message received: {0}", System.Text.Encoding.ASCII.GetString(clientInfo.buffer, 0, bytesRead)));
                        continue;
                    }

                    if (eventHandlers.ContainsKey(message.eventName))
                    {
                        eventHandlers[message.eventName]?.Invoke(clientInfo, message);
                    }
                }
            }

            private void DisconnectClient(ConnectedClientInfo clientInfo)
            {
                clientInfo.socket.Close();
                connectedClients.Remove(clientInfo.clientId);
                DLog.Log(string.Format("SERVER: Client {0} disconnected", clientInfo.clientId));
                OnClientDisconnected?.Invoke(clientInfo);
            }

            public void On(string eventType, System.Action<ConnectedClientInfo, Message> callback)
            {
                if (eventHandlers.ContainsKey(eventType))
                {
                    eventHandlers[eventType] += callback;
                }
                else
                {
                    eventHandlers.Add(eventType, callback);
                }
            }

            public void RemoveEventHandler(string eventType, System.Action<ConnectedClientInfo, Message> callback)
            {
                if (eventHandlers.TryGetValue(eventType, out var eventHandler))
                {
                    eventHandler -= callback;
                }
            }

            public void Send(ConnectedClientInfo clientInfo, string eventName, string data = "")
            {
                if (!clientInfo.socket.Connected) return;

                Message message = new Message()
                {
                    eventName = eventName,
                    data = data
                };
                clientInfo.socket.Send(System.Text.Encoding.ASCII.GetBytes(MessageUtility.CreateMessageJson(message.eventName, message.data)));
            }

            public void SendToAll(string eventName, string data = "")
            {
                foreach (ConnectedClientInfo clientInfo in connectedClients.Values)
                {
                    Send(clientInfo, eventName, data);
                }
            }

            ~Server()
            {
                StopAcceptingClients();
                foreach (ConnectedClientInfo clientInfo in connectedClients.Values)
                {
                    clientInfo.socket.Close();
                }
                serverSocket.Close();
                Instances.Remove(this);
            }
        }

    }
}
