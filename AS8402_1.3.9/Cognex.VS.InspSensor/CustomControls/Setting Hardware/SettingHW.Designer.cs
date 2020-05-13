namespace Cognex.VS.InspSensor.CustomControls.Setting_Hardware
{
    partial class SettingHW
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtCameraAddress = new System.Windows.Forms.TextBox();
            this.btnSaveHW = new System.Windows.Forms.Button();
            this.btnFactoryResetHW = new System.Windows.Forms.Button();
            this.btnResetHW = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnAccept = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnTwoHEUpdate = new System.Windows.Forms.Button();
            this.btnUpdateNoF = new System.Windows.Forms.Button();
            this.btnUpdateOCA = new System.Windows.Forms.Button();
            this.btnOptimizeTools = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.txtCameraAddress);
            this.groupBox1.Controls.Add(this.btnSaveHW);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(394, 113);
            this.groupBox1.TabIndex = 114;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "SLMP Scanner";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 24);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(91, 13);
            this.label9.TabIndex = 135;
            this.label9.Text = "Camera IP Adress";
            // 
            // txtCameraAddress
            // 
            this.txtCameraAddress.Enabled = false;
            this.txtCameraAddress.Location = new System.Drawing.Point(117, 18);
            this.txtCameraAddress.Name = "txtCameraAddress";
            this.txtCameraAddress.Size = new System.Drawing.Size(127, 20);
            this.txtCameraAddress.TabIndex = 134;
            this.txtCameraAddress.TextChanged += new System.EventHandler(this.txtCameraAddress_TextChanged);
            // 
            // btnSaveHW
            // 
            this.btnSaveHW.Location = new System.Drawing.Point(5, 59);
            this.btnSaveHW.Name = "btnSaveHW";
            this.btnSaveHW.Size = new System.Drawing.Size(114, 37);
            this.btnSaveHW.TabIndex = 117;
            this.btnSaveHW.Text = "Setting Networks";
            this.btnSaveHW.UseVisualStyleBackColor = true;
            this.btnSaveHW.Click += new System.EventHandler(this.btnSaveHW_Click);
            // 
            // btnFactoryResetHW
            // 
            this.btnFactoryResetHW.Enabled = false;
            this.btnFactoryResetHW.Location = new System.Drawing.Point(301, 69);
            this.btnFactoryResetHW.Name = "btnFactoryResetHW";
            this.btnFactoryResetHW.Size = new System.Drawing.Size(88, 37);
            this.btnFactoryResetHW.TabIndex = 103;
            this.btnFactoryResetHW.Text = "Factory Reset";
            this.btnFactoryResetHW.UseVisualStyleBackColor = true;
            this.btnFactoryResetHW.Click += new System.EventHandler(this.btnFactoryResetHW_Click);
            // 
            // btnResetHW
            // 
            this.btnResetHW.Location = new System.Drawing.Point(300, 22);
            this.btnResetHW.Name = "btnResetHW";
            this.btnResetHW.Size = new System.Drawing.Size(89, 37);
            this.btnResetHW.TabIndex = 103;
            this.btnResetHW.Text = "Reset Camera";
            this.btnResetHW.UseVisualStyleBackColor = true;
            this.btnResetHW.Click += new System.EventHandler(this.btnResetHW_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Image = global::Cognex.VS.InspSensor.Properties.Resources.TrainExit;
            this.btnCancel.Location = new System.Drawing.Point(307, 252);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(45, 46);
            this.btnCancel.TabIndex = 116;
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnAccept
            // 
            this.btnAccept.Image = global::Cognex.VS.InspSensor.Properties.Resources.TrainDone;
            this.btnAccept.Location = new System.Drawing.Point(351, 252);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(45, 46);
            this.btnAccept.TabIndex = 115;
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.btnAccept_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnTwoHEUpdate);
            this.groupBox2.Controls.Add(this.btnUpdateNoF);
            this.groupBox2.Controls.Add(this.btnUpdateOCA);
            this.groupBox2.Controls.Add(this.btnOptimizeTools);
            this.groupBox2.Controls.Add(this.btnResetHW);
            this.groupBox2.Controls.Add(this.btnFactoryResetHW);
            this.groupBox2.Location = new System.Drawing.Point(3, 122);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(394, 124);
            this.groupBox2.TabIndex = 136;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Hardware Control";
            // 
            // btnTwoHEUpdate
            // 
            this.btnTwoHEUpdate.Location = new System.Drawing.Point(125, 23);
            this.btnTwoHEUpdate.Name = "btnTwoHEUpdate";
            this.btnTwoHEUpdate.Size = new System.Drawing.Size(114, 36);
            this.btnTwoHEUpdate.TabIndex = 120;
            this.btnTwoHEUpdate.Text = "Two HE SLMP";
            this.btnTwoHEUpdate.UseVisualStyleBackColor = true;
            this.btnTwoHEUpdate.Click += new System.EventHandler(this.btnTwoHEUpdate_Click);
            // 
            // btnUpdateNoF
            // 
            this.btnUpdateNoF.Location = new System.Drawing.Point(125, 70);
            this.btnUpdateNoF.Name = "btnUpdateNoF";
            this.btnUpdateNoF.Size = new System.Drawing.Size(114, 36);
            this.btnUpdateNoF.TabIndex = 119;
            this.btnUpdateNoF.Text = "Update Found Number";
            this.btnUpdateNoF.UseVisualStyleBackColor = true;
            this.btnUpdateNoF.Click += new System.EventHandler(this.btnUpdateNoF_Click);
            // 
            // btnUpdateOCA
            // 
            this.btnUpdateOCA.Location = new System.Drawing.Point(5, 69);
            this.btnUpdateOCA.Name = "btnUpdateOCA";
            this.btnUpdateOCA.Size = new System.Drawing.Size(114, 36);
            this.btnUpdateOCA.TabIndex = 118;
            this.btnUpdateOCA.Text = "Update OCA";
            this.btnUpdateOCA.UseVisualStyleBackColor = true;
            this.btnUpdateOCA.Click += new System.EventHandler(this.btnUpdateOCA_Click);
            // 
            // btnOptimizeTools
            // 
            this.btnOptimizeTools.Location = new System.Drawing.Point(5, 23);
            this.btnOptimizeTools.Name = "btnOptimizeTools";
            this.btnOptimizeTools.Size = new System.Drawing.Size(114, 36);
            this.btnOptimizeTools.TabIndex = 117;
            this.btnOptimizeTools.Text = "Optimize PM Tools";
            this.btnOptimizeTools.UseVisualStyleBackColor = true;
            this.btnOptimizeTools.Click += new System.EventHandler(this.btnOptimizeTools_Click);
            // 
            // SettingHW
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnAccept);
            this.Controls.Add(this.groupBox1);
            this.Name = "SettingHW";
            this.Size = new System.Drawing.Size(399, 306);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnFactoryResetHW;
        private System.Windows.Forms.Button btnResetHW;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.Button btnSaveHW;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtCameraAddress;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnOptimizeTools;
        private System.Windows.Forms.Button btnUpdateOCA;
        private System.Windows.Forms.Button btnUpdateNoF;
        private System.Windows.Forms.Button btnTwoHEUpdate;
    }
}
