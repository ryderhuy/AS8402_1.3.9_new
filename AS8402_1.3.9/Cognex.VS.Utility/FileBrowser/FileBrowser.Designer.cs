namespace Cognex.VS.Utility
{
    partial class FileBrowser
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
            this.treeViewList = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // treeViewList
            // 
            this.treeViewList.Location = new System.Drawing.Point(3, 3);
            this.treeViewList.Name = "treeViewList";
            this.treeViewList.Size = new System.Drawing.Size(512, 446);
            this.treeViewList.TabIndex = 0;
            this.treeViewList.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeViewList_BeforeExpand);
            // 
            // FileBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.treeViewList);
            this.Name = "FileBrowser";
            this.Size = new System.Drawing.Size(518, 452);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treeViewList;
    }
}
