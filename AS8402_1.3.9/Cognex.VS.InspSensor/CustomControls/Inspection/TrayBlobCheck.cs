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
    public partial class TrayBlobCheck : UserControl
    {
        public EventHandler SettingDoneEvent;
        private CvsInSightDisplay mCvsInsightDisplay = null;
        private string mFinderTypeName = "Blob";
        private int mBlobType = 0; //0 is black, 1 is white
        private int mBackgroundType = 0; //0 is black, 1 is white
        private int mBlobTypePanel = 0; //0 is black, 1 is white
        private int mBackgroundTypePanel = 0; //0 is black, 1 is white
        public TrayBlobCheck()
        {
            InitializeComponent();
            cbxBlobSearchType.SelectedIndex = 0;
        }

        public void Init(CvsInSightDisplay insightDisplay)
        {
            mCvsInsightDisplay = insightDisplay;
            if (insightDisplay.Connected)
            {
                UpdateGUIParams();
            }
        }

        private void UpdateGUIParams()
        {
            try
            {
                var tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}Enable", mFinderTypeName)));
                if (tag != null)
                {
                    bool isEnable = (bool)GetValue(tag.Location);
                    chxEnable.Checked = (bool)isEnable;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}Type", mFinderTypeName)));
                if (tag != null)
                {
                    int blobType = (int)GetValue(tag.Location);
                    mBlobType = (int)blobType;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}BackgroundType", mFinderTypeName)));
                if (tag != null)
                {
                    int backgroundType = (int)GetValue(tag.Location);
                    mBackgroundType = (int)backgroundType;
                }

                if (mBlobType == 0 && mBackgroundType == 1)
                {
                    cbxBlobSearchType.SelectedIndex = 0;
                }
                else if (mBlobType == 1 && mBackgroundType == 0)
                {
                    cbxBlobSearchType.SelectedIndex = 1;
                }
                else
                {
                    cbxBlobSearchType.SelectedIndex = -1;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}MaxNumber", mFinderTypeName)));
                if (tag != null)
                {
                    int num = (int)GetValue(tag.Location);
                    numBlobNumber.Value = (int)num;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}ContrastThreshold", mFinderTypeName)));
                if (tag != null)
                {
                    float threshold = (float)GetValue(tag.Location);
                    numContrastThreshold.Value = (decimal)threshold;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}MinSize", mFinderTypeName)));
                if (tag != null)
                {
                    int minSize = (int)GetValue(tag.Location);
                    numMinSize.Value = (int)minSize;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}MaxSize", mFinderTypeName)));
                if (tag != null)
                {
                    int maxSize = (int)GetValue(tag.Location);
                    numMaxSize.Value = (int)maxSize;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}ShowGraphicF", mFinderTypeName)));
                if (tag != null)
                {
                    int show = (int)GetValue(tag.Location);
                    numShowGraphicF.Value = (int)show;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}ShowGraphicR", mFinderTypeName)));
                if (tag != null)
                {
                    int show = (int)GetValue(tag.Location);
                    numShowGraphicR.Value = (int)show;
                }

                //Panel Blob

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}PanelEnable", mFinderTypeName)));
                if (tag != null)
                {
                    bool isEnable = (bool)GetValue(tag.Location);
                    chxEnablePanelBlob.Checked = (bool)isEnable;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}PanelType", mFinderTypeName)));
                if (tag != null)
                {
                    int blobType = (int)GetValue(tag.Location);
                    mBlobTypePanel = (int)blobType;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}PanelBackgroundType", mFinderTypeName)));
                if (tag != null)
                {
                    int backgroundTypePanel = (int)GetValue(tag.Location);
                    mBackgroundTypePanel = (int)backgroundTypePanel;
                }

                if (mBlobTypePanel == 0 && mBackgroundTypePanel == 1)
                {
                    cbxPanelBlobSearchType.SelectedIndex = 0;
                }
                else if (mBlobTypePanel == 1 && mBackgroundTypePanel == 0)
                {
                    cbxPanelBlobSearchType.SelectedIndex = 1;
                }
                else
                {
                    cbxBlobSearchType.SelectedIndex = -1;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}PanelMaxNumber", mFinderTypeName)));
                if (tag != null)
                {
                    int num = (int)GetValue(tag.Location);
                    numPanelBlobNumb.Value = (int)num;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}PanelContrastThreshold", mFinderTypeName)));
                if (tag != null)
                {
                    float threshold = (float)GetValue(tag.Location);
                    numPanelContrast.Value = (decimal)threshold;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}PanelMinSize", mFinderTypeName)));
                if (tag != null)
                {
                    int minSize = (int)GetValue(tag.Location);
                    numPanelMinSize.Value = (int)minSize;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}PanelMaxSize", mFinderTypeName)));
                if (tag != null)
                {
                    int maxSize = (int)GetValue(tag.Location);
                    numPanelMaxSize.Value = (int)maxSize;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}PanelShowGraphicF", mFinderTypeName)));
                if (tag != null)
                {
                    int show = (int)GetValue(tag.Location);
                    numPanelShowGraphicF.Value = (int)show;
                }

                tag = GetTag(ResourceUtility.GetString(string.Format("RtInsp{0}PanelShowGraphicR", mFinderTypeName)));
                if (tag != null)
                {
                    int show = (int)GetValue(tag.Location);
                    numPanelShowGraphicR.Value = (int)show;
                }

                tag = GetTag(ResourceUtility.GetString("RtInspPanelNumber"));
                if (tag != null)
                {
                    int number = (int)GetValue(tag.Location);

                    cbxPanelNumb.Items.Clear();
                    for (int i = 0; i < (int)number; ++i)
                    {
                        cbxPanelNumb.Items.Add(string.Format("Region{0}", i + 1));
                    }

                }
                cbxPanelNumb.SelectedIndex = 0;
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

        private void btnSearchRegion_Click(object sender, EventArgs e)
        {
            string nameTag = string.Format("RtInsp{0}SearchRegionF", mFinderTypeName);
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                EditCellGraphic(tag.Location);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Click Search Region F"));
        }

        private void btnSearchRegionR_Click(object sender, EventArgs e)
        {
            string nameTag = string.Format("RtInsp{0}SearchRegionR", mFinderTypeName);
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                EditCellGraphic(tag.Location);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Click Search RegionR"));
        }

        private void numBlobNumber_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}MaxNumber", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)numBlobNumber.Value);
                }

            }
            catch (Exception ex)
            {

            }

        }

        private void numContrastThreshold_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}ContrastThreshold", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numContrastThreshold.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Contrast Threshold: " + numContrastThreshold));
            }
            catch (Exception ex)
            {

            }
        }

        private void numMinSize_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}MinSize", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)numMinSize.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Min Size: " + numMinSize));
            }
            catch (Exception ex)
            {

            }
        }

        private void numMaxSize_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}MaxSize", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)numMaxSize.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Max Size: " + numMaxSize));
            }
            catch (Exception ex)
            {

            }
        }

        private void numShowGraphic_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}ShowGraphicF", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)numShowGraphicF.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Show GraphicF: " + (int)numShowGraphicF.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void numShowGraphicR_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}ShowGraphicR", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)numShowGraphicR.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Show GraphicR: " + (int)numShowGraphicR.Value));
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

        private void cbxBlobSearchType_SelectedIndexChanged(object sender, EventArgs e)
        {
            MessageLoggerManager.Log.Info(String.Format("[Action] Blob Search Type: " + cbxBlobSearchType.SelectedItem));
            try
            {

                if (cbxBlobSearchType.SelectedIndex == 0)
                {
                    string nameTag = string.Format("RtInsp{0}Type", mFinderTypeName);
                    var tag = GetTag(ResourceUtility.GetString(nameTag));
                    if (tag != null)
                    {
                        InsightSetValue(tag.Location, 0);
                    }

                    nameTag = string.Format("RtInsp{0}BackgroundType", mFinderTypeName);
                    tag = GetTag(ResourceUtility.GetString(nameTag));
                    if (tag != null)
                    {
                        InsightSetValue(tag.Location, 1);
                    }
                }
                else if (cbxBlobSearchType.SelectedIndex == 1)
                {
                    string nameTag = string.Format("RtInsp{0}Type", mFinderTypeName);
                    var tag = GetTag(ResourceUtility.GetString(nameTag));
                    if (tag != null)
                    {
                        InsightSetValue(tag.Location, 1);
                    }

                    nameTag = string.Format("RtInsp{0}BackgroundType", mFinderTypeName);
                    tag = GetTag(ResourceUtility.GetString(nameTag));
                    if (tag != null)
                    {
                        InsightSetValue(tag.Location, 0);
                    }
                }
                else
                {
                    throw new ArgumentException("Setup blob type and backgound type is invalid", "Warning");
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void btnPanelSearchRegionF_Click(object sender, EventArgs e)
        {
            string nameTag = string.Format("RtInsp{0}PanelSearchRegionF_{1}", mFinderTypeName, cbxPanelNumb.Text);
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                EditCellGraphic(tag.Location);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Click Panel Search RegionF"));
        }

        private void btnPanelSearchRegionR_Click(object sender, EventArgs e)
        {
            string nameTag = string.Format("RtInsp{0}PanelSearchRegionR_{1}", mFinderTypeName, cbxPanelNumb.Text);
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                EditCellGraphic(tag.Location);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Click Panel Search RegionR"));
        }

        private void numPanelBlobNumb_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}PanelMaxNumber", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)numPanelBlobNumb.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Panel Blob Number: " + (int)numPanelBlobNumb.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void numPanelContrast_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}PanelContrastThreshold", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numPanelContrast.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Panel Contrast: " + (int)numPanelContrast.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void numPanelMinSize_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}PanelMinSize", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)numPanelMinSize.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Panel Min Size: " + (int)numPanelMinSize.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void numPanelMaxSize_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}PanelMaxSize", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)numPanelMaxSize.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Panel Max Size: " + (int)numPanelMaxSize.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void chxEnablePanelBlob_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}PanelEnable", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, chxEnablePanelBlob.Checked);
                }
                if (chxEnable.Checked)
                {

                    MessageLoggerManager.Log.Info(String.Format("[Action] Enable Panel Blob: Checked"));
                }
                else
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] Enable Panel Blob: Unchecked"));
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void cbxPanelBlobSearchType_SelectedIndexChanged(object sender, EventArgs e)
        {
            MessageLoggerManager.Log.Info(String.Format("[Action] Panel Blob Search Type: "+ cbxPanelBlobSearchType.SelectedItem));
            try
            {

                if (cbxPanelBlobSearchType.SelectedIndex == 0)
                {
                    string nameTag = string.Format("RtInsp{0}PanelType", mFinderTypeName);
                    var tag = GetTag(ResourceUtility.GetString(nameTag));
                    if (tag != null)
                    {
                        InsightSetValue(tag.Location, 0);
                    }

                    nameTag = string.Format("RtInsp{0}PanelBackgroundType", mFinderTypeName);
                    tag = GetTag(ResourceUtility.GetString(nameTag));
                    if (tag != null)
                    {
                        InsightSetValue(tag.Location, 1);
                    }
                }
                else if (cbxPanelBlobSearchType.SelectedIndex == 1)
                {
                    string nameTag = string.Format("RtInsp{0}PanelType", mFinderTypeName);
                    var tag = GetTag(ResourceUtility.GetString(nameTag));
                    if (tag != null)
                    {
                        InsightSetValue(tag.Location, 1);
                    }

                    nameTag = string.Format("RtInsp{0}PanelBackgroundType", mFinderTypeName);
                    tag = GetTag(ResourceUtility.GetString(nameTag));
                    if (tag != null)
                    {
                        InsightSetValue(tag.Location, 0);
                    }
                }
                else
                {
                    throw new ArgumentException("Setup blob type and backgound type is invalid", "Warning");
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void numPanelShowGraphicF_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}PanelShowGraphicF", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)numPanelShowGraphicF.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Panel Show GraphicF: "+ (int)numPanelShowGraphicF.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void numPanelShowGraphicR_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = string.Format("RtInsp{0}PanelShowGraphicR", mFinderTypeName);
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)numPanelShowGraphicR.Value);
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Panel Show GraphicR: " + (int)numPanelShowGraphicR.Value));
            }
            catch (Exception ex)
            {

            }
        }

        private void btnPanelSearchRegionFProperty_Click(object sender, EventArgs e)
        {
            string sPanel = cbxPanelNumb.Text;
            string nameTag = string.Format("RtInsp{0}PanelSearchRegionF_{1}", mFinderTypeName, sPanel);
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                mCvsInsightDisplay.OpenPropertySheet(tag.Location, null);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Click Edit Property Panel Search RegionF"));
        }

        private void btnPanelSearchRegionRProperty_Click(object sender, EventArgs e)
        {
            string sPanel = cbxPanelNumb.Text;
            string nameTag = string.Format("RtInsp{0}PanelSearchRegionR_{1}", mFinderTypeName, sPanel);
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                mCvsInsightDisplay.OpenPropertySheet(tag.Location, null);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Click Panel Search Region Property R"));
        }

        private void btnSearchRegionTrayFProperty_Click(object sender, EventArgs e)
        {
            string nameTag = string.Format("RtInsp{0}SearchRegionF", mFinderTypeName);
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                mCvsInsightDisplay.OpenPropertySheet(tag.Location, null);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Eidt Property Search RegionF"));
        }
        private void btnSearchRegionTrayRProperty_Click(object sender, EventArgs e)
        {
            string nameTag = string.Format("RtInsp{0}SearchRegionR", mFinderTypeName);
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                mCvsInsightDisplay.OpenPropertySheet(tag.Location, null);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Eidt Property Search RegionR"));
        }

        private void cbxPanelNumb_SelectedIndexChanged(object sender, EventArgs e)
        {
            MessageLoggerManager.Log.Info(String.Format("[Action] Panel Number selected: " + cbxPanelNumb.SelectedItem));
        }
    }
}

