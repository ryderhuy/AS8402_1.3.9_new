namespace Cognex.VS.Utility.DisplayControl
{
    partial class MessageView
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
            this.mMessageListView = new System.Windows.Forms.ListView();
            this.columnTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnMessage = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnClear = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSaveLogFile = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // mMessageListView
            // 
            this.mMessageListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnTime,
            this.columnMessage});
            this.mMessageListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mMessageListView.Location = new System.Drawing.Point(0, 0);
            this.mMessageListView.Name = "mMessageListView";
            this.mMessageListView.Size = new System.Drawing.Size(832, 265);
            this.mMessageListView.TabIndex = 0;
            this.mMessageListView.UseCompatibleStateImageBehavior = false;
            this.mMessageListView.View = System.Windows.Forms.View.Details;
            // 
            // columnTime
            // 
            this.columnTime.Tag = "Time";
            this.columnTime.Text = "Time";
            this.columnTime.Width = 200;
            // 
            // columnMessage
            // 
            this.columnMessage.Tag = "Message";
            this.columnMessage.Text = "Message";
            this.columnMessage.Width = 400;
            // 
            // btnClear
            // 
            this.btnClear.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClear.Location = new System.Drawing.Point(745, 0);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(87, 25);
            this.btnClear.TabIndex = 1;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
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
            this.splitContainer1.Panel1.Controls.Add(this.btnSaveLogFile);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.btnClear);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.mMessageListView);
            this.splitContainer1.Size = new System.Drawing.Size(832, 294);
            this.splitContainer1.SplitterDistance = 25;
            this.splitContainer1.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 25);
            this.label1.TabIndex = 2;
            this.label1.Text = "Log";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnSaveLogFile
            // 
            this.btnSaveLogFile.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSaveLogFile.Location = new System.Drawing.Point(658, 0);
            this.btnSaveLogFile.Name = "btnSaveLogFile";
            this.btnSaveLogFile.Size = new System.Drawing.Size(87, 25);
            this.btnSaveLogFile.TabIndex = 3;
            this.btnSaveLogFile.Text = "Save Log";
            this.btnSaveLogFile.UseVisualStyleBackColor = true;
            this.btnSaveLogFile.Click += new System.EventHandler(this.BtnSaveLogFile_Click);
            // 
            // MessageView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "MessageView";
            this.Size = new System.Drawing.Size(832, 294);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView mMessageListView;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ColumnHeader columnTime;
        private System.Windows.Forms.ColumnHeader columnMessage;
        private System.Windows.Forms.Button btnSaveLogFile;
    }
}
