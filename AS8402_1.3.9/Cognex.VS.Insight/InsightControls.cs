using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cognex.InSight.Net;
using Cognex.InSight.Controls.Display;
using Cognex.InSight;

namespace Cognex.VS.InSightControl
{
    public partial class InsightControls : UserControl, INotifyPropertyChanged
    {
        private CvsNetworkMonitor mMonitor = null;
        private CvsHostSensor mHostSensor = null;
        private CvsInSight mInSight = null;
        private Dictionary<string, object> mBindingObject = new Dictionary<string, object>();
        private List<string> mHostNameLst = new List<string>();
        private CvsImageOrientation mImageOrientation = CvsImageOrientation.Normal;
        private InsightConfigure mInsightConfigure = null;
        private string mHostIPAddress = string.Empty;
        private bool mCanPanImage = false;
        private bool mIsSavingToLocal = false;
        private bool mIsSavingToFTP = false;
        private string mJobFilePath = string.Empty;
        private int mSavingCount = 0;
        public bool IsConnected
        {
            get
            {
                if (mInSight != null)
                {
                    return (mInSight.State != CvsInSightState.NotConnected);
                }
                else
                    return false;
            }
        }

        public bool CanPanImage
        {
            get { return mCanPanImage; }
            set { mCanPanImage = value; }
        }

        public string HostIPAddress
        {
            get { return mHostIPAddress; }
        }

        public List<string> HostNames
        {
            get { return mHostNameLst; }
            internal set
            {
                mHostNameLst = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("HostNames"));
                }
                //PropertyChanged.BeginInvoke(this, new PropertyChangedEventArgs("HostNames"), null, null);
            }
        }

        public CvsInSight InSight
        {
            get
            {
                try
                {
                    CvsEasyView cvsEasyViewItems = mInSight.GetEasyView();
                }
                catch (Exception ex)
                {

                }
                return mInSight;
            }
            internal set
            {
                if (mInSight != null)
                {
                    mInSight.StateChanged -= new CvsStateChangedEventHandler(mInSight_StateChanged);
                    mInSight.ResultsChanged += new EventHandler(MInSight_ResultsChanged);
                }
                mInSight = value;
                if (mInSight != null)
                {
                    CvsEasyView cvsEasyViewItems = mInSight.GetEasyView();
                    mInSight.StateChanged += new CvsStateChangedEventHandler(mInSight_StateChanged);
                    mInSight.ResultsChanged += new EventHandler(MInSight_ResultsChanged);
                    mInSight.SaveCompleted += MInSight_SaveCompleted;
                }
            }
        }

        private void MInSight_SaveCompleted(object sender, CvsSaveCompletedEventArgs e)
        {
            string jobName = string.Empty;
            int index = mJobFilePath.LastIndexOf("//") + 2;
            jobName = mJobFilePath.Substring(index, mJobFilePath.Length - index);
            mSavingCount++;
            if (mIsSavingToFTP)
            {
                try
                {
                    InSight.File.SaveJobFile(jobName, false);
                    mIsSavingToFTP = false;
                    mIsSavingToLocal = true;
                }
                catch (Exception ex)
                {

                }
            }

            if (mSavingCount == 2 && mIsSavingToLocal)
            {
                mIsSavingToLocal = false;
                mSavingCount = 0;
                this.cvsInSightDisplay1.Cursor = Cursors.Default;
                MessageBox.Show(string.Format("Saved setting to: {0} success!", jobName), "Notification");
            }
        }

        private void MInSight_ResultsChanged(object sender, EventArgs e)
        {

        }

        public string StatusInfoChanged
        {
            get { return cvsInSightDisplay1.StatusInformation; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public InsightControls()
        {
            InitializeComponent();
            Initilize();
        }

        public CvsHostSensor GetHostSensor()
        {
            return mHostSensor;
        }

        public CvsInSightDisplay GetInsightDisplay()
        {
            return this.cvsInSightDisplay1;
        }

        public bool Binding(string mode, object controlObj)
        {
            if (mode == "ZoomImageIn")
                cvsInSightDisplay1.Edit.ZoomImageIn.Bind(controlObj);
            else if (mode == "ZoomImageOut")
                cvsInSightDisplay1.Edit.ZoomImageOut.Bind(controlObj);
            else if (mode == "ZoomImageToFill")
                cvsInSightDisplay1.Edit.ZoomImageToFill.Bind(controlObj);
            else if (mode == "ZoomImageToFit")
                cvsInSightDisplay1.Edit.ZoomImageToFit.Bind(controlObj);
            else if (mode == "ZoomImage1To1")
                cvsInSightDisplay1.Edit.ZoomImage1To1.Bind(controlObj);
            else if (mode == "ShowGrid")
                cvsInSightDisplay1.Edit.ShowGrid.Bind(controlObj);
            else if (mode == "ManualAcquire")
                cvsInSightDisplay1.Edit.ManualAcquire.Bind(controlObj);
            else if (mode == "LiveAcquire")
                cvsInSightDisplay1.Edit.LiveAcquire.Bind(controlObj);
            else if (mode == "Online")
                cvsInSightDisplay1.Edit.SoftOnline.Bind(controlObj);
            else if (mode == "Spreadsheet")
                cvsInSightDisplay1.Edit.ShowGrid.Bind(controlObj);
            else
                return false;
            mBindingObject.Add(mode, controlObj);
            return true;
        }

        public void SetImgOrientation(int angle)
        {
            int orientation = angle + (int)mImageOrientation;
            if (orientation >= 360)
                orientation = 0;
            else if (orientation < 0)
                orientation = orientation + 360;
            mImageOrientation = (CvsImageOrientation)orientation;
            cvsInSightDisplay1.ImageOrientation = mImageOrientation;
        }

        public void ConnectToSensor(string host)
        {
            try
            {
                if (!IsConnected)
                {
                    mHostSensor = mMonitor.Hosts[host];
                    cvsInSightDisplay1.Connect(mHostSensor.IPAddressString, "admin", "", false);
                    mHostIPAddress = mHostSensor.IPAddressString;
                }
                else
                {
                    cvsInSightDisplay1.Disconnect();
                    mHostSensor = null;
                    InSight = null;
                }
                Refresh();
            }
            catch (CvsException cvsEx)
            {
                mMonitor.Enabled = true;
            }
            catch (Exception ex)
            {

            }

            UpdateControls(false);
        }

        public void ConnectToSensorByIP(string ip)
        {
            try
            {

                cvsInSightDisplay1.Connect(ip, "admin", "", false);
                //mHostSensor = mMonitor.Hosts[0];
                mHostIPAddress = ip;
                Refresh();
            }
            catch (CvsException cvsEx)
            {
                mMonitor.Enabled = true;
            }

            UpdateControls(false);
        }

        public string GetRemotePath()
        {
            string sPath = "";
            if (cvsInSightDisplay1.Connected)
            {
                sPath = String.Format("ftp://{0}:{1}//", mHostIPAddress, 21);
            }
            return sPath;
        }

        public bool OpenJobRemote(string path2File)
        {
            bool ret = false;
            try
            {
                InSight.File.LoadJobFile(path2File, true);
                ret = true;
            }
            catch (Exception ex)
            {

            }
            return ret;
        }
        public bool OpenJob(string path2File)
        {
            bool ret = false;
            if (System.IO.File.Exists(path2File))
            {
                try
                {
                    InSight.File.LoadJobFileLocally(path2File);
                    ret = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return ret;
        }

        public bool SaveJob(string path2File)
        {
            bool ret = false;
            //if (!System.IO.File.Exists(path2File))
            {
                try
                {
                    InSight.File.SaveJobFileLocally(path2File);
                    ret = true;
                }
                catch (Exception ex)
                {

                }
            }
            return ret;
        }

        public bool SaveJobRemote(string path2File)
        {
            bool ret = false;
            mJobFilePath = path2File;
            try
            {
                InSight.File.SaveJobFile(path2File, true);
                cvsInSightDisplay1.Cursor = Cursors.WaitCursor;
                mIsSavingToFTP = true;
                ret = true;
            }
            catch (Exception ex)
            {

            }
            return ret;
        }
        public bool LiveModeActivated
        {
            get { return cvsInSightDisplay1.Edit.LiveAcquire.Activated; }
        }

        private void Initilize()
        {
            mMonitor = new CvsNetworkMonitor();
            mMonitor.PingInterval = 0;

            mMonitor.HostsChanged += new EventHandler(mMonitor_HostsChanged);
            mMonitor.Enabled = true;

            cvsInSightDisplay1.InSightChanged += new System.EventHandler(this.cvsInSightDisplay1_InSightChanged);
            cvsInSightDisplay1.StatusInformationChanged += new System.EventHandler(cvsInSightDisplay1_StatusInformationChanged);

            mInsightConfigure = new InsightConfigure(this.cvsInSightDisplay1);
            mInsightConfigure.Hide();
        }

        private void cvsInSightDisplay1_InSightChanged(object sender, EventArgs e)
        {
            InSight = cvsInSightDisplay1.InSight;
            PropertyChanged.BeginInvoke(this, new PropertyChangedEventArgs("IsConnected"), null, null);

            mImageOrientation = cvsInSightDisplay1.ImageOrientation;
        }

        private void mMonitor_HostsChanged(object sender, EventArgs e)
        {
            PopulateHostList();
        }

        private void PopulateHostList()
        {
            List<string> hostNames = new List<string>();

            // Traverse through the hosts, adding them to the combo box (cboDevices).
            foreach (CvsHostSensor host in mMonitor.Hosts)
            {
                hostNames.Add(host.Name);
            }
            HostNames = hostNames;
        }

        private void UpdateControls(bool update)
        {
            //if (!IsConnected)
            //{
            //    viewToolStripMenuItem.Enabled = false;
            //    imageToolStripMenuItem.Enabled = false;
            //    sensorToolStripMenuItem.Enabled = false;
            //    optionsToolStripMenuItem.Enabled = false;
            //    cboDevices.Enabled = (cboDevices.Items.Count > 0);
            //    btnConnect.Text = "Connect";
            //    if (((ConnectStatus)statusCurrent.Tag == ConnectStatus.Connecting) && update)
            //    {
            //        UpdateStatus(ConnectStatus.Disconnected, "Connection Failed");
            //        MessageBox.Show(this, "Failed to Connect", "Error");
            //    }
            //    else
            //    {
            //        UpdateStatus(ConnectStatus.Disconnected);
            //    }
            //    btnConnect.Enabled = (cboDevices.Items.Count > 0);
            //}
            //else
            //{
            //    imageToolStripMenuItem.Enabled = true;
            //    sensorToolStripMenuItem.Enabled = true;
            //    customViewToolStripMenuItem.Checked = mInSight.JobInfo.AnyCustomViews;
            //    overlayToolStripMenuItem.Checked = cvsInSightDisplay1.ShowGrid;

            //    // When in live acquisition mode, disable invalid operations
            //    liveModeToolStripMenuItem.Checked = mInSight.LiveAcquisition;
            //    if (mInSight.LiveAcquisition)
            //    {
            //        sensorToolStripMenuItem.Enabled = false;
            //        viewToolStripMenuItem.Enabled = false;
            //        optionsToolStripMenuItem.Enabled = false;
            //    }
            //    else
            //    {
            //        sensorToolStripMenuItem.Enabled = true;
            //        viewToolStripMenuItem.Enabled = true;
            //        optionsToolStripMenuItem.Enabled = true;
            //    }
            //    cboDevices.Enabled = false;
            //    btnConnect.Text = "Disconnect";
            //    UpdateStatus(ConnectStatus.Connected);
            //    onlineToolStripMenuItem.Text = (mInSight.State == CvsInSightState.Offline) ? "Go Online" : "Go Offline";
            //}
        }

        /// <summary>
        /// Called when the In-Sight SateChanged event raised.
        /// </summary>
        private void mInSight_StateChanged(object sender, CvsStateChangedEventArgs e)
        {
            UpdateControls(true);
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        public void Shutdown(bool disposing)
        {
            if (mInsightConfigure != null)
            {
                mInsightConfigure.Dispose();
                mInsightConfigure = null;
            }

            if (cvsInSightDisplay1 != null)
            {
                if (cvsInSightDisplay1.Connected)
                    cvsInSightDisplay1.Disconnect();
                cvsInSightDisplay1.StatusInformationChanged -= new EventHandler(cvsInSightDisplay1_StatusInformationChanged);
                cvsInSightDisplay1.InSightChanged -= new System.EventHandler(this.cvsInSightDisplay1_InSightChanged);
            }
            InSight = null;

            if (mMonitor != null)
            {
                mMonitor.HostsChanged -= new EventHandler(mMonitor_HostsChanged);
                mMonitor.Dispose();
            }

            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void cvsInSightDisplay1_StatusInformationChanged(object sender, EventArgs e)
        {
            PropertyChanged(this, new PropertyChangedEventArgs("StatusInfoChanged"));
        }

        public void SaveImage(string path2File)
        {
            mInSight.Results.Image.Save(path2File);
        }

        public void OpenPropertySheet(CvsCellLocation location, string expression)
        {
            cvsInSightDisplay1.OpenPropertySheet(location, expression);
        }
        public void ManualTrigger()
        {
            if (!this.InSight.LiveAcquisition)
                this.InSight.ManualAcquire();
        }

        /// <summary>
        /// Open Window to configure parameters of insights
        /// </summary>
        public void Configure()
        {
            if (mInsightConfigure == null)
                mInsightConfigure = new InsightConfigure(this.cvsInSightDisplay1);
            mInsightConfigure.UpdateParam();
            mInsightConfigure.Show();
        }

        private bool isDragging = false;
        private int xPos = 0;
        private int yPos = 0;
        private void cvsInSightDisplay1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && (ModifierKeys & Keys.None) == Keys.None)
            {
                cvsInSightDisplay1.Cursor = Cursors.Hand;
                isDragging = true;
                xPos = e.X;
                yPos = e.Y;
            }
        }

        private void cvsInSightDisplay1_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
            cvsInSightDisplay1.Cursor = Cursors.Default;
        }

        private void cvsInSightDisplay1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                cvsInSightDisplay1.ImageOffset = new PointF((float)((cvsInSightDisplay1.ImageOffset.X + (xPos - e.X) / cvsInSightDisplay1.ImageScale)), (float)((cvsInSightDisplay1.ImageOffset.Y + (yPos - e.Y) / cvsInSightDisplay1.ImageScale)));
                xPos = e.X;
                yPos = e.Y;
            }
        }

        private void cvsInSightDisplay1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && (ModifierKeys & Keys.None) == Keys.None)
            {
                cvsInSightDisplay1.Edit.ZoomImageToFit.Execute();
            }
        }

        private void cvsInSightDisplay1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!cvsInSightDisplay1.Edit.ShowGrid.Activated)
            {
                if (e.Delta > 0)
                {
                    cvsInSightDisplay1.Edit.ZoomImageIn.Execute();
                }
                else
                {
                    cvsInSightDisplay1.Edit.ZoomImageOut.Execute();
                }
            }
        }

        private void cvsInSightDisplay1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.H && e.Modifiers == (Keys.Control | Keys.Shift))
            {
                if (cvsInSightDisplay1 != null)
                    cvsInSightDisplay1.Edit.ShowGrid.Execute();
            }
        }

    }
}
