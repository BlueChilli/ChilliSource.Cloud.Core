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
        /// Returns date which set the day to the first of the month for the specified date.
        /// </summary>
        /// <param name="dt">The specified date and time.</param>
        /// <returns>Date which set the day to the first of the month for the specified date.</returns>
        public static DateTime ThisMonth(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, 1);
        }

        /// <summary>
        /// Returns date which set to first day of next month for the specified date.
        /// </summary>
        /// <param name="dt">The specified date and time.</param>
        /// <returns>Date which set to first day of next month for the specified date.</returns>
        public static DateTime NextMonth(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, 1).AddMonths(1);
        }

        /// <summary>
        /// Returns the number of days in month for the specified date.
        /// </summary>
        /// <param name="dt">The specified date.</param>
        /// <returns>The number of days in month for the specified date.</returns>
        public static int DaysInMonth(this DateTime dt)
        {
            return DateTime.DaysInMonth(dt.Year, dt.Month);
        }

        /// <summary>
        /// Returns the last day of the month for the specified date.
        /// </summary>
        /// <param name="dt">The specified date.</param>
        /// <returns>The last day of the month for the specified date.</returns>
        public static DateTime EndOfMonth(this DateTime dt)
        {
            return dt.ThisMonth().AddMonths(1).AddMilliseconds(-1);
        }

        /// <summary>
        /// Checks whether the specified date is today's date.
        /// </summary>
        /// <param name="compare">The specified date.</param>
        /// <returns>True when the specified date is today's date, otherwise false.</returns>
        public static bool IsToday(this DateTime compare)
        {
            if (DateTime.UtcNow.DayOfYear == compare.DayOfYear && DateTime.UtcNow.Year == compare.Year)
                return true;
            return false;
        }

        /// <summary>
        /// Checks whether the specified date is yesterday's date.
        /// </summary>
        /// <param name="compare">The specified date.</param>
        /// <returns>True when the specified date is yesterday's date, otherwise false.</returns>
        public static bool IsYesterday(this DateTime compare)
        {
            DateTime yesterday = compare.AddDays(1);

            return yesterday.IsToday();
        }

        /// <summary>
        /// Checks whether the month in the specified date is current month.
        /// </summary>
        /// <param name="compare">The specified date.</param>
        /// <returns>True when the month in the specified date is current month, otherwise false.</returns>
        public static bool IsCurrentMonth(this DateTime compare)
        {
            return (compare.Month == DateTime.UtcNow.Month && compare.Year == DateTime.UtcNow.Year);
        }

        /// <summary>
        /// Converts the value of the specified date to its equivalent string representation using the ISO format with date only (yyyy-MM-dd).
        /// </summary>
        /// <param name="dt">The specified date.</param>
        /// <returns>A value of string in ISO format with date only.</returns>
        public static string ToIsoDate(this DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// Converts the value of the specified date to its equivalent string representation using the ISO format with date and time (yyyy-MM-dd HH:mm).
        /// </summary>
        /// <param name="dt">The specified date.</param>
        /// <param name="outputSeconds">Output seconds after minutes</param>
        /// <param name="isISO8601">Output T as separator between date and time</param>
        /// <returns>A value of string in ISO format with date and time.</returns>
        public static string ToIsoDateTime(this DateTime dt, bool outputSeconds = false, bool isISO8601 = false)
        {
            if (outputSeconds)
            {
                return dt.ToString("yyyy-MM-dd{0}HH:mm:ss".FormatWith(isISO8601 ? "T" : " "));
            }
            else
            {
                return dt.ToString("yyyy-MM-dd{0}HH:mm".FormatWith(isISO8601 ? "T" : " "));
            }
        }

        /// <summary>
        /// Returns the minimum date of SQL server.
        /// </summary>
        /// <param name="notUsed">The specified date (not used).</param>
        /// <returns>The minimum date of SQL server (1 January 1753).</returns>
        public static DateTime MinDateForSqlServer(this DateTime notUsed) { return MinDateForSqlServer(); }

        /// <summary>
        /// Returns the minimum date of SQL server.
        /// </summary>
        /// <returns>The minimum date of SQL server (1 January 1753).</returns>
        public static DateTime MinDateForSqlServer() { return new DateTime(1753, 1, 1); }

        /// <summary>
        /// Converts the value of the specified date to its equivalent JavaScript Date object string representation.
        /// </summary>
        /// <param name="dt">The specified date.</param>
        /// <returns>A string that represents the value of this instance in equivalent JavaScript Date object representation.</returns>
        public static string ToJavaScript(this DateTime dt)
        {
            return "new Date({0},{1},{2},{3},{4},{5}".FormatWith(dt.Year, dt.Month, dt.Day, dt.Minute, dt.Second, dt.Millisecond);
        }

        /// <summary>
        /// Convert a datatime to a unix timestamp
        /// </summary>
        public static TimeSpan ToUnixTime(this DateTime date)
        {
            return new TimeSpan(date.Ticks - 621355968000000000);
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

        /// <summary>
        /// Return the closest future date in which the day of week matches the input.
        /// If the current datetime day of week matches the input, the same date is returned.
        /// </summary>
        /// <param name="dt">The specified date.</param>
        /// <param name="nextDay">The day of week to set</param>
        /// <returns>A System.DateTime object.</returns>
        public static DateTime NextDayOfWeek(this DateTime dt, DayOfWeek nextDay = DayOfWeek.Sunday)
        {
            int diff = dt.DayOfWeek - nextDay;
            if (diff < 0)
            {
                diff += 7;
            }
            return dt.AddDays(-1 * diff).Date;
        }
    }
}
