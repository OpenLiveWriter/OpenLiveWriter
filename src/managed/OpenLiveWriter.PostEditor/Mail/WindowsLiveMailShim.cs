// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using OpenLiveWriter.CoreServices.HTML;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.Mail
{
    public static class EmailShim
    {
        internal static uint GetCodepage(uint codepage)
        {
            // Mail sometimes returns 'special' codepages.  We need to convert them to normal codepages
            // This values taken from %inetroot%\client\mail\mail\staticrt\lang.cpp
            switch (codepage)
            {
                case 50949:
                case 50225:
                    return 949;
                case 50932:
                case 50221:
                case 50222:
                    return 50220;
                case 20127:
                    return 28591;
                default:
                    return codepage;

            }
        }

        internal static string GetContentHtml(string sourceUrl, string html)
        {
            StringBuilder sb = new StringBuilder();
            using (StringWriter writer = new StringWriter(sb, CultureInfo.InvariantCulture))
            {
                ExternalHtmlReferenceFixer fixer = new ExternalHtmlReferenceFixer(html, sourceUrl);
                fixer.FixReferences(writer, ExternalReferenceFixer, null);
            }

            return sb.ToString();
        }

        internal static ExternalReferenceFixer ExternalReferenceFixer =
            delegate (BeginTag tag, string reference, string sourceUrl)
            {
                if (reference.StartsWith("cid:", StringComparison.OrdinalIgnoreCase))
                    return sourceUrl + "!" + reference;

                return reference;
            };
    }
}
