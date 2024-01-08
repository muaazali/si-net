using System.Collections.Generic;
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

            private Dictionary<string, System.Action<Message>> eventHandlers = new Dictionary<string, System.Action<Message>>();
            private byte[] readBuffer = new byte[4096];
            private Socket serverSocket;

            public Client(ClientConnectionSettings connectionSettings)
            {
                this.connectionSettings = connectionSettings;
                ConnectToServer();
            }

            private async void ConnectToServer()
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                await serverSocket.ConnectAsync(connectionSettings.ip, connectionSettings.port);

                int bytesRead = await serverSocket.ReceiveAsync(readBuffer, SocketFlags.None);
                Message connectionMessage = MessageUtility.ParseMessage(Encoding.ASCII.GetString(readBuffer, 0, bytesRead));
                if (connectionMessage == null || connectionMessage.eventType != EventType.CLIENT_ID_SENT_TO_CLIENT)
                {
                    DLog.LogError("CLIENT: Invalid message received. Could not receive client ID.");
                    return;
                }
                ClientConnectionResponseData clientConnectionResponseData = JsonConvert.DeserializeObject<ClientConnectionResponseData>(connectionMessage.data);
                clientId = clientConnectionResponseData.clientId;

                string clientIdReceivedConfirmationMessage = MessageUtility.CreateMessageJson(
                    EventType.CLIENT_ID_RECEIVED
                );
                await serverSocket.SendAsync(Encoding.ASCII.GetBytes(clientIdReceivedConfirmationMessage), SocketFlags.None);

                bytesRead = await serverSocket.ReceiveAsync(readBuffer, SocketFlags.None);
                Message clientConnectedMessage = MessageUtility.ParseMessage(Encoding.ASCII.GetString(readBuffer, 0, bytesRead));
                if (clientConnectedMessage == null || clientConnectedMessage.eventType != EventType.CLIENT_CONNECTED)
                {
                    DLog.LogError("CLIENT: Invalid message received. Could not receive client ID.");
                    return;
                }

                DLog.Log(string.Format("CLIENT: Connected to server at {0}:{1}", connectionSettings.ip, connectionSettings.port));
                OnConnectedToServer?.Invoke();

                StartListeningToServer();
            }

            public async void DisconnectFromServer()
            {
                if (serverSocket == null)
                {
                    return;
                }

                string disconnectMessage = MessageUtility.CreateMessageJson(
                    EventType.CLIENT_DISCONNECTED
                );
                try
                {
                    serverSocket.Send(Encoding.ASCII.GetBytes(disconnectMessage));
                }
                catch (System.Exception e)
                {
                    DLog.LogError(string.Format("CLIENT: Error sending disconnect message to server: {0}", e.Message));
                }
                finally
                {
                    serverSocket.Close();
                    serverSocket = null;

                    OnDisconnectedFromServer?.Invoke();
                    DLog.Log(string.Format("CLIENT: Disconnected from server at {0}:{1}", connectionSettings.ip, connectionSettings.port));
                }
            }

            private async void StartListeningToServer()
            {
                while (serverSocket.Connected)
                {
                    int bytesRead = await serverSocket.ReceiveAsync(readBuffer, SocketFlags.None);
                    if (bytesRead == 0)
                    {
                        DisconnectFromServer();
                        break;
                    }
                    Message message = MessageUtility.ParseMessage(Encoding.ASCII.GetString(readBuffer, 0, bytesRead));
                    if (message == null)
                    {
                        DLog.LogError(string.Format("CLIENT: Invalid message received: {0}", Encoding.ASCII.GetString(readBuffer, 0, bytesRead)));
                    }

                    DLog.Log(string.Format("CLIENT: Received message from server: {0}", JsonConvert.SerializeObject(message)));
                    if (eventHandlers.TryGetValue(message.eventType, out System.Action<Message> eventHandler))
                    {
                        eventHandler.Invoke(message);
                    }
                }
            }

            public void On(string eventType, System.Action<Message> callback)
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

            public void RemoveEventHandler(string eventType, System.Action<Message> callback)
            {
                if (eventHandlers.ContainsKey(eventType))
                {
                    eventHandlers[eventType] -= callback;
                }
            }

            public void Send(Message message)
            {
                if (serverSocket == null)
                {
                    return;
                }

                DLog.Log(string.Format("CLIENT: Sending message to server: {0}", JsonConvert.SerializeObject(message)));
                serverSocket.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(message)));
            }

            ~Client()
            {
                eventHandlers.Clear();
                DisconnectFromServer();
            }
        }
    }
}