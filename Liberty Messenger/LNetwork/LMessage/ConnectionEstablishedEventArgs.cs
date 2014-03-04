namespace LNetwork
{
  public delegate void ConnectionEstablishedDelegate(ConnectionEstablishedEventArgs args);

  public struct ConnectionEstablishedEventArgs
  {
    public LClient Client;

    public ConnectionEstablishedEventArgs(LClient client)
    {
      this.Client = client;
    }
  }
}
