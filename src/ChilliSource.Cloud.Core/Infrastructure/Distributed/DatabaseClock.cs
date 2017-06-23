using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    internal class DatabaseClock : IClock
    {
        private readonly DateTime _startTime;
        private readonly Stopwatch _stopWatch;
        private readonly TimeSpan _latency;

        public static async Task<DatabaseClock> CreateAsync(Func<Task<DateTime>> dbTimeGetterTask)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            var dbTime = await dbTimeGetterTask();
            var queryInterval = watch.Elapsed;
            var latency = new TimeSpan(queryInterval.Ticks / 2); //latency estimate

            var startTime = new DateTime(dbTime.Ticks - latency.Ticks);

            return new DatabaseClock(startTime, watch, latency);
        }

        private DatabaseClock(DateTime startTime, Stopwatch watch, TimeSpan latency)
        {
            this._startTime = startTime;
            this._stopWatch = watch;
            this._latency = latency;
        }

        public TimeSpan Latency { get { return _latency; } }

        public DateTime UtcNow
        {
            get
            {
                if (!_stopWatch.IsRunning)
                    throw new ApplicationException("Clock is not running");

                var elapsed = _stopWatch.Elapsed;
                return _startTime.Add(elapsed);
            }
        }
    }
}
