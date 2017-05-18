using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    internal interface IClock
    {
        DateTime UtcNow { get; }
    }

    internal interface IClockProvider
    {
        IClock GetClock();
    }

    internal class RelativeClock : IClock
    {
        private readonly TimeSpan _timeDiff;
        private readonly IClock _clock;

        public RelativeClock(IClock clock, TimeSpan timeDiff)
        {
            _clock = clock;
            _timeDiff = timeDiff;
        }

        public DateTime UtcNow
        {
            get
            {
                return _clock.UtcNow.Add(_timeDiff);
            }
        }
    }

    internal class SystemClock : IClock
    {
        public static readonly SystemClock Instance = new SystemClock();
        public DateTime UtcNow => DateTime.UtcNow;
    }

    internal class SystemClockProvider : IClockProvider
    {
        public IClock GetClock()
        {
            return SystemClock.Instance;
        }
    }
}
