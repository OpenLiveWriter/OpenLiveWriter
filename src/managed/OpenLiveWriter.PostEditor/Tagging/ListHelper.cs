// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Tagging
{
    /// <summary>
    /// Summary description for ListHelper.
    /// </summary>
    /// <summary>
    /// Summary description for ListHelper.
    /// </summary>
    public class ListHelper
    {
        private static readonly string delimsRegexPattern;
        static ListHelper()
        {
            // Sanity checking. If these preconditions are not true, NormalizeList breaks.
            if (Res.Comma.ToString() != Res.ListSeparator.Trim())
                throw new ArgumentException("The localized values for Comma and CommaSpace must differ only by whitespace.");

            char localizedDelim = Res.Comma;
            if (localizedDelim == ';' || localizedDelim == ',')
                delimsRegexPattern = "[;,]";
            else
                delimsRegexPattern = "[;," + Regex.Escape(localizedDelim + "") + "]";
        }

        public static string NormalizeList(string listString, string delim)
        {
            string delimPattern = Regex.Escape(delim);
            // convert semicolons, and commas to delim
            listString = Regex.Replace(listString, delimsRegexPattern, delim, RegexOptions.Multiline);
            // remove whitespace around delim
            listString = Regex.Replace(listString, @"\s*" + delimPattern + @"\s*", delim);
            // compress multiple consecutive delim into delim
            listString = Regex.Replace(listString, "(" + delimPattern + "){2,}", delim);
            // compress multiple consecutive whitespace chars into one space
            listString = Regex.Replace(listString, @"\s+", " ");
            // remove trailing comma
            listString = Regex.Replace(listString, delimPattern + @"$", "", RegexOptions.Multiline);
            // remove leading comma
            listString = Regex.Replace(listString, @"^" + delimPattern, "", RegexOptions.Multiline);

            return listString;
        }
    }
}
