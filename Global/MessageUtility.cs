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
        public static string CreateMessage(string eventType, object data = null)
        {
            return JsonConvert.SerializeObject(new Message()
            {
                eventType = eventType,
                data = JsonConvert.SerializeObject(data)
            });
        }

        public static Message ParseMessage(string message)
        {
            return JsonConvert.DeserializeObject<Message>(message);
        }
    }
}