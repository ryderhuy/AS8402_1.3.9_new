using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telerik.WinControls;
using Cognex.InSight.Controls.Display;

namespace Cognex.VS.InspSensor.CustomControls
{
    public partial class IntrisicControl : UserControl
    {
        public EventHandler AllSettingDoneEvent;
        CvsInSightDisplay mCvsInSightDisplay = null;
        private Telerik.WinControls.UI.RadPageView mPageView;
        private Telerik.WinControls.UI.RadPageViewPage mIntrisicPage;

        private Cognex.VS.InspSensor.CustomControls.IntrisicCalibration mCtrlIntrisicCalib = new IntrisicCalibration();

        public IntrisicControl()
        {
            mPageView = new Telerik.WinControls.UI.RadPageView();
            mPageView.Dock = DockStyle.Fill;
            mPageView.ViewMode = Telerik.WinControls.UI.PageViewMode.Stack;

            //mPageView.Pages.Add( new Telerik.WinControls.UI.RadPageViewPage)
            InitializeComponent();
            mIntrisicPage = new Telerik.WinControls.UI.RadPageViewPage("Intrinsic Calibration");

            mCtrlIntrisicCalib.Dock = DockStyle.Fill;

            mIntrisicPage.Controls.Add(mCtrlIntrisicCalib);

            mPageView.Pages.Add(mIntrisicPage);

            mPageView.AutoScroll = true;
            this.mPageView.EnableTheming = true;
            this.mPageView.ElementTree.EnableApplicationThemeName = true;
            this.mPageView.ThemeName = "Office2010Silver";
            this.Controls.Add(mPageView);
            mCtrlIntrisicCalib.SettingDoneEvent += MCtrlIntrisicCalib_SettingDone;
        }

        private void MCtrlIntrisicCalib_SettingDone(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action)(() => { MCtrlIntrisicCalib_SettingDone(sender, e); }));
            }
            else
                AllSettingDoneEvent.BeginInvoke(null, null, null, null);
        }

        public void InitAllSetting(InSightControl.InsightControls insightControls1)
        {
            mCvsInSightDisplay = insightControls1.GetInsightDisplay();
            mCtrlIntrisicCalib.Init(mCvsInSightDisplay);
        }
    }
}
