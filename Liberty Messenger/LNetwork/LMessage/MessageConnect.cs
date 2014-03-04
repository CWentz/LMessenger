namespace LNetwork.LMessage
{
    [System.Serializable]
    public class MessageConnect : IMessage
    {
        private EMessageType type = EMessageType.Connect;

        private string userName;
        private string nickName;
        private string passWord;
        private bool passwordPulled;

        public EMessageType Message
        {
            get { return this.type; }
        }

        public MessageConnect(string userName, string nickName, string passWord)
        {
            this.userName = userName;
            this.nickName = nickName;
            this.passWord = passWord;
            this.passwordPulled = false;
        }

        public string User
        {
            get { return this.userName; }
        }
        public string Nick
        {
            get { return this.nickName; }
        }
        public string Pass
        {
            get {
                if (this.passwordPulled)
                    return "";
                this.passwordPulled = true;
                return this.passWord; }
        }
    }
}
