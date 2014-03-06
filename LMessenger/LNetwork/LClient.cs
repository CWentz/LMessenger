using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace LNetwork
{
    //http://www.codeproject.com/Articles/429144/Simple-Instant-Messenger-with-SSL-Encryption-in-Cs
    //http://www.codeproject.com/Articles/13232/A-very-basic-TCP-server-written-in-C

    //http://www.dreamincode.net/forums/topic/231058-peer-to-peer-chat-advanced/
    //http://www.dreamincode.net/forums/topic/33396-basic-clientserver-chat-application-in-c%23/

    //http://www.c-sharpcorner.com/UploadFile/bfarley/SocketChatBF11182005013225AM/SocketChatBF.aspx


    public class LClient
    {
        private Socket socket;
        private Thread thread;
        private EndPoint endpoint;

        private string userName;

        public LClient(string userName, EndPoint endpoint, Thread thread, Socket socket)
        {
            this.userName = userName;
            this.endpoint = endpoint;
            this.thread = thread;
            this.socket = socket;
        }

        public string LogData()
        {
 	         return endpoint.ToString() + " : " + this.userName;
        }

        public Thread MyThread
        {
            get { return this.thread; }
            set { this.thread = value; }
        }

        public EndPoint Host
        {
            get { return this.endpoint; }
            set { this.endpoint = value; }
        }

        public Socket MySocket
        {
            get { return this.socket; }
            set { this.socket = value; }
        }

        public string UserName
        {
            get { return this.userName; }
            set { this.userName = value; }
        }

        public bool Shutdown()
        {
            int counter = 0;
            while (this.thread != null && this.socket != null && this.endpoint != null)
            {
                if (this.thread != null)
                {
                    if (this.thread.IsAlive)
                        this.thread.Abort();
                    this.thread = null;
                }

                if (this.socket != null)
                {
                    this.socket.Disconnect(true);
                    this.socket.Close();
                    this.socket.Dispose();
                    this.socket = null;
                }

                if (this.endpoint != null)
                {
                    this.endpoint = null;
                }

                counter++;
                if (counter > 10)
                    return false;
            }

            return true;
        }
    }
}
