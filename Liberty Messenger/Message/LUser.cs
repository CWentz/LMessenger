using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
namespace LMessengerCore
{
    public class LUser// : TcpClient
    {
        private TcpClient client;
        private Thread messageThread;
        StreamReader reader;
        StreamWriter writer;

        private string userName;
        private string nickName;
        private string password;
        
        public TcpClient Client
        {
            get { return this.client; }
        }

        public string UserName
        {
            get { return this.userName; }
            set { this.userName = value; }
        }

        public string NickName
        {
            get { return this.nickName; }
            set { this.nickName = value; }
        }

        //i know this isn't secure but just testing.
        public string Password
        {
            get { return this.password; }
            set { this.password = value; }
        }

        public bool ThreadRunning
        {
            get { return this.messageThread.IsAlive; }
        }


        public LUser(TcpClient client)
        {
            this.userName = "";
            this.nickName = "";
            this.password = "";

            this.client = client;

            this.StartThread();
        }

        public void StartChat()
        {
            reader = new StreamReader(client.GetStream());
            writer = new StreamWriter(client.GetStream());

            writer.WriteLine("Welcome to LMessenger!");

            ELoginError error = LServer.ConnectAsUser(this);
            if(error != ELoginError.None)
            {
                writer.WriteLine("LUser StartChat ERROR: " + error.ToString());
                writer.Flush();
                writer = null;
                reader = null;
                this.StopThread();
                return;
            }
            //connection as user successful
            writer.WriteLine(UserName + " Connected.");

            Thread chatThread = new Thread(new ThreadStart(RunChat));

            chatThread.Start();
        }

        private void RunChat()
        {
            try
            {
                string output = "";

                while(true)
                {
                    output = reader.ReadLine();

                    LServer.SendGlobalMessage(this.NickName, output);
                }

            }
            catch(Exception e)
            {
                Console.WriteLine("LUser Error: " + e);
            }
        }

        private void StartThread()
        {
            if (this.messageThread.IsAlive) return;

            this.messageThread = new Thread(new ThreadStart(StartChat));
            this.messageThread.Start();
        }

        public void StopThread()
        {
            if (!this.messageThread.IsAlive) return;

            this.messageThread.Abort();
        }
    }
}
