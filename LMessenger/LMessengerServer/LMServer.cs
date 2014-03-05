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

            this.ToggleElements();

            this.comboBoxMessageType.Items.Add("All");
            this.comboBoxMessageType.Items.Add("PM");
            this.comboBoxMessageType.Items.Add("File");
            this.comboBoxMessageType.Items.Add("Kick");

            this.comboBoxMessageType.Update();
            this.comboBoxMessageType.SelectedItem = this.comboBoxMessageType.Items[0];
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            this.isRunning = !this.isRunning;
            this.ToggleElements();

            if (this.isRunning)
            {
                this.btnConnect.Text = "Disconnect";
                this.listBoxUsers.Items.Add("Admin");
                
            }
            else
            {
                this.btnConnect.Text = "Deploy";
                this.listBoxUsers.Items.Remove("Admin");
            }

            this.listBoxUsers.Update();
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

        private void ToggleElements()
        {
            this.txtBoxCommand.Enabled = this.isRunning;
            this.btnSendCommand.Enabled = this.isRunning;
            this.comboBoxMessageType.Enabled = this.isRunning;
        }
    }
}
