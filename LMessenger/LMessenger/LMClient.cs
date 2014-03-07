#define DEBUG_SERVER

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

namespace LMessenger
{
    /// <summary>
    /// client form for connecting to server
    /// </summary>
    public partial class LMClient : Form
    {

        #region declarations
        private bool isConnected = false;
        private bool wasKicked = false;
        private NetworkStream netStream;
        private StreamReader reader;
        private StreamWriter writer;
        private TcpClient client;
        private Thread thread;
        private EMessageMode mode = EMessageMode.All;
        private const int messageSize = 1024;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public LMClient()
        {
            InitializeComponent();
            this.EnableConnectBtn();
            this.ToggleElements();


            //adding combo box items for message types
            this.comboBoxMessageType.BeginUpdate();
            this.comboBoxMessageType.Items.Add(EMessageMode.All.ToString());
            this.comboBoxMessageType.Items.Add(EMessageMode.Whisper.ToString());
            this.comboBoxMessageType.Items.Add(EMessageMode.File.ToString());
            this.comboBoxMessageType.EndUpdate();

            this.comboBoxMessageType.SelectedItem = this.comboBoxMessageType.Items[0];
            if (!Directory.Exists("RecievedFiles"))
                Directory.CreateDirectory("RecievedFiles");

            //got tired of entering in values
#if DEBUG_SERVER

            this.txtBoxPassword.Text = "pass";
            this.txtBoxServerIP.Text = "192.168.2.101";
            this.txtBoxUsername.Text = "User";
            this.txtBoxServerPort.Text = "1337";
            this.btnConnect.Enabled = true;

#endif
        }

        #region windows form methods

        #region delegates
        delegate void SetProgressBarCallback(ProgressBar obj, int value);
        delegate void SetBoolCallback(Control obj, bool value);
        delegate void SetTextCallback(Control obj, string text);
        delegate void AddListBoxCallback(ListBox obj, string text);
        delegate void RemoveListBoxCallback(ListBox obj, string text);
        delegate void SetAppendTextCallback(TextBoxBase obj, string text);

        private void SetProgressBar(ProgressBar obj, int value)
        {
            if (obj.InvokeRequired)
            {
                SetProgressBarCallback tcb = new SetProgressBarCallback(SetProgressBar);
                this.Invoke(tcb, new Object[] { obj, value });
            }
            else
            {
                obj.Value = value;
            }
        }

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

        private void SetText(Control obj, string text)
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

        private void SetBool(Control obj, bool value)
        {
            if (obj.InvokeRequired)
            {
                SetBoolCallback tcb = new SetBoolCallback(SetBool);
                this.Invoke(tcb, new Object[] { obj, value });
            }
            else
            {
                obj.Enabled = value;
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
        /// clicked the connect button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConnect_Click(object sender, EventArgs e)
        {
            //if not connected establish tonnection
            if(!this.isConnected)
            {
                this.EstablishConnection();

                //if connection successful register on server
                if(this.isConnected)
                {
                    this.thread = new Thread(new ThreadStart(Recieve));
                    //log into server
                    RegisterOnServer();
                    //toggle ui
                    this.ToggleElements();
                    //start recieve thread
                    this.thread.Start();
                }
            }
            else
            {
                //shutdown connection
                this.Shutdown();
            }            
        }

        /// <summary>
        /// clicked the send message button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSendMessage_Click(object sender, EventArgs e)
        {
            this.MessageButtonActivated();
        }

        /// <summary>
        /// message text box handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtBoxMessage_KeyDown(object sender, KeyEventArgs e)
        {
            //if enter was not hit return
            if (e.KeyCode != Keys.Return) return;

            //activate message button
            this.MessageButtonActivated();
        }

        #endregion


        /// <summary>
        /// Updates reader text box and appends log.
        /// </summary>
        /// <param name="text"></param>
        private void UpdateReader(string text)
        {
            this.SetAppendText(this.txtBoxDisplay, text + "\n");
            this.writer.Write(text + "\n");
        }

        /// <summary>
        /// message button activated. checks mode and sends messages
        /// </summary>
        private void MessageButtonActivated()
        {
            //check what mode the combo box is using
            switch (this.comboBoxMessageType.SelectedItem.ToString())
            {
                case "All":
                    {
                        //sends the message to the server for all to see
                        this.Message(EMessageCode.MessageAll, this.txtBoxMessage.Text);
                    }
                    break;
                case "Whisper":
                    {
                        //tests if a client is selected
                        string temp = GetSelectedClient();
                        if (temp == "") return;

                        //whispers that client only
                        this.Whisper(temp, this.txtBoxMessage.Text);
                    }
                    break;
                case "File":
                    {
                        //tests if a client was selected
                        string temp = GetSelectedClient();
                        //return if no user selected or if you have yourself selected
                        if (temp == "" || temp == this.txtBoxUsername.Text) return;

                        //send a file to the user
                        this.SendFile(this.txtBoxMessage.Text);
                    }
                    break;
            }

            //reset message box text
            this.SetText(this.txtBoxMessage, "");
        }

        /// <summary>
        /// sends a file to the selected user
        /// </summary>
        /// <param name="fileLocationPlusName"></param>
        private void SendFile(string fileLocationPlusName)
        {
            //filename
            string fname = fileLocationPlusName;
            //gets user
            string user = GetSelectedClient();
            //redudant check 
            if (user == "" || user == this.txtBoxUsername.Text) return;
            

            try
            {
                //file doesnt exist.
                if (!File.Exists(fname))
                    return;

                //create buffer of 1kb
                Byte[] buffer = new  Byte[messageSize];

                //check the text box for the file to be sent, split the string.
                string[] words = this.txtBoxMessage.Text.Split(new Char[] { '\\' });

                //iterate to the last element of words;
                int i = 0;
                for (i = 0; i < words.Length; i++ ) { }

                //start filestream
                using (FileStream reader = new FileStream(fname, FileMode.Open, FileAccess.Read))
                {
                    //save length of the file
                    long l = reader.Length;
                    //v = value used for reading return of Read
                    int v = 0;

                    //send a message to the target user that you want to send a file to them.
                    this.Message(EMessageCode.MessageFile, user + "|" + l + "|" + words[i - 1]);
                    //pause for server to send message to other client
                    System.Threading.Thread.Sleep(20);

                    //calculate the interval for progress bar.
                    float interval = (1.0f / reader.Length) * messageSize;
                    //counter to be used with interval
                    float counter = 0;

                    //while the file reader has data continue
                    while (( v = reader.Read(buffer, 0, messageSize)) > 0)
                    {
                        //increment my counter with interval
                        counter += interval;

                        //sets progress bar to teh counter value
                        this.SetProgressBar(this.progressBar, (int)(counter * 100));

                        //write buffer to the netstream.
                        this.netStream.Write(buffer, 0, messageSize);
                        //this.netStream.Flush();

                        //if counter > 1 then too much data is being sent
                        if (counter >= 1)
                            break;

                        //length -= amount of bytes transfered
                        l -= v;
                    }
                    //moment of silence for a second.
                    System.Threading.Thread.Sleep(1000);
                    //ok get rid of it
                    reader.Dispose();
                }
                //reset progress bar
                this.SetProgressBar(this.progressBar, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Sending file.\n" + e );
            }
        }

        /// <summary>
        /// recieves data sent and saves in recieved files.
        /// </summary>
        /// <param name="length">of file in bytes</param>
        /// <param name="filename">name of file</param>
        private void RecieveFile(long length, string filename)
        {
            //if file length is 0 do not pass go.
            if (length == 0) return;

            //destination folder
            string fname = "RecievedFiles\\" + filename;


            Byte[] data = new Byte[messageSize];
            //length of file
            long len = length;
            //value of bytes completed.
            int value = 0;
            //percentage interval of completion each loop
            float interval = (1.0f / length) * messageSize;
            //counter for interval
            float counter = 0;


            try
            {
                //start stream
                using (FileStream write = new FileStream(fname, FileMode.Create, FileAccess.ReadWrite))
                {
                    while ((value = this.client.Client.Receive(data, 0, messageSize, SocketFlags.None)) > 0 && len > 0)
                    {
                        //increment the counter.
                        counter += interval;
                        //update progressbar.
                        this.SetProgressBar(this.progressBar, (int)(counter * 100));

                        //write data to buffer
                        write.Write(data, 0, messageSize);
                        //write buffer to disk
                        write.Flush();

                        //safety check
                        if (counter >= 1)
                            break;

                        //decrease size of length by amount completed this loop
                        len -= value;
                    }
                    //take a little nap
                    System.Threading.Thread.Sleep(300);
                    write.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Reciving file." + e);
            }

            //reset progress bar to 0.
            this.SetProgressBar(this.progressBar, 0);
        }

        /// <summary>
        /// checks the list box for selected user
        /// </summary>
        /// <returns></returns>
        private string GetSelectedClient()
        {
            string selected = "";

            //if user found return value 
            if (this.listBoxUsers.SelectedIndex >= 0)
                selected = (string)this.listBoxUsers.Items[this.listBoxUsers.SelectedIndex];

            return selected;
        }

        /// <summary>
        /// establish connection to server.
        /// </summary>
        private void EstablishConnection()
        {
            this.SetText(this.labelStatus, "Connecting...");

            try
            {
                //establish new client
                this.client = new TcpClient(this.txtBoxServerIP.Text, Convert.ToInt32(this.txtBoxServerPort.Text));
                //stream for quick reference
                this.netStream = client.GetStream();
                //remove delay
                this.client.NoDelay = true;
                this.reader = new StreamReader(this.netStream);
                this.isConnected = true;
                this.wasKicked = false;
                this.btnConnect.Text = "Disconnect";
            }
            catch(Exception e)
            {
                Console.WriteLine("ERROR: Could not connect to server.");
                this.SetText(this.labelStatus, "ERROR: Could not connect to server.");
                //call shutdown to reset all values
                this.Shutdown();
            }
        }

        /// <summary>
        /// binds with server and sends login data
        /// </summary>
        private void RegisterOnServer()
        {
            try
            {
                //message that you want to connect
                this.Message(EMessageCode.HandShake, this.txtBoxPassword.Text);
            }
            catch(Exception e)
            {
                Console.WriteLine("ERROR: Could not register to server.\n\nError: " + e);
                this.SetText(this.labelStatus, "ERROR: Could not register to server.");
                this.Shutdown();
            }
        }

        /// <summary>
        /// sends a message over network through server
        /// </summary>
        /// <param name="code">type of message</param>
        /// <param name="message">message contents</param>
        private void Message(EMessageCode code, string message)
        {
            try
            {
                //create string for message
                string m = ((char)code) + "|" + this.txtBoxUsername.Text + "|" + message + "|";
                //load string into buffer
                Byte[] buffer = System.Text.Encoding.ASCII.GetBytes(m.ToCharArray());

                //encrypt buffer
                buffer = this.EncrptyDecrypt(buffer);

                //write buffer to stream
                this.netStream.Write(buffer, 0, buffer.Length);
            }
            catch(Exception e)
            {
                this.SetText(this.labelStatus, "ERROR: Failed to send message.");
                Console.WriteLine("ERROR: Unable to send message");
            }
        }

        /// <summary>
        /// encrypts message by inverting bits
        /// </summary>
        /// <param name="buffer">buffer to encrypr</param>
        /// <returns></returns>
        private Byte[] EncrptyDecrypt(Byte[] buffer)
        {
            //load buffer into temp buffer;
            Byte[] buf = new Byte[buffer.Length];
            //get the length of buffer before null elements
            int a = 0;
            for (a = 0; a < buffer.Length && buffer[a] != '\0'; a++ ) { }
            
            //reduce a one more to get element before nulls
            a--;
            if (a < 1)
                a = 1;

            //loop through buffer and invert bits
            for (int i = 0; i < buffer.Length && buffer[i] != '\0'; i++)
            {
                this.SetProgressBar(this.progressBar, (int)((i / (float)a) * 100));
                byte b = (byte)buffer[i];
                buf[i] = (Byte)~b; 
            }

            //reset progress bar
            this.SetProgressBar(this.progressBar, 0);

            //return the buffer
            return buf;
        }

        /// <summary>
        /// sends private message
        /// </summary>
        /// <param name="target"></param>
        /// <param name="message"></param>
        private void Whisper(string target, string message)
        {
            this.Message(EMessageCode.MessageWhisper, target + '|' + message);
            this.UpdateReader(target + " : " + message);
        }

        /// <summary>
        /// primary loop
        /// </summary>
        private void Recieve()
        {
            while (this.isConnected)
            {
                try
                {
                    //if you were kicked get out.
                    if (this.wasKicked) 
                        return;

                    //create buffer
                    Byte[] buffer = new Byte[messageSize];
                    //read bufer
                    this.netStream.Read(buffer, 0, buffer.Length);

                    //encrypt buffer
                    buffer = this.EncrptyDecrypt(buffer);

                    //set buffer to string
                    string message = System.Text.Encoding.ASCII.GetString(buffer);

                    //split word up into tokens
                    string[] tokens = message.Split(new Char[] { '|' });


                    //go through switch
                    switch ((EMessageCode)tokens[0][0])
                    {
                        case EMessageCode.None:
                            {
                                //nothing
                            }
                            break;
                        case EMessageCode.HandShake:
                            {
                                ///connecting to server
                                this.SetText((Control)this.labelStatus, "Connected...");
                                this.SetText((Control)this,  "LMessenger - Connected as: " + this.txtBoxUsername.Text);
                                this.LoggingStart();
                            }
                            break;
                        case EMessageCode.GreatSuccess:
                            {
                                //success
                                this.SetText((Control)this.labelStatus, "Great Success...");
                            }
                            break;
                        case EMessageCode.InvalidUsername:
                            {
                                this.SetText((Control)this.labelStatus, "Invalid Username...");
                                MessageBox.Show("Invalid Username", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                this.isConnected = false;
                            }
                            break;
                        case EMessageCode.InvalidPassword:
                            {
                                this.SetText((Control)this.labelStatus, "Invalid Password...");
                                MessageBox.Show("Invalid password", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                this.isConnected = false;                        
                            }
                            break;
                        case EMessageCode.MessageAll:
                            {
                                this.SetAppendText(this.txtBoxDisplay, tokens[1] + " says: " + tokens[2] + "\n");
                                this.writer.WriteLine(tokens[1] + " says: " + tokens[2] + "\n");
                            }
                            break;
                        case EMessageCode.MessageWhisper:
                            {
                                this.SetAppendText(this.txtBoxDisplay,"Whisper from " + tokens[1] + ": " + tokens[2] + "\n");
                                this.writer.WriteLine("Whisper from " + tokens[1] + ": " + tokens[2] + "\n");
                            }
                            break;
                        case EMessageCode.MessageFile:
                            {
                                this.RecieveFile(Convert.ToInt64(tokens[3]), tokens[4]);
                            }
                            break;
                        case EMessageCode.ServerCommand:
                            {
                                
                            }
                            break;
                        case EMessageCode.DropConnection:
                            {
                                this.wasKicked = true;
                                this.Shutdown();
                                MessageBox.Show("Lost Connection\n" + tokens[2], "Kicked", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                this.isConnected = false;
                            }
                            break;
                        case EMessageCode.UserDisconnect:
                            {
                                this.RemoveListBox(this.listBoxUsers, tokens[1]);
                                this.SetAppendText(this.txtBoxDisplay, tokens[1] + " has left the server.\n");
                                this.writer.WriteLine(tokens[1] + " has left the server.\n");
                            }
                            break;
                        case EMessageCode.UserConnected:
                            {
                                this.AddListBox(this.listBoxUsers, tokens[2]);
                                this.SetAppendText(this.txtBoxDisplay, tokens[2] + " has joined the server.\n");
                                this.writer.WriteLine(tokens[2] + " has joined the server.\n");
                            }
                            break;
                        case EMessageCode.UsersOnline:
                            {
                                this.AddListBox(this.listBoxUsers, tokens[2]);
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            this.Shutdown();
        }

        /// <summary>
        /// activates logging
        /// </summary>
        private void LoggingStart()
        {
            if (!Directory.Exists("logs"))
                Directory.CreateDirectory("logs");
            string fname = "logs\\" + DateTime.Now.ToString("dd - MMM - yyyy - HHmm") + ".txt";
            this.writer = new StreamWriter(new FileStream(fname, FileMode.OpenOrCreate,
                FileAccess.Write));
        }

        /// <summary>
        /// tests if all the text fields are full. If they are then the connect button is enabled.
        /// </summary>
        private void EnableConnectBtn()
        {
            bool v = this.txtBoxUsername.Text != "" &&
                     this.txtBoxPassword.Text != "" &&
                     this.txtBoxServerIP.Text != "" &&
                     this.txtBoxServerPort.Text != "";

            this.SetBool(this.btnConnect, v);
        }

        /// <summary>
        /// toggles elements of the window based on isConnected.
        /// </summary>
        private void ToggleElements()
        {
            this.SetBool(this.txtBoxPassword, !this.isConnected);
            this.SetBool(this.txtBoxServerIP, !this.isConnected);
            this.SetBool(this.txtBoxUsername, !this.isConnected);
            this.SetBool(this.txtBoxServerPort, !this.isConnected);
            this.SetBool(this.txtBoxMessage, this.isConnected);
            this.SetBool(this.btnSendMessage, this.isConnected);
            this.SetBool(this.comboBoxMessageType, this.isConnected);
            this.SetBool(this.listBoxUsers, this.isConnected);
        }

        #region text changed

        /// <summary>
        /// monitors character changes in text field
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtBoxServerIP_TextChanged(object sender, EventArgs e)
        {
            this.EnableConnectBtn();
        }
        /// <summary>
        /// monitors character changes in text field
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtBoxServerPort_TextChanged(object sender, EventArgs e)
        {
            //checks state of text fields, sets connect fo false if all empty
            this.EnableConnectBtn();
        }

        /// <summary>
        /// monitors character changes in text field
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtBoxUsername_TextChanged(object sender, EventArgs e)
        {
            //checks state of text fields, sets connect fo false if all empty
            this.EnableConnectBtn();
        }

        /// <summary>
        /// monitors character changes in text field
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtBoxPassword_TextChanged(object sender, EventArgs e)
        {
            //checks state of text fields, sets connect fo false if all empty
            this.EnableConnectBtn();
        }

        #endregion

        #region Close / Disconnect

        /// <summary>
        /// cleans up anything with networking
        /// </summary>
        private void Shutdown()
        {
            
            if (this.netStream != null)
            {
                //if(!this.wasKicked)
                this.Message(EMessageCode.UserDisconnect, "Bye.");
                System.Threading.Thread.Sleep(50);
                this.netStream.Dispose();
                this.netStream.Close(100);
                this.netStream = null;
            }

            if(this.reader != null)
            {
                this.reader.Dispose();
                this.reader.Close();
                this.reader = null;
            }

            if (this.writer != null)
            {
                this.writer.Dispose();
                this.writer.Close();
                this.writer = null;
            }

            

            if (this.client != null)
            {
                this.client.Close();
                this.client = null;
            }

            this.isConnected = false;
            this.ToggleElements();


            for(int i = this.listBoxUsers.Items.Count - 1; i >= 0; i--)
            {
                this.RemoveListBox(this.listBoxUsers, this.listBoxUsers.Items[i].ToString());
            }
            
            this.SetText(this.btnConnect, "Connect");
            this.SetText(this.labelStatus, "Disconnected..."); 
        }

        /// <summary>
        /// when closing do this stuff
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            //ensure network is shutdown
            this.Shutdown();

            if (this.thread != null)
            {
                if (this.thread.IsAlive)
                    this.thread.Abort();
                this.thread = null;
            }
            base.OnClosing(e);
        }

        #endregion

        
    }
}
