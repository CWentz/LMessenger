namespace LMessage
{
    public class MessageText : IMessage
    {
        private string message;

        public string Message
        {
            get { return this.message; }
        }

        public MessageText(string message)
        {
            this.message = message;
        }
    }
}
