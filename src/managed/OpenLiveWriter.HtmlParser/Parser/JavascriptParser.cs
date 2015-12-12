// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.HtmlParser.Parser
{
    /// <summary>
    /// Parses JavaScript into a stream of (ordinary) text,
    /// quoted string literals, and comments.
    /// </summary>
    public struct JavascriptParser : IElementSource
    {
        private readonly string data;
        private readonly int end;
        private int pos;

        public JavascriptParser(string data) : this(data, 0, data.Length)
        {
        }

        public JavascriptParser(string data, int offset, int length)
        {
            this.data = data;

            this.pos = offset;
            this.end = offset + length;
        }

        Element IElementSource.Next()
        {
            return Next();
        }

        public ScriptElement Next()
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
                        return new ScriptLiteral(data, start, pos - start, literalPos, literalEnd - literalPos, '"');
                    }
                case '\'':
                    {
                        int literalPos = pos;
                        int literalEnd;
                        ParseMethods.MatchSingleQuotedLiteral(data, ref pos, end, out literalEnd);
                        return new ScriptLiteral(data, start, pos - start, literalPos, literalEnd - literalPos, '\'');
                    }
                case '/':
                    if (ParseMethods.SubstringMatch(data, pos - 1, end, "//"))
                    {
                        pos++;
                        ParseMethods.MatchSingleLineComment(data, ref pos, end);
                        return new ScriptComment(data, start, pos - start);
                    }
                    else if (ParseMethods.SubstringMatch(data, pos - 1, end, "/*"))
                    {
                        pos++;
                        ParseMethods.MatchMultiLineComment(data, ref pos, end);
                        return new ScriptComment(data, start, pos - start);
                    }
                    goto default;
                case '<':
                    if (ParseMethods.SubstringMatch(data, pos - 1, end, "<!--"))
                    {
                        pos += 3;
                        ParseMethods.MatchSingleLineComment(data, ref pos, end);
                        return new ScriptComment(data, start, pos - start);
                    }
                    goto default;
                case '-':
                    if (ParseMethods.SubstringMatch(data, pos - 1, end, "-->"))
                    {
                        pos += 2;
                        ParseMethods.MatchSingleLineComment(data, ref pos, end);
                        return new ScriptComment(data, start, pos - start);
                    }
                    goto default;
                default:
                    MatchNormal(data, ref pos, end);
                    return new ScriptText(data, start, pos - start);
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
                }
            }
        }

    }
}
