using Newtonsoft.Json;

namespace SiNet
{
    public class Message
    {
        public string eventType;
        public string data;
    }

    public static class MessageUtility
    {
        public static string CreateMessageJson(string eventType, string data = null)
        {
            return JsonConvert.SerializeObject(new Message()
            {
                eventType = eventType,
                data = data
            });
        }

        public static Message ParseMessage(string message)
        {
            return JsonConvert.DeserializeObject<Message>(message);
        }
    }
}