using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.Options;

namespace ChilliSource.Cloud.Core.NetStandard.Tests.Infrastructure.BackgroundTaskQueue
{
    public class BackgroundTaskQueueTests : IDisposable
    {
        private readonly StringBuilder Console = new StringBuilder();
        private readonly ITestOutputHelper _output;

        public BackgroundTaskQueueTests(ITestOutputHelper output)
        {
            _output = output;
        }

        public void Dispose()
        {
            var outputStr = Console.ToString();
            if (outputStr.Length > 0)
            {
                _output.WriteLine(outputStr);
            }
        }

        [Fact]
        public async Task TestMaxConcurrency()
        {
            var hostBuilder = new HostBuilder().ConfigureServices(svc =>
                        {
                            svc.Configure<HostOptions>(option =>
                            {
                                option.ShutdownTimeout = System.TimeSpan.FromSeconds(30);
                            });

                            svc.AddOptions<QueuedHostedServiceOptions>()
                                .Configure(options => options.MaxConcurrency = 3);

                            svc.AddBackgroundTaskQueue();
                        });

            using (var host = new TestHostHelper(hostBuilder))
            {
                await host.BuildAndStartAsync();

                var taskQueue = host.ServiceProvider.GetRequiredService<IBackgroundTaskQueue>();
                var options = host.ServiceProvider.GetRequiredService<IOptions<QueuedHostedServiceOptions>>().Value;

                int concurrency = 0;
                int executed = 0;
                int numTasks = 100;
                int[] concurrencyValues = new int[numTasks];

                for (int i = 0; i < numTasks; i++)
                {
                    var index = i;
                    taskQueue.QueueBackgroundWorkItem(async (ct) =>
                    {
                        concurrencyValues[index] = Interlocked.Increment(ref concurrency);

                        try
                        {
                            await Task.Delay(1);
                            await Task.Delay(20000, ct);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref concurrency);

                            Interlocked.Increment(ref executed);
                        }
                    });
                }

                await Task.Delay(100);
                host.RequestShutdown();
                await host.WaitShutdown();

                var maxConcurrency = concurrencyValues.Max();

                //Max concurrency was respected
                Assert.True(maxConcurrency <= options.MaxConcurrency, "maxConcurrency <= options.MaxConcurrency failed");

                //Host waited for all tasks to complete.
                Assert.Equal(numTasks, executed);
            }
        }
    }

    public class TestHostHelper : IDisposable
    {
        private CancellationTokenSource _tokenSource;
        private IHostBuilder _hostBuilder;
        private Task _hostTask;

        public IServiceProvider ServiceProvider { get; private set; }

        public TestHostHelper(IHostBuilder hostBuilder)
        {
            _hostBuilder = hostBuilder;
            _tokenSource = new CancellationTokenSource();
        }

        public async Task BuildAndStartAsync()
        {
            var hostStarted = new AsyncManualResetEvent();
            var host = _hostBuilder.Build();

            this.ServiceProvider = host.Services;

            _hostTask = Task.Factory.StartNew(async () =>
            {
                try
                {
                    hostStarted.Set();
                    await host.RunAsync(_tokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    /* noop */
                }
            }, TaskCreationOptions.LongRunning).Unwrap();
            await hostStarted.WaitAsync();
        }

        public void RequestShutdown()
        {
            _tokenSource.Cancel();
        }

        public async Task WaitShutdown()
        {
            await _hostTask;
        }

        bool disposed = false;

        public void Dispose()
        {
            if (disposed)
                return;

            _tokenSource.Dispose();
            disposed = true;
        }
    }
}
