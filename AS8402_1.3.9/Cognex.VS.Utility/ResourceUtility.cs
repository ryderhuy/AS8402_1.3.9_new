using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Resources;

namespace Cognex.VS.Utility
{
    public class ResourceUtility
    {
        // helper class to wrap string resources for this application
        private static ResourceManager mResources;
        static ResourceUtility()
        {
            mResources = new ResourceManager("Cognex.VS.Utility.strings",
              System.Reflection.Assembly.GetExecutingAssembly());
        }

        public static string GetString(string resname)
        {
            string str = mResources.GetString(resname);
            if (str == null)
                str = "ERROR(" + resname + ")";
            return str;
        }

        public static string FormatString(string resname, string arg0)
        {
            try
            {
                return string.Format(GetString(resname), arg0);
            }
            catch (Exception)
            {
            }

            return "ERROR(" + resname + ")";
        }

        public static string FormatString(string resname, string arg0, string arg1)
        {
            try
            {
                return string.Format(GetString(resname), arg0, arg1);
            }
            catch (Exception)
            {
            }

            return "ERROR(" + resname + ")";
        }
    }
}
