using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    public static class BoolExtensions
    {
        /// <summary>
        /// Converts the specified Boolean value to the equivalent 32-bit signed integer.
        /// </summary>
        /// <param name="value">The Boolean value to convert</param>
        /// <returns>The number 1 if value is true; otherwise, 0.</returns>
        public static int ToInt(this bool value)
        {
            return Convert.ToInt32(value);
        }

        /// <summary>
        /// Negates !the specified Boolean value.
        /// </summary>
        public static bool Toggle(this bool value)
        {
            return !value;
        }      
    }
}
