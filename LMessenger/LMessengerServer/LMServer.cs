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
        //private List<LClient> clients;
        private Socket socket;
        private Thread thread;
        private Thread clientService;
        private StreamWriter logger;
        private Dictionary<string, string> credentials;
        private Dictionary<string, LClient> clientsDict;
        /// <summary>
        /// constructor
        /// </summary>
        /// 

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

        #region delegates

        delegate void SetTextCallback(Control obj, string text);
        delegate void AddListBoxCallback(ListBox obj, string text);
        delegate void RemoveListBoxCallback(ListBox obj, string text);
        delegate void SetAppendTextCallback(TextBoxBase obj, string text);

        private void AddListBox(ListBox obj, string text)
        {
            if (obj.InvokeRequired)
            {
                AddListBoxCallback tcb = new AddListBoxCallback(AddListBox);
                this.Invoke(tcb, new Object[] { obj, text });
            }
            else
            {
                if (obj.Items.Contains(text)) return;
                obj.BeginUpdate();
                obj.Items.Add(text);
                obj.EndUpdate();
            }
        }

        private void RemoveListBox(ListBox obj, string text)
        {
            if (obj.InvokeRequired)
            {
                RemoveListBoxCallback tcb = new RemoveListBoxCallback(RemoveListBox);
                this.Invoke(tcb, new Object[] { obj, text });
            }
            else
            {
                if (!obj.Items.Contains(text)) return;
                obj.BeginUpdate();
                obj.Items.Remove(text);
                obj.EndUpdate();
            }
        }

        void SetText(Control obj, string text)
        {
            if (obj.InvokeRequired)
            {
                SetTextCallback tcb = new SetTextCallback(SetText);
                this.Invoke(tcb, new Object[] { obj, text });
            }
            else
            {
                obj.Text = text;
            }
        }

        private void SetAppendText(TextBoxBase obj, string text)
        {
            if (obj.InvokeRequired)
            {
                SetAppendTextCallback tcb = new SetAppendTextCallback(SetAppendText);
                this.Invoke(tcb, new Object[] { obj, text });
            }
            else
            {
                obj.AppendText(text);
            }
        }

        #endregion

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
                this.SetText(this.btnConnect, "Disconnect");
                this.AddListBox(this.listBoxUsers, "Admin");
                this.DeployServer();
            }
            else
            {
                this.SetText(this.btnConnect, "Deploy");
                this.RemoveListBox(this.listBoxUsers, "Admin");
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
            if(this.isRunning)
            switch(this.comboBoxMessageType.SelectedItem.ToString())
            {
                case "All":
                    this.SendToAllClients(EMessageCode.MessageAll, this.txtBoxCommand.Text);
                    break;
                case "Whisper":
                    {
                        string tempWhisper = "";
                        foreach (KeyValuePair<string, LClient> entry in this.clientsDict)
                        {
                            if((string)this.listBoxUsers.Items[this.listBoxUsers.SelectedIndex] == entry.Value.LogData())
                            {
                                tempWhisper = entry.Key;
                            }
                        }
                        if(this.clientsDict.ContainsKey(tempWhisper))
                            this.SendToClient(this.clientsDict[tempWhisper], EMessageCode.MessageWhisper, this.txtBoxCommand.Text);
                    }
                    break;
                case "File":
                    break;
                case "Kick":
                    {
                        string tempWhisper = "";
                        foreach (KeyValuePair<string, LClient> entry in this.clientsDict)
                        {
                            if ((string)this.listBoxUsers.Items[this.listBoxUsers.SelectedIndex] == entry.Value.LogData())
                            {
                                tempWhisper = entry.Key;
                            }
                        }
                        if (this.clientsDict.ContainsKey(tempWhisper))
                            this.SendToClient(this.clientsDict[tempWhisper], EMessageCode.DropConnection, this.txtBoxCommand.Text);
                    }
                    break;

            }
            this.SetAppendText(this.txtBoxReader, "Admin: "+ this.txtBoxCommand.Text + "\n");
            this.logger.Write("Admin: " + this.txtBoxCommand.Text + "\n");
            System.Threading.Thread.Sleep(100);
            this.SetText(this.txtBoxCommand, "");    
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
            string fname = "logs\\" + DateTime.Now.ToString("dd - MMM - yyyy - HHmm") + ".txt";
            this.logger = new StreamWriter(new FileStream(fname, FileMode.OpenOrCreate,
                FileAccess.Write));
        }

        private void DeployServer()
        {
            this.clientsDict = new Dictionary<string, LClient>();
            //this.clients = new List<LClient>();
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

            while (true)
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
                                //checks credentials
                                if(this.CheckCredentials(tokens[1], tokens[2]))
                                {
                                    //checks if the client name isn't already in use.
                                    if(!this.clientsDict.ContainsKey(tokens[1]))
                                    {
                                        EndPoint ep = client.RemoteEndPoint;
                                        LClient c = new LClient(tokens[1],ep,clientService, client);

                                        this.clientsDict.Add(c.UserName, c);
                                        this.SendToClient(c, EMessageCode.HandShake, "Very nice");
                                        //pause for the client to setup before receiving next message.
                                        System.Threading.Thread.Sleep(500);
                                        this.SendToAllClients(EMessageCode.UserConnected, c.UserName);
                                        this.AddListBox(this.listBoxUsers, c.LogData());
                                    }
                                    else
                                    {
                                        this.logger.Write("DOUBLE ACCESS:\nAttempting access: " + tokens[1] + "Current Logged in User: " + this.clientsDict[tokens[1]].MySocket.RemoteEndPoint.ToString() + "\nAttempting Access: " + client.RemoteEndPoint.ToString());
                                        byte[] bufferUser = System.Text.Encoding.ASCII.GetBytes((((byte)EMessageCode.InvalidUsername) + "|Admin|User name in use.").ToCharArray());
                                        client.Send(bufferUser, bufferUser.Length, 0);
                                    }
                                }
                                else
                                {
                                    this.logger.Write("INVALID PASSWORD: \n" + "User: " +tokens[1] + "\nIP: " + client.RemoteEndPoint.ToString());
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
                                m += "\n";
                                this.RelayToAllClients(EMessageCode.MessageAll, tokens[1], m);
                                this.SetAppendText(this.txtBoxReader, m);
                                this.logger.WriteLine(m);
                                isLooping = false;
                            }
                            break;
                        case EMessageCode.MessageWhisper:
                            {
                                string m = "";
                                for (int i = 2; i < tokens.Length; i++)
                                {
                                    m += tokens[i] + "|";
                                }
                                m += "\n";
                                this.SetAppendText(this.txtBoxReader, tokens[1] + " : " + client.RemoteEndPoint.ToString() + " whispers to " + tokens[2] + ": " + m);
                                this.logger.WriteLine(tokens[1] + " : " + client.RemoteEndPoint.ToString() + " whispers to " + tokens[2] + ": " + m + "\n");
                                if (this.clientsDict.ContainsKey(tokens[2]))
                                {
                                    this.RelayToClient(this.clientsDict[tokens[2]], EMessageCode.MessageWhisper, tokens[1], m);
                                }
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
                                this.SendToAllClients(EMessageCode.UserDisconnect, tokens[1]);
                                this.SetAppendText(this.txtBoxReader, tokens[1] + " has left the server.\n");
                                this.logger.WriteLine(tokens[1] + " has left the server.\n");

                                isLooping = false;
                            }
                            break;
                        case EMessageCode.UserConnected:
                            {
                                this.SetAppendText(this.txtBoxReader, tokens[1] + " has joined the server.\n");
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

        private void SendToAllClients(EMessageCode code, string message)
        {
            foreach(KeyValuePair<string,LClient> entry in this.clientsDict)
            {
                this.SendToClient(entry.Value, code, message);
            }
        }

        private void SendToClient(LClient cl, EMessageCode code, string message)
        {
            try
            {
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes((((char)code) + "|Admin|" + message + "|\n").ToCharArray());
                cl.MySocket.Send(buffer, buffer.Length, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                cl.MySocket.Close();
                cl.MyThread.Abort();
                this.clientsDict.Remove(cl.UserName);
                this.RemoveListBox(this.listBoxUsers, cl.LogData());
            }
        }

        private void RelayToClient(LClient cl, EMessageCode code, string sender, string message)
        {
            try
            {
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes((((char)code) + "|" + sender + "|" + message + "|").ToCharArray());
                cl.MySocket.Send(buffer, buffer.Length, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                cl.MySocket.Close();
                cl.MyThread.Abort();
                this.clientsDict.Remove(cl.UserName);
                this.RemoveListBox(this.listBoxUsers, cl.LogData());
            }
        }

        private void RelayToAllClients(EMessageCode code, string sender, string message)
        {
            foreach (KeyValuePair<string, LClient> entry in this.clientsDict)
            {
                this.RelayToClient(entry.Value, code, sender, message);
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
                MessageBox.Show("Error creating credentials:\n\n" + e, "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
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
                MessageBox.Show("Error Loading credentials:\n\n" + e, "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }

        }

        #endregion

        private void Shutdown()
        {
            if (!this.isRunning) return;
            this.isRunning = false;

            foreach(LClient c in this.clientsDict.Values)
            {
                this.SendToClient(c, EMessageCode.DropConnection, "Server shutdown");
                if(!c.Shutdown())
                {
                    Console.WriteLine(c.UserName + " failed to shutdown.");
                }

            }


            if (this.listener != null)
            {
                listener.Stop();
                listener = null;
            }


            if (this.clientsDict != null)
            {
                this.clientsDict.Clear();
                this.clientsDict = null;
            }

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
