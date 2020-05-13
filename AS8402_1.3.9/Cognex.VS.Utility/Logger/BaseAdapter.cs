using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cognex.VS.Utility
{
    public abstract class BaseAdapter
    {
        private ContentObserver mContentObserver;

        public BaseAdapter()
        {
            mContentObserver = new ContentObserver();
            mContentObserver.OnNewContentEvent += MContentObserver_OnNewContentEvent;
        }

        public ContentObserver Observer
        {
            get
            {
                return mContentObserver;
            }
        }

        private void MContentObserver_OnNewContentEvent(object sender, EventArgs e)
        {
            ContentEventArgs args = e as ContentEventArgs;
            if (e == null)
                return;
            ProcessContent(args.Content);
        }

        protected abstract void ProcessContent(IContent content);

        public virtual void Shutdown()
        {
            mContentObserver.OnNewContentEvent -= MContentObserver_OnNewContentEvent;
        }
    }
}
