// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for ResponseHeaderHelper.
    /// </summary>
    public class ResponseHeaderHelper
    {
        /// <summary>
        /// Gets the value of a particular header from a set of response headers
        /// </summary>
        /// <param name="header">The response header to get the value of</param>
        /// <param name="ResponseString">The string containing the response headers</param>
        /// <returns>The value, null if the header cannot be found</returns>
        public static string GetHeaderValue(string header, string ResponseString)
        {
            string headerValue = null;
            string[] headerLines = ResponseString.Split('\n');
            foreach (string currentLine in headerLines)
            {
                if (currentLine.IndexOf(":", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    string[] headerItems = currentLine.Split(':');
                    if (headerItems[0].Trim() == header)
                    {
                        headerValue = headerItems[1].Trim();
                        break;
                    }
                }
            }
            return headerValue;
        }
    }
}
