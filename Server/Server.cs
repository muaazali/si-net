using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;

namespace SiNet.Server
{
    public partial class Server
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

        private ServerConnectionProcessor connectionProcessor;
        private ServerMessageListener messageListener;

#pragma warning disable CS8618

        public Server(ServerConnectionSettings connectionSettings)
#pragma warning restore CS8618

        {
            if (Instances.Find(x => x.connectionSettings.port == connectionSettings.port) != null)
            {
                DLog.LogError(string.Format("SERVER: A server is already running on {0}", this.connectionSettings.port));
                return;
            }

            this.connectionSettings = connectionSettings;
            Instances.Add(this);
            connectionProcessor = new ServerConnectionProcessor(this);
            messageListener = new ServerMessageListener(this);

            int returnCode = StartListening();
            if (returnCode != 0)
            {
                DLog.LogError(string.Format("SERVER: Failed to start server on {0}", this.connectionSettings.port));
            }

            OnClientConnected += StartListeningToClient;
        }

        private int StartListening()
        {
            try
            {
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, connectionSettings.port);
                serverSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(ipEndPoint);
                serverSocket.Listen();

                StartAcceptingClients();
                DLog.Log(string.Format("SERVER: Server started on {0}", connectionSettings.port));
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
            DLog.Log(string.Format("SERVER: Started accepting clients. Total Clients: {0}", connectedClients.Count));
            while (shouldAcceptClients)
            {
                Socket clientSocket = await serverSocket.AcceptAsync();
                connectionProcessor.ProcessClientConnection(clientSocket);
            }
        }

        private void StopAcceptingClients()
        {
            shouldAcceptClients = false;
            DLog.Log(string.Format("SERVER: Stopped accepting clients. Total Clients: {0}", connectedClients.Count));
        }

        private void StartListeningToClient(ConnectedClientInfo clientInfo)
        {
            messageListener.ListenToMessages(clientInfo);
        }

        private void DisconnectClient(ConnectedClientInfo clientInfo)
        {
            clientInfo.socket.Close();
            if (connectedClients.ContainsKey(clientInfo.clientId))
            {
                connectedClients.Remove(clientInfo.clientId);
            }
            DLog.Log(string.Format("SERVER: Client {0} disconnected. Total Clients: {1}", clientInfo.clientId, connectedClients.Count));
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
            if (clientInfo.socket == null) return;
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