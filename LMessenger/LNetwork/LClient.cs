using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace LNetwork
{
    /// <summary>
    /// client class for TCP using sockets
    /// </summary>
    public class LClient
    {
        private Socket socket;
        private Thread thread;
        private EndPoint endpoint;
        private int fileBytecount = 0;
        private string userName;

        /// <summary>
        /// LClient is used to hold networking "pointers" and username for port.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="endpoint"></param>
        /// <param name="thread"></param>
        /// <param name="socket"></param>
        public LClient(string userName, EndPoint endpoint, Thread thread, Socket socket)
        {
            this.userName = userName;
            this.endpoint = endpoint;
            this.thread = thread;
            this.socket = socket;
        }

        /// <summary>
        /// returns endpoint + username
        /// </summary>
        /// <returns></returns>
        public string LogData()
        {
 	         return endpoint.ToString() + " : " + this.userName;
        }

        /// <summary>
        /// get/set for thread
        /// </summary>
        public Thread MyThread
        {
            get { return this.thread; }
            set { this.thread = value; }
        }

        /// <summary>
        /// get/set for endpoint
        /// </summary>
        public EndPoint Host
        {
            get { return this.endpoint; }
            set { this.endpoint = value; }
        }

        /// <summary>
        /// get/set for socket
        /// </summary>
        public Socket MySocket
        {
            get { return this.socket; }
            set { this.socket = value; }
        }

        /// <summary>
        /// get/set for username
        /// </summary>
        public string UserName
        {
            get { return this.userName; }
            set { this.userName = value; }
        }

        /// <summary>
        /// shuts down all connects of this LClient
        /// </summary>
        public void Shutdown()
        {
            
            if (this.socket != null)
            {
                if(this.socket.Connected)
                    this.socket.Disconnect(true);
                this.socket.Close();
                this.socket.Dispose();
                this.socket = null;
            }

            if (this.endpoint != null)
            {
                this.endpoint = null;
            }

        }
    }
}
