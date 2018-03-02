using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientNode
{
    public partial class StartClientApplication : Form
    {
        public static StartClientApplication _StartClientApplication;
        string ClientIP;
        string ClientPort;
        string CloudPort;

        public StartClientApplication()
        {
            InitializeComponent();
            _StartClientApplication = this;
        }

        private void buttonStartClient_Click(object sender, EventArgs e)
        {
            ClientIP = textBoxClientIP.Text;
            if (ClientIP == "127.0.0.2" || ClientIP == "127.0.0.4" || ClientIP == "127.0.0.6")
            {
                ClientPort = textBoxClientPort.Text;
                CloudPort = textBoxCloudPort.Text;


                _StartClientApplication.Hide();
                var ClientApplicationForm = new ClientApplication(ClientIP, ClientPort, CloudPort);
                ClientApplicationForm.Closed += (s, args) => _StartClientApplication.Close();
                ClientApplicationForm.Show();
            }
            else
            {
                MessageBox.Show("Enter yours IP again", "Important Message.",
                         MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
            }
        }

        private void textBoxClientIP_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                buttonStartClient.PerformClick();
            }
        }
    }
}
