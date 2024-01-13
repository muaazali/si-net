using System.Net.Sockets;
using Newtonsoft.Json;

namespace SiNet.Server
{
    public partial class Server
    {
        private class ServerMessageListener
        {
            private Server server;

            public ServerMessageListener(Server server)
            {
                this.server = server;
            }

            public async void ListenToMessages(ConnectedClientInfo clientInfo)
            {
                while (clientInfo.socket.Connected)
                {
                    try
                    {
                        int bytesRead = await clientInfo.socket.ReceiveAsync(clientInfo.buffer, SocketFlags.None);
                        if (bytesRead == 0)
                        {
                            server.DisconnectClient(clientInfo);
                            break;
                        }
                        Message? message = MessageUtility.ParseMessage(System.Text.Encoding.ASCII.GetString(clientInfo.buffer, 0, bytesRead));
                        if (message == null)
                        {
                            DLog.LogError(string.Format("SERVER: Invalid message received: {0}", System.Text.Encoding.ASCII.GetString(clientInfo.buffer, 0, bytesRead)));
                            continue;
                        }

                        if (server.eventHandlers.ContainsKey(message.eventName))
                        {
                            server.eventHandlers[message.eventName]?.Invoke(clientInfo, message);
                        }
                    }
                    catch (System.Exception e)
                    {
                        DLog.LogError(string.Format("SERVER: Error while listening to messages from {0}: {1}", clientInfo.clientId, e.Message));
                        continue;
                    }
                }
            }
        }
    }
}