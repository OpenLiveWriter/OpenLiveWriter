// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace OpenLiveWriter.CoreServices
{
    public enum LineEnding
    {
        CR,
        LF,
        CRLF
    }

    public class LineEndingHelper
    {
        /// <summary>
        /// Normalizes occurrences of "\r", "\n", and "\r\n" to the desired style.
        /// </summary>
        public static string Normalize(string input, LineEnding style)
        {
            if (input == null)
                return null;

            string ending;
            switch (style)
            {
                case LineEnding.CR:
                    ending = "\r"; break;
                case LineEnding.CRLF:
                    ending = "\r\n"; break;
                case LineEnding.LF:
                    ending = "\n"; break;
                default:
                    throw new ArgumentException("Unexpected style type", "style");
            }

            StringBuilder result = null;
            int len = input.Length;
            for (int i = 0; i < len; i++)
            {
                char c = input[i];
                switch (c)
                {
                    case '\r':
                    case '\n':
                        bool isCrLf = c == '\r' && input.Length > i + 1 && input[i + 1] == '\n';

                        if (result == null && !IsMatch(style, c, isCrLf))
                        {
                            // line ending is different; will need to return
                            // a result that is different than the input
                            result = new StringBuilder(style == LineEnding.CRLF ? (int)(input.Length * 1.1) : input.Length);
                            result.Append(input, 0, i);
                        }
                        if (result != null)
                        {
                            result.Append(ending);
                        }

                        if (isCrLf)
                            i++;
                        break;

                    default:
                        if (result != null)
                            result.Append(c);
                        break;
                }
            }

            if (result == null)
                return input;
            else
                return result.ToString();
        }

        public static bool EqualIgnoreNewlines(string s1, string s2)
        {
            s1 = Normalize(s1, LineEnding.LF);
            s2 = Normalize(s2, LineEnding.LF);

            return s1 == s2;
        }

        private static bool IsMatch(LineEnding style, char c, bool isCrLf)
        {
            switch (style)
            {
                case LineEnding.CR:
                    return !isCrLf && c == '\r';
                case LineEnding.LF:
                    return c == '\n';
                case LineEnding.CRLF:
                    return isCrLf;
                default:
                    throw new ArgumentException("Unexpected style type", "style");
            }
        }

        [Conditional("FALSE")]
        public static void Test()
        {
            TestHelper("", "", LineEnding.CR);
            TestHelper("", "", LineEnding.LF);
            TestHelper("", "", LineEnding.CRLF);
            TestHelper("\r", "\r", LineEnding.CR);
            TestHelper("\r", "\n", LineEnding.LF);
            TestHelper("\r", "\r\n", LineEnding.CRLF);
            TestHelper("\n", "\r", LineEnding.CR);
            TestHelper("\n", "\n", LineEnding.LF);
            TestHelper("\n", "\r\n", LineEnding.CRLF);
            TestHelper("\r\n", "\r", LineEnding.CR);
            TestHelper("\r\n", "\n", LineEnding.LF);
            TestHelper("\r\n", "\r\n", LineEnding.CRLF);
            TestHelper("abc\r\n\n\r\r\n", "abc\r\n\r\n\r\n\r\n", LineEnding.CRLF);
            TestHelper("abc\r\n\n\rd\r\n", "abc\r\r\rd\r", LineEnding.CR);
            TestHelper("abc\r\n\n\rd\r\n", "abc\n\n\nd\n", LineEnding.LF);
            TestHelper("a\ra\ra\r", "a\na\na\n", LineEnding.LF);
            TestHelper("a\ra\ra\r", "a\r\na\r\na\r\n", LineEnding.CRLF);
            TestHelper("the quick brown fox", "the quick brown fox", LineEnding.CR);
            TestHelper("\rthe quick brown fox", "\nthe quick brown fox", LineEnding.LF);
            TestHelper("\nthe quick brown fox", "\nthe quick brown fox", LineEnding.LF);
        }

        public static void TestHelper(string input, string expectedOutput, LineEnding style)
        {
            string output = Normalize(input, style);
            if (output != expectedOutput)
                Trace.Fail(string.Format(CultureInfo.InvariantCulture, "\"{0}\" != \"{1}\"", Escape(output), Escape(expectedOutput)));
        }

        private static string Escape(string s)
        {
            return s.Replace("\r", "\\r").Replace("\n", "\\n");
        }
    }
}
