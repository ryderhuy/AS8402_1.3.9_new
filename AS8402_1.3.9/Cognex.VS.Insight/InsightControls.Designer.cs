using System;
using System.Windows.Forms;

namespace Cognex.VS.InSightControl
{
    partial class InsightControls
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.cvsInSightDisplay1 = new Cognex.InSight.Controls.Display.CvsInSightDisplay();
            this.SuspendLayout();
            // 
            // cvsInSightDisplay1
            // 
            this.cvsInSightDisplay1.DefaultTextScaleMode = Cognex.InSight.Controls.Display.CvsInSightDisplay.TextScaleModeType.Proportional;
            this.cvsInSightDisplay1.DialogIcon = null;
            this.cvsInSightDisplay1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cvsInSightDisplay1.Location = new System.Drawing.Point(0, 0);
            this.cvsInSightDisplay1.Name = "cvsInSightDisplay1";
            this.cvsInSightDisplay1.PreferredCropScaleMode = Cognex.InSight.Controls.Display.CvsInSightDisplayCropScaleMode.Default;
            this.cvsInSightDisplay1.Size = new System.Drawing.Size(690, 486);
            this.cvsInSightDisplay1.TabIndex = 0;
            this.cvsInSightDisplay1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cvsInSightDisplay1_KeyDown);
            this.cvsInSightDisplay1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.cvsInSightDisplay1_MouseDoubleClick);
            this.cvsInSightDisplay1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.cvsInSightDisplay1_MouseDown);
            this.cvsInSightDisplay1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.cvsInSightDisplay1_MouseMove);
            this.cvsInSightDisplay1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.cvsInSightDisplay1_MouseUp);
            this.cvsInSightDisplay1.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.cvsInSightDisplay1_MouseWheel);
            // 
            // InsightControls
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cvsInSightDisplay1);
            this.Name = "InsightControls";
            this.Size = new System.Drawing.Size(690, 486);
            this.ResumeLayout(false);

        }


        #endregion

        private InSight.Controls.Display.CvsInSightDisplay cvsInSightDisplay1;
    }
}
