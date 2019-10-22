using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ChilliSource.Cloud.Core.NetStandard.Tests.Infrastructure
{
    public class ManagedThreadPoolTests
    {
        private readonly StringBuilder Console = new StringBuilder();
        private readonly ITestOutputHelper _output;

        public ManagedThreadPoolTests(ITestOutputHelper output)
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

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void TestEnqueue(int numThreads)
        {
            var pool = new ManagedThreadPool(numThreads);

            int value1 = 0, value2 = 0;

            pool.QueueUserWorkItem((_) =>
            {
                value1++;
            }, null);

            pool.QueueUserWorkItem((_) =>
            {
                value2++;
            }, null);

            Thread.Sleep(10);
            Thread.Sleep(10);

            pool.StopPool(waitTillStops: true);

            Assert.True(value1 == 1);
            Assert.True(value2 == 1);
        }


        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task TestEnqueueAsync(int numThreads)
        {
            var pool = new ManagedThreadPool(numThreads);

            int value1 = 0, value2 = 0, value3 = 0;
            int thread1 = 0, thread2 = 0, thread3 = 0;

            var task1 = pool.TaskFactory.StartNew(async () =>
            {
                thread1 = Thread.CurrentThread.ManagedThreadId;
                await Task.Delay(50);
                value1++;
            }).Unwrap();

            var task2 = pool.TaskFactory.StartNew(async () =>
            {
                thread2 = Thread.CurrentThread.ManagedThreadId;
                Assert.True(TaskScheduler.Current == pool.TaskFactory.Scheduler);

                value2 = await Task2(value2, async () =>
                {
                    thread3 = Thread.CurrentThread.ManagedThreadId;
                    await Task.Delay(10);
                    value3++;
                });                
            }).Unwrap();

            await task1;
            await task2;

            pool.StopPool(waitTillStops: true);            

            _output.WriteLine("-------------------");
            _output.WriteLine("Thread1: " + thread1);
            _output.WriteLine("Thread2: " + thread2);
            _output.WriteLine("Thread3: " + thread3);
            _output.WriteLine("-------------------");

            Assert.True(value1 == 1);
            Assert.True(value2 == 1);
            Assert.True(value3 == 1);
        }

        public async Task<int> Task2(int value2, Func<Task> another)
        {
            await Task.Delay(10);
            
            var childTask = TaskHelper.Run(another, TaskScheduler.Current);
            value2++;
            await Task.Delay(50);
            await childTask;
            return value2;
        }
    }
}
