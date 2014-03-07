namespace LNetwork
{
    
    /// <summary>
    /// sent as a prefix of messages so the server knows what message it is
    /// </summary>
    public enum EMessageCode : byte
    {
        None = 0,
        HandShake = 90,
        DirtyHand = 91,
        GreatSuccess = 1,
        InvalidUsername = 2,
        InvalidPassword = 3,
        MessageAll = 4,
        MessageWhisper = 5,
        MessageFile = 6,
        ServerCommand = 7,
        DropConnection = 8,
        UserDisconnect = 9,
        UserConnected = 10,
        UsersOnline = 11
    }

    public enum EMessageMode
    {
        All = 4,
        Whisper = 5,
        File = 6
    }
}
