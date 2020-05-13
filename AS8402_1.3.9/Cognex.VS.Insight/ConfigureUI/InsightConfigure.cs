using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cognex.VS.Utility;
using System.Windows.Forms;
using Cognex.InSight.Controls.Display;
using Cognex.InSight;

namespace Cognex.VS.InSightControl
{
    public class InsightConfigure : UserControl
    {
        private CvsInSightDisplay mCvsInsightDisplay = null;
        public InsightConfigure(CvsInSightDisplay insightDisplay) : base()
        {
            InitializeComponent();
            mCvsInsightDisplay = insightDisplay;

            cbNumPanel.SelectedIndex = 0;

            if (insightDisplay.Connected)
            {
                UpdateParam();
            }

        }

        private void InsightConfigure_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.ApplicationExitCall)
            {
                e.Cancel = true;
                Hide();
            }
        }

        #region Windows Form Designer generated code
        private System.Windows.Forms.SplitContainer splitContainer1;
        protected System.Windows.Forms.Panel panel1;
        protected System.Windows.Forms.Button mBtnReset;
        protected System.Windows.Forms.Button mBtnOK;
        protected System.Windows.Forms.Button mBtnCancel;
        private System.Windows.Forms.Button btnPanelTrainRegion;
        private Button btnPanelSearchRegion;
        private GroupBox groupBox1;
        private ComboBox cbNumPanel;
        private Button btnSearchPanel;
        private NumericUpDown nudPanelInspShowGraphic;
        private Label label2;
        private NumericUpDown nudPanelInspThreshold;
        private Label label1;
        private GroupBox groupBox2;
        private NumericUpDown nudTrayInspThreshold;
        private Label label3;
        private Button btnTrayTrainRegion;
        private Button btnTrayTrainButton;
        private Button btnTraySearchRegion;
        private GroupBox Sorting;
        private Label label4;
        private ComboBox cbPatmaxSorting;
        private System.Windows.Forms.Button btnPanelTrain;

        private void InitializeComponent()
        {
            this.btnPanelTrainRegion = new System.Windows.Forms.Button();
            this.btnPanelTrain = new System.Windows.Forms.Button();
            this.btnPanelSearchRegion = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.nudPanelInspShowGraphic = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.nudPanelInspThreshold = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.cbNumPanel = new System.Windows.Forms.ComboBox();
            this.btnSearchPanel = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.nudTrayInspThreshold = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.btnTrayTrainRegion = new System.Windows.Forms.Button();
            this.btnTrayTrainButton = new System.Windows.Forms.Button();
            this.btnTraySearchRegion = new System.Windows.Forms.Button();
            this.Sorting = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cbPatmaxSorting = new System.Windows.Forms.ComboBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.mBtnOK = new System.Windows.Forms.Button();
            this.mBtnReset = new System.Windows.Forms.Button();
            this.mBtnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPanelInspShowGraphic)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPanelInspThreshold)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTrayInspThreshold)).BeginInit();
            this.Sorting.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panel1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.mBtnOK);
            this.splitContainer1.Panel2.Controls.Add(this.mBtnReset);
            this.splitContainer1.Panel2.Controls.Add(this.mBtnCancel);
            this.splitContainer1.Size = new System.Drawing.Size(933, 415);
            this.splitContainer1.SplitterDistance = 382;
            this.splitContainer1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(933, 382);
            this.panel1.TabIndex = 0;
            // 
            // mBtnOK
            // 
            this.mBtnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.mBtnOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.mBtnOK.Location = new System.Drawing.Point(672, 0);
            this.mBtnOK.Name = "mBtnOK";
            this.mBtnOK.Size = new System.Drawing.Size(87, 29);
            this.mBtnOK.TabIndex = 2;
            this.mBtnOK.Text = "OK";
            this.mBtnOK.UseVisualStyleBackColor = true;
            // 
            // mBtnReset
            // 
            this.mBtnReset.Dock = System.Windows.Forms.DockStyle.Right;
            this.mBtnReset.Location = new System.Drawing.Point(759, 0);
            this.mBtnReset.Name = "mBtnReset";
            this.mBtnReset.Size = new System.Drawing.Size(87, 29);
            this.mBtnReset.TabIndex = 1;
            this.mBtnReset.Text = "Reset";
            this.mBtnReset.UseVisualStyleBackColor = true;
            // 
            // mBtnCancel
            // 
            this.mBtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.mBtnCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.mBtnCancel.Location = new System.Drawing.Point(846, 0);
            this.mBtnCancel.Name = "mBtnCancel";
            this.mBtnCancel.Size = new System.Drawing.Size(87, 29);
            this.mBtnCancel.TabIndex = 0;
            this.mBtnCancel.Text = "Cancel";
            this.mBtnCancel.UseVisualStyleBackColor = true;
            // 
            // BaseDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(933, 415);
            this.Controls.Add(this.splitContainer1);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.Sorting);
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.groupBox1);
            // 
            // mBtnOK
            // 
            this.mBtnOK.Click += new System.EventHandler(this.mBtnOK_Click);
            // 
            // btnPanelTrainRegion
            // 
            this.btnPanelTrainRegion.Location = new System.Drawing.Point(16, 34);
            this.btnPanelTrainRegion.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnPanelTrainRegion.Name = "btnPanelTrainRegion";
            this.btnPanelTrainRegion.Size = new System.Drawing.Size(131, 33);
            this.btnPanelTrainRegion.TabIndex = 0;
            this.btnPanelTrainRegion.Text = "Train Region";
            this.btnPanelTrainRegion.UseVisualStyleBackColor = true;
            this.btnPanelTrainRegion.Click += new System.EventHandler(this.BtnTrainRegion_Click);
            // 
            // btnPanelTrain
            // 
            this.btnPanelTrain.Location = new System.Drawing.Point(16, 116);
            this.btnPanelTrain.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnPanelTrain.Name = "btnPanelTrain";
            this.btnPanelTrain.Size = new System.Drawing.Size(131, 33);
            this.btnPanelTrain.TabIndex = 1;
            this.btnPanelTrain.Text = "Train Patmax";
            this.btnPanelTrain.UseVisualStyleBackColor = true;
            this.btnPanelTrain.Click += new System.EventHandler(this.BtnTrainPatmax_Click);
            // 
            // btnPanelSearchRegion
            // 
            this.btnPanelSearchRegion.Location = new System.Drawing.Point(16, 75);
            this.btnPanelSearchRegion.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnPanelSearchRegion.Name = "btnPanelSearchRegion";
            this.btnPanelSearchRegion.Size = new System.Drawing.Size(131, 33);
            this.btnPanelSearchRegion.TabIndex = 2;
            this.btnPanelSearchRegion.Text = "Search Region";
            this.btnPanelSearchRegion.UseVisualStyleBackColor = true;
            this.btnPanelSearchRegion.Click += new System.EventHandler(this.BtnPanelSearchRegion_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.nudPanelInspShowGraphic);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.nudPanelInspThreshold);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.cbNumPanel);
            this.groupBox1.Controls.Add(this.btnSearchPanel);
            this.groupBox1.Controls.Add(this.btnPanelTrainRegion);
            this.groupBox1.Controls.Add(this.btnPanelTrain);
            this.groupBox1.Controls.Add(this.btnPanelSearchRegion);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(11, 25);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(429, 232);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "PanelInspection";
            // 
            // nudPanelInspShowGraphic
            // 
            this.nudPanelInspShowGraphic.Location = new System.Drawing.Point(340, 65);
            this.nudPanelInspShowGraphic.Maximum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.nudPanelInspShowGraphic.Name = "nudPanelInspShowGraphic";
            this.nudPanelInspShowGraphic.Size = new System.Drawing.Size(70, 24);
            this.nudPanelInspShowGraphic.TabIndex = 11;
            this.nudPanelInspShowGraphic.ValueChanged += new System.EventHandler(this.nudPanelInspShowGraphic_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(236, 67);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 18);
            this.label2.TabIndex = 10;
            this.label2.Text = "Show graphic";
            // 
            // nudPanelInspThreshold
            // 
            this.nudPanelInspThreshold.Location = new System.Drawing.Point(340, 35);
            this.nudPanelInspThreshold.Name = "nudPanelInspThreshold";
            this.nudPanelInspThreshold.Size = new System.Drawing.Size(70, 24);
            this.nudPanelInspThreshold.TabIndex = 9;
            this.nudPanelInspThreshold.ValueChanged += new System.EventHandler(this.nudPanelInspThreshold_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(236, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 18);
            this.label1.TabIndex = 8;
            this.label1.Text = "Threshold";
            // 
            // cbNumPanel
            // 
            this.cbNumPanel.FormattingEnabled = true;
            this.cbNumPanel.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6"});
            this.cbNumPanel.Location = new System.Drawing.Point(154, 187);
            this.cbNumPanel.Name = "cbNumPanel";
            this.cbNumPanel.Size = new System.Drawing.Size(66, 26);
            this.cbNumPanel.TabIndex = 6;
            // 
            // btnSearchPanel
            // 
            this.btnSearchPanel.Location = new System.Drawing.Point(17, 183);
            this.btnSearchPanel.Name = "btnSearchPanel";
            this.btnSearchPanel.Size = new System.Drawing.Size(131, 33);
            this.btnSearchPanel.TabIndex = 5;
            this.btnSearchPanel.Text = "Search Panel";
            this.btnSearchPanel.UseVisualStyleBackColor = true;
            this.btnSearchPanel.Click += new System.EventHandler(this.BtnSearchPanel_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.nudTrayInspThreshold);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.btnTrayTrainRegion);
            this.groupBox2.Controls.Add(this.btnTrayTrainButton);
            this.groupBox2.Controls.Add(this.btnTraySearchRegion);
            this.groupBox2.Location = new System.Drawing.Point(446, 25);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(344, 232);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "TrayDirection";
            // 
            // nudTrayInspThreshold
            // 
            this.nudTrayInspThreshold.Location = new System.Drawing.Point(254, 35);
            this.nudTrayInspThreshold.Name = "nudTrayInspThreshold";
            this.nudTrayInspThreshold.Size = new System.Drawing.Size(70, 22);
            this.nudTrayInspThreshold.TabIndex = 13;
            this.nudTrayInspThreshold.ValueChanged += new System.EventHandler(this.nudTrayInspThreshold_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(176, 38);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 17);
            this.label3.TabIndex = 12;
            this.label3.Text = "Threshold";
            // 
            // btnTrayTrainRegion
            // 
            this.btnTrayTrainRegion.Location = new System.Drawing.Point(6, 29);
            this.btnTrayTrainRegion.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnTrayTrainRegion.Name = "btnTrayTrainRegion";
            this.btnTrayTrainRegion.Size = new System.Drawing.Size(131, 33);
            this.btnTrayTrainRegion.TabIndex = 3;
            this.btnTrayTrainRegion.Text = "Train Region";
            this.btnTrayTrainRegion.UseVisualStyleBackColor = true;
            this.btnTrayTrainRegion.Click += new System.EventHandler(this.btnTrayTrainRegion_Click);
            // 
            // btnTrayTrainButton
            // 
            this.btnTrayTrainButton.Location = new System.Drawing.Point(6, 111);
            this.btnTrayTrainButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnTrayTrainButton.Name = "btnTrayTrainButton";
            this.btnTrayTrainButton.Size = new System.Drawing.Size(131, 33);
            this.btnTrayTrainButton.TabIndex = 4;
            this.btnTrayTrainButton.Text = "Train Patmax";
            this.btnTrayTrainButton.UseVisualStyleBackColor = true;
            this.btnTrayTrainButton.Click += new System.EventHandler(this.btnTrayTrainButton_Click);
            // 
            // btnTraySearchRegion
            // 
            this.btnTraySearchRegion.Location = new System.Drawing.Point(6, 70);
            this.btnTraySearchRegion.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnTraySearchRegion.Name = "btnTraySearchRegion";
            this.btnTraySearchRegion.Size = new System.Drawing.Size(131, 33);
            this.btnTraySearchRegion.TabIndex = 5;
            this.btnTraySearchRegion.Text = "Search Region";
            this.btnTraySearchRegion.UseVisualStyleBackColor = true;
            this.btnTraySearchRegion.Click += new System.EventHandler(this.btnTraySearchRegion_Click);
            // 
            // Sorting
            // 
            this.Sorting.Controls.Add(this.cbPatmaxSorting);
            this.Sorting.Controls.Add(this.label4);
            this.Sorting.Location = new System.Drawing.Point(11, 263);
            this.Sorting.Name = "Sorting";
            this.Sorting.Size = new System.Drawing.Size(429, 241);
            this.Sorting.TabIndex = 7;
            this.Sorting.TabStop = false;
            this.Sorting.Text = "Panel Sorting";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 36);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(34, 17);
            this.label4.TabIndex = 1;
            this.label4.Text = "Sort";
            // 
            // cbPatmaxSorting
            // 
            this.cbPatmaxSorting.FormattingEnabled = true;
            this.cbPatmaxSorting.Items.AddRange(new object[] {
            "1 - X inc",
            "2 - X dec",
            "3 - Y inc",
            "4 - Y dec"});
            this.cbPatmaxSorting.Location = new System.Drawing.Point(59, 33);
            this.cbPatmaxSorting.Name = "cbPatmaxSorting";
            this.cbPatmaxSorting.Size = new System.Drawing.Size(121, 24);
            this.cbPatmaxSorting.TabIndex = 2;
            this.cbPatmaxSorting.SelectedIndexChanged += new System.EventHandler(this.cbPatmaxSorting_SelectedIndexChanged);
            // 
            // InsightConfigure
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.ClientSize = new System.Drawing.Size(1066, 553);
            this.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.Name = "InsightConfigure";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPanelInspShowGraphic)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPanelInspThreshold)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTrayInspThreshold)).EndInit();
            this.Sorting.ResumeLayout(false);
            this.Sorting.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion

        public void UpdateParam()
        {
            try
            {
                var tag = GetTag(ResourceUtility.GetString("RtInspPanelThreshold"));
                if (tag != null)
                {
                    float threshold = (float)GetValue(tag.Location);
                    nudPanelInspThreshold.Value = (decimal)threshold;
                }

                tag = GetTag(ResourceUtility.GetString("RtInspPanelShowgraphic"));
                if (tag != null)
                {
                    int showGraphic = (int)GetValue(tag.Location);
                    nudPanelInspShowGraphic.Value = (decimal)showGraphic;
                }

                tag = GetTag(ResourceUtility.GetString("RtInspTrayThreshold"));
                if (tag != null)
                {
                    float threshTray = (float)GetValue(tag.Location);
                    nudTrayInspThreshold.Value = (decimal)threshTray;
                }

                tag = GetTag(ResourceUtility.GetString("RtAlignmentPatmaxSorting"));
                if(tag != null)
                {
                    int iSort = (int)GetValue(tag.Location);
                    cbPatmaxSorting.SelectedIndex = iSort - 1;
                }
            }
            catch(Exception ex)
            {

            }
        }

        private void BtnTrainRegion_Click(object sender, EventArgs e)
        {
            var tag = GetTag(ResourceUtility.GetString("RtInspPanelTrainRegion"));
            if (tag != null)
            {
                EditCellGraphic(tag.Location);
            }
        }

        private void BtnTrainPatmax_Click(object sender, EventArgs e)
        {
            var tag = GetTag(ResourceUtility.GetString("RtInspPanelTrainButton"));
            if (tag != null)
            {
                InsightClickButton(tag.Location);
            }
        }
        private void btnTrayTrainRegion_Click(object sender, EventArgs e)
        {
            var tag = GetTag(ResourceUtility.GetString("RtInspTrayTrainRegion"));
            if (tag != null)
            {
                EditCellGraphic(tag.Location);
            }
        }

        private void btnTraySearchRegion_Click(object sender, EventArgs e)
        {
            var tag = GetTag(ResourceUtility.GetString("RtInspTraySearchRegion"));
//<<<<<<< HEAD
//=======
//            ContentQueue.gOnly.Info("RtInspTraySearchRegion: " + tag.Name);
//>>>>>>> 1602afb7180481b005393225a6e2f682a20085cf
            if (tag != null)
            {
                EditCellGraphic(tag.Location);
            }
        }

        private void btnTrayTrainButton_Click(object sender, EventArgs e)
        {
            var tag = GetTag(ResourceUtility.GetString("RtInspTrayTrainButton"));
            //<<<<<<< HEAD
            if (tag != null)
            {
                //=======
                ContentQueue.gOnly.Info("RtInspTrayTrainButton: " + tag.Name);
                if (tag != null)
                {
                    ContentQueue.gOnly.Info("RtInspTrayTrainButton. Click");
                    //>>>>>>> 1602afb7180481b005393225a6e2f682a20085cf
                    InsightClickButton(tag.Location);
                }
            }
        }
       
        protected override void Dispose(bool disposing)
        {
            mCvsInsightDisplay = null;
            base.Dispose(disposing);
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
            mCvsInsightDisplay.EditCellGraphic(location);
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

        private void BtnPanelSearchRegion_Click(object sender, EventArgs e)
        {
            var tag = GetTag(ResourceUtility.GetString("RtInspPanelSearchRegion"));
            if (tag != null)
            {
                EditCellGraphic(tag.Location);
            }
        }

        private void BtnGetValue_Click(object sender, EventArgs e)
        {
            var tag = GetTag(ResourceUtility.GetString("RtInspPanelThreshold"));
            if (tag != null)
            {
                float threshold = (float)GetValue(tag.Location);

            }
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
            return null;
        }

        private void BtnSearchPanel_Click(object sender, EventArgs e)
        {
            try
            {
                string sNumPanel = cbNumPanel.Text;
                string str = "RtInspPanelSearchRegion_" + sNumPanel;
                var tag = GetTag(ResourceUtility.GetString(str));
                if (tag != null)
                {
                    EditCellGraphic(tag.Location);
                }
            }
            catch (CvsException ex)
            {

            }
        }

        private void nudPanelInspThreshold_ValueChanged(object sender, EventArgs e)
        {
            try
            { 
                string str = "RtInspPanelThreshold";
                var tag = GetTag(ResourceUtility.GetString(str));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)nudPanelInspThreshold.Value);
                }
            }
            catch (CvsException ex)
            {

            }  
        }
        private void nudTrayInspThreshold_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string str = "RtInspTrayAcceptThreshold";
                var tag = GetTag(ResourceUtility.GetString(str));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (float)nudTrayInspThreshold.Value);
                }
            }
            catch (CvsException ex)
            {

            }
        }
        private void nudPanelInspShowGraphic_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string str = "RtInspPanelShowgraphic";
                var tag = GetTag(ResourceUtility.GetString(str));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, (int)nudPanelInspShowGraphic.Value);
                }
            }
            catch (CvsException ex)
            {

            }
        }

        private void mBtnOK_Click(object sender, EventArgs e)
        {

        }

        private void nudPatmaxSorting_ValueChanged(object sender, EventArgs e)
        {

        }

        private void cbPatmaxSorting_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {               
                int iSort = cbPatmaxSorting.SelectedIndex;
                string str = "RtAlignmentPatmaxSorting";
                var tag = GetTag(ResourceUtility.GetString(str));
                if (tag != null)
                {
                    InsightSetValue(tag.Location, iSort+1);
                }
            }
            catch (CvsException ex)
            {

            }
        }

    }
}
