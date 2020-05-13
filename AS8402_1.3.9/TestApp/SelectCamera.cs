using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestApp
{
    public partial class SelectCamera : Form
    {
        private List<string> mListIP = new List<string>();
        public delegate void AccessButtonClickDelegate(string cmd);
        public event AccessButtonClickDelegate AccessButtonClickEvent;
        public SelectCamera(List<string> listIP)
        {
            InitializeComponent();
            mListIP = listIP;
            if (mListIP != null && mListIP.Count > 1)
            {
                lblMasterCam.Text = "Master: " + mListIP[0];
                btnConnectMaster.Enabled = true;
                lblSlaveCam.Text = "Slave: " + mListIP[1];
                btnConnectSlave.Enabled = true;

            }
            else
            {
                if (mListIP.Count > 0)
                {
                    if (AccessButtonClickEvent != null)
                        AccessButtonClickEvent.BeginInvoke("Master", null, null);
                }
                else
                {
                    MessageBox.Show("No sensor found!", "Warning");
                    AccessButtonClickEvent.BeginInvoke("Exit", null, null);
                    this.Close();
                    this.Dispose();
                }
            }
        }

        public List<string> ListIP
        {
            get { return mListIP; }
            set { mListIP = value; }
        }

        private void btnConnectMaster_Click(object sender, EventArgs e)
        {
            if (AccessButtonClickEvent != null)
                AccessButtonClickEvent.BeginInvoke("Master", null, null);
        }

        private void btnConnectSlave_Click(object sender, EventArgs e)
        {
            if (AccessButtonClickEvent != null)
                AccessButtonClickEvent.BeginInvoke("Slave", null, null);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            if (AccessButtonClickEvent != null)
                AccessButtonClickEvent.BeginInvoke("Exit", null, null);

            this.Close();
            this.Dispose();
        }
        public void Exit()
        {
            
        }
    }
}
