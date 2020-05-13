namespace MessageManager
{
    partial class MessageViewerGUI
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
            this.listview_messageManager = new System.Windows.Forms.ListView();
            this.Time = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Message = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // listview_messageManager
            // 
            this.listview_messageManager.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Time,
            this.Message});
            this.listview_messageManager.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listview_messageManager.HideSelection = false;
            this.listview_messageManager.Location = new System.Drawing.Point(0, 0);
            this.listview_messageManager.Name = "listview_messageManager";
            this.listview_messageManager.Size = new System.Drawing.Size(726, 315);
            this.listview_messageManager.TabIndex = 0;
            this.listview_messageManager.UseCompatibleStateImageBehavior = false;
            this.listview_messageManager.View = System.Windows.Forms.View.Details;
            this.listview_messageManager.SizeChanged += new System.EventHandler(this.listview_messageManager_SizeChanged);
            // 
            // Time
            // 
            this.Time.Text = "Time";
            this.Time.Width = 109;
            // 
            // Message
            // 
            this.Message.Text = "Message";
            this.Message.Width = 619;
            // 
            // MessageViewerGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listview_messageManager);
            this.Name = "MessageViewerGUI";
            this.Size = new System.Drawing.Size(726, 315);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listview_messageManager;
        private System.Windows.Forms.ColumnHeader Time;
        private System.Windows.Forms.ColumnHeader Message;
    }
}
