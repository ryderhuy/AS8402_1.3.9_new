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
    public partial class DisplayControl : UserControl
    {
        public EventHandler SettingDoneEvent;
        private CvsInSightDisplay mCvsInsightDisplay = null;
        private string mFinderTypeName = "Conveyor";
        public DisplayControl()
        {
            InitializeComponent();
        }

        public void Init(CvsInSightDisplay insightDisplay)
        {
            mCvsInsightDisplay = insightDisplay;
            if (insightDisplay.Connected)
            {
                UpdateGUIParams();
            }
        }
        public CheckBox getChx8Pocket()
        {
            return this.chx8Pocket;
        }
        public CheckBox getChxPanelInspection()
        {
            return this.chxPanelEnable;
        }
        private void UpdateGUIParams()
        {
            try
            {
                var tag = GetTag(ResourceUtility.GetString("RtInspTrayDisplayEnable"));
                if (tag != null)
                {
                    bool enable = (bool)GetValue(tag.Location);
                    chxTrayEnable.Checked = (bool)enable;
                }

                tag = GetTag(ResourceUtility.GetString("RtInspPanelDisplayEnable"));
                if (tag != null)
                {
                    bool enable = (bool)GetValue(tag.Location);
                    chxPanelEnable.Checked = (bool)enable;
                }

                tag = GetTag(ResourceUtility.GetString("RtInspBlobDisplayEnable"));
                if (tag != null)
                {
                    bool enable = (bool)GetValue(tag.Location);
                    chxBlobEnable.Checked = (bool)enable;
                }

                tag = GetTag(ResourceUtility.GetString("RtInspBlobPanelDisplayEnable"));
                if (tag != null)
                {
                    bool enable = (bool)GetValue(tag.Location);
                    chxBlobPanelEnable.Checked = (bool)enable;
                }

                tag = GetTag(ResourceUtility.GetString("RtInspConveyorDisplayEnable"));
                if (tag != null)
                {
                    bool enable = (bool)GetValue(tag.Location);
                    chxConveyorEnable.Checked = (bool)enable;
                }

                tag = GetTag(ResourceUtility.GetString("RtInspPanelAlignDisplayEnable"));
                if (tag != null)
                {
                    bool enable = (bool)GetValue(tag.Location);
                    chkPanelAlignEnable.Checked = (bool)enable;
                }

                tag = GetTag(ResourceUtility.GetString("RtInspResultGraphicDisplayEnable"));
                if (tag != null)
                {
                    bool enable = (bool)GetValue(tag.Location);
                    chxResultGraphic.Checked = (bool)enable;
                }

                tag = GetTag("Inspection.1.PlotControl.ResultCommand");
                if (tag != null)
                {
                    bool enable = (bool)GetValue(tag.Location);
                    chxReturnResult.Checked = (bool)enable;
                }
                tag = GetTag("RtInspTrayInspection8Pocket");
                if (tag != null)
                {
                    bool enable = (bool)GetValue(tag.Location);
                    chx8Pocket.Checked = (bool)enable;
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

        private void btnAccept_Click(object sender, EventArgs e)
        {
            try
            {
                SettingDoneEvent.BeginInvoke(null, null, null, null);
            }
            catch (CvsException ex)
            {

            }
        }

        private void chxTrayEnable_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                string nameTag = "RtInspTrayDisplayEnable";
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (bool)chxTrayEnable.Checked);
                }
                if (chxTrayEnable.Checked)
                {

                    MessageLoggerManager.Log.Info(String.Format("[Action] Tray Display Enable: Checked"));
                }
                else
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] Tray Display Enable: Unchecked"));
                }
                nameTag = "RtInspPanelDisplayEnable";
                tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (bool)chxPanelEnable.Checked);
                }
                if (chxPanelEnable.Checked)
                {

                    MessageLoggerManager.Log.Info(String.Format("[Action] Panel Display Enable: Checked"));
                }
                else
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] Panel Display Enable: Unchecked"));
                }
                nameTag = "RtInspBlobDisplayEnable";
                tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (bool)chxBlobEnable.Checked);
                } 
                if (chxBlobEnable.Checked)
                {

                    MessageLoggerManager.Log.Info(String.Format("[Action] Inspection Blob Enable: Checked"));
                }
                else
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] Inspection Blob Enable: Unchecked"));
                }

                nameTag = "RtInspBlobPanelDisplayEnable";
                tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (bool)chxBlobPanelEnable.Checked);
                }
                if (chxBlobPanelEnable.Checked)
                {

                    MessageLoggerManager.Log.Info(String.Format("[Action] Inspection Blob Panel Enable: Checked"));
                }
                else
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] Inspection Blob Panel Enable: Unchecked"));
                }
                nameTag = "RtInspConveyorDisplayEnable";
                tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (bool)chxConveyorEnable.Checked);
                }
                if (chxConveyorEnable.Checked)
                {

                    MessageLoggerManager.Log.Info(String.Format("[Action] Conveyor Display Enable: Checked"));
                }
                else
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] Conveyor Display Enable: Unchecked"));
                }
                nameTag = "RtInspPanelAlignDisplayEnable";
                tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (bool)chkPanelAlignEnable.Checked);
                }
                if (chkPanelAlignEnable.Checked)
                {

                    MessageLoggerManager.Log.Info(String.Format("[Action] Pane lAlign Enable: Checked"));
                }
                else
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] Pane lAlign Enable: Unchecked"));
                }
                nameTag = "RtInspResultGraphicDisplayEnable";
                tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (bool)chxResultGraphic.Checked);
                }
                if (chxResultGraphic.Checked)
                {

                    MessageLoggerManager.Log.Info(String.Format("[Action] Inspection Result Graphic Display Enable: Checked"));
                }
                else
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] Inspection Result Graphic Display Enable: Unchecked"));
                }
                nameTag = "Inspection.1.PlotControl.ResultCommand";
                tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (bool)chxReturnResult.Checked);
                }
                if (chxReturnResult.Checked)
                {

                    MessageLoggerManager.Log.Info(String.Format("[Action] Return Result Enable: Checked"));
                }
                else
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] Return Result Enable: Unchecked"));
                }
                nameTag = "RtInspTrayInspection8Pocket";
                tag = GetTag(nameTag);
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (bool)chx8Pocket.Checked);
                }
                if (chx8Pocket.Checked)
                {

                    MessageLoggerManager.Log.Info(String.Format("[Action] Inspection 8 Pocket Enable: Checked"));
                }
                else
                {
                    MessageLoggerManager.Log.Info(String.Format("[Action] Inspection 8 Pocket Enable: Unchecked"));
                }
            }
            catch(Exception ex)
            {

            }
        }
        public delegate void EnableAllInspectionHandler(bool isEnable);
        public event EnableAllInspectionHandler EnableAllInspectionEvent;
        private void chkInspectionEnable_CheckedChanged(object sender, EventArgs e)
        {
            if(EnableAllInspectionEvent!=null)
            {
                EnableAllInspectionEvent.Invoke(chkInspectionEnable.Checked);
            }
            if (chkInspectionEnable.Checked)
            {

                MessageLoggerManager.Log.Info(String.Format("[Action] Inspection Enable: Checked"));
            }
            else
            {
                MessageLoggerManager.Log.Info(String.Format("[Action] Inspection Enable: Unchecked"));
            }
        }

        private void chxShowTrain1_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CvsInSightDisplayEdit mCvsInSightDisplayEdit = mCvsInsightDisplay.Edit;
                if (chxShowTrain1.Checked)
                {
                    MessageManager.MessageLoggerManager.Log.Info("[Action] Show Train Pattern 1...");
                    chxShowTrain2.Checked = false;
                    mCvsInsightDisplay.SetCurrentCell(51, 11);
                }
                else
                {
                    MessageManager.MessageLoggerManager.Log.Info("[Action] Hide Train Pattern 1...");
                    mCvsInsightDisplay.SetCurrentCell(2, 1);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void chxShowTrain2_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CvsInSightDisplayEdit mCvsInSightDisplayEdit = mCvsInsightDisplay.Edit;
                if (chxShowTrain2.Checked)
                {
                    MessageManager.MessageLoggerManager.Log.Info("[Action] Show Train Pattern 2...");
                    chxShowTrain1.Checked = false;
                    mCvsInsightDisplay.SetCurrentCell(120, 11);
                }
                else
                {
                    MessageManager.MessageLoggerManager.Log.Info("[Action] Hide Train Pattern 2...");
                    mCvsInsightDisplay.SetCurrentCell(2, 1);
                }
            }
            catch (Exception ex)
            {

            }
        }
    }

}

