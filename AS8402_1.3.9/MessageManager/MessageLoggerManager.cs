using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Globalization;

namespace MessageManager
{
    [Serializable]
    public enum MessageType
    {
        Info,
        Warning,
        Alarm,
        OK,
        NG
    }

    [Serializable]
    public enum AlarmType : int
    {
        Unknown = 0,
        CameraHardwareError = 1,
        ToolblockLoadError = 2,
        CameraAcqFailure = 3,
        CalibrationParamError,
        InspectionError,
        InspectionMissing,
        HardDiskFull,
        OutOfMemory
    }

    [Serializable]
    public class MessageContentAndInfo
    {
        String m_message;
        AlarmType m_alarmType;
        MessageType m_messageType;
        DateTime m_timestamp;

        public MessageContentAndInfo(String message, MessageType msgType)
        {
            m_message = message;
            m_messageType = msgType;
            m_timestamp = DateTime.Now;
        }

        public MessageContentAndInfo(String message, AlarmType alarmType)
        {
            m_message = message;
            m_alarmType = alarmType;
            m_messageType = MessageType.Alarm;
            m_timestamp = DateTime.Now;
        }

        public MessageContentAndInfo(String message)
        {
            m_message = message;
            m_messageType = MessageType.Info;
            m_timestamp = DateTime.Now;
        }

        public String Message
        {
            get { return m_message; }
        }

        public AlarmType AlarmType
        {
            get { return m_alarmType; }
        }

        public MessageType MessageType
        {
            get { return m_messageType; }
        }

        public DateTime MessageTime
        {
            get { return m_timestamp; }
        }
    }


    public class MessageLoggerManager
    {
        #region Singleton Stuff
        private static object m_syncRoot = new Object();
        private static volatile MessageLoggerManager m_this = null;
        public static void Init()
        {
            if (m_this == null)
                m_this = new MessageLoggerManager();
            if (m_this == null)
                throw new Exception("Can not create MessageManager object.");
            m_this.init();
        }
        public static void ShutDown()
        {
            if (m_this != null)
                m_this.shutdown();
        }
        public static MessageLoggerManager Log
        {
            get
            {
                if (m_this == null)
                {
                    lock (m_syncRoot)
                    {
                        if (m_this == null)
                        {
                            m_this = new MessageLoggerManager();
                            m_this.init();
                            //if (m_this == null)
                            //  throw new Exception("Failed to init TBProcessor object.");
                        }
                    }
                }

                return m_this;
            }
        }
        private MessageLoggerManager()
        {
        }
        ~MessageLoggerManager()
        {
        }
        #endregion

        Queue<MessageContentAndInfo> m_incomingMessages;
        SemaphoreSlim m_waitingSemaphore;
        List<MessageContentAndInfo> m_messages;
        //String logFilePath;
        Thread m_processingThread;
        bool m_exit;
        int m_messageBufferLength = 1000;

        public void init()
        {
            m_messages = new List<MessageContentAndInfo>();
            m_incomingMessages = new Queue<MessageContentAndInfo>();
            m_waitingSemaphore = new SemaphoreSlim(0, int.MaxValue);

            m_exit = false;
            m_processingThread = new Thread(new ThreadStart(processIncomingMessages));
            m_processingThread.SetApartmentState(System.Threading.ApartmentState.MTA);
            m_processingThread.Name = "Message Processing Thread";
            m_processingThread.Priority = ThreadPriority.Normal;
            m_processingThread.IsBackground = true;
            m_processingThread.Start();
        }


        public void Alarm(String message, AlarmType alarmType, Exception ex = null)
        {
            lock (m_incomingMessages)
            {
                m_incomingMessages.Enqueue(new MessageContentAndInfo(message, alarmType));
                m_waitingSemaphore.Release();
                if (ex != null)
                    enqueueExceptionStack(ex);
            }
        }

        public void Alarm(String message, Exception ex = null)
        {
            lock (m_incomingMessages)
            {
                m_incomingMessages.Enqueue(new MessageContentAndInfo(message, AlarmType.Unknown));
                m_waitingSemaphore.Release();
                if (ex != null)
                    enqueueExceptionStack(ex);
            }
        }

        public void Warn(String message, Exception ex = null)
        {
            lock (m_incomingMessages)
            {
                m_incomingMessages.Enqueue(new MessageContentAndInfo(message, MessageType.Warning));
                m_waitingSemaphore.Release();
                if (ex != null)
                    enqueueExceptionStack(ex);
            }
        }

        public void OK(String message, Exception ex = null)
        {
            lock (m_incomingMessages)
            {
                m_incomingMessages.Enqueue(new MessageContentAndInfo(message, MessageType.OK));
                m_waitingSemaphore.Release();
                if (ex != null)
                    enqueueExceptionStack(ex);
            }
        }

        public void NG(String message, Exception ex = null)
        {
            lock (m_incomingMessages)
            {
                m_incomingMessages.Enqueue(new MessageContentAndInfo(message, MessageType.NG));
                m_waitingSemaphore.Release();
                if (ex != null)
                    enqueueExceptionStack(ex);
            }
        }

        public void Info(String message, Exception ex = null)
        {
            lock (m_incomingMessages)
            {
                m_incomingMessages.Enqueue(new MessageContentAndInfo(message));
                m_waitingSemaphore.Release();
                if (ex != null)
                    enqueueExceptionStack(ex);
            }
        }

        private void enqueueExceptionStack(Exception ex, int maxDepth = 8, int currentDepth = 0)
        {
            lock (m_incomingMessages)
            {
                if (ex.Message != null)
                {
                    m_incomingMessages.Enqueue(new MessageContentAndInfo(String.Format(CultureInfo.InvariantCulture, "{0}{1}", Tabs(currentDepth + 1), ex.Message)));
                    m_waitingSemaphore.Release();

                    if (!String.IsNullOrWhiteSpace(ex.StackTrace))
                    {
                        m_incomingMessages.Enqueue(new MessageContentAndInfo(String.Format(CultureInfo.InvariantCulture, "{0}{1}", Tabs(currentDepth + 1), ex.StackTrace)));
                        m_waitingSemaphore.Release();
                    }

                }

                if (ex.InnerException != null && currentDepth < maxDepth)
                {
                    enqueueExceptionStack(ex.InnerException, maxDepth, currentDepth + 1);
                }
            }
        }

        private String Tabs(int i)
        {
            return new String('\t', i);
        }

        public void GlobalParamsReady()
        {
        }

        private void processIncomingMessages()
        {
            while (!m_exit || m_incomingMessages.Count > 0)
            {
                try
                {
                    if (m_waitingSemaphore.Wait(1000))
                    {
                        MessageContentAndInfo message;
                        lock (m_incomingMessages)
                        {
                            message = m_incomingMessages.Dequeue();
                        }
                        m_messages.Add(message);

                        if (m_messages.Count > m_messageBufferLength)
                            m_messages.RemoveAt(0);

                        OnNewMessageReceived(message);
                    }
                }
                catch (Exception exp)
                {
                    MessageContentAndInfo message = new MessageContentAndInfo("processIncomingMessages:" + exp.Message);
                    OnNewMessageReceived(message);
                }
            }
        }
        public delegate void NewMessageReceivedEventHandler(object sender, MessageContentAndInfo args);
        public event NewMessageReceivedEventHandler NewMessageReceived;
        private void OnNewMessageReceived(MessageContentAndInfo message)
        {
            // Copy the delegate for thread safety
            NewMessageReceivedEventHandler handlers = NewMessageReceived;
            if (handlers != null)
            {
                handlers(this, message);
            }
        }

        public List<MessageContentAndInfo> Messages
        {
            get { return m_messages; }
        }

        public void shutdown()
        {
            m_exit = true;
            while (!m_processingThread.Join(10))
                Application.DoEvents();
        }

    }
}
