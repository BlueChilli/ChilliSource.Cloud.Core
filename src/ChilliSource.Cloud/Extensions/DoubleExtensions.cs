using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Extensions
{
    /// <summary>
    /// Extension methods for double value.
    /// </summary>
    public static class DoubleExtensions
    {
        /// <summary>
        /// Converts the double value to its equivalent radian representation.
        /// </summary>
        /// <param name="source">The double value to convert.</param>
        /// <returns>The double value in radian representation.</returns>
        public static double ToRadian(this double source)
        {
            return (Math.PI / 180) * source;
        }

        /// <summary>
        /// Converts the double value from radian representation.
        /// </summary>
        /// <param name="source">The double value in radian representation.</param>
        /// <returns>The double value converted from radian representation.</returns>
        public static double FromRadian(this double source)
        {
            return (source * 180) / Math.PI;
        }
    }
}
