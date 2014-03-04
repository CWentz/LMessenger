using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using LNetwork;
using System.Net;

namespace LMessenger_Client
{
    public partial class LMessengerClient : Form
    {
        private LClient user;
        
        public LMessengerClient()
        {
            InitializeComponent();

        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            this.user = new LClient(this.textBoxUser.Text, this.textBoxNick.Text, this.textBoxPass.Text);
            this.user.Connect(IPAddress.Parse("192.168.2.101"), 1337);
        }
    }
}
