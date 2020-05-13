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

namespace Cognex.VS.InspSensor.CustomControls.HandEyeResult
{
    public partial class HandEyeStatus : UserControl
    {
        public EventHandler SettingDoneEvent;
        private CvsInSightDisplay mCvsInsightDisplay = null;
        private string mFinderTypeName = "Conveyor";
        bool mInit = false;
        HandEyeResultForm mHandEyeResultForm = null;
        public HandEyeStatus()
        {
            InitializeComponent();
        }

        public void Init(CvsInSightDisplay cvsInSightDisplay)
        {
            mCvsInsightDisplay = cvsInSightDisplay;
            LoadHandEyeStatus();
        }

        private void LoadHandEyeStatus()
        {
            var tag = GetTag("HECalibration.1.Valid");
            if (tag != null)
            {
                bool isCalib = (bool)GetValue(tag.Location);
                if (isCalib)
                {

                    tag = GetTag("HECalibration.Image2DGrade_1");
                    if (tag != null)
                    {
                        float grade = (float)GetValue(tag.Location);
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
                }
                else
                {
                    trackBar1.Value = 0;
                    lblStatus1.Text = "Not Calibrated";
                }
            }


            tag = GetTag("HECalibration.2.Valid");
            if (tag != null)
            {
                bool isCalib = (bool)GetValue(tag.Location);
                if(isCalib)
                {
                    tag = GetTag("HECalibration.Image2DGrade_2");
                    if (tag != null)
                    {
                        float grade = (float)GetValue(tag.Location);
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
                else
                {
                    trackBar2.Value = 0;
                    lblStatus2.Text = "Not Calibrated";
                }
            }

            
        }

        private void btnDetails_Click(object sender, EventArgs e)
        {
            OpenHandEyeResultForm();
        }
        private delegate void OpenHeFormDel();
        public void OpenHandEyeResultForm()
        {
            if (this.InvokeRequired)
            {
                OpenHeFormDel openFormDel = new OpenHeFormDel(OpenHandEyeResultForm);
                this.Invoke(openFormDel);
            }
            else
            {
                if (mHandEyeResultForm == null)
                {
                    mHandEyeResultForm = new HandEyeResultForm(mCvsInsightDisplay);
                    mHandEyeResultForm.HandEyeResultFormExit += HandEyeFormExitEvent;
                    mHandEyeResultForm.Show();
                }
                else
                {
                    mHandEyeResultForm.BringToFront();
                }
            }
        }
        private void HandEyeFormExitEvent(object sender, EventArgs e)
        {
            if(mHandEyeResultForm != null)
            {
                mHandEyeResultForm.HandEyeResultFormExit -= HandEyeFormExitEvent;
                mHandEyeResultForm.Dispose();
                mHandEyeResultForm.Close();
                mHandEyeResultForm = null;
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
    }
}
