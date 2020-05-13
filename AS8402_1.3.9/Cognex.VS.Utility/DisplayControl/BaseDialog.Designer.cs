namespace Cognex.VS.Utility
{
    partial class BaseDialog
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.mBtnOK = new System.Windows.Forms.Button();
            this.mBtnReset = new System.Windows.Forms.Button();
            this.mBtnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
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
            this.mBtnOK.Click += new System.EventHandler(this.MBtnOK_Click);
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
            this.mBtnReset.Click += new System.EventHandler(this.mBtnReset_Click);
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
            this.mBtnCancel.Click += new System.EventHandler(this.MBtnCancel_Click);
            // 
            // BaseDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(933, 415);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BaseDialog";
            this.ShowInTaskbar = false;
            this.Text = "BaseDialog";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        protected System.Windows.Forms.Panel panel1;
        protected System.Windows.Forms.Button mBtnReset;
        protected System.Windows.Forms.Button mBtnOK;
        protected System.Windows.Forms.Button mBtnCancel;
    }
}