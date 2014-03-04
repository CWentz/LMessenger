namespace LNetwork.LMessage
{
    [System.Serializable]
    public class MessageChat : IMessage
    {
        private const EMessageType type = EMessageType.Chat;
        public string chat;

        public EMessageType Message
        {
            get { return type; }
        }

        public MessageChat(string message)
        {
            this.chat = message;
        }

        public string Chat
        {
            get { return this.chat; }
        }
    }
}
