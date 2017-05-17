using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ChilliSource.Cloud.Core.Tests
{
    public class ThreadSafeRandomTests : IDisposable
    {
        private readonly StringBuilder Console = new StringBuilder();
        private readonly ITestOutputHelper _output;

        public ThreadSafeRandomTests(ITestOutputHelper output)
        {
            _output = output;
        }

        public void Dispose()
        {
            _output.WriteLine(Console.ToString());
        }

        [Fact]
        public void TestRandomQuality()
        {
            var MAX_NUMBER = 100;
            var startEvent = new ManualResetEvent(false);
            var containers = Enumerable.Repeat<Func<ThreadContainer>>(() => new ThreadContainer(startEvent, maxNumber: MAX_NUMBER), 20)
                                .Select(a => a()).ToList();

            containers.ForEach(c => c.WaitThreadStart()); //waits for all threads to start
            startEvent.Set(); //runs all threads at the same time.
            containers.ForEach(c => c.WaitThreadEnd()); //waits for all threads to finish

            var distributions = containers.Select(c => CreateDistribution(c.Numbers)).ToList();

            var allNumbers = containers.SelectMany(c => c.Numbers).ToList();
            var totaldistribution = CreateDistribution(allNumbers);

            for (int i = 1; i <= MAX_NUMBER; i++)
            {
                Console.Append($"[Number {i.ToString().PadLeft(3, '0')}]: ");
                Console.Append($"T({AssertDist(totaldistribution[i])}) |");

                distributions.ForEach(d => Console.Append($"{AssertDist(d[i])} |"));

                Console.AppendLine();
            }
        }

        private string AssertDist(double v)
        {
            Assert.True(v > 0.8 && v < 1.2);
            return v.ToString("N4");
        }

        private Dictionary<int, double> CreateDistribution(IList<int> numbers)
        {
            double total = numbers.Count;
            var dict = numbers.AsQueryable().GroupBy(n => n)
                            .ToDictionary(g => g.Key, g => Math.Round((g.Count() / total) * 100d, 4));

            return dict;
        }

        public class ThreadContainer
        {
            Thread _thread;
            List<int> _numbers = new List<int>();
            int _exclusiveUpper;
            WaitHandle _startEvent;
            ManualResetEvent _threadStartedEvent;

            public ThreadContainer(WaitHandle startEvent, int maxNumber)
            {
                _exclusiveUpper = maxNumber + 1;
                _startEvent = startEvent;
                _threadStartedEvent = new ManualResetEvent(false);

                _thread = new Thread(thread_start);
                _thread.Start();
            }

            public void WaitThreadStart()
            {
                _threadStartedEvent.WaitOne();
            }

            public void WaitThreadEnd()
            {
                _thread.Join();
            }

            public IList<int> Numbers { get { return _numbers; } }

            private void thread_start()
            {
                _threadStartedEvent.Set();
                _startEvent.WaitOne();

                var random = ThreadSafeRandom.Get();
                var maxloop = (_exclusiveUpper - 1) * 1000;
                for (int i = 0; i < maxloop; i++)
                {
                    var value = random.Next(1, _exclusiveUpper);
                    _numbers.Add(value);
                }
            }
        }
    }
}
