namespace LNetwork.LMessage
{
    [System.Serializable]
    public class MessageDisconnect : IMessage
    {
        private const EMessageType type = EMessageType.Disconnect;

        public EMessageType Message
        {
            get { return type; }
        }
    }
}
