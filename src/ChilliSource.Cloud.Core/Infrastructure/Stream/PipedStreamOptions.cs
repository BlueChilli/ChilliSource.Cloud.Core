using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    public class PipedStreamOptions
    {
        public PipedStreamOptions()
        {
            this.BlockSize = 32 * 1024;
            this.MaxBlocks = 1;
            this.Multiplexed = false;
            this.WriteTimeout = null;
            this.ReadTimeout = null;
            this.AutoFlush = false;
        }

        public int BlockSize { get; set; }
        public int MaxBlocks { get; set; }

        public TimeSpan? WriteTimeout { get; set; }
        public TimeSpan? ReadTimeout { get; set; }
        public bool Multiplexed { get; set; }
        public bool AutoFlush { get; set; }

        internal void EnsureValidity()
        {
            if (BlockSize < 1)
                throw new ApplicationException("Invalid BlockSize value.");

            if (MaxBlocks < 1)
                throw new ArgumentException("Invalid MaxBlocks value.");

            if (WriteTimeout != null &&
                (WriteTimeout.Value.TotalMilliseconds <= 0 || WriteTimeout.Value.TotalMilliseconds > Int32.MaxValue))
                throw new ArgumentException("Invalid WriteTimeout value.");

            if (ReadTimeout != null &&
                (ReadTimeout.Value.TotalMilliseconds <= 0 || ReadTimeout.Value.TotalMilliseconds > Int32.MaxValue))
                throw new ArgumentException("Invalid ReadTimeout value.");
        }

        internal PipedStreamOptions Clone()
        {
            return (PipedStreamOptions)this.MemberwiseClone();
        }
    }
}
