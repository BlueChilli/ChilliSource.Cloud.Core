#if !NET_4X
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue, IBackgroundTaskQueueConsumer
    {
        private AsyncProducerConsumerQueue<Func<CancellationToken, Task>> _workItems;

        public BackgroundTaskQueue()
        {
            _workItems = new AsyncProducerConsumerQueue<Func<CancellationToken, Task>>();
        }

        public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            _workItems.Enqueue(workItem);
        }

        public Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            return _workItems.DequeueAsync(cancellationToken);
        }
    }
}
#endif