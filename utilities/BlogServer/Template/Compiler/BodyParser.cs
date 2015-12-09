using System;
using System.Text;
using System.IO;

namespace DynamicTemplate.Compiler
{
    class BodyParser
    {
    	public static TemplateOperation Parse(string str, params ArgumentDescription[] args)
    	{
    		return new BodyParser().ParseInternal(str, args);
    	}
    	
        private TemplateOperation ParseInternal(string str, params ArgumentDescription[] args)
        {
            LanguageProvider lp = new CSharpLanguageProvider();

            lp.Start(args);

            IndexToPosition lcf = new IndexToPosition(str);
            int pos = 0;
            int codeStart;
            while (str.Length > pos && 0 <= (codeStart = str.IndexOf("<%", pos)))
            {
                if (pos < codeStart)
                {
                    lp.Literal(str.Substring(pos, codeStart - pos), lcf.Find(pos));
                }

                pos = codeStart + 2;

                bool isExprMode = false;
                if (str.Length > pos && str[pos] == '=')
                {
                    pos++;
                    isExprMode = true;
                }

                int endIndex = str.IndexOf("%>", pos);
                if (endIndex == -1)
                {
                    throw new TemplateCompilationException(
                        "Couldn't find a matching \"%>\"",
                        lcf.Find(pos)
                        );
                }

                Position sourcePos = lcf.Find(pos);
                string code = str.Substring(pos, endIndex - pos);
                if (isExprMode)
                    lp.Expression(code, sourcePos);
                else
                    lp.Code(code, sourcePos);
                
                pos = endIndex + 2;

                // skip trailing CRLF? Only if the current code block is
                // not in expression mode, and if there's no significant text
                // on the same line as the <% or the %>.
                if (!isExprMode
                    && IsLinePrefixOnlyWhitespace(str, codeStart)
                    && IsLineSuffixOnlyWhitespace(str, pos))
                {
                    while (pos < str.Length && str[pos] != '\n')
                        pos++;
                    if (pos < str.Length)
                        pos++;
                }
            }

            if (pos < str.Length)
            {
                lp.Literal(str.Substring(pos), lcf.Find(pos));
            }

            return lp.End();
        }

        private static bool IsLinePrefixOnlyWhitespace(string str, int index)
        {
            for (int i = index - 1; i >= 0; i--)
            {
                if (!char.IsWhiteSpace(str[i]))
                    return false;
                else if (str[i] == '\n')
                    return true;
            }
            return true;
        }

        private static bool IsLineSuffixOnlyWhitespace(string str, int index)
        {
            for (int i = index; i < str.Length; i++)
            {
                if (!char.IsWhiteSpace(str[i]))
                    return false;
                else if (str[i] == '\n')
                    return true;
            }
            return true;
        }
    }

    /// <summary>
    /// Given an index into a string, returns a line/column position.
    /// This implementation is a stateful class that optimizes performance
    /// for multiple queries whose index values are further and further along
    /// in the string.
    /// </summary>
    class IndexToPosition
    {
        private readonly string _str;
        private int _pos;
        private int _line;
        private int _col;

        public IndexToPosition(string str)
        {
            _str = str;
            _pos = 0;
            _line = 1;
            _col = 1;
        }

        public Position Find(int index)
        {
            if (_pos > index)
            {
                // we've gone backwards; start over

                _pos = 0;
                _line = 1;
                _col = 1;
            }

            for (; _pos < index; _pos++)
            {
                switch (_str[_pos])
                {
                    case '\n':
                        _line++;
                        _col = 1;
                        break;
                    default:
                        _col++;
                        break;
                }
            }

            return new Position(_line, _col);
        }

        public static int ReverseFind(string str, int line, int column)
        {
            int pos = -1;
            for (int i = 1; i < line; i++)
            {
                pos = str.IndexOf('\n', pos + 1);
                if (pos == -1)
                    return -1;
            }
            pos++;

            for (int i = 1; i < column; i++)
            {
                pos++;
                if (pos >= str.Length)
                    return -1;
                if ('\n' == str[pos])
                    return -1;
            }

            if (pos >= str.Length)
                return -1;

            return pos;
        }
    }
}
