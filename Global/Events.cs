namespace SiNet
{
    [System.Serializable]
    public static partial class EventType
    {
        public const string CLIENT_CONNECTED = "CLIENT_CONNECTED";
        public const string CLIENT_DISCONNECTED = "CLIENT_DISCONNECTED";
        public const string CLIENT_ID_SENT_TO_CLIENT = "CLIENT_ID_SENT_TO_CLIENT";
        public const string CLIENT_ID_RECEIVED = "CLIENT_ID_RECEIVED";
    }
}