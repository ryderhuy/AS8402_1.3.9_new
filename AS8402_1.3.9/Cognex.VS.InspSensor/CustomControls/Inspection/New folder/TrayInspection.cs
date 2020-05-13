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

namespace Cognex.VS.InspSensor.CustomControls
{
    public partial class TrayInspection : UserControl
    {
        public EventHandler SettingDoneEvent;
        private CvsInSightDisplay mCvsInsightDisplay = null;
        private string mFinderTypeName = "Tray";
        private bool isInit = true;
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
            mCvsInsightDisplay.InSight.ClickButton(location);
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

        }

        private void btnTrainRegionR_Click(object sender, EventArgs e)
        {
            string nameTag = string.Format("RtInsp{0}TrainRegionR", mFinderTypeName);
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                EditCellGraphic(tag.Location);
            }
        }

        private void btnSearchRegionF_Click(object sender, EventArgs e)
        {
            string nameTag = string.Format("RtInsp{0}SearchRegionF", mFinderTypeName);
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                EditCellGraphic(tag.Location);
            }
        }

        private void btnSearchRegionR_Click(object sender, EventArgs e)
        {
            string nameTag = string.Format("RtInsp{0}SearchRegionR", mFinderTypeName);
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                EditCellGraphic(tag.Location);
            }
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
        }

        private void btnTrainPMF_Click(object sender, EventArgs e)
        {
            string nameTag = string.Format("RtInsp{0}TrainButtonF", mFinderTypeName);
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                InsightClickButton(tag.Location);
            }
        }

        private void btnTrainPMR_Click(object sender, EventArgs e)
        {
            string nameTag = string.Format("RtInsp{0}TrainButtonR", mFinderTypeName);
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                InsightClickButton(tag.Location);
            }
        }

        private void lblTrainGoldenR_Click(object sender, EventArgs e)
        {

        }

        private void numContrastThresholdF_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                //Forward
                string nameTag = string.Format("RtInsp{0}ContrastThresholdF", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numContrastThresholdF.Value);
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
                //Forward
                string nameTag = string.Format("RtInsp{0}AngleRangeF", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numAngleRangeF.Value);
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
                string nameTag = string.Format("RtInsp{0}ContrastThresholdR", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numContrastThresholdR.Value);
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
                string nameTag = string.Format("RtInsp{0}AngleRangeR", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numAngleRangeR.Value);
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

            }
            catch (Exception ex)
            {

            }
        }

        private void numScoreThresholdF_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                //Reverse
                string nameTag = string.Format("RtInsp{0}ScoreThresholdF", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numScoreThresholdF.Value);
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
                string nameTag = string.Format("RtInsp{0}ScoreThresholdR", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numScoreThresholdR.Value);
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
            }
            catch (Exception ex)
            {

            }

        }

        private void numAlignPocketGraphic_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtAlignTrayGraphic");
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numAlignPocketGraphic.Value);
                }
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
            }
            catch (Exception ex)
            {

            }
        }

        private void cbxPocketTolerance_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var tag = GetTag(string.Format("Align.Pocket.Tolerance.X_{0}",cbxPocketTolerance.SelectedIndex + 1));
                if (tag != null)
                {
                    float Value = (float)GetValue(tag.Location);
                    numToleranceX.Value = (decimal)Value;
                }

                tag = GetTag(string.Format("Align.Pocket.Tolerance.Y_{0}", cbxPocketTolerance.SelectedIndex + 1));
                if (tag != null)
                {
                    float Value = (float)GetValue(tag.Location);
                    numToleranceY.Value = (decimal)Value;
                }

                tag = GetTag(string.Format("Align.Pocket.Tolerance.T_{0}", cbxPocketTolerance.SelectedIndex + 1));
                if (tag != null)
                {
                    float Value = (float)GetValue(tag.Location);
                    numToleranceTheta.Value = (decimal)Value;
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
                var tag = GetTag(nameTag);//GetTag(ResourceUtility.GetString(nameTag));
                string test = ResourceUtility.GetString(nameTag);
                if (tag != null)
                {
                    EditCellGraphic(tag.Location);
                }
            }
            catch { }
        }

        private void btnSearchRegionR2_Click(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}SearchRegionR_2", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    EditCellGraphic(tag.Location);
                }
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
            }
            catch { }
        }

        private void btnGetOffsetR_Click(object sender, EventArgs e)
        {
            float x1 = 0, y1 = 0;
            // get Offset Value
            try
            {
                string nameTagX1 = string.Format("RtInsp{0}TrainButtonR", mFinderTypeName);
                var tag1 = GetTag(ResourceUtility.GetString(nameTagX1));
                if (tag1 != null)
                {
                    x1 = (float)GetValue(tag1.Location);
                }

                string nameTagY1 = string.Format("RtInsp{0}TrainButtonR", mFinderTypeName);
                var tag2 = GetTag(ResourceUtility.GetString(nameTagY1));
                if (tag2 != null)
                {
                    y1 = (float)GetValue(tag2.Location);
                }
            }
            catch { }

            // Set Offset value
            try
            {
                string nameTagX2 = string.Format("RtInsp{0}TrainButtonR", mFinderTypeName);
                var tag1 = GetTag(ResourceUtility.GetString(nameTagX2));
                if (tag1 != null)
                {
                    InsightSetValue(tag1.Location, x1);
                }

                string nameTagY2 = string.Format("RtInsp{0}TrainButtonR", mFinderTypeName);
                var tag2 = GetTag(ResourceUtility.GetString(nameTagY2));
                if (tag2 != null)
                {
                    InsightSetValue(tag2.Location, y1);
                }
            }
            catch
            {

            }
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
                var tag = GetTag(nameTag);//GetTag(ResourceUtility.GetString(nameTag));
                string test = ResourceUtility.GetString(nameTag);
                if (tag != null)
                {
                    EditCellGraphic(tag.Location);
                }
            }
            catch { }
        }

        private void btnSearchRegionF2_Click(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}SearchRegionF_2", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    EditCellGraphic(tag.Location);
                }
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
            }
            catch { }
        }
        #endregion

        private void btnGetOffsetF_Click(object sender, EventArgs e)
        {
            float x1 = 0, y1 = 0;
            // get Offset Value
            try
            {
                string nameTagX1 = string.Format("RtInsp{0}TrainButtonF", mFinderTypeName);
                var tag1 = GetTag(ResourceUtility.GetString(nameTagX1));
                if (tag1 != null)
                {
                    x1 = (float)GetValue(tag1.Location);
                }

                string nameTagY1 = string.Format("RtInsp{0}TrainButtonF", mFinderTypeName);
                var tag2 = GetTag(ResourceUtility.GetString(nameTagY1));
                if (tag2 != null)
                {
                    y1 = (float)GetValue(tag2.Location);
                }
            }
            catch { }

            // Set Offset value
            try
            {
                string nameTagX2 = string.Format("RtInsp{0}TrainButtonF", mFinderTypeName);
                var tag1 = GetTag(ResourceUtility.GetString(nameTagX2));
                if (tag1 != null)
                {
                    InsightSetValue(tag1.Location, x1);
                }

                string nameTagY2 = string.Format("RtInsp{0}TrainButtonF", mFinderTypeName);
                var tag2 = GetTag(ResourceUtility.GetString(nameTagY2));
                if (tag2 != null)
                {
                    InsightSetValue(tag2.Location, y1);
                }
            }
            catch
            {

            }
        }
    }
}
