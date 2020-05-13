namespace Cognex.VS.InspSensor.CustomControls
{
    partial class TrayBlobCheck
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label9 = new System.Windows.Forms.Label();
            this.numMinSize = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.numContrastThreshold = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cbxBlobSearchType = new System.Windows.Forms.ComboBox();
            this.numBlobNumber = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.chxEnable = new System.Windows.Forms.CheckBox();
            this.numShowGraphicF = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnSearchRegionTrayRProperty = new System.Windows.Forms.Button();
            this.btnSearchRegionTrayFProperty = new System.Windows.Forms.Button();
            this.numShowGraphicR = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.btnSearchRegionF = new System.Windows.Forms.Button();
            this.numMaxSize = new System.Windows.Forms.NumericUpDown();
            this.btnSearchRegionR = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnAccept = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnPanelSearchRegionRProperty = new System.Windows.Forms.Button();
            this.btnPanelSearchRegionFProperty = new System.Windows.Forms.Button();
            this.cbxPanelNumb = new System.Windows.Forms.ComboBox();
            this.label21 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.chxEnablePanelBlob = new System.Windows.Forms.CheckBox();
            this.numPanelShowGraphicR = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.numPanelShowGraphicF = new System.Windows.Forms.NumericUpDown();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.btnPanelSearchRegionF = new System.Windows.Forms.Button();
            this.numPanelMaxSize = new System.Windows.Forms.NumericUpDown();
            this.btnPanelSearchRegionR = new System.Windows.Forms.Button();
            this.label15 = new System.Windows.Forms.Label();
            this.cbxPanelBlobSearchType = new System.Windows.Forms.ComboBox();
            this.numPanelBlobNumb = new System.Windows.Forms.NumericUpDown();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.numPanelMinSize = new System.Windows.Forms.NumericUpDown();
            this.numPanelContrast = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numMinSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numContrastThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBlobNumber)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numShowGraphicF)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numShowGraphicR)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxSize)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPanelShowGraphicR)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPanelShowGraphicF)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPanelMaxSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPanelBlobNumb)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPanelMinSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPanelContrast)).BeginInit();
            this.SuspendLayout();
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 65);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(45, 39);
            this.label9.TabIndex = 81;
            this.label9.Text = "Search\r\nRegion\r\nForward";
            this.label9.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // numMinSize
            // 
            this.numMinSize.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numMinSize.Location = new System.Drawing.Point(308, 124);
            this.numMinSize.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.numMinSize.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numMinSize.Name = "numMinSize";
            this.numMinSize.Size = new System.Drawing.Size(71, 20);
            this.numMinSize.TabIndex = 70;
            this.numMinSize.Value = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.numMinSize.ValueChanged += new System.EventHandler(this.numMinSize_ValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(248, 128);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 13);
            this.label5.TabIndex = 69;
            this.label5.Text = "Min Size";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numContrastThreshold
            // 
            this.numContrastThreshold.DecimalPlaces = 1;
            this.numContrastThreshold.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.numContrastThreshold.Location = new System.Drawing.Point(308, 87);
            this.numContrastThreshold.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.numContrastThreshold.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numContrastThreshold.Name = "numContrastThreshold";
            this.numContrastThreshold.Size = new System.Drawing.Size(71, 20);
            this.numContrastThreshold.TabIndex = 68;
            this.numContrastThreshold.Value = new decimal(new int[] {
            150,
            0,
            0,
            65536});
            this.numContrastThreshold.ValueChanged += new System.EventHandler(this.numContrastThreshold_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(242, 84);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(54, 26);
            this.label4.TabIndex = 67;
            this.label4.Text = "Contrast\r\nThreshold";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(36, 179);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 26);
            this.label2.TabIndex = 83;
            this.label2.Text = "Blob\r\nType";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // cbxBlobSearchType
            // 
            this.cbxBlobSearchType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxBlobSearchType.FormattingEnabled = true;
            this.cbxBlobSearchType.Items.AddRange(new object[] {
            "Dark On Bright",
            "Bright On Dark"});
            this.cbxBlobSearchType.Location = new System.Drawing.Point(71, 179);
            this.cbxBlobSearchType.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cbxBlobSearchType.Name = "cbxBlobSearchType";
            this.cbxBlobSearchType.Size = new System.Drawing.Size(120, 21);
            this.cbxBlobSearchType.TabIndex = 82;
            this.cbxBlobSearchType.SelectedIndexChanged += new System.EventHandler(this.cbxBlobSearchType_SelectedIndexChanged);
            // 
            // numBlobNumber
            // 
            this.numBlobNumber.Location = new System.Drawing.Point(308, 45);
            this.numBlobNumber.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.numBlobNumber.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numBlobNumber.Name = "numBlobNumber";
            this.numBlobNumber.Size = new System.Drawing.Size(71, 20);
            this.numBlobNumber.TabIndex = 85;
            this.numBlobNumber.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numBlobNumber.ValueChanged += new System.EventHandler(this.numBlobNumber_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(228, 45);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 26);
            this.label1.TabIndex = 84;
            this.label1.Text = "Maximize\r\nBlob Number";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 19);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 13);
            this.label3.TabIndex = 88;
            this.label3.Text = "Enable";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // chxEnable
            // 
            this.chxEnable.AutoSize = true;
            this.chxEnable.Location = new System.Drawing.Point(48, 16);
            this.chxEnable.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.chxEnable.Name = "chxEnable";
            this.chxEnable.Size = new System.Drawing.Size(15, 14);
            this.chxEnable.TabIndex = 87;
            this.chxEnable.UseVisualStyleBackColor = true;
            this.chxEnable.CheckedChanged += new System.EventHandler(this.chxEnable_CheckedChanged);
            // 
            // numShowGraphicF
            // 
            this.numShowGraphicF.Location = new System.Drawing.Point(71, 137);
            this.numShowGraphicF.Margin = new System.Windows.Forms.Padding(2);
            this.numShowGraphicF.Maximum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numShowGraphicF.Name = "numShowGraphicF";
            this.numShowGraphicF.Size = new System.Drawing.Size(31, 20);
            this.numShowGraphicF.TabIndex = 90;
            this.numShowGraphicF.ValueChanged += new System.EventHandler(this.numShowGraphic_ValueChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(24, 136);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(44, 26);
            this.label8.TabIndex = 89;
            this.label8.Text = "Show\r\nGraphic";
            this.label8.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.SystemColors.Control;
            this.groupBox1.Controls.Add(this.btnSearchRegionTrayRProperty);
            this.groupBox1.Controls.Add(this.btnSearchRegionTrayFProperty);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.chxEnable);
            this.groupBox1.Controls.Add(this.numShowGraphicR);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.numShowGraphicF);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.btnSearchRegionF);
            this.groupBox1.Controls.Add(this.numMaxSize);
            this.groupBox1.Controls.Add(this.btnSearchRegionR);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.cbxBlobSearchType);
            this.groupBox1.Controls.Add(this.numBlobNumber);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.numMinSize);
            this.groupBox1.Controls.Add(this.numContrastThreshold);
            this.groupBox1.Location = new System.Drawing.Point(2, 2);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.groupBox1.Size = new System.Drawing.Size(384, 225);
            this.groupBox1.TabIndex = 91;
            this.groupBox1.TabStop = false;
            // 
            // btnSearchRegionTrayRProperty
            // 
            this.btnSearchRegionTrayRProperty.Location = new System.Drawing.Point(174, 107);
            this.btnSearchRegionTrayRProperty.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnSearchRegionTrayRProperty.Name = "btnSearchRegionTrayRProperty";
            this.btnSearchRegionTrayRProperty.Size = new System.Drawing.Size(44, 23);
            this.btnSearchRegionTrayRProperty.TabIndex = 101;
            this.btnSearchRegionTrayRProperty.Text = "Edit";
            this.btnSearchRegionTrayRProperty.UseVisualStyleBackColor = true;
            this.btnSearchRegionTrayRProperty.Click += new System.EventHandler(this.btnSearchRegionTrayRProperty_Click);
            // 
            // btnSearchRegionTrayFProperty
            // 
            this.btnSearchRegionTrayFProperty.Location = new System.Drawing.Point(61, 107);
            this.btnSearchRegionTrayFProperty.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnSearchRegionTrayFProperty.Name = "btnSearchRegionTrayFProperty";
            this.btnSearchRegionTrayFProperty.Size = new System.Drawing.Size(44, 23);
            this.btnSearchRegionTrayFProperty.TabIndex = 100;
            this.btnSearchRegionTrayFProperty.Text = "Edit";
            this.btnSearchRegionTrayFProperty.UseVisualStyleBackColor = true;
            this.btnSearchRegionTrayFProperty.Click += new System.EventHandler(this.btnSearchRegionTrayFProperty_Click);
            // 
            // numShowGraphicR
            // 
            this.numShowGraphicR.Location = new System.Drawing.Point(180, 137);
            this.numShowGraphicR.Margin = new System.Windows.Forms.Padding(2);
            this.numShowGraphicR.Maximum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numShowGraphicR.Name = "numShowGraphicR";
            this.numShowGraphicR.Size = new System.Drawing.Size(31, 20);
            this.numShowGraphicR.TabIndex = 95;
            this.numShowGraphicR.ValueChanged += new System.EventHandler(this.numShowGraphicR_ValueChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(130, 136);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(44, 26);
            this.label10.TabIndex = 94;
            this.label10.Text = "Show\r\nGraphic";
            this.label10.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(125, 63);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(47, 39);
            this.label7.TabIndex = 93;
            this.label7.Text = "Search\r\nRegion\r\nReverse";
            this.label7.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // btnSearchRegionF
            // 
            this.btnSearchRegionF.Image = global::Cognex.VS.InspSensor.Properties.Resources.SearchRegion;
            this.btnSearchRegionF.Location = new System.Drawing.Point(61, 58);
            this.btnSearchRegionF.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnSearchRegionF.Name = "btnSearchRegionF";
            this.btnSearchRegionF.Size = new System.Drawing.Size(44, 46);
            this.btnSearchRegionF.TabIndex = 80;
            this.btnSearchRegionF.UseVisualStyleBackColor = true;
            this.btnSearchRegionF.Click += new System.EventHandler(this.btnSearchRegion_Click);
            // 
            // numMaxSize
            // 
            this.numMaxSize.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numMaxSize.Location = new System.Drawing.Point(308, 162);
            this.numMaxSize.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.numMaxSize.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numMaxSize.Name = "numMaxSize";
            this.numMaxSize.Size = new System.Drawing.Size(71, 20);
            this.numMaxSize.TabIndex = 93;
            this.numMaxSize.Value = new decimal(new int[] {
            8000,
            0,
            0,
            0});
            this.numMaxSize.ValueChanged += new System.EventHandler(this.numMaxSize_ValueChanged);
            // 
            // btnSearchRegionR
            // 
            this.btnSearchRegionR.Image = global::Cognex.VS.InspSensor.Properties.Resources.SearchRegion;
            this.btnSearchRegionR.Location = new System.Drawing.Point(174, 58);
            this.btnSearchRegionR.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnSearchRegionR.Name = "btnSearchRegionR";
            this.btnSearchRegionR.Size = new System.Drawing.Size(44, 46);
            this.btnSearchRegionR.TabIndex = 92;
            this.btnSearchRegionR.UseVisualStyleBackColor = true;
            this.btnSearchRegionR.Click += new System.EventHandler(this.btnSearchRegionR_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(246, 167);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(50, 13);
            this.label6.TabIndex = 92;
            this.label6.Text = "Max Size";
            // 
            // btnCancel
            // 
            this.btnCancel.Image = global::Cognex.VS.InspSensor.Properties.Resources.TrainExit;
            this.btnCancel.Location = new System.Drawing.Point(304, 253);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(44, 44);
            this.btnCancel.TabIndex = 86;
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnAccept
            // 
            this.btnAccept.Image = global::Cognex.VS.InspSensor.Properties.Resources.TrainDone;
            this.btnAccept.Location = new System.Drawing.Point(352, 253);
            this.btnAccept.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(44, 44);
            this.btnAccept.TabIndex = 77;
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.btnDone_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(4, 4);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(392, 244);
            this.tabControl1.TabIndex = 92;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tabPage1.Size = new System.Drawing.Size(384, 218);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Tray Region";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.groupBox2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tabPage2.Size = new System.Drawing.Size(384, 218);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Panel on Pocket";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.SystemColors.Control;
            this.groupBox2.Controls.Add(this.btnPanelSearchRegionRProperty);
            this.groupBox2.Controls.Add(this.btnPanelSearchRegionFProperty);
            this.groupBox2.Controls.Add(this.cbxPanelNumb);
            this.groupBox2.Controls.Add(this.label21);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.chxEnablePanelBlob);
            this.groupBox2.Controls.Add(this.numPanelShowGraphicR);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.numPanelShowGraphicF);
            this.groupBox2.Controls.Add(this.label13);
            this.groupBox2.Controls.Add(this.label14);
            this.groupBox2.Controls.Add(this.btnPanelSearchRegionF);
            this.groupBox2.Controls.Add(this.numPanelMaxSize);
            this.groupBox2.Controls.Add(this.btnPanelSearchRegionR);
            this.groupBox2.Controls.Add(this.label15);
            this.groupBox2.Controls.Add(this.cbxPanelBlobSearchType);
            this.groupBox2.Controls.Add(this.numPanelBlobNumb);
            this.groupBox2.Controls.Add(this.label16);
            this.groupBox2.Controls.Add(this.label17);
            this.groupBox2.Controls.Add(this.label18);
            this.groupBox2.Controls.Add(this.label19);
            this.groupBox2.Controls.Add(this.label20);
            this.groupBox2.Controls.Add(this.numPanelMinSize);
            this.groupBox2.Controls.Add(this.numPanelContrast);
            this.groupBox2.Location = new System.Drawing.Point(2, 2);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.groupBox2.Size = new System.Drawing.Size(384, 219);
            this.groupBox2.TabIndex = 92;
            this.groupBox2.TabStop = false;
            // 
            // btnPanelSearchRegionRProperty
            // 
            this.btnPanelSearchRegionRProperty.Location = new System.Drawing.Point(174, 107);
            this.btnPanelSearchRegionRProperty.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnPanelSearchRegionRProperty.Name = "btnPanelSearchRegionRProperty";
            this.btnPanelSearchRegionRProperty.Size = new System.Drawing.Size(44, 23);
            this.btnPanelSearchRegionRProperty.TabIndex = 99;
            this.btnPanelSearchRegionRProperty.Text = "Edit";
            this.btnPanelSearchRegionRProperty.UseVisualStyleBackColor = true;
            this.btnPanelSearchRegionRProperty.Click += new System.EventHandler(this.btnPanelSearchRegionRProperty_Click);
            // 
            // btnPanelSearchRegionFProperty
            // 
            this.btnPanelSearchRegionFProperty.Location = new System.Drawing.Point(61, 107);
            this.btnPanelSearchRegionFProperty.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnPanelSearchRegionFProperty.Name = "btnPanelSearchRegionFProperty";
            this.btnPanelSearchRegionFProperty.Size = new System.Drawing.Size(44, 23);
            this.btnPanelSearchRegionFProperty.TabIndex = 98;
            this.btnPanelSearchRegionFProperty.Text = "Edit";
            this.btnPanelSearchRegionFProperty.UseVisualStyleBackColor = true;
            this.btnPanelSearchRegionFProperty.Click += new System.EventHandler(this.btnPanelSearchRegionFProperty_Click);
            // 
            // cbxPanelNumb
            // 
            this.cbxPanelNumb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxPanelNumb.FormattingEnabled = true;
            this.cbxPanelNumb.Location = new System.Drawing.Point(127, 15);
            this.cbxPanelNumb.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cbxPanelNumb.Name = "cbxPanelNumb";
            this.cbxPanelNumb.Size = new System.Drawing.Size(120, 21);
            this.cbxPanelNumb.TabIndex = 96;
            this.cbxPanelNumb.SelectedIndexChanged += new System.EventHandler(this.cbxPanelNumb_SelectedIndexChanged);
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(92, 20);
            this.label21.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(34, 13);
            this.label21.TabIndex = 97;
            this.label21.Text = "Panel";
            this.label21.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(5, 19);
            this.label11.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(40, 13);
            this.label11.TabIndex = 88;
            this.label11.Text = "Enable";
            this.label11.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // chxEnablePanelBlob
            // 
            this.chxEnablePanelBlob.AutoSize = true;
            this.chxEnablePanelBlob.Location = new System.Drawing.Point(48, 16);
            this.chxEnablePanelBlob.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.chxEnablePanelBlob.Name = "chxEnablePanelBlob";
            this.chxEnablePanelBlob.Size = new System.Drawing.Size(15, 14);
            this.chxEnablePanelBlob.TabIndex = 87;
            this.chxEnablePanelBlob.UseVisualStyleBackColor = true;
            this.chxEnablePanelBlob.CheckedChanged += new System.EventHandler(this.chxEnablePanelBlob_CheckedChanged);
            // 
            // numPanelShowGraphicR
            // 
            this.numPanelShowGraphicR.Location = new System.Drawing.Point(180, 137);
            this.numPanelShowGraphicR.Margin = new System.Windows.Forms.Padding(2);
            this.numPanelShowGraphicR.Maximum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numPanelShowGraphicR.Name = "numPanelShowGraphicR";
            this.numPanelShowGraphicR.Size = new System.Drawing.Size(31, 20);
            this.numPanelShowGraphicR.TabIndex = 95;
            this.numPanelShowGraphicR.ValueChanged += new System.EventHandler(this.numPanelShowGraphicR_ValueChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(130, 136);
            this.label12.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(44, 26);
            this.label12.TabIndex = 94;
            this.label12.Text = "Show\r\nGraphic";
            this.label12.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // numPanelShowGraphicF
            // 
            this.numPanelShowGraphicF.Location = new System.Drawing.Point(71, 137);
            this.numPanelShowGraphicF.Margin = new System.Windows.Forms.Padding(2);
            this.numPanelShowGraphicF.Maximum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numPanelShowGraphicF.Name = "numPanelShowGraphicF";
            this.numPanelShowGraphicF.Size = new System.Drawing.Size(31, 20);
            this.numPanelShowGraphicF.TabIndex = 90;
            this.numPanelShowGraphicF.ValueChanged += new System.EventHandler(this.numPanelShowGraphicF_ValueChanged);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(125, 63);
            this.label13.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(47, 39);
            this.label13.TabIndex = 93;
            this.label13.Text = "Search\r\nRegion\r\nReverse";
            this.label13.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(24, 136);
            this.label14.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(44, 26);
            this.label14.TabIndex = 89;
            this.label14.Text = "Show\r\nGraphic";
            this.label14.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // btnPanelSearchRegionF
            // 
            this.btnPanelSearchRegionF.Image = global::Cognex.VS.InspSensor.Properties.Resources.SearchRegion;
            this.btnPanelSearchRegionF.Location = new System.Drawing.Point(61, 58);
            this.btnPanelSearchRegionF.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnPanelSearchRegionF.Name = "btnPanelSearchRegionF";
            this.btnPanelSearchRegionF.Size = new System.Drawing.Size(44, 46);
            this.btnPanelSearchRegionF.TabIndex = 80;
            this.btnPanelSearchRegionF.UseVisualStyleBackColor = true;
            this.btnPanelSearchRegionF.Click += new System.EventHandler(this.btnPanelSearchRegionF_Click);
            // 
            // numPanelMaxSize
            // 
            this.numPanelMaxSize.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numPanelMaxSize.Location = new System.Drawing.Point(308, 162);
            this.numPanelMaxSize.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.numPanelMaxSize.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numPanelMaxSize.Name = "numPanelMaxSize";
            this.numPanelMaxSize.Size = new System.Drawing.Size(71, 20);
            this.numPanelMaxSize.TabIndex = 93;
            this.numPanelMaxSize.Value = new decimal(new int[] {
            8000,
            0,
            0,
            0});
            this.numPanelMaxSize.ValueChanged += new System.EventHandler(this.numPanelMaxSize_ValueChanged);
            // 
            // btnPanelSearchRegionR
            // 
            this.btnPanelSearchRegionR.Image = global::Cognex.VS.InspSensor.Properties.Resources.SearchRegion;
            this.btnPanelSearchRegionR.Location = new System.Drawing.Point(174, 58);
            this.btnPanelSearchRegionR.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnPanelSearchRegionR.Name = "btnPanelSearchRegionR";
            this.btnPanelSearchRegionR.Size = new System.Drawing.Size(44, 46);
            this.btnPanelSearchRegionR.TabIndex = 92;
            this.btnPanelSearchRegionR.UseVisualStyleBackColor = true;
            this.btnPanelSearchRegionR.Click += new System.EventHandler(this.btnPanelSearchRegionR_Click);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(246, 167);
            this.label15.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(50, 13);
            this.label15.TabIndex = 92;
            this.label15.Text = "Max Size";
            // 
            // cbxPanelBlobSearchType
            // 
            this.cbxPanelBlobSearchType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxPanelBlobSearchType.FormattingEnabled = true;
            this.cbxPanelBlobSearchType.Items.AddRange(new object[] {
            "Dark On Bright",
            "Bright On Dark"});
            this.cbxPanelBlobSearchType.Location = new System.Drawing.Point(71, 179);
            this.cbxPanelBlobSearchType.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cbxPanelBlobSearchType.Name = "cbxPanelBlobSearchType";
            this.cbxPanelBlobSearchType.Size = new System.Drawing.Size(120, 21);
            this.cbxPanelBlobSearchType.TabIndex = 82;
            this.cbxPanelBlobSearchType.SelectedIndexChanged += new System.EventHandler(this.cbxPanelBlobSearchType_SelectedIndexChanged);
            // 
            // numPanelBlobNumb
            // 
            this.numPanelBlobNumb.Location = new System.Drawing.Point(308, 45);
            this.numPanelBlobNumb.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.numPanelBlobNumb.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numPanelBlobNumb.Name = "numPanelBlobNumb";
            this.numPanelBlobNumb.Size = new System.Drawing.Size(71, 20);
            this.numPanelBlobNumb.TabIndex = 85;
            this.numPanelBlobNumb.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numPanelBlobNumb.ValueChanged += new System.EventHandler(this.numPanelBlobNumb_ValueChanged);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(36, 179);
            this.label16.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(31, 26);
            this.label16.TabIndex = 83;
            this.label16.Text = "Blob\r\nType";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(228, 45);
            this.label17.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(68, 26);
            this.label17.TabIndex = 84;
            this.label17.Text = "Maximize\r\nBlob Number";
            this.label17.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(12, 65);
            this.label18.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(45, 39);
            this.label18.TabIndex = 81;
            this.label18.Text = "Search\r\nRegion\r\nForward";
            this.label18.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(248, 128);
            this.label19.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(47, 13);
            this.label19.TabIndex = 69;
            this.label19.Text = "Min Size";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(242, 84);
            this.label20.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(54, 26);
            this.label20.TabIndex = 67;
            this.label20.Text = "Contrast\r\nThreshold";
            this.label20.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // numPanelMinSize
            // 
            this.numPanelMinSize.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numPanelMinSize.Location = new System.Drawing.Point(308, 124);
            this.numPanelMinSize.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.numPanelMinSize.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numPanelMinSize.Name = "numPanelMinSize";
            this.numPanelMinSize.Size = new System.Drawing.Size(71, 20);
            this.numPanelMinSize.TabIndex = 70;
            this.numPanelMinSize.Value = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.numPanelMinSize.ValueChanged += new System.EventHandler(this.numPanelMinSize_ValueChanged);
            // 
            // numPanelContrast
            // 
            this.numPanelContrast.DecimalPlaces = 1;
            this.numPanelContrast.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.numPanelContrast.Location = new System.Drawing.Point(308, 87);
            this.numPanelContrast.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.numPanelContrast.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numPanelContrast.Name = "numPanelContrast";
            this.numPanelContrast.Size = new System.Drawing.Size(71, 20);
            this.numPanelContrast.TabIndex = 68;
            this.numPanelContrast.Value = new decimal(new int[] {
            150,
            0,
            0,
            65536});
            this.numPanelContrast.ValueChanged += new System.EventHandler(this.numPanelContrast_ValueChanged);
            // 
            // TrayBlobCheck
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnAccept);
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "TrayBlobCheck";
            this.Size = new System.Drawing.Size(400, 300);
            ((System.ComponentModel.ISupportInitialize)(this.numMinSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numContrastThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBlobNumber)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numShowGraphicF)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numShowGraphicR)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxSize)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPanelShowGraphicR)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPanelShowGraphicF)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPanelMaxSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPanelBlobNumb)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPanelMinSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPanelContrast)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button btnSearchRegionF;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.NumericUpDown numMinSize;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numContrastThreshold;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbxBlobSearchType;
        private System.Windows.Forms.NumericUpDown numBlobNumber;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chxEnable;
        private System.Windows.Forms.NumericUpDown numShowGraphicF;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown numMaxSize;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnSearchRegionR;
        private System.Windows.Forms.NumericUpDown numShowGraphicR;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.CheckBox chxEnablePanelBlob;
        private System.Windows.Forms.NumericUpDown numPanelShowGraphicR;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.NumericUpDown numPanelShowGraphicF;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Button btnPanelSearchRegionF;
        private System.Windows.Forms.NumericUpDown numPanelMaxSize;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.ComboBox cbxPanelBlobSearchType;
        private System.Windows.Forms.NumericUpDown numPanelBlobNumb;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.NumericUpDown numPanelMinSize;
        private System.Windows.Forms.NumericUpDown numPanelContrast;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button btnPanelSearchRegionR;
        private System.Windows.Forms.ComboBox cbxPanelNumb;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Button btnPanelSearchRegionFProperty;
        private System.Windows.Forms.Button btnPanelSearchRegionRProperty;
        private System.Windows.Forms.Button btnSearchRegionTrayRProperty;
        private System.Windows.Forms.Button btnSearchRegionTrayFProperty;
    }
}
