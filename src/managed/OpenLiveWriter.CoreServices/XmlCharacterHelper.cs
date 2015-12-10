// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Text;
using System.Xml;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for XmlHelper.
    /// </summary>
    public class XmlCharacterHelper
    {
        private XmlCharacterHelper()
        {
        }

        public static string RemoveInvalidXmlChars(string xmlString)
        {
            if (xmlString == null || xmlString.Length == 0)
                return xmlString;

            StringBuilder sb = null;
            for (int i = 0; i < xmlString.Length; i++)
            {
                char ch = xmlString[i];
                if (IsValidXmlChar(ch))
                {
                    if (sb != null)
                        sb.Append(ch);
                }
                else
                {
                    if (sb == null)
                    {
                        //there are invalid characters in this string, so move all valid chars
                        //up to this point into the string builder
                        sb = new StringBuilder(xmlString.Substring(0, i));
                    }
                }
            }
            if (sb == null)
                return xmlString;
            else
                return sb.ToString();
        }

        public static bool IsValidXmlChar(char ch)
        {
            //is the character from the valid XML character ranges?
            //Note: these ranges were discovered using a program that tested
            //all possible character values.
            bool isValid = (ch >= 9 && ch <= 10) ||
                (ch == 13) ||
                (ch >= 32 && ch <= 55295) ||
                (ch >= 57344 && ch <= 65533);
            return isValid;
        }

        /// <summary>
        /// Creates a text reader that will replace invalid XML chars with whitespace
        /// so that the XML parser will not abort while reading the XML.
        /// </summary>
        /// <param name="reader"></param>
        public static TextReader CreateSafeTextReader(TextReader reader)
        {
            if (!(reader is SafeXmlTextReader))
            {
                reader = new SafeXmlTextReader(reader);
            }
            return reader;
        }

        /// <summary>
        /// A text reader that replaces invalid characters reader from the underlying
        /// text reader with whitespace.
        /// </summary>
        private class SafeXmlTextReader : TextReader
        {
            TextReader _reader;
            public SafeXmlTextReader(TextReader reader)
            {
                _reader = reader;
            }
            public override int Read()
            {
                currentReadVal = _reader.Read();
                if (currentReadVal != -1 && !XmlCharacterHelper.IsValidXmlChar((char)currentReadVal))
                {
                    //this is an invalid character, so replace it with whitespace (since that is almost always safe)
                    currentReadVal = ' ';
                }
                return currentReadVal;
            }
            int currentReadVal = -2;

            public override int Peek()
            {
                if (currentReadVal == -2)
                    return _reader.Peek();
                else
                    return currentReadVal;
            }
            public override void Close()
            {
                _reader.Close();
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                if (disposing)
                {
                    ((IDisposable)_reader).Dispose();
                }
            }
        }
    }
}
