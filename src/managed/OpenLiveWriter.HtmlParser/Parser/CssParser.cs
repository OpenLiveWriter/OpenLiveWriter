// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.HtmlParser.Parser
{
    public struct CssParser : IElementSource
    {
        private readonly string data;
        private readonly int end;
        private int pos;

        public CssParser(string data) : this(data, 0, data.Length)
        {
        }

        public CssParser(string data, int offset, int length)
        {
            this.data = data;

            this.pos = offset;
            this.end = offset + length;
        }

        Element IElementSource.Next()
        {
            return Next();
        }

        public StyleElement Next()
        {
            if (pos >= end)
                return null;

            int start = pos;

            switch (data[pos++])
            {
                case '"':
                    {
                        int literalPos = pos;
                        int literalEnd;
                        ParseMethods.MatchDoubleQuotedLiteral(data, ref pos, end, out literalEnd);
                        return new StyleLiteral(data, start, pos - start, literalPos, literalEnd - literalPos, '"');
                    }
                case '\'':
                    {
                        int literalPos = pos;
                        int literalEnd;
                        ParseMethods.MatchSingleQuotedLiteral(data, ref pos, end, out literalEnd);
                        return new StyleLiteral(data, start, pos - start, literalPos, literalEnd - literalPos, '\'');
                    }
                case '/':
                    if (ParseMethods.SubstringMatch(data, pos - 1, end, "//"))
                    {
                        pos++;
                        ParseMethods.MatchSingleLineComment(data, ref pos, end);
                        return new StyleComment(data, start, pos - start);
                    }
                    else if (ParseMethods.SubstringMatch(data, pos - 1, end, "/*"))
                    {
                        pos++;
                        ParseMethods.MatchMultiLineComment(data, ref pos, end);
                        return new StyleComment(data, start, pos - start);
                    }
                    goto default;
                case '<':
                    if (ParseMethods.SubstringMatch(data, pos - 1, end, "<!--"))
                    {
                        pos += 3;
                        ParseMethods.MatchSingleLineComment(data, ref pos, end);
                        return new StyleComment(data, start, pos - start);
                    }
                    goto default;
                case '-':
                    if (ParseMethods.SubstringMatch(data, pos - 1, end, "-->"))
                    {
                        pos += 2;
                        ParseMethods.MatchSingleLineComment(data, ref pos, end);
                        return new StyleComment(data, start, pos - start);
                    }
                    goto default;
                case 'u':
                case 'U':
                    {
                        if (ParseMethods.SubstringMatchIgnoreCase(data, pos - 1, end, "url("))
                        {
                            int literalPos;
                            int literalEnd;
                            char quotChar;

                            pos += 3;
                            MatchUrl(data, ref pos, end, out literalPos, out literalEnd, out quotChar);
                            return new StyleUrl(data, start, pos - start, literalPos, literalEnd - literalPos, quotChar);
                        }
                        goto default;
                    }
                case '@':
                    {
                        const string IMPORT = "@import";
                        if (ParseMethods.SubstringMatchIgnoreCase(data, pos - 1, end, IMPORT))
                        {
                            // We play a little fast and loose with CSS here.  Technically
                            // it's possible for "@import" to be immediately followed by
                            // something besides a whitespace char and still be valid, but
                            // we will not recognize those cases as @import statements,
                            // except when the following char is a quote.
                            int nextCharIndex = pos - 1 + IMPORT.Length;
                            if (data.Length > nextCharIndex)
                            {
                                char nextChar = data[nextCharIndex];
                                if (IsWhitespace(nextChar))
                                    pos += IMPORT.Length;
                                else if (nextChar == '\'' || nextChar == '"')
                                    pos += IMPORT.Length - 1;
                                else
                                    goto default;  // per note above, treat as normal text

                                int literalPos;
                                int literalEnd;
                                char quotChar;

                                MatchImport(data, ref pos, end, out literalPos, out literalEnd, out quotChar);
                                return new StyleImport(data, start, pos - start, literalPos, literalEnd - literalPos, quotChar);
                            }
                        }
                        goto default;
                    }
                default:
                    MatchNormal(data, ref pos, end);
                    return new StyleText(data, start, pos - start);
            }

        }

        private static void MatchNormal(string data, ref int pos, int end)
        {
            for (; pos < end; pos++)
            {
                switch (data[pos])
                {
                    case '\'':
                        return;
                    case '\"':
                        return;
                    case '/':
                        if (end > pos + 1)
                        {
                            switch (data[pos + 1])
                            {
                                case '/':
                                case '*':
                                    return;
                            }
                        }
                        break;
                    case '<':
                        if (ParseMethods.SubstringMatch(data, pos, end, "<!--"))
                            return;
                        break;
                    case '-':
                        if (ParseMethods.SubstringMatch(data, pos, end, "-->"))
                            return;
                        break;
                    case 'u':
                    case 'U':
                        if (ParseMethods.SubstringMatchIgnoreCase(data, pos, end, "url("))
                            return;
                        break;
                    case '@':
                        if (ParseMethods.SubstringMatchIgnoreCase(data, pos, end, "@import"))
                            return;
                        break;
                }
            }
        }

        private static void MatchUrl(string data, ref int pos, int end, out int literalPos, out int literalEnd, out char quotChar)
        {
            literalPos = pos;
            literalEnd = pos;
            quotChar = (char)0;

            // skip (illegal) leading whitespace, e.g. url( http://www.microsoft.com)
            MatchWhitespace(data, ref pos, end);

            if (pos >= end)
            {
                return;
            }

            switch (data[pos++])
            {
                case '\'':
                    quotChar = '\'';
                    literalPos = pos;
                    ParseMethods.MatchSingleQuotedLiteral(data, ref pos, end, out literalEnd);
                    MatchWhitespace(data, ref pos, end);
                    ParseMethods.MatchUntilChar(')', true, data, ref pos, end);
                    break;
                case '"':
                    quotChar = '"';
                    literalPos = pos;
                    ParseMethods.MatchDoubleQuotedLiteral(data, ref pos, end, out literalEnd);
                    MatchWhitespace(data, ref pos, end);
                    ParseMethods.MatchUntilChar(')', true, data, ref pos, end);
                    break;
                default:
                    quotChar = (char)0;
                    literalPos = --pos;
                    ParseMethods.MatchUntilChar(')', true, data, ref pos, end);
                    literalEnd = Math.Max(literalPos, pos - 1);
                    TrimWhitespace(data, ref literalPos, ref literalEnd);
                    break;
            }
        }

        private static void MatchImport(string data, ref int pos, int end, out int literalPos, out int literalEnd, out char quotChar)
        {
            literalPos = pos;
            literalEnd = pos;
            quotChar = (char)0;

            // skip (illegal) leading whitespace, e.g. url( http://www.microsoft.com)
            MatchWhitespace(data, ref pos, end);

            if (pos >= end)
            {
                return;
            }

            if (ParseMethods.SubstringMatchIgnoreCase(data, pos, end, "url("))
            {
                pos += 4;
                MatchUrl(data, ref pos, end, out literalPos, out literalEnd, out quotChar);
                ParseMethods.MatchUntilChar(';', true, data, ref pos, end);
                return;
            }

            switch (data[pos++])
            {
                case '\'':
                    quotChar = '\'';
                    literalPos = pos;
                    ParseMethods.MatchSingleQuotedLiteral(data, ref pos, end, out literalEnd);
                    MatchWhitespace(data, ref pos, end);
                    ParseMethods.MatchUntilChar(';', true, data, ref pos, end);
                    break;
                case '"':
                    quotChar = '"';
                    literalPos = pos;
                    ParseMethods.MatchDoubleQuotedLiteral(data, ref pos, end, out literalEnd);
                    MatchWhitespace(data, ref pos, end);
                    ParseMethods.MatchUntilChar(';', true, data, ref pos, end);
                    break;
                default:
                    quotChar = (char)0;
                    literalPos = --pos;
                    ParseMethods.MatchUntilChar(';', true, data, ref pos, end);
                    literalEnd = Math.Max(literalPos, pos - 1);
                    TrimWhitespace(data, ref literalPos, ref literalEnd);
                    break;
            }
        }

        private static void MatchWhitespace(string data, ref int pos, int end)
        {
            for (; pos < end; pos++)
            {
                if (!IsWhitespace(data[pos]))
                    return;
            }
        }

        private static void TrimWhitespace(string data, ref int pos, ref int end)
        {
            MatchWhitespace(data, ref pos, end);
            for (; pos < end; end--)
            {
                if (!IsWhitespace(data[end - 1]))
                    return;
            }
        }

        private static bool IsWhitespace(char c)
        {
            switch (c)
            {
                case ' ':
                case '\t':
                case '\r':
                case '\n':
                    return true;
                default:
                    return false;
            }
        }
    }
}
