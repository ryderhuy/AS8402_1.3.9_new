using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace Cognex.VS.Utility
{
    public class ContentQueue
    {
        protected Queue<IContent> mIncomingMessage;
        protected SemaphoreSlim mWaitingSemaphore;
        protected Thread mProcessingThread;
        protected bool mIsExit;
        private List<ContentObserver> mContentObsLst;
        private bool mEnableLog = true;

        private static object m_syncRoot = new Object();
        private static volatile ContentQueue m_this = null;
        public static void gInit()
        {
            if (m_this == null)
                m_this = new ContentQueue();
            if (m_this == null)
                throw new Exception("Can not create MessageManager object.");
            m_this.Initiliaze();
        }
        public static void gShutDown()
        {
            if (m_this != null)
                m_this.Shutdown();
        }
        public static ContentQueue gOnly
        {
            get
            {
                if (m_this == null)
                {
                    lock (m_syncRoot)
                    {
                        if (m_this == null)
                        {
                            m_this = new ContentQueue();
                            m_this.Initiliaze();
                        }
                    }
                }

                return m_this;
            }
        }
        private ContentQueue() { }
        ~ContentQueue() { }

        private void Initiliaze()
        {
            mIncomingMessage = new Queue<IContent>();
            mWaitingSemaphore = new SemaphoreSlim(0, int.MaxValue);

            mContentObsLst = new List<ContentObserver>();
        }

        public void Start()
        {
            mIsExit = false;
            mEnableLog = true;
            mProcessingThread = new Thread(new ThreadStart(ProcessIncomingMessages));
            mProcessingThread.SetApartmentState(System.Threading.ApartmentState.MTA);
            mProcessingThread.Name = "Message Processing Thread";
            mProcessingThread.Priority = ThreadPriority.Normal;
            mProcessingThread.Start();
        }

        public void SetEnableLog(bool enable)
        {
            mEnableLog = enable;
        }

        public bool RegisterObserver(ContentObserver observer)
        {
            if (mContentObsLst.Contains(observer))
                return false;
            mContentObsLst.Add(observer);
            return true;
        }

        public bool UnRegisterObserver(ContentObserver observer)
        {
            if (!mContentObsLst.Contains(observer))
                return false;
            mContentObsLst.Remove(observer);
            return true;
        }

        public void Alarm(string message, AlarmSeverity alarmType, UserType user, Exception ex = null)
        {
            if (!mEnableLog)
                return;
            lock (mIncomingMessage)
            {
                mIncomingMessage.Enqueue(new AlarmContent(user, alarmType, message));
                mWaitingSemaphore.Release();
                if (ex != null)
                    EnqueueExceptionStack(ex);
            }
        }

        public void Alarm(string message, Exception ex = null)
        {
            Alarm(message, AlarmSeverity.Unknown, UserType.Unknown, ex);
        }

        public void Warning(string message, Exception ex = null)
        {
            Info(InformSeverity.Warning, message, ex);
        }

        public void Info(string message, Exception ex = null)
        {
            Info(InformSeverity.Inform, message, ex);
        }

        public void Error(string message, Exception ex = null)
        {
            Info(InformSeverity.Error, message, ex);
        }

        public void Info(InformSeverity severity, string message, Exception ex = null)
        {
            if (!mEnableLog)
                return;
            lock (mIncomingMessage)
            {
                mIncomingMessage.Enqueue(new InformContent(severity, message));
                mWaitingSemaphore.Release();
                if (ex != null)
                    EnqueueExceptionStack(ex);
            }
        }

        private void EnqueueExceptionStack(Exception ex, int maxDepth = 8, int currentDepth = 0)
        {
            lock (mIncomingMessage)
            {
                if (ex.Message != null)
                {
                    mIncomingMessage.Enqueue(new InformContent(InformSeverity.Error, String.Format(CultureInfo.InvariantCulture, "{0}{1}", Tabs(currentDepth + 1), ex.Message)));
                    mWaitingSemaphore.Release();

                    if (!String.IsNullOrWhiteSpace(ex.StackTrace))
                    {
                        mIncomingMessage.Enqueue(new InformContent(InformSeverity.Error, String.Format(CultureInfo.InvariantCulture, "{0}{1}", Tabs(currentDepth + 1), ex.StackTrace)));
                        mWaitingSemaphore.Release();
                    }
                }

                if (ex.InnerException != null && currentDepth < maxDepth)
                {
                    EnqueueExceptionStack(ex.InnerException, maxDepth, currentDepth + 1);
                }
            }
        }

        private String Tabs(int i)
        {
            return new String('\t', i);
        }

        private void GlobalParamsReady()
        {
        }

        private void ProcessIncomingMessages()
        {
            while (!mIsExit || mIncomingMessage.Count > 0)
            {
                try
                {
                    if (mWaitingSemaphore.Wait(1000))
                    {
                        IContent message;
                        lock (mIncomingMessage)
                        {
                            message = mIncomingMessage.Dequeue();
                        }
                        OnNewMessageReceived(message);
                    }
                }
                catch (Exception exp)
                {
                    IContent message = new InformContent(InformSeverity.Error, "ProcessIncomingMessages:" + exp.Message);
                    OnNewMessageReceived(message);
                }
            }
        }

        private void OnNewMessageReceived(IContent content)
        {
            foreach (var observer in mContentObsLst)
                observer.IncomingContent(content);
        }

        private void Shutdown()
        {
            mIsExit = true;
            while (!mProcessingThread.Join(10))
                Application.DoEvents();

        }
    }
}
