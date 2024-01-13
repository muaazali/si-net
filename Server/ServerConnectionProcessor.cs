using System.Net.Sockets;
using Newtonsoft.Json;

namespace SiNet.Server
{
    public partial class Server
    {
        private class ServerConnectionProcessor
        {
            private Server server;

            public ServerConnectionProcessor(Server server)
            {
                this.server = server;
            }

            public async void ProcessClientConnection(Socket clientSocket)
            {
                // if (clientSocket == null || !clientSocket.Connected) return;
                DLog.Log("Client socket not null.");
                if (clientSocket.RemoteEndPoint == null) return;
                DLog.Log("Client remote endpoint not null.");

                string[]? clientIP = clientSocket.RemoteEndPoint.ToString()?.Split(':');
                if (clientIP == null || clientIP.Length != 2) return;
                DLog.Log("Client IP not null.");

                DLog.Log(string.Format("SERVER: Processing client connection from {0}", clientSocket.RemoteEndPoint.ToString()));
                ConnectedClientInfo connectedClientInfo = new ConnectedClientInfo(clientIP[0], int.Parse(clientIP[1]), clientSocket, System.Guid.NewGuid().ToString(), server.connectionSettings.maxPacketSize);

                if (server.connectedClients.Count < server.connectionSettings.maxClients)
                {
                    server.connectedClients.Add(connectedClientInfo.clientId, connectedClientInfo);
                }
                else
                {
                    server.DisconnectClient(connectedClientInfo);
                    DLog.Log(string.Format("SERVER: Max clients reached. Not accepting any more clients."));
                    return;
                }

                if (server.connectedClients.Count >= server.connectionSettings.maxClients)
                {
                    DLog.Log(string.Format("SERVER: Max clients reached. Not accepting any more clients."));
                    server.StopAcceptingClients();
                    return;
                }

                try
                {
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

                    server.OnClientConnected?.Invoke(connectedClientInfo);
                    DLog.Log(string.Format("SERVER: Client connected from {0}. Total Clients: {1}", clientSocket.RemoteEndPoint.ToString(), server.connectedClients.Count));

                    await Task.Yield();
                }
                catch (System.Exception e)
                {
                    DLog.LogError(string.Format("SERVER: Failed to process client {0} connection: {1}", e.Message));
                    server.DisconnectClient(connectedClientInfo);
                }
            }
        }
    }
}