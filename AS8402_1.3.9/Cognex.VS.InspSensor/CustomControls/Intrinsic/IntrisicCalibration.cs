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
using System.Threading;
using Cognex.InSight.Controls;
using MessageManager;

namespace Cognex.VS.InspSensor.CustomControls
{
    public partial class IntrisicCalibration : UserControl
    {
        public EventHandler SettingDoneEvent;
        private CvsInSightDisplay mCvsInsightDisplay = null;
        private string mFinderTypeName = "Blob";
        private int mBlobType = 0; //0 is black, 1 is white
        private int mBackgroundType = 0; //0 is black, 1 is white
        CvsInSightDisplayEdit mCvsInSightDisplayEdit;
        public IntrisicCalibration()
        {
            InitializeComponent();
            cbxUnit.SelectedIndex = 0;
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
                var tag = GetTag(ResourceUtility.GetString("RtIntrisicSizeX"));
                if (tag != null)
                {
                    float sizeX = (float)GetValue(tag.Location);
                    numSizeX.Value = (decimal)sizeX;
                }

                tag = GetTag(ResourceUtility.GetString("RtIntrisicSizeY"));
                if (tag != null)
                {
                    float sizeY = (float)GetValue(tag.Location);
                    numSizeY.Value = (decimal)sizeY;
                }

                tag = GetTag(ResourceUtility.GetString("RtIntrisicUnit"));
                if (tag != null)
                {
                    int unit = (int)GetValue(tag.Location);
                    cbxUnit.SelectedIndex = (int)unit;
                }

                tag = GetTag(ResourceUtility.GetString("RtIntrisicShowGraphic"));
                if (tag != null)
                {
                    int show = (int)GetValue(tag.Location);
                    numShowGraphic.Value = (int)show;
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
                string nameTag = "RtIntrisicSizeX";
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numSizeX.Value);
                }

                nameTag = "RtIntrisicSizeY";
                tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)numSizeY.Value);
                }

                nameTag = "RtIntrisicUnit";
                tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightSetListBox(tag.Location, (int)cbxUnit.SelectedIndex);
                }
                */
                SettingDoneEvent.BeginInvoke(null, null, null, null);
            }
            catch (CvsException ex)
            {

            }
        }

        private void btnTrainRegion_Click(object sender, EventArgs e)
        {
            string nameTag = "RtIntrisicTrainRegion";
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                EditCellGraphic(tag.Location);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Click Train Region"));
        }

        private void btnMaximizeRegion_Click(object sender, EventArgs e)
        {
            string nameTag = "RtIntrisicMaximizeRegion";
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                InsightClickButton(tag.Location);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Click Maximize Region"));
        }

        private void btnCalcIntrisic_Click(object sender, EventArgs e)
        {
            try
            {
                MessageManager.MessageLoggerManager.Log.Info("[Action] Calculate Intrinsic...");
                string nameTag = "RtIntrisicExecute";
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {

                    //Thread t = new Thread(new ThreadStart(() => InsightClickButton(tag.Location)));
                    //InsightClickButton(tag.Location);
                    //t.Start();
                    InsightClickButton(tag.Location);
                    //MessageBox.Show("Done");
                }
                MessageLoggerManager.Log.Info(String.Format("[Action] Click Excute Catulation"));
            }
            catch(Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }

        private void numSizeX_ValueChanged(object sender, EventArgs e)
        {
            string nameTag = "RtIntrisicSizeX";
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                InsightSetValue(tag.Location, (float)numSizeX.Value);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Size X: " + (float)numSizeX.Value));
        }

        private void numSizeY_ValueChanged(object sender, EventArgs e)
        {
            string nameTag = "RtIntrisicSizeY";
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                InsightSetValue(tag.Location, (float)numSizeY.Value);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Size Y: " + (float)numSizeY.Value));
        }

        private void cbxUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            string nameTag = "RtIntrisicUnit";
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                InsightSetListBox(tag.Location, (int)cbxUnit.SelectedIndex);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Unit: " + cbxUnit.SelectedItem));
        }

        private void numShowGraphic_ValueChanged(object sender, EventArgs e)
        {
            string nameTag = "RtIntrisicShowGraphic";
            var tag = GetTag(ResourceUtility.GetString(nameTag));
            if (tag != null)
            {
                InsightSetValue(tag.Location, (int)numShowGraphic.Value);
            }
            MessageLoggerManager.Log.Info(String.Format("[Action] Show Graphic: " + (float)numShowGraphic.Value));
        }

        private void btnInstrinsicImport_Click(object sender, EventArgs e)
        {
            try
            {
                /*
                //Import by file
                string nameTag = "RtIntrinsicImport";
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightClickButton(tag.Location);
                }
                */

                //Import by Snippet
                mCvsInSightDisplayEdit = mCvsInsightDisplay.Edit;
                mCvsInsightDisplay.SetCurrentCell(79, 1);
                CvsCellLocation cvs =  mCvsInsightDisplay.CurrentCellNow;
                int col = mCvsInsightDisplay.CurrentCellNow.Column;
                int row = mCvsInsightDisplay.CurrentCellNow.Row;
                CvsAction.SetEnabled(mCvsInSightDisplayEdit.ImportSnippet, true);
                bool isEnable = mCvsInSightDisplayEdit.ImportSnippet.Enabled;

                mCvsInSightDisplayEdit.ImportSnippet.Execute();
                MessageLoggerManager.Log.Info(String.Format("[Action] Click Import"));
            }
            catch (Exception ex)
            {

            }
        }

        private void btnInstrinsicExport_Click(object sender, EventArgs e)
        {
            try
            {
                /*
                //Export by file
                string nameTag = "RtIntrinsicExport";
                var tag = GetTag(ResourceUtility.GetString(nameTag));
                if (tag != null)
                {
                    InsightClickButton(tag.Location);
                }
                */
                //Export by Snippet
                mCvsInSightDisplayEdit = mCvsInsightDisplay.Edit;
                mCvsInsightDisplay.SetCurrentCell(79, 1);
                CvsCellLocation cvs = mCvsInsightDisplay.CurrentCellNow;
                int col = mCvsInsightDisplay.CurrentCellNow.Column;  
                int row = mCvsInsightDisplay.CurrentCellNow.Row;
                CvsAction.SetEnabled(mCvsInSightDisplayEdit.ExportSnippet, true);
                bool isEnable = mCvsInSightDisplayEdit.ExportSnippet.Enabled;

                mCvsInSightDisplayEdit.ExportSnippet.Execute();
                MessageLoggerManager.Log.Info(String.Format("[Action] Click Export"));
            }
            catch (Exception ex)
            {

            }
        }
    }
}

