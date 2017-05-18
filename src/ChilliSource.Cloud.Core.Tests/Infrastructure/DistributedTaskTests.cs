using ChilliSource.Cloud.Core.Distributed;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ChilliSource.Cloud.Core.Tests
{
    [Collection(DistributedTestsCollection.Name)]
    public class DistributedTaskTests : IDisposable
    {
        private readonly StringBuilder Console = new StringBuilder();
        private readonly ITestOutputHelper _output;

        public DistributedTaskTests(ITestOutputHelper output)
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
        public void TestSingle()
        {
            var manager = TaskManagerFactory.Create(() => TestDbContext.Create(), new TaskManagerOptions() { MainLoopWait = 100 });

            manager.RegisterTaskType(typeof(MyTask1), new TaskSettings("D591C9C1-F034-4151-993D-AB2A76374994"));
            MyTask1.TaskExecuted = 0;
            var taskId = manager.EnqueueSingleTask<MyTask1>();

            manager.StartListener();

            Thread.Sleep(2000);
            manager.StopListener(waitTillStops: true);

            Assert.False(manager.IsListenning);
            Assert.True(manager.LatestListenerException == null);
            Assert.True(MyTask1.TaskExecuted == 1);

            using (var db = TestDbContext.Create())
            {
                var task = db.SingleTasks.Where(t => t.Id == taskId).FirstOrDefault();
                Assert.True(task.Status == SingleTaskStatus.Completed);
            }
        }

        [Fact]
        public void TestSingleById()
        {
            var manager = TaskManagerFactory.Create(() => TestDbContext.Create(), new TaskManagerOptions() { MainLoopWait = 100 });
            manager.StartListener();

            var guid = "139E764E-AA4D-4B99-9FF1-603C743ECC32";
            manager.RegisterTaskType(typeof(MyTask1), new TaskSettings(guid));
            manager.EnqueueSingleTask(new Guid(guid));

            MyTask1.TaskExecuted = 0;

            Thread.Sleep(3000);
            manager.StopListener(waitTillStops: true);

            Assert.True(manager.LatestListenerException == null);
            Assert.True(MyTask1.TaskExecuted == 1);
        }

        [Fact]
        public void TestSingleAndRecurrent()
        {
            var manager = TaskManagerFactory.Create(() => TestDbContext.Create(), new TaskManagerOptions() { MainLoopWait = 100 });
            manager.StartListener();

            manager.RegisterTaskType(typeof(MyTaskRecurrent1), new TaskSettings("BF168DA8-3D80-429E-93AF-0958E80128A1"));

            manager.EnqueueRecurrentTask<MyTaskRecurrent1>(1000);
            manager.EnqueueSingleTask<MyTaskRecurrent1>();

            MyTaskRecurrent1.TaskExecuted = 0;

            int tickCount = 0;
            manager.SubscribeToListener(() => { if (tickCount++ > 1) manager.StopListener(); });
            manager.WaitTillListenerStops();

            Assert.True(MyTaskRecurrent1.TaskExecuted > 0);
            Assert.True(manager.LatestListenerException == null);
        }

        [Fact]
        public void TestSingleParam()
        {
            var manager = TaskManagerFactory.Create(() => TestDbContext.Create(), new TaskManagerOptions() { MainLoopWait = 100 });
            manager.StartListener();

            manager.RegisterTaskType(typeof(MyTask2), new TaskSettings("8DCB87AA-FAE8-4E03-9A66-533705C4803C"));
            manager.EnqueueSingleTask<MyTask2, Task2Parameter>(new Task2Parameter() { Value = 456 });

            MyTask2.ParamValue = 0;

            int tickCount = 0;
            manager.SubscribeToListener(() => { if (tickCount++ > 1) manager.StopListener(); });
            manager.WaitTillListenerStops();

            Assert.True(MyTask2.ParamValue == 456);
            Assert.True(manager.LatestListenerException == null);
        }

        [Fact]
        public void TestSingleParamById()
        {
            var manager = TaskManagerFactory.Create(() => TestDbContext.Create(), new TaskManagerOptions() { MainLoopWait = 100 });

            var guid = "0FE51C65-C0F7-4BC0-9CD1-9BF8CC98AB3D";
            manager.RegisterTaskType(typeof(MyTask2), new TaskSettings(guid));
            manager.EnqueueSingleTask<Task2Parameter>(new Guid(guid), new Task2Parameter() { Value = 456 });

            MyTask2.ParamValue = 0;

            int tickCount = 0;
            manager.StartListener();
            manager.SubscribeToListener(() => { if (tickCount++ > 1) manager.StopListener(); });
            manager.WaitTillListenerStops();

            Assert.True(MyTask2.ParamValue == 456);
            Assert.True(manager.LatestListenerException == null);
        }

        [Fact]
        public void TestRecurrentTask()
        {
            var manager = TaskManagerFactory.Create(() => TestDbContext.Create(), new TaskManagerOptions() { MainLoopWait = 100 });
            manager.RegisterTaskType(typeof(MyTaskRecurrent1), new TaskSettings("C4A0CCD2-A04D-4E59-8346-2A8AA4E2EA0E"));

            MyTaskRecurrent1.TaskExecuted = 0;
            var recurrentId = manager.EnqueueRecurrentTask<MyTaskRecurrent1>(1000);

            int count = 0;
            manager.SubscribeToListener(() =>
            {
                count++;
                if (count > 1)
                {
                    Thread.Sleep(2000);
                    manager.StopListener();
                }
                else
                {
                    Thread.Sleep(1000);
                }
            });

            Thread.Sleep(1000);
            manager.StartListener();
            manager.WaitTillListenerStops();

            Assert.True(MyTaskRecurrent1.TaskExecuted > 1, $"Task should've executed more than once: {MyTaskRecurrent1.TaskExecuted}");
            Assert.True(manager.LatestListenerException == null, "No exception exptected");
        }


        [Fact]
        public void TestRecurrentTaskById()
        {
            var manager = TaskManagerFactory.Create(() => TestDbContext.Create(), new TaskManagerOptions() { MainLoopWait = 100 });
            var guid = "FDC1CFA0-197B-4548-A5D6-2A1183472C37";
            manager.RegisterTaskType(typeof(MyTaskRecurrent1), new TaskSettings(guid));

            MyTaskRecurrent1.TaskExecuted = 0;
            var recurrentId = manager.EnqueueRecurrentTask(new Guid(guid), 1000);

            int count = 0;
            manager.SubscribeToListener(() =>
            {
                count++;
                if (count > 1)
                {
                    Thread.Sleep(2000);
                    manager.StopListener();
                }
                else
                {
                    Thread.Sleep(1000);
                }
            });

            manager.StartListener();
            manager.WaitTillListenerStops();

            Console.AppendLine($"Recurrent Task executed {MyTaskRecurrent1.TaskExecuted} times.");

            Assert.True(MyTaskRecurrent1.TaskExecuted > 1, "Task should've executed more than once");
            Assert.True(manager.LatestListenerException == null, "No exception exptected");
        }

        [Fact]
        public void TestTypeNotRegisteredWithManager()
        {
            var manager = TaskManagerFactory.Create(() => TestDbContext.Create(), new TaskManagerOptions() { MainLoopWait = 100 });
            manager.StartListener();

            var otherManager = TaskManagerFactory.Create(() => TestDbContext.Create(), new TaskManagerOptions() { MainLoopWait = 100 });
            otherManager.RegisterTaskType(typeof(MyTask1), new TaskSettings("41CE95B5-A7DC-4A9B-A0C2-CA441639BF5E"));
            var taskId = otherManager.EnqueueSingleTask<MyTask1>();

            //The second manager is not listenning yet.
            //Waits for the first manager to pick up a non-registered task.            
            Thread.Sleep(3000);

            Assert.True(manager.IsListenning);

            manager.StopListener();
            otherManager.StopListener();

            Assert.True(manager.RemoveSingleTask(taskId));
            Assert.True(manager.LatestListenerException == null);
            Assert.True(otherManager.LatestListenerException == null);
        }

        [Fact]
        public void MultipleManagers()
        {
            var manager = TaskManagerFactory.Create(() => TestDbContext.Create(), new TaskManagerOptions() { MainLoopWait = 100 });
            var manager2 = TaskManagerFactory.Create(() => TestDbContext.Create(), new TaskManagerOptions() { MainLoopWait = 100 });
            var manager3 = TaskManagerFactory.Create(() => TestDbContext.Create(), new TaskManagerOptions() { MainLoopWait = 100 });
            var TOTAL_TASK_COUNT = 80;

            manager.StartListener();
            manager2.StartListener();
            manager3.StartListener();

            Thread.Sleep(300);

            var guid = "2A71134F-AED4-47D5-AB3C-69B3EBC87C00";
            manager.RegisterTaskType(typeof(MyTask), new TaskSettings(guid));
            manager2.RegisterTaskType(typeof(MyTask), new TaskSettings(guid));
            manager3.RegisterTaskType(typeof(MyTask), new TaskSettings(guid));

            MyTask.TaskExecuted = 0;
            MyTask.elements = new List<int?>();
            MyTask.duplicate = new List<int?>();
            var taskIds = new List<int>();

            for (int i = 0; i < TOTAL_TASK_COUNT; i++)
            {
                taskIds.Add(manager.EnqueueSingleTask<MyTask, int?>(i));
            }

            using (var db = TestDbContext.Create())
            {
                while (db.SingleTasks.Where(t => taskIds.Contains(t.Id)).Count(t => t.Status == SingleTaskStatus.Scheduled) > 0) { Thread.Sleep(333); }
            }

            Assert.True(manager.IsListenning);
            Assert.True(manager2.IsListenning);
            Assert.True(manager3.IsListenning);

            manager.StopListener(true);
            manager2.StopListener(true);
            manager3.StopListener(true);

            Assert.True(MyTask.TaskExecuted == TOTAL_TASK_COUNT);
            Assert.True(MyTask.elements.Count() == TOTAL_TASK_COUNT);
            Assert.True(MyTask.duplicate.Count() == 0);

            Assert.True(manager.TasksExecutedCount > 0);
            Assert.True(manager2.TasksExecutedCount > 0);
            Assert.True(manager3.TasksExecutedCount > 0);

            Console.AppendLine($"manager 1: {manager.TasksExecutedCount}");
            Console.AppendLine($"manager 2: {manager2.TasksExecutedCount}");
            Console.AppendLine($"manager 3: {manager3.TasksExecutedCount}");
            Assert.Equal(TOTAL_TASK_COUNT, manager.TasksExecutedCount + manager2.TasksExecutedCount + manager3.TasksExecutedCount);
        }

        [Fact]
        public void TestAutoRenewLock()
        {
            // MyTaskAutoRenewLock
            // AliveCycle - 1 sec
            // LockCycle - 2 secs
            // Runs for - 3 secs
            // Task Lock auto-renewed every 1 seconds

            var manager = TaskManagerFactory.Create(() => TestDbContext.Create(), new TaskManagerOptions() { MainLoopWait = 100 });

            manager.RegisterTaskType(typeof(MyTaskAutoRenewLock),
                new TaskSettings("1BE679EB-D6D6-4239-90FB-5116C031762F")
                {
                    AliveCycle = new TimeSpan(TimeSpan.TicksPerSecond)
                });

            var taskId = manager.EnqueueSingleTask<MyTaskAutoRenewLock>();

            MyTaskAutoRenewLock.TaskStarted = false;
            MyTaskAutoRenewLock.CancelationRequested = false;
            MyTaskAutoRenewLock.ProcessedAllRecords = false;

            int tickCount = 0;
            manager.StartListener();
            manager.SubscribeToListener(() => { if (tickCount++ > 3) manager.StopListener(); });
            manager.WaitTillListenerStops();

            Assert.True(MyTaskAutoRenewLock.TaskStarted, "TaskStarted should be true");
            Assert.False(MyTaskAutoRenewLock.CancelationRequested, "CancelationRequested should be false");

            using (var db = TestDbContext.Create())
            {
                var task = db.SingleTasks.Where(t => t.Id == taskId).FirstOrDefault();
                Assert.Equal(SingleTaskStatus.Completed, task.Status);
            }

            Assert.True(MyTaskAutoRenewLock.ProcessedAllRecords, "ProcessedAllRecords should be true");
            Assert.True(manager.LatestListenerException == null, "LatestListenerException should be null");
        }

        [Fact]
        public void TestAutoCancelTask()
        {
            // MyTaskAutoCancelTask
            // AliveCycle = 1 sec
            // LockCycle - 2 secs
            // Runs for - 3 secs

            var manager = TaskManagerFactory.Create(() => TestDbContext.Create(), new TaskManagerOptions() { MainLoopWait = 100 });

            manager.RegisterTaskType(typeof(MyTaskAutoCancelTask),
                new TaskSettings("73793DDA-CE7A-4B16-A751-4F90B5799AFA")
                {
                    AliveCycle = new TimeSpan(TimeSpan.TicksPerSecond)
                });

            manager.EnqueueSingleTask<MyTaskAutoCancelTask>();

            MyTaskAutoCancelTask.CancelationRequested = false;
            MyTaskAutoCancelTask.ProcessedAllRecords = null;

            int tickCount = 0;
            manager.StartListener();
            manager.SubscribeToListener(() => { if (tickCount++ > 1) manager.StopListener(); });
            manager.WaitTillListenerStops();

            Assert.True(MyTaskAutoCancelTask.CancelationRequested == true);
            Assert.True(MyTaskAutoCancelTask.ProcessedAllRecords == false);
            Assert.True(manager.LatestListenerException == null);
        }

        [Fact]
        public void TestForceCancelTask()
        {
            // MyTaskAutoCancelTask
            // AliveCycle = 1 sec
            // LockCycle - 2 secs
            // Runs for - 1 hours (till force cancelled)

            var manager = TaskManagerFactory.Create(() => TestDbContext.Create(), new TaskManagerOptions() { MainLoopWait = 100 });

            manager.RegisterTaskType(typeof(MyTaskForceCancelTask),
                new TaskSettings("52BB4E12-3237-4B89-885E-ECEC41C56DE5")
                {
                    AliveCycle = new TimeSpan(TimeSpan.TicksPerSecond)
                });

            manager.EnqueueSingleTask<MyTaskForceCancelTask>();

            MyTaskForceCancelTask.ExecutedTillEnd = null;
            MyTaskForceCancelTask.CancelationRequested = false;

            int tickCount = 0;
            manager.StartListener();
            manager.SubscribeToListener(() => { if (tickCount++ > 1) manager.StopListener(); });
            manager.WaitTillListenerStops();

            Assert.True(MyTaskForceCancelTask.CancelationRequested == true);
            Assert.True(MyTaskForceCancelTask.ExecutedTillEnd == false);
            Assert.True(manager.LatestListenerException == null);
        }

        [Fact]
        public void TestAbandonedTask()
        {
            var manager = TaskManagerFactory.Create(() => TestDbContext.Create(), new TaskManagerOptions() { MainLoopWait = 100 });
            manager.StartListener();

            manager.RegisterTaskType(typeof(MyTaskLongTask),
                new TaskSettings("7EBA6938-15A7-4C9F-9CA2-5DC3C5EFE6EA")
                {
                    AliveCycle = new TimeSpan(Convert.ToInt64(TimeSpan.TicksPerSecond * 2.5))
                });

            var taskId = manager.EnqueueSingleTask<MyTaskLongTask>();

            int tickCount = 0;
            manager.SubscribeToListener(() => { if (tickCount++ > 0) manager.StopListener(); });
            manager.WaitTillListenerStops();

            using (var db = TestDbContext.Create())
            {
                var task = db.SingleTasks.Where(t => t.Id == taskId).FirstOrDefault();
                Assert.True(task.Status == SingleTaskStatus.CompletedAborted, "Status should be CompletedAborted");

                //Fake running status
                task.SetStatus(SingleTaskStatus.Running);
                task.LockedUntil = DateTime.UtcNow.AddDays(-1); // Ideally should use database-based clock

                db.SaveChanges();
            }

            var manager2 = TaskManagerFactory.Create(() => TestDbContext.Create(), new TaskManagerOptions() { MainLoopWait = 100 });
            //starts listener
            manager2.StartListener();
            Thread.Sleep(500);
            manager2.StopListener(true);

            using (var db = TestDbContext.Create())
            {
                var task = db.SingleTasks.Where(t => t.Id == taskId).FirstOrDefault();
                Assert.True(task.Status == SingleTaskStatus.CompletedAbandoned, "Status should be CompletedAbandoned");
            }
        }

        [Fact]
        public void TestForceCancelTaskAndRecovery()
        {
            // MyTaskAutoCancelTask
            // AliveCycle = 1 sec
            // LockCycle - 2 secs
            // Runs for - 1 hours (till force cancelled)

            var options = new TaskManagerOptions() { MainLoopWait = 100, MaxWorkerThreads = 1 };
            var manager = TaskManagerFactory.Create(() => TestDbContext.Create(), options);

            manager.RegisterTaskType(typeof(MyTaskLongTask),
                new TaskSettings("CBC68EFE-532D-44C7-87D8-3443636F6425")
                {
                    AliveCycle = new TimeSpan(TimeSpan.TicksPerSecond)
                });

            var longTaskId = manager.EnqueueSingleTask<MyTaskLongTask>();

            manager.StartListener();
            Thread.Sleep(500);

            manager.RegisterTaskType(typeof(MyTask), new TaskSettings("BC54E6F5-9EDF-48C2-8765-269BCC472DDF"));
            var taskId = manager.EnqueueSingleTask<MyTask>();

            int tickCount = 0;
            manager.SubscribeToListener(() => { if (tickCount++ > 2) { manager.StopListener(); } });
            manager.WaitTillListenerStops();

            Assert.True(manager.LatestListenerException == null);

            using (var db = TestDbContext.Create())
            {
                var longTask = db.SingleTasks.Where(t => t.Id == longTaskId).FirstOrDefault();
                Assert.True(longTask.Status == SingleTaskStatus.CompletedAborted);

                var task = db.SingleTasks.Where(t => t.Id == taskId).FirstOrDefault();
                Assert.True(task.Status == SingleTaskStatus.Completed);
            }
        }

        public class MyTaskAutoRenewLock : IDistributedTask<object>
        {
            public static bool TaskStarted;
            public static bool CancelationRequested;
            public static bool ProcessedAllRecords;

            public void Run(object parameter, ITaskExecutionInfo executionInfo)
            {
                TaskStarted = true;
                int i = 0;
                //Runs for 3 secs
                for (; i < 30; i++)
                {
                    executionInfo.SendAliveSignal();

                    if (CancelationRequested = executionInfo.IsCancellationRequested)
                        break;

                    Thread.Sleep(100);
                }

                ProcessedAllRecords = (i == 30);
            }
        }

        public class MyTaskAutoCancelTask : IDistributedTask<object>
        {
            public static bool CancelationRequested;
            public static bool? ProcessedAllRecords;

            public void Run(object parameter, ITaskExecutionInfo executionInfo)
            {
                int i = 0;
                //Runs for 3 sec
                for (; i < 30 && !(CancelationRequested = executionInfo.IsCancellationRequested); i++)
                {
                    //No alive notification sent. The task should be cancelled.

                    Thread.Sleep(100);
                }

                ProcessedAllRecords = (i == 30);
            }
        }

        public class MyTaskForceCancelTask : IDistributedTask<object>
        {
            public static bool? ExecutedTillEnd;
            public static bool CancelationRequested;

            public void Run(object parameter, ITaskExecutionInfo executionInfo)
            {
                ExecutedTillEnd = false;
                int i = 0;
                while (!(CancelationRequested = executionInfo.IsCancellationRequested))
                {
                    i++;
                    Thread.Sleep(500);
                }

                //Sleeps for a long period and no alive signal sent.
                Thread.Sleep(60 * 60 * 1000);

                ExecutedTillEnd = true;
            }
        }

        public class MyTask : IDistributedTask<int?>
        {
            private static readonly object _lock = new object();
            public static int TaskExecuted;
            public static List<int?> elements = new List<int?>();
            public static List<int?> duplicate = new List<int?>();

            public void Run(int? parameter, ITaskExecutionInfo executionInfo)
            {
                lock (_lock)
                {
                    if (elements.Contains(parameter))
                    {
                        duplicate.Add(parameter);
                    }
                    else
                    {
                        elements.Add(parameter);
                    }
                }

                Thread.Sleep(1);

                lock (_lock)
                {
                    TaskExecuted++;
                }
                Thread.Sleep(1);
            }
        }

        public class MyTaskLongTask : IDistributedTask<object>
        {
            public void Run(object parameter, ITaskExecutionInfo executionInfo)
            {
                //Sleeps for a long period and no alive signal sent.
                Thread.Sleep(60 * 60 * 1000);
            }
        }

        public class Task2Parameter
        {
            public int Value { get; set; }
        }

        public class MyTask1 : IDistributedTask<int?>
        {
            private static readonly object _lock = new object();
            public static int TaskExecuted;
            public static List<int?> elements = new List<int?>();
            public static List<int?> duplicate = new List<int?>();

            public void Run(int? parameter, ITaskExecutionInfo executionInfo)
            {
                lock (_lock)
                {
                    if (elements.Contains(parameter))
                    {
                        duplicate.Add(parameter);
                    }
                    else
                    {
                        elements.Add(parameter);
                    }
                }

                Thread.Sleep(1);

                lock (_lock)
                {
                    TaskExecuted++;
                }
                Thread.Sleep(1);
            }
        }

        public class MyTaskRecurrent1 : IDistributedTask<object>
        {
            private static readonly object _lock = new object();
            public static int TaskExecuted;

            public void Run(object parameter, ITaskExecutionInfo executionInfo)
            {
                lock (_lock)
                {
                    TaskExecuted++;
                }
                Thread.Sleep(1);
            }
        }

        public class MyTask2 : IDistributedTask<Task2Parameter>
        {
            public static int ParamValue;

            public void Run(Task2Parameter parameter, ITaskExecutionInfo executionInfo)
            {
                ParamValue = parameter.Value;
            }
        }
    }
}
