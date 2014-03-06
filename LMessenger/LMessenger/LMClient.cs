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
    public partial class LMClient : Form
    {

        #region declarations
        private bool isConnected = false;

        private NetworkStream netStream;
        private StreamReader reader;
        private StreamWriter writer;
        private TcpClient client;
        private Thread thread;
        private EMessageMode mode = EMessageMode.All;


        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public LMClient()
        {
            InitializeComponent();
            this.EnableConnectBtn();
            this.ToggleElements();
            this.comboBoxMessageType.BeginUpdate();
            this.comboBoxMessageType.Items.Add(EMessageMode.All.ToString());
            this.comboBoxMessageType.Items.Add(EMessageMode.Whisper.ToString());
            this.comboBoxMessageType.Items.Add(EMessageMode.File.ToString());
            this.comboBoxMessageType.EndUpdate();
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
            if(!this.isConnected)
            {
                this.EstablishConnection();
                if(this.isConnected)
                {
                    this.thread = new Thread(new ThreadStart(Recieve));
                    RegisterOnServer();
                    this.ToggleElements();
                    this.thread.Start();
                }
            }
            else
            {
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
            switch (this.comboBoxMessageType.SelectedItem.ToString())
            {
                case "All":
                    {
                        this.Message(EMessageCode.MessageAll, this.txtBoxMessage.Text);
                    }
                    break;
                case "Whisper":
                    {
                        this.Message(EMessageCode.MessageWhisper, this.txtBoxMessage.Text);
                    }
                    break;
                case "File":
                    {
                        this.Message(EMessageCode.MessageFile, this.txtBoxMessage.Text);
                    }
                    break;
            }

            //this.txtBoxDisplay.AppendText(this.txtBoxUsername.Text + ": " +this.txtBoxMessage.Text + "\n");
            this.txtBoxMessage.Text = "";
        }

        private void EstablishConnection()
        {
            this.SetText(this.labelStatus, "Connecting...");

            try
            {
                //TODO: clean up here
                IPAddress ipAdd = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
                IPEndPoint ipEnd = new IPEndPoint(ipAdd, Convert.ToInt32(this.txtBoxServerPort.Text));
                IPAddress testIP = IPAddress.Parse(this.txtBoxServerIP.Text);
                int port = Convert.ToInt32(this.txtBoxServerPort.Text);

                this.client = new TcpClient(this.txtBoxServerIP.Text, port);
                this.netStream = client.GetStream();
                this.reader = new StreamReader(this.netStream);
                this.isConnected = true;
                this.btnConnect.Text = "Disconnect";
            }
            catch(Exception e)
            {
                MessageBox.Show("Unable to Connect.\n\nError: " + e, "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                this.SetText(this.labelStatus, "ERROR: Could not connect to server.");
                this.Shutdown();
            }
        }

        private void RegisterOnServer()
        {
            try
            {
                this.Message(EMessageCode.HandShake, this.txtBoxPassword.Text);
            }
            catch(Exception e)
            {
                MessageBox.Show("ERROR: Could not register to server.\n\nError: " + e, "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                this.SetText(this.labelStatus, "ERROR: Could not register to server.");
            }
        }

        private void Message(EMessageCode code, string message)
        {
            try
            {
                string m = ((char)code) + "|" + this.txtBoxUsername.Text + "|" + message + "|";
                Byte[] outBytes = System.Text.Encoding.ASCII.GetBytes(m.ToCharArray());
                this.netStream.Write(outBytes, 0, outBytes.Length);
            }
            catch(Exception e)
            {
                this.SetText(this.labelStatus, "ERROR: Failed to send message.");
                MessageBox.Show("ERROR: Failed to send message.\n\nError: " + e, "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private void Whisper(string target, string message)
        {
            this.Message(EMessageCode.MessageWhisper, target + '|' + message);
        }

        private void Recieve()
        {
            while (this.isConnected)
            {
                try
                {
                    Byte[] buffer = new Byte[1024];
                    this.netStream.Read(buffer, 0, buffer.Length);
                    string message = System.Text.Encoding.ASCII.GetString(buffer);

                    string[] tokens = message.Split(new Char[] { '|' });

                    switch ((EMessageCode)tokens[0][0])
                    {
                        case EMessageCode.None:
                            {
                                
                            }
                            break;
                        case EMessageCode.HandShake:
                            {
                                this.SetText((Control)this.labelStatus, "Connected...");
                                this.SetText((Control)this,  "LMessenger - Connected as: " + this.txtBoxUsername.Text);
                                this.LoggingStart();
                                
                            }
                            break;
                        case EMessageCode.GreatSuccess:
                            {
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
                                
                            }
                            break;
                        case EMessageCode.ServerCommand:
                            {
                                
                            }
                            break;
                        case EMessageCode.DropConnection:
                            {
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
                                this.AddListBox(this.listBoxUsers, tokens[1]);
                                this.SetAppendText(this.txtBoxDisplay, tokens[1] + " has joined the server.\n");
                                this.writer.WriteLine(tokens[1] + " has joined the server.\n");
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

            this.btnConnect.Enabled = v;
        }

        /// <summary>
        /// toggles elements of the window based on isConnected.
        /// </summary>
        private void ToggleElements()
        {
            this.txtBoxPassword.ReadOnly = this.isConnected;
            this.txtBoxServerIP.ReadOnly = this.isConnected;
            this.txtBoxUsername.ReadOnly = this.isConnected;
            this.txtBoxServerPort.ReadOnly = this.isConnected;
            this.txtBoxMessage.ReadOnly = !this.isConnected;
            this.btnSendMessage.Enabled = this.isConnected;
            this.comboBoxMessageType.Enabled = this.isConnected;
            this.listBoxUsers.Enabled = this.isConnected;
        }

        #region text changed

        private void txtBoxServerIP_TextChanged(object sender, EventArgs e)
        {
            this.EnableConnectBtn();
        }

        private void txtBoxServerPort_TextChanged(object sender, EventArgs e)
        {
            this.EnableConnectBtn();
        }

        private void txtBoxUsername_TextChanged(object sender, EventArgs e)
        {
            this.EnableConnectBtn();
        }

        private void txtBoxPassword_TextChanged(object sender, EventArgs e)
        {
            this.EnableConnectBtn();
        }

        #endregion

        #region Close / Disconnect

        private void Shutdown()
        {
            if (this.netStream != null)
            {
                this.Message(EMessageCode.UserDisconnect, "Bye.");
                this.netStream.Dispose();
                this.netStream.Close();
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

            if(this.thread != null)
            {
                if(this.thread.IsAlive)
                    this.thread.Abort();
                this.thread = null;
            }

            this.isConnected = false;
            this.ToggleElements();
            this.SetText(this.btnConnect, "Connect");
            this.SetText(this.labelStatus, "Disconnected...");
           
            
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Shutdown();

            base.OnClosing(e);
        }

        #endregion

    }
}
