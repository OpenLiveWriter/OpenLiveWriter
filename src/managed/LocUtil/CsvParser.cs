// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace LocUtil
{
    public class CsvParser : IEnumerable, IDisposable
    {
        private readonly TextReader _input;
        private bool _atLineEnd = false;

        public CsvParser(TextReader input, bool skipFirstLine)
        {
            _input = input;
            if (skipFirstLine)
            {
                while (NextWord() != null) { }
                NextLine();
            }
        }

        private string NextWord()
        {
            if (_atLineEnd)
                return null;

            EatSpaces();
            switch (_input.Peek())
            {
                case -1:
                    return null;
                case '"':
                    return MatchQuoted();
                case '\r':
                case '\n':
                    EatLineEndings();
                    return null;
                default:
                    return MatchUnquoted();
            }
        }

        private string MatchQuoted()
        {
            Debug.Assert(_input.Read() == '"');
            StringBuilder sb = new StringBuilder();
            for (int c = _input.Read(); c != -1; c = _input.Read())
            {
                if (c == '"')
                {
                    if (_input.Peek() == '"')
                    {
                        _input.Read();
                        sb.Append('"');
                    }
                    else
                    {
                        EatSpaces();
                        switch (_input.Peek())
                        {
                            case ',':
                                _input.Read();
                                return sb.ToString();
                            case '\r':
                            case '\n':
                            case -1:
                                _atLineEnd = true;
                                EatLineEndings();
                                return sb.ToString();
                            default:
                                throw new ArgumentException("Malformed CSV: unexpected character after string \"" + sb.ToString() + "\"");
                        }
                    }

                }
                else
                    sb.Append((char)c);
            }
            Debug.Fail("Unterminated quoted string: " + sb.ToString());
            return sb.ToString();
        }

        private string MatchUnquoted()
        {
            StringBuilder sb = new StringBuilder();
            for (int c = _input.Read(); c != -1; c = _input.Read())
            {
                switch (c)
                {
                    case ',':
                        return sb.ToString();
                    case '\r':
                    case '\n':
                        _atLineEnd = true;
                        EatLineEndings();
                        return sb.ToString();
                    default:
                        sb.Append((char)c);
                        break;
                }
            }
            // EOF
            _atLineEnd = true;
            return sb.ToString();
        }

        private void EatSpaces()
        {
            while (_input.Peek() == ' ')
                _input.Read();
        }

        private void EatLineEndings()
        {
            while (true)
            {
                switch (_input.Peek())
                {
                    case '\r':
                    case '\n':
                        _input.Read();
                        break;
                    default:
                        return;
                }
            }
        }

        private bool NextLine()
        {
            if (_input.Peek() != -1)
            {
                _atLineEnd = false;
                return true;
            }
            else
                return false;
        }

        public void Dispose()
        {
            _input.Close();
        }

        private class LineEnumerator : IEnumerator
        {
            CsvParser parent;
            string[] line;

            public LineEnumerator(CsvParser parent)
            {
                this.parent = parent;
            }

            public bool MoveNext()
            {
            start:
                if (line != null)
                {
                    if (!parent.NextLine())
                        return false;
                }

                ArrayList words = new ArrayList();
                string word;
                while (null != (word = parent.NextWord()))
                    words.Add(word);
                line = (string[])words.ToArray(typeof(string));
                if (line.Length == 0)
                    goto start;
                return true;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            public object Current
            {
                get { return line; }
            }
        }

        public IEnumerator GetEnumerator()
        {
            return new LineEnumerator(this);
        }
    }
}
