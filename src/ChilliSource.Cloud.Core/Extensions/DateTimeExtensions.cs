using Humanizer;
using NodaTime;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    /// <summary>
    /// Extension methods for DateTime object.
    /// </summary>
    public static class DateTimeExtensions
    {

        #region Timezone
        /// <summary>
        /// Converts the value of specified date between two specified time zones. (Used to convert server time to user time, so times display correctly).
        /// </summary>
        /// <param name="value">The specified date.</param>
        /// <param name="toTimezone">Time zone which the specified date converted to.</param>
        /// <param name="fromTimezone">Time zone which the specified date converted from.</param>
        /// <returns>A value of specified date in specified time zone.</returns>
        public static DateTime ToTimezone(this DateTime value, string toTimezone, string fromTimezone = "UTC")
        {
            if (toTimezone == null)
            {
                throw new ArgumentNullException("toTimezone is null");
            }

            if (fromTimezone == null)
            {
                throw new ArgumentNullException("fromTimezone is null");
            }

            var oldTz = DateTimeZoneProviders.Tzdb[fromTimezone];
            var newTz = DateTimeZoneProviders.Tzdb[toTimezone];
            var dt = LocalDateTime.FromDateTime(value);
            var zdt = dt.InZoneLeniently(oldTz);
            return zdt.WithZone(newTz).ToDateTimeOffset().DateTime;
        }

        /// <summary>
        /// Converts the value of specified date between two specified time zones. (Used to convert server time to user time, so times display correctly).
        /// </summary>
        /// <param name="value">The specified date.</param>
        /// <param name="toTimezone">Time zone which the specified date converted to.</param>
        /// <param name="fromTimezone">Time zone which the specified date converted from.</param>
        /// <returns>A value of specified date in specified time zone.</returns>
        public static DateTime ToUtcTimezone(this DateTime value, string fromTimezone)
        {
            return value.ToTimezone("UTC", fromTimezone);
        }
        #endregion

        #region Custom date formatting

        /// <summary>
        /// Converts the value of the specified date to its equivalent string representation based on specified options.
        /// </summary>
        /// <param name="dt">The specified date.</param>
        /// <param name="showYear">True to display year in string returned, otherwise not.</param>
        /// <param name="shortMonth">True to display abbreviated month name in string returned, otherwise to display full month name in string returned.</param>
        /// <param name="dayOfWeek">Display day of week in string returned based on the value of BlueChilli.Lib.CustomDateDayOfWeek.</param>
        /// <param name="uppercase">True to use upper case for string returned, otherwise to use lower case for string returned.</param>
        /// <returns>A string that represents the value of this instance based on specified options.</returns>
        public static string ToCustomDate(this DateTime dt, bool showYear = true, bool shortMonth = false, CustomDateDayOfWeek dayOfWeek = CustomDateDayOfWeek.Short)
        {
            string format = String.Format("{0} {1} {2} {3}", dayOfWeek.Humanize(), "{0}", shortMonth ? "MMM" : "MMMM", showYear ? "yyyy" : "");
            var result = String.Format(dt.ToString(format), dt.Day.Ordinalize());
            return result.Trim();
        }

        #endregion

    }

    /// <summary>
    /// Enumeration values of the day of week.
    /// </summary>
    public enum CustomDateDayOfWeek
    {
        /// <summary>
        /// Display day of week in the abbreviated name.
        /// </summary>
        [Description("ddd")]
        Short,
        /// <summary>
        /// Display day of week in the full name.
        /// </summary>
        [Description("dddd")]
        Long,
        /// <summary>
        /// Do not display day of week.
        /// </summary>
        [Description("")]
        Hide
    }
}
