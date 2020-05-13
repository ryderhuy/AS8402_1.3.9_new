using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cognex.VS.Utility
{ 
    public class ContentEventArgs : EventArgs
    {
        public IContent Content
        {
            get; set;
        }
    }

    public class ContentObserver
    {
        public event EventHandler OnNewContentEvent;
        public void IncomingContent(IContent content)
        {
            if (OnNewContentEvent != null)
                OnNewContentEvent(this, new ContentEventArgs() { Content = content });
        }
    }
}
