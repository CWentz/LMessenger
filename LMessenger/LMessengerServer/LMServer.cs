using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LMessengerServer
{
    public partial class LMServer : Form
    {
        private bool isRunning = false;

        public LMServer()
        {
            InitializeComponent();

            this.txtBoxCommand.Enabled = false;
            this.btnSendCommand.Enabled = false;
            this.comboBoxMessageType.BeginUpdate();
            this.comboBoxMessageType.Items.Add("All");
            this.comboBoxMessageType.Items.Add("PM");
            this.comboBoxMessageType.Items.Add("File");
            this.comboBoxMessageType.Items.Add("Kick");
            this.comboBoxMessageType.EndUpdate();
            this.comboBoxMessageType.SelectedItem = this.comboBoxMessageType.Items[0];
            this.comboBoxMessageType.Enabled = false;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            this.isRunning = !this.isRunning;

            if (this.isRunning)
            {
                this.btnConnect.Text = "Disconnect";
                this.txtBoxCommand.Enabled = true;
                this.btnSendCommand.Enabled = true;
                this.listBoxUsers.BeginUpdate();
                this.listBoxUsers.Items.Add("Admin");
                this.listBoxUsers.EndUpdate();
                this.comboBoxMessageType.Enabled = true;
            }
            else
            {
                this.btnConnect.Text = "Deploy";
                this.txtBoxCommand.Enabled = false;
                this.btnSendCommand.Enabled = false;
                this.listBoxUsers.BeginUpdate();
                this.listBoxUsers.Items.Remove("Admin");
                this.listBoxUsers.EndUpdate();
                this.comboBoxMessageType.Enabled = false;
                
            }
        }

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
    }
}
