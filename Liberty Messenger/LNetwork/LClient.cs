using LNetwork.LMessage;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace LNetwork
{
    public class LClient
    {
        private MessageConnect userInfo;
        private TcpListener listener;
        private TcpClient client;
        private Thread thread;
        private NetworkStream stream;
        private EConnectionState state;

        public event ConnectionMessageReceivedDelegate OnMessageReceived;
        public event ConnectionEstablishedDelegate OnConnectionEstablished;

        public IMessage Info
        {
            get { return this.userInfo; }
        }

        public TcpClient Client
        {
            get { return this.client; }
        }

        public LClient(TcpClient client)
        {
            this.LoadClient(client);
        }

        public LClient(string user, string nickname, string password)
        {
            this.userInfo = new MessageConnect(user, nickname, password);
        }

        public void Connect(IPAddress addy, int port)
        {
            this.client = new TcpClient();

            this.client.Connect(addy, port);

            this.LoadClient(client);
        }

        public void LoadClient(TcpClient client)
        {
            this.client = client;
            this.stream = client.GetStream();

            if(this.stream.CanWrite)
            {
                this.Message(this.userInfo);
            }

            this.thread = new Thread(this.Process);
            this.thread.Start();
        }

        public void Message(IMessage message)
        {
            //can't write don't try
            if (!this.stream.CanWrite) return;

            try
            {
                MemoryStream ms = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();

                formatter.Serialize(ms, message);

                byte[] data = ms.GetBuffer();
                byte[] size = BitConverter.GetBytes(data.Length);

                this.stream.Write(size, 0, 4);
                this.stream.Write(data, 0, data.Length);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                this.Disconnect();
            }
        }

        public void Disconnect()
        {
            this.state = EConnectionState.Disconnect;
        }

        private void Process()
        {
            this.state = EConnectionState.Connecting;

            if (this.OnConnectionEstablished != null)
                this.OnConnectionEstablished(new ConnectionEstablishedEventArgs(this));

            this.state = EConnectionState.Connected;
            this.client.ReceiveTimeout = 2500;

            while(this.state != EConnectionState.Disconnect)
            {
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    MemoryStream ms = new MemoryStream();

                    byte[] sizeBuffer = new byte[4];

                    // Read size of message
                    this.stream.Read(sizeBuffer, 0, 4);

                    // Total size of message
                    int totalSize = BitConverter.ToInt32(sizeBuffer, 0);
                    int size = totalSize;

                    while (size > 0)
                    {
                        byte[] data = new byte[1024];

                        int received = this.stream.Read(data, 0, 1024);

                        ms.Write(data, 0, received);

                        size -= received;
                    }

                    // Message has been completely received

                    if (totalSize > 0)
                    {
                        ms.Position = 0;
                        IMessage message = (IMessage)formatter.Deserialize(ms);

                        if (this.OnMessageReceived != null)
                            this.OnMessageReceived(new ConnectionMessageReceivedEventArgs(
                            message, this));
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine(e);
                }
            }

            this.state = EConnectionState.Disconnected;
            this.stream.Close();
            this.stream.Dispose();
            this.client.Close();
        }
    }
}
