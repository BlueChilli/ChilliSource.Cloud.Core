using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChilliSource.Cloud
{
    /// <summary> 
    /// Interface for dealing with randomness. 
    /// </summary> 
    public interface IThreadSafeRandom
    {
        /// <summary>See <see cref="Random.Next()" /></summary> 
        int Next();
        /// <summary>See <see cref="Random.Next(int)" /></summary> 
        int Next(int maxValue);

        /// <summary>See <see cref="Random.Next(int, int)" /></summary> 
        int Next(int minValue, int maxValue);

        /// <summary>See <see cref="Random.NextDouble()" /></summary> 
        double NextDouble();

        /// <summary>See <see cref="Random.NextBytes(byte[])" /></summary> 
        void NextBytes(byte[] buffer);
    }

    /// <summary>
    /// Creates a thread-safe random generator
    /// </summary>
    public static class ThreadSafeRandom
    {
        private static readonly ThreadSafeRandomInstance _singleton = new ThreadSafeRandomInstance();

        /// <summary>
        ///     Creates a thread-safe random generator
        /// </summary>
        /// <returns>thread-safe random generator</returns>
        public static IThreadSafeRandom Get() { return _singleton; }
    }

    /// <summary> 
    /// Convenience class for dealing with randomness. 
    /// </summary> 
    internal class ThreadSafeRandomInstance : IThreadSafeRandom
    {
        /// <summary> 
        /// Random number generator used to generate seeds, 
        /// which are then used to create new random number 
        /// generators on a per-thread basis. 
        /// </summary> 
        private static readonly Random globalRandom = new Random();
        private static readonly object globalLock = new object();

        /// <summary> 
        /// Random number generator 
        /// </summary> 
        private static readonly ThreadLocal<Random> threadRandom = new ThreadLocal<Random>(() =>
        {
            lock (globalLock)
            {
                return new Random(globalRandom.Next());
            }
        });

        /// <summary>See <see cref="Random.Next()" /></summary> 
        public int Next()
        {
            return threadRandom.Value.Next();
        }

        /// <summary>See <see cref="Random.Next(int)" /></summary> 
        public int Next(int maxValue)
        {
            return threadRandom.Value.Next(maxValue);
        }

        /// <summary>See <see cref="Random.Next(int, int)" /></summary> 
        public int Next(int minValue, int maxValue)
        {
            return threadRandom.Value.Next(minValue, maxValue);
        }

        /// <summary>See <see cref="Random.NextDouble()" /></summary> 
        public double NextDouble()
        {
            return threadRandom.Value.NextDouble();
        }

        /// <summary>See <see cref="Random.NextBytes(byte[])" /></summary> 
        public void NextBytes(byte[] buffer)
        {
            threadRandom.Value.NextBytes(buffer);
        }
    }
}
