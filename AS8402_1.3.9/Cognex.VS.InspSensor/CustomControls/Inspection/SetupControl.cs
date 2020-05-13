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
    public partial class SetupControl : UserControl
    {
        public EventHandler AllSettingDoneEvent;
        CvsInSightDisplay mCvsInSightDisplay = null;
        private Telerik.WinControls.UI.RadPageView mPageView;
        private Telerik.WinControls.UI.RadPageViewPage mTrayInspectionPage;
        private Telerik.WinControls.UI.RadPageViewPage mPanelInspectionPage;
        private Telerik.WinControls.UI.RadPageViewPage mTrayBlobCheckPage;
        private Telerik.WinControls.UI.RadPageViewPage mConveyorInspectionPage;
        private Telerik.WinControls.UI.RadPageViewPage mDisplayControlPage;

        private Cognex.VS.InspSensor.CustomControls.TrayInspection mCtrlTrayInspection = new TrayInspection();
        private Cognex.VS.InspSensor.CustomControls.PanelInspection mCtrlPanelInspection = new PanelInspection();
        private Cognex.VS.InspSensor.CustomControls.TrayBlobCheck mCtrlTrayBlobCheck = new TrayBlobCheck();
        private Cognex.VS.InspSensor.CustomControls.ConveyorInspection mCtrlConveyorInspection = new ConveyorInspection();
        private Cognex.VS.InspSensor.CustomControls.DisplayControl mCtrlDisplayControl = new DisplayControl();

        public ConveyorInspection ConveyorInspection
        {
            get { return mCtrlConveyorInspection; }
        }
        public Cognex.VS.InspSensor.CustomControls.DisplayControl CtrlDisplayControl
        {
            get
            {
                return mCtrlDisplayControl;
            }
            set
            {
                mCtrlDisplayControl = value;
            }
        }
        public SetupControl()
        {
            mPageView = new Telerik.WinControls.UI.RadPageView();
            mPageView.Dock = DockStyle.Fill;
            mPageView.ViewMode = Telerik.WinControls.UI.PageViewMode.Stack;

            //mPageView.Pages.Add( new Telerik.WinControls.UI.RadPageViewPage)
            InitializeComponent();
            mTrayInspectionPage = new Telerik.WinControls.UI.RadPageViewPage("Tray Inspection");
            mPanelInspectionPage = new Telerik.WinControls.UI.RadPageViewPage("Panel Inspection");
            mTrayBlobCheckPage = new Telerik.WinControls.UI.RadPageViewPage("Blob Finder");
            mConveyorInspectionPage = new Telerik.WinControls.UI.RadPageViewPage("Conveyor/Align Inspection");
            mDisplayControlPage = new Telerik.WinControls.UI.RadPageViewPage("Display Control");

            mCtrlTrayInspection.Dock = DockStyle.Fill;
            mCtrlPanelInspection.Dock = DockStyle.Fill;
            mCtrlTrayBlobCheck.Dock = DockStyle.Fill;
            mCtrlConveyorInspection.Dock = DockStyle.Fill;
            mCtrlDisplayControl.Dock = DockStyle.Fill;

            mTrayInspectionPage.Controls.Add(mCtrlTrayInspection);
            mPanelInspectionPage.Controls.Add(mCtrlPanelInspection);
            mTrayBlobCheckPage.Controls.Add(mCtrlTrayBlobCheck);
            mConveyorInspectionPage.Controls.Add(mCtrlConveyorInspection);
            mDisplayControlPage.Controls.Add(mCtrlDisplayControl);

            mPageView.Pages.Add(mTrayInspectionPage);
            mPageView.Pages.Add(mPanelInspectionPage);
            mPageView.Pages.Add(mTrayBlobCheckPage);
            mPageView.Pages.Add(mConveyorInspectionPage);
            mPageView.Pages.Add(mDisplayControlPage);

            mPageView.AutoScroll = true;
            //mConveyorSetupPage.Item.BackColor = Color.LightGray;
            //mConveyorSetupPage.Item.BackColor2 = Color.LightGray;
            //mConveyorSetupPage.Item.BackColor3 = Color.LightGray;
            //mConveyorSetupPage.Item.BackColor4 = Color.LightGray;
            //mPageView.ViewElement.Header.BackColor = Color.Orange;
            //mPageView.ViewElement.Header.BackColor2 = Color.Orange;
            //mPageView.ViewElement.Header.BackColor3 = Color.Orange;
            //mPageView.ViewElement.Header.BackColor4 = Color.Orange;
            //mConveyorSetupPage.Item.GradientStyle = GradientStyles.Solid;
            this.mPageView.EnableTheming = true;
            this.mPageView.ElementTree.EnableApplicationThemeName = true;
            this.mPageView.ThemeName = "Office2010Silver";
            mPageView.SelectedPageChanged += MPageView_SelectedPageChanged;
            this.Controls.Add(mPageView);
            mCtrlTrayInspection.SettingDoneEvent += MCtrlTrayInspection_SettingDone;
            mCtrlPanelInspection.SettingDoneEvent += MCtrlPanelInspection_SettingDone;
            mCtrlTrayBlobCheck.SettingDoneEvent += MCtrlTrayBlobCheck_SettingDone;
            mCtrlConveyorInspection.SettingDoneEvent += MCtrlConveyorInspection_SettingDone;
            mCtrlDisplayControl.SettingDoneEvent += MCtrlDisplayControl_SettingDone;
        }

        private void MPageView_SelectedPageChanged(object sender, EventArgs e)
        {
            try
            {
                Telerik.WinControls.UI.RadPageView currentPageView = (Telerik.WinControls.UI.RadPageView)sender;
                string pageName = currentPageView.AccessibilityObject.Name.ToString();
                string trim = pageName.Trim();
                switch (trim)
                {
                    case "Tray Inspection":
                        mCtrlTrayInspection.Init(mCvsInSightDisplay);
                        break;
                    case "Panel Inspection":
                        mCtrlPanelInspection.Init(mCvsInSightDisplay);
                        break;
                    case "Blob Finder":
                        mCtrlTrayBlobCheck.Init(mCvsInSightDisplay);
                        break;
                    case "Conveyor/Align Inspection":
                        mCtrlConveyorInspection.Init(mCvsInSightDisplay);
                        break;
                    case "Display Control":
                        mCtrlDisplayControl.Init(mCvsInSightDisplay);
                        break;
                    default:
                        mCtrlTrayInspection.Init(mCvsInSightDisplay);
                        break;
                }
                MessageManager.MessageLoggerManager.Log.Info("[Action] Inspection Setting Page: " + trim);
            }
            catch (Exception ex)
            {

            }
        }

        private void MCtrlTrayInspection_SettingDone(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action)(() => { MCtrlTrayInspection_SettingDone(sender, e); }));
            }
            else
                mPageView.SelectedPage = mPanelInspectionPage;
        }

        private void MCtrlPanelInspection_SettingDone(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action)(() => { MCtrlPanelInspection_SettingDone(sender, e); }));
            }
            else
                mPageView.SelectedPage = mTrayBlobCheckPage;
        }

        private void MCtrlTrayBlobCheck_SettingDone(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action)(() => { MCtrlTrayBlobCheck_SettingDone(sender, e); }));
            }
            else
                mPageView.SelectedPage = mConveyorInspectionPage;
        }

        private void MCtrlConveyorInspection_SettingDone(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action)(() => { MCtrlConveyorInspection_SettingDone(sender, e); }));
            }
            else
                mPageView.SelectedPage = mDisplayControlPage;
        }

        private void MCtrlDisplayControl_SettingDone(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action)(() => { MCtrlDisplayControl_SettingDone(sender, e); }));
            }
            else
                AllSettingDoneEvent.BeginInvoke(null, null, null, null);
        }

        public void InitAllSetting(InSightControl.InsightControls insightControls1)
        {
            mCvsInSightDisplay = insightControls1.GetInsightDisplay();
            insightControls1.InSight.SoftOnline = false;
            mCtrlTrayInspection.Init(mCvsInSightDisplay);
            //mCtrlPanelInspection.Init(mCvsInSightDisplay);
            //mCtrlTrayBlobCheck.Init(mCvsInSightDisplay);
            //mCtrlConveyorInspection.Init(mCvsInSightDisplay);
            //mCtrlDisplayControl.Init(mCvsInSightDisplay);
        }
    }
}
