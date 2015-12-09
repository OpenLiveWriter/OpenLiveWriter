// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.CoreServices
{
    public class StringHelper
    {
        public static Encoding GetEncoding(string charset, Encoding defaultEncoding)
        {
            if (!String.IsNullOrEmpty(charset))
            {
                if (string.Compare(Encoding.UTF8.WebName, charset, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return new UTF8Encoding(false, false);
                }
                else
                {
                    try
                    {
                        return Encoding.GetEncoding(charset);
                    }
                    catch (ArgumentException e)
                    {
                        Debug.WriteLine("BUG: Failed getting encoding for charset " + charset + "with error: " + e);
                    }
                }
            }

            return defaultEncoding;
        }

        public static string Join(object[] anArray)
        {
            if (anArray == null)
                return null;
            return Join(anArray, ", ");
        }

        public static string Join(object[] anArray, string delimiter)
        {
            return Join(anArray, delimiter, false);
        }

        public static string Join(object[] anArray, string delimiter, bool removeEmpty)
        {
            StringBuilder o = new StringBuilder();
            string delim = "";
            foreach (object obj in anArray)
            {
                string str = String.Empty;

                if (obj != null)
                    str = obj.ToString().Trim();

                if (!removeEmpty || (str != String.Empty))
                {
                    o.Append(delim);
                    if (obj != null)
                    {
                        o.Append(str);
                        delim = delimiter;
                    }
                }

            }
            return o.ToString();
        }

        public static string[] Split(string aString, string delimiter)
        {
            if (aString.Length == 0)
                return new string[0];

            ArrayList list = new ArrayList();
            int start = 0;
            int next;
            int delimLength = delimiter.Length;
            while (start < aString.Length && (next = aString.IndexOf(delimiter, start, StringComparison.CurrentCulture)) != -1)
            {
                string chunk = aString.Substring(start, (next - start)).Trim();
                if (chunk != "")
                    list.Add(chunk);
                start = next + delimLength;
            }

            if (start == 0)
            {
                // short circuit when none found
                return new string[] { aString };
            }

            if (start < aString.Length)
            {
                string chunk = aString.Substring(start).Trim();
                if (chunk != "")
                    list.Add(chunk);
            }

            return (string[])list.ToArray(typeof(string));
        }

        // strips spaces and tabs from around hard returns
        private static LazyLoader<Regex> _stripSpaces;
        private static Regex StripSpacesRegex
        {
            get
            {
                return _stripSpaces.Value;
            }

        }
        // turns single hard return into a space
        private static LazyLoader<Regex> _stripSingleLineFeeds;
        private static Regex StripSingleLineFeedsRegex
        {
            get
            {
                return _stripSingleLineFeeds.Value;
            }

        }

        static StringHelper()
        {
            _stripSingleLineFeeds = new LazyLoader<Regex>(() => new Regex(@"(?<=\S)\r?\n(?=\S)"));
            _stripSpaces = new LazyLoader<Regex>(() => new Regex(@"[ \t]*\r?\n[ \t]*"));
        }

        public static string StripSingleLineFeeds(string val)
        {
            return StripSingleLineFeedsRegex.Replace(StripSpacesRegex.Replace(val, "\r\n"), " ");
        }

        public static string Ellipsis(string val, int threshold)
        {
            /*
             * Some edge cases that are somewhat bogus:
             * Ellipsis(".........", 8) => "..."
             * Ellipsis(". . . . .", 8) => "..."
             */

            if (val.Length > threshold)
            {
                return string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.WithEllipses),
                                     val.Substring(0, threshold).TrimEnd(' ', '\r', '\n', '\t', '.'));
            }
            else
                return val;
        }

        /// <summary>
        /// Splits a string at a specified delimiter, with escape logic.  For example:
        ///
        /// SplitWithEscape("one/two/three", '/', '_') => ["one", "two", "three"]
        /// SplitWithEscape("one_/two/three_/four", '/', '_') => ["one/two", "three/four"]
        /// SplitWithEscape("one__two/three", '/', '_') => ["one_two", "three"]
        /// SplitWithEscape("one/two/three_", '/', '_') => ["one", "two", "three_"]
        /// SplitWithEscape("", '/', '_') => []
        /// </summary>
        public static string[] SplitWithEscape(string val, char delimiter, char escapeChar)
        {
            if (delimiter == escapeChar)
                throw new ArgumentException("Delimiter and escape characters cannot be identical.");

            ArrayList results = new ArrayList();
            char[] buffer = new char[val.Length];  // no element can be longer than the original string
            int pos = 0;  // our pointer into the buffer

            bool escaped = false;
            foreach (char thisChar in val.ToCharArray())
            {
                if (escaped)
                {
                    // the last character was the escape char; this char
                    // should not be evaluated, just written

                    buffer[pos++] = thisChar;
                    escaped = false;
                }
                else
                {
                    if (thisChar == escapeChar)
                    {
                        // encountering escape char; do nothing, just make
                        // sure next character is written

                        escaped = true;
                    }
                    else if (thisChar == delimiter)
                    {
                        // encountering delimiter; add current buffer to results

                        results.Add(new string(buffer, 0, pos));
                        pos = 0;
                    }
                    else
                    {
                        // normal character; just print

                        buffer[pos++] = thisChar;
                    }
                }
            }

            // If last char was the escape char, add it to the end of the last string.
            // If this happens, the string was actually malformed, but whatever.
            if (escaped)
                buffer[pos++] = escapeChar;

            // add the last string to the collection
            if (pos != 0)
                results.Add(new string(buffer, 0, pos));

            return (string[])results.ToArray(typeof(string));
        }

        public static string RestrictLength(string val, int maxLen)
        {
            if (val.Length <= maxLen)
                return val;
            else
                return val.Substring(0, maxLen);
        }

        public static string RestrictLengthAtWords(string content, int maxSize)
        {
            if (maxSize == 0)
                return string.Empty;

            //chop off the word at the last space before index maxSize.
            if (content != null && content.Length > maxSize)
            {
                int lastWordIndex = content.LastIndexOfAny(Whitespace, maxSize - 1, maxSize);
                if (lastWordIndex != -1)
                {
                    return content.Substring(0, lastWordIndex).TrimEnd(Whitespace);
                }
            }
            return content;
        }

        public static string GetLastWord(string content)
        {
            if (String.IsNullOrEmpty(content))
                return string.Empty;

            // Any whitespace at the end is not considered part of the last word.
            content = content.TrimEnd(Whitespace);

            if (String.IsNullOrEmpty(content))
                return string.Empty;

            int beforeLastWord = content.LastIndexOfAny(Whitespace);
            if (beforeLastWord > -1 && beforeLastWord < content.Length - 1)
            {
                content = content.Substring(beforeLastWord + 1);
            }

            return content;
        }

        private static readonly char[] Whitespace = new char[] { ' ', '\r', '\n', '\t' };
        private const long KB = 1024L;
        private const long MB = KB * KB;
        private const long GB = KB * KB * KB;
        private const string PATTERN = "{0:#,##0.0000}";

        /// <summary>
        /// Same as FormatByteCount, but negative values will be
        /// interpreted as (not available).
        /// </summary>
        public static string FormatByteCountNoNegatives(long bytes, string naString)
        {
            if (bytes < 0)
                return naString;
            else
                return FormatByteCount(bytes);
        }

        /// <summary>
        /// Format a bytecount in a nice, pretty way.  Similar to the
        /// way Windows Explorer displays sizes.
        ///
        /// 1) Decide what the units will be.
        /// 2) Scale the bytecount to the chosen unit, as a double.
        /// 3) Format the double, to a relatively high degree of precision.
        /// 4) Truncate the number of digits shown to the greater of:
        ///       a) the number of digits in the whole-number part
        ///       b) 3
        /// </summary>
        public static string FormatByteCount(long bytes)
        {
            string num;
            string format;
            if (bytes < KB)
            {
                num = bytes.ToString("N", CultureInfo.CurrentCulture);
                format = Res.Get(StringId.BytesFormat);
            }
            else if (bytes < (MB * 0.97))  // this is what Windows does
            {
                num = FormatNum((double)bytes / KB);
                format = Res.Get(StringId.KilobytesFormat);
            }
            else if (bytes < (GB * 0.97))  // this is what Windows does
            {
                num = FormatNum((double)bytes / MB);
                format = Res.Get(StringId.MegabytesFormat);
            }
            else
            {
                num = FormatNum((double)bytes / GB);
                format = Res.Get(StringId.GigabytesFormat);
            }
            return string.Format(CultureInfo.CurrentCulture, format, num);
        }

        private static string FormatNum(double num)
        {
            NumberFormatInfo nf = (NumberFormatInfo)NumberFormatInfo.CurrentInfo.Clone();
            if (num >= 100)
                nf.NumberDecimalDigits = 0;
            else if (num >= 10)
                nf.NumberDecimalDigits = 1;
            else if (num >= 1)
                nf.NumberDecimalDigits = 2;
            else
                nf.NumberDecimalDigits = 3;

            return num.ToString("N", nf);
        }

        /// <summary>
        /// Returns the longest common prefix that is shared by the
        /// two strings.
        /// </summary>
        public static string CommonPrefix(string one, string two)
        {
            int minLen = Math.Min(one.Length, two.Length);

            for (int i = 0; i < minLen; i++)
                if (one[i] != two[i])
                    return one.Substring(0, i);

            return one.Length < two.Length ? one : two;
        }

        /// <summary>
        /// Should be faster than string.StartsWith(string) because this
        /// ignores culture info
        /// </summary>
        public static bool StartsWith(string stringValue, string prefix)
        {
            if (stringValue.Length < prefix.Length)
                return false;
            for (int i = 0; i < prefix.Length; i++)
                if (stringValue[i] != prefix[i])
                    return false;
            return true;
        }

        /// <summary>
        /// Strips any occurrences of A, An, The, and whitespace from the beginning of the string.
        /// </summary>
        public static string GetSignificantSubstring(string title)
        {
            if (title == null || title == string.Empty)
                return title;

            string pattern = @"^\s*((the|a|an)($|\s+))*\s*";
            Match match = Regex.Match(title, pattern, RegexOptions.IgnoreCase);
            if (!match.Success)
                return title;
            else
                return title.Substring(match.Length);
        }

        private static string GetIndent(string strVal)
        {
            int pos;
            for (pos = 0; pos < strVal.Length; pos++)
            {
                switch (strVal[pos])
                {
                    case ' ':
                    case '\t':
                        break;
                    default:
                        return strVal.Substring(0, pos);
                }
            }
            return strVal;  // all whitespace
        }

        public static string Reverse(string strVal)
        {
            char[] chars = strVal.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        public static string CompressExcessWhitespace(string str)
        {
            // RegexOptions.Multiline only works with \n, not \r\n
            str = LineEndingHelper.Normalize(str, LineEnding.LF);

            // trim lines made up of only spaces and tabs (but keep the line breaks)
            //str = Regex.Replace(str, @"[ \t]+(?=\r\n|$)", "");
            str = Regex.Replace(str, @"[ \t]+$", "", RegexOptions.Multiline);
            //str = Regex.Replace(str, @"(?<=^|\n)[ \t]+(?=\r\n|$)", "");

            str = str.TrimStart('\n');
            str = str.TrimEnd('\n');
            str = Regex.Replace(str, @"\n{3,}", "\n\n");

            str = LineEndingHelper.Normalize(str, LineEnding.CRLF);
            return str;
        }

        public static bool ToBool(string boolValue, bool defaultValue)
        {
            if (boolValue != null)
            {
                switch (boolValue.Trim().ToUpperInvariant())
                {
                    case "YES":
                    case "TRUE":
                    case "1":
                        return true;
                    case "NO":
                    case "FALSE":
                    case "0":
                        return false;
                }
            }
            return defaultValue;
        }

        public static string Replace(string value, int offset, int length, string replacement)
        {
            StringBuilder output = new StringBuilder(value.Length + (replacement.Length - length));
            if (offset > 0)
                output.Append(value.Substring(0, offset));
            output.Append(replacement);
            if (offset + length < value.Length)
                output.Append(value.Substring(offset + length));
            return output.ToString();
        }

        /// <summary>
        /// Gets a hashcode for a string that is stable across multiple versions of .NET
        /// This implementation was taken from .NET 2.0
        /// http://msdn.microsoft.com/en-us/library/system.string.gethashcode.aspx
        /// </summary>
        public static unsafe int GetHashCodeStable(String stringToHash)
        {
            fixed (char* str = stringToHash)
            {
                char* chPtr = str;
                int num = 0x15051505;
                int num2 = num;
                int* numPtr = (int*)chPtr;
                for (int i = stringToHash.Length; i > 0; i -= 4)
                {
                    num = (((num << 5) + num) + (num >> 0x1b)) ^ numPtr[0];
                    if (i <= 2)
                    {
                        break;
                    }
                    num2 = (((num2 << 5) + num2) + (num2 >> 0x1b)) ^ numPtr[1];
                    numPtr += 2;
                }
                return (num + (num2 * 0x5d588b65));
            }
        }
    }
}
