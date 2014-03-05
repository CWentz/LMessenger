namespace LMessengerServer
{
    partial class LMServer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LMServer));
            this.btnConnect = new System.Windows.Forms.Button();
            this.txtBoxReader = new System.Windows.Forms.TextBox();
            this.txtBoxCommand = new System.Windows.Forms.TextBox();
            this.listBoxUsers = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSendCommand = new System.Windows.Forms.Button();
            this.comboBoxMessageType = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(13, 13);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 0;
            this.btnConnect.Text = "Deploy";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // txtBoxReader
            // 
            this.txtBoxReader.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.txtBoxReader.Location = new System.Drawing.Point(13, 43);
            this.txtBoxReader.Multiline = true;
            this.txtBoxReader.Name = "txtBoxReader";
            this.txtBoxReader.ReadOnly = true;
            this.txtBoxReader.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtBoxReader.Size = new System.Drawing.Size(370, 366);
            this.txtBoxReader.TabIndex = 1;
            // 
            // txtBoxCommand
            // 
            this.txtBoxCommand.Location = new System.Drawing.Point(140, 424);
            this.txtBoxCommand.Name = "txtBoxCommand";
            this.txtBoxCommand.Size = new System.Drawing.Size(243, 20);
            this.txtBoxCommand.TabIndex = 2;
            // 
            // listBoxUsers
            // 
            this.listBoxUsers.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listBoxUsers.FormattingEnabled = true;
            this.listBoxUsers.Location = new System.Drawing.Point(390, 43);
            this.listBoxUsers.Name = "listBoxUsers";
            this.listBoxUsers.Size = new System.Drawing.Size(120, 366);
            this.listBoxUsers.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(517, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Commands:";
            // 
            // btnSendCommand
            // 
            this.btnSendCommand.Location = new System.Drawing.Point(390, 422);
            this.btnSendCommand.Name = "btnSendCommand";
            this.btnSendCommand.Size = new System.Drawing.Size(120, 23);
            this.btnSendCommand.TabIndex = 5;
            this.btnSendCommand.Text = "Send Command";
            this.btnSendCommand.UseVisualStyleBackColor = true;
            this.btnSendCommand.Click += new System.EventHandler(this.btnSendCommand_Click);
            // 
            // comboBoxMessageType
            // 
            this.comboBoxMessageType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMessageType.FormattingEnabled = true;
            this.comboBoxMessageType.Location = new System.Drawing.Point(13, 424);
            this.comboBoxMessageType.Name = "comboBoxMessageType";
            this.comboBoxMessageType.Size = new System.Drawing.Size(121, 21);
            this.comboBoxMessageType.Sorted = true;
            this.comboBoxMessageType.TabIndex = 6;
            // 
            // LMServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(671, 464);
            this.Controls.Add(this.comboBoxMessageType);
            this.Controls.Add(this.btnSendCommand);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listBoxUsers);
            this.Controls.Add(this.txtBoxCommand);
            this.Controls.Add(this.txtBoxReader);
            this.Controls.Add(this.btnConnect);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LMServer";
            this.RightToLeftLayout = true;
            this.Text = "LMessenger Server";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.TextBox txtBoxReader;
        private System.Windows.Forms.TextBox txtBoxCommand;
        private System.Windows.Forms.ListBox listBoxUsers;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSendCommand;
        private System.Windows.Forms.ComboBox comboBoxMessageType;
    }
}

