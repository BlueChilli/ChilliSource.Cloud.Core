#if !NET_4X
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    public static class BackgroundTaskQueueExtensions
    {
        public static void AddBackgroundTaskQueue(this IServiceCollection services)
        {
            services.AddSingleton<BackgroundTaskQueue>();
            services.AddSingleton<IBackgroundTaskQueue>(provider => provider.GetRequiredService<BackgroundTaskQueue>());
            services.AddSingleton<IBackgroundTaskQueueConsumer>(provider => provider.GetRequiredService<BackgroundTaskQueue>());

            services.AddHostedService<QueuedHostedService>();
        }

        public static void AddBackgroundTaskQueue(this IServiceCollection services, Action<QueuedHostedServiceOptions> setupAction)
        {
            if (setupAction != null)
            {
                services.AddOptions<QueuedHostedServiceOptions>()
                    .Configure(setupAction);
            }

            AddBackgroundTaskQueue(services);
        }

        public static Task QueueBackgroundWorkTask(this IBackgroundTaskQueue queue, Func<CancellationToken, Task> workItem)
        {
            return QueueBackgroundWorkTask<object>(queue, async (ct) =>
            {
                await workItem(ct);
                return null;
            });
        }

        public static Task<T> QueueBackgroundWorkTask<T>(this IBackgroundTaskQueue queue, Func<CancellationToken, Task<T>> workItem)
        {
            var tcs = new TaskCompletionSource<T>();

            queue.QueueBackgroundWorkItem(async (ct) =>
            {
                try
                {
                    var result = await workItem(ct);
                    tcs.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    if (ct.IsCancellationRequested)
                    {
                        tcs.TrySetCanceled();
                    }
                    else
                    {
                        tcs.TrySetException(ex);
                    }
                }
            });

            return tcs.Task;
        }
    }
}
#endif