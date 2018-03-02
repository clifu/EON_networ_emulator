namespace NewNMS
{
    partial class Application
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
            this.buttonSend = new System.Windows.Forms.Button();
            this.buttonListen = new System.Windows.Forms.Button();
            this.listBoxReceived = new System.Windows.Forms.ListBox();
            this.comboBoxRouters = new System.Windows.Forms.ComboBox();
            this.comboBoxActions = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxTables = new System.Windows.Forms.ComboBox();
            this.labelTable = new System.Windows.Forms.Label();
            this.textBox_IP_IN = new System.Windows.Forms.TextBox();
            this.textBox_Port_IN = new System.Windows.Forms.TextBox();
            this.textBoxBand_IN = new System.Windows.Forms.TextBox();
            this.textBoxFrequencyIN = new System.Windows.Forms.TextBox();
            this.textBoxModulation = new System.Windows.Forms.TextBox();
            this.textBoxBitrate = new System.Windows.Forms.TextBox();
            this.textBoxDestination_IP = new System.Windows.Forms.TextBox();
            this.textBoxPort_OUT = new System.Windows.Forms.TextBox();
            this.textBoxHops = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.textBoxFrequencyOUT = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxBand_OUT = new System.Windows.Forms.TextBox();
            this.Band_OUT = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonSend
            // 
            this.buttonSend.Location = new System.Drawing.Point(832, 253);
            this.buttonSend.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonSend.Name = "buttonSend";
            this.buttonSend.Size = new System.Drawing.Size(195, 58);
            this.buttonSend.TabIndex = 0;
            this.buttonSend.Text = "Send";
            this.buttonSend.UseVisualStyleBackColor = true;
            this.buttonSend.Click += new System.EventHandler(this.buttonSend_Click);
            // 
            // buttonListen
            // 
            this.buttonListen.Location = new System.Drawing.Point(209, 34);
            this.buttonListen.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonListen.Name = "buttonListen";
            this.buttonListen.Size = new System.Drawing.Size(193, 80);
            this.buttonListen.TabIndex = 1;
            this.buttonListen.Text = "Run";
            this.buttonListen.UseVisualStyleBackColor = true;
            this.buttonListen.Click += new System.EventHandler(this.buttonListen_Click_1);
            // 
            // listBoxReceived
            // 
            this.listBoxReceived.FormattingEnabled = true;
            this.listBoxReceived.HorizontalScrollbar = true;
            this.listBoxReceived.ItemHeight = 16;
            this.listBoxReceived.Location = new System.Drawing.Point(-8, 320);
            this.listBoxReceived.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.listBoxReceived.Name = "listBoxReceived";
            this.listBoxReceived.ScrollAlwaysVisible = true;
            this.listBoxReceived.Size = new System.Drawing.Size(1080, 196);
            this.listBoxReceived.TabIndex = 2;
            // 
            // comboBoxRouters
            // 
            this.comboBoxRouters.FormattingEnabled = true;
            this.comboBoxRouters.Location = new System.Drawing.Point(12, 96);
            this.comboBoxRouters.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.comboBoxRouters.Name = "comboBoxRouters";
            this.comboBoxRouters.Size = new System.Drawing.Size(145, 24);
            this.comboBoxRouters.TabIndex = 3;
            // 
            // comboBoxActions
            // 
            this.comboBoxActions.FormattingEnabled = true;
            this.comboBoxActions.Location = new System.Drawing.Point(924, 172);
            this.comboBoxActions.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.comboBoxActions.Name = "comboBoxActions";
            this.comboBoxActions.Size = new System.Drawing.Size(138, 24);
            this.comboBoxActions.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 77);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(148, 17);
            this.label1.TabIndex = 8;
            this.label1.Text = "Agent to communicate";
            // 
            // comboBoxTables
            // 
            this.comboBoxTables.FormattingEnabled = true;
            this.comboBoxTables.Location = new System.Drawing.Point(746, 173);
            this.comboBoxTables.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxTables.Name = "comboBoxTables";
            this.comboBoxTables.Size = new System.Drawing.Size(160, 24);
            this.comboBoxTables.TabIndex = 13;
            // 
            // labelTable
            // 
            this.labelTable.AutoSize = true;
            this.labelTable.Location = new System.Drawing.Point(780, 155);
            this.labelTable.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelTable.Name = "labelTable";
            this.labelTable.Size = new System.Drawing.Size(96, 17);
            this.labelTable.TabIndex = 14;
            this.labelTable.Text = "Choose Table";
            // 
            // textBox_IP_IN
            // 
            this.textBox_IP_IN.Location = new System.Drawing.Point(12, 175);
            this.textBox_IP_IN.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_IP_IN.Name = "textBox_IP_IN";
            this.textBox_IP_IN.Size = new System.Drawing.Size(105, 22);
            this.textBox_IP_IN.TabIndex = 15;
            // 
            // textBox_Port_IN
            // 
            this.textBox_Port_IN.Location = new System.Drawing.Point(131, 176);
            this.textBox_Port_IN.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_Port_IN.Name = "textBox_Port_IN";
            this.textBox_Port_IN.Size = new System.Drawing.Size(64, 22);
            this.textBox_Port_IN.TabIndex = 16;
            // 
            // textBoxBand_IN
            // 
            this.textBoxBand_IN.Location = new System.Drawing.Point(209, 176);
            this.textBoxBand_IN.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxBand_IN.Name = "textBoxBand_IN";
            this.textBoxBand_IN.Size = new System.Drawing.Size(100, 22);
            this.textBoxBand_IN.TabIndex = 17;
            // 
            // textBoxFrequencyIN
            // 
            this.textBoxFrequencyIN.Location = new System.Drawing.Point(328, 176);
            this.textBoxFrequencyIN.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxFrequencyIN.Name = "textBoxFrequencyIN";
            this.textBoxFrequencyIN.Size = new System.Drawing.Size(95, 22);
            this.textBoxFrequencyIN.TabIndex = 18;
            // 
            // textBoxModulation
            // 
            this.textBoxModulation.Location = new System.Drawing.Point(470, 274);
            this.textBoxModulation.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxModulation.Name = "textBoxModulation";
            this.textBoxModulation.Size = new System.Drawing.Size(114, 22);
            this.textBoxModulation.TabIndex = 19;
            // 
            // textBoxBitrate
            // 
            this.textBoxBitrate.Location = new System.Drawing.Point(454, 174);
            this.textBoxBitrate.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxBitrate.Name = "textBoxBitrate";
            this.textBoxBitrate.Size = new System.Drawing.Size(114, 22);
            this.textBoxBitrate.TabIndex = 20;
            // 
            // textBoxDestination_IP
            // 
            this.textBoxDestination_IP.Location = new System.Drawing.Point(12, 273);
            this.textBoxDestination_IP.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxDestination_IP.Name = "textBoxDestination_IP";
            this.textBoxDestination_IP.Size = new System.Drawing.Size(104, 22);
            this.textBoxDestination_IP.TabIndex = 21;
            // 
            // textBoxPort_OUT
            // 
            this.textBoxPort_OUT.Location = new System.Drawing.Point(131, 274);
            this.textBoxPort_OUT.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxPort_OUT.Name = "textBoxPort_OUT";
            this.textBoxPort_OUT.Size = new System.Drawing.Size(71, 22);
            this.textBoxPort_OUT.TabIndex = 22;
            // 
            // textBoxHops
            // 
            this.textBoxHops.Location = new System.Drawing.Point(593, 176);
            this.textBoxHops.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxHops.Name = "textBoxHops";
            this.textBoxHops.Size = new System.Drawing.Size(114, 22);
            this.textBoxHops.TabIndex = 23;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(37, 155);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 17);
            this.label4.TabIndex = 24;
            this.label4.Text = "IP_IN";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(135, 155);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(55, 17);
            this.label5.TabIndex = 25;
            this.label5.Text = "Port_IN";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(226, 153);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(62, 17);
            this.label6.TabIndex = 26;
            this.label6.Text = "Band_IN";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(327, 155);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(96, 17);
            this.label7.TabIndex = 27;
            this.label7.Text = "Frequency_IN";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(481, 253);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(77, 17);
            this.label8.TabIndex = 28;
            this.label8.Text = "Modulation";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(481, 155);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(54, 17);
            this.label9.TabIndex = 29;
            this.label9.Text = "BitRate";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(13, 253);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(99, 17);
            this.label10.TabIndex = 30;
            this.label10.Text = "Destination_IP";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(130, 253);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(72, 17);
            this.label11.TabIndex = 31;
            this.label11.Text = "Port_OUT";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(630, 155);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(41, 17);
            this.label12.TabIndex = 32;
            this.label12.Text = "Hops";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(943, 155);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(98, 17);
            this.label13.TabIndex = 33;
            this.label13.Text = "Choose action";
            // 
            // textBoxFrequencyOUT
            // 
            this.textBoxFrequencyOUT.Location = new System.Drawing.Point(333, 273);
            this.textBoxFrequencyOUT.Name = "textBoxFrequencyOUT";
            this.textBoxFrequencyOUT.Size = new System.Drawing.Size(120, 22);
            this.textBoxFrequencyOUT.TabIndex = 34;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(340, 253);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 17);
            this.label2.TabIndex = 35;
            this.label2.Text = "Frequency_OUT";
            // 
            // textBoxBand_OUT
            // 
            this.textBoxBand_OUT.Location = new System.Drawing.Point(209, 274);
            this.textBoxBand_OUT.Name = "textBoxBand_OUT";
            this.textBoxBand_OUT.Size = new System.Drawing.Size(118, 22);
            this.textBoxBand_OUT.TabIndex = 36;
            // 
            // Band_OUT
            // 
            this.Band_OUT.AutoSize = true;
            this.Band_OUT.Location = new System.Drawing.Point(230, 255);
            this.Band_OUT.Name = "Band_OUT";
            this.Band_OUT.Size = new System.Drawing.Size(79, 17);
            this.Band_OUT.TabIndex = 37;
            this.Band_OUT.Text = "Band_OUT";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(611, 255);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(200, 55);
            this.button1.TabIndex = 38;
            this.button1.Text = "Enable textBoxes";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(484, 46);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(187, 68);
            this.button2.TabIndex = 39;
            this.button2.Text = "Hide textBoxes";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Application
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(1071, 533);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.Band_OUT);
            this.Controls.Add(this.textBoxBand_OUT);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxFrequencyOUT);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxHops);
            this.Controls.Add(this.textBoxPort_OUT);
            this.Controls.Add(this.textBoxDestination_IP);
            this.Controls.Add(this.textBoxBitrate);
            this.Controls.Add(this.textBoxModulation);
            this.Controls.Add(this.textBoxFrequencyIN);
            this.Controls.Add(this.textBoxBand_IN);
            this.Controls.Add(this.textBox_Port_IN);
            this.Controls.Add(this.textBox_IP_IN);
            this.Controls.Add(this.labelTable);
            this.Controls.Add(this.comboBoxTables);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxActions);
            this.Controls.Add(this.comboBoxRouters);
            this.Controls.Add(this.listBoxReceived);
            this.Controls.Add(this.buttonListen);
            this.Controls.Add(this.buttonSend);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Application";
            this.Text = "Application";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonSend;
        private System.Windows.Forms.Button buttonListen;
        private System.Windows.Forms.ListBox listBoxReceived;
        private System.Windows.Forms.ComboBox comboBoxRouters;
        private System.Windows.Forms.ComboBox comboBoxActions;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxTables;
        private System.Windows.Forms.Label labelTable;
        private System.Windows.Forms.TextBox textBox_IP_IN;
        private System.Windows.Forms.TextBox textBox_Port_IN;
        private System.Windows.Forms.TextBox textBoxBand_IN;
        private System.Windows.Forms.TextBox textBoxFrequencyIN;
        private System.Windows.Forms.TextBox textBoxModulation;
        private System.Windows.Forms.TextBox textBoxBitrate;
        private System.Windows.Forms.TextBox textBoxDestination_IP;
        private System.Windows.Forms.TextBox textBoxPort_OUT;
        private System.Windows.Forms.TextBox textBoxHops;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox textBoxFrequencyOUT;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxBand_OUT;
        private System.Windows.Forms.Label Band_OUT;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}

