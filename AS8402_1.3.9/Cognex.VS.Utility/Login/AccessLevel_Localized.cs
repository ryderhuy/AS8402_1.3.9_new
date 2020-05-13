using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cognex.VS.Utility
{
    public class AccessLevel_Localized
    {
        public AccessLevel_Localized(AccessLevel v, string t)
        {
            val = v;
            text = t;
        }

        public override string ToString()
        {
            // return the localized name
            return text;
        }

        public AccessLevel val;
        public string text;
    }
}
