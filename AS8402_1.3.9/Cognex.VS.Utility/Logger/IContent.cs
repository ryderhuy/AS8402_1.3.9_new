using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cognex.VS.Utility
{
    public abstract class IContent : INotifyPropertyChanged
    {
        protected string mMessage;
        public string Message
        {
            get { return mMessage; }
            internal set
            {
                mMessage = value;
                OnPropertyChanged("Message");
            }
        }

        protected DateTime mTimeStamp;

        public DateTime TimeStamp
        {
            get { return mTimeStamp; }
            set
            {
                mTimeStamp = value;
                OnPropertyChanged("TimeStamp");
            }
        }

        protected UserType mUserType;
        public UserType User
        {
            get { return mUserType; }
            set
            {
                mUserType = value;
                OnPropertyChanged("User");
            }
        }

        protected MessageType mMessageType;
        public MessageType MessageType
        {
            get { return mMessageType; }
            set
            {
                mMessageType = value;
                OnPropertyChanged("MessageType");
            }
        }

        public string Severity
        {
            get
            {
                return GetSeverity().ToString();
            }
        }

        public IContent(DateTime timeStamp, UserType userType, string message)
        {
            mTimeStamp = timeStamp;
            mMessage = message;
            mUserType = userType;
        }

        public IContent(UserType user, string message)
        {
            mTimeStamp = DateTime.Now;
            mUserType = user;
            mMessage = message;
        }

        public abstract object GetSeverity();

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool IsValidValues(object[] values)
        {
            foreach (object value in values)
            {
                if (!(value is Double || value is Int32 || value is DateTime || value is String || value is Boolean))
                    return false;
            }
            return true;
        }
    }
}
