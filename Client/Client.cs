using System;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace SiNet
{
    namespace Client
    {
        public class Client
        {
            public string clientId;
            public ClientConnectionSettings connectionSettings { get; private set; }

            #region Events
            public System.Action OnConnectedToServer;
            public System.Action OnDisconnectedFromServer;
            #endregion

            private byte[] readBuffer = new byte[4096];
            private Socket clientSocket;

            public Client(ClientConnectionSettings connectionSettings)
            {
                this.connectionSettings = connectionSettings;
                ConnectToServer();
            }

            private async void ConnectToServer()
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await clientSocket.ConnectAsync(connectionSettings.ip, connectionSettings.port);

                int bytesRead = await clientSocket.ReceiveAsync(readBuffer, SocketFlags.None);
                Message connectionMessage = MessageUtility.ParseMessage(Encoding.ASCII.GetString(readBuffer, 0, bytesRead));
                if (connectionMessage == null || connectionMessage.eventType != EventType.CLIENT_ID_SENT_TO_CLIENT)
                {
                    DLog.LogError("CLIENT: Invalid message received. Could not receive client ID.");
                    return;
                }
                ClientConnectionResponseData clientConnectionResponseData = JsonConvert.DeserializeObject<ClientConnectionResponseData>(connectionMessage.data);
                clientId = clientConnectionResponseData.clientId;

                string clientIdReceivedConfirmationMessage = MessageUtility.CreateMessage(
                    EventType.CLIENT_ID_RECEIVED
                );
                await clientSocket.SendAsync(Encoding.ASCII.GetBytes(clientIdReceivedConfirmationMessage), SocketFlags.None);

                bytesRead = await clientSocket.ReceiveAsync(readBuffer, SocketFlags.None);
                Message clientConnectedMessage = MessageUtility.ParseMessage(Encoding.ASCII.GetString(readBuffer, 0, bytesRead));
                if (clientConnectedMessage == null || clientConnectedMessage.eventType != EventType.CLIENT_CONNECTED)
                {
                    DLog.LogError("CLIENT: Invalid message received. Could not receive client ID.");
                    return;
                }

                DLog.Log(string.Format("CLIENT: Connected to server at {0}:{1}", connectionSettings.ip, connectionSettings.port));
                OnConnectedToServer?.Invoke();
            }

            public async void DisconnectFromServer()
            {
                if (clientSocket == null)
                {
                    return;
                }

                string disconnectMessage = MessageUtility.CreateMessage(
                    EventType.CLIENT_DISCONNECTED
                );
                try
                {
                    await clientSocket.SendAsync(Encoding.ASCII.GetBytes(disconnectMessage), SocketFlags.None);
                }
                catch (Exception e)
                {
                    DLog.LogError(string.Format("CLIENT: Error sending disconnect message to server: {0}", e.Message));
                }
                finally
                {
                    clientSocket.Close();
                    clientSocket = null;

                    OnDisconnectedFromServer?.Invoke();
                    DLog.Log(string.Format("CLIENT: Disconnected from server at {0}:{1}", connectionSettings.ip, connectionSettings.port));
                }
            }

            public void SendMessageToServer(string message)
            {
                if (clientSocket == null)
                {
                    return;
                }

                clientSocket.Send(Encoding.ASCII.GetBytes(message));
            }
        }
    }
}