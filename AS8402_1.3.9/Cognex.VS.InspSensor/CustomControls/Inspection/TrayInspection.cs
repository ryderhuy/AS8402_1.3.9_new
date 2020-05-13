using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cognex.InSight;
using Cognex.InSight.Controls.Display;
using Cognex.VS.Utility;
using System.Threading;
using MessageManager;

namespace Cognex.VS.InspSensor.CustomControls
{
    public partial class TrayInspection : UserControl
    {
        public EventHandler SettingDoneEvent;
        private CvsInSightDisplay mCvsInsightDisplay = null;
        private string mFinderTypeName = "Tray";
        private bool isInit = true;
        private bool checkSearchRegionR1 = true;
        private bool checkSearchRegionF1 = true;
        public TrayInspection()
        {
            isInit = true;
            InitializeComponent();
            isInit = false;
        }

        public void Init(CvsInSightDisplay insightDisplay)
        {
            mCvsInsightDisplay = insightDisplay;

            if (insightDisplay.Connected)
            {
                isInit = true;
                UpdateGUIParams();
                isInit = false;
            }
        }

        private void UpdateGUIParams()
        {
            try
            {
                checkSearchRegionF1 = true;
                checkSearchRegionR1 = true;
                var tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}EnableF", mFinderTypeName)));
                if (tag != null)
                {
                    bool isEnableF = (bool)GetValue(tag.Location);
                    chxEnableF.Checked = (bool)isEnableF;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}ContrastThresholdF", mFinderTypeName)));
                if (tag != null)
                {
                    float thresholdF = (float)GetValue(tag.Location);
                    numContrastThresholdF.Value = (decimal)thresholdF;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}AngleRangeF", mFinderTypeName)));
                if (tag != null)
                {
                    float angleRangeF = (float)GetValue(tag.Location);
                    numAngleRangeF.Value = (decimal)angleRangeF;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}ScoreThresholdF", mFinderTypeName)));
                if (tag != null)
                {
                    float scoreThresholdF = (float)GetValue(tag.Location);
                    numScoreThresholdF.Value = (decimal)scoreThresholdF;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}ShowgraphicF", mFinderTypeName)));
                if (tag != null)
                {
                    int showGraphicF = (int)GetValue(tag.Location);
                    numShowGraphicF.Value = (int)showGraphicF;
                }


                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}EnableR", mFinderTypeName)));
                if (tag != null)
                {
                    bool isEnableR = (bool)GetValue(tag.Location);
                    chxEnableR.Checked = (bool)isEnableR;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}ContrastThresholdR", mFinderTypeName)));
                if (tag != null)
                {
                    float thresholdR = (float)GetValue(tag.Location);
                    numContrastThresholdR.Value = (decimal)thresholdR;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}AngleRangeR", mFinderTypeName)));
                if (tag != null)
                {
                    float angleRangeR = (float)GetValue(tag.Location);
                    numAngleRangeR.Value = (decimal)angleRangeR;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}ScoreThresholdR", mFinderTypeName)));
                if (tag != null)
                {
                    float scoreThresholdR = (float)GetValue(tag.Location);
                    numScoreThresholdR.Value = (decimal)scoreThresholdR;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}ShowgraphicR", mFinderTypeName)));
                if (tag != null)
                {
                    int showGraphicR = (int)GetValue(tag.Location);
                    numShowGraphicR.Value = (int)showGraphicR;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}FirstTimeExposure", mFinderTypeName)));
                if (tag != null)
                {
                    float firstTimeExp = (float)GetValue(tag.Location);
                    numExposureTimeForFirstTray.Value = (decimal)firstTimeExp;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtAlignTrayContrastThresh")));
                if (tag != null)
                {
                    float Value = (float)GetValue(tag.Location);
                    numAlignPocketContrastThresh.Value = (decimal)Value;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtAlignTrayAngleRange")));
                if (tag != null)
                {
                    float Value = (float)GetValue(tag.Location);
                    numAlignPocketAngleRange.Value = (decimal)Value;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtAlignTrayScoreThresh")));
                if (tag != null)
                {
                    float Value = (float)GetValue(tag.Location);
                    numAlignPocketScoreThresh.Value = (decimal)Value;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtAlignTrayGraphic")));
                if (tag != null)
                {
                    float Value = (float)GetValue(tag.Location);
                    numAlignPocketGraphic.Value = (decimal)Value;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtAlignTrayEnable")));
                if (tag != null)
                {
                    bool isEnable = (bool)GetValue(tag.Location);
                    chxAlignPocketonTrayEnable.Checked = (bool)isEnable;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtAlignTrayNumofPocket")));
                if (tag != null)
                {
                    int Value = (int)GetValue(tag.Location);
                    numNumberAlignPocket.Value = (int)Value;
                    numAPNumbPocket.Value = (int)Value;
                }

                if (cbxSearchRegionAlignPocket != null)
                    cbxSearchRegionAlignPocket.Items.Clear();

                if (cbxAPSearchRegion != null)
                    cbxAPSearchRegion.Items.Clear();

                if (cbxPocketTolerance != null)
                    cbxPocketTolerance.Items.Clear();

                for (int i = 0; i < (int)numNumberAlignPocket.Value; ++i)
                {
                    cbxSearchRegionAlignPocket.Items.Add(string.Format("Region{0}", i + 1));
                    cbxAPSearchRegion.Items.Add(string.Format("Region{0}", i + 1));
                    cbxPocketTolerance.Items.Add(string.Format("Pocket{0}", i + 1));
                }

                cbxSearchRegionAlignPocket.SelectedIndex = 0;
                cbxAPSearchRegion.SelectedIndex = 0;
                cbxPocketTolerance.SelectedIndex = 0;

                tag = GetTag("Align.Tray.XTTEnable");
                if (tag != null)
                {
                    bool isEnable = (bool)GetValue(tag.Location);
                    chxEnableSendXTT.Checked = (bool)isEnable;
                }

                tag = GetTag("Inspection.6.AcquisitionSettings.ExposureTime");
                if (tag != null)
                {
                    float Value = (float)GetValue(tag.Location);
                    numAPExposure.Value = (decimal)Value;
                }

                tag = GetTag("Align.Pocket.Tolerance.Enable");
                if (tag != null)
                {
                    bool isEnable = (bool)GetValue(tag.Location);
                    chxPocketToleranceEnable.Checked = (bool)isEnable;
                }

                tag = GetTag("Align.Pocket.Tolerance.X_1");
                if (tag != null)
                {
                    float Value = (float)GetValue(tag.Location);
                    numToleranceX.Value = (decimal)Value;
                }

                tag = GetTag("Align.Pocket.Tolerance.Y_1");
                if (tag != null)
                {
                    float Value = (float)GetValue(tag.Location);
                    numToleranceY.Value = (decimal)Value;
                }

                tag = GetTag("Align.Pocket.Tolerance.T_1");
                if (tag != null)
                {
                    float Value = (float)GetValue(tag.Location);
                    numToleranceTheta.Value = (decimal)Value;
                }

                tag = GetTag("Align.Pocket.Limit.Enable");
                if (tag != null)
                {
                    bool isEnable = (bool)GetValue(tag.Location);
                    chxPocketLimitEnable.Checked = (bool)isEnable;
                }

                tag = GetTag("Align.Pocket.Litmit.Max.X");
                if (tag != null)
                {
                    float Value = (float)GetValue(tag.Location);
                    numPocketLimMaxX.Value = (decimal)Value;
                }

                tag = GetTag("Align.Pocket.Litmit.Max.Y");
                if (tag != null)
                {
                    float Value = (float)GetValue(tag.Location);
                    numPocketLimMaxY.Value = (decimal)Value;
                }

                tag = GetTag("Align.Pocket.Litmit.Max.Theta");
                if (tag != null)
                {
                    float Value = (float)GetValue(tag.Location);
                    numPocketLimMaxTheta.Value = (decimal)Value;
                }

                tag = GetTag("Align.Pocket.Litmit.Min.X");
                if (tag != null)
                {
                    float Value = (float)GetValue(tag.Location);
                    numPocketLimMinX.Value = (decimal)Value;
                }

                tag = GetTag("Align.Pocket.Litmit.Min.Y");
                if (tag != null)
                {
                    float Value = (float)GetValue(tag.Location);
                    numPocketLimMinY.Value = (decimal)Value;
                }

                tag = GetTag("Align.Pocket.Litmit.Min.Theta");
                if (tag != null)
                {
                    float Value = (float)GetValue(tag.Location);
                    numPocketLimMinTheta.Value = (decimal)Value;
                }

                UpdateValueTrayInspection();

                //update display 8 pocket
                tag = GetTag("RtInspTrayInspection8Pocket");
                if (tag != null)
                {
                    if (!(Boolean)GetValue(tag.Location))
                    {
                        this.tabControl1.TabPages.Remove(this.tabPage4);
                        this.tabControl1.TabPages.Remove(this.tabPage3);
                        this.tabControl1.TabPages.Remove(this.tabPage5);
                    }
                    else
                    {
                        if (!this.tabControl1.TabPages.Contains(this.tabPage3))
                        {
                            this.tabControl1.TabPages.Add(this.tabPage3);
                        }
                        if (!this.tabControl1.TabPages.Contains(this.tabPage4))
                        {
                            this.tabControl1.TabPages.Add(this.tabPage4);
                        }
                        if (!this.tabControl1.TabPages.Contains(this.tabPage5))
                        {
                            this.tabControl1.TabPages.Add(this.tabPage5);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

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
            try
            {
                mCvsInsightDisplay.InSight.ClickButton(location);
            }
            catch { }
        }

        private void InsightSetValue(CvsCellLocation location, float value)
        {
            try
            {
                mCvsInsightDisplay.InSight.SetFloat(location, value);
            }
            catch { }
        }

        private void InsightSetValue(CvsCellLocation location, int value)
        {
            try
            {
                mCvsInsightDisplay.InSight.SetInteger(location, value);
            }
            catch { }
        }

        private void InsightSetValue(CvsCellLocation location, string value)
        {
            try
            {
                mCvsInsightDisplay.InSight.SetString(location, value);
            }
            catch { }
        }

        private void InsightSetValue(CvsCellLocation location, bool value)
        {
            try
            {
            mCvsInsightDisplay.InSight.SetCheckBox(location, value);
            }
            catch { }
        }

        private object GetValue(CvsCellLocation location)
        {
            Cognex.InSight.Cell.CvsCell c = mCvsInsightDisplay.InSight.Results.Cells[location];
            try
            {
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
                else
                    return null;
            }
            catch { return null; }
        }
        private string GetValueStringCell(CvsCellLocation location)
        {
            string strRet = "";
            try
            {
                strRet = mCvsInsightDisplay.InSight.Results.Cells[location].Text;
            }
            catch
            {
                strRet = "";
            }
            return strRet;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to discard current setting?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                SettingDoneEvent.BeginInvoke(null, null, null, null);
            }
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            try
            {

                /*
                if (!chxEnableF.Checked|| !chxEnableR.Checked)
                {
                    if (MessageBox.Show("Do you want to enable this function?", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                    {
                        nameTag = string.Format("RtInsp{0}EnableF", mFinderTypeName);
                        tag = GetTag(ResourceUtility.GetString(nameTag));
                        if (tag != null)
                        {
                            InsightSetValue(tag.Location, true);
                        }
                        chxEnableF.Checked = true;

                        nameTag = string.Format("RtInsp{0}EnableR", mFinderTypeName);
                        tag = GetTag(ResourceUtility.GetString(nameTag));
                        if (tag != null)
                        {
                            InsightSetValue(tag.Location, true);
                        }
                        chxEnableR.Checked = true;

                    }
                }
                */

                SettingDoneEvent.BeginInvoke(null, null, null, null);
            }
            catch (CvsException ex)
            {

            }
        }

        private void btnTrainRegionF_Click(object sender, EventArgs e)
        {
            string nameTag = string.Format("RtInsp{0}TrainRegionF", mFinderTypeName);
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                EditCellGraphic(tag.Location);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Click Train Region F1"));

        }

        private void btnTrainRegionR_Click(object sender, EventArgs e)
        {
            string nameTag = string.Format("RtInsp{0}TrainRegionR", mFinderTypeName);
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                EditCellGraphic(tag.Location);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Click Train Region R1"));
        }

        private void btnSearchRegionF_Click(object sender, EventArgs e)
        {
            checkSearchRegionF1 = true;
            //update value for Search Region F
            var tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}ContrastThresholdF", mFinderTypeName)));
            if (tag != null)
            {
                float thresholdF = (float)GetValue(tag.Location);
                numContrastThresholdF.Value = (decimal)thresholdF;
            }

            tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}AngleRangeF", mFinderTypeName)));
            if (tag != null)
            {
                float angleRangeF = (float)GetValue(tag.Location);
                numAngleRangeF.Value = (decimal)angleRangeF;
            }

            tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}ScoreThresholdF", mFinderTypeName)));
            if (tag != null)
            {
                float scoreThresholdF = (float)GetValue(tag.Location);
                numScoreThresholdF.Value = (decimal)scoreThresholdF;
            }

            string nameTag = string.Format("RtInsp{0}SearchRegionF", mFinderTypeName);
            tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                EditCellGraphic(tag.Location);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Click Search Region F1"));
        }

        private void btnSearchRegionR_Click(object sender, EventArgs e)
        {
            checkSearchRegionR1 = true;
            //update value for Search Region F
            var tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}ContrastThresholdR", mFinderTypeName)));
            if (tag != null)
            {
                float thresholdF = (float)GetValue(tag.Location);
                numContrastThresholdR.Value = (decimal)thresholdF;
            }

            tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}AngleRangeR", mFinderTypeName)));
            if (tag != null)
            {
                float angleRangeF = (float)GetValue(tag.Location);
                numAngleRangeR.Value = (decimal)angleRangeF;
            }

            tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}ScoreThresholdR", mFinderTypeName)));
            if (tag != null)
            {
                float scoreThresholdF = (float)GetValue(tag.Location);
                numScoreThresholdR.Value = (decimal)scoreThresholdF;
            }
            string nameTag = string.Format("RtInsp{0}SearchRegionR", mFinderTypeName);
            tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                EditCellGraphic(tag.Location);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Click Search Region R1"));
        }

        private void btnTrainGoldenR_Click(object sender, EventArgs e)
        {
            string nameTag = string.Format("RtInsp{0}TrainGoldenButtonR", mFinderTypeName);
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                InsightClickButton(tag.Location);
            }
        }

        private void btnTrainGoldenF_Click(object sender, EventArgs e)
        {
            string nameTag = string.Format("RtInsp{0}TrainGoldenButtonF", mFinderTypeName);
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                InsightClickButton(tag.Location);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Click button Train Golden F"));
        }

        private void btnTrainPMF_Click(object sender, EventArgs e)
        {
            string nameTag = string.Format("RtInsp{0}TrainButtonF", mFinderTypeName);
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                InsightClickButton(tag.Location);
            }

            Thread.Sleep(50);
            UpdateValueTrayInspection();
            MessageLoggerManager.Log.Info(String.Format("[Action] Click Train F1"));
        }

        private void btnTrainPMR_Click(object sender, EventArgs e)
        {
            string nameTag = string.Format("RtInsp{0}TrainButtonR", mFinderTypeName);
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                InsightClickButton(tag.Location);
            }

            Thread.Sleep(50);
            UpdateValueTrayInspection();
            MessageLoggerManager.Log.Info(String.Format("[Action] Click Train R1"));
        }

        private void lblTrainGoldenR_Click(object sender, EventArgs e)
        {

        }

        private void numContrastThresholdF_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                //Forward
                if (checkSearchRegionF1)
                {
                    string nameTag = string.Format("Inspection.1.TrayInspection.FW.ContrastThreshold");
                    var tag = GetTag(nameTag);
                    if (tag != null)
                    {
                        InsightSetValue(tag.Location, (float)numContrastThresholdF.Value);
                    }
                    MessageLoggerManager.Log.Info(String.Format("[Action] Contrast Threshold F : " + (float)numContrastThresholdF.Value));
                }
                else
                {
                    //Forward  region 2
                    string nameTag = string.Format("Inspection.1.TrayInspection.FW.ContrastThreshold_2");
                    var tag = GetTag(nameTag);
                    if (tag != null)
                    {
                        InsightSetValue(tag.Location, (float)numContrastThresholdF.Value);
                    }
                    MessageLoggerManager.Log.Info(String.Format("[Action] Contrast Threshold F2 : " + (float)numContrastThresholdF.Value));
                }
               
            }
            catch (Exception ex)
            {

            }
        }

        private void numAngleRangeF_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (checkSearchRegionF1)
                {
                    //Forward
                    string nameTag = string.Format("Inspection.1.TrayInspection.FW.AngleRange");
                    var tag = GetTag(nameTag);
                    if (tag != null)
                    {
                        InsightSetValue(tag.Location, (float)numAngleRangeF.Value);
                    }
                    MessageLoggerManager.Log.Info(String.Format("[Action] Angle Range F : " + (float)numAngleRangeF.Value));
                }
                else
                {
                    //Forward region 2
                    string nameTag = string.Format("Inspection.1.TrayInspection.FW.AngleRange_2");
                    var tag = GetTag(nameTag);
                    if (tag != null)
                    {
                        InsightSetValue(tag.Location, (float)numAngleRangeF.Value);
                    }
                    MessageLoggerManager.Log.Info(String.Format("[Action] Angle Range F : " + (float)numAngleRangeF.Value));
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void numShowGraphicF_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                //Forward
                string nameTag = string.Format("RtInsp{0}ShowgraphicF", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)numShowGraphicF.Value);
                }
                 MessageLoggerManager.Log.Info(String.Format("[Action] Show Graphic F : " + (float)numShowGraphicF.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void chxEnableF_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                //Forward
                string nameTag = string.Format("RtInsp{0}EnableF", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, chxEnableF.Checked);
                }
                if (chxEnableF.Checked)
                {

                    MessageLoggerManager.Log.Info(String.Format("[Action] Enable 1: Checked"));
                }
                else
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] Enable 1: Unchecked"));
                }
            }
            catch (Exception ex)
            {

            }
            
        }

        private void numContrastThresholdR_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                //Reverse
                if (checkSearchRegionR1)
                {
                    string nameTag = string.Format("RtInsp{0}ContrastThresholdR", mFinderTypeName);
                    var tag = GetTag(ResourceUtility.GetString(nameTag));
                    if (tag != null)
                    {
                        InsightSetValue(tag.Location, (float)numContrastThresholdR.Value);
                    }
                    MessageLoggerManager.Log.Info(String.Format("[Action] ContrastThreshold R: " + numContrastThresholdR.Value));
                }
                else
                {
                    //Forward  region 2
                    string nameTag = string.Format("Inspection.1.TrayInspection.RV.ContrastThreshold_2");
                    var tag = GetTag(nameTag);
                    if (tag != null)
                    {
                        InsightSetValue(tag.Location, (float)numContrastThresholdR.Value);
                    }
                    MessageLoggerManager.Log.Info(String.Format("[Action] Contrast Threshold R2 : " + (float)numContrastThresholdR.Value));
                }
                

            }
            catch (Exception ex)
            {

            }
        }

        private void numAngleRangeR_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                //Reverse
                if (checkSearchRegionR1)
                {
                    string nameTag = string.Format("RtInsp{0}AngleRangeR", mFinderTypeName);
                    var tag = GetTag(ResourceUtility.GetString(nameTag));
                    if (tag != null)
                    {
                        InsightSetValue(tag.Location, (float)numAngleRangeR.Value);
                    }
                    MessageLoggerManager.Log.Info(String.Format("[Action] AngleRange R: " + numAngleRangeR.Value));
                }
                else
                {
                    //Forward region 2
                    string nameTag = string.Format("Inspection.1.TrayInspection.RV.AngleRange_2");
                    var tag = GetTag(nameTag);
                    if (tag != null)
                    {
                        InsightSetValue(tag.Location, (float)numAngleRangeR.Value);
                    }
                    MessageLoggerManager.Log.Info(String.Format("[Action] Angle Range R2: " + (float)numAngleRangeR.Value));
                }
                
            }
            catch (Exception ex)
            {

            }
        }

        private void numShowGraphicR_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                //Reverse
                string nameTag = string.Format("RtInsp{0}ShowgraphicR", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)numShowGraphicR.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Show Graphic R: " + numShowGraphicR.Value));

            }
            catch (Exception ex)
            {

            }
        }

        private void chxEnableR_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                //Reverse
                string nameTag = string.Format("RtInsp{0}EnableR", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, chxEnableR.Checked);
                }
                if (chxEnableR.Checked)
                {

                    MessageLoggerManager.Log.Info(String.Format("[Action] Enable R1: Checked"));
                }
                else
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] Enable R1: Unchecked"));
                }

            }
            catch (Exception ex)
            {

            }
        }

        private void numScoreThresholdF_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (checkSearchRegionF1)
                {
                    //Foward
                    string nameTag = string.Format("Inspection.1.TrayInspection.FW.ScoreThreshold");
                    var tag = GetTag(nameTag);
                    if (tag != null)
                    {
                        InsightSetValue(tag.Location, (float)numScoreThresholdF.Value);
                    }
                    MessageLoggerManager.Log.Info(String.Format("[Action] Score Threshold F : " + (float)numScoreThresholdF.Value));
                }
                else
                {
                    //Foward Region 2
                    string nameTag = string.Format("Inspection.1.TrayInspection.FW.ScoreThreshold_2");
                    var tag = GetTag(nameTag);
                    if (tag != null)
                    {
                        InsightSetValue(tag.Location, (float)numScoreThresholdF.Value);
                    }
                    MessageLoggerManager.Log.Info(String.Format("[Action] Score Threshold F2 : " + (float)numScoreThresholdF.Value));
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void numScoreThresholdR_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                //Reverse
                if (checkSearchRegionR1)
                {
                    string nameTag = string.Format("RtInsp{0}ScoreThresholdR", mFinderTypeName);
                    var tag = GetTag(ResourceUtility.GetString(nameTag));
                    if (tag != null)
                    {
                        InsightSetValue(tag.Location, (float)numScoreThresholdR.Value);
                    }
                    MessageLoggerManager.Log.Info(String.Format("[Action] Score Threshold R: " + numScoreThresholdR));

                }
                else
                {
                    //Foward Region 2
                    string nameTag = string.Format("Inspection.1.TrayInspection.RV.ScoreThreshold_2");
                    var tag = GetTag(nameTag);
                    if (tag != null)
                    {
                        InsightSetValue(tag.Location, (float)numScoreThresholdR.Value);
                    }
                    MessageLoggerManager.Log.Info(String.Format("[Action] Score Threshold R2 : " + (float)numScoreThresholdR.Value));
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void numExposureTimeForFirstTray_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                //Reverse
                string nameTag = string.Format("RtInsp{0}FirstTimeExposure", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numExposureTimeForFirstTray.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Exposure Time For First Tray : " + (float)numExposureTimeForFirstTray.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void numNumberAlignPocket_ValueChanged(object sender, EventArgs e)
        {
            //isInit = true;
            numAPNumbPocket.Value = numNumberAlignPocket.Value;
            try
            {
                string nameTag = string.Format("RtAlignTrayNumofPocket");
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)numNumberAlignPocket.Value);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("errr");
            }


            if (cbxSearchRegionAlignPocket != null)
                cbxSearchRegionAlignPocket.Items.Clear();

            if (cbxAPSearchRegion != null)
                cbxAPSearchRegion.Items.Clear();

            for (int i = 0; i < (int)numNumberAlignPocket.Value; ++i)
            {
                cbxSearchRegionAlignPocket.Items.Add(string.Format("Region{0}", i + 1));
                cbxAPSearchRegion.Items.Add(string.Format("Region{0}", i + 1));
            }
            cbxSearchRegionAlignPocket.SelectedIndex = 0;
            cbxAPSearchRegion.SelectedIndex = 0;
            //isInit = false;
            MessageLoggerManager.Log.Info(String.Format("[Action] Number Align Pocket : " + (int)numNumberAlignPocket.Value));
        }

        private void cbxSearchRegionAlignPocket_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cbxSearchRegionAlignPocket.Focused)
                {
                    string sNumPanel = cbxSearchRegionAlignPocket.Text;
                    string str = "RtAlignTraySearch" + sNumPanel;
                    var tag = GetTag(ResourceUtility.GetString(str));
                    if (tag != null)
                    {
                        EditCellGraphic(tag.Location);
                    }
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Search Region Align Pocket : " + cbxSearchRegionAlignPocket.SelectedItem));
            }
            catch (CvsException ex)
            {

            }
        }

        private void numAlignPocketContrastThresh_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtAlignTrayContrastThresh");
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numAlignPocketContrastThresh.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Align Pocket Contrast Thresh : " + (float)numAlignPocketContrastThresh.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void numAlignPocketAngleRange_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtAlignTrayAngleRange");
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numAlignPocketAngleRange.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Align Pocket Angle Range : " + (float)numAlignPocketAngleRange.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void numAlignPocketScoreThresh_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtAlignTrayScoreThresh");
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numAlignPocketScoreThresh.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Align Tray Score Thresh : " + (float)numAlignPocketScoreThresh.Value));
            }
            catch (Exception ex)
            {

            }

        }

        private void numAlignPocketGraphic_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtAlignPocketGraphic");
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numAlignPocketGraphic.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Align Pocket Graphic : " + (float)numAlignPocketGraphic.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void chxAlignPocketonTrayEnable_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtAlignTrayEnable");
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, chxAlignPocketonTrayEnable.Checked);
                }
                if (chxAlignPocketonTrayEnable.Checked)
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] Align Tray Enable: Checked"));
                }
                else
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] Align Tray Enable: Unchecked"));
                }
                
            }
            catch (Exception ex)
            {

            }
        }

        private void EnableSendXTT_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtAlignTrayXTTEnable");
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, chxEnableSendXTT.Checked);
                }
                if (chxAlignPocketonTrayEnable.Checked)
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] Align TrayXTT Enable : Checked"));
                }
                else
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] Align TrayXTT Enable : Unchecked"));
                }
            }
            catch (Exception ex)
            {

            }
        }
        private void UpdatePatmaxFeature(int number)
        {
            try
            {
                var tag = GetTag(string.Format("Inspection.6.Tray.Contrast_{0}", number));
                if (tag != null)
                {
                    float contrast = (float)GetValue(tag.Location);
                    numAPContrast.Value = (decimal)contrast;
                }

                tag = GetTag(string.Format("Inspection.6.Tray.AngleRange_{0}", number));
                if (tag != null)
                {
                    float angle = (float)GetValue(tag.Location);
                    numAPAngleRange.Value = (decimal)angle;
                }

                tag = GetTag(string.Format("Inspection.6.Tray.AcceptThreshold_{0}", number));
                if (tag != null)
                {
                    float score = (float)GetValue(tag.Location);
                    numAPScore.Value = (decimal)score;
                }
            }
            catch
            {
            }
        }

        private void numAPExposure_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Inspection.6.AcquisitionSettings.ExposureTime");
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numAPExposure.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] APExposure : " + (float)numAPExposure.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void numAPNumbPocket_ValueChanged(object sender, EventArgs e)
        {
            //isInit = true;
            numNumberAlignPocket.Value = numAPNumbPocket.Value;
            try
            {
                string nameTag = string.Format("RtAlignTrayNumofPocket");
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)numAPNumbPocket.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] APNumbPocket : " + (int)numAPNumbPocket.Value));
            }
            catch (Exception ex)
            {
                //MessageBox.Show("errr");
            }

            if (cbxSearchRegionAlignPocket != null)
                cbxSearchRegionAlignPocket.Items.Clear();

            if (cbxAPSearchRegion != null)
                cbxAPSearchRegion.Items.Clear();

            for (int i = 0; i < (int)numNumberAlignPocket.Value; ++i)
            {
                cbxSearchRegionAlignPocket.Items.Add(string.Format("Region{0}", i + 1));
                cbxAPSearchRegion.Items.Add(string.Format("Region{0}", i + 1));
            }
            cbxSearchRegionAlignPocket.SelectedIndex = 0;
            cbxAPSearchRegion.SelectedIndex = 0;

            //isInit = false;
        }

        private void cbxAPSearchRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePatmaxFeature(cbxAPSearchRegion.SelectedIndex + 1);
            if (cbxAPSearchRegion.Focused)
            {
                string nameTag = string.Empty;
                nameTag = string.Format("Inspection.6.Tray.SearchRegion_{0}", cbxAPSearchRegion.SelectedIndex + 1);

                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    EditCellGraphic(tag.Location);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] AP Search Region : " + cbxAPSearchRegion.SelectedItem));
            }
            
        }

        private void btnAPTrainRegion_Click(object sender, EventArgs e)
        {
            string nameTag = string.Empty;
            nameTag = string.Format("Inspection.6.Tray.TrainRegion_{0}", cbxAPSearchRegion.SelectedIndex + 1);
            var tag = GetTag(nameTag);
            if (tag != null)
            {
                EditCellGraphic(tag.Location);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Click APTrain Region"));

        }

        private void btnAPSearchRegion_Click(object sender, EventArgs e)
        {
            string nameTag = string.Empty;
            nameTag = string.Format("Inspection.6.Tray.SearchRegion_{0}", cbxAPSearchRegion.SelectedIndex + 1);

            var tag = GetTag(nameTag);
            if (tag != null)
            {
                EditCellGraphic(tag.Location);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Click APSearch Region"));
        }

        private void btnAPTrain_Click(object sender, EventArgs e)
        {
            string nameTag = string.Empty;
            nameTag = string.Format("Inspection.6.Tray.Train_{0}", cbxAPSearchRegion.SelectedIndex + 1);

            var tag = GetTag(nameTag);
            if (tag != null)
            {
                InsightClickButton(tag.Location);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Click APTrain"));
        }

        private void numAPShowGraphic_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = "Inspection.6.Tray.ShowGraphic";
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)numAPShowGraphic.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] APShow Graphic : " + (float)numAPShowGraphic.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void numAPContrast_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Inspection.6.Tray.Contrast_{0}", cbxAPSearchRegion.SelectedIndex + 1);
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numAPContrast.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] APContrast : " + (float)numAPContrast.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void numAPAngleRange_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Inspection.6.Tray.AngleRange_{0}", cbxAPSearchRegion.SelectedIndex + 1);
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numAPAngleRange.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] APAngleRange : " + (float)numAPAngleRange.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void numAPScore_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Inspection.6.Tray.AcceptThreshold_{0}", cbxAPSearchRegion.SelectedIndex + 1);
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numAPScore.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] APScore : " + (float)numAPScore.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void btnAPMoveOrigin_Click(object sender, EventArgs e)
        {
            string nameTag = string.Empty;
            nameTag = string.Format("Inspection.6.Origin_{0}", cbxAPSearchRegion.SelectedIndex + 1);
            var tag = GetTag(nameTag);
            if (tag != null)
            {
                EditCellGraphic(tag.Location);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Click APMove Origin Click"));
        }

        private void btnAPResetOrigin_Click(object sender, EventArgs e)
        {
            string nameTag = string.Empty;
            nameTag = string.Format("Inspection.6.ResetOrigin_{0}", cbxAPSearchRegion.SelectedIndex + 1);

            var tag = GetTag(nameTag);
            if (tag != null)
            {
                InsightClickButton(tag.Location);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Click APReset"));
        }

        private void chxPocketLimitEnable_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Align.Pocket.Limit.Enable");
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, chxPocketLimitEnable.Checked);
                }

                if (chxPocketLimitEnable.Checked)
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] Pocket Limit Enable: Checked"));
                }
                else
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] Pocket Limit Enable: Unchecked"));
                }
            }
            catch (Exception ex)
            {
            }
        }
        private void chxPocketToleranceEnable_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Align.Pocket.Tolerance.Enable");
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, chxPocketToleranceEnable.Checked);
                }
                if (chxPocketToleranceEnable.Checked)
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] Pocket To lerance Enable: Checked"));
                }
                else
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] Pocket To lerance Enable: Unchecked"));
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void numPocketLimMaxX_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Align.Pocket.Litmit.Max.X");
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numPocketLimMaxX.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Pocket Lim Max X : " + (float)numPocketLimMaxX.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void numPocketLimMaxY_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Align.Pocket.Litmit.Max.Y");
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numPocketLimMaxY.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Pocket Lim Max Y : " + (float)numPocketLimMaxY.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void numPocketLimMaxTheta_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Align.Pocket.Litmit.Max.Theta");
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numPocketLimMaxTheta.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Pocket Lim Max Theta : " + (float)numPocketLimMaxTheta.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void numPocketLimMinX_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Align.Pocket.Litmit.Min.X");
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numPocketLimMinX.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Pocket Lim Min X : " + (float)numPocketLimMinX.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void numPocketLimMinY_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Align.Pocket.Litmit.Min.Y");
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numPocketLimMinY.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Pocket Lim Min Y : " + (float)numPocketLimMinY.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void numPocketLimMinTheta_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Align.Pocket.Litmit.Min.Theta");
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numPocketLimMinTheta.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Pocket Lim Min Theta : " + (float)numPocketLimMinTheta.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void cbxPocketTolerance_SelectedIndexChanged(object sender, EventArgs e)
        {
            MessageLoggerManager.Log.Info(String.Format("[Action] Pocket Tolerance Selected Item  : " + cbxPocketTolerance.SelectedItem));
            try
            {
                var tag = GetTag(string.Format("Align.Pocket.Tolerance.X_{0}",cbxPocketTolerance.SelectedIndex + 1));
                if (tag != null)
                {
                    float Value = (float)GetValue(tag.Location);
                    numToleranceX.Value = (decimal)Value;
                    MessageLoggerManager.Log.Info(String.Format("[Action] Tolerance X : " + (float)numToleranceX.Value));
                }

                tag = GetTag(string.Format("Align.Pocket.Tolerance.Y_{0}", cbxPocketTolerance.SelectedIndex + 1));
                if (tag != null)
                {
                    float Value = (float)GetValue(tag.Location);
                    numToleranceY.Value = (decimal)Value;
                    MessageLoggerManager.Log.Info(String.Format("[Action] Tolerance Y : " + (float)numToleranceY.Value));
                }

                tag = GetTag(string.Format("Align.Pocket.Tolerance.T_{0}", cbxPocketTolerance.SelectedIndex + 1));
                if (tag != null)
                {
                    float Value = (float)GetValue(tag.Location);
                    numToleranceTheta.Value = (decimal)Value;
                    MessageLoggerManager.Log.Info(String.Format("[Action] Tolerance Theta : " + (float)numToleranceTheta.Value));
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void numToleranceX_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Align.Pocket.Tolerance.X_{0}", cbxPocketTolerance.SelectedIndex + 1);
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numToleranceX.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Tolerance X : " + (float)numToleranceX.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void numToleranceY_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Align.Pocket.Tolerance.Y_{0}", cbxPocketTolerance.SelectedIndex + 1);
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numToleranceY.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Tolerance Y : " + (float)numToleranceY.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void numToleranceTheta_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Align.Pocket.Tolerance.T_{0}", cbxPocketTolerance.SelectedIndex + 1);
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numToleranceTheta.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Tolerance Theta : " + (float)numToleranceTheta.Value));
            }
            catch (Exception ex)
            {

            }
        }

        #region Update UI for Multi Pattern Tray
        private void chxEnableR2_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                //Forward
                string nameTag = string.Format("RtInsp{0}EnableR_2", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, chxEnableF2.Checked);
                }
                if (chxEnableR2.Checked)
                {

                    MessageLoggerManager.Log.Info(String.Format("[Action] Enable R2: Checked"));
                }
                else
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] Enable R2: Unchecked"));
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void btnTrainRegionR2_Click(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}TrainRegionR_2", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag)); //GetTag(nameTag);
                string test = ResourceUtility.GetString(nameTag);
                if (tag != null)
                {
                    EditCellGraphic(tag.Location);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Click Train Region R2"));
            }
            catch { }
        }

        private void btnSearchRegionR2_Click(object sender, EventArgs e)
        {
            checkSearchRegionR1 = false;
            try
            {
                //update for value for Search Reion R2
                var tag = GetTag("Inspection.1.TrayInspection.RV.ContrastThreshold_2");
                if (tag != null)
                {
                    float thresholdF = (float)GetValue(tag.Location);
                    numContrastThresholdR.Value = (decimal)thresholdF;
                }

                tag = GetTag("Inspection.1.TrayInspection.RV.AngleRange_2");
                if (tag != null)
                {
                    float angleRangeF = (float)GetValue(tag.Location);
                    numAngleRangeR.Value = (decimal)angleRangeF;
                }

                tag = GetTag("Inspection.1.TrayInspection.RV.ScoreThreshold_2");
                if (tag != null)
                {
                    float scoreThresholdF = (float)GetValue(tag.Location);
                    numScoreThresholdR.Value = (decimal)scoreThresholdF;
                }

                string nameTag = string.Format("RtInsp{0}SearchRegionR_2", mFinderTypeName);
                tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    EditCellGraphic(tag.Location);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Click Search Region R2"));
            }
            catch { }
        }

        private void btnTrainPMR2_Click(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}TrainButtonR_2", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightClickButton(tag.Location);
                }

                Thread.Sleep(50);
                UpdateValueTrayInspection();
                MessageLoggerManager.Log.Info(String.Format("[Action] Click Train R2"));
            }
            catch { }
        }

       

        private void chxEnableF2_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                //Forward
                string nameTag = string.Format("RtInsp{0}EnableF_2", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, chxEnableF2.Checked);
                }
                    if (chxEnableF2.Checked)
                {

                    MessageLoggerManager.Log.Info(String.Format("[Action] Enable 2: Checked"));
                }
                else
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] Enable 2: Unchecked"));
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void btnTrainRegionF2_Click(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}TrainRegionF_2", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag)); //GetTag(nameTag)
                string test = ResourceUtility.GetString(nameTag);
                if (tag != null)
                {
                    EditCellGraphic(tag.Location);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Click Train Region F2"));
            }
            catch { }
        }
        private void btnSearchRegionF2_Click(object sender, EventArgs e)
        {
            checkSearchRegionF1 = false;
            try
            {
                //update for value for Search Reion F2
                var tag = GetTag("Inspection.1.TrayInspection.FW.ContrastThreshold_2");
                if (tag != null)
                {
                    float thresholdF = (float)GetValue(tag.Location);
                    numContrastThresholdF.Value = (decimal)thresholdF;
                }

                tag = GetTag("Inspection.1.TrayInspection.FW.AngleRange_2");
                if (tag != null)
                {
                    float angleRangeF = (float)GetValue(tag.Location);
                    numAngleRangeF.Value = (decimal)angleRangeF;
                }

                tag = GetTag("Inspection.1.TrayInspection.FW.ScoreThreshold_2");
                if (tag != null)
                {
                    float scoreThresholdF = (float)GetValue(tag.Location);
                    numScoreThresholdF.Value = (decimal)scoreThresholdF;
                }

                string nameTag = string.Format("RtInsp{0}SearchRegionF_2", mFinderTypeName);
                tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    EditCellGraphic(tag.Location);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Click Search Region F2"));
            }
                
            catch { }
        }

        private void btnTrainPMF2_Click(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}TrainButtonF_2", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightClickButton(tag.Location);
                }

                Thread.Sleep(50);
                UpdateValueTrayInspection();
                MessageLoggerManager.Log.Info(String.Format("[Action] Click Train F2"));
            }
            catch { }
        }
        #endregion

        private float ConvertStringToFloat(string strInput, float fDefault)
        {
            float fRet = 0;
            try
            {
                fRet = float.Parse(strInput);
            }
            catch
            {
                fRet = fDefault;
            }
            return fRet;
        }
       
        private void UpdateValueTrayInspection()
        {
            var tag = GetTag(ResourceUtility.GetString("RtInspTrayEnableF_2"));
            if (tag != null)
            {
                bool bEnableF_2 = (bool)GetValue(tag.Location);
                chxEnableF2.Checked = bEnableF_2;
            }

            tag = GetTag(ResourceUtility.GetString("RtInspTrayEnableR_2"));
            if (tag != null)
            {
                chxEnableR2.Checked = (bool)GetValue(tag.Location);
            }
            tag = GetTag("RtInspTrayMultiPatternShowOffset_FW");
            if (tag != null)
            {
                chxShowOffsetF.Checked = (bool)GetValue(tag.Location);
            }
            tag = GetTag("RtInspTrayMultiPatternShowOffset_RV");
            if (tag != null)
            {
                chxShowOffsetR.Checked = (bool)GetValue(tag.Location);
            }

          
        }
        private void btnSearchRegionA_Click(object sender, EventArgs e)
        {
            //string nameTag = string.Format("RtInsp{0}TrainRegionR", mFinderTypeName);
            //var tag = GetTag(ResourceUtility.GetString(nameTag));
            //if (tag != null)
            //{
            //    EditCellGraphic(tag.Location);
            //}
            MessageLoggerManager.Log.Info(String.Format("[Action] Click Search Region Align"));
        }

        private void chxShowOffsetF_CheckedChanged(object sender, EventArgs e)
        {
            var tag = GetTag("RtInspTrayMultiPatternShowOffset_FW");
            if (tag != null)
            {
                InsightSetValue(tag.Location, chxShowOffsetF.Checked);
            }
            if (chxShowOffsetF.Checked)
            {

                MessageLoggerManager.Log.Info(String.Format("[Action] Show Offset F: Checked"));
            }
            else
            {
                MessageLoggerManager.Log.Info(String.Format("[Action] Show Offset F: Unchecked"));
            }
        }

        private void chxShowOffsetR_CheckedChanged(object sender, EventArgs e)
        {
            var tag = GetTag("RtInspTrayMultiPatternShowOffset_RV");
            if (tag != null)
            {
                InsightSetValue(tag.Location, chxShowOffsetR.Checked);
            }
            if (chxShowOffsetR.Checked)
            {

                MessageLoggerManager.Log.Info(String.Format("[Action] Show Offset R: Checked"));
            }
            else
            {
                MessageLoggerManager.Log.Info(String.Format("[Action] Show Offset R: Unchecked"));
            }
        }

    }
}
