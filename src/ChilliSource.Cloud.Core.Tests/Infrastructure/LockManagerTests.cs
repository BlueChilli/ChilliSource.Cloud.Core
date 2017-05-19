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
    public class LockManagerTests: IDisposable
    {
        private readonly StringBuilder Console = new StringBuilder();
        private readonly ITestOutputHelper _output;

        public LockManagerTests(ITestOutputHelper output)
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
        public void BasicTest()
        {
            BasicTestImpl("B5FF501B-383A-49DD-BD84-74511F70FCE9");
        }

        private void BasicTestImpl(string guid)
        {
            using (var manager = LockManagerFactory.Create(() => TestDbContext.Create()))
            {
                var resource1 = new Guid(guid);
                LockInfo lockInfo1;

                var firstLock = manager.TryLock(resource1, new TimeSpan(TimeSpan.TicksPerMinute), out lockInfo1);
                Assert.True(firstLock && lockInfo1.AsImmutable().HasLock());
                manager.Release(lockInfo1);

                Assert.False(lockInfo1.AsImmutable().HasLock());
            }
        }

        [Fact]
        public void BasicTestSingleThread()
        {
            AsyncContext.Run(() => BasicTestImpl("FA4A43A7-4671-4664-88EB-7995CFEF98F1"));
        }

        [Fact]
        public async Task BasicTestAsync()
        {
            using (ILockManagerAsync manager = LockManagerFactory.Create(() => TestDbContext.Create()))
            {
                var resource1 = new Guid("A80E0BEC-7E4C-4FB1-B8A9-8D21355E596D");
                var lockInfo1 = await manager.TryLockAsync(resource1, new TimeSpan(TimeSpan.TicksPerMinute));
                Assert.True(lockInfo1 != null && lockInfo1.AsImmutable().HasLock());
                await manager.ReleaseAsync(lockInfo1);

                Assert.False(lockInfo1.AsImmutable().HasLock());
            }
        }

        [Fact]
        public void DoubleLockSingleThread()
        {
            AsyncContext.Run(() => DoubleLockImpl("636DDD37-A028-4D5E-A82B-1A813DB10EEF", "814C6456-CBC4-4C84-8E1F-E5833FA5A9A6"));
        }

        [Fact]
        public void DoubleLock()
        {
            DoubleLockImpl("ACC9D515-1529-49BD-AECE-E163D9200E0F", "A7FE17DA-BE0F-40FD-8AE6-7F1F7C615C72");
        }

        private void DoubleLockImpl(string guid1, string guid2)
        {
            using (var manager = LockManagerFactory.Create(() => TestDbContext.Create()))
            {
                var resource1 = new Guid(guid1);
                var resource2 = new Guid(guid2);
                LockInfo lockInfo1, lockInfo2, lockInfo3;

                var firstLock = manager.TryLock(resource1, new TimeSpan(TimeSpan.TicksPerMinute * 2), out lockInfo1);
                manager.TryLock(resource1, new TimeSpan(TimeSpan.TicksPerMinute), out lockInfo2);
                manager.TryLock(resource2, new TimeSpan(TimeSpan.TicksPerMinute), out lockInfo3);

                Assert.True(firstLock && lockInfo1.AsImmutable().HasLock());
                Assert.False(lockInfo2.AsImmutable().HasLock());
                Assert.True(lockInfo3.AsImmutable().HasLock());

                manager.Release(lockInfo1);
                manager.Release(lockInfo3);

                manager.TryLock(resource1, new TimeSpan(TimeSpan.TicksPerMinute), out lockInfo2);
                Assert.True(lockInfo2.AsImmutable().HasLock());

                manager.Release(lockInfo2);

                Assert.False(lockInfo1.AsImmutable().HasLock());
                Assert.False(lockInfo2.AsImmutable().HasLock());
                Assert.False(lockInfo3.AsImmutable().HasLock());
            }
        }

        [Fact]
        public void RenewLockSingleThread()
        {
            AsyncContext.Run(() => RenewLockImpl("76546919-E163-4F31-BC1E-ED567B884542"));
        }

        [Fact]
        public void RenewLock()
        {
            RenewLockImpl("64FC617D-7469-4C1D-B64A-0342A7C44D57");
        }

        private void RenewLockImpl(string guid)
        {
            //*** TIME-SENSITIVE TEST, don't use debug-mode
            using (var manager = LockManagerFactory.Create(() => TestDbContext.Create()))
            {
                var resource1 = new Guid(guid);
                LockInfo lockInfo1, lockInfo2;

                manager.TryLock(resource1, new TimeSpan((int)(1.5 * TimeSpan.TicksPerSecond)), out lockInfo1);
                Assert.True(lockInfo1.AsImmutable().HasLock(), "Should've acquired lock");

                Thread.Sleep(500);
                var renewed = manager.TryRenewLock(lockInfo1); // 1 sec renew
                Assert.True(renewed, "Should have renewed lock after 500 ms");

                Thread.Sleep(1000);
                Assert.True(lockInfo1.AsImmutable().HasLock(), "Should have lock after 1000 ms");

                Thread.Sleep(1000); //total 2000 ms sleep time
                Assert.False(lockInfo1.AsImmutable().HasLock(), "Should NOT have lock after 2000 ms");

                manager.TryLock(resource1, new TimeSpan(TimeSpan.TicksPerSecond), out lockInfo2);
                Assert.True(lockInfo2.AsImmutable().HasLock(), "Should've acquired lock again");

                manager.Release(lockInfo1);
                manager.Release(lockInfo2);
            }
        }

        [Fact]
        public void RenewLock2SingleThread()
        {
            AsyncContext.Run(() => RenewLock2Impl("2414CB3C-1441-4497-BD99-B9509E79244A"));
        }

        [Fact]
        public void RenewLock2()
        {
            RenewLock2Impl("AFE160D8-0172-4F0B-8A83-E44489080541");
        }

        private void RenewLock2Impl(string guid)
        {
            //*** TIME-SENSITIVE TEST, don't use debug-mode
            using (var manager = LockManagerFactory.Create(() => TestDbContext.Create()))
            {
                var resource1 = new Guid(guid);
                LockInfo lockInfo1;

                manager.TryLock(resource1, new TimeSpan(TimeSpan.TicksPerSecond), out lockInfo1);
                Assert.True(lockInfo1.AsImmutable().HasLock());
                Thread.Sleep(1100);
                Assert.False(lockInfo1.AsImmutable().HasLock());

                manager.TryRenewLock(lockInfo1, retryLock: false);
                Assert.False(lockInfo1.AsImmutable().HasLock());

                manager.TryRenewLock(lockInfo1, retryLock: true);
                Assert.True(lockInfo1.AsImmutable().HasLock());

                manager.Release(lockInfo1);
            }
        }

        [Fact]
        public void LockReferenceOverflowSingleThread()
        {
            AsyncContext.Run(() => LockReferenceOverflowImpl("0A323B23-5BA4-469A-8249-0BF7B678FE73"));
        }

        [Fact]
        public void LockReferenceOverflow()
        {
            LockReferenceOverflowImpl("555279D1-8E95-483F-93ED-012DCE98EE73");
        }

        private void LockReferenceOverflowImpl(string guid)
        {
            using (var manager = LockManagerFactory.Create(() => TestDbContext.Create()))
            {
                var resource1 = new Guid(guid);
                LockInfo lockInfo1;

                manager.TryLock(resource1, new TimeSpan(TimeSpan.TicksPerSecond), out lockInfo1);
                Thread.Sleep(1050);
                Assert.False(lockInfo1.AsImmutable().HasLock());

                using (var context = TestDbContext.Create())
                {
                    var maxReference = new SqlParameter("lockReference", int.MaxValue);
                    var resource = new SqlParameter("resource", lockInfo1.Resource);
                    context.Database.ExecuteSqlCommand("UPDATE DistributedLocks Set LockReference = @lockReference where Resource = @resource", maxReference, resource);
                }

                manager.TryLock(resource1, new TimeSpan(TimeSpan.TicksPerMinute), out lockInfo1);
                Assert.True(lockInfo1.AsImmutable().HasLock());

                manager.Release(lockInfo1);
            }
        }

        const int ConcurrentLock_LOOP = 100;
        [Fact]
        public void ConcurrentLock()
        {
            var NTHREADS = 10;
            var signal = new ManualResetEvent(false);
            var counterContext = new CounterContext() { Counter = 0 };
            var resource = new Guid("65917ECA-4A6B-451B-AE90-33236023E822");

            var contexts = Enumerable.Repeat<Func<ConcurrentContext>>(() => new ConcurrentContext(resource, signal, counterContext, ConcurrentLock_LOOP, waitForLock: false), NTHREADS)
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
        public void ConcurrentWaitForLock()
        {
            var NTHREADS = 10;
            var signal = new ManualResetEvent(false);
            var counterContext = new CounterContext() { Counter = 0 };
            var resource = new Guid("213EEF97-B05B-4A0D-8E99-0C46EA2DBF6F");

            var contexts = Enumerable.Repeat<Func<ConcurrentContext>>(() => new ConcurrentContext(resource, signal, counterContext, 1, waitForLock: true), NTHREADS)
                                    .Select(a => a()).ToList();

            var threads = contexts.Select(c => new Thread(c.ConcurrentLock_Start)).ToList();
            threads.ForEach(t => t.Start());

            //Runs all threads at the same time.
            contexts.ForEach(c => c.ThreadStartSignal.WaitOne());
            signal.Set();

            //Waits until all threads finish
            threads.ForEach(t => t.Join());

            //Each thread adds ConcurrentLock_LOOP to the counter. If everything is ok the sum should be (ConcurrentLock_LOOP * NTHREADS threads);

            Assert.Equal(NTHREADS, counterContext.Counter);
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
            Guid _resource;

            public ConcurrentContext(Guid resource, ManualResetEvent signal, CounterContext counterContext, int loopCount, bool waitForLock)
            {
                _resource = resource;
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
                using (var manager = LockManagerFactory.Create(() => TestDbContext.Create()))
                {
                    LockInfo lockInfo = null;

                    _ThreadStartSignal.Set();

                    //Waits until all threads are initialized, then fire them together.
                    _signal.WaitOne();

                    for (int i = 0; i < _loopCount;)
                    {
                        try
                        {
                            var lockAcquired = _waitForLock ? manager.WaitForLock(_resource, OneMinute, OneMinute, out lockInfo)
                                                            : manager.TryLock(_resource, OneMinute, out lockInfo);

                            if (lockAcquired)
                            //if (true) //** uncoment this line and comment lockAcquired to ignore lock and debug that the test is properly implemented.
                            {
                                var counterRead = _counterContext.Counter;
                                //Allows the execution of other threads. If there's no locks multiple threads will read the same value and the final sum will be wrong.
                                Thread.Sleep(1);
                                _counterContext.Counter = counterRead + 1;

                                i++; // increase loop counter only when acquired lock.
                            }
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                        finally
                        {
                            manager.Release(lockInfo);
                        }

                        //Allows the execution of other threads. 
                        Thread.Sleep(1);
                    }
                }
            }
        }

        [Fact]
        public void LockSpeedSingleThread()
        {
            AsyncContext.Run(() => LockSpeed());
        }

        [Fact]
        public void LockSpeed()
        {
            using (var manager = LockManagerFactory.Create(() => TestDbContext.Create()))
            {
                var resource1 = new Guid("315A4649-12FE-44B2-8402-BE7DB8F2ADB6");
                LockInfo lockInfo1;

                var watch = new Stopwatch();

                //ignores first lock speed;
                manager.TryLock(resource1, new TimeSpan(TimeSpan.TicksPerSecond), out lockInfo1);
                manager.Release(lockInfo1);

                watch.Start();
                for (int i = 0; i < 100; i++)
                {
                    var locked = manager.TryLock(resource1, new TimeSpan(TimeSpan.TicksPerSecond), out lockInfo1);
                    Assert.True(locked, "Should've acquired lock.");
                    manager.Release(lockInfo1);
                }
                watch.Stop();

                Assert.True(watch.Elapsed.Ticks < TimeSpan.TicksPerSecond * 3, $"A hundred locks should take less than 3 secs. Total time: {watch.Elapsed.TotalSeconds} secs");
                Console.AppendLine($"Total time: {watch.Elapsed.TotalSeconds} secs");
            }
        }
    }
}