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

            this.EnableConnect();
            this.ToggleElements();
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
            }
            else
            {
                this.btnConnect.Text = "Connect";
            }

            this.ToggleElements();
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
    }
}
