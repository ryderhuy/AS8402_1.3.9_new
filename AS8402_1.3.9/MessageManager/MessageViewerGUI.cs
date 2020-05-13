using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel.Design;

namespace MessageManager
{
    [Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(IDesigner))]
    public partial class MessageViewerGUI : UserControl
    {
        int m_messageBufferSize = 1000;
        List<ListViewItem> newMessages;
        Timer updateTimer;

        public MessageViewerGUI()
        {
            InitializeComponent();

            updateTimer = new Timer();
            updateTimer.Interval = 100;
            updateTimer.Tick += updateTimer_Tick;
            updateTimer.Enabled = true;

            newMessages = new List<ListViewItem>();


            if (!DesignMode && MessageLoggerManager.Log != null)
            {
                MessageLoggerManager.Log.NewMessageReceived += new MessageLoggerManager.NewMessageReceivedEventHandler(gOnly_NewMessageReceived);
                foreach (MessageContentAndInfo msg in MessageLoggerManager.Log.Messages)
                {
                    gOnly_NewMessageReceived(null, msg);
                }
            }

            listview_messageManager_SizeChanged(null, null);

        }

        public void ScrollToBottom()
        {
            // Swallow any exceptions that come up as a result of scrolling to see the most recent message.
            try
            {
                listview_messageManager.Items[listview_messageManager.Items.Count - 1].EnsureVisible();
            }
            catch { }
        }

        private delegate void gOnly_NewMessageReceivedDelegate(object sender, MessageContentAndInfo args);
        void gOnly_NewMessageReceived(object sender, MessageContentAndInfo args)
        {
            ListViewItem newMsg = new ListViewItem(args.MessageTime.ToString("HH:mm:ss.fff"));
            newMsg.SubItems.Add(new ListViewItem.ListViewSubItem(newMsg, args.Message));

            if (args.MessageType == MessageType.Alarm)
                newMsg.BackColor = Color.Red;
            else if (args.MessageType == MessageType.Warning)
                newMsg.BackColor = Color.Yellow;
            else if (args.MessageType == MessageType.Info)
                newMsg.BackColor = Color.White;
            else if (args.MessageType == MessageType.OK)
                newMsg.BackColor = Color.Lime;
            else if (args.MessageType == MessageType.NG)
                newMsg.BackColor = Color.DarkRed;
            lock (newMessages)
            {
                newMessages.Add(newMsg);
            }
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            if (!listview_messageManager.IsDisposed)

            {
                //listview_messageManager.BeginUpdate();
                //listview_messageManager.SuspendLayout();

                bool scrollToBottom = false;

                lock (newMessages)
                {
                    foreach (ListViewItem newMsg in newMessages)
                    {
                        if (listview_messageManager.Items.Count > 0 &&
                            listview_messageManager.Items[listview_messageManager.Items.Count - 1].Bounds.IntersectsWith(listview_messageManager.ClientRectangle))
                        {
                            scrollToBottom = true;
                        }

                        listview_messageManager.Items.Add(newMsg);

                        if (listview_messageManager.Items.Count > m_messageBufferSize)
                        {
                            listview_messageManager.Items.RemoveAt(0);
                        }
                    }

                    newMessages.Clear();
                }
                listview_messageManager.Update();
                //listview_messageManager.ResumeLayout();
                //listview_messageManager.EndUpdate();

                if (scrollToBottom)
                    this.ScrollToBottom();

            }
        }


        private void listview_messageManager_SizeChanged(object sender, EventArgs e)
        {
            int width = listview_messageManager.Size.Width - 24;
            Time.Width = 150;
            width -= 90;
            Message.Width = 2048; //width;
        }

        public void ClearMessages()
        {
            listview_messageManager.Items.Clear();
        }
    }
}
