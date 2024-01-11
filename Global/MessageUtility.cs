using Newtonsoft.Json;

namespace SiNet
{
    public class Message
    {
        public string eventName = "";
        public string data = "";
    }

    public static class MessageUtility
    {
        public static string CreateMessageJson(string eventType, string data = "")
        {
            return JsonConvert.SerializeObject(new Message()
            {
                eventName = eventType,
                data = data
            });
        }

        public static Message? ParseMessage(string message)
        {
            return JsonConvert.DeserializeObject<Message>(message);
        }
    }
}