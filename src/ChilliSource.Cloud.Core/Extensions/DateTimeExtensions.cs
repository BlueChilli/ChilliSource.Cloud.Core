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
        /// <summary>
        /// Returns a value of string representing interval between now and specified date in days.
        /// </summary>
        /// <param name="dt">The specified date and time. (Assumed to be UTC)</param>
        /// <param name="dateOnly">True to calculate interval using date only, ignore hour, minute and second, otherwise false.</param>
        /// <param name="text" >Text appended to the string returned.</param>
        /// <returns>A value of string representing interval between now and specified date in days.</returns>
        public static string DaysToGo(this DateTime dt, bool dateOnly = false, string text = "to go")
        {
            var now = DateTime.UtcNow;
            if (dateOnly)
            {
                now = now.Date;
                dt = dt.Date;
            }

            TimeSpan timeSpan = TimeSpan.Zero;
            if (dt >= now)
                timeSpan = dt - now;

            return String.Concat("day".ToPlural(timeSpan.Days, outputCount: true), " ", text);
        }

        /// <summary>
        /// Returns a value of string representing time in past.
        /// </summary>
        /// <param name="dt">The specified date and time. (Assumed to be UTC)</param>
        /// <param name="text">Text appended to the string returned.</param>
        /// <returns>A value of string representing time in past.</returns>
        /// <example>
        /// DateTime.UtcNow.AddMinutes(-1).ago() returns "60 seconds ago".
        /// DateTime.UtcNow.AddHours(-1).ago() returns "60 minutes ago".
        /// DateTime.UtcNow.AddDays(-1).ago() returns "24 hours ago".
        /// DateTime.UtcNow.AddMonths(-1).ago() returns "30 days ago".
        /// DateTime.UtcNow.AddYears(-1).ago() returns "12 months ago".
        /// DateTime.UtcNow.AddYears(-2).ago() returns "2 years ago".
        /// </example>
        public static string Ago(this DateTime dt, string text = "ago")
        {
            var timeSpan = DateTime.UtcNow - dt;

            if (timeSpan <= TimeSpan.FromSeconds(60)) return "second".ToPlural(timeSpan.Seconds, outputCount: true) + " " + text;

            if (timeSpan <= TimeSpan.FromMinutes(60)) return "minute".ToPlural(timeSpan.Minutes, outputCount: true) + " " + text;

            if (timeSpan <= TimeSpan.FromHours(24)) return "hour".ToPlural(timeSpan.Hours, outputCount: true) + " " + text;

            if (timeSpan <= TimeSpan.FromDays(30)) return timeSpan.Days > 1 ? String.Format("{0} days {1}", timeSpan.Days, text) : "yesterday";

            if (timeSpan <= TimeSpan.FromDays(365)) return "month".ToPlural(timeSpan.Days / 30, outputCount: true) + " " + text;

            return "year".ToPlural(timeSpan.Days / 365, outputCount: true) + " " + text;
        }


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
        public static DateTime ToUTCTimezone(this DateTime value, string fromTimezone)
        {
            return value.ToTimezone("UTC", fromTimezone);
        }
    }
}
