using LNetwork;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LMessenger_Server
{
    public partial class LMessenger_Server : Form
    {
        private LServer server;
        private bool isServerRunning;

        public LMessenger_Server()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ip = IPAddress.Parse("192.168.2.101");
            this.server = new LServer(ip, 1337, 100);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (this.isServerRunning)
            {
                this.server.StartServer();
                this.btnConnect.Text = "Disconnect";
            }
            else
            {
                this.server.StopServer();
                this.btnConnect.Text = "Connect";
            }

            this.isServerRunning = !this.isServerRunning;
            
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            //ensuring the server is shutdown before closing
            this.server.StopServer();

            base.OnFormClosing(e);
        }

    }
}
