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

#pragma warning disable CS8618

            public Client(ClientConnectionSettings connectionSettings)
#pragma warning restore CS8618

            {
                this.connectionSettings = connectionSettings;
                ConnectToServer();
            }

            private async void ConnectToServer()
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                await serverSocket.ConnectAsync(connectionSettings.ip, connectionSettings.port);

                int bytesRead = await serverSocket.ReceiveAsync(readBuffer, SocketFlags.None);
                Message? connectionMessage = MessageUtility.ParseMessage(Encoding.ASCII.GetString(readBuffer, 0, bytesRead));
                if (connectionMessage == null || connectionMessage.eventName != EventType.CLIENT_ID_SENT_TO_CLIENT)
                {
                    DLog.LogError("CLIENT: Invalid message received. Could not receive client ID.");
                    return;
                }
                ClientConnectionResponseData? clientConnectionResponseData = JsonConvert.DeserializeObject<ClientConnectionResponseData>(connectionMessage.data);
                if (clientConnectionResponseData == null)
                {
                    DLog.LogError("CLIENT: Invalid message received. Could not receive client ID.");
                    return;
                }
                clientId = clientConnectionResponseData.clientId;

                DLog.Log(string.Format("CLIENT: Connected to server at {0}:{1}", connectionSettings.ip, connectionSettings.port));
                OnConnectedToServer?.Invoke();

                StartListeningToServer();
            }

            public void DisconnectFromServer()
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
                    Message? message = MessageUtility.ParseMessage(Encoding.ASCII.GetString(readBuffer, 0, bytesRead));
                    if (message == null)
                    {
                        DLog.LogError(string.Format("CLIENT: Invalid message received: {0}", Encoding.ASCII.GetString(readBuffer, 0, bytesRead)));
                        continue;
                    }

                    DLog.Log(string.Format("CLIENT: Received message from server: {0}", JsonConvert.SerializeObject(message)));
                    if (eventHandlers.ContainsKey(message.eventName))
                    {
                        eventHandlers[message.eventName]?.Invoke(message);
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
                if (eventHandlers.TryGetValue(eventType, out var eventHandler))
                {
                    eventHandler -= callback;
                }
            }

            public async void Send(string eventType, string data = "")
            {
                // TODO Use a proper async await library like Cysharp.UniTasks.
                while (serverSocket == null || !serverSocket.Connected || clientId == null || clientId == string.Empty)
                {
                    await System.Threading.Tasks.Task.Delay(100);
                }

                if (serverSocket == null)
                {
                    return;
                }
                Message message = new Message()
                {
                    eventName = eventType,
                    data = data
                };
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