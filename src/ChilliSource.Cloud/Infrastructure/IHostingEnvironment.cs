using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChilliSource.Cloud
{
    public interface IHostingEnvironment
    {
        //
        // Summary:
        //     Schedules a task which can run in the background, independent of any request.
        //
        // Parameters:
        //   workItem:
        //     A unit of execution.
        void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);

        //
        // Summary:
        //     Schedules a task which can run in the background, independent of any request.
        //
        // Parameters:
        //   workItem:
        //     A unit of execution.
        void QueueBackgroundWorkItem(Action<CancellationToken> workItem);
    }
}
