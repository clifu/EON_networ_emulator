using System.Drawing;

namespace ClientNode
{
    partial class ClientApplication
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
            this.textBoxReceived = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBoxClients = new System.Windows.Forms.ComboBox();
            this.buttonConnectToCloud = new System.Windows.Forms.Button();
            this.buttonDifferentMessages = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxHowManyMessages = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.BreakConnectionButton = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxDemandedCapacity = new System.Windows.Forms.TextBox();
            this.establishConnectionButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.textCPCClogs = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxReceived
            // 
            this.textBoxReceived.Location = new System.Drawing.Point(13, 64);
            this.textBoxReceived.Multiline = true;
            this.textBoxReceived.Name = "textBoxReceived";
            this.textBoxReceived.ReadOnly = true;
            this.textBoxReceived.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxReceived.Size = new System.Drawing.Size(502, 62);
            this.textBoxReceived.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Received message";
            // 
            // textBoxLog
            // 
            this.textBoxLog.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textBoxLog.ForeColor = System.Drawing.SystemColors.WindowText;
            this.textBoxLog.Location = new System.Drawing.Point(13, 182);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ReadOnly = true;
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxLog.Size = new System.Drawing.Size(502, 81);
            this.textBoxLog.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(21, 166);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(25, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Log";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 129);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Choose client";
            // 
            // comboBoxClients
            // 
            this.comboBoxClients.FormattingEnabled = true;
            this.comboBoxClients.Location = new System.Drawing.Point(13, 142);
            this.comboBoxClients.Name = "comboBoxClients";
            this.comboBoxClients.Size = new System.Drawing.Size(161, 21);
            this.comboBoxClients.TabIndex = 7;
            // 
            // buttonConnectToCloud
            // 
            this.buttonConnectToCloud.Location = new System.Drawing.Point(13, 13);
            this.buttonConnectToCloud.Name = "buttonConnectToCloud";
            this.buttonConnectToCloud.Size = new System.Drawing.Size(502, 23);
            this.buttonConnectToCloud.TabIndex = 9;
            this.buttonConnectToCloud.Text = "CONNECT AND LISTEN";
            this.buttonConnectToCloud.UseVisualStyleBackColor = true;
            this.buttonConnectToCloud.Click += new System.EventHandler(this.buttonConnectToCloud_Click);
            // 
            // buttonDifferentMessages
            // 
            this.buttonDifferentMessages.Location = new System.Drawing.Point(366, 136);
            this.buttonDifferentMessages.Name = "buttonDifferentMessages";
            this.buttonDifferentMessages.Size = new System.Drawing.Size(149, 31);
            this.buttonDifferentMessages.TabIndex = 11;
            this.buttonDifferentMessages.Text = "Send messages";
            this.buttonDifferentMessages.UseVisualStyleBackColor = true;
            this.buttonDifferentMessages.Click += new System.EventHandler(this.buttonDifferentMessages_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(187, 129);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(110, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "How many messages:";
            // 
            // textBoxHowManyMessages
            // 
            this.textBoxHowManyMessages.Location = new System.Drawing.Point(190, 142);
            this.textBoxHowManyMessages.Name = "textBoxHowManyMessages";
            this.textBoxHowManyMessages.Size = new System.Drawing.Size(161, 20);
            this.textBoxHowManyMessages.TabIndex = 13;
            this.textBoxHowManyMessages.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.BreakConnectionButton);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.textBoxDemandedCapacity);
            this.groupBox1.Controls.Add(this.establishConnectionButton);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.textCPCClogs);
            this.groupBox1.Location = new System.Drawing.Point(13, 269);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(502, 153);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "CPCC";
            // 
            // BreakConnectionButton
            // 
            this.BreakConnectionButton.Location = new System.Drawing.Point(177, 10);
            this.BreakConnectionButton.Name = "BreakConnectionButton";
            this.BreakConnectionButton.Size = new System.Drawing.Size(93, 40);
            this.BreakConnectionButton.TabIndex = 5;
            this.BreakConnectionButton.Text = "Break connection";
            this.BreakConnectionButton.UseVisualStyleBackColor = true;
            this.BreakConnectionButton.Click += new System.EventHandler(this.BreakConnectionButton_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(317, 16);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(146, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "Demanded capacity (in Gb/s)";
            // 
            // textBoxDemandedCapacity
            // 
            this.textBoxDemandedCapacity.Location = new System.Drawing.Point(310, 32);
            this.textBoxDemandedCapacity.Name = "textBoxDemandedCapacity";
            this.textBoxDemandedCapacity.Size = new System.Drawing.Size(176, 20);
            this.textBoxDemandedCapacity.TabIndex = 3;
            this.textBoxDemandedCapacity.Text = "null";
            this.textBoxDemandedCapacity.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // establishConnectionButton
            // 
            this.establishConnectionButton.Location = new System.Drawing.Point(60, 10);
            this.establishConnectionButton.Name = "establishConnectionButton";
            this.establishConnectionButton.Size = new System.Drawing.Size(93, 40);
            this.establishConnectionButton.TabIndex = 2;
            this.establishConnectionButton.Text = "Establish connection";
            this.establishConnectionButton.UseVisualStyleBackColor = true;
            this.establishConnectionButton.Click += new System.EventHandler(this.establishConnectionButton_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 46);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(57, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "CPCC logs";
            // 
            // textCPCClogs
            // 
            this.textCPCClogs.Location = new System.Drawing.Point(11, 63);
            this.textCPCClogs.Multiline = true;
            this.textCPCClogs.Name = "textCPCClogs";
            this.textCPCClogs.ReadOnly = true;
            this.textCPCClogs.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textCPCClogs.Size = new System.Drawing.Size(485, 79);
            this.textCPCClogs.TabIndex = 0;
            // 
            // ClientApplication
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.ClientSize = new System.Drawing.Size(523, 427);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.textBoxHowManyMessages);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonDifferentMessages);
            this.Controls.Add(this.buttonConnectToCloud);
            this.Controls.Add(this.comboBoxClients);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxLog);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxReceived);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Name = "ClientApplication";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ClientNode";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox textBoxReceived;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBoxClients;
        private System.Windows.Forms.Button buttonConnectToCloud;
        private System.Windows.Forms.Button buttonDifferentMessages;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxHowManyMessages;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button establishConnectionButton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textCPCClogs;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxDemandedCapacity;
        private System.Windows.Forms.Button BreakConnectionButton;
    }
}

