namespace TestApp
{
    partial class SelectCamera
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnConnectMaster = new System.Windows.Forms.Button();
            this.btnConnectSlave = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.lblMasterCam = new System.Windows.Forms.Label();
            this.lblSlaveCam = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnConnectMaster
            // 
            this.btnConnectMaster.Enabled = false;
            this.btnConnectMaster.Location = new System.Drawing.Point(13, 92);
            this.btnConnectMaster.Name = "btnConnectMaster";
            this.btnConnectMaster.Size = new System.Drawing.Size(115, 61);
            this.btnConnectMaster.TabIndex = 0;
            this.btnConnectMaster.Text = "Connect Master";
            this.btnConnectMaster.UseVisualStyleBackColor = true;
            this.btnConnectMaster.Click += new System.EventHandler(this.btnConnectMaster_Click);
            // 
            // btnConnectSlave
            // 
            this.btnConnectSlave.Enabled = false;
            this.btnConnectSlave.Location = new System.Drawing.Point(201, 92);
            this.btnConnectSlave.Name = "btnConnectSlave";
            this.btnConnectSlave.Size = new System.Drawing.Size(115, 61);
            this.btnConnectSlave.TabIndex = 1;
            this.btnConnectSlave.Text = "Connect Slave";
            this.btnConnectSlave.UseVisualStyleBackColor = true;
            this.btnConnectSlave.Click += new System.EventHandler(this.btnConnectSlave_Click);
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(389, 92);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(115, 61);
            this.btnExit.TabIndex = 2;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // lblMasterCam
            // 
            this.lblMasterCam.AutoSize = true;
            this.lblMasterCam.Location = new System.Drawing.Point(13, 57);
            this.lblMasterCam.Name = "lblMasterCam";
            this.lblMasterCam.Size = new System.Drawing.Size(72, 12);
            this.lblMasterCam.TabIndex = 3;
            this.lblMasterCam.Text = "Master: null";
            // 
            // lblSlaveCam
            // 
            this.lblSlaveCam.AutoSize = true;
            this.lblSlaveCam.Location = new System.Drawing.Point(204, 57);
            this.lblSlaveCam.Name = "lblSlaveCam";
            this.lblSlaveCam.Size = new System.Drawing.Size(64, 12);
            this.lblSlaveCam.TabIndex = 4;
            this.lblSlaveCam.Text = "Slave: null";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(162, 12);
            this.label1.TabIndex = 5;
            this.label1.Text = "Select camera to connect...";
            // 
            // SelectCamera
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(517, 174);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblSlaveCam);
            this.Controls.Add(this.lblMasterCam);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnConnectSlave);
            this.Controls.Add(this.btnConnectMaster);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "SelectCamera";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SelectCamera";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnConnectMaster;
        private System.Windows.Forms.Button btnConnectSlave;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Label lblMasterCam;
        private System.Windows.Forms.Label lblSlaveCam;
        private System.Windows.Forms.Label label1;
    }
}