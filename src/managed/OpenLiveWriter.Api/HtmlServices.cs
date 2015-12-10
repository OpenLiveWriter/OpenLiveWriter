// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// Utility methods for the manipulation of HTML (and related) content.
    /// </summary>
    public sealed class HtmlServices
    {
        /// <summary>
        /// Convert plain-text to HTML by escaping standard HTML entities.
        /// </summary>
        /// <param name="plainText">Plain-text to escape.</param>
        /// <returns>Escaped HTML content.</returns>
        public static string HtmlEncode(string plainText)
        {
            return HtmlUtils.EscapeEntities(plainText);
        }

        /// <summary>
        /// Unescapes HTML entities.
        /// </summary>
        /// <param name="htmlText">The string to unescape.</param>
        /// <returns>The unescaped string.</returns>
        public static string HtmlDecode(string htmlText)
        {
            return HtmlUtils.UnEscapeEntities(htmlText, HtmlUtils.UnEscapeMode.Default);
        }

        /*
                /// <summary>
                /// An operation that transforms HTML text.
                /// </summary>
                /// <param name="htmlText">The input HTML text. May or may not be escaped,
                /// depending on the context in which the operation is called.</param>
                /// <returns>Transformed HTML text.</returns>
                public delegate string TextTransformationOperation(string htmlText);

                /// <summary>
                /// Searches and replaces only the text of the given HTML--tags,
                /// comments, and directives will be unchanged.
                /// </summary>
                /// <param name="html">The input HTML that will be searched.</param>
                /// <param name="autoEscape">If true, then inputs to the TextTransformationOperation will
                /// be pre-unescaped, and the return value from the TextTransformationOperation will be
                /// escaped before inserting it into the output HTML.</param>
                /// <param name="operation">An operation that transforms text.</param>
                /// <returns>The transformed HTML.</returns>
                /// <example>This example replaces the word "foo" with the word "bar".
                ///
                /// <code>string newHtml = TransformHtmlText(html, true,
                ///     (text) => { Regex.Replace(text, @"\bfoo\b", "bar") });</code>
                /// </example>
                public static string TransformHtmlText(string html, bool autoEscape, TextTransformationOperation operation)
                {
                    StringBuilder sb = new StringBuilder();

                    SimpleHtmlParser p = new SimpleHtmlParser(html);
                    Element e;
                    while (null != (e = p.Next()))
                    {
                        if (e is Text)
                        {
                            string val = e.RawText;

                            if (autoEscape)
                                val = HtmlDecode(val);

                            val = operation(val) ?? "";

                            if (autoEscape)
                                val = HtmlEncode(val);

                            sb.Append(val);
                        }
                        else
                        {
                            sb.Append(html, e.Offset, e.Length);
                        }
                    }

                    return sb.ToString();
                }
        */
    }

}
