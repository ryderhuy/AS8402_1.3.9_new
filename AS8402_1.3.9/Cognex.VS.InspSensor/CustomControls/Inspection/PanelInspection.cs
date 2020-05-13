using Cognex.InSight;
using Cognex.InSight.Controls.Display;
using Cognex.VS.Utility;
using MessageManager;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Cognex.VS.InspSensor.CustomControls
{
    public partial class PanelInspection : UserControl
    {
        public EventHandler SettingDoneEvent;
        private CvsInSightDisplay mCvsInsightDisplay = null;
        private string mFinderTypeName = "Panel";
        bool isInit = false;
        public PanelInspection()
        {
            InitializeComponent();
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
                //enable multi pattern
                tag = GetTag("RtInspPanelEnableMultipattern");
                if (tag != null)
                {
                    bool isEnableMultiPatternF = (bool)GetValue(tag.Location);
                    chxEnableMultiPatternF.Checked = (bool)isEnableMultiPatternF;
                    chxEnableMultiPatternR.Checked = (bool)isEnableMultiPatternF;
                    cbxTrainRegionF.Enabled = (bool)isEnableMultiPatternF;
                    cbxTrainRegionR.Enabled = (bool)isEnableMultiPatternF;
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


                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}ShowgraphicR", mFinderTypeName)));
                if (tag != null)
                {
                    int showGraphicR = (int)GetValue(tag.Location);
                    numShowGraphicR.Value = (int)showGraphicR;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}VisEnable", mFinderTypeName)));
                if (tag != null)
                {
                    bool isEnableVis = (bool)GetValue(tag.Location);
                    chxVisEnable.Checked = (bool)isEnableVis;
                }


                /*
                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}VisMeanThresh", mFinderTypeName)));
                if (tag != null)
                {
                    float visMean = (float)GetValue(tag.Location);
                    numVisMeanThresh.Value = (decimal)visMean;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}VisCountThresh", mFinderTypeName)));
                if (tag != null)
                {
                    float visCount = (float)GetValue(tag.Location);
                    numVisCountThresh.Value = (decimal)visCount;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}VisFirstBin", mFinderTypeName)));
                if (tag != null)
                {
                    float firstBin = (float)GetValue(tag.Location);
                    numVisFirstBin.Value = (decimal)firstBin;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}VisLastBin", mFinderTypeName)));
                if (tag != null)
                {
                    float lastBin = (float)GetValue(tag.Location);
                    numVisLastBin.Value = (decimal)lastBin;
                }
                */
                tag = GetTag(ResourceUtility.GetString(string.Format("RtInspVisEnable", mFinderTypeName)));
                if (tag != null)
                {
                    bool isEnable = (bool)GetValue(tag.Location);
                    chxVisEnable.Checked = (bool)isEnable;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}Number", mFinderTypeName)));
                if (tag != null)
                {
                    int number = (int)GetValue(tag.Location);
                    numNumberPocket.Value = (int)number;

                    cbxSearchRegionF.Items.Clear();
                    cbxSearchRegionR.Items.Clear();
                    cbxTrainRegionF.Items.Clear();
                    cbxTrainRegionR.Items.Clear();
                    cbxPanelParams.Items.Clear();
                    cbxVisSearchRegion.Items.Clear();
                    for (int i = 0; i < (int)numNumberPocket.Value; ++i)
                    {
                        cbxSearchRegionF.Items.Add(string.Format("Region{0}", i + 1));
                        cbxSearchRegionR.Items.Add(string.Format("Region{0}", i + 1));
                        cbxTrainRegionF.Items.Add(string.Format("Region{0}", i + 1));
                        cbxTrainRegionR.Items.Add(string.Format("Region{0}", i + 1));
                        cbxPanelParams.Items.Add(string.Format("Region{0}", i + 1));
                        cbxVisSearchRegion.Items.Add(string.Format("Region{0}", i + 1));
                    }

                }


                //tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}MaxDeltaX", mFinderTypeName)));
                //if (tag != null)
                //{
                //    float maxDeltaX = (float)GetValue(tag.Location);
                //    numMaxDeltaX.Value = (decimal)maxDeltaX;
                //}

                //tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}MaxDeltaY", mFinderTypeName)));
                //if (tag != null)
                //{
                //    float maxDeltaY = (float)GetValue(tag.Location);
                //    numMaxDeltaY.Value = (decimal)maxDeltaY;
                //}

                //tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}MaxDeltaTheta", mFinderTypeName)));
                //if (tag != null)
                //{
                //    float maxDeltaTheta = (float)GetValue(tag.Location);
                //    numMaxDeltaTheta.Value = (decimal)maxDeltaTheta;
                //}

                cbxSearchRegionF.SelectedIndex = 0;
                cbxSearchRegionR.SelectedIndex = 0;
                cbxTrainRegionF.SelectedIndex = 0;
                cbxTrainRegionR.SelectedIndex = 0;
                cbxPanelParams.SelectedIndex = 0;
                cbxVisSearchRegion.SelectedIndex = 0;
                cbxVisType.SelectedIndex = 0;

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}Vis{1}_Showgraphic", mFinderTypeName, cbxVisType.Text)));
                if (tag != null)
                {
                    float showgraphic = (float)GetValue(tag.Location);
                    numVisShowGraphic.Value = (decimal)showgraphic;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}MaxDeltaX_{1}", mFinderTypeName, cbxPanelParams.Text)));
                if (tag != null)
                {
                    float maxDeltaX = (float)GetValue(tag.Location);
                    numMaxDeltaX.Value = (decimal)maxDeltaX;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}MaxDeltaY_{1}", mFinderTypeName, cbxPanelParams.Text)));
                if (tag != null)
                {
                    float maxDeltaY = (float)GetValue(tag.Location);
                    numMaxDeltaY.Value = (decimal)maxDeltaY;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}MaxDeltaTheta_{1}", mFinderTypeName, cbxPanelParams.Text)));
                if (tag != null)
                {
                    float maxDeltaTheta = (float)GetValue(tag.Location);
                    numMaxDeltaTheta.Value = (decimal)maxDeltaTheta;
                }


                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}ScoreThresholdF_{1}", mFinderTypeName, cbxSearchRegionF.Text)));
                if (tag != null)
                {
                    float scoreThresholdF = (float)GetValue(tag.Location);
                    numScoreThresholdF.Value = (decimal)scoreThresholdF;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}ScoreThresholdR_{1}", mFinderTypeName, cbxSearchRegionR.Text)));
                if (tag != null)
                {
                    float scoreThresholdR = (float)GetValue(tag.Location);
                    numScoreThresholdR.Value = (decimal)scoreThresholdR;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInspPaneDirectionEnable")));
                if (tag != null)
                {
                    bool isEnablePanelInsp = (bool)GetValue(tag.Location);
                    chxPanelDirectionEnable.Checked = (bool)isEnablePanelInsp;
                }

                string sNumPanel = cbxVisSearchRegion.Text;
                UpdateGUIParamsVisEachRegion(sNumPanel);


                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}CheckShiftedType", mFinderTypeName)));
                if (tag != null)
                {
                    int type = (int)GetValue(tag.Location);
                    cbxCheckShiftType.SelectedIndex = (int)type;
                }

                tag = GetTag("Inspection.1.PanelInspection.CheckPresentType");
                if (tag != null)
                {
                    int type = (int)GetValue(tag.Location);
                    cbxPreAbsType.SelectedIndex = (int)type;
                }

                //Update for Line check
                cbxLineSearchRegion.SelectedIndex = 0;
                cbxLineRegionType.SelectedIndex = 0;
                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}LineEnable", mFinderTypeName)));
                if (tag != null)
                {
                    bool isEnable = (bool)GetValue(tag.Location);
                    chxEnableLine.Checked = (bool)isEnable;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}LineShowGraphic", mFinderTypeName)));
                if (tag != null)
                {
                    int showGraphic = (int)GetValue(tag.Location);
                    numLineShowGraphic.Value = (int)showGraphic;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}LinePolarity", mFinderTypeName)));
                if (tag != null)
                {
                    int polar = (int)GetValue(tag.Location);
                    cbxLinePolarity.SelectedIndex = (int)polar;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}LineFindType", mFinderTypeName)));
                if (tag != null)
                {
                    int find = (int)GetValue(tag.Location);
                    cbxLineFindBy.SelectedIndex = (int)find;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}LineWidth", mFinderTypeName)));
                if (tag != null)
                {
                    int width = (int)GetValue(tag.Location);
                    numEdgeWidth.Value = (int)width;
                }

                string sLineRegion = cbxLineSearchRegion.Text;
                UpdateGUIParamsLine(sLineRegion);


            }
            catch (Exception ex)
            {
                MessageManager.MessageLoggerManager.Log.Warn("UpdateGUIParams: " + ex.Message);
            }
        }

        private void UpdateGUIParamsLine(string sLineRegion)
        {
            string lineType = "";

            var tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}LineThresh{1}_{2}", mFinderTypeName, lineType, sLineRegion)));
            if (tag != null)
            {
                int thresh = (int)GetValue(tag.Location);
                numLineThreshold.Value = (int)thresh;
            }
            tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}LineLimit{1}_{2}", mFinderTypeName, lineType, sLineRegion)));
            if (tag != null)
            {
                float limit = (float)GetValue(tag.Location);
                numLineLitmitAngle.Value = (decimal)limit;
            }

            lineType = "Org";
            tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}LineLimit{1}_{2}", mFinderTypeName, lineType, sLineRegion)));
            if (tag != null)
            {
                float limit = (float)GetValue(tag.Location);
                numLineLimitDis.Value = (decimal)limit;
            }
        }

        private void UpdateGUIParamsVisEachRegion(string snumPanel)
        {
            //string snumPanel = cbxVisSearchRegion.Text;

            var tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}VisMeanThresh_{1}", mFinderTypeName, snumPanel)));
            if (tag != null)
            {
                float visMean = (float)GetValue(tag.Location);
                numVisMeanThresh.Value = (decimal)visMean;
            }

            tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}VisCountThresh_{1}", mFinderTypeName, snumPanel)));
            if (tag != null)
            {
                float visCount = (float)GetValue(tag.Location);
                numVisCountThresh.Value = (decimal)visCount;
            }

            tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}VisFirstBin_{1}", mFinderTypeName, snumPanel)));
            if (tag != null)
            {
                float firstBin = (float)GetValue(tag.Location);
                numVisFirstBin.Value = (decimal)firstBin;
            }

            tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}VisLastBin_{1}", mFinderTypeName, snumPanel)));
            if (tag != null)
            {
                float lastBin = (float)GetValue(tag.Location);
                numVisLastBin.Value = (decimal)lastBin;
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
                if (!chxEnableF.Checked || !chxEnableR.Checked)
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
            MessageLoggerManager.Log.Info(String.Format("[Action] Click Train Region F"));
        }

        private void btnTrainRegionR_Click(object sender, EventArgs e)
        {
            string nameTag = string.Format("RtInsp{0}TrainRegionR", mFinderTypeName);
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                EditCellGraphic(tag.Location);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Click Train Region R"));
        }

        private void btnTrainGoldenR_Click(object sender, EventArgs e)
        {
            string nameTag = string.Format("RtInsp{0}TrainGoldenButtonR", mFinderTypeName);
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                InsightClickButton(tag.Location);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Click Train Golden R"));
        }

        private void btnTrainGoldenF_Click(object sender, EventArgs e)
        {
            string nameTag = string.Format("RtInsp{0}TrainGoldenButtonF", mFinderTypeName);
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                InsightClickButton(tag.Location);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Click Train Golden F"));
        }

        private void numNumberPocketF_ValueChanged(object sender, EventArgs e)
        {
            cbxSearchRegionF.Items.Clear();
            cbxSearchRegionR.Items.Clear();
            cbxTrainRegionR.Items.Clear();
            cbxTrainRegionF.Items.Clear();
            cbxPanelParams.Items.Clear();
            cbxVisSearchRegion.Items.Clear();
            cbxLineSearchRegion.Items.Clear();
            for (int i = 0; i < (int)numNumberPocket.Value; ++i)
            {
                cbxSearchRegionF.Items.Add(string.Format("Region{0}", i + 1));
                cbxSearchRegionR.Items.Add(string.Format("Region{0}", i + 1));
                cbxTrainRegionR.Items.Add(string.Format("Region{0}", i + 1));
                cbxTrainRegionF.Items.Add(string.Format("Region{0}", i + 1));
                cbxPanelParams.Items.Add(string.Format("Region{0}", i + 1));
                cbxVisSearchRegion.Items.Add(string.Format("Region{0}", i + 1));
                cbxLineSearchRegion.Items.Add(string.Format("Region{0}", i + 1));
            }

            cbxSearchRegionF.SelectedIndex = 0;
            cbxSearchRegionR.SelectedIndex = 0;
            cbxTrainRegionR.SelectedIndex = 0;
            cbxTrainRegionF.SelectedIndex = 0;
            cbxPanelParams.SelectedIndex = 0;
            cbxVisSearchRegion.SelectedIndex = 0;
            cbxLineSearchRegion.SelectedIndex = 0;

            string nameTag = string.Format("RtInsp{0}Number", mFinderTypeName);
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                InsightSetValue(tag.Location, (int)numNumberPocket.Value);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Number Pocket F: " + (int)numNumberPocket.Value));
            //this.UpdateGUIParams();
        }

        private void btnSearchRegionF_Click(object sender, EventArgs e)
        {
            try
            {
                string sNumPanel = cbxSearchRegionF.Text;
                string str = "RtInspPanelSearchRegionF_" + sNumPanel;
                var tag = GetTag(ResourceUtility.GetString(str));
                if (tag != null)
                {
                    EditCellGraphic(tag.Location);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Click Search Region F"));
            }
            catch (CvsException ex)
            {

            }
        }

        private void btnSearchRegionR_Click(object sender, EventArgs e)
        {
            try
            {
                string sNumPanel = cbxSearchRegionR.Text;
                string str = "RtInspPanelSearchRegionR_" + sNumPanel;
                var tag = GetTag(ResourceUtility.GetString(str));
                if (tag != null)
                {
                    EditCellGraphic(tag.Location);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Click Search Region R"));
            }
            catch (CvsException ex)
            {

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
            MessageLoggerManager.Log.Info(String.Format("[Action] Click Train Button F"));
        }

        private void btnTrainPMR_Click(object sender, EventArgs e)
        {
            string nameTag = string.Format("RtInsp{0}TrainButtonR", mFinderTypeName);
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                InsightClickButton(tag.Location);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Click TrainPM R"));
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
                MessageLoggerManager.Log.Info(String.Format("[Action] Contrast Threshold F: " + (float)numContrastThresholdF.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void numContrastThresholdR_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}ContrastThresholdR", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numContrastThresholdR.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Contrast Threshold R: " + (float)numContrastThresholdR.Value));

            }
            catch (Exception ex)
            {

            }
        }

        private void numMaxDeltaX_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                //RtInspPanelMaxDeltaXRegion1
                string nameTag = string.Format("RtInsp{0}MaxDeltaX_{1}", mFinderTypeName, cbxPanelParams.Text);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numMaxDeltaX.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Max Delta X: " + (float)numMaxDeltaX.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void numMaxDeltaY_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}MaxDeltaY_{1}", mFinderTypeName, cbxPanelParams.Text);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numMaxDeltaY.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Max Delta Y: " + (float)numMaxDeltaY.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void numMaxDeltaTheta_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}MaxDeltaTheta_{1}", mFinderTypeName, cbxPanelParams.Text);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numMaxDeltaTheta.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Max Delta Theta: " + (float)numMaxDeltaTheta.Value));
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
                MessageLoggerManager.Log.Info(String.Format("[Action] Angle Range F: " + (float)numAngleRangeF.Value));

            }
            catch (Exception ex)
            {

            }
        }

        private void numShowGraphicF_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}ShowgraphicF", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)numShowGraphicF.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Show Graphic F: " + (float)numShowGraphicF.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void chxEnableF_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}EnableF", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, chxEnableF.Checked);
                }
                if (chxEnableF.Checked)
                {

                    MessageLoggerManager.Log.Info(String.Format("[Action] Enable F: Checked"));
                }
                else
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] Enable F: Unchecked"));
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
                //Forward
                string nameTag = string.Format("RtInsp{0}AngleRangeR", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numAngleRangeR.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Angle Range R: " + (float)numAngleRangeR.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void numShowGraphicR_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}ShowgraphicR", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)numShowGraphicR.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Show Graphic R: " + (int)numShowGraphicR.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void chxEnableR_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}EnableR", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, chxEnableR.Checked);
                }
                if (chxEnableR.Checked)
                {

                    MessageLoggerManager.Log.Info(String.Format("[Action] Enable R: Checked"));
                }
                else
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] Enable R: Unchecked"));
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void cbxSearchRegionF_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxSearchRegionF.Focused)
            {
                try
                {
                    var tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}ScoreThresholdF_{1}", mFinderTypeName, cbxSearchRegionF.Text)));
                    if (tag != null)
                    {
                        float scoreThresholdF = (float)GetValue(tag.Location);
                        numScoreThresholdF.Value = (decimal)scoreThresholdF;
                    }

                    string sNumPanel = cbxSearchRegionF.Text;
                    string str = "RtInspPanelSearchRegionF_" + sNumPanel;
                    tag = GetTag(ResourceUtility.GetString(str));
                    if (tag != null)
                    {
                        EditCellGraphic(tag.Location);
                    }
                    MessageLoggerManager.Log.Info(String.Format("[Action] Search Region F: "+ cbxSearchRegionF.SelectedItem));
                }
                catch (CvsException ex)
                {

                }
            }
        }

        private void cbxSearchRegionR_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxSearchRegionR.Focused)
            {
                try
                {
                    var tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}ScoreThresholdR_{1}", mFinderTypeName, cbxSearchRegionR.Text)));
                    if (tag != null)
                    {
                        float scoreThresholdR = (float)GetValue(tag.Location);
                        numScoreThresholdR.Value = (decimal)scoreThresholdR;
                    }

                    string sNumPanel = cbxSearchRegionR.Text;
                    string str = "RtInspPanelSearchRegionR_" + sNumPanel;
                    tag = GetTag(ResourceUtility.GetString(str));
                    if (tag != null)
                    {
                        EditCellGraphic(tag.Location);
                    }
                    MessageLoggerManager.Log.Info(String.Format("[Action] Search Region R: " + cbxSearchRegionR.SelectedItem));
                }
                catch (CvsException ex)
                {

                }
            }
        }

        private void cbxPanelParams_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}MaxDeltaX_{1}", mFinderTypeName, cbxPanelParams.Text)));
                if (tag != null)
                {
                    float maxDeltaX = (float)GetValue(tag.Location);
                    numMaxDeltaX.Value = (decimal)maxDeltaX;
                    MessageLoggerManager.Log.Info(String.Format("[Action] Max Delta X: " + numMaxDeltaX.Value));
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}MaxDeltaY_{1}", mFinderTypeName, cbxPanelParams.Text)));
                if (tag != null)
                {
                    float maxDeltaY = (float)GetValue(tag.Location);
                    numMaxDeltaY.Value = (decimal)maxDeltaY;
                    MessageLoggerManager.Log.Info(String.Format("[Action] Max Delta Y: " + numMaxDeltaY.Value));
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}MaxDeltaTheta_{1}", mFinderTypeName, cbxPanelParams.Text)));
                if (tag != null)
                {
                    float maxDeltaTheta = (float)GetValue(tag.Location);
                    numMaxDeltaTheta.Value = (decimal)maxDeltaTheta;
                    MessageLoggerManager.Log.Info(String.Format("[Action] Max Delta Theta: " + numMaxDeltaTheta.Value));
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Panel Params: " + cbxPanelParams.SelectedItem));
            }
            catch (Exception ex)
            {

            }
        }

        private void cbxVisSearchRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxVisSearchRegion.Focused)
            {
                try
                {
                    string sNumPanel = cbxVisSearchRegion.Text;

                    UpdateGUIParamsVisEachRegion(sNumPanel);

                    string str = string.Format("RtInspVisCheckRegion{0}_{1}", cbxVisType.Text, sNumPanel);
                    var tag = GetTag(ResourceUtility.GetString(str));
                    if (tag != null)
                    {
                        EditCellGraphic(tag.Location);
                    }
                    MessageLoggerManager.Log.Info(String.Format("[Action] Search Region: " + cbxVisSearchRegion.SelectedItem));
                }
                catch (CvsException ex)
                {

                }
            }
        }

        private void numVisMeanThresh_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                //Forward
                string sNumPanel = cbxVisSearchRegion.Text;
                string nameTag = string.Format("RtInsp{0}VisMeanThresh_{1}", mFinderTypeName, sNumPanel);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numVisMeanThresh.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Vis Mean Thresh: " + (float)numVisMeanThresh.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void numVisCountThresh_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                //Forward
                string sNumPanel = cbxVisSearchRegion.Text;
                string nameTag = string.Format("RtInsp{0}VisCountThresh_{1}", mFinderTypeName, sNumPanel);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numVisCountThresh.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Vis Count Thresh: " + (float)numVisCountThresh.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void numVisFirstBin_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                //Forward
                string sNumPanel = cbxVisSearchRegion.Text;
                string nameTag = string.Format("RtInsp{0}VisFirstBin_{1}", mFinderTypeName, sNumPanel);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numVisFirstBin.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Vis First Bin: " + (float)numVisFirstBin.Value));

            }
            catch (Exception ex)
            {

            }
        }

        private void numVisLastBin_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                //Forward
                string sNumPanel = cbxVisSearchRegion.Text;
                string nameTag = string.Format("RtInsp{0}VisLastBin_{1}", mFinderTypeName, sNumPanel);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numVisLastBin.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Vis Last Bin: " + (float)numVisLastBin.Value));

            }
            catch (Exception ex)
            {

            }
        }

        private void btnVisSearchRegion_Click(object sender, EventArgs e)
        {
            try
            {
                string sNumPanel = cbxSearchRegionR.Text;
                string str = string.Format("RtInspVisCheckRegion{0}_{1}", cbxVisType.Text, sNumPanel);
                var tag = GetTag(ResourceUtility.GetString(str));
                if (tag != null)
                {
                    EditCellGraphic(tag.Location);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Click Vis Search Region"));
            }
            catch (CvsException ex)
            {

            }

        }

        private void numScoreThresholdF_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                //Forward
                string nameTag = string.Format("RtInsp{0}ScoreThresholdF_{1}", mFinderTypeName, cbxSearchRegionF.Text);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numScoreThresholdF.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Score Threshold F: " + (float)numScoreThresholdF.Value));

            }
            catch (Exception ex)
            {

            }
        }

        private void numScoreThresholdR_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                //Forward
                string nameTag = string.Format("RtInsp{0}ScoreThresholdR_{1}", mFinderTypeName, cbxSearchRegionR.Text);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numScoreThresholdR.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Score Threshold R: " + (float)numScoreThresholdR.Value));

            }
            catch (Exception ex)
            {

            }
        }

        private void chxVisEnable_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInspVisEnable");
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, chxVisEnable.Checked);
                }
                if (chxVisEnable.Checked)
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

        private void btnVisResetAllRegion_Click(object sender, EventArgs e)
        {
            string sNumPanel = cbxVisSearchRegion.Text;
            string str = string.Format("RtInspVisCheckRegion{0}_{1}", cbxVisType.Text, sNumPanel);
            var tag = GetTag(ResourceUtility.GetString(str));
            if (tag != null)
            {
                mCvsInsightDisplay.OpenPropertySheet(tag.Location, null);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Click button Reset All Region"));
        }

        private void cbxVisType_SelectedIndexChanged(object sender, EventArgs e)
        {
            string sVisType = cbxVisType.Text;
            string nameTag = string.Format("RtInsp{0}Vis{1}_Showgraphic", mFinderTypeName, sVisType);
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                float showgraphic = (float)GetValue(tag.Location);
                numVisShowGraphic.Value = (decimal)showgraphic;
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Fwd/Rev: " + cbxVisType.SelectedItem));
        }

        private void numVisShowGraphic_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                //Forward
                string sVisType = cbxVisType.Text;
                string nameTag = string.Format("RtInsp{0}Vis{1}_Showgraphic", mFinderTypeName, sVisType);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numVisShowGraphic.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Vis Show Graphic: " + (float)numVisShowGraphic.Value));

            }
            catch (Exception ex)
            {

            }
        }

        private void numNumberPocket_Validating(object sender, CancelEventArgs e)
        {

        }

        private void chxPanelDirectionEnable_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInspPaneDirectionEnable");
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, chxPanelDirectionEnable.Checked);
                }
                if (chxPanelDirectionEnable.Checked)
                {

                    MessageLoggerManager.Log.Info(String.Format("[Action] Panel Direction Enable : Checked"));
                }
                else
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] Panel Direction Enable: Unchecked"));
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void cbxCheckShiftType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}CheckShiftedType", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)cbxCheckShiftType.SelectedIndex);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Check Shift Type: " + cbxCheckShiftType.SelectedItem));
            }
            catch (Exception ex)
            {

            }
        }

        private void cbxLineSearchRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxLineSearchRegion.Focused)
            {
                try
                {
                    string sLinePanel = cbxLineSearchRegion.Text;

                    UpdateGUIParamsLine(sLinePanel);

                    string str = string.Format("RtInspLineCheckRegion{0}_{1}", cbxLineRegionType.Text, sLinePanel);
                    var tag = GetTag(ResourceUtility.GetString(str));
                    if (tag != null)
                    {
                        EditCellGraphic(tag.Location);
                    }
                    MessageLoggerManager.Log.Info(String.Format("[Action] Line Search Region: " + cbxLineSearchRegion.SelectedItem));
                }
                catch (CvsException ex)
                {

                }
            }
        }

        private void btnLineSearchRegion_Click(object sender, EventArgs e)
        {
            try
            {
                string sNumPanel = cbxLineSearchRegion.Text;
                string str = string.Format("RtInspLineCheckRegion{0}_{1}", cbxLineRegionType.Text, sNumPanel);
                var tag = GetTag(ResourceUtility.GetString(str));
                if (tag != null)
                {
                    EditCellGraphic(tag.Location);
                }
                //write log
                MessageLoggerManager.Log.Info(String.Format("[Action] Click Line Search Region"));
            }
            catch (CvsException ex)
            {

            }
        }

        private void btnLineResetAllRegion_Click(object sender, EventArgs e)
        {
            string sNumPanel = cbxLineSearchRegion.Text;
            string str = string.Format("RtInspLineCheckRegion{0}_{1}", cbxLineRegionType.Text, sNumPanel);
            var tag = GetTag(ResourceUtility.GetString(str));
            if (tag != null)
            {
                mCvsInsightDisplay.OpenPropertySheet(tag.Location, null);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Click Line Reset All Region"));
        }

        private void cbxLinePolarity_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}LinePolarity", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)cbxLinePolarity.SelectedIndex);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Line Polarity: " + cbxLinePolarity.SelectedItem));
            }
            catch (Exception ex)
            {

            }
        }

        private void cbxLineFindBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}LineFindType", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)cbxLineFindBy.SelectedIndex);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Line Find By: " + cbxLineFindBy.SelectedItem));
            }
            catch (Exception ex)
            {

            }
        }

        private void numLineShowGraphic_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}LineShowGraphic", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)numLineShowGraphic.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Line Show Graphic: " + (int)numLineShowGraphic.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void numLineThreshold_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string lineType = "";
                if (cbxLineRegionType.Text.Contains("Org"))
                {
                    lineType = "Org";
                }
                string sNumPanel = cbxLineSearchRegion.Text;
                string nameTag = string.Format("RtInsp{0}LineThresh{1}_{2}", mFinderTypeName, lineType, sNumPanel);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)numLineThreshold.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Line Threshold: " + (int)numLineThreshold.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void numLineLitmitAngle_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string lineType = "";
                string sNumPanel = cbxLineSearchRegion.Text;
                string nameTag = string.Format("RtInsp{0}LineLimit{1}_{2}", mFinderTypeName, lineType, sNumPanel);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numLineLitmitAngle.Value);
                }

                lineType = "Org";
                nameTag = string.Format("RtInsp{0}LineLimit{1}_{2}", mFinderTypeName, lineType, sNumPanel);
                tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numLineLimitDis.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Line Limit Angle: " + (int)numLineLitmitAngle.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void btnLineTrainGolden_Click(object sender, EventArgs e)
        {
            string lineType = "Forward";
            if (cbxLineRegionType.Text.Contains("Reverse"))
            {
                lineType = "Reverse";
            }
            string nameTag = string.Format("RtInsp{0}LineTrainGolden_{1}", mFinderTypeName, lineType);
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                InsightClickButton(tag.Location);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Click Line Train Golden"));
        }

        private void cbxLineRegionType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isInit)
            {
                try
                {
                    string sLinePanel = cbxLineSearchRegion.Text;
                    UpdateGUIParamsLine(sLinePanel);
                    MessageLoggerManager.Log.Info(String.Format("[Action] Line Region Type: " + cbxLineRegionType.SelectedItem));
                }
                catch (CvsException ex)
                {

                }
            }
        }

        private void numEdgeWidth_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}LineWidth", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)numEdgeWidth.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Edge Width: " + (int)numEdgeWidth.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void cbxPreAbsType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("Inspection.1.PanelInspection.CheckPresentType");
                var tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)cbxPreAbsType.SelectedIndex);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Find type: "+ cbxPreAbsType.SelectedItem));
            }
            catch (Exception ex)
            {

            }
        }

        private void chxEnableLine_CheckedChanged(object sender, EventArgs e)
        {
            if (chxEnableLine.Checked)
            {

                MessageLoggerManager.Log.Info(String.Format("[Action] Enable Line: Checked"));
            }
            else
            {
                MessageLoggerManager.Log.Info(String.Format("[Action] Enable Line: Unchecked"));
            }
        }

        private void cbxTrainRegionF_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var tag = GetTag("RtInspPanelShiftTRegionFWIndex");
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)cbxTrainRegionF.SelectedIndex + 1);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Train Region F: " + cbxTrainRegionF.SelectedItem));
            }
            catch (Exception ex)
            {

            }
        }

        private void chxEnableMultiPatternF_CheckedChanged(object sender, EventArgs e)
        {
                try
            {
                var tag = GetTag("RtInspPanelEnableMultipattern");
                if (tag != null)
                {
                    InsightSetValue(tag.Location, chxEnableMultiPatternF.Checked);
                    chxEnableMultiPatternR.Checked = chxEnableMultiPatternF.Checked;
                    cbxTrainRegionF.Enabled = chxEnableMultiPatternF.Checked;
                    cbxTrainRegionR.Enabled = chxEnableMultiPatternF.Checked;
                }
                if (chxEnableMultiPatternF.Checked)
                {

                    MessageLoggerManager.Log.Info(String.Format("[Action] Enable Multi Pattern F: Checked"));
                }
                else
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] Enable Multi Pattern F: Unchecked"));
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void chxEnableMultiPatternR_CheckedChanged(object sender, EventArgs e)
        {
            chxEnableMultiPatternF.Checked = chxEnableMultiPatternR.Checked;
        }

        private void cbxTrainRegionR_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var tag = GetTag("RtInspPanelShiftTRegionRVIndex");
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)cbxTrainRegionR.SelectedIndex+1);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Train Region R: " + cbxTrainRegionR.SelectedItem));
            }
            catch (Exception ex)
            {

            }
        }

        private void chxUnlinkFixture_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                var tag = GetTag("Inspection.1.PanelInspection.PanelSearchRegion_Unlink");
                if (tag != null)
                {
                    InsightSetValue(tag.Location, chxUnlinkFixture.Checked);
                    chxUnlinkFixtureR.Checked = chxUnlinkFixture.Checked;
                }
                if (chxEnableMultiPatternF.Checked)
                {

                    MessageLoggerManager.Log.Info(String.Format("[Action] Enable Multi Pattern F: Checked"));
                }
                else
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] Enable Multi Pattern F: Unchecked"));
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void chxUnlinkFixtureR_CheckedChanged(object sender, EventArgs e)
        {
            chxUnlinkFixture.Checked = chxUnlinkFixtureR.Checked;
        }
    }
}
