using Cognex.InSight;
using Cognex.InSight.Controls.Display;
using Cognex.InSight.Ftp;
using Cognex.VS.Utility;
using MessageManager;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Cognex.VS.InspSensor
{
    public partial class InspectionForm : Form
    {
        string mDateOfVersion = "08/05/2020";
        private AccessLevel mCurrentAccessLevel = AccessLevel.Operator;
        private PasswordFile mCurrentPasswordFile = null;
        private string mDefaultAdministratorPassword = "1";
        private string mDefaultSupervisorPassword = "1";
        private string mHostName = String.Empty;
        public CvsInSightDisplay mCvsInsightDisplay;
        private FormPasswordPrompt mFormPasswordPromp = null;
        private FileJobBrowser mFileJobBrowser = new FileJobBrowser("", "");
        string mCurrentHostName = string.Empty;
        Timer mReconnectCameraTimer = new Timer();
        int mCurrentSpreadSheetLoggingCount = 0;
        bool mIsFirstTimeMessage = true;
        BackgroundWorker mSaveLogAndImageWorker = new BackgroundWorker();
        string mSensorName = string.Empty;
        Timer mPlaybackTimer = new Timer();
        string mLogDirectory = string.Empty;
        string mImageDirectory = string.Empty;
        string mApplicationName = string.Empty;
        public InspectionForm()
        {
            InitializeComponent();
            this.AutoSize = false;
            this.ControlBox = false;
            Load += InspForm_Load;
            LoadSettingFromProperties();
            mApplicationName = System.IO.Path.GetFileNameWithoutExtension(Application.ExecutablePath);
            MessageManagerSavedFile.Init(mLogDirectory + "\\Log\\", mApplicationName, 30);
            MessageLoggerManager.Log.Info("Starting program...");
            btnLiveImage.Checked = false;
            setupControl.CtrlDisplayControl.EnableAllInspectionEvent += CtrlDisplayControl_EnableAllInspectionEvent;
            UpdateControlsEnabled(false);
            UpdateStatus("");
            this.setupControl.AllSettingDoneEvent += SetupControl_AllSettingDoneEvent;
            this.intrisicControl.AllSettingDoneEvent += IntrisicControl_AllSettingDoneEnvent;
            this.settingHWControl1.AllSettingDoneEvent += settingHWControl_AllSettingDoneEvent;
            this.settingHWControl1.ExitEvent += exitButton1_ButtonClick;
            mSaveLogAndImageWorker.WorkerSupportsCancellation = true;
            mSaveLogAndImageWorker.DoWork += MSaveLogAndImageWorker_DoWork;
            mReconnectCameraTimer.Interval = 20000;
            mReconnectCameraTimer.Tick += MReconnectCameraTimer_Tick;
            mPlaybackTimer.Interval = 1000;
            mPlaybackTimer.Tick += MPlaybackTimer_Tick;
            
        }

        private void LoadSettingFromProperties()
        {
            txtLogDirectory.Text = Properties.Settings.Default.LogDirectory;
            txtImageDirectory.Text = Properties.Settings.Default.ImageDirectory;
            if (Properties.Settings.Default.LogDirectory != "")
            {
                mLogDirectory = Properties.Settings.Default.LogDirectory;
            }
            else
            {
                mLogDirectory = Environment.CurrentDirectory;
                //txtLogDirectory.Text = Environment.CurrentDirectory;
            }

            if (Properties.Settings.Default.ImageDirectory != "")
            {
                mImageDirectory = Properties.Settings.Default.ImageDirectory;
            }
            else
            {
                mImageDirectory = Environment.CurrentDirectory;
                //txtImageDirectory.Text = Environment.CurrentDirectory;
            }
            chxSaveRaw.Checked = Properties.Settings.Default.IsSaveRaw;
            chxSaveNGOnly.Checked = Properties.Settings.Default.IsSaveNGOnly;
            chxSaveGraphicImage.Checked = Properties.Settings.Default.IsSaveGraphic;
            numSavedDay.Value = decimal.Parse(Properties.Settings.Default.SavingDay.ToString());
        }

        private bool isDeleteOldFolder = false;
        private void MSaveLogAndImageWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                string message = string.Empty;
                //Delete old folder
                if (isDeleteOldFolder)
                {
                    DirectoryInfo rawFolder = new DirectoryInfo(mImageDirectory + "\\Raw\\");
                    DirectoryInfo screenFolder = new DirectoryInfo(mImageDirectory + "\\Screenshot\\");
                    if (!System.IO.Directory.Exists(mImageDirectory + "\\Raw\\"))
                        System.IO.Directory.CreateDirectory(mImageDirectory + "\\Raw\\");
                    if (!System.IO.Directory.Exists(mImageDirectory + "\\Screenshot\\"))
                        System.IO.Directory.CreateDirectory(mImageDirectory + "\\Screenshot\\");
                    foreach (DirectoryInfo dir in rawFolder.GetDirectories())
                    {
                        //Day mmddyyyy
                        DateTime dateTime = new DateTime(int.Parse(dir.Name.Substring(4, 4)), int.Parse(dir.Name.Substring(0, 2)), int.Parse(dir.Name.Substring(2, 2)));
                        if ((DateTime.Now - dateTime) > new TimeSpan(int.Parse(numSavedDay.Value.ToString()), 0, 0, 0))
                        {
                            if (rawFolder.GetDirectories().Count() > int.Parse(numSavedDay.Value.ToString()))
                            {
                                dir.Delete(true);
                            }
                        }
                    }

                    foreach (DirectoryInfo dir in screenFolder.GetDirectories())
                    {
                        //Day mmddyyyy
                        DateTime dateTime = new DateTime(int.Parse(dir.Name.Substring(4, 4)), int.Parse(dir.Name.Substring(0, 2)), int.Parse(dir.Name.Substring(2, 2)));
                        if ((DateTime.Now - dateTime) > new TimeSpan(int.Parse(numSavedDay.Value.ToString()), 0, 0, 0))
                        {
                            if (screenFolder.GetDirectories().Count() > int.Parse(numSavedDay.Value.ToString()))
                            {
                                dir.Delete(true);
                            }
                        }
                    }

                    isDeleteOldFolder = false;
                }
                //SaveLog
                //if (mCvsInsightDisplay != null && mCvsInsightDisplay.Connected && btnConnect.Text.Contains("Dis"))
                if (mCvsInsightDisplay != null && mCvsInsightDisplay.Connected)
                {
                    try
                    {
                        string nameTag = string.Format("Logging.Counter");
                        var tag = GetTag(nameTag);
                        int messageLength = 0;
                        bool isOK = true;
                        if (tag != null)
                        {
                            if (mIsFirstTimeMessage == true)
                            {
                                int count = (int)GetValue(tag.Location);
                                mCurrentSpreadSheetLoggingCount = count;
                                mIsFirstTimeMessage = false;
                            }

                            else
                            {
                                int count = (int)GetValue(tag.Location);
                                if (mCurrentSpreadSheetLoggingCount != count)
                                {
                                    for (int i = (count - mCurrentSpreadSheetLoggingCount); i >= 1; --i)
                                    {
                                        nameTag = string.Format("Logging.Message_" + i.ToString());
                                        tag = GetTag(nameTag);
                                        if (tag != null)
                                        {
                                            if (RemoveTimeString(message) != RemoveTimeString((string)GetValue(tag.Location)))
                                            {
                                                message = (string)GetValue(tag.Location);
                                                string lastmes = RemoveTimeString(message);
                                                MessageLoggerManager.Log.Info(lastmes);
                                                if ((lastmes.Contains("<-") && lastmes.Contains("HEE")) || (lastmes.Contains("<-") && lastmes.Contains("AC,1")))
                                                {
                                                    handEyeStatus1.OpenHandEyeResultForm();
                                                }
                                            }
                                        }
                                    }

                                    messageLength = RemoveTimeString(message).Length;
                                    if (messageLength < 20)
                                    {
                                        isOK = false;
                                    }
                                    mCurrentSpreadSheetLoggingCount = count;

                                    //if (chxSaveScreen.Checked)
                                    //{
                                    //    //save screenshot here
                                    //    //Bitmap image = GetScreenShot();
                                    //    Bitmap image = GetMinimizedScreenShot();
                                    //    //Bitmap image = mCvsInsightDisplay.GetBitmap();
                                    //    SaveScreenImage(GetCommandFromLog(message, isOK), image, false);
                                    //}
                                    if (chxSaveGraphicImage.Checked)
                                    {
                                        if (chxSaveNGOnly.Checked)
                                        {
                                            if (!isOK)
                                            {
                                                SaveGraphicImageOnly(GetCommandFromLog(message, isOK), isOK);
                                            }
                                        }
                                        else
                                        {
                                            SaveGraphicImageOnly(GetCommandFromLog(message, isOK), isOK);
                                        }

                                    }

                                    if (chxSaveRaw.Checked)
                                    {
                                        if (chxSaveNGOnly.Checked)
                                        {
                                            if (!isOK)
                                            {
                                                SaveRawImage(message, isOK);
                                            }
                                        }
                                        else
                                        {
                                            SaveRawImage(message, isOK);
                                        }

                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageLoggerManager.Log.Alarm("Update MessageView Fail", ex);
                    }

                    if (mSaveLogAndImageWorker.CancellationPending)
                    {
                        e.Cancel = true;
                    }
                    System.Threading.Thread.Sleep(100);
                }
            }
        }

        private void SaveRawImage(string message, bool isOK)
        {
            //CvsImage cvsImage = insightControls1.InSight.Results.Image;
            //Bitmap imageRaw = cvsImage.ToBitmap();
            //SaveScreenImage(GetCommandFromLog(message), imageRaw, true
            Stopwatch sw = new Stopwatch();
            sw.Start();
            CvsFtpClient ftp = new CvsFtpClient();
            ftp.Connect(System.Net.IPAddress.Parse(insightControls1.HostIPAddress), "admin", "");
            DateTime now = DateTime.Now;
            string imagePath = string.Empty;
            string imageName = string.Empty;
            string command = GetCommandFromLog(message, isOK);
            string okFolder = string.Empty;
            string imageCommand = string.Empty;

            if (command.Contains("XI"))
            {
                imageCommand = "Inspection";
            }
            else if (command.Contains("XT") || command.Contains("XA"))
            {
                imageCommand = "Alignment";
            }
            else
            {
                imageCommand = "Others";
            }


            if (isOK)
            {
                okFolder = "OK";
            }
            else
            {
                okFolder = "NG";
            }

            try
            {
                string imageDirectory = mImageDirectory;
                imageName = string.Format("{0}_{1}_{2}", mSensorName, command, now.ToString("HHmmss_fff"));
                imagePath = imageDirectory + "\\Raw\\" + now.ToString("MMddyyyy") + "\\" + imageCommand + "\\" + okFolder + "\\";
                imageName += ".bmp";
                string imageFileName = imagePath + imageName;

                if (!System.IO.Directory.Exists(imagePath))
                    System.IO.Directory.CreateDirectory(imagePath);

                ftp.GetFileFromInSight(imageFileName, "image.bmp");
                ftp.Disconnect();
                sw.Stop();
                MessageLoggerManager.Log.Info("Save Raw Image: " + sw.ElapsedMilliseconds.ToString() + " ms");
            }
            catch (Exception ex)
            {
                MessageLoggerManager.Log.Alarm("Save Raw Image Fail", ex);
            }
        }
        private void SaveGraphicImageOnly(string cmd, bool isOK)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SaveGraphicImageOnly(cmd, isOK)));
                return;
            }

            try
            {
                Bitmap image = mCvsInsightDisplay.GetBitmap();
                SaveScreenImage(cmd, image, false, isOK);
            }
            catch (Exception ex)
            {
                MessageLoggerManager.Log.Alarm("Save Graphic Image Fail", ex);
            }
        }

        private Bitmap GetMinimizedScreenShot()
        {
            Bitmap image = null;
            try
            {
                WindowSnap snap = WindowSnap.GetWindowSnap(WinGetHandle("Inspection"), true);
                if (snap.Image != null)
                    image = snap.Image;
            }
            catch
            {
                MessageBox.Show("Wrong Handle Using");
            }

            return image;
        }

        public static IntPtr WinGetHandle(string wName)
        {
            IntPtr hWnd = IntPtr.Zero;
            foreach (Process pList in Process.GetProcesses())
            {
                if (pList.MainWindowTitle.Contains(wName))
                {
                    hWnd = pList.MainWindowHandle;
                    //MessageLoggerManager.Log.Info("Get handle: " + pList.ProcessName);
                }
            }
            return hWnd;
        }

        private Bitmap GetScreenShot()
        {
            Bitmap screenshot = new Bitmap(SystemInformation.VirtualScreen.Width,
                                               SystemInformation.VirtualScreen.Height,
                                               PixelFormat.Format32bppArgb);
            Graphics screenGraph = Graphics.FromImage(screenshot);
            screenGraph.CopyFromScreen(SystemInformation.VirtualScreen.X,
                                       SystemInformation.VirtualScreen.Y,
                                       0,
                                       0,
                                       SystemInformation.VirtualScreen.Size,
                                       CopyPixelOperation.SourceCopy);
            return screenshot;
        }
        private string GetCommandFromLog(string message, bool isOK)
        {
            string isOKstring = "_OK";
            if (!isOK)
            {
                isOKstring = "_NG";
            }
            if (message != "")
            {
                string removedTime = RemoveTimeString(message);
                removedTime = removedTime.Substring(3, removedTime.Length - 3);
                return removedTime.Substring(0, removedTime.IndexOf(",")) + isOKstring;
            }
            else
                return "Nan";
        }

        private void SaveScreenImage(string command, Bitmap screenshot, bool isRawImage, bool isOK)
        {
            TimeSpan t1 = new TimeSpan(DateTime.Now.Ticks);
            DateTime now = DateTime.Now;
            string imagePath = string.Empty;
            string imageName = string.Empty;
            string imageCommand = string.Empty;
            string okFolder = string.Empty;

            if (command.Contains("XI"))
            {
                imageCommand = "Inspection";
            }
            else if (command.Contains("XT") || command.Contains("XA"))
            {
                imageCommand = "Alignment";
            }
            else
            {
                imageCommand = "Others";
            }

            if (isOK)
            {
                okFolder = "OK";
            }
            else
            {
                okFolder = "NG";
            }
            try
            {
                string imageDirectory = mImageDirectory;
                imageName = string.Format("{0}_{1}_{2}", mSensorName, command, now.ToString("HHmmss_fff"));
                if (isRawImage)
                {
                    if (imageCommand != "Others")
                        imagePath = imageDirectory + "\\Raw\\" + now.ToString("MMddyyyy") + "\\" + imageCommand + "\\" + okFolder + "\\";
                    else
                        imagePath = imageDirectory + "\\Raw\\" + now.ToString("MMddyyyy") + "\\" + imageCommand + "\\";
                    imageName += ".bmp";
                }
                else
                {
                    if (imageCommand != "Others")
                        imagePath = imageDirectory + "\\Screenshot\\" + now.ToString("MMddyyyy") + "\\" + imageCommand + "\\" + okFolder + "\\";
                    else
                        imagePath = imageDirectory + "\\Screenshot\\" + now.ToString("MMddyyyy") + "\\" + imageCommand + "\\";
                    imageName += ".jpg";
                }
                string imageFileName = imagePath + imageName;
                string pathRoot = Path.GetPathRoot(imagePath);
                if (!System.IO.Directory.Exists(imagePath))
                    System.IO.Directory.CreateDirectory(imagePath);
                //if (chk_saveScreenImage.Checked)
                SaveScreenImageToFolder(imageFileName, screenshot, isRawImage);

            }
            catch (Exception ex)
            {
                MessageLoggerManager.Log.Alarm("Save image fail!", ex);
            }

            TimeSpan t2 = new TimeSpan(DateTime.Now.Ticks);
            double timeSpan = t2.Subtract(t1).TotalMilliseconds;
            //MessageLoggerManager.Log.Info("[Saved] Screen saved time: " + timeSpan.ToString() + " ms");

        }

        private void SaveScreenImageToFolder(string imageName, Bitmap inputImage, bool isBitMap)
        {
            try
            {
                if (isBitMap)
                {
                    inputImage.Save(imageName, ImageFormat.Bmp);
                }
                else
                {
                    inputImage.Save(imageName, ImageFormat.Jpeg);
                }
            }
            catch (Exception ex)
            {
                MessageLoggerManager.Log.Alarm("Save screenshot fail!", ex);
            }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        private void MReconnectCameraTimer_Tick(object sender, EventArgs e)
        {
            if (mCvsInsightDisplay != null && !insightControls1.IsConnected && !mIsManualDisconnect && mCvsInsightDisplay.State != CvsDisplayState.Connecting)
            {
                try
                {
                    insightControls1.ConnectToSensor(mCurrentHostName);
                    insightControls1.GetInsightDisplay().ImageZoomMode = CvsDisplayZoom.Fit;
                    mCvsInsightDisplay = insightControls1.GetInsightDisplay();
                    MessageLoggerManager.Log.Warn("Auto reconnect Camera...");
                }
                catch (Exception ex)
                {
                    MessageLoggerManager.Log.Alarm("Auto reconnect Camera Fail!", ex);
                }
            }
        }
        private string RemoveTimeString(string message)
        {
            if (message != "")
                return message.Substring(message.IndexOf(" ") + 1, message.Length - message.IndexOf(" ") - 1);
            else
                return "Nan";
        }

        private void settingHWControl_AllSettingDoneEvent(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action)(() =>
                {
                    settingHWControl_AllSettingDoneEvent(sender, e);
                }));
            }
            else
            {
                settingHWControl1.Visible = false;
                btnSettingHWDone.Visible = false;
                groupBoxSaveImage.Visible = true;
                groupBoxPlayback.Visible = true;
                groupBoxHandEye.Visible = true;
            }
        }

        private void CtrlDisplayControl_EnableAllInspectionEvent(bool isEnable)
        {
            EnableAllInspection(isEnable);
        }

        private void IntrisicControl_AllSettingDoneEnvent(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action)(() =>
                {
                    IntrisicControl_AllSettingDoneEnvent(sender, e);
                }));
            }
            else
            {
                intrisicControl.Visible = false;
                btnIntrisicDone.Visible = false;
                groupBoxSaveImage.Visible = true;
                groupBoxPlayback.Visible = true;
                groupBoxHandEye.Visible = true;
            }

        }

        private void SetupControl_AllSettingDoneEvent(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action)(() =>
                {
                    SetupControl_AllSettingDoneEvent(sender, e);
                }));
            }
            else
            {
                AllSetupDone();
                groupBoxSaveImage.Visible = true;
                groupBoxPlayback.Visible = true;
                groupBoxHandEye.Visible = true;
            }
        }

        private void InspForm_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            //Set tooltip
            toolTip.SetToolTip(btnZoomFit, ResourceUtility.GetString("RtZoomFitToolTip"));
            toolTip.SetToolTip(btnZoomIn, ResourceUtility.GetString("RtZoomInToolTip"));
            toolTip.SetToolTip(btnZoomOut, ResourceUtility.GetString("RtZoomOutToolTip"));
            toolTip.SetToolTip(btnSaveImage, ResourceUtility.GetString("RtSaveImageToolTip"));
            toolTip.SetToolTip(btnLiveImage, ResourceUtility.GetString("RtLiveImageToolTip"));
            toolTip.SetToolTip(btnLogin, ResourceUtility.GetString("RtLoginToolTip"));
            toolTip.SetToolTip(btnOnline, ResourceUtility.GetString("RtOnline"));
            toolTip.SetToolTip(btnTeachMode, ResourceUtility.GetString("RtSensoMode"));
            toolTip.SetToolTip(btnSaveSetting, ResourceUtility.GetString("RtSaveSetting"));
            toolTip.SetToolTip(checkBoxSpreadsheet, ResourceUtility.GetString("RtShowSpreadsheet"));
            toolTip.SetToolTip(btnRotationClockwise, ResourceUtility.GetString("RtRotationClockwise"));
            toolTip.SetToolTip(btnRotationCounterClockwise, ResourceUtility.GetString("RtRotationCounterClockwise"));
            toolTip.SetToolTip(btnTrigger, ResourceUtility.GetString("RtManualTrigger"));
            toolTip.SetToolTip(chxHandPan, "Pan/Drag Image by mouse");

            PerformedRequiredInit();

            this.insightControls1.PropertyChanged += InsightControls1_PropertyChanged;
            this.insightControls1.Binding("ZoomImageToFit", this.btnZoomFit);
            this.insightControls1.Binding("ZoomImageOut", this.btnZoomOut);
            this.insightControls1.Binding("ZoomImageIn", this.btnZoomIn);
            this.insightControls1.Binding("LiveAcquire", this.btnLiveImage);
            this.insightControls1.Binding("Online", this.btnOnline);
            this.insightControls1.Binding("Spreadsheet", this.checkBoxSpreadsheet);
            //insightControls1.MouseWheel += InsightControls1_MouseWheel;
            checkBoxSpreadsheet.Checked = false;
            checkBoxSpreadsheet.Visible = false;

            if (File.Exists(@"C:\ProgramData\Cognex\Firmware\Guiding System\AS8402.key"))
            {
                checkBoxSpreadsheet.Visible = true;
            }

            groupBoxPlayback.Visible = false;
            groupBoxSaveImage.Visible = false;
            groupBoxHandEye.Visible = false;
        }

        private void MCvsInsightDisplay_CurrentCellChanged(object sender, EventArgs e)
        {
            string log = string.Empty;
            var tag = GetTag(ResourceUtility.GetString("Log_1"));
            if (tag != null)
            {
                log = (string)GetValue(tag.Location);
            }
            ContentQueue.gOnly.Info(log);
        }

        private void InsightControls1_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                PropertyChangedEventHandler delHandler = new PropertyChangedEventHandler(InsightControls1_PropertyChanged);
                Invoke(delHandler, sender, e);
            }
            else
            {
                string propertyName = e.PropertyName;
                switch (propertyName)
                {
                    case "HostNames":
                        {
                            comboBoxDevices.Items.Clear();
                            comboBoxDevices.Enabled = (this.insightControls1.HostNames.Count > 0);
                            foreach (var host in this.insightControls1.HostNames)
                            {
                                comboBoxDevices.Items.Add(host);
                            }

                            if (comboBoxDevices.Items.Count > 0)
                            {
                                this.btnConnect.Enabled = true;
                                if (this.insightControls1.IsConnected && (this.insightControls1.GetHostSensor() != null))
                                {
                                    comboBoxDevices.SelectedIndex = comboBoxDevices.Items.IndexOf(this.insightControls1.GetHostSensor().Name);
                                }
                                else
                                {
                                    comboBoxDevices.SelectedIndex = 0;
                                }
                            }
                        }
                        UpdateStatus("");
                        break;
                    case "IsConnected":
                        {
                            try
                            {
                                bool isControlConnected = this.insightControls1.IsConnected;
                                bool isDisplayConnected = this.mCvsInsightDisplay.Connected;
                                bool isConnected = isControlConnected || isDisplayConnected;
                                MessageLoggerManager.Log.Info(String.Format("{0} to sensor", isConnected ? "Connected" : "Disconnected"));
                                comboBoxDevices.SelectedIndex = comboBoxDevices.Items.IndexOf(mCurrentHostName);
                                UpdateStatus(isConnected ? ConnectStatus.Connected : ConnectStatus.Disconnected, "");
                                if (isConnected)
                                    UpdateControlsEnabled();
                            }
                            catch (Exception ex) { }
                        }
                        break;
                    case "StatusInfoChanged":
                        {
                            this.toolStripStatusStatus.Text = this.insightControls1.StatusInfoChanged;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void MFormSetPasswords_PasswordRequireChanged(object sender, EventArgs e)
        {
            //using (FormSetPasswords form = new FormSetPasswords(mCurrentPasswordFile))
            //{
            //    if (form.ShowDialog(this) == DialogResult.OK)
            //    {

            //    }
            //}
        }

        private void UpdateControlsEnabled(bool isAuto = false)
        {
            bool isAdminLevel = mCurrentAccessLevel == AccessLevel.Administrator;
            bool isSupervisorLevel = mCurrentAccessLevel == AccessLevel.Supervisor;
            if (isAuto == true)
            {
                isSupervisorLevel = true;
            }
            //checkBoxSpreadsheet.Visible = isAdminLevel;
            //this.btnOpenJob.Visible = isAdminLevel;
            //this.btnSaveJob.Visible = isAdminLevel;
            this.isDeleteOldFolder = isAdminLevel | isSupervisorLevel;
            this.lblExposureTime.Visible = isAdminLevel | isSupervisorLevel;
            this.numExposureTime.Visible = isAdminLevel | isSupervisorLevel;

            this.btnSetupInsp.Visible = isAdminLevel | isSupervisorLevel;
            this.btnIntrisic.Visible = isAdminLevel | isSupervisorLevel;
            this.btnSettingHW.Visible = isAdminLevel | isSupervisorLevel;
            this.btnOnline.Visible = isAdminLevel | isSupervisorLevel;
            this.btnTeachMode.Visible = isAdminLevel | isSupervisorLevel;

            groupBoxPlayback.Visible = isAdminLevel | isSupervisorLevel;
            groupBoxSaveImage.Visible = isAdminLevel | isSupervisorLevel;
            //set value for chechbox save log file
            if (groupBoxSaveImage.Visible)
            {

                var tag = GetTag("RtInspSaveLogFileAll");
                if (tag != null)
                {
                    chxSaveLogFileAll.Checked = (bool)GetValue(tag.Location);
                }
            }
            groupBoxHandEye.Visible = isAdminLevel | isSupervisorLevel;
            //chxSaveGraphicImage.Visible = isAdminLevel | isSupervisorLevel;
            //chxSaveRaw.Visible = isAdminLevel | isSupervisorLevel;
            //chxSaveScreen.Visible = isAdminLevel | isSupervisorLevel;
            //btnPlaybackFirst.Visible = isAdminLevel | isSupervisorLevel;
            //btnPlaybackFolder.Visible = isAdminLevel | isSupervisorLevel;
            //btnPlaybackLast.Visible = isAdminLevel | isSupervisorLevel;
            //btnPlaybackNext.Visible = isAdminLevel | isSupervisorLevel;
            //btnPlaybackPrevious.Visible = isAdminLevel | isSupervisorLevel;

            if (this.intrisicControl.Visible)
            {
                this.intrisicControl.Visible = isAdminLevel | isSupervisorLevel;
            }
            if (this.setupControl.Visible)
            {
                this.setupControl.Visible = isAdminLevel | isSupervisorLevel;
            }
            if (this.settingHWControl1.Visible)
            {
                this.settingHWControl1.Visible = isAdminLevel | isSupervisorLevel;
            }
            if (this.btnIntrisicDone.Visible)
            {
                this.btnIntrisicDone.Visible = isAdminLevel | isSupervisorLevel;
            }
            if (this.btnSetupInspDone.Visible)
            {
                this.btnSetupInspDone.Visible = isAdminLevel | isSupervisorLevel;
            }
            if (this.btnSettingHWDone.Visible)
            {
                this.btnSettingHWDone.Visible = isAdminLevel | isSupervisorLevel;
            }


            if (mCurrentAccessLevel > AccessLevel.Operator)
            {
                this.btnLogin.BackgroundImage = global::Cognex.VS.InspSensor.Properties.Resources.padlock_unlock;
            }
            else
            {
                this.btnLogin.BackgroundImage = global::Cognex.VS.InspSensor.Properties.Resources.locked_padlock;
            }

            if (this.insightControls1.IsConnected)
            {
                handEyeStatus1.Init(mCvsInsightDisplay);
            }

            try
            {
                if (this.insightControls1.IsConnected)
                {
                    string nameTag = string.Format("RtInspectionTeachingMode");
                    var tag = GetTag(ResourceUtility.GetString(nameTag));
                    if (tag != null)
                    {
                        bool isTeaching = (bool)GetValue(tag.Location);
                        if (!isTeaching)
                        {
                            btnTeachMode.Text = "Auto Mode";
                            btnTeachMode.BackColor = Color.Lime;
                        }
                        else
                        {
                            btnTeachMode.Text = "Teach Mode";
                            btnTeachMode.BackColor = Color.Red;
                        }
                    }
                }
                // update inspection exposure time to HMI 
                if (this.insightControls1.IsConnected)
                {
                    mCvsInsightDisplay = insightControls1.GetInsightDisplay();
                    var tag = GetTag(ResourceUtility.GetString("RtInspAcqExposure"));
                    if (tag != null)
                    {
                        float exposure = (float)GetValue(tag.Location);
                        numExposureTime.Value = (decimal)exposure;
                    }
                }
            }
            catch
            { }
        }

        private void PerformedRequiredInit()
        {
            string passwordfname = Utility.Utility.GetThisExecutableDirectory() + "passwords.txt";
            mCurrentPasswordFile = new PasswordFile(passwordfname);

            if (mCurrentPasswordFile.PasswordFileFound && !mCurrentPasswordFile.PasswordFileValid)
            {
                string quoted = "\"" + mCurrentPasswordFile.PasswordFilename + "\"";
                string message = ResourceUtility.FormatString("RtInvalidPasswordFile", quoted);
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            mCurrentPasswordFile.SetDefaultPassword(AccessLevel.Operator, "");
            mCurrentPasswordFile.SetDefaultPassword(AccessLevel.Administrator, mDefaultAdministratorPassword);
            mCurrentPasswordFile.SetDefaultPassword(AccessLevel.Supervisor, mDefaultSupervisorPassword);
        }

        public void SetSystemTitle(string title, bool isAllCap = false)
        {
            this.labelInspSystemLabel.Text = isAllCap ? title.ToUpper() : title;
        }

        public void SetConnectionStatus(ConnectStatus status)
        {
            string statusStr = status.ToString();
            this.labelSensorStatus.Text = ResourceUtility.GetString(String.Format("Rt{0}", statusStr));
        }

        private void timerSystem_Tick(object sender, EventArgs e)
        {
            //toolStripStatusClock.Text = String.Format("{0}{1}{2:yyyy/MM/dd HH:mm:ss}", "v1.1.0.",mDateOfVersion, DateTime.Now);
            toolStripStatusClock.Text = String.Format("{0} Released: {1} ", "Ver1.3.9", mDateOfVersion);
            labelInspSystemLabel.Text = String.Format("AS8402 Inspection    [{0:HH:mm:ss}]", DateTime.Now);
        }

        private void exitButton1_ButtonClick(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure to close program?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                try
                {
                    this.Cursor = Cursors.WaitCursor;
                    insightControls1.Cursor = Cursors.WaitCursor;
                    if (this.insightControls1.IsConnected)
                    {
                        // Disable Teaching mode
                        string nameTag = string.Format("RtInspectionTeachingMode");
                        var tag = GetTag(ResourceUtility.GetString(nameTag));
                        if (tag != null)
                        {
                            InsightSetValue(tag.Location, false);
                        }

                        //string jobName = "";
                        //tag = GetTag(ResourceUtility.GetString("RtJobName"));
                        //if (tag != null)
                        //{
                        //    jobName = (string)GetValue(tag.Location);
                        //}
                        //insightControls1.SaveJobRemote(this.insightControls1.GetRemotePath() + "//" + jobName);

                        //SaveJobSetting(false);

                        //Bring back Online State
                        if (!this.mCvsInsightDisplay.SoftOnline)
                        {
                            this.mCvsInsightDisplay.SoftOnline = true;
                        }
                        mCvsInsightDisplay.Disconnect();
                        //insightControls1.Dispose();
                    }
                    if (mSaveLogAndImageWorker.IsBusy)
                    {
                        mSaveLogAndImageWorker.CancelAsync();
                    }
                    SaveSettingToProperties();
                    MessageManager.MessageLoggerManager.Log.Info("[Action] Exit program...");
                    MessageManager.MessageLoggerManager.ShutDown();
                    Application.Exit();
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void SaveSettingToProperties()
        {
            Properties.Settings.Default.LogDirectory = txtLogDirectory.Text;
            Properties.Settings.Default.ImageDirectory = txtImageDirectory.Text;
            Properties.Settings.Default.IsSaveRaw = chxSaveRaw.Checked;
            Properties.Settings.Default.IsSaveGraphic = chxSaveGraphicImage.Checked;
            Properties.Settings.Default.IsSaveNGOnly = chxSaveNGOnly.Checked;
            Properties.Settings.Default.SavingDay = numSavedDay.Value.ToString();
            Properties.Settings.Default.Save();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            using (FormPasswordPrompt form = new FormPasswordPrompt(mCurrentPasswordFile, mCurrentAccessLevel))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    mCurrentAccessLevel = form.CurrentAccessLevel;
                    toolStripCurrentAccess.Text = mCurrentAccessLevel.ToString();
                    UpdateControlsEnabled();
                }
            }
        }

        /// <summary>
        /// Updates status with the specified string.
        /// </summary>
        /// <param name="message"></param>
        private void UpdateStatus(string message)
        {
            UpdateStatus(ConnectStatus.NA, message);
        }

        /// <summary>
        /// Updates the message in the status bar.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        private void UpdateStatus(ConnectStatus state, string message)
        {
            string statusText = "";
            toolStripStatusConnection.Tag = statusText;

            switch (state)
            {
                case ConnectStatus.Connecting:
                    statusText = "Connecting...";
                    break;
                case ConnectStatus.Disconnecting:
                    statusText = "Disconnecting...";
                    break;
                case ConnectStatus.Connected:
                    statusText = "Connected";
                    break;
                case ConnectStatus.Disconnected:
                    statusText = "Not connected";
                    break;
                case ConnectStatus.NA:
                    break;
            }

            toolStripStatusConnection.Text = statusText;

            //Update Control
            if (ConnectStatus.Connected == state)
            {
                this.btnConnect.Text = "Disconnect";
            }
            else
            {
                this.btnConnect.Text = "Connect";
            }


            //Title
            if (this.comboBoxDevices.Items.Count > 0)
            {
                if (ConnectStatus.Connected == state)
                    this.labelSensorStatus.Text = String.Format("Connect to {0}", this.comboBoxDevices.SelectedItem as String);
                else
                    this.labelSensorStatus.Text = "";
            }
            else
                this.labelSensorStatus.Text = "No sensor found";
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (mFormPasswordPromp != null) Dispose();
                if (disposing && (components != null))
                {
                    components.Dispose();
                }
                base.Dispose(disposing);
            }
            catch (Exception ex)
            { }
        }
        private bool mIsManualDisconnect = false;
        private void btnConnect_Click(object sender, EventArgs e)
        {
            btnTeachMode.Text = "Auto Mode";
            btnTeachMode.BackColor = Color.Lime;

            if (btnConnect.Text.Contains("Dis"))
            {
                lblExposureTime.Visible = false;
                numExposureTime.Visible = false;

                btnIntrisic.Visible = false;
                btnIntrisicDone.Visible = false;
                btnSetupInsp.Visible = false;
                btnSetupInspDone.Visible = false;
                btnSettingHW.Visible = false;
                btnSettingHWDone.Visible = false;

                setupControl.Visible = false;
                settingHWControl1.Visible = false;
                intrisicControl.Visible = false;

                groupBoxSaveImage.Visible = false;
                groupBoxPlayback.Visible = false;
                groupBoxHandEye.Visible = false;
                //chxSaveGraphicImage.Visible = false;
                //chxSaveRaw.Visible = false;
                //chxSaveScreen.Visible = false;
                //btnPlaybackFirst.Visible = false;
                //btnPlaybackFolder.Visible = false;
                //btnPlaybackLast.Visible = false;
                //btnPlaybackNext.Visible = false;
                //btnPlaybackPrevious.Visible = false;

                //mSyncMessageTimer.Stop();
                if (mSaveLogAndImageWorker.IsBusy)
                {
                    mSaveLogAndImageWorker.CancelAsync();
                }

                // Disable Teaching mode
                string nameTag = string.Format("RtInspectionTeachingMode");
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, false);
                }
                mIsManualDisconnect = true;
            }
            else
            {
                mSensorName = comboBoxDevices.Text;
                //mSyncMessageTimer.Start();
                if (!mSaveLogAndImageWorker.IsBusy)
                {
                    mSaveLogAndImageWorker.RunWorkerAsync();
                }
                mIsManualDisconnect = false;
            }

            if (comboBoxDevices.SelectedItem != null)
            {
                this.insightControls1.ConnectToSensor(comboBoxDevices.SelectedItem.ToString());
                mCurrentHostName = comboBoxDevices.SelectedItem.ToString();
                mReconnectCameraTimer.Start();
            }
            this.insightControls1.GetInsightDisplay().ImageZoomMode = CvsDisplayZoom.Fit;
            mCvsInsightDisplay = insightControls1.GetInsightDisplay();

            MessageManager.MessageLoggerManager.Log.Info("[Action] Connect/Disconect...");
        }

        private void btnRotationClockwise_Click(object sender, EventArgs e)
        {
            this.insightControls1.SetImgOrientation(90);
        }

        private void btnRotationCounterClockwise_Click(object sender, EventArgs e)
        {
            this.insightControls1.SetImgOrientation(-90);
        }

        private void checkBoxSpreadsheet_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkbox = sender as CheckBox;
            //if (checkbox != null && checkbox.Checked)
            //    MessageLoggerManager.Log.Alarm("Show spreadsheet");
            //else
            //    MessageLoggerManager.Log.Info("Hide spreadsheet");
        }

        private void BtnLiveImage_CheckedChanged(object sender, EventArgs e)
        {
            //MessageLoggerManager.Log.Info(String.Format("{0} Live mode", (!this.insightControls1.LiveModeActivated ? "Turn on" : "Turn off")));
            if (btnLiveImage.Checked)
            {
                btnLiveImage.Image = Properties.Resources.Live1_on;
            }
            else
            {
                btnLiveImage.Image = Properties.Resources.Live1;
            }
        }

        private void BtnOpenJob_Click(object sender, EventArgs e)
        {
            //FolderBrowserDialog fbd = new FolderBrowserDialog();
            //fbd.ShowDialog();
            //string sPath = fbd.SelectedPath;

            mFileJobBrowser = null;
            mFileJobBrowser = new FileJobBrowser(this.insightControls1.GetRemotePath(), "Open");
            if (mFileJobBrowser.ShowDialog() == DialogResult.OK)
            {
                if (mFileJobBrowser.IsSensor())
                    this.insightControls1.OpenJobRemote(mFileJobBrowser.GetRemotePath());
                else
                    this.insightControls1.OpenJob(mFileJobBrowser.GetLocalPath());
            }

        }
        private void BtnSaveImage_Click_1(object sender, EventArgs e)
        {
            //SaveFileDialog SFD = new SaveFileDialog();
            //SFD.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
            //SFD.Title = "Save an Image File";
            //if (SFD.ShowDialog() == DialogResult.OK)
            //{
            //    this.insightControls1.SaveImage(SFD.FileName);
            //}

            Stopwatch sw = new Stopwatch();
            sw.Start();
            CvsFtpClient ftp = new CvsFtpClient();
            ftp.Connect(System.Net.IPAddress.Parse(insightControls1.HostIPAddress), "admin", "");
            DateTime now = DateTime.Now;
            string imagePath = string.Empty;
            string imageName = string.Empty;
            string command = "MasterImage";
            string origin1X = this.setupControl.ConveyorInspection.Origin1X;
            string origin2X = this.setupControl.ConveyorInspection.Origin2X;
            string origin1Y = this.setupControl.ConveyorInspection.Origin1Y;
            string origin2Y = this.setupControl.ConveyorInspection.Origin2Y;
            string origin = "Feature1 " + origin1X + "," + origin1Y + "_Feature2 " + origin2X + "," + origin2Y;
            try
            {
                string imageDirectory = Environment.CurrentDirectory;
                imageName = string.Format("{0}_{1}_{2}_{3}", mSensorName, command, origin, now.ToString("HHmmss_fff"));
                imagePath = imageDirectory + "\\Master\\" + now.ToString("MMddyyyy") + "\\";
                imageName += ".bmp";
                string imageFileName = imagePath + imageName;

                if (!System.IO.Directory.Exists(imagePath))
                    System.IO.Directory.CreateDirectory(imagePath);

                ftp.GetFileFromInSight(imageFileName, "image.bmp");
                ftp.Disconnect();
                sw.Stop();
                MessageLoggerManager.Log.Info("Save Master Image done: " + sw.ElapsedMilliseconds.ToString() + " ms");
                Process.Start("explorer.exe", imagePath);
            }
            catch (Exception ex) { };
        }
        private void BtnTrigger_Click(object sender, EventArgs e)
        {
            this.insightControls1.ManualTrigger();
        }

        private void BtnConfigure_Click(object sender, EventArgs e)
        {
            //this.insightControls1.Configure();
            setupControl.InitAllSetting(insightControls1);
            setupControl.Visible = true;
            btnSetupInspDone.Visible = true;
            intrisicControl.Visible = false;
            btnIntrisicDone.Visible = false;

            settingHWControl1.Visible = false;
            btnSettingHWDone.Visible = false;
            //try
            //{
            //    mCvsInsightDisplay = insightControls1.GetInsightDisplay();
            //    var tag = GetTag(ResourceUtility.GetString("RtInspAcqExposure"));
            //    if (tag != null)
            //    {
            //        float exposure = (float)GetValue(tag.Location);
            //        numExposureTime.Value = (decimal)exposure;
            //    }
            //}
            //catch (Exception ex)
            //{

            //}
            lblExposureTime.Visible = true;
            numExposureTime.Visible = true;

            groupBoxSaveImage.Visible = false;
            groupBoxPlayback.Visible = false;
            groupBoxHandEye.Visible = false;
            try
            {
                // Enable Teaching mode
                string nameTag = string.Format("RtInspectionTeachingMode");
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, true);
                }
                btnTeachMode.Text = "Teach Mode";
                btnTeachMode.BackColor = Color.Red;
            }
            catch (Exception ex)
            {

            }
            MessageManager.MessageLoggerManager.Log.Info("[Action] Go to Setup Insp...");
            MessageManager.MessageLoggerManager.Log.Info("[Action] Change Camera Mode: " + btnTeachMode.Text);
        }

        private void exitButton1_Load(object sender, EventArgs e)
        {

        }

        private void btnSaveJob_Click(object sender, EventArgs e)
        {
            mFileJobBrowser = null;
            mFileJobBrowser = new FileJobBrowser(this.insightControls1.GetRemotePath(), "Save");
            if (mFileJobBrowser.ShowDialog() == DialogResult.OK)
            {
                if (mFileJobBrowser.IsSensor())
                    this.insightControls1.SaveJobRemote(mFileJobBrowser.GetRemotePath());
                else
                    this.insightControls1.SaveJob(mFileJobBrowser.GetLocalPath());
            }

        }

        private void btnSetupDone_Click(object sender, EventArgs e)
        {
            AllSetupDone();
            groupBoxSaveImage.Visible = true;
            groupBoxPlayback.Visible = true;
            groupBoxHandEye.Visible = true;
        }

        private void AllSetupDone()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action)(() =>
                {
                    AllSetupDone();
                }));
            }
            else
            {
                setupControl.Visible = false;
                btnSetupInspDone.Visible = false;
                try
                {
                    // Disable Teaching mode
                    string nameTag = string.Format("RtInspectionTeachingMode");
                    var tag = GetTag(ResourceUtility.GetString(nameTag));
                    if (tag != null)
                    {
                        InsightSetValue(tag.Location, false);
                    }
                    btnTeachMode.Text = "Auto Mode";
                    btnTeachMode.BackColor = Color.Lime;
                    if (!this.mCvsInsightDisplay.SoftOnline)
                    {
                        this.mCvsInsightDisplay.SoftOnline = true;
                    }

                }
                catch (Exception ex)
                { }
            }
        }

        private CvsSymbolicTag GetTag(string tagName)
        {
            try
            {
                if (mCvsInsightDisplay != null)
                {
                    var tags = mCvsInsightDisplay.InSight.GetSymbolicTagCollection();
                    CvsSymbolicTag tag = tags[tagName];
                    return tag;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("GetTag: " + ex.Message, "Error");
            }

            return null;
        }

        private void EditCellGraphic(CvsCellLocation location)
        {
            mCvsInsightDisplay.EditCellGraphic(location);
        }

        private void InsightClickButton(CvsCellLocation location)
        {
            mCvsInsightDisplay.InSight.ClickButton(location);
        }

        private void InsightSetListBoxValue(CvsCellLocation location, int value)
        {
            mCvsInsightDisplay.InSight.SetListBoxIndex(location, value);
        }

        private void InsightSetValue(CvsCellLocation location, float value)
        {
            mCvsInsightDisplay.InSight.SetFloat(location, value);
        }

        private void InsightSetValue(CvsCellLocation location, int value)
        {
            mCvsInsightDisplay.InSight.SetInteger(location, value);
        }

        private void InsightSetValue(CvsCellLocation location, string value)
        {
            mCvsInsightDisplay.InSight.SetString(location, value);
        }

        private void InsightSetValue(CvsCellLocation location, bool value)
        {
            mCvsInsightDisplay.InSight.SetCheckBox(location, value);
        }

        private object GetValue(CvsCellLocation location)
        {
            Cognex.InSight.Cell.CvsCell c = mCvsInsightDisplay.InSight.Results.Cells[location];
            if (c.GetType() == typeof(Cognex.InSight.Cell.CvsCellEditFloat))
            {
                return ((Cognex.InSight.Cell.CvsCellEditFloat)c).Value;
            }
            else if (c.GetType() == typeof(Cognex.InSight.Cell.CvsCellEditInt))
            {
                return ((Cognex.InSight.Cell.CvsCellEditInt)c).Value;
            }
            else if (c.GetType() == typeof(Cognex.InSight.Cell.CvsCellEditString))
            {
                return ((Cognex.InSight.Cell.CvsCellEditString)c).Text;
            }
            else if (c.GetType() == typeof(Cognex.InSight.Cell.CvsCellCheckBox))
            {
                return ((Cognex.InSight.Cell.CvsCellCheckBox)c).Checked;
            }
            return null;
        }

        private void numExposureTime_ValueChanged(object sender, EventArgs e)
        {
            string nameTag = "RtInspAcqExposure";
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                InsightSetValue(tag.Location, (float)numExposureTime.Value);
            }

            nameTag = "AcquisitionSettings.Selector";
            tag = GetTag(nameTag);
            if (tag != null)
            {
                InsightSetListBoxValue(tag.Location, 2);
            }
        }

        private void btnIntrisic_Click(object sender, EventArgs e)
        {
            //this.insightControls1.Configure();
            intrisicControl.InitAllSetting(insightControls1);
            intrisicControl.Visible = true;
            btnIntrisicDone.Visible = true;
            setupControl.Visible = false;
            btnSetupInspDone.Visible = false;

            settingHWControl1.Visible = false;
            btnSettingHWDone.Visible = false;
            groupBoxSaveImage.Visible = false;
            groupBoxPlayback.Visible = false;
            groupBoxHandEye.Visible = false;
            MessageManager.MessageLoggerManager.Log.Info("[Action] Go to Intrinsic...");
        }

        private void btnIntrisicDone_Click(object sender, EventArgs e)
        {
            intrisicControl.Visible = false;
            btnIntrisicDone.Visible = false;
            groupBoxSaveImage.Visible = true;
            groupBoxPlayback.Visible = true;
            groupBoxHandEye.Visible = true;
        }

        private void btnSaveSetting_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure to save setting?", "Notification", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                SaveJobSetting(false);
            }

            MessageManager.MessageLoggerManager.Log.Info("[Action] Save Setting...");
        }

        private void SaveJobSetting(bool isNotify)
        {
            string jobName = "";
            var tag = GetTag(ResourceUtility.GetString("RtJobName"));
            if (tag != null)
            {
                jobName = (string)GetValue(tag.Location);
            }
            if (jobName.Contains("ftp"))
            {
                insightControls1.SaveJobRemote(jobName);
                if (isNotify)
                    MessageBox.Show(string.Format("Save setting done to ftp {0}!", jobName), "Notification");
            }
            else
            {
                insightControls1.SaveJobRemote(this.insightControls1.GetRemotePath() + "//" + jobName);
                if (isNotify)
                    MessageBox.Show(string.Format("Save setting done to camera {0}!", jobName), "Notification");
            }

        }
        private void EnableAllInspection(bool bEnable)
        {
            try
            {
                string[] listEnble = new string[] {
                    "RtInspBlobEnable",
                    "RtInspBlobPanelEnable",
                    "RtInspVisEnable",
                    "RtInspPanelEnableF",
                    "RtInspPanelEnableR",
                    "RtInspTrayEnableF",
                    "RtInspTrayEnableR" };
                foreach (string nameTag in listEnble)
                {
                    var tag = GetTag(ResourceUtility.GetString(nameTag));
                    if (tag != null)
                    {
                        InsightSetValue(tag.Location, bEnable);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void btnSettingHW_Click(object sender, EventArgs e)
        {
            settingHWControl1.InitAllSetting(insightControls1, mCvsInsightDisplay, comboBoxDevices.SelectedItem.ToString());
            intrisicControl.Visible = false;
            btnIntrisicDone.Visible = false;
            setupControl.Visible = false;
            btnSetupInspDone.Visible = false;
            settingHWControl1.Visible = true;
            btnSettingHWDone.Visible = true;

            groupBoxSaveImage.Visible = false;
            groupBoxPlayback.Visible = false;
            groupBoxHandEye.Visible = false;
            MessageManager.MessageLoggerManager.Log.Info("[Action] Go to Setting HW...");
        }

        private void btnSettingHWDone_Click(object sender, EventArgs e)
        {
            settingHWControl1.Visible = false;
            btnSettingHWDone.Visible = false;
            groupBoxSaveImage.Visible = true;
            groupBoxPlayback.Visible = true;
            groupBoxHandEye.Visible = true;
        }
        private void btnClearLog_Click(object sender, EventArgs e)
        {
            messageViewerGUI1.ClearMessages();
        }

        private void chxSaveRaw_CheckedChanged(object sender, EventArgs e)
        {
            if (chxSaveRaw.Checked)
            {
                chxSaveRaw.BackColor = Color.Lime;
            }
            else
            {
                chxSaveRaw.BackColor = Color.Tomato;
            }
        }

        private void chxSaveScreen_CheckedChanged(object sender, EventArgs e)
        {
            if (chxSaveNGOnly.Checked)
            {
                chxSaveNGOnly.BackColor = Color.Lime;
            }
            else
            {
                chxSaveNGOnly.BackColor = Color.Tomato;
            }
        }

        private void chxSaveGraphicImage_CheckedChanged(object sender, EventArgs e)
        {
            if (chxSaveGraphicImage.Checked)
            {
                chxSaveGraphicImage.BackColor = Color.Lime;
            }
            else
            {
                chxSaveGraphicImage.BackColor = Color.Tomato;
            }
        }


        private void btnPlaybackFolder_Click(object sender, EventArgs e)
        {
            MessageManager.MessageLoggerManager.Log.Info("[Action] Open Playback Image Folder...");
            if (mCvsInsightDisplay != null)
            {
                mCvsInsightDisplay.Edit.EasyBuilderPlaybackOptions.Execute();

            }
            else
            {
                MessageBox.Show("Sensor is disconnected!", "Warn");
            }
        }

        private void btnPlaybackNext_Click(object sender, EventArgs e)
        {
            if (mCvsInsightDisplay != null)
            {
                MessageManager.MessageLoggerManager.Log.Info("[Action] Playback Image Next...");
                mCvsInsightDisplay.Edit.PlayNext.Execute();
            }
        }

        private void btnPlaybackPrevious_Click(object sender, EventArgs e)
        {
            if (mCvsInsightDisplay != null)
            {
                MessageManager.MessageLoggerManager.Log.Info("[Action] Playback Image Previous...");
                mCvsInsightDisplay.Edit.PlayPrevious.Execute();
            }
        }

        private void btnPlaybackFirst_Click(object sender, EventArgs e)
        {
            if (mCvsInsightDisplay != null)
            {
                MessageManager.MessageLoggerManager.Log.Info("[Action] Playback Image First...");
                mCvsInsightDisplay.Edit.PlayFirst.Execute();
            }
        }

        private void btnPlaybackLast_Click(object sender, EventArgs e)
        {
            if (mCvsInsightDisplay != null)
            {
                MessageManager.MessageLoggerManager.Log.Info("[Action] Playback Image Last...");
                mCvsInsightDisplay.Edit.PlayLast.Execute();
            }
        }
        private void InsightControls1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (mCvsInsightDisplay != null)
            {
                if (e.Delta > 0)
                {
                    mCvsInsightDisplay.Edit.ZoomImageIn.Execute();
                }
                else
                {
                    mCvsInsightDisplay.Edit.ZoomImageOut.Execute();
                }
            }
        }

        private void mainPanel2SplitContainer_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnTeachMode_Click(object sender, EventArgs e)
        {
            if (btnTeachMode.Text == "Teach Mode")
            {
                string nameTag = string.Format("RtInspectionTeachingMode");
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, false);
                }

                btnTeachMode.Text = "Auto Mode";
                btnTeachMode.BackColor = Color.Lime;
            }
            else
            {
                string nameTag = string.Format("RtInspectionTeachingMode");
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, true);
                }

                btnTeachMode.Text = "Teach Mode";
                btnTeachMode.BackColor = Color.Red;
            }
            MessageManager.MessageLoggerManager.Log.Info("[Action] Change Camera Mode: " + btnTeachMode.Text);

        }

        private void labelSensorStatus_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //MessageBox.Show("click");
            //this.WindowState = FormWindowState.Maximized;
            if (e.Button == MouseButtons.Left)
            {
                if (this.WindowState == FormWindowState.Maximized)
                {
                    this.WindowState = FormWindowState.Normal;
                    this.Size = new Size(1280, 768);
                }
                else
                {
                    this.WindowState = FormWindowState.Maximized;
                }
            }
        }

        private void labelSensorStatus_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.WindowState = FormWindowState.Minimized;
            }
        }

        private void btnOnline_CheckedChanged(object sender, EventArgs e)
        {
            if (btnOnline.Checked)
            {
                btnOnline.Image = Properties.Resources.power_on;
            }
            else
            {
                btnOnline.Image = Properties.Resources.power;
            }
        }

        private void btnOpenImageFolder_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("explorer.exe", mImageDirectory);
            }
            catch (Exception ex)
            { }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            //CvsImage cvsImage = insightControls1.InSight.Results.Image;
            //Bitmap imageRaw = cvsImage.ToBitmap();
            //SaveScreenImage("Test", imageRaw, true);

            //var fileManager = new CvsFileManager();
            //fileManager.GetFileFromInSight("E:\\AS8402.bmp","image.bmp")
            CvsFtpClient ftp = new CvsFtpClient();
            ftp.Connect(System.Net.IPAddress.Parse(insightControls1.HostIPAddress), "admin", "");
            ftp.GetFileFromInSight("E:\\AS8402.bmp", "image.bmp");
        }

        private bool mIsMouseDown = false;
        private Point mLastFormLocation;
        private void labelSensorStatus_MouseDown(object sender, MouseEventArgs e)
        {
            mIsMouseDown = true;
            mLastFormLocation = e.Location;
        }

        private void labelSensorStatus_MouseMove(object sender, MouseEventArgs e)
        {
            if (mIsMouseDown && this.WindowState != FormWindowState.Maximized)
            {
                this.Location = new Point((this.Location.X - mLastFormLocation.X) + e.X, (this.Location.Y - mLastFormLocation.Y) + e.Y);
                this.Update();
            }
        }

        private void labelSensorStatus_MouseUp(object sender, MouseEventArgs e)
        {
            mIsMouseDown = false;
        }

        private void toolStripStatusClock_Click(object sender, EventArgs e)
        {
            if (checkBoxSpreadsheet.Checked)
            {
                if (mCvsInsightDisplay != null)
                {
                    mCvsInsightDisplay.OpenPropertySheet(mCvsInsightDisplay.CurrentCellNow, null);
                }
            }
        }

        private void btnContinue_Click(object sender, EventArgs e)
        {
            if (btnContinue.Text == "Continue")
            {
                mPlaybackTimer.Interval = int.Parse(txtDelayPlayback.Text);
                mPlaybackTimer.Start();
                btnContinue.Text = "Stop";
            }
            else
            {
                mPlaybackTimer.Stop();
                btnContinue.Text = "Continue";
            }
            MessageManager.MessageLoggerManager.Log.Info("[Action] Playback Image Mode: " + btnContinue.Text);
        }

        private void MPlaybackTimer_Tick(object sender, EventArgs e)
        {
            if (mCvsInsightDisplay != null)
            {
                MessageManager.MessageLoggerManager.Log.Info("[Action] Playback Image Next...");
                mCvsInsightDisplay.Edit.PlayNext.Execute();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            MessageManager.MessageLoggerManager.Log.Info("[Action] HMI Display Changed...");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                this.Size = new Size(1280, 768);
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
            }
            MessageManager.MessageLoggerManager.Log.Info("[Action] HMI Display Changed...");
        }

        private void btnLogOpenFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.ShowNewFolderButton = true;
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                mLogDirectory = folderBrowserDialog.SelectedPath;
                txtLogDirectory.Text = folderBrowserDialog.SelectedPath;
                MessageManager.MessageLoggerManager.Log.Info("[Action] Save Log Directory Changed...");
                SaveSettingToProperties();
                if (MessageBox.Show("Do you want to restart program to update new Log Directory? \r\nOtherwise, Log is still saved in old folder.", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                {
                    Application.Restart();
                    Environment.Exit(0);
                }
            }
        }

        private void btnImageOpenFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.ShowNewFolderButton = true;
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                mImageDirectory = folderBrowserDialog.SelectedPath;
                txtImageDirectory.Text = folderBrowserDialog.SelectedPath;
                MessageManager.MessageLoggerManager.Log.Info("[Action] Save Image Directory Changed...");
                SaveSettingToProperties();
            }
        }

        private void chxHandPan_CheckedChanged(object sender, EventArgs e)
        {
            //MessageLoggerManager.Log.Info(String.Format("{0} Live mode", (!this.insightControls1.LiveModeActivated ? "Turn on" : "Turn off")));
            if (chxHandPan.Checked)
            {
                chxHandPan.Image = Properties.Resources.Hand_On;
                insightControls1.CanPanImage = true;
            }
            else
            {
                chxHandPan.Image = Properties.Resources.Hand_Off;
                insightControls1.CanPanImage = false;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //string jobName = "";
            //var tag = GetTag(ResourceUtility.GetString("RtJobName"));
            //if (tag != null)
            //{
            //    jobName = (string)GetValue(tag.Location);
            //}
            //insightControls1.InSight.File.SaveJobFile(jobName, false);
            string jobName = string.Empty;
            string mJobFilePath = "ftp:\\192.168.3.2:21\\\\001_jobfile.job";
            int index = mJobFilePath.LastIndexOf("\\") + 1 ;
            jobName = mJobFilePath.Substring(index, mJobFilePath.Length - index);
        }

        private void chxSaveLogFileAll_CheckedChanged(object sender, EventArgs e)
        {
            var tag = GetTag("RtInspSaveLogFileAll");
            if (tag != null)
            {
                InsightSetValue(tag.Location, chxSaveLogFileAll.Checked);
            }
            if (chxSaveLogFileAll.Checked)
            {
                chxSaveLogFileAll.BackColor = Color.Lime;
                MessageLoggerManager.Log.Info(String.Format("[Action] Save log file all: Checked"));
            }
            else
            {
                chxSaveLogFileAll.BackColor = Color.Tomato;
                MessageLoggerManager.Log.Info(String.Format("[Action] Save log file all: Unchecked"));
            }
        }
    }
}
