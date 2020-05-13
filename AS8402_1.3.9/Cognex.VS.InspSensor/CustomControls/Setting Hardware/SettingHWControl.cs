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

namespace Cognex.VS.InspSensor.CustomControls.Setting_Hardware
{
    public partial class SettingHWControl : UserControl
    {
        public EventHandler AllSettingDoneEvent;
        public EventHandler ExitEvent;
        CvsInSightDisplay mCvsInSightDisplay = null;
        private Telerik.WinControls.UI.RadPageView mPageView;
        private Telerik.WinControls.UI.RadPageViewPage mSettingHWPage;
        private Cognex.VS.InspSensor.CustomControls.Setting_Hardware.SettingHW mSettingHW = new SettingHW();

        public SettingHWControl()
        {
            InitializeComponent();

            mPageView = new Telerik.WinControls.UI.RadPageView();
            mPageView.Dock = DockStyle.Fill;
            mPageView.ViewMode = Telerik.WinControls.UI.PageViewMode.Stack;

            //mPageView.Pages.Add( new Telerik.WinControls.UI.RadPageViewPage)
            //InitializeComponent();
            mSettingHWPage = new Telerik.WinControls.UI.RadPageViewPage("Setting Hardware");

            mSettingHW.Dock = DockStyle.Fill;

            mSettingHWPage.Controls.Add(mSettingHW);

            mPageView.Pages.Add(mSettingHWPage);

            mPageView.AutoScroll = true;
            this.mPageView.EnableTheming = true;
            this.mPageView.ElementTree.EnableApplicationThemeName = true;
            this.mPageView.ThemeName = "Office2010Silver";
            this.Controls.Add(mPageView);
            mSettingHW.SettingDoneEvent += MCtrlIntrisicCalib_SettingDone;
            mSettingHW.ExitProgramByFactoryResetEvent += MExit_Event;
        }

        private void MExit_Event(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action)(() => { MExit_Event(sender, e); }));
            }
            else
                ExitEvent.BeginInvoke(null, null, null, null);
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

        public void InitAllSetting(InSightControl.InsightControls insightControls1, CvsInSightDisplay cvsInsightDisplay, string hostName)
        {
            mCvsInSightDisplay = cvsInsightDisplay;
            mSettingHW.Init(insightControls1, cvsInsightDisplay, hostName);
        }
    }
}
