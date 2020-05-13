using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cognex.VS.Utility
{
    public enum AccessLevel { Operator, Supervisor, Administrator }

    public class AccessLevelChangedEventArgs : EventArgs
    {
        public AccessLevel Level { get; internal set; }
        public AccessLevelChangedEventArgs(AccessLevel level)
        {
            Level = level;
        }
    }
}
