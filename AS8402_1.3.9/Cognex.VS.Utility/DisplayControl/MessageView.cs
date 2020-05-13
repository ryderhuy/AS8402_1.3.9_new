using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Cognex.VS.Utility.DisplayControl
{
    public partial class MessageView : UserControl
    {
        private int mMessageBufferSize = 1000;
        private List<ListViewItem> mNewMessages;
        private Timer mUpdateTimer;

        private ContentObserver mLogObserver = null;

        public MessageView()
        {
            InitializeComponent();
            mNewMessages = new List<ListViewItem>();

            mLogObserver = new ContentObserver();
            ContentQueue.gOnly.RegisterObserver(mLogObserver);
            mLogObserver.OnNewContentEvent += MLogObserver_OnNewContentEvent;

            mUpdateTimer = new Timer();
            mUpdateTimer.Interval = 1000;
            mUpdateTimer.Tick += updateTimer_Tick;
            mUpdateTimer.Enabled = true;
        }

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

        private void MBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<object> args = e.Argument as List<object>;

            string path2File = args[0] as String;
            ListView.ListViewItemCollection items = args[1] as ListView.ListViewItemCollection;
            BackgroundWorker worker = sender as BackgroundWorker;
            using (StreamWriter sw = new StreamWriter(path2File, false))
            {
                foreach (ListViewItem item in items)
                {
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }
                    else
                    {
                        string message = item.SubItems[0].Text;
                        sw.WriteLine(sw);
                    }
                }
            }
        }

        private void MBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string result = e.Result.ToString();
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (e.Cancelled)
            {

            }
            else
            {
                MessageBox.Show("Save File Done", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        void updateTimer_Tick(object sender, EventArgs e)
        {
            if (!mMessageListView.IsDisposed)
            {
                mMessageListView.BeginUpdate();
                mMessageListView.SuspendLayout();

                bool scrollToBottom = false;

                lock (mNewMessages)
                {
                    foreach (ListViewItem newMsg in mNewMessages)
                    {
                        if (mMessageListView.Items.Count > 0 &&
                            mMessageListView.Items[mMessageListView.Items.Count - 1].Bounds.IntersectsWith(mMessageListView.ClientRectangle))
                        {
                            scrollToBottom = true;
                        }

                        //mMessageListView.Items.Add(newMsg);
                        mMessageListView.Items.Insert(0, newMsg);

                        if (mMessageListView.Items.Count > mMessageBufferSize)
                        {
                            mMessageListView.Items.RemoveAt(0);
                        }
                    }

                    mNewMessages.Clear();
                }

                //mMessageListView.ResumeLayout();
                mMessageListView.EndUpdate();

                if (scrollToBottom)
                    this.ScrollToBottom();
            }
        }

        public void ScrollToBottom()
        {
            // Swallow any exceptions that come up as a result of scrolling to see the most recent message.
            try
            {
                mMessageListView.Items[mMessageListView.Items.Count - 1].EnsureVisible();
            }
            catch { }
        }

        private void MLogObserver_OnNewContentEvent(object sender, EventArgs e)
        {
            ContentEventArgs args = e as ContentEventArgs;
            if (args != null)
            {
                ListViewItem newMsg = new ListViewItem();
                newMsg.Text = args.Content.TimeStamp.ToString("yyyy/MM/dd HH:mm:ss.ffff");
                newMsg.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = args.Content.Message });

                if (args.Content.MessageType == MessageType.Alarm)
                    newMsg.BackColor = Color.Red;
                else if (args.Content.MessageType == MessageType.Inform)
                    newMsg.BackColor = Color.White;

                lock (mNewMessages)
                {
                    mNewMessages.Add(newMsg);
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            mMessageListView.Items.Clear();
        }

        private void BtnSaveLogFile_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.InitialDirectory = @"C:\";
                saveFileDialog.Title = "Save Log File";
                saveFileDialog.DefaultExt = "bmp";
                saveFileDialog.Filter = "Log files (*.txt)|*.txt";
                saveFileDialog.OverwritePrompt = true;
                saveFileDialog.RestoreDirectory = true;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string path2File = saveFileDialog.FileName;
                    using (StreamWriter sw = new StreamWriter(path2File, false))
                    {
                        foreach (ListViewItem item in mMessageListView.Items)
                        {
                            string message = String.Format("{0},{1}", item.SubItems[0].Text, item.SubItems[1].Text);
                            sw.WriteLine(message);
                        }
                    }
                }
            }
        }
    }
}
