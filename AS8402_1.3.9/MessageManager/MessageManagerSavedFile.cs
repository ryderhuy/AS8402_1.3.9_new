using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageManager;

namespace MessageManager
{
    public class MessageManagerSavedFile
    {
        #region Singleton Stuff
        private static MessageManagerSavedFile m_this = null;
        public static void Init(String loggingFolderPath, String applicationName, int maxAllowedLogAgeInDays = 30)
        {
            if (m_this == null)
                m_this = new MessageManagerSavedFile(loggingFolderPath, applicationName, maxAllowedLogAgeInDays = 30);
            if (m_this == null)
                throw new Exception("Can not create MessageManagerLogger object.");
            m_this.init();
        }
        public static void ShutDown()
        {
            if (m_this != null)
                m_this.shutdown();
        }
        public static MessageManagerSavedFile gOnly
        {
            get { return m_this; }
        }
        #endregion

        static String m_loggingFolderPath;
        String m_applicationName;
        String m_currentLogFilePath;
        StreamWriter m_currentLogFile;
        DateTime m_currentLogFileCreationTime, m_currentLogFileExpirationTime;
        int m_maxAllowedLogAgeInDays;
        double m_maxLogFileSizeMB;

        Object m_lockObject;

        private MessageManagerSavedFile(String loggingFolderPath, String applicationName, int maxAllowedLogAgeInDays = 30)
        {
            m_loggingFolderPath = loggingFolderPath;
            m_applicationName = applicationName;
            m_maxAllowedLogAgeInDays = maxAllowedLogAgeInDays;
            m_maxLogFileSizeMB = 5.0;
            m_lockObject = new Object();
        }

        ~MessageManagerSavedFile()
        {
            MessageLoggerManager.Log.NewMessageReceived -= gOnly_NewMessageReceived;
        }
        public static string LoggingFolderPath
        {
            get { return m_loggingFolderPath; }
            set
            {
                m_loggingFolderPath = value;
            }
        }
        private void init()
        {
            if (!Directory.Exists(m_loggingFolderPath))
            {
                Directory.CreateDirectory(m_loggingFolderPath);
            }

            trimDirectoryByAge(m_loggingFolderPath, m_maxAllowedLogAgeInDays);

            CreateNewLogFile();

            MessageLoggerManager.Log.NewMessageReceived += gOnly_NewMessageReceived;
        }

        private void shutdown()
        {
            if (m_currentLogFile != null)
            {
                m_currentLogFile.Close();
            }
        }

        private static double GetFileSizeMB(string filename)
        {
            FileInfo fi = new FileInfo(filename);
            return (double)fi.Length / 1024.0d / 1024.0d;
        }

        public void CreateNewLogFile()
        {
            try
            {
                if (m_currentLogFile != null)
                {
                    m_currentLogFile.Close();
                }

                // Have this log file expire in 6 hours
                m_currentLogFileCreationTime = DateTime.Now;
                m_currentLogFileExpirationTime = m_currentLogFileCreationTime.AddHours(6);

                int disambiguationNumber = 0;
                m_currentLogFilePath = String.Format(CultureInfo.InvariantCulture, "{0}{1}{2}{3}_{4}_Log.txt", m_loggingFolderPath, m_currentLogFileCreationTime.Year.ToString("D4"), m_currentLogFileCreationTime.Month.ToString("D2"), m_currentLogFileCreationTime.Day.ToString("D2"), m_applicationName);

                while (File.Exists(m_currentLogFilePath))
                {
                    // Use this file?
                    // If the expiration time of this file would have been in the future and this file is less than 5MB, use it
                    if (DateTime.Now < File.GetCreationTime(m_currentLogFilePath).AddHours(6) && GetFileSizeMB(m_currentLogFilePath) < m_maxLogFileSizeMB)
                    {
                        m_currentLogFileCreationTime = File.GetCreationTime(m_currentLogFilePath);
                        m_currentLogFileExpirationTime = m_currentLogFileCreationTime.AddDays(0.25);
                        break;
                    }

                    // Otherwise, disambiguate the name and continue
                    disambiguationNumber++;
                    m_currentLogFilePath = String.Format(CultureInfo.InvariantCulture, "{0}{1}{2}{3}_{4}_{5}_Log.txt", m_loggingFolderPath, m_currentLogFileCreationTime.Year.ToString("D4"), m_currentLogFileCreationTime.Month.ToString("D2"), m_currentLogFileCreationTime.Day.ToString("D2"), disambiguationNumber, m_applicationName);
                }

                m_currentLogFile = new StreamWriter(m_currentLogFilePath, true);
                m_currentLogFile.AutoFlush = true;
            }
            catch (Exception ex)
            {
                m_currentLogFile = null;
                MessageManager.MessageLoggerManager.Log.Alarm("Create log file fail", ex);
            }
        }

        public void WriteToLogFileSynchronous(MessageContentAndInfo message)
        {
            gOnly_NewMessageReceived(null, message);
        }

        void gOnly_NewMessageReceived(object sender, MessageContentAndInfo args)
        {
            lock (m_lockObject)
            {
                if (DateTime.Now > m_currentLogFileExpirationTime || m_currentLogFile == null)
                {
                    CreateNewLogFile();
                }
                if (m_currentLogFile != null)
                {
                    m_currentLogFile.WriteLine("{0}\t{1}\t{2}", args.MessageTime.ToString("yyyyMMdd_HH:mm:ss"), args.MessageType.ToString(), args.Message);
                }
            }
        }

        void trimDirectoryByAge(String p, int maxDaysOld)
        {
            DateTime oldRecordCutoff = DateTime.Now.Subtract(new TimeSpan(maxDaysOld, 0, 0, 0));

            IEnumerable<String> folderContents = Directory.EnumerateFiles(p, String.Format(CultureInfo.InvariantCulture, "*_{0}_Log.txt", m_applicationName), SearchOption.TopDirectoryOnly);
            List<String> filesToDelete = new List<String>();
            foreach (String filePath in folderContents)
            {
                if (File.GetCreationTime(filePath) < oldRecordCutoff)
                {
                    filesToDelete.Add(filePath);
                }
            }

            foreach (String filePath in filesToDelete)
            {
                File.Delete(filePath);
            }

        }
    }
}
