// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace LocUtil
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    using JetBrains.Annotations;

    /// <summary>
    /// Class CsvParser.
    /// </summary>
    /// <seealso cref="System.Collections.IEnumerable" />
    /// <seealso cref="System.IDisposable" />
    public class CsvParser : IEnumerable, IDisposable
    {
        /// <summary>
        /// At line end
        /// </summary>
        private bool atLineEnd = false;

        /// <summary>
        /// The input
        /// </summary>
        private readonly TextReader input;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvParser"/> class.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="skipFirstLine">if set to <c>true</c> [skip first line].</param>
        public CsvParser([NotNull] TextReader input, bool skipFirstLine)
        {
            this.input = input;
            if (skipFirstLine)
            {
                while (this.NextWord() != null)
                {
                }

                this.NextLine();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.input.Close();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        public IEnumerator GetEnumerator() => new LineEnumerator(this);

        /// <summary>
        /// Eats the line endings.
        /// </summary>
        private void EatLineEndings()
        {
            while (true)
            {
                switch (this.input.Peek())
                {
                    case '\r':
                    case '\n':
                        this.input.Read();
                        break;
                    default:
                        return;
                }
            }
        }

        /// <summary>
        /// Eats the spaces.
        /// </summary>
        private void EatSpaces()
        {
            while (this.input.Peek() == ' ')
            {
                this.input.Read();
            }
        }

        /// <summary>
        /// Matches the quoted.
        /// </summary>
        /// <returns>The string.</returns>
        /// <exception cref="System.ArgumentException">Malformed CSV: unexpected character after string \"" + sb.ToString() + "\"</exception>
        private string MatchQuoted()
        {
            Debug.Assert(this.input.Read() == '"');
            var sb = new StringBuilder();
            for (var c = this.input.Read(); c != -1; c = this.input.Read())
            {
                if (c == '"')
                {
                    if (this.input.Peek() == '"')
                    {
                        this.input.Read();
                        sb.Append('"');
                    }
                    else
                    {
                        this.EatSpaces();
                        switch (this.input.Peek())
                        {
                            case ',':
                                this.input.Read();
                                return sb.ToString();
                            case '\r':
                            case '\n':
                            case -1:
                                this.atLineEnd = true;
                                this.EatLineEndings();
                                return sb.ToString();
                            default:
                                throw new ArgumentException(
                                          "Malformed CSV: unexpected character after string \"" + sb.ToString() + "\"");
                        }
                    }

                }
                else
                {
                    sb.Append((char)c);
                }
            }

            Debug.Fail("Unterminated quoted string: " + sb.ToString());
            return sb.ToString();
        }

        /// <summary>
        /// Matches the unquoted.
        /// </summary>
        /// <returns>System.String.</returns>
        private string MatchUnquoted()
        {
            var sb = new StringBuilder();
            for (var c = this.input.Read(); c != -1; c = this.input.Read())
            {
                switch (c)
                {
                    case ',':
                        return sb.ToString();
                    case '\r':
                    case '\n':
                        this.atLineEnd = true;
                        this.EatLineEndings();
                        return sb.ToString();
                    default:
                        sb.Append((char)c);
                        break;
                }
            }

            // EOF
            this.atLineEnd = true;
            return sb.ToString();
        }

        /// <summary>
        /// Nexts the line.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool NextLine()
        {
            if (this.input.Peek() == -1)
            {
                return false;
            }

            this.atLineEnd = false;
            return true;
        }

        /// <summary>
        /// Gets the next word.
        /// </summary>
        /// <returns>The next word.</returns>
        [CanBeNull]
        private string NextWord()
        {
            if (this.atLineEnd)
            {
                return null;
            }

            this.EatSpaces();
            switch (this.input.Peek())
            {
                case -1:
                    return null;
                case '"':
                    return this.MatchQuoted();
                case '\r':
                case '\n':
                    this.EatLineEndings();
                    return null;
                default:
                    return this.MatchUnquoted();
            }
        }

        /// <summary>
        /// Class LineEnumerator.
        /// </summary>
        /// <seealso cref="System.Collections.IEnumerator" />
        private class LineEnumerator : IEnumerator
        {
            /// <summary>
            /// The parent
            /// </summary>
            CsvParser parent;

            /// <summary>
            /// The line
            /// </summary>
            string[] line;

            /// <summary>
            /// Initializes a new instance of the <see cref="LineEnumerator"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            public LineEnumerator(CsvParser parent)
            {
                this.parent = parent;
            }

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
            public bool MoveNext()
            {
                start:
                if (this.line != null && !this.parent.NextLine())
                {
                    return false;
                }

                var words = new ArrayList();
                string word;
                while ((word = this.parent.NextWord()) != null)
                {
                    words.Add(word);
                }

                this.line = (string[])words.ToArray(typeof(string));
                if (this.line.Length == 0)
                {
                    goto start;
                }

                return true;
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            /// <exception cref="System.NotSupportedException">Not supported.</exception>
            public void Reset()
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            /// <value>The current.</value>
            public object Current => this.line;
        }
    }
}
