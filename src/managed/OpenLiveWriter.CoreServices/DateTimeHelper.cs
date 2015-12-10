// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using OpenLiveWriter.Interop.Windows;
using System.Windows.Forms;

namespace OpenLiveWriter.CoreServices
{
    public enum TimePeriod { MINUTES = 0, HOURS = 1, DAYS = 2, WEEKS = 3, SECONDS = 4 };

    /// <summary>
    /// Summary description for DateTimeHelper.
    /// </summary>
    public class DateTimeHelper
    {
        private DateTimeHelper()
        {
        }

        /// <summary>
        /// Returns the current time in UTC.
        /// </summary>
        public static DateTime UtcNow
        {
            get
            {
                return LocalToUtc(DateTime.Now);
            }
        }

        private const long BASETICKS = 504911232000000000; //new DateTime(1601, 1, 1).Ticks;

        /// <summary>
        /// Converts local DateTime to local FILETIME,
        /// or UTC DateTime to UTC FILETIME.
        /// </summary>
        public static System.Runtime.InteropServices.ComTypes.FILETIME ToFileTime(DateTime dateTime)
        {
            long fileTimeVal = dateTime.Ticks - BASETICKS;
            System.Runtime.InteropServices.ComTypes.FILETIME result = new System.Runtime.InteropServices.ComTypes.FILETIME();
            result.dwHighDateTime = (int)(fileTimeVal >> 32);
            result.dwLowDateTime = (int)(fileTimeVal & 0xFFFFFFFF);
            return result;
        }

        /// <summary>
        /// Converts local FILETIME to local DateTime,
        /// or UTC FILETIME to UTC DateTime.
        /// </summary>
        public static DateTime ToDateTime(System.Runtime.InteropServices.ComTypes.FILETIME fileTime)
        {
            long ticks = BASETICKS + ((fileTime.dwLowDateTime & 0xFFFFFFFF) | ((long)fileTime.dwHighDateTime << 32));
            return new DateTime(Math.Min(Math.Max(DateTime.MinValue.Ticks, ticks), DateTime.MaxValue.Ticks));
        }

        /// <summary>
        /// Convert from a .NET to a W3C date-time (http://www.w3.org/TR/NOTE-datetime)
        /// </summary>
        /// <param name="dateTime">date time </param>
        /// <returns>w3c date-time</returns>
        public static string ToW3CDateTime(DateTime dateTime)
        {
            return dateTime.ToString(W3C_DATE_FORMAT, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Convert from a W3C (http://www.w3.org/TR/NOTE-datetime) to a .NET DateTime (based in local timezone)
        /// </summary>
        /// <param name="w3cDateTime">w3c date-time</param>
        /// <returns></returns>
        public static DateTime ToDateTime(string w3cDateTime)
        {
            IFormatProvider culture = new CultureInfo("en-US", true);
            DateTime dateTime;
            try
            {
                dateTime = DateTime.ParseExact(w3cDateTime, W3C_DATE_FORMATS, culture, DateTimeStyles.AllowWhiteSpaces);
            }
            catch (Exception)
            {
                //Now try the W3C UTC date formats
                //Since .NET doesn't realize the the 'Z' is an indicator of the GMT timezone,
                //ParseExact will return the date exactly as parsed (no shift for GMT) so we have
                //to call ToLocalTime() on it to get it into the local time zone.
                dateTime = DateTime.ParseExact(w3cDateTime, W3C_DATE_FORMATS_UTC, culture, DateTimeStyles.AllowWhiteSpaces);
                dateTime = LocalToUtc(dateTime);
            }
            return dateTime;
        }
        private const string W3C_DATE_FORMAT = "yyyy'-'MM'-'dd'T'HH':'mm':'sszzz";
        private static readonly string[] W3C_DATE_FORMATS = new string[] { "yyyy'-'MM'-'dd'T'HH':'mm':'sszzz" };
        private static readonly string[] W3C_DATE_FORMATS_UTC = new string[] { "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", "yyyy'-'MM'-'dd'T'HH':'mm'Z'" };

        public static DateTime LocalToUtc(DateTime localTime)
        {
            System.Runtime.InteropServices.ComTypes.FILETIME localFileTime = ToFileTime(localTime);
            System.Runtime.InteropServices.ComTypes.FILETIME utcFileTime;
            Kernel32.LocalFileTimeToFileTime(ref localFileTime, out utcFileTime);
            return ToDateTime(utcFileTime);
        }

        public static DateTime UtcToLocal(DateTime utcTime)
        {
            System.Runtime.InteropServices.ComTypes.FILETIME utcFileTime = ToFileTime(utcTime);
            System.Runtime.InteropServices.ComTypes.FILETIME localFileTime;
            Kernel32.FileTimeToLocalFileTime(ref utcFileTime, out localFileTime);
            return ToDateTime(localFileTime);
        }

        public static TimeSpan GetUtcOffset(DateTime forLocalTime)
        {
            DateTime utc = LocalToUtc(forLocalTime);
            return forLocalTime - utc;
        }

        /// <summary>
        /// Gets the start and end dates for "this week" date-range, relative to a specified date.
        /// </summary>
        /// <param name="dateTime">The DateTime that the calculation is relative to.</param>
        /// <param name="start">Start date.</param>
        /// <param name="end">End date.</param>
        public static void GetThisWeekDateRange(DateTime dateTime, out DateTime start, out DateTime end)
        {
            dateTime = dateTime.Date;
            start = dateTime.AddDays(-(int)dateTime.DayOfWeek);
            end = dateTime.AddDays((int)DayOfWeek.Saturday - (int)dateTime.DayOfWeek);
        }

        /// <summary>
        /// Gets the start and end dates for the "last week" date-range, relative to a specified date.
        /// </summary>
        /// <param name="dateTime">The DateTime that the calculation is relative to.</param>
        /// <param name="start">Start date.</param>
        /// <param name="end">End date.</param>
        public static void GetLastWeekDateRange(DateTime dateTime, out DateTime start, out DateTime end)
        {
            dateTime = dateTime.Date.AddDays(-7);
            start = dateTime.AddDays(-(int)dateTime.DayOfWeek);
            end = dateTime.AddDays((int)DayOfWeek.Saturday - (int)dateTime.DayOfWeek);
        }

        /// <summary>
        /// Gets the start and end dates for the "two weeks ago" date-range, relative to a specified date.
        /// </summary>
        /// <param name="dateTime">The DateTime that the calculation is relative to.</param>
        /// <param name="start">Start date.</param>
        /// <param name="end">End date.</param>
        public static void GetTwoWeeksAgoDateRange(DateTime dateTime, out DateTime start, out DateTime end)
        {
            dateTime = dateTime.Date.AddDays(-14);
            start = dateTime.AddDays(-(int)dateTime.DayOfWeek);
            end = dateTime.AddDays((int)DayOfWeek.Saturday - (int)dateTime.DayOfWeek);
        }

        /// <summary>
        /// Gets the start and end dates for the "three weeks ago" date-range, relative to a specified date.
        /// </summary>
        /// <param name="dateTime">The DateTime that the calculation is relative to.</param>
        /// <param name="start">Start date.</param>
        /// <param name="end">End date.</param>
        public static void GetThreeWeeksAgoDateRange(DateTime dateTime, out DateTime start, out DateTime end)
        {
            dateTime = dateTime.Date.AddDays(-21);
            start = dateTime.AddDays(-(int)dateTime.DayOfWeek);
            end = dateTime.AddDays((int)DayOfWeek.Saturday - (int)dateTime.DayOfWeek);
        }

        /// <summary>
        /// Gets the start and end dates for "this month" date-range, relative to a specified date.
        /// </summary>
        /// <param name="dateTime">The DateTime that the calculation is relative to.</param>
        /// <param name="start">Start date.</param>
        /// <param name="end">End date.</param>
        public static void GetThisMonthDateRange(DateTime dateTime, out DateTime start, out DateTime end)
        {
            dateTime = dateTime.Date;
            start = dateTime.AddDays(-(dateTime.Day - 1));
            end = dateTime.AddDays(DateTime.DaysInMonth(dateTime.Year, dateTime.Month) - dateTime.Day);
        }

        /// <summary>
        /// Gets the start and end dates for "last month" date-range, relative to a specified date.
        /// </summary>
        /// <param name="dateTime">The DateTime that the calculation is relative to.</param>
        /// <param name="start">Start date.</param>
        /// <param name="end">End date.</param>
        public static void GetLastMonthDateRange(DateTime dateTime, out DateTime start, out DateTime end)
        {
            dateTime = dateTime.Date;
            end = dateTime.AddDays(-dateTime.Day);
            start = end.AddDays(-(end.Day - 1));
        }

        /// <summary>
        /// Gets the start and end dates for "last 3 days" date-range, relative to a specified date.
        /// </summary>
        /// <param name="dateTime">The DateTime that the calculation is relative to.</param>
        /// <param name="start">Start date.</param>
        /// <param name="end">End date.</param>
        public static void GetLast3DaysDateRange(DateTime dateTime, out DateTime start, out DateTime end)
        {
            end = dateTime.Date;
            start = end.AddDays(-2);
        }

        /// <summary>
        /// Gets the start and end dates for "last 7 days" date-range, relative to a specified date.
        /// </summary>
        /// <param name="dateTime">The DateTime that the calculation is relative to.</param>
        /// <param name="start">Start date.</param>
        /// <param name="end">End date.</param>
        public static void GetLast7DaysDateRange(DateTime dateTime, out DateTime start, out DateTime end)
        {
            end = dateTime.Date;
            start = end.AddDays(-6);
        }

        /// <summary>
        /// Gets the start and end dates for "last 30 days" date-range, relative to a specified date.
        /// </summary>
        /// <param name="dateTime">The DateTime that the calculation is relative to.</param>
        /// <param name="start">Start date.</param>
        /// <param name="end">End date.</param>
        public static void GetLast30DaysDateRange(DateTime dateTime, out DateTime start, out DateTime end)
        {
            end = dateTime.Date;
            start = end.AddDays(-29);
        }

        /// <summary>
        /// Strips Whidbey dates of their high byte information (which distinguishes
        /// UTC times from local times, or "Unspecified").
        ///
        /// If the high byte information is allowed to remain in the object, we get
        /// big problems trying to deserialize these values from .NET 1.1 processes.
        /// </summary>
        public static DateTime MakeSafeForSerialization(DateTime val)
        {
            return new DateTime(val.Ticks);
        }
    }

    public class DateTimerPickerExtended : DateTimePicker
    {
        public DateTimerPickerExtended() : base()
        {

        }

        private bool lastTimeChangedWasChecked = false;
        protected override void OnValueChanged(EventArgs eventargs)
        {
            lastTimeChangedWasChecked = Checked;
            base.OnValueChanged(eventargs);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (lastTimeChangedWasChecked != Checked)
                OnValueChanged(EventArgs.Empty);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (lastTimeChangedWasChecked != Checked)
                OnValueChanged(EventArgs.Empty);
        }
    }
}
