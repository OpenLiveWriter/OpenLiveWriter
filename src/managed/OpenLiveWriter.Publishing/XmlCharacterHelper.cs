// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Text;

namespace OpenLiveWriter.Publishing
{
    /// <summary>
    /// Cross-platform port of <c>OpenLiveWriter.CoreServices.XmlCharacterHelper</c>.
    /// Strips characters that are not valid in XML 1.0 so that the XML-RPC payload
    /// serializes without aborting the parser. The valid-range logic is copied
    /// verbatim from the Windows implementation.
    /// </summary>
    public static class XmlCharacterHelper
    {
        public static string RemoveInvalidXmlChars(string xmlString)
        {
            if (string.IsNullOrEmpty(xmlString))
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
                        // there are invalid characters in this string, so move all valid chars
                        // up to this point into the string builder
                        sb = new StringBuilder(xmlString.Substring(0, i));
                    }
                }
            }
            return sb == null ? xmlString : sb.ToString();
        }

        public static bool IsValidXmlChar(char ch)
        {
            // is the character from the valid XML character ranges?
            // Note: these ranges were discovered using a program that tested
            // all possible character values.
            return (ch >= 9 && ch <= 10) ||
                   (ch == 13) ||
                   (ch >= 32 && ch <= 55295) ||
                   (ch >= 57344 && ch <= 65533);
        }
    }
}
