// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Linq;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace OpenLiveWriter.EditorTests.Automated.Infrastructure
{
    /// <summary>
    /// Small AngleSharp wrapper so tests assert on real parsed DOM structure
    /// (tags/attributes) instead of brittle substring matching on HTML strings.
    /// </summary>
    public static class Dom
    {
        private static readonly HtmlParser Parser = new HtmlParser();

        /// <summary>Parses an editor-body HTML fragment into a document.</summary>
        public static IDocument Parse(string html)
        {
            return Parser.ParseDocument("<!DOCTYPE html><html><body>" + (html ?? string.Empty) + "</body></html>");
        }

        /// <summary>Parses a fragment and returns the body element.</summary>
        public static IElement ParseBody(string html) => Parse(html).Body;

        /// <summary>Returns true when at least one element matching the selector exists.</summary>
        public static bool Has(string html, string selector) =>
            Parse(html).QuerySelector(selector) != null;

        /// <summary>Counts elements matching the selector.</summary>
        public static int Count(string html, string selector) =>
            Parse(html).QuerySelectorAll(selector).Length;

        /// <summary>Returns the first element matching the selector (or null).</summary>
        public static IElement Select(string html, string selector) =>
            Parse(html).QuerySelector(selector);

        /// <summary>
        /// Returns the set of distinct lower-cased element tag names present in the
        /// body of the supplied HTML fragment.
        /// </summary>
        public static string[] TagNames(string html) =>
            Parse(html).Body.QuerySelectorAll("*")
                .Select(e => e.LocalName.ToLowerInvariant())
                .Distinct()
                .ToArray();
    }
}
