// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Text;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Breaks a string into tokens according to one or more delimiters.
    /// </summary>
    public class StringTokenizer : IEnumerable, IEnumerator
    {
        private readonly string stringValue;
        private readonly bool returnDelimiters;
        private readonly string[] delimiters;

        private int offset;
        private int len;

        public StringTokenizer(string stringValue, bool returnDelimiters, params string[] delimiters)
        {
            foreach (string delimiter in delimiters)
            {
                if (delimiter == null || delimiter == string.Empty)
                {
                    Trace.Fail("Null or empty string cannot be used as delimiter");
                    throw new ArgumentOutOfRangeException("Null or empty string cannot be used as delimiter");
                }
            }

            this.stringValue = stringValue;
            this.returnDelimiters = returnDelimiters;
            this.delimiters = delimiters;

            this.offset = 0;
            this.len = 0;
        }

        /// <summary>
        /// Initializes a tokenizer; delimiters will not be returned.
        /// </summary>
        /// <param name="stringValue">The string to be tokenized.</param>
        /// <param name="delimiters">The strings that delimit tokens.</param>
        public StringTokenizer(string stringValue, params string[] delimiters) : this(stringValue, false, delimiters)
        {
        }

        public bool MoveNext()
        {
            retry:

            offset += len;
            len = 0;
            if (offset >= stringValue.Length)
                return false;

            int delimStart;
            int delimLen;
            FindFirstDelimiter(offset, out delimStart, out delimLen);

            if (delimStart == offset)
            {
                len = delimLen;
                if (returnDelimiters)
                    return true;
                else
                    goto retry;  // same as calling return MoveNext(), but without using up the stack
            }
            else
            {
                len = delimStart - offset;
                return true;
            }
        }

        /// <summary>
        /// Finds the first delimiter from startIndex on.
        /// If none found, offset will be greater than stringValue.Length
        /// and len will be -1.
        /// </summary>
        private void FindFirstDelimiter(int startIndex, out int offset, out int len)
        {
            for (offset = startIndex; offset < stringValue.Length; offset++)
            {
                len = DetectDelimiter(offset);
                if (len != -1)
                    return;
            }
            len = -1;
        }

        private int DetectDelimiter(int offset)
        {
            int lenLeft = stringValue.Length - offset;
            foreach (string delimiter in delimiters)
            {
                // not enough space for this delimiter
                if (lenLeft < delimiter.Length)
                    continue;

                if (stringValue.Substring(offset, delimiter.Length) == delimiter)
                    return delimiter.Length;
            }
            return -1;
        }

        public void Reset()
        {
            this.offset = 0;
            this.len = 0;
        }

        public IEnumerator GetEnumerator()
        {
            return this;
        }

        public object Current
        {
            get
            {
                if (len == 0)
                    throw new InvalidOperationException();

                return stringValue.Substring(offset, len);
            }
        }
    }
}
