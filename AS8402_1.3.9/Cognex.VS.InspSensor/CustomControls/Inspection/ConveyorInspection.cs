using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cognex.InSight.Controls.Display;
using Cognex.VS.Utility;
using Cognex.InSight;
using MessageManager;

namespace Cognex.VS.InspSensor.CustomControls
{
    public partial class ConveyorInspection : UserControl
    {
        public EventHandler SettingDoneEvent;
        private CvsInSightDisplay mCvsInsightDisplay = null;
        private string mFinderTypeName = "Conveyor";
        bool mInit = false;
        public ConveyorInspection()
        {
            InitializeComponent();
        }

        public string Origin1X
        {
            get { return txtOrigin1X.Text; }
        }

        public string Origin1Y
        {
            get { return txtOrigin1Y.Text; }
        }

        public string Origin2X
        {
            get { return txtOrigin2X.Text; }
        }

        public string Origin2Y
        {
            get { return txtOrigin2Y.Text; }
        }


        public void Init(CvsInSightDisplay insightDisplay)
        {
            mCvsInsightDisplay = insightDisplay;
            if (insightDisplay.Connected)
            {
                mInit = true;
                UpdateGUIParams();
                mInit = false;
            }
        }

        private void UpdateGUIParams()
        {
            try
            {
                var tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}SortType", mFinderTypeName)));
                if (tag != null)
                {
                    int index = (int)GetValue(tag.Location);
                    cbxSortFinderResult.SelectedIndex = index;
                }

                tag = GetTag(ResourceUtility.GetString("RtInspConveyorEnableCopyPose"));
                if (tag != null)
                {
                    bool isCheck = (bool)GetValue(tag.Location);
                    chxEnableCopyPose.Checked = (bool)isCheck;
                }


                tag = GetTag("Target.1.Pattern.ContrastThreshold");
                if (tag != null)
                {
                    float contrast = (float)GetValue(tag.Location);
                    numContrastThresholdF1.Value = (decimal)contrast;
                }

                tag = GetTag("Target.1.Pattern.AngleRange");
                if (tag != null)
                {
                    float angle = (float)GetValue(tag.Location);
                    numAngleRangeF1.Value = (decimal)angle;
                }

                tag = GetTag("Target.1.Pattern.AcceptThreshold");
                if (tag != null)
                {
                    float score = (float)GetValue(tag.Location);
                    numScoreThresholdF1.Value = (decimal)score;
                }

                tag = GetTag("Target.1.Pattern.ScaleRange");
                if (tag != null)
                {
                    float scale = (float)GetValue(tag.Location);
                    numAlignScaleF1.Value = (decimal)scale;
                }

                tag = GetTag("Target.1.Pattern.ShowOffset");
                if (tag != null)
                {
                    bool isShow = (bool)GetValue(tag.Location);
                    chxShowOffsetF1.Checked = (bool)isShow;
                }

                tag = GetTag("Target.2.Pattern.ContrastThreshold");
                if (tag != null)
                {
                    float contrast = (float)GetValue(tag.Location);
                    numContrastThresholdF2.Value = (decimal)contrast;
                }

                tag = GetTag("Target.2.Pattern.AngleRange");
                if (tag != null)
                {
                    float angle = (float)GetValue(tag.Location);
                    numAngleRangeF2.Value = (decimal)angle;
                }

                tag = GetTag("Target.2.Pattern.AcceptThreshold");
                if (tag != null)
                {
                    float score = (float)GetValue(tag.Location);
                    numScoreThresholdF2.Value = (decimal)score;
                }

                tag = GetTag("Target.2.Pattern.ScaleRange");
                if (tag != null)
                {
                    float scale = (float)GetValue(tag.Location);
                    numAlignScaleF2.Value = (decimal)scale;
                }

                tag = GetTag("Target.2.Pattern.ShowOffset");
                if (tag != null)
                {
                    bool isShow = (bool)GetValue(tag.Location);
                    chxShowOffsetF2.Checked = (bool)isShow;
                }

                tag = GetTag("Target.Pattern.FinderType");
                if (tag != null)
                {
                    int index = (int)GetValue(tag.Location);
                    cbxFinalFinderType.SelectedIndex = index;
                }

                tag = GetTag("Target.1.Pattern.SaveOrigin.X");
                if (tag != null)
                {
                    float origin = (float)GetValue(tag.Location);
                    txtOrigin1X.Text = origin.ToString();
                }

                tag = GetTag("Target.1.Pattern.SaveOrigin.Y");
                if (tag != null)
                {
                    float origin = (float)GetValue(tag.Location);
                    txtOrigin1Y.Text = origin.ToString();
                }

                tag = GetTag("Target.2.Pattern.SaveOrigin.X");
                if (tag != null)
                {
                    float origin = (float)GetValue(tag.Location);
                    txtOrigin2X.Text = origin.ToString();
                }

                tag = GetTag("Target.2.Pattern.SaveOrigin.Y");
                if (tag != null)
                {
                    float origin = (float)GetValue(tag.Location);
                    txtOrigin2Y.Text = origin.ToString();
                }

                tag = GetTag("AcquisitionSettings.1.ExposureTime");
                if (tag != null)
                {
                    float exp = (float)GetValue(tag.Location);
                    numExposure1.Value = (decimal)exp;
                }

                tag = GetTag("AcquisitionSettings.2.ExposureTime");
                if (tag != null)
                {
                    float exp = (float)GetValue(tag.Location);
                    numExposure2.Value = (decimal)exp;
                }

                //Intersection setting

                tag = GetTag("Target.1.Feature.Type");
                if (tag != null)
                {
                    int type = (int)GetValue(tag.Location);
                    cbxFinderType.SelectedIndex = (int)type;
                }

                tag = GetTag("Target.1.CornerFinder.FixtureType");
                if (tag != null)
                {
                    int type = (int)GetValue(tag.Location);
                    cbxEdgeFixtureType1.SelectedIndex = (int)type;
                }

                tag = GetTag("Target.2.CornerFinder.FixtureType");
                if (tag != null)
                {
                    int type = (int)GetValue(tag.Location);
                    cbxEdgeFixtureType2.SelectedIndex = (int)type;
                }

                cbxLineSearchRegion1.SelectedIndex = 0;
                cbxLineSearchRegion2.SelectedIndex = 0;

                #region Condition Params

                tag = GetTag("Align.Condition.PM.Contrast");
                if (tag != null)
                {
                    float contrast = (float)GetValue(tag.Location);
                    numPMContrast.Value = (decimal)contrast;
                }

                tag = GetTag("Align.Condition.PM.Score");
                if (tag != null)
                {
                    float score = (float)GetValue(tag.Location);
                    numPMScore.Value = (decimal)score;
                }

                tag = GetTag("Align.Condition.PM.Angle");
                if (tag != null)
                {
                    float angle = (float)GetValue(tag.Location);
                    numPMAngle.Value = (decimal)angle;
                }

                tag = GetTag("Align.Condition.Hist.FirstBin");
                if (tag != null)
                {
                    float first = (float)GetValue(tag.Location);
                    numHistFirstBin.Value = (decimal)first;
                }

                tag = GetTag("Align.Condition.Hist.LastBin");
                if (tag != null)
                {
                    float last = (float)GetValue(tag.Location);
                    numHistLastBin.Value = (decimal)last;
                }

                tag = GetTag("Align.Condition.Hist.MeanThreshold");
                if (tag != null)
                {
                    float mean = (float)GetValue(tag.Location);
                    numHistMeanThresh.Value = (decimal)mean;
                }

                tag = GetTag("Align.Condition.Hist.CountThreshold");
                if (tag != null)
                {
                    float count = (float)GetValue(tag.Location);
                    numHistCountThresh.Value = (decimal)count;
                }

                tag = GetTag("Align.Litmit.X");
                if (tag != null)
                {
                    float x = (float)GetValue(tag.Location);
                    numLimitX.Value = (decimal)x;
                }

                tag = GetTag("Align.Litmit.Y");
                if (tag != null)
                {
                    float y = (float)GetValue(tag.Location);
                    numLimitY.Value = (decimal)y;
                }

                tag = GetTag("Align.Litmit.Theta");
                if (tag != null)
                {
                    float theta = (float)GetValue(tag.Location);
                    numLimitTheta.Value = (decimal)theta;
                }

                tag = GetTag("Align.Condition.ShowGraphic");
                if (tag != null)
                {
                    bool isCheck = (bool)GetValue(tag.Location);
                    chxConditionGraphic.Checked = (bool)isCheck;
                }

                tag = GetTag("Align.Condition.Hist.Type");
                if (tag != null)
                {
                    int type = (int)GetValue(tag.Location);
                    cbxHistCondType.SelectedIndex = (int)type;
                }

                tag = GetTag("Align.Condition.PM.Type");
                if (tag != null)
                {
                    int type = (int)GetValue(tag.Location);
                    cbxPatmaxCondType.SelectedIndex = (int)type;
                }

                tag = GetTag("Align.Condition.PM.Enable");
                if (tag != null)
                {
                    bool isCheck = (bool)GetValue(tag.Location);
                    chxPatmaxCondEnable.Checked = (bool)isCheck;
                }

                tag = GetTag("Align.Condition.Hist.Enable");
                if (tag != null)
                {
                    bool isCheck = (bool)GetValue(tag.Location);
                    chxHistCondEnable.Checked = (bool)isCheck;
                }

                tag = GetTag("Align.Limit.Enable");
                if (tag != null)
                {
                    bool isCheck = (bool)GetValue(tag.Location);
                    chxEnableLimit.Checked = (bool)isCheck;
                }

                #endregion
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

        private void InsightSetListBoxValue(CvsCellLocation location, int value)
        {
            mCvsInsightDisplay.InSight.SetListBoxIndex(location, value);
        }

        private object GetValue(CvsCellLocation location)
        {
            Cognex.InSight.Cell.CvsCell c = mCvsInsightDisplay.InSight.Results.Cells[location];
            var type = c.GetType();
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
            else if (c.GetType() == typeof(Cognex.InSight.Cell.CvsCellFloat))
            {
                return ((Cognex.InSight.Cell.CvsCellFloat)c).Value;
            }
            else if (c.GetType() == typeof(Cognex.InSight.Cell.CvsCellListBox))
            {
                return ((Cognex.InSight.Cell.CvsCellListBox)c).Value;
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
                if (!chxEnable.Checked)
                {
                    if (MessageBox.Show("Do you want to enable this function?", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                    {
                        string nameTag = string.Format("RtInsp{0}Enable", mFinderTypeName);
                        var tag = GetTag(ResourceUtility.GetString(nameTag));
                        if (tag != null)
                        {
                            InsightSetValue(tag.Location, true);
                        }
                        chxEnable.Checked = true;
                    }
                }

                SettingDoneEvent.BeginInvoke(null, null, null, null);
            }
            catch (CvsException ex)
            {

            }
        }

        private void cbxSortFinderResult_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}SortType", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)(cbxSortFinderResult.SelectedIndex));
                }
                MessageLoggerManager.Log.Info("[Action] Sort Finder Result: " + cbxSortFinderResult.SelectedItem);
            }
            catch (Exception ex)
            {

            }
        }

        private void chxInspectionDistance_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}Distance", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (bool)chxInspectionDistance.Checked);
                }
                if (chxInspectionDistance.Checked)
                {

                    MessageLoggerManager.Log.Info(String.Format("[Action] Inspection Distance: Checked"));
                }
                else
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] Inspection Distance: Unchecked"));
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void chxEnable_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}Enable", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, chxEnable.Checked);
                }
                if (chxEnable.Checked)
                {

                    MessageLoggerManager.Log.Info(String.Format("[Action] Enable: Checked"));
                }
                else
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] Enable: Unchecked"));
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void chxEnableCopyPose_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInspConveyorEnableCopyPose");
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, chxEnableCopyPose.Checked);
                }
                if (chxEnableCopyPose.Checked)
                {

                    MessageLoggerManager.Log.Info(String.Format("[Action] One Target For Two Tools: Checked"));
                }
                else
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] One Target For Two Tools: Unchecked"));
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void numAlignFeature1Scale_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = "Target.1.Pattern.ScaleRange";
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)(numAlignScaleF1.Value));
                }
                MessageLoggerManager.Log.Info("[Action] Align Feature Scale F: " + (float)numAlignScaleF1.Value);
            }
            catch (Exception ex)
            {

            }
        }

        private void numAlignFeature2Scale_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = "Target.2.Pattern.ScaleRange";
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)(numAlignScaleF2.Value));
                }
                MessageLoggerManager.Log.Info("[Action] Align Feature Scale 2: " + (float)numAlignScaleF2.Value);
            }
            catch (Exception ex)
            {

            }
        }
        #region Open Patmax Cell
        private void btnTrainPM1_Click(object sender, EventArgs e)
        {
            try
            {
                CvsCellLocation location = new CvsCellLocation(51, 'L');
                if (true)
                {
                    mCvsInsightDisplay.OpenPropertySheet(location, null);
                }
                MessageLoggerManager.Log.Info("[Action] Click TrainPM F");
            }
            catch (Exception ex)
            {

            }
        }

        private void btnFindPM1_Click(object sender, EventArgs e)
        {
            try
            {
                CvsCellLocation location = new CvsCellLocation(67, 'K');
                if (true)
                {
                    mCvsInsightDisplay.OpenPropertySheet(location, null);
                }
                MessageLoggerManager.Log.Info("[Action] Click FindPM F");
            }
            catch (Exception ex)
            {

            }
        }

        private void btnTrainPM2_Click(object sender, EventArgs e)
        {
            try
            {
                CvsCellLocation location = new CvsCellLocation(120, 'L');
                if (true)
                {
                    mCvsInsightDisplay.OpenPropertySheet(location, null);
                }
                MessageLoggerManager.Log.Info("[Action] Click TrainPM F2");
            }
            catch (Exception ex)
            {

            }
        }

        private void btnFindPM2_Click(object sender, EventArgs e)
        {
            try
            {
                CvsCellLocation location = new CvsCellLocation(136, 'K');
                if (true)
                {
                    mCvsInsightDisplay.OpenPropertySheet(location, null);
                }
                MessageLoggerManager.Log.Info("[Action] Click FindPM F2");
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        private void EditRegionWithName(string name)
        {
            try
            {
                string nameTag = name;
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    EditCellGraphic(tag.Location);
                }
            }
            catch (Exception ex) { }
        }

        private void ClickButtonWithName(string name)
        {
            try
            {
                string nameTag = name;
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightClickButton(tag.Location);
                }
            }
            catch (Exception ex) { }
        }

        private void ResetRegionWithName(string name)
        {
            string nameTag = name;
            var tag = GetTag(nameTag);
            if (tag != null)
            {
                mCvsInsightDisplay.OpenPropertySheet(tag.Location, null);
            }
        }

        private void btnTrainRegionF1_Click(object sender, EventArgs e)
        {
            EditRegionWithName("Target.1.Pattern.TrainRegion");
            MessageLoggerManager.Log.Info("[Action] Click Train Region F");
        }

        private void btnTrainPMF1_Click(object sender, EventArgs e)
        {
            ClickButtonWithName("Target.1.Pattern.Train");
            MessageLoggerManager.Log.Info("[Action] Click Train F");
        }

        private void btnSearchRegionF1_Click(object sender, EventArgs e)
        {
            EditRegionWithName("Target.1.Pattern.SearchRegion");
            MessageLoggerManager.Log.Info("[Action] Click Search Region F");
        }

        private void btnTrainGoldenF1_Click(object sender, EventArgs e)
        {
            MessageLoggerManager.Log.Info("[Action] Click Train Golden F");
            if (MessageBox.Show("Is this \"MASTER\" position?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                ClickButtonWithName("Target.1.Pattern.SaveOrigin");

                //System.Threading.Thread.Sleep(5000);
                var tag = GetTag("Target.1.Pattern.SaveOrigin.X");
                if (tag != null)
                {
                    float origin = (float)GetValue(tag.Location);
                    txtOrigin1X.Text = origin.ToString();
                }

                tag = GetTag("Target.1.Pattern.SaveOrigin.Y");
                if (tag != null)
                {
                    float origin = (float)GetValue(tag.Location);
                    txtOrigin1Y.Text = origin.ToString();
                }
            }
        }

        private void chxShowOffsetF1_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Target.1.Pattern.ShowOffset");
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, chxShowOffsetF1.Checked);
                }
                if (chxShowOffsetF1.Checked)
                {
                    MessageLoggerManager.Log.Info("[Action] Show Offset F: Checked");
                }
                else
                {
                    MessageLoggerManager.Log.Info("[Action] Show Offset F: Unchecked");
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void numContrastThresholdF1_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Target.1.Pattern.ContrastThreshold");
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numContrastThresholdF1.Value);
                }
                MessageLoggerManager.Log.Info("[Action] Contrast Threshold F: " + (float)numContrastThresholdF1.Value);
            }
            catch (Exception ex)
            {

            }
        }

        private void numAngleRangeF1_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Target.1.Pattern.AngleRange");
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numAngleRangeF1.Value);
                }
                MessageLoggerManager.Log.Info("[Action] Contrast Angle Range F: " + (float)numAngleRangeF1.Value);
            }
            catch (Exception ex)
            {

            }
        }

        private void numScoreThresholdF1_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Target.1.Pattern.AcceptThreshold");
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numScoreThresholdF1.Value);
                }
                MessageLoggerManager.Log.Info("[Action] Score Threshold F: " + (float)numScoreThresholdF1.Value);
            }
            catch (Exception ex)
            {

            }
        }

        private void btnTrainRegionF2_Click(object sender, EventArgs e)
        {
            EditRegionWithName("Target.2.Pattern.TrainRegion");
            MessageLoggerManager.Log.Info("[Action] Click Train Region F2");
        }

        private void btnTrainPMF2_Click(object sender, EventArgs e)
        {
            ClickButtonWithName("Target.2.Pattern.Train");
            MessageLoggerManager.Log.Info("[Action] Click TrainPM F2");
        }

        private void btnSearchRegionF2_Click(object sender, EventArgs e)
        {
            EditRegionWithName("Target.2.Pattern.SearchRegion");
            MessageLoggerManager.Log.Info("[Action] Click Search Region F2");
        }

        private void btnTrainGoldenF2_Click(object sender, EventArgs e)
        {
            MessageLoggerManager.Log.Info("[Action] Click Train Golden F2");
            if (MessageBox.Show("Is this \"MASTER\" position?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                ClickButtonWithName("Target.2.Pattern.SaveOrigin");
                //System.Threading.Thread.Sleep(5000);
                var tag = GetTag("Target.2.Pattern.SaveOrigin.X");
                if (tag != null)
                {
                    float origin = (float)GetValue(tag.Location);
                    txtOrigin2X.Text = origin.ToString();
                }

                tag = GetTag("Target.2.Pattern.SaveOrigin.Y");
                if (tag != null)
                {
                    float origin = (float)GetValue(tag.Location);
                    txtOrigin2Y.Text = origin.ToString();
                }
            }
        }

        private void chxShowOffsetF2_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Target.2.Pattern.ShowOffset");
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, chxShowOffsetF2.Checked);
                }
                if (chxShowOffsetF2.Checked)
                {
                    MessageLoggerManager.Log.Info("[Action] Show Offset F2: Checked");
                }
                else
                {
                    MessageLoggerManager.Log.Info("[Action] Show Offset F2: Unchecked");
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void numContrastThresholdF2_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Target.2.Pattern.ContrastThreshold");
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numContrastThresholdF2.Value);
                }
                MessageLoggerManager.Log.Info("[Action] ContrastThreshold F2: " + (float)numContrastThresholdF2.Value);
            }
            catch (Exception ex)
            {

            }
        }

        private void numAngleRangeF2_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Target.2.Pattern.AngleRange");
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numAngleRangeF2.Value);
                }
                MessageLoggerManager.Log.Info("[Action] Angle Range F2: " + (float)numAngleRangeF2.Value);
            }
            catch (Exception ex)
            {

            }
        }

        private void numScoreThresholdF2_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Target.2.Pattern.AcceptThreshold");
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numScoreThresholdF2.Value);
                }
                MessageLoggerManager.Log.Info("[Action] Score Threshold F2: " + (float)numScoreThresholdF2.Value);
            }
            catch (Exception ex)
            {

            }
        }

        private void cbxFinderType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Target.Pattern.FinderType");
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)cbxFinalFinderType.SelectedIndex);
                }
                MessageLoggerManager.Log.Info("[Action] Finder Type: " + cbxFinalFinderType.SelectedItem);
            }
            catch (Exception ex)
            {

            }
        }

        private void btnLoadOrigin2_Click(object sender, EventArgs e)
        {
            //if (MessageBox.Show("Are you sure to input Master 2 Origin?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            //{
            //    if (txtOrigin2X.Text != "" && txtOrigin2Y.Text != "")
            //    {
            //        string nameTag = string.Format("Target.2.Pattern.SaveOrigin.X");
            //        var tag = GetTag(nameTag);
            //        if (tag != null)
            //        {
            //            InsightSetValue(tag.Location, float.Parse(txtOrigin2X.Text));
            //        }

            //        nameTag = string.Format("Target.2.Pattern.SaveOrigin.Y");
            //        tag = GetTag(nameTag);
            //        if (tag != null)
            //        {
            //            InsightSetValue(tag.Location, float.Parse(txtOrigin2Y.Text));
            //        }
            //    }
            //    else
            //    {
            //        MessageBox.Show("Master Origin can not be null", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    }
            //}
        }

        private void btnLoadOrigin1_Click(object sender, EventArgs e)
        {
            //if (MessageBox.Show("Are you sure to input Master 1 Origin?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            //{
            //    if (txtOrigin1X.Text != "" && txtOrigin1Y.Text != "")
            //    {
            //        string nameTag = string.Format("Target.1.Pattern.SaveOrigin.X");
            //        var tag = GetTag(nameTag);
            //        if (tag != null)
            //        {
            //            InsightSetValue(tag.Location, float.Parse(txtOrigin1X.Text));
            //        }

            //        nameTag = string.Format("Target.1.Pattern.SaveOrigin.Y");
            //        tag = GetTag(nameTag);
            //        if (tag != null)
            //        {
            //            InsightSetValue(tag.Location, float.Parse(txtOrigin1Y.Text));
            //        }
            //    }
            //    else
            //    {
            //        MessageBox.Show("Master Origin can not be null", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    }
            //}
        }

        private void UpdateParamsIntersection(int featureNumber, int selectedIndex)
        {
            var tag = GetTag(string.Format("Target.{0}.CornerFinder.FixtureType", featureNumber));
            if (tag != null)
            {
                int type = (int)GetValue(tag.Location);
                cbxEdgeFixtureType1.SelectedIndex = (int)type;
            }

            tag = GetTag(string.Format("Target.{0}.CornerFinder.FixtureType", featureNumber));
            if (tag != null)
            {
                int type = (int)GetValue(tag.Location);
                cbxEdgeFixtureType1.SelectedIndex = (int)type;
            }

            tag = GetTag(string.Format("Target.{0}.CornerFinder.Edge{1}.Polarity", featureNumber, selectedIndex));
            if (tag != null)
            {
                int type = (int)GetValue(tag.Location);
                if (featureNumber == 1)
                    cbxLinePolarity1.SelectedIndex = (int)type;
                else
                    cbxLinePolarity2.SelectedIndex = (int)type;
            }

            tag = GetTag(string.Format("Target.{0}.CornerFinder.Edge{1}.FindBy", featureNumber, selectedIndex));
            if (tag != null)
            {
                int type = (int)GetValue(tag.Location);
                if (featureNumber == 1)
                    cbxLineFindBy1.SelectedIndex = (int)type;
                else
                    cbxLineFindBy2.SelectedIndex = (int)type;
            }

            tag = GetTag(string.Format("Target.{0}.CornerFinder.Edge{1}.EdgeWidth", featureNumber, selectedIndex));
            if (tag != null)
            {
                int type = (int)GetValue(tag.Location);
                if (featureNumber == 1)
                    numEdgeWidth1.Value = (int)type;
                else
                    numEdgeWidth2.Value = (int)type;
            }

            tag = GetTag(string.Format("Target.{0}.CornerFinder.Edge{1}.AcceptThreshold", featureNumber, selectedIndex));
            if (tag != null)
            {
                int type = (int)GetValue(tag.Location);
                if (featureNumber == 1)
                    numLineThreshold1.Value = (int)type;
                else
                    numLineThreshold2.Value = (int)type;
            }

        }

        private void cbxLineSearchRegion1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!mInit)
            {
                EditRegionWithName(string.Format("Target.1.CornerFinder.Edge{0}.Region", cbxLineSearchRegion1.SelectedIndex));
            }
            MessageLoggerManager.Log.Info("[Action] Line Search Region: " + cbxLineSearchRegion1.SelectedItem);
            UpdateParamsIntersection(1, cbxLineSearchRegion1.SelectedIndex);
        }

        private void btnLineSearchRegion1_Click(object sender, EventArgs e)
        {
            EditRegionWithName(string.Format("Target.1.CornerFinder.Edge{0}.Region", cbxLineSearchRegion1.SelectedIndex));
            MessageLoggerManager.Log.Info("[Action] Click Line Search Region");
        }

        private void btnLineTrainGolden1_Click(object sender, EventArgs e)
        {
            ClickButtonWithName(string.Format("Target.1.CornerFinder.Edge{0}.Train", cbxLineSearchRegion1.SelectedIndex));
            MessageLoggerManager.Log.Info("[Action] Click Line Train Golden");
        }

        private void btnLineResetAllRegion1_Click(object sender, EventArgs e)
        {
            ResetRegionWithName(string.Format("Target.1.CornerFinder.Edge{0}.Region", cbxLineSearchRegion1.SelectedIndex));
            MessageLoggerManager.Log.Info("[Action] Click Line Reset All Region");
        }

        private void cbxLinePolarity1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Target.1.CornerFinder.Edge{0}.Polarity", cbxLineSearchRegion1.SelectedIndex);
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetListBoxValue(tag.Location, (int)cbxLinePolarity1.SelectedIndex);
                }
                MessageLoggerManager.Log.Info("[Action] Line Polarity: " + cbxLinePolarity1.SelectedItem);
            }
            catch (Exception ex)
            {

            }
        }

        private void cbxLineFindBy1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Target.1.CornerFinder.Edge{0}.FindBy", cbxLineSearchRegion1.SelectedIndex);
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetListBoxValue(tag.Location, (int)cbxLineFindBy1.SelectedIndex);
                }
                MessageLoggerManager.Log.Info("[Action] Line Find By : " + cbxLineFindBy1.SelectedItem);
            }
            catch (Exception ex)
            {

            }
        }

        private void numEdgeWidth1_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Target.1.CornerFinder.Edge{0}.EdgeWidth", cbxLineSearchRegion1.SelectedIndex);
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)numEdgeWidth1.Value);
                }
                MessageLoggerManager.Log.Info("[Action] Edge Width: " + (int)numEdgeWidth1.Value);
            }
            catch (Exception ex)
            {

            }
        }

        private void numLineThreshold1_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Target.1.CornerFinder.Edge{0}.AcceptThreshold", cbxLineSearchRegion1.SelectedIndex);
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)numLineThreshold1.Value);
                }
                MessageLoggerManager.Log.Info("[Action] Line Threshold: " + (int)numLineThreshold1.Value);
            }
            catch (Exception ex)
            {

            }
        }

        private void cbxLineSearchRegion2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!mInit)
            {
                EditRegionWithName(string.Format("Target.2.CornerFinder.Edge{0}.Region", cbxLineSearchRegion2.SelectedIndex));
            }
            MessageLoggerManager.Log.Info("[Action] Line Search Region 2: " + cbxLineSearchRegion2.SelectedItem);
            UpdateParamsIntersection(2, cbxLineSearchRegion2.SelectedIndex);
        }

        private void btnLineSearchRegion2_Click(object sender, EventArgs e)
        {
            EditRegionWithName(string.Format("Target.2.CornerFinder.Edge{0}.Region", cbxLineSearchRegion2.SelectedIndex));
            MessageLoggerManager.Log.Info("[Action] Click Line Search Region 2");
        }

        private void btnLineResetAllRegion2_Click(object sender, EventArgs e)
        {
            ResetRegionWithName(string.Format("Target.2.CornerFinder.Edge{0}.Region", cbxLineSearchRegion2.SelectedIndex));
            MessageLoggerManager.Log.Info("[Action] Click Line Reset All Region 2");
        }

        private void btnLineTrainGolden2_Click(object sender, EventArgs e)
        {
            ClickButtonWithName(string.Format("Target.2.CornerFinder.Edge{0}.Train", cbxLineSearchRegion2.SelectedIndex));
            MessageLoggerManager.Log.Info("[Action] Click Line Train Golden 2");
        }

        private void cbxLinePolarity2_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Target.2.CornerFinder.Edge{0}.Polarity", cbxLineSearchRegion2.SelectedIndex);
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetListBoxValue(tag.Location, (int)cbxLinePolarity2.SelectedIndex);
                }
                MessageLoggerManager.Log.Info("[Action] Line Polarity 2: " + cbxLinePolarity2.SelectedItem);
            }
            catch (Exception ex)
            {

            }
        }

        private void cbxLineFindBy2_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Target.2.CornerFinder.Edge{0}.FindBy", cbxLineSearchRegion2.SelectedIndex);
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetListBoxValue(tag.Location, (int)cbxLineFindBy2.SelectedIndex);
                }
                MessageLoggerManager.Log.Info("[Action] Line Find By 2: " + cbxLineFindBy2.SelectedItem);
            }
            catch (Exception ex)
            {

            }
        }

        private void numEdgeWidth2_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Target.2.CornerFinder.Edge{0}.EdgeWidth", cbxLineSearchRegion2.SelectedIndex);
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)numEdgeWidth2.Value);
                }
                MessageLoggerManager.Log.Info("[Action] Edge Width 2: " + (int)numEdgeWidth2.Value);
            }
            catch (Exception ex)
            {

            }
        }

        private void numLineThreshold2_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Target.2.CornerFinder.Edge{0}.AcceptThreshold", cbxLineSearchRegion2.SelectedIndex);
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)numLineThreshold2.Value);
                }
                MessageLoggerManager.Log.Info("[Action] Line Threshold 2: " + (int)numLineThreshold2.Value);
            }
            catch (Exception ex)
            {

            }
        }

        private void cbxEdgeFixtureType1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = "Target.1.CornerFinder.FixtureType";
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetListBoxValue(tag.Location, (int)cbxEdgeFixtureType1.SelectedIndex);
                }
                MessageLoggerManager.Log.Info("[Action] Edge Fixture Type: " + cbxEdgeFixtureType1.SelectedItem);
            }
            catch (Exception ex)
            {

            }
        }

        private void cbxEdgeFixtureType2_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = "Target.2.CornerFinder.FixtureType";
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetListBoxValue(tag.Location, (int)cbxEdgeFixtureType2.SelectedIndex);
                }
                MessageLoggerManager.Log.Info("[Action] Line Edge Fixture Type 2: " + cbxEdgeFixtureType2.SelectedItem);
            }
            catch (Exception ex)
            {

            }
        }

        private void cbxFinderType_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            try
            {
                string nameTag = "Target.1.Feature.Type";
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetListBoxValue(tag.Location, (int)cbxFinderType.SelectedIndex);
                }

                nameTag = "Target.2.Feature.Type";
                tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetListBoxValue(tag.Location, (int)cbxFinderType.SelectedIndex);
                }
                MessageLoggerManager.Log.Info("[Action] Finder Type: " + cbxFinderType.SelectedItem);

            }
            catch (Exception ex)
            {

            }
        }

        private void btnPMTrainRegion_Click(object sender, EventArgs e)
        {
            EditRegionWithName("Align.Condition.PM.TrainRegion");
            MessageLoggerManager.Log.Info("[Action] Click PM Train Region");
        }

        private void btnPMSearchRegion_Click(object sender, EventArgs e)
        {
            EditRegionWithName("Align.Condition.PM.SearchRegion");
            MessageLoggerManager.Log.Info("[Action] Click PM Search Region");
        }

        private void btnPMTrain_Click(object sender, EventArgs e)
        {
            ClickButtonWithName("Align.Condition.PM.TrainPattern");
            MessageLoggerManager.Log.Info("[Action] Click PM Train");
        }

        private void numPMContrast_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Align.Condition.PM.Contrast");
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numPMContrast.Value);
                }
                MessageLoggerManager.Log.Info("[Action] PM Contrast: " + (float)numPMContrast.Value);
                nameTag = string.Format("Align.Condition.PM.Score");
                tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numPMScore.Value);
                }
                MessageLoggerManager.Log.Info("[Action] PM Score: " + (float)numPMScore.Value);
                nameTag = string.Format("Align.Condition.PM.Angle");
                tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numPMAngle.Value);
                }
                MessageLoggerManager.Log.Info("[Action] PM Angle: " + (float)numPMAngle.Value);
            }
            catch (Exception ex)
            {

            }
        }

        private void numHistMeanThresh_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Align.Condition.Hist.FirstBin");
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numHistFirstBin.Value);
                }
                MessageLoggerManager.Log.Info("[Action] Hist First Bin: " + (float)numHistFirstBin.Value);

                nameTag = string.Format("Align.Condition.Hist.LastBin");
                tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numHistLastBin.Value);
                }
                MessageLoggerManager.Log.Info("[Action] Hist Last Bin: " + (float)numHistLastBin.Value);
                nameTag = string.Format("Align.Condition.Hist.MeanThreshold");
                tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numHistMeanThresh.Value);
                }
                MessageLoggerManager.Log.Info("[Action] Hist Mean Threst: " + (float)numHistMeanThresh.Value);
                nameTag = string.Format("Align.Condition.Hist.CountThreshold");
                tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numHistCountThresh.Value);
                }
                MessageLoggerManager.Log.Info("[Action] Hist Count Threst: " + (float)numHistCountThresh.Value);
            }
            catch (Exception ex)
            {

            }
        }

        private void btnHistSearchRegion_Click(object sender, EventArgs e)
        {
            EditRegionWithName("Align.Condition.Hist.Region");
            MessageLoggerManager.Log.Info("[Action] Click Hist Search Region");
        }

        private void numLimitX_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Align.Litmit.X");
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numLimitX.Value);
                }
                MessageLoggerManager.Log.Info("[Action] Limit X: " + (float)numLimitX.Value);
                nameTag = string.Format("Align.Litmit.Y");
                tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numLimitY.Value);
                }
                MessageLoggerManager.Log.Info("[Action] Limit Y: " + (float)numLimitY.Value);
                nameTag = string.Format("Align.Litmit.Theta");
                tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numLimitTheta.Value);
                }
                MessageLoggerManager.Log.Info("[Action] Limit Theta: " + (float)numLimitTheta.Value);
            }
            catch (Exception ex)
            {

            }
        }

        private void chxConditionGraphic_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Align.Condition.ShowGraphic");
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, chxConditionGraphic.Checked);
                }
                if (chxConditionGraphic.Checked)
                {
                    MessageLoggerManager.Log.Info("[Action] Show Graphic: Checked");
                }
                else
                {
                    MessageLoggerManager.Log.Info("[Action] Show Graphic: Unchecked");
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void cbxConditionType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = "Align.Condition.Hist.Type";
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetListBoxValue(tag.Location, (int)cbxHistCondType.SelectedIndex);
                }
                MessageLoggerManager.Log.Info("[Action] Condition Type: " + cbxHistCondType.SelectedItem);
            }
            catch (Exception ex)
            {

            }
        }

        private void chxEnableLimit_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Align.Limit.Enable");
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, chxEnableLimit.Checked);
                }
                if(chxEnableLimit.Checked){
                    MessageLoggerManager.Log.Info("[Action] Enable Limit: Checked");
                }else{
                    MessageLoggerManager.Log.Info("[Action] Enable Limit: Unchecked");
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void chxPatmaxCondEnable_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Align.Condition.PM.Enable");
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, chxPatmaxCondEnable.Checked);
                }
                if (chxPatmaxCondEnable.Checked)
                {
                    MessageLoggerManager.Log.Info("[Action] Enable Patmax: Checked");
                }
                else
                {
                    MessageLoggerManager.Log.Info("[Action] Enable Patmax: Unchecked");
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void chxHistCondEnable_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Align.Condition.Hist.Enable");
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, chxHistCondEnable.Checked);
                }
                if (chxHistCondEnable.Checked)
                {
                    MessageLoggerManager.Log.Info("[Action] Enable Histogram: Checked");
                }
                else
                {
                    MessageLoggerManager.Log.Info("[Action] Enable Histogram: Unchecked");
                }

            }
            catch (Exception ex)
            {

            }
        }

        private void cbxPatmaxCondType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = "Align.Condition.PM.Type";
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetListBoxValue(tag.Location, (int)cbxPatmaxCondType.SelectedIndex);
                }
                MessageLoggerManager.Log.Info("[Action] Patmax CondType: " + cbxPatmaxCondType.SelectedItem);
            }
            catch (Exception ex)
            {

            }
        }

        private void numExposure1_ValueChanged(object sender, EventArgs e)
        {
            string nameTag = "AcquisitionSettings.1.ExposureTime";
            var tag = GetTag(nameTag);
            if (tag != null)
            {
                InsightSetValue(tag.Location, (float)numExposure1.Value);
            }
            MessageLoggerManager.Log.Info("[Action] Exposure: " + (float)numExposure1.Value);
            nameTag = "AcquisitionSettings.Selector";
            tag = GetTag(nameTag);
            if (tag != null)
            {
                InsightSetListBoxValue(tag.Location, 0);
            }
        }

        private void numExposure2_ValueChanged(object sender, EventArgs e)
        {
            string nameTag = "AcquisitionSettings.2.ExposureTime";
            var tag = GetTag(nameTag);
            if (tag != null)
            {
                InsightSetValue(tag.Location, (float)numExposure2.Value);
            }
            MessageLoggerManager.Log.Info("[Action] Exposure: " + (float)numExposure2.Value);
            nameTag = "AcquisitionSettings.Selector";
            tag = GetTag(nameTag);
            if (tag != null)
            {
                InsightSetListBoxValue(tag.Location, 1);
            }
        }

        private void chxInspectionDistance_CheckedChanged_1(object sender, EventArgs e)
        {
            if (chxInspectionDistance.Checked)
            {

                MessageLoggerManager.Log.Info(String.Format("[Action] Inspection Distance: Checked"));
            }
            else
            {
                MessageLoggerManager.Log.Info(String.Format("[Action] Inspection Distance: Checked"));
            }
        }

        private void txtOrigin1X_TextChanged(object sender, EventArgs e)
        {
            MessageLoggerManager.Log.Info("[Action] X: " + txtOrigin1X.Text);
        }

        private void txtOrigin1Y_TextChanged(object sender, EventArgs e)
        {
            MessageLoggerManager.Log.Info("[Action] Y: " + txtOrigin1Y.Text);
        }

        private void txtOrigin2X_TextChanged(object sender, EventArgs e)
        {
            MessageLoggerManager.Log.Info("[Action] X 2: " + txtOrigin2X.Text);
        }

        private void txtOrigin2Y_TextChanged(object sender, EventArgs e)
        {
            MessageLoggerManager.Log.Info("[Action] Y 2: " + txtOrigin2Y.Text);
        }
    }
}

