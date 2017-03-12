// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace OpenLiveWriter.Localization
{
    public class CultureHelper
    {
        public static bool GdiPlusLineCenteringBroken
        {
            get
            {
                return _gdiPlusLineCenteringBroken;
            }
        }
        private static bool _gdiPlusLineCenteringBroken = false;

        /// <summary>
        /// Applies the given culture name to the current thread.
        /// </summary>
        /// <param name="cultureName"></param>
        public static void ApplyUICulture(string cultureName)
        {
            if (cultureName == null)
            {
                throw new ArgumentNullException("cultureName");
            }

            CultureInfo culture = GetBestCulture(cultureName);

            Thread.CurrentThread.CurrentUICulture = culture;
            _gdiPlusLineCenteringBroken = CultureInfo.CurrentUICulture.ThreeLetterWindowsLanguageName == "CHT";

            FixupDateTimeFormat();
        }

        private static void FixupDateTimeFormat()
        {
            if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.ToUpperInvariant() == "AR")
            {
                // Ensure that no spaces, slashes, or dashes will make the date not formmatted correctly by forcing all chars RTL
                CultureInfo ci = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
                ci.DateTimeFormat.ShortDatePattern = Regex.Replace(ci.DateTimeFormat.ShortDatePattern, "[Mdy]+", "\u200F$0");
                Thread.CurrentThread.CurrentCulture = ci;

            }
        }

        public static CultureInfo GetBestCulture(string cultureName)
        {
            try
            {
                // Dotnet won't load 'ml'
                switch (cultureName.ToUpperInvariant())
                {
                    case ("ML"):
                        return CultureInfo.CreateSpecificCulture("ml-in");
                    case ("PT"):
                        return CultureInfo.CreateSpecificCulture("pt-pt");
                    case ("SR-CYRL"):
                        return CultureInfo.CreateSpecificCulture("sr-cyrl-CS");
                    case ("SR-LATN-CS"):
                        try
                        {
                            return CultureInfo.CreateSpecificCulture(cultureName);
                        }
                        catch (ArgumentException)
                        {
                            return CultureInfo.CreateSpecificCulture("sr-sp-latn");
                        }
                    default:
                        return CultureInfo.CreateSpecificCulture(cultureName);
                }
            }
            catch (ArgumentException)
            {
                // Specific culture didn't succeed, see if we can make
                // a culture-neutral language identifier
                int dashAt = cultureName.IndexOf('-');
                if (dashAt >= 0)
                {
                    try
                    {
                        return CultureInfo.CreateSpecificCulture(cultureName.Substring(0, dashAt));
                    }
                    catch (ArgumentException)
                    {
                    }
                }

                throw;
            }
        }

        public static void FixupTextboxForNumber(TextBox textBox)
        {
            if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.ToUpperInvariant() == "HE")
            {
                textBox.RightToLeft = RightToLeft.No;
                textBox.TextAlign = HorizontalAlignment.Right;
            }
        }

        public static string GetDateTimeCombinedPattern(string date, string time)
        {
            // Simple way to control what comes first, date or time when displaying to the user
            if (Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToUpperInvariant() == "AR"
                && Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.ToUpperInvariant() == "AR")
            {
                return "\u200F" + time + " " + date;
            }
            else
            {
                return date + " " + time;
            }
        }

        public static string GetShortDateTimePatternForDateTimePicker()
        {
            // DateTimPicker controls have a problem with RTL and custom formats.  To get around this we hardcore the time in the reverse order.
            if (Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToUpperInvariant() == "AR"
                && Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.ToUpperInvariant() == "AR")
            {
                return "mm:hh";
            }
            else
            {
                return CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
            }

        }

        [Obsolete("NOT FULLY TESTED")]
        public static bool IsImeActive(IntPtr windowHandle)
        {
            bool isActive = false;

            try
            {
                IntPtr handle = Imm32.ImmGetContext(windowHandle);

                if (handle == IntPtr.Zero)
                    return false;

                try
                {
                    isActive = Imm32.ImmGetOpenStatus(handle);
                }
                finally
                {
                    Imm32.ImmReleaseContext(windowHandle, handle);
                }

                return isActive;
            }
            catch (Exception ex)
            {
                Trace.Fail("Failed to check if IME is active: " + ex);
                return isActive;
            }

        }

        [Obsolete("NOT FULLY TESTED")]
        public static class Imm32
        {
            [DllImport("imm32.dll")]
            public static extern IntPtr ImmGetContext(IntPtr hWnd);

            [DllImport("imm32.dll")]
            public static extern bool ImmGetOpenStatus(IntPtr hIMC);

            [DllImport("imm32.dll")]
            public static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);
        }

        public static bool IsRtlCodepage(uint codepage)
        {
            switch (codepage)
            {
                case 708:
                case 720:
                case 864:
                case 1256:
                case 10004:
                case 20420:
                case 28596:

                case 862:
                case 1255:
                case 10005:
                case 20424:
                case 28598:
                case 38598:

                    return true;
            }

            return false;
        }

        public static bool IsRtlLcid(int lcid)
        {
            return new CultureInfo(lcid).TextInfo.IsRightToLeft;
        }

        public static bool IsRtlCulture(string bcp47Code)
        {
            try
            {
                return new CultureInfo(bcp47Code).TextInfo.IsRightToLeft;
            }
            catch
            {
                return false;
            }
        }
    }
}
