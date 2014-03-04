using LNetwork.LMessage;

namespace LNetwork
{
  public delegate void ConnectionMessageReceivedDelegate(ConnectionMessageReceivedEventArgs args);

  public struct ConnectionMessageReceivedEventArgs
  {
    public LClient Client;

    public IMessage Message;

    public ConnectionMessageReceivedEventArgs(IMessage message, LClient client)
    {
      this.Client = client;
      this.Message = message;
    }
  }
}
