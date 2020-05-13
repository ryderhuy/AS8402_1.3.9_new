namespace Cognex.VS.InspSensor.CustomControls
{
    partial class IntrisicCalibration
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
            this.label16 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.numSizeY = new System.Windows.Forms.NumericUpDown();
            this.numSizeX = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.numShowGraphic = new System.Windows.Forms.NumericUpDown();
            this.btnMaximizeRegion = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cbxUnit = new System.Windows.Forms.ComboBox();
            this.btnTrainRegion = new System.Windows.Forms.Button();
            this.btnInstrinsicExport = new System.Windows.Forms.Button();
            this.btnInstrinsicImport = new System.Windows.Forms.Button();
            this.btnCalcIntrisic = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnAccept = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numSizeY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSizeX)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numShowGraphic)).BeginInit();
            this.SuspendLayout();
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(326, 220);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(77, 34);
            this.label16.TabIndex = 110;
            this.label16.Text = "Execute\r\nCalculation";
            this.label16.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(59, 141);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(48, 17);
            this.label5.TabIndex = 106;
            this.label5.Text = "Y Size";
            this.label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(59, 101);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 17);
            this.label4.TabIndex = 105;
            this.label4.Text = "X Size";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // numSizeY
            // 
            this.numSizeY.DecimalPlaces = 1;
            this.numSizeY.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.numSizeY.Location = new System.Drawing.Point(114, 139);
            this.numSizeY.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.numSizeY.Name = "numSizeY";
            this.numSizeY.Size = new System.Drawing.Size(67, 22);
            this.numSizeY.TabIndex = 97;
            this.numSizeY.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numSizeY.ValueChanged += new System.EventHandler(this.numSizeY_ValueChanged);
            // 
            // numSizeX
            // 
            this.numSizeX.DecimalPlaces = 1;
            this.numSizeX.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.numSizeX.Location = new System.Drawing.Point(114, 99);
            this.numSizeX.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.numSizeX.Name = "numSizeX";
            this.numSizeX.Size = new System.Drawing.Size(67, 22);
            this.numSizeX.TabIndex = 96;
            this.numSizeX.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numSizeX.ValueChanged += new System.EventHandler(this.numSizeX_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(326, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 34);
            this.label1.TabIndex = 95;
            this.label1.Text = "Calculated\r\nRegion";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.numShowGraphic);
            this.groupBox1.Controls.Add(this.btnMaximizeRegion);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.cbxUnit);
            this.groupBox1.Controls.Add(this.label16);
            this.groupBox1.Controls.Add(this.btnTrainRegion);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.btnInstrinsicExport);
            this.groupBox1.Controls.Add(this.btnInstrinsicImport);
            this.groupBox1.Controls.Add(this.btnCalcIntrisic);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.numSizeX);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.numSizeY);
            this.groupBox1.Location = new System.Drawing.Point(3, 4);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Size = new System.Drawing.Size(526, 299);
            this.groupBox1.TabIndex = 113;
            this.groupBox1.TabStop = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(50, 185);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(58, 34);
            this.label6.TabIndex = 116;
            this.label6.Text = "Show\r\nGraphic";
            this.label6.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // numShowGraphic
            // 
            this.numShowGraphic.Location = new System.Drawing.Point(114, 186);
            this.numShowGraphic.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.numShowGraphic.Maximum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numShowGraphic.Name = "numShowGraphic";
            this.numShowGraphic.Size = new System.Drawing.Size(67, 22);
            this.numShowGraphic.TabIndex = 115;
            this.numShowGraphic.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numShowGraphic.ValueChanged += new System.EventHandler(this.numShowGraphic_ValueChanged);
            // 
            // btnMaximizeRegion
            // 
            this.btnMaximizeRegion.Image = global::Cognex.VS.InspSensor.Properties.Resources.MaximizeRegion;
            this.btnMaximizeRegion.Location = new System.Drawing.Point(415, 127);
            this.btnMaximizeRegion.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnMaximizeRegion.Name = "btnMaximizeRegion";
            this.btnMaximizeRegion.Size = new System.Drawing.Size(59, 56);
            this.btnMaximizeRegion.TabIndex = 113;
            this.btnMaximizeRegion.UseVisualStyleBackColor = true;
            this.btnMaximizeRegion.Click += new System.EventHandler(this.btnMaximizeRegion_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(331, 132);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 34);
            this.label3.TabIndex = 114;
            this.label3.Text = "Maximize\r\nRegion";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(72, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 17);
            this.label2.TabIndex = 112;
            this.label2.Text = "Unit";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // cbxUnit
            // 
            this.cbxUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxUnit.FormattingEnabled = true;
            this.cbxUnit.Items.AddRange(new object[] {
            "Microns",
            "Milimeters",
            "Centimeters",
            "Inches"});
            this.cbxUnit.Location = new System.Drawing.Point(114, 52);
            this.cbxUnit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbxUnit.Name = "cbxUnit";
            this.cbxUnit.Size = new System.Drawing.Size(138, 24);
            this.cbxUnit.TabIndex = 111;
            this.cbxUnit.SelectedIndexChanged += new System.EventHandler(this.cbxUnit_SelectedIndexChanged);
            // 
            // btnTrainRegion
            // 
            this.btnTrainRegion.Image = global::Cognex.VS.InspSensor.Properties.Resources.TrainRegion;
            this.btnTrainRegion.Location = new System.Drawing.Point(415, 40);
            this.btnTrainRegion.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnTrainRegion.Name = "btnTrainRegion";
            this.btnTrainRegion.Size = new System.Drawing.Size(59, 56);
            this.btnTrainRegion.TabIndex = 94;
            this.btnTrainRegion.UseVisualStyleBackColor = true;
            this.btnTrainRegion.Click += new System.EventHandler(this.btnTrainRegion_Click);
            // 
            // btnInstrinsicExport
            // 
            this.btnInstrinsicExport.Location = new System.Drawing.Point(155, 246);
            this.btnInstrinsicExport.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnInstrinsicExport.Name = "btnInstrinsicExport";
            this.btnInstrinsicExport.Size = new System.Drawing.Size(97, 45);
            this.btnInstrinsicExport.TabIndex = 103;
            this.btnInstrinsicExport.Text = "Export";
            this.btnInstrinsicExport.UseVisualStyleBackColor = true;
            this.btnInstrinsicExport.Click += new System.EventHandler(this.btnInstrinsicExport_Click);
            // 
            // btnInstrinsicImport
            // 
            this.btnInstrinsicImport.Location = new System.Drawing.Point(52, 246);
            this.btnInstrinsicImport.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnInstrinsicImport.Name = "btnInstrinsicImport";
            this.btnInstrinsicImport.Size = new System.Drawing.Size(97, 45);
            this.btnInstrinsicImport.TabIndex = 103;
            this.btnInstrinsicImport.Text = "Import";
            this.btnInstrinsicImport.UseVisualStyleBackColor = true;
            this.btnInstrinsicImport.Click += new System.EventHandler(this.btnInstrinsicImport_Click);
            // 
            // btnCalcIntrisic
            // 
            this.btnCalcIntrisic.Image = global::Cognex.VS.InspSensor.Properties.Resources.Intrisic1;
            this.btnCalcIntrisic.Location = new System.Drawing.Point(415, 215);
            this.btnCalcIntrisic.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnCalcIntrisic.Name = "btnCalcIntrisic";
            this.btnCalcIntrisic.Size = new System.Drawing.Size(59, 56);
            this.btnCalcIntrisic.TabIndex = 103;
            this.btnCalcIntrisic.UseVisualStyleBackColor = true;
            this.btnCalcIntrisic.Click += new System.EventHandler(this.btnCalcIntrisic_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Image = global::Cognex.VS.InspSensor.Properties.Resources.TrainExit;
            this.btnCancel.Location = new System.Drawing.Point(410, 311);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(59, 56);
            this.btnCancel.TabIndex = 112;
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnAccept
            // 
            this.btnAccept.Image = global::Cognex.VS.InspSensor.Properties.Resources.TrainDone;
            this.btnAccept.Location = new System.Drawing.Point(470, 311);
            this.btnAccept.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(59, 56);
            this.btnAccept.TabIndex = 111;
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.btnDone_Click);
            // 
            // IntrisicCalibration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnAccept);
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "IntrisicCalibration";
            this.Size = new System.Drawing.Size(534, 369);
            ((System.ComponentModel.ISupportInitialize)(this.numSizeY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSizeX)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numShowGraphic)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnCalcIntrisic;
        private System.Windows.Forms.NumericUpDown numSizeY;
        private System.Windows.Forms.NumericUpDown numSizeX;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnTrainRegion;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbxUnit;
        private System.Windows.Forms.Button btnMaximizeRegion;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numShowGraphic;
        private System.Windows.Forms.Button btnInstrinsicExport;
        private System.Windows.Forms.Button btnInstrinsicImport;
    }
}
