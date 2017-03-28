using ChilliSource.Cloud;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Web.Hosting;

namespace ChilliSource.Cloud.Web.Infrastructure
{
    public class WebHostingEnvironment : IHostingEnvironment
    {
        public void QueueBackgroundWorkItem(Action<CancellationToken> workItem)
        {
            HostingEnvironment.QueueBackgroundWorkItem(workItem);
        }

        public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
        {
            HostingEnvironment.QueueBackgroundWorkItem(workItem);
        }
    }
}
