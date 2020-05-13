using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cognex.VS.Utility
{
    public class InformContent : IContent
    {
        private InformSeverity mSeverity;

        public InformContent(DateTime timeStamp, UserType userType, InformSeverity severity, string message)
            : base(timeStamp, userType, message)
        {
            mSeverity = severity;
            mMessageType = MessageType.Inform;
        }

        public InformContent(UserType user, InformSeverity severity, string message)
            : base(user, message)
        {
            mSeverity = severity;
            mMessageType = MessageType.Inform;
        }

        public InformContent(InformSeverity severity, string message)
            : base(UserType.Operator, message)
        {
            mSeverity = severity;
            mMessageType = MessageType.Inform;
        }

        public override object GetSeverity()
        {
            return mSeverity;
        }

        public override string ToString()
        {
            return "InformContent";
        }

    }
}
