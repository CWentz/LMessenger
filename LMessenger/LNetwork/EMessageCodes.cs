namespace LNetwork
{
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

}
