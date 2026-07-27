// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    /// <summary>
    /// Applies <see cref="HtmlSyntaxSpans"/> classification as per-line colorization
    /// in the Source view (dark-theme palette modelled on VS Code's Dark+).
    /// </summary>
    internal sealed class HtmlSyntaxColorizer : DocumentColorizingTransformer
    {
        private static readonly IBrush CommentBrush = new SolidColorBrush(Color.FromRgb(0x6A, 0x99, 0x55));
        private static readonly IBrush TagBrush = new SolidColorBrush(Color.FromRgb(0x56, 0x9C, 0xD6));
        private static readonly IBrush AttributeBrush = new SolidColorBrush(Color.FromRgb(0x9C, 0xDC, 0xFE));
        private static readonly IBrush ValueBrush = new SolidColorBrush(Color.FromRgb(0xCE, 0x91, 0x78));
        private static readonly IBrush TokenBrush = new SolidColorBrush(Color.FromRgb(0xC5, 0x86, 0xC0));

        protected override void ColorizeLine(DocumentLine line)
        {
            string text = CurrentContext.Document.GetText(line);
            if (string.IsNullOrEmpty(text))
                return;

            foreach (HtmlSyntaxSpans.Span span in HtmlSyntaxSpans.Compute(text))
            {
                IBrush brush = span.Kind switch
                {
                    HtmlSpanKind.Comment => CommentBrush,
                    HtmlSpanKind.TagName => TagBrush,
                    HtmlSpanKind.AttributeName => AttributeBrush,
                    HtmlSpanKind.AttributeValue => ValueBrush,
                    HtmlSpanKind.EmbeddedImageToken => TokenBrush,
                    _ => null,
                };
                if (brush == null)
                    continue;

                int start = line.Offset + span.Start;
                int end = start + span.Length;
                if (start < line.EndOffset)
                    ChangeLinePart(start, System.Math.Min(end, line.EndOffset),
                        element => element.TextRunProperties.SetForegroundBrush(brush));
            }
        }
    }
}
