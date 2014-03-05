using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LMessenger
{
    public partial class LMClient : Form
    {
        private bool isConnected = false;

        public LMClient()
        {
            InitializeComponent();

            this.btnConnect.Text = "Connect";
            this.txtBoxPassword.ReadOnly = false;
            this.txtBoxServerIP.ReadOnly = false;
            this.txtBoxUsername.ReadOnly = false;
            this.txtBoxServerPort.ReadOnly = false;
            this.txtBoxMessage.ReadOnly = true;
            this.btnSendMessage.Enabled = false;
            this.btnConnect.Enabled = false;
            this.comboBoxMessageType.Enabled = false;
            this.listBoxUsers.Enabled = false;
            this.comboBoxMessageType.BeginUpdate();
            this.comboBoxMessageType.Items.Add("All");
            this.comboBoxMessageType.Items.Add("PM");
            this.comboBoxMessageType.Items.Add("File");
            this.comboBoxMessageType.EndUpdate();
            this.comboBoxMessageType.SelectedItem = this.comboBoxMessageType.Items[0];
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            this.isConnected = !this.isConnected;

            if(this.isConnected)
            {
                this.btnConnect.Text = "Disconnect";
                this.txtBoxPassword.ReadOnly = true;
                this.txtBoxServerIP.ReadOnly = true;
                this.txtBoxUsername.ReadOnly = true;
                this.txtBoxServerPort.ReadOnly = true;
                this.txtBoxMessage.ReadOnly = false;
                this.btnSendMessage.Enabled = true;
                this.comboBoxMessageType.Enabled = true;
                this.listBoxUsers.Enabled = true;
            }
            else
            {
                this.btnConnect.Text = "Connect";
                this.txtBoxPassword.ReadOnly = false;
                this.txtBoxServerIP.ReadOnly = false;
                this.txtBoxUsername.ReadOnly = false;
                this.txtBoxServerPort.ReadOnly = false;
                this.txtBoxMessage.ReadOnly = true;
                this.btnSendMessage.Enabled = false;
                this.comboBoxMessageType.Enabled = false;
                this.listBoxUsers.Enabled = false;
            }
        }

        private void btnSendMessage_Click(object sender, EventArgs e)
        {
            switch (this.comboBoxMessageType.SelectedItem.ToString())
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

            this.txtBoxDisplay.AppendText(this.txtBoxUsername.Text + ": " +this.txtBoxMessage.Text + "\n");
            this.txtBoxMessage.Text = "";
        }

        #region text changed
        private void txtBoxServerIP_TextChanged(object sender, EventArgs e)
        {
            this.EnableConnect();
        }

        private void txtBoxServerPort_TextChanged(object sender, EventArgs e)
        {
            this.EnableConnect();
        }

        private void txtBoxUsername_TextChanged(object sender, EventArgs e)
        {
            this.EnableConnect();
        }

        private void txtBoxPassword_TextChanged(object sender, EventArgs e)
        {
            this.EnableConnect();
        }

        #endregion

        private void EnableConnect()
        {
            bool v = this.txtBoxUsername.Text != "" &&
                     this.txtBoxPassword.Text != "" &&
                     this.txtBoxServerIP.Text != "" &&
                     this.txtBoxServerPort.Text != "";

            this.btnConnect.Enabled = v;
        }

        private void comboBoxMessageType_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
