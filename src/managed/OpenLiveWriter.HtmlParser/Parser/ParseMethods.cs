// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.HtmlParser.Parser
{
    public class ParseMethods
    {
        private static readonly char[] EOL = { (char)13, (char)10 };

        public static void MatchDoubleQuotedLiteral(string data, ref int pos, int end, out int literalEnd)
        {
            for (; pos < end; pos++)
            {
                switch (data[pos])
                {
                    case '\\':
                        if (pos < end - 1)
                        {
                            pos++;
                            if (data[pos] == '\r' && pos < end - 2 && data[pos] == '\n')
                                pos++;
                        }
                        break;

                    case '"':
                        literalEnd = pos;
                        pos++;
                        return;

                    case '\r':
                    case '\n':
                        literalEnd = pos;
                        return;
                }
            }
            literalEnd = pos;
        }

        public static void MatchSingleQuotedLiteral(string data, ref int pos, int end, out int literalEnd)
        {
            for (; pos < end; pos++)
            {
                switch (data[pos])
                {
                    case '\\':
                        if (pos < end - 1)
                        {
                            pos++;
                            if (data[pos] == '\r' && pos < end - 2 && data[pos] == '\n')
                                pos++;
                        }
                        break;

                    case '\'':
                        literalEnd = pos;
                        pos++;
                        return;

                    case '\r':
                    case '\n':
                        literalEnd = pos;
                        return;
                }
            }
            literalEnd = pos;
        }

        public static void MatchSingleLineComment(string data, ref int pos, int end)
        {
            int eol = data.IndexOfAny(EOL, pos, end - pos);
            if (eol == -1)
                pos = end;
            else
            {
                pos = eol;
            }
        }

        public static void MatchMultiLineComment(string data, ref int pos, int end)
        {
            int commentEnd = data.IndexOf("*/", pos, end - pos, System.StringComparison.Ordinal);
            if (commentEnd == -1)
                pos = end;
            else
            {
                pos = commentEnd + 2;
            }
        }

        public static void MatchUntilChar(char c, bool inclusive, string data, ref int pos, int end)
        {
            for (; pos < end; pos++)
            {
                if (data[pos] == c)
                {
                    if (inclusive)
                        pos++;
                    return;
                }
            }
        }

        public static void MatchUntilAnyChar(bool inclusive, string data, ref int pos, int end, params char[] chars)
        {
            for (; pos < end; pos++)
            {
                for (int i = 0; i < chars.Length; i++)
                {
                    if (data[pos] == chars[i])
                    {
                        if (inclusive)
                            pos++;
                        return;
                    }
                }
            }
        }

        public static bool SubstringMatch(string data, int index, int end, string substring)
        {
            int p = 0;
            if (end < index + substring.Length)
                return false;

            for (int i = index; i < end && p < substring.Length; i++)
            {
                if (substring[p++] != data[i])
                    return false;
            }
            return true;
        }

        public static bool SubstringMatchIgnoreCase(string data, int index, int end, string substring)
        {
            int p = 0;
            if (end < index + substring.Length)
                return false;

            for (int i = index; i < end && p < substring.Length; i++)
            {
                if (ToAsciiLower(substring[p++]) != ToAsciiLower(data[i]))
                    return false;
            }
            return true;
        }

        private static char ToAsciiLower(char c)
        {
            const int OFFSET = 'a' - 'A';
            if (c >= 'A' && c <= 'Z')
                return (char)(c + OFFSET);
            else
                return c;
        }

    }
}
