// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace OpenLiveWriter.Markdown
{
    /// <summary>
    /// Walks HTML with AngleSharp and emits GFM Markdown. Unknown elements are preserved
    /// as raw HTML passthrough blocks.
    /// </summary>
    public sealed class HtmlToMarkdownConverter
    {
        private static readonly HashSet<string> KnownBlockTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "blockquote", "div", "h1", "h2", "h3", "h4", "h5", "h6", "hr", "li", "ol", "p", "pre", "table", "ul"
        };

        private static readonly HashSet<string> KnownInlineTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "a", "b", "br", "code", "del", "em", "i", "img", "input", "s", "strong", "span", "sub", "sup"
        };

        private static readonly HashSet<string> TableSectionTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "thead", "tbody", "tfoot", "tr", "th", "td", "colgroup", "col", "caption"
        };

        /// <summary>
        /// Converts an HTML fragment or document to GFM Markdown.
        /// </summary>
        public string Convert(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return string.Empty;
            }

            var context = BrowsingContext.New(Configuration.Default);
            var parser = context.GetService<IHtmlParser>();
            var document = parser.ParseDocument("<body>" + html + "</body>");
            var output = new StringBuilder();
            ConvertBlockChildren(document.Body, output);
            return NormalizeOutput(output.ToString());
        }

        private void ConvertBlockChildren(IElement parent, StringBuilder output)
        {
            var pendingNewline = false;
            foreach (var node in parent.ChildNodes)
            {
                if (node.NodeType == NodeType.Text)
                {
                    var text = node.TextContent;
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        if (pendingNewline && output.Length > 0)
                        {
                            output.AppendLine();
                            output.AppendLine();
                            pendingNewline = false;
                        }

                        AppendParagraphText(output, text.Trim());
                        output.AppendLine();
                        output.AppendLine();
                    }

                    continue;
                }

                if (node.NodeType == NodeType.Comment)
                {
                    if (pendingNewline && output.Length > 0)
                    {
                        output.AppendLine();
                        output.AppendLine();
                        pendingNewline = false;
                    }

                    output.Append("<!--").Append(node.TextContent).AppendLine("-->");
                    pendingNewline = true;
                    continue;
                }

                if (node.NodeType != NodeType.Element)
                {
                    continue;
                }

                var element = (IElement)node;
                var tag = element.TagName;

                if (TableSectionTags.Contains(tag) || string.Equals(tag, "table", StringComparison.OrdinalIgnoreCase))
                {
                    if (pendingNewline && output.Length > 0)
                    {
                        output.AppendLine();
                        output.AppendLine();
                        pendingNewline = false;
                    }

                    ConvertTable(element, output);
                    pendingNewline = true;
                    continue;
                }

                if (IsHeading(tag))
                {
                    if (pendingNewline && output.Length > 0)
                    {
                        output.AppendLine();
                        output.AppendLine();
                        pendingNewline = false;
                    }

                    var level = tag[1] - '0';
                    output.Append(new string('#', level));
                    output.Append(' ');
                    ConvertInlineChildren(element, output);
                    output.AppendLine();
                    pendingNewline = true;
                    continue;
                }

                if (string.Equals(tag, "p", StringComparison.OrdinalIgnoreCase))
                {
                    if (pendingNewline && output.Length > 0)
                    {
                        output.AppendLine();
                        output.AppendLine();
                        pendingNewline = false;
                    }

                    ConvertInlineChildren(element, output);
                    output.AppendLine();
                    pendingNewline = true;
                    continue;
                }

                if (string.Equals(tag, "blockquote", StringComparison.OrdinalIgnoreCase))
                {
                    if (pendingNewline && output.Length > 0)
                    {
                        output.AppendLine();
                        output.AppendLine();
                        pendingNewline = false;
                    }

                    ConvertBlockquote(element, output);
                    pendingNewline = true;
                    continue;
                }

                if (string.Equals(tag, "pre", StringComparison.OrdinalIgnoreCase))
                {
                    if (pendingNewline && output.Length > 0)
                    {
                        output.AppendLine();
                        output.AppendLine();
                        pendingNewline = false;
                    }

                    ConvertPreformatted(element, output);
                    pendingNewline = true;
                    continue;
                }

                if (string.Equals(tag, "ul", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(tag, "ol", StringComparison.OrdinalIgnoreCase))
                {
                    if (pendingNewline && output.Length > 0)
                    {
                        output.AppendLine();
                        output.AppendLine();
                        pendingNewline = false;
                    }

                    ConvertList(element, output, ordered: string.Equals(tag, "ol", StringComparison.OrdinalIgnoreCase));
                    pendingNewline = true;
                    continue;
                }

                if (string.Equals(tag, "hr", StringComparison.OrdinalIgnoreCase))
                {
                    if (pendingNewline && output.Length > 0)
                    {
                        output.AppendLine();
                        output.AppendLine();
                        pendingNewline = false;
                    }

                    output.AppendLine("---");
                    pendingNewline = true;
                    continue;
                }

                if (string.Equals(tag, "div", StringComparison.OrdinalIgnoreCase))
                {
                    if (element.Attributes.Length > 0)
                    {
                        if (pendingNewline && output.Length > 0)
                        {
                            output.AppendLine();
                            output.AppendLine();
                            pendingNewline = false;
                        }

                        AppendRawHtml(element, output);
                        pendingNewline = true;
                        continue;
                    }

                    if (ElementHasOnlyInlineContent(element))
                    {
                        if (pendingNewline && output.Length > 0)
                        {
                            output.AppendLine();
                            output.AppendLine();
                            pendingNewline = false;
                        }

                        ConvertInlineChildren(element, output);
                        output.AppendLine();
                        pendingNewline = true;
                    }
                    else
                    {
                        ConvertBlockChildren(element, output);
                        pendingNewline = true;
                    }

                    continue;
                }

                if (KnownBlockTags.Contains(tag) || KnownInlineTags.Contains(tag))
                {
                    if (pendingNewline && output.Length > 0)
                    {
                        output.AppendLine();
                        output.AppendLine();
                        pendingNewline = false;
                    }

                    AppendRawHtml(element, output);
                    pendingNewline = true;
                    continue;
                }

                if (pendingNewline && output.Length > 0)
                {
                    output.AppendLine();
                    output.AppendLine();
                    pendingNewline = false;
                }

                AppendRawHtml(element, output);
                pendingNewline = true;
            }
        }

        private void ConvertInlineChildren(IElement parent, StringBuilder output)
        {
            foreach (var node in parent.ChildNodes)
            {
                ConvertInlineNode(node, output);
            }
        }

        private void ConvertInlineNode(INode node, StringBuilder output)
        {
            if (node.NodeType == NodeType.Text)
            {
                AppendInlineText(output, node.TextContent);
                return;
            }

            if (node.NodeType == NodeType.Comment)
            {
                output.Append("<!--").Append(node.TextContent).Append("-->");
                return;
            }

            if (node.NodeType != NodeType.Element)
            {
                return;
            }

            var element = (IElement)node;
            var tag = element.TagName;

            if (string.Equals(tag, "strong", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(tag, "b", StringComparison.OrdinalIgnoreCase))
            {
                output.Append("**");
                ConvertInlineChildren(element, output);
                output.Append("**");
                return;
            }

            if (string.Equals(tag, "em", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(tag, "i", StringComparison.OrdinalIgnoreCase))
            {
                output.Append('*');
                ConvertInlineChildren(element, output);
                output.Append('*');
                return;
            }

            if (string.Equals(tag, "del", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(tag, "s", StringComparison.OrdinalIgnoreCase))
            {
                output.Append("~~");
                ConvertInlineChildren(element, output);
                output.Append("~~");
                return;
            }

            if (string.Equals(tag, "code", StringComparison.OrdinalIgnoreCase))
            {
                output.Append('`');
                output.Append(element.TextContent);
                output.Append('`');
                return;
            }

            if (string.Equals(tag, "a", StringComparison.OrdinalIgnoreCase))
            {
                var href = element.GetAttribute("href") ?? string.Empty;
                output.Append('[');
                ConvertInlineChildren(element, output);
                output.Append("](").Append(href).Append(')');
                return;
            }

            if (string.Equals(tag, "img", StringComparison.OrdinalIgnoreCase))
            {
                var alt = element.GetAttribute("alt") ?? string.Empty;
                var src = element.GetAttribute("src") ?? string.Empty;
                output.Append("![").Append(alt).Append("](").Append(src).Append(')');
                return;
            }

            if (string.Equals(tag, "br", StringComparison.OrdinalIgnoreCase))
            {
                output.AppendLine();
                return;
            }

            if (string.Equals(tag, "input", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (KnownInlineTags.Contains(tag) || KnownBlockTags.Contains(tag))
            {
                AppendRawHtml(element, output);
                return;
            }

            AppendRawHtml(element, output);
        }

        private void ConvertBlockquote(IElement element, StringBuilder output)
        {
            var inner = new StringBuilder();
            ConvertBlockChildren(element, inner);
            var lines = inner.ToString().Replace("\r\n", "\n").Split('\n');
            foreach (var line in lines)
            {
                if (line.Length == 0)
                {
                    output.AppendLine(">");
                    continue;
                }

                output.Append("> ").AppendLine(line);
            }
        }

        private void ConvertPreformatted(IElement element, StringBuilder output)
        {
            var codeElement = element.QuerySelector("code");
            var text = codeElement != null ? codeElement.TextContent : element.TextContent;
            text = text?.Replace("\r\n", "\n") ?? string.Empty;
            if (text.EndsWith("\n", StringComparison.Ordinal))
            {
                text = text.Substring(0, text.Length - 1);
            }

            output.AppendLine("```");
            output.AppendLine(text);
            output.AppendLine("```");
        }

        private void ConvertList(IElement listElement, StringBuilder output, bool ordered)
        {
            var index = 1;
            foreach (var child in listElement.Children)
            {
                if (!string.Equals(child.TagName, "li", StringComparison.OrdinalIgnoreCase))
                {
                    AppendRawHtml(child, output);
                    continue;
                }

                var checkbox = child.QuerySelector("input[type=checkbox]");
                if (checkbox != null)
                {
                    var isChecked = checkbox.HasAttribute("checked");
                    output.Append("- [");
                    output.Append(isChecked ? 'x' : ' ');
                    output.Append("] ");

                    var itemContent = new StringBuilder();
                    foreach (var node in child.ChildNodes)
                    {
                        if (node.NodeType == NodeType.Element &&
                            string.Equals(((IElement)node).TagName, "input", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        ConvertInlineNode(node, itemContent);
                    }

                    output.Append(itemContent.ToString().TrimStart());
                    output.AppendLine();
                    continue;
                }

                if (ordered)
                {
                    output.Append(index).Append(". ");
                    index++;
                }
                else
                {
                    output.Append("- ");
                }

                ConvertListItemContent(child, output);
                output.AppendLine();
            }
        }

        private void ConvertListItemContent(IElement li, StringBuilder output)
        {
            foreach (var node in li.ChildNodes)
            {
                if (node.NodeType == NodeType.Element)
                {
                    var childElement = (IElement)node;
                    if (IsBlockContainer(childElement))
                    {
                        output.AppendLine();
                        ConvertBlockChildren(childElement, output);
                        continue;
                    }
                }

                ConvertInlineNode(node, output);
            }
        }

        private void ConvertTable(IElement tableElement, StringBuilder output)
        {
            IElement table = string.Equals(tableElement.TagName, "table", StringComparison.OrdinalIgnoreCase)
                ? tableElement
                : tableElement.Closest("table");

            if (table == null)
            {
                AppendRawHtml(tableElement, output);
                return;
            }

            var rows = table.QuerySelectorAll("tr");
            if (rows.Length == 0)
            {
                AppendRawHtml(table, output);
                return;
            }

            var rowIndex = 0;
            foreach (var row in rows)
            {
                var cells = row.QuerySelectorAll("th,td");
                if (cells.Length == 0)
                {
                    continue;
                }

                output.Append('|');
                foreach (var cell in cells)
                {
                    var cellText = GetCellText(cell);
                    output.Append(' ').Append(EscapeTableCell(cellText)).Append(" |");
                }

                output.AppendLine();

                if (rowIndex == 0)
                {
                    output.Append('|');
                    for (var i = 0; i < cells.Length; i++)
                    {
                        output.Append(" --- |");
                    }

                    output.AppendLine();
                }

                rowIndex++;
            }
        }

        private static string GetCellText(IElement cell)
        {
            var sb = new StringBuilder();
            foreach (var node in cell.ChildNodes)
            {
                if (node.NodeType == NodeType.Text)
                {
                    sb.Append(node.TextContent);
                }
                else if (node.NodeType == NodeType.Element)
                {
                    sb.Append(((IElement)node).TextContent);
                }
            }

            return sb.ToString().Replace("\r\n", " ").Replace('\n', ' ').Trim();
        }

        private static string EscapeTableCell(string text)
        {
            return text.Replace("|", "\\|", StringComparison.Ordinal);
        }

        private static void AppendInlineText(StringBuilder output, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            output.Append(text.Replace("\r\n", "\n"));
        }

        private static void AppendParagraphText(StringBuilder output, string text)
        {
            output.Append(text);
        }

        private static void AppendRawHtml(IElement element, StringBuilder output)
        {
            output.Append(element.OuterHtml);
            output.AppendLine();
        }

        private static bool IsHeading(string tag)
        {
            if (tag.Length != 2)
            {
                return false;
            }

            var normalized = char.ToLowerInvariant(tag[0]);
            return normalized == 'h' && tag[1] >= '1' && tag[1] <= '6';
        }

        private static bool ElementHasOnlyInlineContent(IElement element)
        {
            foreach (var child in element.ChildNodes)
            {
                if (child.NodeType == NodeType.Text)
                {
                    continue;
                }

                if (child.NodeType == NodeType.Element)
                {
                    var childTag = ((IElement)child).TagName;
                    if (IsBlockContainer((IElement)child) && !IsHeading(childTag))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool IsBlockContainer(IElement element)
        {
            var tag = element.TagName;
            return IsHeading(tag) ||
                   string.Equals(tag, "p", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(tag, "div", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(tag, "blockquote", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(tag, "pre", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(tag, "ul", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(tag, "ol", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(tag, "table", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(tag, "li", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeOutput(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
            {
                return string.Empty;
            }

            var normalized = markdown.Replace("\r\n", "\n").Trim();
            while (normalized.Contains("\n\n\n", StringComparison.Ordinal))
            {
                normalized = normalized.Replace("\n\n\n", "\n\n", StringComparison.Ordinal);
            }

            return normalized;
        }
    }
}
