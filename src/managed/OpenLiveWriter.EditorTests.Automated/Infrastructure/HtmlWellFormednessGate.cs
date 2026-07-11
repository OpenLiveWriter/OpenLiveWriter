// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace OpenLiveWriter.EditorTests.Automated.Infrastructure
{
    /// <summary>
    /// The publish-readiness gate (scenario A16). Verifies that a chunk of editor
    /// HTML is well-formed enough to be embedded in an XML-RPC MetaWeblog payload:
    /// every tag closed / properly nested, only XML-safe characters, and no
    /// undeclared entities. Unlike a forgiving HTML parser (which silently repairs
    /// malformed input), this validates the markup as XML so genuinely broken
    /// output is rejected.
    /// </summary>
    public static class HtmlWellFormednessGate
    {
        // HTML void elements that never have a closing tag.
        private static readonly string[] VoidElements =
        {
            "area", "base", "br", "col", "embed", "hr", "img", "input",
            "link", "meta", "param", "source", "track", "wbr"
        };

        // The handful of named entities the editor commonly emits, mapped to the
        // numeric references that a bare XML reader understands.
        private static readonly (string Named, string Numeric)[] NamedEntities =
        {
            ("&nbsp;", "&#160;"),
            ("&copy;", "&#169;"),
            ("&reg;", "&#174;"),
            ("&trade;", "&#8482;"),
            ("&mdash;", "&#8212;"),
            ("&ndash;", "&#8211;"),
            ("&hellip;", "&#8230;"),
            ("&laquo;", "&#171;"),
            ("&raquo;", "&#187;"),
            ("&rsquo;", "&#8217;"),
            ("&lsquo;", "&#8216;"),
            ("&ldquo;", "&#8220;"),
            ("&rdquo;", "&#8221;")
        };

        public sealed class Result
        {
            public bool IsWellFormed => Errors.Count == 0;
            public List<string> Errors { get; } = new List<string>();
            public override string ToString() =>
                IsWellFormed ? "well-formed" : "NOT well-formed: " + string.Join("; ", Errors);
        }

        public static Result Validate(string html)
        {
            var result = new Result();
            html ??= string.Empty;

            foreach (var ch in html)
            {
                if (!IsValidXmlChar(ch))
                {
                    result.Errors.Add($"invalid XML character U+{(int)ch:X4}");
                    break;
                }
            }

            var xml = "<root>" + PrepareForXml(html) + "</root>";
            try
            {
                var settings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Prohibit,
                    CheckCharacters = true
                };
                using var reader = XmlReader.Create(new StringReader(xml), settings);
                while (reader.Read()) { /* consume to surface any structural errors */ }
            }
            catch (XmlException ex)
            {
                result.Errors.Add($"XML parse error: {ex.Message}");
            }

            return result;
        }

        public static bool IsWellFormed(string html) => Validate(html).IsWellFormed;

        // Converts an editor HTML fragment into XML-parseable markup: self-closes
        // void elements and rewrites known named entities to numeric references.
        private static string PrepareForXml(string html)
        {
            foreach (var (named, numeric) in NamedEntities)
                html = html.Replace(named, numeric);

            foreach (var tag in VoidElements)
            {
                // <hr>, <hr/>, <br class="x">  ->  <hr/>, <br class="x"/>
                html = Regex.Replace(
                    html,
                    $@"<{tag}(\s[^>]*?)?\s*/?>",
                    m =>
                    {
                        var attrs = m.Groups[1].Success ? m.Groups[1].Value.TrimEnd() : string.Empty;
                        return $"<{tag}{attrs}/>";
                    },
                    RegexOptions.IgnoreCase);
            }

            return html;
        }

        // Mirrors OpenLiveWriter.CoreServices.XmlCharacterHelper.IsValidXmlChar so
        // the publish gate uses the same character contract as the Windows pipeline
        // (CoreServices is net10.0-windows and not referenceable from this project).
        public static bool IsValidXmlChar(char ch) =>
            (ch >= 9 && ch <= 10) ||
            (ch == 13) ||
            (ch >= 32 && ch <= 55295) ||
            (ch >= 57344 && ch <= 65533);
    }
}
