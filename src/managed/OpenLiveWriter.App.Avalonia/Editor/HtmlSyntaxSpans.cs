// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    /// <summary>Semantic span kinds for HTML source highlighting.</summary>
    internal enum HtmlSpanKind
    {
        Text,
        Comment,
        TagName,
        AttributeName,
        AttributeValue,
        EmbeddedImageToken,
    }

    /// <summary>
    /// A tiny, dependency-free HTML span classifier for the Source view's syntax
    /// highlighting. Blog post HTML is simple and well-formed enough that a
    /// hand-rolled scanner beats dragging in a grammar engine: it tags comments,
    /// tag names, attribute names/values, and the <c>data-olw-img:N</c> elision
    /// tokens (see <see cref="SourceViewSanitizer"/>). Pure and unit-testable.
    /// </summary>
    internal static class HtmlSyntaxSpans
    {
        public readonly struct Span
        {
            public Span(int start, int length, HtmlSpanKind kind)
            {
                Start = start;
                Length = length;
                Kind = kind;
            }

            public int Start { get; }
            public int Length { get; }
            public HtmlSpanKind Kind { get; }
        }

        /// <summary>Classifies a line of HTML into highlightable spans.</summary>
        public static List<Span> Compute(string text)
        {
            var spans = new List<Span>();
            if (string.IsNullOrEmpty(text))
                return spans;

            int i = 0, n = text.Length, textStart = 0;
            void FlushText(int end)
            {
                if (end > textStart)
                    spans.Add(new Span(textStart, end - textStart, HtmlSpanKind.Text));
            }

            while (i < n)
            {
                if (text[i] != '<')
                {
                    i++;
                    continue;
                }

                // Comment <!-- ... -->
                if (i + 3 < n && text[i + 1] == '!' && text[i + 2] == '-' && text[i + 3] == '-')
                {
                    FlushText(i);
                    int end = text.IndexOf("-->", i + 4, StringComparison.Ordinal);
                    end = end < 0 ? n : end + 3;
                    spans.Add(new Span(i, end - i, HtmlSpanKind.Comment));
                    i = textStart = end;
                    continue;
                }

                // Tag open/close: classify tag name, then attributes to '>'
                FlushText(i);
                i++;
                if (i < n && text[i] == '/')
                    i++;

                int nameStart = i;
                while (i < n && (char.IsLetterOrDigit(text[i]) || text[i] == '-' || text[i] == ':'))
                    i++;
                if (i > nameStart)
                    spans.Add(new Span(nameStart, i - nameStart, HtmlSpanKind.TagName));

                // Attributes until '>' or '/>'
                while (i < n && text[i] != '>')
                {
                    if (char.IsWhiteSpace(text[i]) || text[i] == '/')
                    {
                        i++;
                        continue;
                    }

                    int attrStart = i;
                    while (i < n && (char.IsLetterOrDigit(text[i]) || text[i] == '-' || text[i] == ':' || text[i] == '_'))
                        i++;
                    if (i > attrStart)
                        spans.Add(new Span(attrStart, i - attrStart, HtmlSpanKind.AttributeName));

                    while (i < n && char.IsWhiteSpace(text[i]))
                        i++;
                    if (i < n && text[i] == '=')
                    {
                        i++;
                        while (i < n && char.IsWhiteSpace(text[i]))
                            i++;
                        if (i < n && (text[i] == '"' || text[i] == '\''))
                        {
                            char quote = text[i];
                            int valStart = i;
                            i++;
                            while (i < n && text[i] != quote)
                                i++;
                            if (i < n)
                                i++;

                            bool isImageToken = text.AsSpan(valStart, i - valStart)
                                .Contains(SourceViewSanitizer.TokenPrefix.AsSpan(), StringComparison.Ordinal);
                            spans.Add(new Span(valStart, i - valStart,
                                isImageToken ? HtmlSpanKind.EmbeddedImageToken : HtmlSpanKind.AttributeValue));
                        }
                    }
                }
                if (i < n && text[i] == '>')
                    i++;
                textStart = i;
            }

            FlushText(n);
            return spans;
        }
    }
}
