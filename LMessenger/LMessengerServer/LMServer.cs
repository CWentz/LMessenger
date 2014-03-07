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

        #region declarations
        private bool isRunning = false;
        private const int port = 1337;
        private TcpListener listener;
        private Socket socket;
        private Thread thread;
        private Thread clientService;
        private StreamWriter logger;
        private Dictionary<string, string> credentials;
        private Dictionary<string, LClient> clientsDict;
        #endregion

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

        private void UpdateReader(string text)
        {
            this.logger.Write(text + Environment.NewLine);
            this.SetAppendText(this.txtBoxReader, text + Environment.NewLine);
        }

        /// <summary>
        /// clicked send command button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSendCommand_Click(object sender, EventArgs e)
        {
            if (!this.isRunning) return;

           
               
            switch(this.comboBoxMessageType.SelectedItem.ToString())
            {
                case "All":
                    this.SendToAllClients(EMessageCode.MessageAll, this.txtBoxCommand.Text);
                    break;
                case "Whisper":
                    {
                        LClient c = GetSelectedClient();
                        if (c == null) return;

                        this.SendToClient(c, EMessageCode.MessageWhisper, this.txtBoxCommand.Text);
                    }
                    break;
                case "File":
                    break;
                case "Kick":
                    {
                        LClient c = GetSelectedClient();
                        if (c == null) return;

                        this.SendToClient(c, EMessageCode.DropConnection, this.txtBoxCommand.Text);
                    }
                    break;

            }

            this.UpdateReader( this.comboBoxMessageType.SelectedItem.ToString() + " : Admin : "+ this.txtBoxCommand.Text + Environment.NewLine);

            System.Threading.Thread.Sleep(100);
            this.SetText(this.txtBoxCommand, "");    
        }

        private LClient GetSelectedClient()
        {
            string tempWhisper = "";
            string selected = "";
            if (this.listBoxUsers.SelectedIndex >= 0)
                selected = (string)this.listBoxUsers.Items[this.listBoxUsers.SelectedIndex];

            if (selected == "") return null;

            foreach (KeyValuePair<string, LClient> entry in this.clientsDict)
            {
                if (selected == entry.Value.LogData())
                {
                    tempWhisper = entry.Key;
                }
            }
            if (this.clientsDict.ContainsKey(tempWhisper))
                return this.clientsDict[tempWhisper];
            return null;
        }

        private Byte[] EncrptyDecrypt(Byte[] buffer)
        {
            Byte[] buf = new Byte[buffer.Length];

            for (int i = 0; i < buffer.Length && buffer[i] != '\0'; i++)
            {
                byte b = (byte)buffer[i];
                buf[i] = (Byte)~b; 
            }

            return buf;
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
                FileAccess.ReadWrite));
        }

        private void DeployServer()
        {
            this.clientsDict = new Dictionary<string, LClient>();
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
            bool loop = true;

            while (loop && c.Connected)
            {
                try
                {
                    this.Recieve(c);
                }
                catch(Exception e)
                {
                    loop = false;
                }
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

                    buffer = this.EncrptyDecrypt(buffer);
                    
                    string message = System.Text.Encoding.ASCII.GetString(buffer);

                    string[] tokens = message.Split(new Char[] { '|' });

                    EMessageCode code = (EMessageCode)tokens[0][0];


                    if(code != EMessageCode.None)
                        this.LogMessage(tokens);

                    switch (code)
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
                                        //get endpoint for creating an LClient
                                        EndPoint ep = client.RemoteEndPoint;
                                        LClient c = new LClient(tokens[1],ep,clientService, client);

                                        //add client into dictionary
                                        this.clientsDict.Add(c.UserName, c);

                                        //message client a handshake
                                        this.SendToClient(c, EMessageCode.HandShake, "Very nice");
                                        
                                        //add the client data to the list box.
                                        this.AddListBox(this.listBoxUsers, c.LogData());

                                        //update log reader
                                        this.UpdateReader(c.UserName + " has Connected.");

                                        //pause for the client to setup before receiving next message.
                                        System.Threading.Thread.Sleep(250);
                                        
                                        //send messages to all connected clients that a new client is connected.
                                        //also sends a message to the client that connected with all connected clients.
                                        foreach(KeyValuePair<string,LClient> entry in this.clientsDict)
                                        {
                                            this.SendToClient(entry.Value, EMessageCode.UserConnected, c.UserName);
                                            this.SendToClient(c, EMessageCode.UsersOnline, entry.Value.UserName);
                                            System.Threading.Thread.Sleep(50);
                                        }
                                    }
                                    else
                                    {
                                        //mulitple users logged in with the name. rejects connection.
                                        string doubleTemp = "DOUBLE ACCESS:\nAttempting access: " + tokens[1] + "Current Logged in User: " + this.clientsDict[tokens[1]].MySocket.RemoteEndPoint.ToString() + Environment.NewLine + "Attempting Access: " + client.RemoteEndPoint.ToString();
                                        //log warning added
                                        this.UpdateReader(doubleTemp);
                                        byte[] bufferUser = System.Text.Encoding.ASCII.GetBytes((((byte)EMessageCode.InvalidUsername) + "|Admin|User name in use.").ToCharArray());
                                        client.Send(bufferUser, bufferUser.Length, 0);
                                    }
                                }
                                else
                                {
                                    //user tried to log in with invalid password. rejects connection obviously.
                                    string passTemp = "INVALID PASSWORD: \n" + "User: " + tokens[1] + Environment.NewLine + "IP: " + client.RemoteEndPoint.ToString();
                                    //log warning added
                                    this.UpdateReader(passTemp);
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
                                
                                for(int i = 2; i < tokens.Length - 1; i++)
                                {
                                    m += tokens[i] + "|";
                                }

                                this.RelayToAllClients(EMessageCode.MessageAll, tokens[1], m);

                                isLooping = false;
                            }
                            break;
                        case EMessageCode.MessageWhisper:
                            {
                                string m = "";

                                for (int i = 2; i < tokens.Length - 1; i++)
                                {
                                    m += tokens[i] + "|";
                                }

                                //this.UpdateReader();
                                //this.SetAppendText(this.txtBoxReader, tokens[1] + " : " + client.RemoteEndPoint.ToString() + " whispers to " + tokens[2] + ": " + m);
                                //this.logger.WriteLine(tokens[1] + " : " + client.RemoteEndPoint.ToString() + " whispers to " + tokens[2] + ": " + m + Environment.NewLine);
                                if (this.clientsDict.ContainsKey(tokens[2]))
                                {
                                    this.RelayToClient(this.clientsDict[tokens[2]], EMessageCode.MessageWhisper, tokens[1], m);
                                }
                                isLooping = false;
                            }
                            break;
                        case EMessageCode.MessageFile:
                            {
                                this.RelayFile(this.clientsDict[tokens[1]], this.clientsDict[tokens[2]], Convert.ToInt64(tokens[3]), tokens[4]);
                                //this.RecieveFile(this.clientsDict[tokens[1]], this.clientsDict[tokens[2]], Convert.ToInt64(tokens[3]), tokens[4]);
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
                                this.RelayToAllClients(EMessageCode.UserDisconnect, tokens[1], "bye bye\n");
                                this.UpdateReader(tokens[1] + " has left the server.");
                                if (this.clientsDict.ContainsKey(tokens[1]))
                                {
                                    this.RemoveListBox(this.listBoxUsers, this.clientsDict[tokens[1]].LogData());
                                    this.clientsDict[tokens[1]].Shutdown();
                                    this.clientsDict.Remove(tokens[1]);
                                }
                                isLooping = false;
                            }
                            break;
                        case EMessageCode.UserConnected:
                            {
                                this.UpdateReader(tokens[1] + " has joined the server.");
                                isLooping = false;
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    isLooping = false;
                }
            }
        }

        private void LogMessage(string[] inputMessage)
        {
            string tempM = "";

            tempM += ((EMessageCode)inputMessage[0][0]).ToString() + " : ";

            for(int i = 1; i < inputMessage.Length - 2; i++)
            {
                tempM += inputMessage[i] + " : ";
            }
            tempM += inputMessage[inputMessage.Length - 2];

            this.UpdateReader(tempM);
        }

        private void SendToAllClients(EMessageCode code, string message)
        {
            if (this.clientsDict == null || this.clientsDict.Count == 0) return;

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

                buffer = this.EncrptyDecrypt(buffer);

                cl.MySocket.Send(buffer, buffer.Length, 0);
            }
            catch (Exception e)
            {
                this.RemoveListBox(this.listBoxUsers, cl.LogData());
                cl.Shutdown();
                this.clientsDict.Remove(cl.UserName);
            }
        }

        private void RelayToClient(LClient cl, EMessageCode code, string sender, string message)
        {
            try
            {
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes((((char)code) + "|" + sender + "|" + message + "|").ToCharArray());

                buffer = this.EncrptyDecrypt(buffer);

                cl.MySocket.Send(buffer, buffer.Length, 0);
            }
            catch (Exception e)
            {
                this.RemoveListBox(this.listBoxUsers, cl.LogData());
                cl.Shutdown();
                this.clientsDict.Remove(cl.UserName);
            }
        }

        private void RelayToAllClients(EMessageCode code, string sender, string message)
        {
            if (this.clientsDict == null || this.clientsDict.Count == 0) return;

            foreach (KeyValuePair<string, LClient> entry in this.clientsDict)
            {
                this.RelayToClient(entry.Value, code, sender, message);
            }
        }

        #region credentials & files

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

        private void RecieveFile(LClient client, long length, string filename)
        {
            //client agreed
            int max = 1024;
            Byte[] data = new Byte[max];
            long l = length;
            if (!Directory.Exists("RecievedFiles"))
                Directory.CreateDirectory("RecievedFiles");
            string fname = "RecievedFiles\\" + filename;

            try
            {
                using (FileStream write = new FileStream(fname, FileMode.Create, FileAccess.ReadWrite))
                {
                    while (client.MySocket.Receive(data, 0, max, SocketFlags.None) > 0 && l > 0)
                    {
                        write.Write(data, 0, max);
                        write.Flush();
                        l -= data.Length;
                    }
                    System.Threading.Thread.Sleep(300);
                    write.Close();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error creating file:\n\n" + e, "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }


            this.SendToClient(client, EMessageCode.GreatSuccess, (char)EMessageCode.MessageFile + "|" + (char)EMessageCode.GreatSuccess);

        }

        private void SendFile(LClient client, string fileLocationPlusName)
        {
            string fname = fileLocationPlusName;

            try
            {
                //file doesnt exist.
                if (!File.Exists(fname))
                    return;


                Byte[] buffer = new Byte[1024];

                string[] words = fname.Split(new Char[] { '\\' });

                int i = 0;
                for (i = 0; i < words.Length; i++) { }


                using (FileStream reader = new FileStream(fname, FileMode.Open, FileAccess.Read))
                {
                    long l = reader.Length;

                    this.SendToClient(client, EMessageCode.MessageFile, "Admin|" + l + "|" + words[i - 1]);

                    while (reader.Read(buffer, 0, 1024) > 0)
                    {
                        client.MySocket.Send(buffer, 0, 1024, SocketFlags.None);
                    }
                    System.Threading.Thread.Sleep(1000);
                    reader.Dispose();
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Error reading file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private void RelayFile(LClient clientSender, LClient clientReciever, long length, string filename)
        {
            try
            {
                long le = length;
                long countR = 0;
                long countS = 0;
                Byte[] buffer = new Byte[1024];

                this.SendToClient(clientReciever, EMessageCode.MessageFile, clientSender.UserName + "|" + length + "|" + filename);

                while (countR < length)
                {
                    if (clientSender.MySocket.Receive(buffer) > 0)
                        countR += clientReciever.MySocket.Send(buffer);
                }
            }
            catch(SocketException e)
            {
                Console.WriteLine("RelayFile FAILED");
            }
        }


        #endregion

        private void Shutdown()
        {
            this.isRunning = false;


            this.SendToAllClients(EMessageCode.DropConnection, "Server shutdown");
            System.Threading.Thread.Sleep(500);

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

        private void txtBoxCommand_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Return) return;

            if (!this.isRunning) return;

            switch (this.comboBoxMessageType.SelectedItem.ToString())
            {
                case "All":
                    this.SendToAllClients(EMessageCode.MessageAll, this.txtBoxCommand.Text);
                    break;
                case "Whisper":
                    {
                        LClient c = GetSelectedClient();
                        if (c == null) return;

                        this.SendToClient(c, EMessageCode.MessageWhisper, this.txtBoxCommand.Text);
                    }
                    break;
                case "File":
                    break;
                case "Kick":
                    {
                        LClient c = GetSelectedClient();
                        if (c == null) return;

                        this.SendToClient(c, EMessageCode.DropConnection, this.txtBoxCommand.Text);
                    }
                    break;

            }

            this.UpdateReader(this.comboBoxMessageType.SelectedItem.ToString() + " : Admin : " + this.txtBoxCommand.Text + Environment.NewLine);

            System.Threading.Thread.Sleep(100);
            this.SetText(this.txtBoxCommand, "");    

        }
    }
}
