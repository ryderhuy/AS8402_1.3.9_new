using Cognex.InSight;
using Cognex.InSight.Controls.Display;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cognex.VS.InspSensor.CustomControls.HandEyeResult
{
    public partial class HandEyeResultForm : Form
    {
        public EventHandler HandEyeResultFormExit;
        private CvsInSightDisplay mCvsInsightDisplay = null;

        public HandEyeResultForm(CvsInSightDisplay cvsInSightDisplay)
        {
            InitializeComponent();
            mCvsInsightDisplay = cvsInSightDisplay;
            UpdateHandEyeResult(1);
            UpdateHandEyeResult(2);
        }

        private void UpdateHandEyeResult(int index)
        {
            try
            {
                var tag = GetTag(string.Format("HECalibration.{0}.Valid", index));
                if (tag != null)
                {
                    bool isCalib = (bool)GetValue(tag.Location);
                    if (isCalib)
                    {

                        tag = GetTag(string.Format("HECalibration.Image2DGrade_{0}", index));
                        if (tag != null)
                        {
                            float grade = (float)GetValue(tag.Location);
                            if (index == 1)
                            {
                                trackBar1.Value = (int)grade;
                                switch ((int)grade)
                                {
                                    case 0:
                                        {
                                            lblStatus1.Text = "Fail";
                                        }
                                        break;
                                    case 1:
                                        {
                                            lblStatus1.Text = "Poor";
                                        }
                                        break;
                                    case 2:
                                        {
                                            lblStatus1.Text = "Good";
                                        }
                                        break;
                                    case 3:
                                        {
                                            lblStatus1.Text = "Excellent";
                                        }
                                        break;
                                }
                            }
                            else if (index == 2)
                            {
                                trackBar2.Value = (int)grade;
                                switch ((int)grade)
                                {
                                    case 0:
                                        {
                                            lblStatus2.Text = "Fail";
                                        }
                                        break;
                                    case 1:
                                        {
                                            lblStatus2.Text = "Poor";
                                        }
                                        break;
                                    case 2:
                                        {
                                            lblStatus2.Text = "Good";
                                        }
                                        break;
                                    case 3:
                                        {
                                            lblStatus2.Text = "Excellent";
                                        }
                                        break;
                                }
                            }
                        }


                        tag = GetTag(string.Format("HECalibration.{0}.PixelSizeX", index));
                        if (tag != null)
                        {
                            float value = (float)GetValue(tag.Location);
                            if (index == 1)
                                lblPixelX1.Text = value.ToString("0.000");
                            else if (index == 2)
                                lblPixelX2.Text = value.ToString("0.000");
                        }

                        tag = GetTag(string.Format("HECalibration.{0}.PixelSizeY", index));
                        if (tag != null)
                        {
                            float value = (float)GetValue(tag.Location);
                            if (index == 1)
                                lblPixelY1.Text = value.ToString("0.000");
                            else if (index == 2)
                                lblPixelY2.Text = value.ToString("0.000");
                        }

                        tag = GetTag(string.Format("HECalibration.{0}.RMSImage2D", index));
                        if (tag != null)
                        {
                            float value = (float)GetValue(tag.Location);
                            if (index == 1)
                                lblRmsImage1.Text = value.ToString("0.000");
                            if (index == 2)
                                lblRmsImage2.Text = value.ToString("0.000");
                        }

                        tag = GetTag(string.Format("HECalibration.{0}.RMSHome2D", index));
                        if (tag != null)
                        {
                            float value = (float)GetValue(tag.Location);
                            if (index == 1)
                                lblRmsHome1.Text = value.ToString("0.000");
                            if (index == 2)
                                lblRmsHome2.Text = value.ToString("0.000");
                        }

                        tag = GetTag("HECalibration.FoV_X");
                        if (tag != null)
                        {
                            float value = (float)GetValue(tag.Location);
                            if (index == 1)
                                lblFovX1.Text = value.ToString("0.000");
                            else if (index == 2)
                                lblFovX2.Text = value.ToString("0.000");
                        }

                        tag = GetTag("HECalibration.FoV_Y");
                        if (tag != null)
                        {
                            float value = (float)GetValue(tag.Location);
                            if (index == 1)
                                lblFovY1.Text = value.ToString("0.000");
                            else if (index == 2)
                                lblFovY2.Text = value.ToString("0.000");
                        }
                    }
                    else
                    {
                        if (index == 1)
                        {
                            trackBar1.Value = 0;
                            lblStatus1.Text = "Not Calibrated";
                        }
                        else if (index == 2)
                        {
                            trackBar2.Value = 0;
                            lblStatus2.Text = "Not Calibrated";
                        }
                    }
                }
            }
            catch (Exception ex)
            { }
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

        private void btnAccept_Click(object sender, EventArgs e)
        {
            if (HandEyeResultFormExit != null)
                HandEyeResultFormExit.Invoke(null, null);
        }
    }
}
