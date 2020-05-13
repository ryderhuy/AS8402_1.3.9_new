using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cognex.VS.Utility
{
    public class AlarmContent : IContent
    {
        private AlarmSeverity mSeverity;

        public AlarmContent(UserType user, AlarmSeverity severity, string message)
            : base(user, message)
        {
            mSeverity = severity;
            mMessageType = MessageType.Alarm;
        }

        public AlarmContent(AlarmSeverity severity, string message)
            : base(UserType.Operator, message)
        {
            mSeverity = severity;
            mMessageType = MessageType.Alarm;
        }

        public AlarmContent(DateTime timeStamp, UserType user, AlarmSeverity severity, string message)
            : base(timeStamp, user, message)
        {
            mSeverity = severity;
            mMessageType = MessageType.Alarm;
        }

        public override object GetSeverity()
        {
            return mSeverity;
        }

        public override string ToString()
        {
            return "AlarmContent";
        }

    }
}
