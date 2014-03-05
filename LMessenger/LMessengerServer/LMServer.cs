using LNetwork;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LMessengerServer
{
    public partial class LMServer : Form
    {
        private bool isRunning = false;
        private const int port = 1337;
        private TcpListener listener;
        private List<LClient> clients;
        private Socket socket;
        private Thread thread;
        private Thread clientService;
        private StreamWriter logger;
        private Dictionary<string, string> credentials;
        private Dictionary<string, LClient> clientsDict;
        /// <summary>
        /// constructor
        /// </summary>
        public LMServer()
        {
            InitializeComponent();
            this.ToggleElements();

            this.comboBoxMessageType.Items.Add(EMessageMode.All.ToString());
            this.comboBoxMessageType.Items.Add(EMessageMode.Whisper.ToString());
            this.comboBoxMessageType.Items.Add(EMessageMode.File.ToString());
            this.comboBoxMessageType.Items.Add("Kick");

            this.comboBoxMessageType.Update();
            this.comboBoxMessageType.SelectedItem = this.comboBoxMessageType.Items[0];
        }

        /// <summary>
        /// clicked connect button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConnect_Click(object sender, EventArgs e)
        {
            this.isRunning = !this.isRunning;
            this.ToggleElements();

            if (this.isRunning)
            {
                this.btnConnect.Text = "Disconnect";
                this.listBoxUsers.Items.Add("Admin");
                this.DeployServer();
            }
            else
            {
                this.btnConnect.Text = "Deploy";
                this.listBoxUsers.Items.Remove("Admin");
                this.Shutdown();
            }

            this.listBoxUsers.Update();
        }

        /// <summary>
        /// clicked send command button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSendCommand_Click(object sender, EventArgs e)
        {
            switch(this.comboBoxMessageType.SelectedItem.ToString())
            {
                case "All":
                    break;
                case "PM":
                    break;
                case "File":
                    break;
                case "Kick":
                    break;

            }

            this.txtBoxReader.AppendText("Admin: " + this.txtBoxCommand.Text + "\n");
            this.txtBoxCommand.Text = "";
                
        }

        /// <summary>
        /// toggles the window elements based off of isRunning.
        /// </summary>
        private void ToggleElements()
        {
            this.txtBoxCommand.Enabled = this.isRunning;
            this.btnSendCommand.Enabled = this.isRunning;
            this.comboBoxMessageType.Enabled = this.isRunning;
        }

        private void LoggingStart()
        {
            if (!Directory.Exists("logs"))
                Directory.CreateDirectory("logs");
            string fname = "logs\\" + DateTime.Now.ToString("ddMMyyHHmm") + ".txt";
            this.logger = new StreamWriter(new FileStream(fname, FileMode.OpenOrCreate,
                FileAccess.Write));
        }

        private void DeployServer()
        {
            this.clientsDict = new Dictionary<string, LClient>();
            this.clients = new List<LClient>();
            this.credentials = new Dictionary<string, string>();
            this.LoadCredential();
            this.LoggingStart();

            this.thread = new Thread(new ThreadStart(StartListening));
            this.thread.Start();
        }

        private void StartListening()
        {
            this.isRunning = true;
            IPAddress ipAdd = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
            IPEndPoint ipEnd = new IPEndPoint(ipAdd, port);
            IPAddress ipTest = IPAddress.Parse("192.168.2.101");
            
            this.listener = new TcpListener(ipTest, port);
            this.listener.Start();
            while(this.isRunning)
            {
                try
                {
                    Socket s = listener.AcceptSocket();
                    this.socket = s;
                    this.clientService = new Thread(new ThreadStart(ServiceClient));
                    this.clientService.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private void ServiceClient()
        {
            Socket c = this.socket;
            bool isProcessing = true;

            while (isProcessing)
            {
                this.Recieve(c);
            }
        }

        private void Recieve(Socket client)
        {
            bool isLooping = true;
            while (isLooping)
            {
                try
                {
                    Byte[] buffer = new Byte[2048];
                    client.Receive(buffer);
                    string message = System.Text.Encoding.ASCII.GetString(buffer);

                    string[] tokens = message.Split(new Char[] { '|' });

                    switch ((EMessageCode)tokens[0][0])
                    {
                        case EMessageCode.None:
                            {
                                isLooping = false;
                            }
                            break;
                        case EMessageCode.HandShake:
                            {
                                if(this.CheckCredentials(tokens[1], tokens[2]))
                                {
                                    if(!this.clientsDict.ContainsKey(tokens[1]))
                                    {
                                        EndPoint ep = client.RemoteEndPoint;
                                        LClient c = new LClient(tokens[1],ep,clientService, client);

                                        this.clientsDict.Add(c.UserName, c);

                                        this.SendToClient(c, EMessageCode.HandShake, "Very nice");
                                        this.SentToAllClients(EMessageCode.UserConnected, c.UserName);
                                    }
                                    else
                                    {
                                        byte[] bufferUser = System.Text.Encoding.ASCII.GetBytes((((byte)EMessageCode.InvalidUsername) + "|Admin|User name in use.").ToCharArray());
                                        client.Send(bufferUser, bufferUser.Length, 0);
                                    }
                                }
                                else
                                {
                                    byte[] bufferPass = System.Text.Encoding.ASCII.GetBytes((((byte)EMessageCode.InvalidPassword) + "|Admin|Invalid password.").ToCharArray());
                                    client.Send(bufferPass, bufferPass.Length, 0);
                                }
                                isLooping = false;
                            }
                            break;
                        case EMessageCode.GreatSuccess:
                            {
                                isLooping = false;
                            }
                            break;
                        case EMessageCode.InvalidUsername:
                            {

                                isLooping = false;
                            }
                            break;
                        case EMessageCode.InvalidPassword:
                            {

                                isLooping = false;
                            }
                            break;
                        case EMessageCode.MessageAll:
                            {
                                string m = "";
                                for(int i = 1; i < tokens.Length; i++)
                                {
                                    m += tokens[i] + "|";
                                }
                                this.SentToAllClients(EMessageCode.MessageAll, m);
                                this.txtBoxReader.AppendText(tokens[1] + ": " + tokens[2]);
                                this.logger.WriteLine(tokens[1] + ": " + tokens[2]+"\n");
                                isLooping = false;
                            }
                            break;
                        case EMessageCode.MessageWhisper:
                            {

                                isLooping = false;
                            }
                            break;
                        case EMessageCode.MessageFile:
                            {
                                isLooping = false;
                            }
                            break;
                        case EMessageCode.ServerCommand:
                            {
                                isLooping = false;
                            }
                            break;
                        case EMessageCode.DropConnection:
                            {

                                isLooping = false;
                            }
                            break;
                        case EMessageCode.UserDisconnect:
                            {
                                //this.listBoxUsers.BeginUpdate();
                                //this.listBoxUsers.Items.Remove(tokens[1]);
                                //this.listBoxUsers.EndUpdate();
                                this.txtBoxReader.AppendText(tokens[1] + " has left the server.\n");
                                this.logger.WriteLine(tokens[1] + " has left the server.\n");
                                isLooping = false;
                            }
                            break;
                        case EMessageCode.UserConnected:
                            {
                                //this.listBoxUsers.BeginUpdate();
                                //this.listBoxUsers.Items.Add(tokens[1]);
                                //this.listBoxUsers.EndUpdate();
                                this.txtBoxReader.AppendText(tokens[1] + " has joined the server.\n");
                                this.logger.WriteLine(tokens[1] + " has joined the server.\n");
                                isLooping = false;
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private void SentToAllClients(EMessageCode code, string message)
        {
            foreach(KeyValuePair<string,LClient> entry in this.clientsDict)
            {
                this.SendToClient(entry.Value, code, message);
            }
        }

        private void SendToClient(LClient cl, EMessageCode code,  string message)
        {
            try
            {
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes((((char)code) + "|" + message + "|").ToCharArray());
                cl.MySocket.Send(buffer, buffer.Length, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                cl.MySocket.Close();
                cl.MyThread.Abort();
                this.clientsDict.Remove(cl.UserName);
                this.listBoxUsers.Items.Remove(cl.UserName + " : " + cl.Host.ToString());
            }
            
        }

        #region credentials

        private bool CheckCredentials(string username, string password)
        {
            foreach (KeyValuePair<string, string> entry in this.credentials)
            {
                if(entry.Key == username)
                {
                    //returns true if credentials are right.
                    if (entry.Value == password)
                        return true;

                    //wrong password
                    return false;
                }
            }

           
            //username doesn't exist so i create a user
            this.CreateCredential(username, password);

            //returns true since it is a new user.
            return true;
        }

        private void CreateCredential(string username, string password)
        {
            if (!Directory.Exists("Credentials"))
                Directory.CreateDirectory("Credentials");
            string fname = "Credentials\\" + "users" + ".cred";
            try
            {
                using (StreamWriter write = new StreamWriter(new FileStream(fname, FileMode.Append, FileAccess.Write)))
                {
                    //i am aware this isn't secure but it is temporary
                    write.WriteLine(username + '|' + password + "|\n");
                }
            }
            catch(IOException e)
            {
                Console.WriteLine(e);
                Console.WriteLine("error creating credential.");
            }

            this.credentials.Add(username, password);
        }

        private void LoadCredential()
        {
            string fname = "Credentials\\" + "users" + ".cred";
            try
            { 
                if (!Directory.Exists("Credentials"))
                    Directory.CreateDirectory("Credentials");

                if (!File.Exists(fname))
                    this.CreateCredential("your", "mom");

                using(StreamReader reader = new StreamReader(fname))
                {
                    string line = "";
            
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line != "")
                        {
                            string[] users = line.Split(new Char[] { '|' });
                            if (!this.credentials.ContainsKey(users[0]))
                                this.credentials.Add(users[0], users[1]);
                        }
                    }
                }
                }
            catch(IOException e)
            {
                Console.WriteLine(e);
                Console.WriteLine("error loading credentials.");
            }

        }

        #endregion

        private void Shutdown()
        {
            this.isRunning = false;
            listener.Stop();

            foreach(LClient c in clients)
            {
                c.MySocket.Disconnect(false);
                if(c.MyThread != null && c.MyThread.IsAlive)
                    c.MyThread.Abort();
            }

            clients.Clear();

            if (logger != null)
            {
                logger.Flush();
                logger.Close();
                logger = null;
            }

            if (this.clientService != null)
            {
                if (this.clientService.IsAlive)
                    this.clientService.Abort();
                this.clientService = null;
            }

            if(this.thread != null)
            {
                if (this.thread.IsAlive)
                    this.thread.Abort();
                this.thread = null;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Shutdown();
            base.OnClosing(e);
        }
    }
}
