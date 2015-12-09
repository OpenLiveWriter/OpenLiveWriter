using System;
using System.Text;

namespace DynamicTemplate.Compiler
{
    /// <summary>
    /// A string buffer that can be appended to, and keeps track of what
    /// position (line/column) it is currently at.
    /// </summary>
    class PositionBuffer
    {
        private StringBuilder _buf = new StringBuilder();
        private int _line = 1;
        private int _col = 1;

        public void Append(string str)
        {
            _buf.Append(str);
            foreach (char c in str)
            {
                switch (c)
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
        }

        public void AppendFormat(string format, params object[] parameters)
        {
            Append(string.Format(format, parameters));
        }

        public Position Position { get { return new Position(_line, _col); } }

        public override string ToString()
        {
            return _buf.ToString();
        }
    }
}
