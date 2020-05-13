using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cognex.VS.Utility
{
    public delegate void OnTriggerEvent(IContent content);
    public class ContentTrigger
    {
        public event OnTriggerEvent OnTrigger;

        public void FireTrigger(IContent content)
        {
            if (OnTrigger != null)
                OnTrigger(content);
        }
    }
}
