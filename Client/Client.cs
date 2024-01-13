using System.Collections.Generic;
using System.Net;
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
            private byte[] readBuffer;
            private Socket serverSocket;

#pragma warning disable CS8618

            public Client(ClientConnectionSettings connectionSettings)
#pragma warning restore CS8618

            {
                this.connectionSettings = connectionSettings;
                readBuffer = new byte[connectionSettings.maxPacketSize];
                ConnectToServer();
            }

            private async void ConnectToServer()
            {
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(connectionSettings.ip), connectionSettings.port);
                serverSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    await serverSocket.ConnectAsync(ipEndPoint);
                }
                catch (System.Exception e)
                {
                    DLog.LogError(string.Format("CLIENT: Failed to connect to server at {0}:{1}: {2}", connectionSettings.ip, connectionSettings.port, e.Message));
                    return;
                }

                DLog.Log(string.Format("CLIENT: Connected to server at {0}:{1}", connectionSettings.ip, connectionSettings.port));

                OnConnectedToServer?.Invoke();
                On(EventType.CLIENT_ID_SENT_TO_CLIENT, OnReceiveConnectionResponseData);

                StartListeningToServer();
            }

            public void DisconnectFromServer()
            {
                if (serverSocket == null)
                {
                    return;
                }

                serverSocket.Close();

                OnDisconnectedFromServer?.Invoke();
                RemoveEventHandler(EventType.CLIENT_ID_SENT_TO_CLIENT, OnReceiveConnectionResponseData);
                DLog.Log(string.Format("CLIENT: Disconnected from server at {0}:{1}", connectionSettings.ip, connectionSettings.port));
            }

            private async void StartListeningToServer()
            {
                while (serverSocket.Connected)
                {
                    int bytesRead = 0;
                    try
                    {
                        bytesRead = await serverSocket.ReceiveAsync(readBuffer, SocketFlags.None);
                    }
                    catch (System.Exception e)
                    {
                        DLog.LogError(string.Format("CLIENT: Failed to receive message from server: {0}", e.Message));
                        DisconnectFromServer();
                        break;
                    }
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
                if (serverSocket == null)
                {
                    DLog.LogError("CLIENT: Cannot send message to server. Not connected to server.");
                    return;
                }

                while (!serverSocket.Connected)
                {
                    await System.Threading.Tasks.Task.Delay(100);
                }

                Message message = new Message()
                {
                    eventName = eventType,
                    data = data
                };
                DLog.Log(string.Format("CLIENT: Sending message to server: {0}", JsonConvert.SerializeObject(message)));
                serverSocket.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(message)));
            }

            private void OnReceiveConnectionResponseData(Message message)
            {
                ClientConnectionResponseData? data = JsonConvert.DeserializeObject<ClientConnectionResponseData>(message.data);
                if (data == null)
                {
                    DLog.LogError("CLIENT: Failed to deserialize client connection response data.");
                    return;
                }
                clientId = data.clientId;
            }

            ~Client()
            {
                eventHandlers.Clear();
                DisconnectFromServer();
            }
        }
    }
}