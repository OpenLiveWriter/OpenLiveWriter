// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.BlogClient.Clients
{
    /// <summary>
    /// Summary description for SimpleTextLineFormatter.
    /// </summary>
    [BlogPostContentFilter("LineBreak2PBR")]
    internal class LineBreak2PBRInputFormatter : IBlogPostContentFilter
    {
        public LineBreak2PBRInputFormatter()
        {
        }

        public string OpenFilter(string content)
        {
            return ReplaceLineFormattedBreaks(content);
        }

        public string PublishFilter(string content)
        {
            return content;
        }

        private static Regex usesHtmlLineBreaks = new Regex(@"<(p|br)(\s|/?>)", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
        static private string ReplaceLineFormattedBreaks(string html)
        {
            if (usesHtmlLineBreaks.IsMatch(html))
                return html;

            StringBuilder sb = new StringBuilder();
            StringReader reader = new StringReader(html);
            StringBuilder pBuilder = new StringBuilder();
            bool paragraphsAdded = false;
            while (ReadToParagraphEnd(reader, pBuilder))
            {
                paragraphsAdded = true;
                sb.AppendFormat("<p>{0}</p>", pBuilder.ToString());
                pBuilder.Length = 0;
            }
            if (pBuilder.Length > 0)
            {
                if (paragraphsAdded) //only wrap the last paragraph in <p> if other paragraphs where present in the post.
                    sb.AppendFormat("<p>{0}</p>", pBuilder.ToString());
                else
                    sb.Append(pBuilder.ToString());
            }

            string newHtml = sb.ToString();
            return newHtml;
        }

        static private bool ReadToParagraphEnd(StringReader reader, StringBuilder paragraphBuilder)
        {
            string line = reader.ReadLine();
            int pendingLineBreaks = 0;
            while (line != null)
            {
                if (pendingLineBreaks == 1 && line == String.Empty)
                {
                    return true;
                }
                else if (pendingLineBreaks == 1 && line != String.Empty)
                {
                    paragraphBuilder.Append("<br>");
                    pendingLineBreaks = 0;
                }
                paragraphBuilder.Append(line);

                pendingLineBreaks++;
                line = reader.ReadLine();
            }
            return false;
        }
    }

    /// <summary>
    /// Summary description for SimpleTextLineFormatter.
    /// </summary>
    [BlogPostContentFilter("LineBreak2BR")]
    internal class LineBreak2BRInputFormatter : IBlogPostContentFilter
    {
        public LineBreak2BRInputFormatter()
        {
        }

        public string OpenFilter(string content)
        {
            return ReplaceLineFormattedBreaks(content);
        }

        public string PublishFilter(string content)
        {
            return content;
        }

        private static Regex usesHtmlLineBreaks = new Regex(@"<(br)(\s|/?>)", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
        static private string ReplaceLineFormattedBreaks(string html)
        {
            if (usesHtmlLineBreaks.IsMatch(html))
                return html;

            StringBuilder sb = new StringBuilder();
            StringReader reader = new StringReader(html);
            string line = reader.ReadLine();
            while (line != null)
            {
                sb.Append(line);
                sb.Append("<br>");
                line = reader.ReadLine();
            }
            string newHtml = sb.ToString();
            return newHtml;
        }
    }

    [BlogPostContentFilter("WordPress")]
    public class WordPressInputFormatter : IBlogPostContentFilter
    {
        public string OpenFilter(string c)
        {
            // Find the script/style/pre regions, and don't transform those.
            // Everything else goes through DoBreakFormatting.

            StringBuilder result = new StringBuilder();
            int pos = 0;
            for (Match m = Regex.Match(c, @"<(pre|script|style)\b.*?<\/\1>", RegexOptions.Singleline);
                 m.Success;
                 m = m.NextMatch())
            {
                result.Append(DoBreakFormatting(c.Substring(pos, m.Index - pos))).Append("\r\n");
                pos = m.Index + m.Length;
                result.Append(m.Value).Append("\r\n");
            }

            if (pos < c.Length)
                result.Append(DoBreakFormatting(c.Substring(pos)));

            return result.ToString();
        }

        private string DoBreakFormatting(string c)
        {
            const string blocks = @"(?:address|blockquote|caption|colgroup|dd|div|dl|dt|embed|form|h1|h2|h3|h4|h5|h6|li|math|object|ol|p|param|table|tbody|td|tfoot|th|thead|tr|ul)(?=\s|>)";

            // Normalize hard returns
            Gsub(ref c, @"\r\n", "\n");
            // Normalize <br>
            Gsub(ref c, @"<br\s*\/?>", "\n");
            // Normalize <p> and </p>
            Gsub(ref c, @"<\/?p>", "\n\n");
            // Insert \n\n before each block start tag
            Gsub(ref c, @"<" + blocks, "\n\n$&");
            // Insert \n\n after each block end tag
            Gsub(ref c, @"<\/" + blocks + ">", "$&\n\n");
            // Coalesce 3 or more hard returns into one
            Gsub(ref c, @"(\s*\n){3,}\s*", "\n\n");

            // Now split the string into blocks, which are now delimited
            // by \n\n. Some blocks will be enclosed by block tags, which
            // we generally leave alone. Blocks that are not enclosed by
            // block tags will generally have <p>...</p> added to them.

            string[] chunks = StringHelper.Split(c, "\n\n");

            for (int i = 0; i < chunks.Length; i++)
            {
                string chunk = chunks[i];

                if (!Regex.IsMatch(chunk, @"^<\/?" + blocks + "[^>]*>$"))
                {
                    // Special case for blockquote. Blockquotes are the only blocks
                    // as far as I can tell that will wrap their contents in <p> if
                    // they don't already immediately contain a block.
                    Gsub(ref chunk, @"^<blockquote(?:\s[^>]*)?>(?!$)", "$&<p>");
                    Gsub(ref chunk, @"(?<!^)<\/blockquote>$", "</p>$&");

                    // If this chunk doesn't start with a block, add a <p>
                    if (!Regex.IsMatch(chunk, @"^<" + blocks)) //&& !Regex.IsMatch(chunk, @"^<\/" + blocks + ">"))
                        chunk = "<p>" + chunk;

                    // If this chunk starts with a <p> tag--either because
                    // it always did (like <p class="foo">, which doesn't get
                    // dropped either by WordPress or by our regexs above), or
                    // because we added one just now--we want to end it with
                    // a </p> if necessary.
                    if (Regex.IsMatch(chunk, @"<p(?:\s|>)") && !Regex.IsMatch(chunk, @"</p>"))
                    {
                        Match m = Regex.Match(chunk, @"<\/" + blocks + ">$");
                        if (m.Success)
                            chunk = chunk.Substring(0, m.Index) + "</p>" + chunk.Substring(m.Index);
                        else
                            chunk = chunk + "</p>";
                    }

                    // Convert all remaining hard returns to <br />
                    Gsub(ref chunk, @"\n", "<br />\r\n");
                }

                chunks[i] = chunk;
            }

            // Put the blocks back together before returning.
            return StringHelper.Join(chunks, "\r\n");
        }

        private void Gsub(ref string val, string pattern, string replacement)
        {
            val = Regex.Replace(val, pattern, replacement);
        }

        public string PublishFilter(string content)
        {
            return content;
        }
    }
}
