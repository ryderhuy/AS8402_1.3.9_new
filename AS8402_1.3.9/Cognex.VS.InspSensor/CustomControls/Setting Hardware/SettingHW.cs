using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
using Cognex.VS.Utility;
using System.Net;
using Cognex.InSight.Controls.Display;
using System.Net.NetworkInformation;
using Cognex.InSight;
using Cognex.InSight.Controls;
using MessageManager;
using Cognex.InSight.Sensor;

namespace Cognex.VS.InspSensor.CustomControls.Setting_Hardware
{
    public partial class SettingHW : UserControl
    {
        private TcpClient mVisionTcpClient;
        private NetworkStream mVisionTcpStream;
        private bool mIsVisionConnected = false;
        private int mVisionTcpConnectTimeOut = 20000;
        private int mVisionTcpSendTimeOut = 20000;
        private int mVisionTcpReceiveTimeOut = 20000;
        Timer reconnectTimer = new Timer();
        public EventHandler SettingDoneEvent;
        public EventHandler ExitProgramByFactoryResetEvent;
        InSightControl.InsightControls mInSightControl;
        CvsInSightDisplay mCvsInsightDisplay;
        string mHostName = string.Empty;
        CvsInSightDisplayEdit mCvsInSightDisplayEdit;
        public SettingHW()
        {
            InitializeComponent();
            reconnectTimer.Interval = 1000;
            reconnectTimer.Tick += ReconnectTimer_Tick;
        }

        private void ReconnectTimer_Tick(object sender, EventArgs e)
        {
            bool isSuccess = false;

            isSuccess = PingHost(mInSightControl.HostIPAddress);
            ContentQueue.gOnly.Info("Connecting...");

            if (isSuccess)
            {
                ConnectAgainWithCurrentHostName();
            }
        }

        public void Init(InSightControl.InsightControls inSightControl, CvsInSightDisplay cvsInSightDisplay, string hostName)
        {
            mInSightControl = inSightControl;
            mCvsInsightDisplay = cvsInSightDisplay;
            txtCameraAddress.Text = mInSightControl.HostIPAddress;
            mHostName = hostName;
            mInSightControl.InSight.SoftOnline = false;
            mCvsInSightDisplayEdit = mCvsInsightDisplay.Edit;
            if (mCvsInsightDisplay.Connected)
            {
                UpdateGUIParams();
            }

            /*
            //mCvsInsightDisplay.cell
            mCvsInsightDisplay.SetCurrentCell(2, 2);

            //mCvsInSightDisplayEdit.ExportSnippet.Execute();
            
            int col = mCvsInsightDisplay.CurrentCellNow.Column;
            int row = mCvsInsightDisplay.CurrentCellNow.Row;
            int r1 = mCvsInsightDisplay.SelectedRange.Row;
            int c1 = mCvsInsightDisplay.SelectedRange.Column;
            int rn = mCvsInsightDisplay.SelectedRange.Rows;
            int cn = mCvsInsightDisplay.SelectedRange.Columns;
            CvsAction.SetEnabled(mCvsInsightDisplayEdit.ExportSnippet, true);
            bool isEnable = mCvsInsightDisplayEdit.ExportSnippet.Enabled;
            
            mCvsInsightDisplayEdit.ExportSnippet.Execute();
            */

        }

        private void ConnectToVision(string address, int port)
        {
            if (mVisionTcpClient != null && mVisionTcpClient.Connected)
                mVisionTcpClient.Close();

            mVisionTcpClient = new TcpClient();
            if (!mVisionTcpClient.Connected)
            {
                mIsVisionConnected = mVisionTcpClient.ConnectAsync(address, port).Wait(mVisionTcpConnectTimeOut);

                if (!mIsVisionConnected)
                    return;

                mVisionTcpClient.ReceiveTimeout = mVisionTcpSendTimeOut;
                mVisionTcpClient.SendTimeout = mVisionTcpReceiveTimeOut;
                mVisionTcpStream = mVisionTcpClient.GetStream();
                mVisionTcpClient.NoDelay = true;
                mVisionTcpClient.Client.NoDelay = true;
                //ContentQueue.gOnly.Info(String.Format("{0} to sensor", mIsVisionConnected ? "Connected" : "Disconnected"));
            }

        }

        private void DisconnectToVision()
        {
            if (mVisionTcpClient != null && mVisionTcpClient.Connected)
            {
                mVisionTcpClient.Close();
                mVisionTcpClient = null;
                mVisionTcpStream = null;
                mIsVisionConnected = false;
            }
        }
        private string SendCommandToVision(string cmd, bool isWaitingResponse = true)
        {
            try
            {
                NetworkStream ns = mVisionTcpStream;
                byte[] cmdData = Encoding.ASCII.GetBytes(cmd);

                try
                {
                    ns.Write(cmdData, 0, cmdData.Length);
                    ns.Flush();


                }
                catch (Exception ex)
                {
                    //IsVisionConnected = false;
                    //throw new Exception("Socket Write Error");
                    MessageLoggerManager.Log.Warn("Exception: SendCommandToVision, Write", ex);
                }
                if (isWaitingResponse)
                {
                    //System.Threading.Thread.Sleep(100);
                    var buff = new byte[1024];
                    using (var ms = new MemoryStream())
                    {

                        try
                        {

                            do
                            {
                                //ns.ReadTimeout = mTimeOutInMS;
                                while (!ns.DataAvailable)
                                {
                                    System.Threading.Thread.Sleep(5);
                                }
                                int sz = ns.Read(buff, 0, 255);
                                if (sz == 0)
                                {
                                    throw new Exception("Disconnected");
                                }
                                else
                                {
                                    break;
                                }
                            } while (ns.DataAvailable);
                        }
                        catch (Exception ex)
                        {
                            //throw new Exception("Socket read error");
                            MessageLoggerManager.Log.Warn("Exception: SendCommandToVision, Read", ex);
                        }
                        return Encoding.ASCII.GetString(buff);
                    }
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                MessageLoggerManager.Log.Alarm("Exception: SendCommandToVision, Connect", ex);
                if (ex.Message.Contains("Socket"))
                {
                    mIsVisionConnected = false;
                    return "404";
                }
                else
                {
                    return "";
                }

            }

        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            try
            {
                SettingDoneEvent.BeginInvoke(null, null, null, null);
            }
            catch (Exception ex)
            {

            }
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to discard current setting?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                SettingDoneEvent.BeginInvoke(null, null, null, null);
            }
        }

        private void btnSaveHW_Click(object sender, EventArgs e)
        {
            MessageManager.MessageLoggerManager.Log.Info("[Action] Go to Setting Networks...");
            mCvsInsightDisplay.Edit.EditNetworkSettings.Execute();
        }

        private void WriteMCSetting()
        {
            //string nameTag = "RtPLCAddress";
            //var tag = GetTag(ResourceUtility.GetString(nameTag));
            //if (tag != null)
            //{
            //    InsightSetValue(tag.Location, txtPLCAddress.Text);
            //}

            //nameTag = "RtPLCOutput";
            //tag = GetTag(ResourceUtility.GetString(nameTag));
            //if (tag != null)
            //{
            //    InsightSetValue(tag.Location, txtOutputAddress.Text);
            //}

            //nameTag = "RtPLCPort";
            //tag = GetTag(ResourceUtility.GetString(nameTag));
            //if (tag != null)
            //{
            //    InsightSetValue(tag.Location, int.Parse(txtPLCPort.Text));
            //}
        }

        private void WriteSLMPSetting()
        {
            //bool mIsWriteSettingDone = false;
            //ConnectToVision(txtCameraAddress.Text, 23);
            //string str = SendCommandToVision("");
            //str = SendCommandToVision("admin\r\n");
            //if (str.Contains("Password"))
            //{
            //    str = SendCommandToVision("\r\n");
            //    if (str.Contains("Logged"))
            //    {
            //        str = SendCommandToVision("EV SetSystemConfig(\"ServicesEnabled\",16,127)\r\n");
            //        if (str.Contains("1"))
            //        {
            //            System.Threading.Thread.Sleep(500);
            //            string setting = string.Format("(\"{0}\",{1},3,1000,100,0,255,1023,\"{2}{3}\",\"{4}{5}\",\"{6}{7}\",50,\"{8}{9}\",29,\"\",1,\"\",1)"
            //                , txtPLCAddress.Text, txtPLCPort.Text
            //                , txtCBRegister.Text, txtCBOffset.Text
            //                , txtSBRegister.Text, txtSBOffset.Text
            //                , txtOBRegister.Text, txtOBOffset.Text
            //                , txtIBRegister.Text, txtIBOffset.Text
            //                );
            //            string send = "EV SetMCProtocolScannerParams" + setting + "\r\n";
            //            str = SendCommandToVision(send);
            //            if (str.Contains("1"))
            //            {
            //                str = SendCommandToVision("TS\r\n");
            //                System.Threading.Thread.Sleep(500);
            //                MessageLoggerManager.Log.Warn("Save SLMP Setting Done!");
            //                mIsWriteSettingDone = true;
            //            }
            //            else
            //            {
            //                MessageLoggerManager.Log.Alarm("Save SLMP Setting Fail! " + str);
            //            }

            //        }
            //        else
            //        {
            //            MessageLoggerManager.Log.Alarm("Save SLMP Setting Fail! " + str);
            //        }
            //    }
            //}
            //DisconnectToVision();
        }

        private void btnResetHW_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to reset camera?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                bool mIsWriteSettingDone = false;
                ConnectToVision(txtCameraAddress.Text, 23);
                string str = SendCommandToVision("");
                str = SendCommandToVision("admin\r\n");
                if (str.Contains("Password"))
                {
                    str = SendCommandToVision("\r\n");
                    if (str.Contains("Logged"))
                    {
                        mInSightControl.GetInsightDisplay().Disconnect();
                        SettingDoneEvent.BeginInvoke(null, null, null, null);
                        reconnectTimer.Start();
                        str = SendCommandToVision("RT\r\n", false);
                        MessageLoggerManager.Log.Warn("Camera is reseting...");
                    }
                }
                DisconnectToVision();
            }
        }

        private void btnFactoryResetHW_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to factory reset camera?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                string ip = string.Format(@"ftp://{0}/", txtCameraAddress.Text);
                List<string> list = GetAllFileNameOnFTPServer(new Uri(ip), "admin", "");
                if (list.Count > 0)
                {
                    foreach (string l in list)
                    {
                        try
                        {
                            DeleteFileOnFTPServer(new Uri(ip + l), "admin", "");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Alarm");
                        }
                    }
                    MessageLoggerManager.Log.Warn("Camera is initialized. Need open \"Alignment Application\" to setup Camera...");
                }
                else
                {
                    MessageBox.Show("Reset factory fail! IP address is not valid", "Warning");
                }
                MessageLoggerManager.Log.Warn("Camera is factory reseted!");
                MessageBox.Show("Camera is initialized. Need open \"Alignment Application\" to setup Camera...", "Warning");
                ExitProgramByFactoryResetEvent.BeginInvoke(null, null, null, null);
            }

        }

        private List<string> GetAllFileNameOnFTPServer(Uri uri, string acc, string pass)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
            request.Credentials = new NetworkCredential(acc, pass);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            string names = reader.ReadToEnd();

            reader.Close();
            response.Close();

            //MessageBox.Show(names, "File List");
            List<string> list = names.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            return list;
        }

        private void DeleteFileOnFTPServer(Uri uri, string acc, string pass)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
            request.Credentials = new NetworkCredential(acc, pass);
            request.Method = WebRequestMethods.Ftp.DeleteFile;
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            response.Close();
        }

        private void ConnectAgainWithCurrentHostName()
        {
            System.Threading.Thread.Sleep(2000);
            mCvsInsightDisplay = mInSightControl.GetInsightDisplay();
            mInSightControl.ConnectToSensor(mHostName);
            mInSightControl.GetInsightDisplay().ImageZoomMode = CvsDisplayZoom.Fit;
            reconnectTimer.Stop();
            MessageLoggerManager.Log.Warn("Camera is connected!");
        }

        private static bool PingHost(string address)
        {
            bool pingable = false;
            Ping pinger = null;

            try
            {
                pinger = new Ping();
                PingReply reply = pinger.Send(address);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }

            return pingable;
        }



        private void UpdateGUIParams()
        {
            //try
            //{
            //    var tag = GetTag(ResourceUtility.GetString("RtPLCAddress"));
            //    if (tag != null)
            //    {
            //        string plcaddress = (string)GetValue(tag.Location);
            //        txtPLCAddress.Text = plcaddress;
            //    }

            //    tag = GetTag(ResourceUtility.GetString("RtPLCOutput"));
            //    if (tag != null)
            //    {
            //        string address = (string)GetValue(tag.Location);
            //        txtOutputAddress.Text = address;
            //    }

            //    tag = GetTag(ResourceUtility.GetString("RtPLCPort"));
            //    if (tag != null)
            //    {
            //        int port = (int)GetValue(tag.Location);
            //        txtPLCPort.Text = port.ToString();
            //    }
            //}
            //catch (Exception ex)
            //{

            //}
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
            try
            {
                mCvsInsightDisplay.EditCellGraphic(location);
            }
            catch (Exception ex)
            {

            }
        }

        private void InsightClickButton(CvsCellLocation location)
        {
            mCvsInsightDisplay.InSight.ClickButton(location);
        }

        private void InsightSetListBox(CvsCellLocation location, int index)
        {
            mCvsInsightDisplay.InSight.SetListBoxIndex(location, index);
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
            else if (c.GetType() == typeof(Cognex.InSight.Cell.CvsCellListBox))
            {
                return ((Cognex.InSight.Cell.CvsCellListBox)c).Value;
            }
            return null;
        }

        private void btnOptimizeTools_Click(object sender, EventArgs e)
        {
            MessageManager.MessageLoggerManager.Log.Info("[Action] Update Optimized PM...");
            try
            {
                //Import by Snippet
                mCvsInSightDisplayEdit = mCvsInsightDisplay.Edit;
                mCvsInsightDisplay.SetCurrentCell(39, 10);
                CvsCellLocation cvs = mCvsInsightDisplay.CurrentCellNow;
                int col = mCvsInsightDisplay.CurrentCellNow.Column;
                int row = mCvsInsightDisplay.CurrentCellNow.Row;
                CvsAction.SetEnabled(mCvsInSightDisplayEdit.ImportSnippet, true);
                bool isEnable = mCvsInSightDisplayEdit.ImportSnippet.Enabled;

                mCvsInSightDisplayEdit.ImportSnippet.Execute();
            }
            catch (Exception ex)
            {

            }
        }

        private void btnUpdateOCA_Click(object sender, EventArgs e)
        {
            MessageManager.MessageLoggerManager.Log.Info("[Action] Update OCA...");
            try
            {
                //Import by Snippet
                mCvsInSightDisplayEdit = mCvsInsightDisplay.Edit;
                mCvsInsightDisplay.SetCurrentCell(182, 9);
                CvsCellLocation cvs = mCvsInsightDisplay.CurrentCellNow;
                int col = mCvsInsightDisplay.CurrentCellNow.Column;
                int row = mCvsInsightDisplay.CurrentCellNow.Row;
                CvsAction.SetEnabled(mCvsInSightDisplayEdit.ImportSnippet, true);
                bool isEnable = mCvsInSightDisplayEdit.ImportSnippet.Enabled;

                mCvsInSightDisplayEdit.ImportSnippet.Execute();
            }
            catch (Exception ex)
            {

            }
        }

        private void btnUpdateNoF_Click(object sender, EventArgs e)
        {
            MessageManager.MessageLoggerManager.Log.Info("[Action] Update Found Number...");
            try
            {
                //Import by Snippet
                mCvsInSightDisplayEdit = mCvsInsightDisplay.Edit;
                mCvsInsightDisplay.SetCurrentCell(179, 9);
                CvsCellLocation cvs = mCvsInsightDisplay.CurrentCellNow;
                int col = mCvsInsightDisplay.CurrentCellNow.Column;
                int row = mCvsInsightDisplay.CurrentCellNow.Row;
                CvsAction.SetEnabled(mCvsInSightDisplayEdit.ImportSnippet, true);
                bool isEnable = mCvsInSightDisplayEdit.ImportSnippet.Enabled;

                mCvsInSightDisplayEdit.ImportSnippet.Execute();
            }
            catch (Exception ex)
            {

            }
        }

        private void btnTwoHEUpdate_Click(object sender, EventArgs e)
        {
            MessageManager.MessageLoggerManager.Log.Info("[Action] Update Two HE SLMP...");
            try
            {
                //Import by Snippet
                mCvsInSightDisplayEdit = mCvsInsightDisplay.Edit;
                mCvsInsightDisplay.SetCurrentCell(88, 19);
                CvsCellLocation cvs = mCvsInsightDisplay.CurrentCellNow;
                int col = mCvsInsightDisplay.CurrentCellNow.Column;
                int row = mCvsInsightDisplay.CurrentCellNow.Row;
                CvsAction.SetEnabled(mCvsInSightDisplayEdit.ImportSnippet, true);
                bool isEnable = mCvsInSightDisplayEdit.ImportSnippet.Enabled;

                mCvsInSightDisplayEdit.ImportSnippet.Execute();
            }
            catch (Exception ex)
            {

            }
        }

        private void txtCameraAddress_TextChanged(object sender, EventArgs e)
        {
            MessageLoggerManager.Log.Info(String.Format("[Action]Camera Address: "+ txtCameraAddress.Text));
        }
    }
}
