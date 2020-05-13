using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cognex.VS.Utility
{
    public class Utility
    {
        public static string GetThisExecutableDirectory()
        {
            string loc = Application.ExecutablePath;
            loc = System.IO.Path.GetDirectoryName(loc) + "\\";
            return loc;
        }
    }

}
