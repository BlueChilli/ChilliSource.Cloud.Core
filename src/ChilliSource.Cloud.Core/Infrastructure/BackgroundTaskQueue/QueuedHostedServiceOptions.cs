#if !NET_4X
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilliSource.Cloud.Core
{
    public class QueuedHostedServiceOptions
    {
        public QueuedHostedServiceOptions()
        {
            MaxConcurrency = 200;
        }

        int _maxConcurrency;
        /// <summary>
        /// Defines the maximum number of tasks executing in parallel. This could be superseeded by the TaskScheduler settings.
        /// It defaults to 200
        /// </summary>
        public int MaxConcurrency
        {
            get
            {
                return _maxConcurrency;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentException("MaxConcurrency value must be at least 1.");
                _maxConcurrency = value;
            }
        }
    }
}
#endif