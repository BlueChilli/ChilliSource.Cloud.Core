using System;
using System.Collections.Generic;
using Xunit;
using System.Data.Entity;
using System.Threading;
using System.Linq;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Data.Entity.Migrations;
using System.Threading.Tasks;
using Nito.AsyncEx;
using ChilliSource.Cloud.Core.Distributed;
using Serilog;
using System.Text;
using Xunit.Abstractions;

namespace ChilliSource.Cloud.Core.Tests
{
    [Collection(DistributedTestsCollection.Name)]
    public class LockManagerTests : IDisposable
    {
        private readonly StringBuilder Console = new StringBuilder();
        private readonly ITestOutputHelper _output;

        public LockManagerTests(ITestOutputHelper output)
        {
            _output = output;

            using (var context = TestDbContext.Create())
            {
                Database.SetInitializer(new MigrateDatabaseToLatestVersion<TestDbContext, TestDbConfiguration>());
                context.Database.Initialize(true);

                context.Database.ExecuteSqlCommand("DELETE FROM DistributedLocks");
                context.SaveChanges();
            }
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
        public void BasicTest()
        {
            var manager = LockManagerFactory.Create(() => TestDbContext.Create());

            var resource1 = new Guid("B5FF501B-383A-49DD-BD84-74511F70FCE9");
            LockInfo lockInfo1;

            var firstLock = manager.TryLock(resource1, new TimeSpan(TimeSpan.TicksPerMinute), out lockInfo1);
            Assert.True(firstLock && lockInfo1.HasLock);
            manager.Release(lockInfo1);

            Assert.False(lockInfo1.HasLock);
        }

        [Fact]
        public void BasicTestSingleThread()
        {
            AsyncContext.Run(() => BasicTest());
        }

        [Fact]
        public async Task BasicTestAsync()
        {
            ILockManagerAsync manager = LockManagerFactory.Create(() => TestDbContext.Create());

            var resource1 = new Guid("B5FF501B-383A-49DD-BD84-74511F70FCE9");
            var lockInfo1 = await manager.TryLockAsync(resource1, new TimeSpan(TimeSpan.TicksPerMinute));
            Assert.True(lockInfo1 != null && lockInfo1.HasLock);
            await manager.ReleaseAsync(lockInfo1);

            Assert.False(lockInfo1.HasLock);
        }

        [Fact]
        public void DoubleLockSingleThread()
        {
            AsyncContext.Run(() => DoubleLock());
        }

        [Fact]
        public void DoubleLock()
        {
            var manager = LockManagerFactory.Create(() => TestDbContext.Create());

            var resource1 = new Guid("ACC9D515-1529-49BD-AECE-E163D9200E0F");
            var resource2 = new Guid("A7FE17DA-BE0F-40FD-8AE6-7F1F7C615C72");
            LockInfo lockInfo1, lockInfo2, lockInfo3;

            var firstLock = manager.TryLock(resource1, new TimeSpan(TimeSpan.TicksPerMinute * 2), out lockInfo1);
            manager.TryLock(resource1, new TimeSpan(TimeSpan.TicksPerMinute), out lockInfo2);
            manager.TryLock(resource2, new TimeSpan(TimeSpan.TicksPerMinute), out lockInfo3);

            Assert.True(firstLock && lockInfo1.HasLock);
            Assert.False(lockInfo2.HasLock);
            Assert.True(lockInfo3.HasLock);

            manager.Release(lockInfo1);
            manager.Release(lockInfo3);

            manager.TryLock(resource1, new TimeSpan(TimeSpan.TicksPerMinute), out lockInfo2);
            Assert.True(lockInfo2.HasLock);

            manager.Release(lockInfo2);

            Assert.False(lockInfo1.HasLock);
            Assert.False(lockInfo2.HasLock);
            Assert.False(lockInfo3.HasLock);
        }

        [Fact]
        public void RenewLockSingleThread()
        {
            AsyncContext.Run(() => RenewLock());
        }

        [Fact]
        public void RenewLock()
        {
            //*** TIME-SENSITIVE TEST, don't use debug-mode
            var manager = LockManagerFactory.Create(() => TestDbContext.Create());

            var resource1 = new Guid("CB2A9AD3-79F5-4FAA-A37E-FD21A1C688EB");
            LockInfo lockInfo1, lockInfo2;

            manager.TryLock(resource1, new TimeSpan(TimeSpan.TicksPerSecond), out lockInfo1);
            Assert.True(lockInfo1.HasLock);

            Thread.Sleep(500);
            manager.TryRenewLock(lockInfo1); // 1 sec renew

            Thread.Sleep(700);
            Assert.True(lockInfo1.HasLock);

            Thread.Sleep(500);
            Assert.False(lockInfo1.HasLock);

            manager.TryLock(resource1, new TimeSpan(TimeSpan.TicksPerSecond), out lockInfo2);
            Assert.True(lockInfo2.HasLock);

            manager.Release(lockInfo1);
            manager.Release(lockInfo2);
        }

        [Fact]
        public void RenewLock2SingleThread()
        {
            AsyncContext.Run(() => RenewLock2());
        }

        [Fact]
        public void RenewLock2()
        {
            //*** TIME-SENSITIVE TEST, don't use debug-mode
            var manager = LockManagerFactory.Create(() => TestDbContext.Create());

            var resource1 = new Guid("AFE160D8-0172-4F0B-8A83-E44489080541");
            LockInfo lockInfo1;

            manager.TryLock(resource1, new TimeSpan(TimeSpan.TicksPerSecond), out lockInfo1);
            Assert.True(lockInfo1.HasLock);
            Thread.Sleep(1100);
            Assert.False(lockInfo1.HasLock);

            manager.TryRenewLock(lockInfo1, retryLock: false);
            Assert.False(lockInfo1.HasLock);

            manager.TryRenewLock(lockInfo1, retryLock: true);
            Assert.True(lockInfo1.HasLock);

            manager.Release(lockInfo1);
        }

        const int ConcurrentLock_LOOP = 20;
        [Fact]
        public void ConcurrentLock()
        {
            var NTHREADS = 10;
            var signal = new ManualResetEvent(false);
            var counterContext = new CounterContext() { Counter = 0 };
            var contexts = Enumerable.Repeat<Func<ConcurrentContext>>(() => new ConcurrentContext(signal, counterContext, ConcurrentLock_LOOP, waitForLock: false), NTHREADS)
                                    .Select(a => a()).ToList();

            var threads = contexts.Select(c => new Thread(c.ConcurrentLock_Start)).ToList();
            threads.ForEach(t => t.Start());

            //Runs all threads at the same time.
            contexts.ForEach(c => c.ThreadStartSignal.WaitOne());
            signal.Set();

            //Waits until all threads finish
            threads.ForEach(t => t.Join());

            //Each thread adds ConcurrentLock_LOOP to the counter. If everything is ok the sum should be (ConcurrentLock_LOOP * NTHREADS threads);

            Assert.Equal(ConcurrentLock_LOOP * NTHREADS, counterContext.Counter);
        }

        [Fact]
        public async Task TestWaitForLockTimeoutAsync()
        {
            using (var manager = LockManagerFactory.Create(() => TestDbContext.Create()))
            {
                var resource = new Guid("3767EF33-8296-4363-95CE-120E0453E3D0");

                LockInfo lockInfo = await manager.TryLockAsync(resource, TimeSpan.FromMinutes(5));
                Assert.True(lockInfo.AsImmutable().HasLock());

                LockInfo lockInfo2 = await manager.WaitForLockAsync(resource, TimeSpan.FromMinutes(1), TimeSpan.FromMilliseconds(50));
                Assert.False(lockInfo2.AsImmutable().HasLock());

                //Already cancelled token
                LockInfo lockInfo3 = await manager.WaitForLockAsync(resource, TimeSpan.FromMinutes(1), TimeSpan.Zero);
                Assert.False(lockInfo3.AsImmutable().HasLock());

                await manager.ReleaseAsync(lockInfo);
                await manager.ReleaseAsync(lockInfo2);
                await manager.ReleaseAsync(lockInfo3);
            }
        }

        [Fact]
        public void TestWaitForLockTimeout()
        {
            using (var manager = LockManagerFactory.Create(() => TestDbContext.Create()))
            {
                var resource = new Guid("F4139466-2A16-47CC-8359-D53521C2C5EC");

                LockInfo lockInfo = null;
                LockInfo lockInfo2 = null;
                LockInfo lockInfo3 = null;
                Assert.True(manager.TryLock(resource, TimeSpan.FromMinutes(5), out lockInfo));
                Assert.False(manager.WaitForLock(resource, TimeSpan.FromMinutes(1), TimeSpan.FromMilliseconds(50), out lockInfo2));
                Assert.False(manager.WaitForLock(resource, TimeSpan.FromMinutes(1), TimeSpan.Zero, out lockInfo3));

                manager.Release(lockInfo);
                manager.Release(lockInfo2);
                manager.Release(lockInfo3);
            }
        }

        [Fact]
        public async Task RunWithLockTimeoutAsync()
        {
            using (var manager = LockManagerFactory.Create(() => TestDbContext.Create()))
            {
                var resource = new Guid("B10F8164-6D72-4997-8CFC-60A0494A6818");
                bool firstLockRun = false;

                await manager.RunWithLockAsync(resource, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(1), async (_lock) =>
                {
                    firstLockRun = true;

                    Exception ex = null;
                    bool run = false;

                    try
                    {
                        await manager.RunWithLockAsync(resource, TimeSpan.FromMinutes(1), TimeSpan.FromMilliseconds(50),
                            async (_lock2) =>
                            {
                                run = true;
                            });
                    }
                    catch (Exception exc)
                    {
                        ex = exc;
                    }

                    Assert.True(ex != null, "Exception was expected");
                    Assert.False(run);

                    try
                    {
                        await manager.RunWithLockAsync(resource, TimeSpan.FromMinutes(1), TimeSpan.Zero,
                            async (_lock3) =>
                            {
                                run = true;
                            });
                    }
                    catch (Exception exc)
                    {
                        ex = exc;
                    }

                    Assert.True(ex != null, "Exception was expected");
                    Assert.False(run);
                });

                Assert.True(firstLockRun);
            }
        }

        [Fact]
        public void RunWithLockTimeout()
        {
            using (var manager = LockManagerFactory.Create(() => TestDbContext.Create()))
            {
                var resource = new Guid("D295C57E-BFBD-4760-8424-950875A594F7");
                bool firstLockRun = false;

                manager.RunWithLock(resource, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(1), (_lock) =>
                {
                    firstLockRun = true;

                    Exception ex = null;
                    bool run = false;

                    try
                    {
                        manager.RunWithLock(resource, TimeSpan.FromMinutes(1), TimeSpan.FromMilliseconds(50),
                            (_lock2) =>
                            {
                                run = true;
                            });
                    }
                    catch (Exception exc)
                    {
                        ex = exc;
                    }

                    Assert.True(ex != null, "Exception was expected");
                    Assert.False(run);

                    try
                    {
                        manager.RunWithLock(resource, TimeSpan.FromMinutes(1), TimeSpan.Zero,
                            (_lock3) =>
                            {
                                run = true;
                            });
                    }
                    catch (Exception exc)
                    {
                        ex = exc;
                    }

                    Assert.True(ex != null, "Exception was expected");
                    Assert.False(run);
                });

                Assert.True(firstLockRun);
            }
        }

        [Fact]
        public void ConcurrentWaitForLock()
        {
            var NTHREADS = 1;
            var signal = new ManualResetEvent(false);
            var counterContext = new CounterContext() { Counter = 0 };
            var contexts = Enumerable.Repeat<Func<ConcurrentContext>>(() => new ConcurrentContext(signal, counterContext, ConcurrentLock_LOOP, waitForLock: true), NTHREADS)
                                    .Select(a => a()).ToList();

            var threads = contexts.Select(c => new Thread(c.ConcurrentLock_Start)).ToList();
            threads.ForEach(t => t.Start());

            //Runs all threads at the same time.
            contexts.ForEach(c => c.ThreadStartSignal.WaitOne());
            signal.Set();

            //Waits until all threads finish
            threads.ForEach(t => t.Join());

            //Each thread adds ConcurrentLock_LOOP to the counter. If everything is ok the sum should be (ConcurrentLock_LOOP * NTHREADS threads);

            Assert.Equal(ConcurrentLock_LOOP * NTHREADS, counterContext.Counter);
        }

        private class CounterContext
        {
            public int Counter;
        }

        private class ConcurrentContext
        {
            ManualResetEvent _signal;
            CounterContext _counterContext;
            bool _waitForLock;
            int _loopCount;

            public ConcurrentContext(ManualResetEvent signal, CounterContext counterContext, int loopCount, bool waitForLock)
            {
                _counterContext = counterContext;
                _signal = signal;
                _waitForLock = waitForLock;
                _loopCount = loopCount;
                _ThreadStartSignal = new ManualResetEvent(false);
            }

            ManualResetEvent _ThreadStartSignal;
            public ManualResetEvent ThreadStartSignal { get { return _ThreadStartSignal; } }

            private static readonly TimeSpan OneMinute = new TimeSpan(TimeSpan.TicksPerMinute);

            public void ConcurrentLock_Start()
            {
                var manager = LockManagerFactory.Create(() => TestDbContext.Create());

                var resource = new Guid("65917ECA-4A6B-451B-AE90-33236023E822");
                LockInfo lockInfo = null;

                _ThreadStartSignal.Set();

                //Waits until all threads are initialized, then fire them together.
                _signal.WaitOne();

                for (int i = 0; i < _loopCount;)
                {
                    try
                    {
                        if (_waitForLock)
                        {
                            manager.RunWithLock(resource, OneMinute, OneMinute, (_lock) =>
                            {
                                IncreaseCounter();

                                i++;
                            });
                        }
                        else
                        {
                            var lockAcquired = manager.TryLock(resource, OneMinute, out lockInfo);

                            if (lockAcquired)
                            //if (true) //** uncoment this line and comment lockAcquired to ignore lock and debug that the test is properly implemented.
                            {
                                IncreaseCounter();

                                i++; // increase loop counter only when acquired lock.
                            }
                        }
                    }
                    finally
                    {
                        manager.Release(lockInfo);
                    }

                    //Allows the execution of other threads. 
                    Thread.Sleep(1);
                }
            }

            private void IncreaseCounter()
            {
                var counterRead = _counterContext.Counter;
                //Allows the execution of other threads. If there's no locks multiple threads will read the same value and the final sum will be wrong.
                Thread.Sleep(1);
                _counterContext.Counter = counterRead + 1;
            }
        }

        [Fact]
        public void LockReferenceOverflowSingleThread()
        {
            AsyncContext.Run(() => LockReferenceOverflow());
        }

        [Fact]
        public void LockReferenceOverflow()
        {
            var manager = LockManagerFactory.Create(() => TestDbContext.Create());

            var resource1 = new Guid("555279D1-8E95-483F-93ED-012DCE98EE73");
            LockInfo lockInfo1;

            manager.TryLock(resource1, new TimeSpan(TimeSpan.TicksPerSecond), out lockInfo1);
            Thread.Sleep(1050);
            Assert.False(lockInfo1.HasLock);

            using (var context = TestDbContext.Create())
            {
                var maxReference = new SqlParameter("lockReference", int.MaxValue);
                var resource = new SqlParameter("resource", lockInfo1.Resource);
                context.Database.ExecuteSqlCommand("UPDATE DistributedLocks Set LockReference = @lockReference where Resource = @resource", maxReference, resource);
            }

            manager.TryLock(resource1, new TimeSpan(TimeSpan.TicksPerMinute), out lockInfo1);
            Assert.True(lockInfo1.HasLock);

            manager.Release(lockInfo1);
        }

        [Fact]
        public void LockSpeedSingleThread()
        {
            AsyncContext.Run(() => LockSpeed());
        }

        [Fact]
        public void LockSpeed()
        {
            var manager = LockManagerFactory.Create(() => TestDbContext.Create());

            var resource1 = new Guid("315A4649-12FE-44B2-8402-BE7DB8F2ADB6");
            LockInfo lockInfo1;

            var watch = new Stopwatch();

            //ignores first lock speed;
            manager.TryLock(resource1, new TimeSpan(TimeSpan.TicksPerSecond), out lockInfo1);

            watch.Start();
            for (int i = 0; i < 1000; i++)
            {
                manager.TryLock(resource1, new TimeSpan(TimeSpan.TicksPerSecond), out lockInfo1);
                manager.Release(lockInfo1);
            }
            watch.Stop();

            Console.AppendLine("Elapsed (ms): " + watch.ElapsedMilliseconds);

            Assert.True(watch.ElapsedTicks < TimeSpan.TicksPerSecond * 5);
        }
    }
}