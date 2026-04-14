// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using mshtml;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// HTMLDataObject is a Mindshare Data Object that contains HTML text fragments.
    /// It includes an html document based upon the HTML DOM (IHTMLDocument2) as well
    /// as various properties derived from the HTML fragment contained in the IDataObject.
    /// </summary>
    public class HTMLDataObject : DataObjectBase
    {
        /// <summary>
        /// Creates an HTMLDataObject from a html string and source URL.
        /// This can return null if the HTMLDataObject couldn't be created.
        /// </summary>
        /// <param name="html">The html from which to create the dataobject</param>
        /// <param name="sourceUrl">The source Url that the html is from</param>
        /// <returns>The HTMLDataObject, null if it couldn't be created</returns>
        public HTMLDataObject(string html, string sourceUrl)
        {
            IDataObject =
                new DataObject(DataFormats.Html, GetHTMLFormatString(html, sourceUrl));
        }

        public HTMLDataObject(IHTMLDocument2 document)
        {
            string html = HTMLDocumentHelper.HTMLDocToString(document);
            IDataObject = new DataObject(DataFormats.Html, GetHTMLFormatString(html, document.url));

        }

        /// <summary>
        /// Creates an HTMLDataObject from an IHTMLDocument2 and an IHTMLSelectionObject.
        /// </summary>
        /// <param name="document">The document containing the selection</param>
        /// <param name="selection">The selection from which to create the HTMLDataObject</param>
        /// <returns>An HTMLDataObject, null if no HTMLDataObject could be created.</returns>
        public HTMLDataObject(IHTMLTxtRange textRange, IHTMLDocument2 document)
        {
            IDataObject = new DataObject(DataFormats.Html,
                GetHTMLFormatString(textRange.htmlText, document.url));
        }

        /// <summary>
        /// Creates an HTMLDataObject from an IHTMLDocument2 and an IHTMLElement.
        /// </summary>
        /// <param name="document">The document containing the element</param>
        /// <param name="element">The element from which to create the HTMLDataObject</param>
        /// <returns>An HTMLDataObject, null if no HTMLDataObject could be created.</returns>
        public HTMLDataObject(IHTMLElement element, IHTMLDocument2 document)
        {
            // try creating based upon the element
            IDataObject = new DataObject(DataFormats.Html,
                GetHTMLFormatString(element.outerHTML, document.url));
        }

        /// <summary>
        /// Strips any fragment markers from an HTML string
        /// </summary>
        /// <param name="html">The html</param>
        /// <returns>The html without any fragment markers</returns>
        public static string StripFragmentMarkers(string html)
        {
            html = html.Replace(START_FRAGMENT_MARKER, string.Empty);
            html = html.Replace(END_FRAGMENT_MARKER, string.Empty);
            return html;
        }

        public const string START_FRAGMENT_MARKER = "<!--StartFragment-->";
        public const string END_FRAGMENT_MARKER = "<!--EndFragment-->";

        /// <summary>
        /// Converts HTML into a html clipboard format string, including headers
        /// required to place html into the html dataformat.
        /// </summary>
        /// <param name="html">The html</param>
        /// <param name="sourceUrl">The source url for the html</param>
        /// <returns>The html clipboard format string.</returns>
        public static string GetHTMLFormatString(string html, string sourceUrl)
        {
            // The length (in bytes) of the HTML header before adding the
            // source URL
            const int HEADER_LENGTH_NO_URL = 155;

            if (html == null)
                return null;

            // Mark the Fragment.
            if (html.IndexOf("<HTML>", StringComparison.OrdinalIgnoreCase) == -1)
                html = START_FRAGMENT_MARKER + html + END_FRAGMENT_MARKER;
            else
            {
                Match match = Regex.Match(html, @"<body(\s[^>]*)?>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (match.Success)
                {
                    html = html.Insert(match.Index + match.Length, START_FRAGMENT_MARKER);
                    Match match2 = Regex.Match(html, @"</body\s*>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    Trace.Assert(match2.Success, "No close body tag found");
                    html = html.Insert(match2.Index, END_FRAGMENT_MARKER);
                }
                else
                {
                    // no body tag.  look for frameset.
                    Match fmatch = Regex.Match(html, @"<frameset(\s[^>]*)?>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    if (fmatch.Success)
                    {
                        html = html.Insert(fmatch.Index, START_FRAGMENT_MARKER);
                        Match fmatch2 = Regex.Match(html, @"</frameset\s*>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        Trace.Assert(fmatch2.Success, "No close frameset tag found");
                        html = html.Insert(fmatch2.Index + fmatch2.Length, END_FRAGMENT_MARKER);
                    }
                    else
                    {
                        Trace.Fail("Neither frameset nor body found");
                        html = START_FRAGMENT_MARKER + html + END_FRAGMENT_MARKER;
                    }
                }
            }

            string validHTML = html;

            // Create a valid HTML document for the fragment
            // string validHTML = HTMLDocumentHelper.CreateValidHTMLDocument(html, sourceUrl);

            // Calculate the bytes to encode the whole html document
            int htmlDocLength = Encoding.UTF8.GetByteCount(validHTML);

            // The start of the HTML (where the header ends).
            // Its safe to use the character count because the header and URL
            // are always low ascii
            int startHTML = HEADER_LENGTH_NO_URL + sourceUrl.Length;

            // The end of the HTML
            int EndHTML = startHTML + htmlDocLength;

            // find the fragment and get the string leading up to it
            int startFragment;
            int endFragment;
            int startSelection;
            int endSelection;
            int fragLocation = validHTML.IndexOf(START_FRAGMENT_MARKER, StringComparison.OrdinalIgnoreCase);
            if (fragLocation > 0)
            {
                // Calculate the length of the string leading up to the fragment
                string htmlLeadingString = validHTML.Substring(0, fragLocation);
                int htmlLeadingLength = Encoding.UTF8.GetByteCount(htmlLeadingString);

                // Get the start and length of the html fragment (as a string)
                int startFragString = validHTML.IndexOf(START_FRAGMENT_MARKER, StringComparison.OrdinalIgnoreCase) + START_FRAGMENT_MARKER.Length;
                int fragStringLength = validHTML.IndexOf(END_FRAGMENT_MARKER, StringComparison.OrdinalIgnoreCase) - startFragString;

                // Get the html fragment
                string htmlFragmentString = validHTML.Substring(
                        startFragString,
                        fragStringLength);

                startFragment = startHTML + htmlLeadingLength + START_FRAGMENT_MARKER.Length;
                int lenFragBytes = Encoding.UTF8.GetByteCount(htmlFragmentString);
                endFragment = startFragment + lenFragBytes;
            }
            else
            {
                startFragment = startHTML;
                endFragment = EndHTML;
            }

            // The selection is the same as the selection
            startSelection = startFragment;
            endSelection = endFragment;

            StringBuilder hBuilder = new StringBuilder();
            hBuilder.Append(Headers.Version + ":1.0\n");
            hBuilder.Append(Headers.StartHTML + ":" + startHTML.ToString(ByteCountFormatMask, CultureInfo.InvariantCulture) + "\n");
            hBuilder.Append(Headers.EndHTML + ":" + EndHTML.ToString(ByteCountFormatMask, CultureInfo.InvariantCulture) + "\n");
            hBuilder.Append(Headers.StartFragment + ":" + startFragment.ToString(ByteCountFormatMask, CultureInfo.InvariantCulture) + "\n");
            hBuilder.Append(Headers.EndFragment + ":" + endFragment.ToString(ByteCountFormatMask, CultureInfo.InvariantCulture) + "\n");
            hBuilder.Append(Headers.StartSelection + ":" + startSelection.ToString(ByteCountFormatMask, CultureInfo.InvariantCulture) + "\n");
            hBuilder.Append(Headers.EndSelection + ":" + endSelection.ToString(ByteCountFormatMask, CultureInfo.InvariantCulture) + "\n");
            hBuilder.Append(Headers.SourceURL + ":" + sourceUrl + "\n");
            hBuilder.Append(validHTML);

            return hBuilder.ToString();
        }

        /// <summary>
        /// Headers used in the Clipboard HTML Format
        /// </summary>
        public struct Headers
        {
            public const string Version = "Version";
            public const string StartHTML = "StartHTML";
            public const string EndHTML = "EndHTML";
            public const string StartFragment = "StartFragment";
            public const string EndFragment = "EndFragment";
            public const string StartSelection = "StartSelection";
            public const string EndSelection = "EndSelection";
            public const string SourceURL = "SourceURL";
        }

        /// <summary>
        /// The mask used in Clipboard HTML Format headers indicating byte
        /// count position
        /// </summary>
        public const string ByteCountFormatMask = "000000000";
    }
}
