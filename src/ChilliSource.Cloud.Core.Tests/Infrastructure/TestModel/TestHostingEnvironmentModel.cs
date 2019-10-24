using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core.Tests.Infrastructure.TestModel
{
    public class TestHostingEnvironmentModel : IHostingEnvironment
    {
        public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
        {
            Task.Run(() => workItem(CancellationToken.None));
        }

        public void QueueBackgroundWorkItem(Action<CancellationToken> workItem)
        {
            ThreadPool.QueueUserWorkItem((object state) => workItem(CancellationToken.None));
        }
    }
}
