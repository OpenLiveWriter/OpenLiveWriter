// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.CoreServices.HTML;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for TextHelper.
    /// </summary>
    public class TextHelper
    {

        public const int TITLE_LENGTH = 50;

        public enum Units
        {
            Characters,
            Pixels
        }

        private const string Ellipsis = "\u2026";

        /// <summary>
        ///
        /// </summary>
        /// <param name="html"></param>
        /// <param name="maxLength">The maximum number of visible text characters.  HTML tags are not part of this total.  Thus, the string returned may be longer than maxLength.</param>
        /// <returns></returns>
        public static string GetTitleFromHtml(string html, int maxLength)
        {
            return HTMLBalancer.Balance(html = html.Trim(), maxLength, new TextOnlyCostFilter(), true);
        }

        /// <summary>
        /// Gets a shortened title from a longer string
        /// </summary>
        /// <param name="text">The string to shorten</param>
        /// <returns>A shortened string</returns>
        public static string GetTitleFromText(string text, int maxLength, Units units)
        {
            // Get Rid of any whitespace
            text = text.Trim();

            string title = string.Empty;

            switch (units)
            {
                case Units.Characters:
                    {
                        // First, try to grab the first line of text
                        int firstLineEnd = text.IndexOf("\n", StringComparison.OrdinalIgnoreCase);
                        if (firstLineEnd > 0 && firstLineEnd < maxLength)
                        {
                            title = text.Substring(0, firstLineEnd).Trim();
                        }
                        else
                        {
                            // Since we can't use the first line, just grab the first set of characters
                            int len = text.Trim().Length;

                            if (len <= maxLength)
                                title = text.Trim();
                            else
                            {
                                title = text.Substring(0, maxLength);
                                int lastSpace = title.LastIndexOf(" ", StringComparison.OrdinalIgnoreCase);
                                if (lastSpace > 0)
                                    title = text.Substring(0, lastSpace);
                                title = title + Ellipsis;
                            }
                        }
                    }
                    break;
                case Units.Pixels:
                    {
                        // Measure the text.
                        title = text;
                        Size measuredSize = TextRenderer.MeasureText(title, Res.DefaultFont);

                        if (measuredSize.Width < maxLength)
                        {
                            // Add whitespace
                            while (measuredSize.Width < maxLength)
                            {
                                title = title + " ";
                                measuredSize = TextRenderer.MeasureText(title, Res.DefaultFont);
                            }

                        }
                        else
                        {
                            // If it is too small, then cut back
                            while (measuredSize.Width >= maxLength && title.Length > 1)
                            {
                                // Shave off a character
                                title = title.Substring(0, title.Length - 2) + Ellipsis;
                                measuredSize = TextRenderer.MeasureText(title + Ellipsis, Res.DefaultFont);
                            }
                        }

                    }
                    break;
                default:
                    break;
            }

            return title;
        }

        /// <summary>
        /// To display an ampersand in a ribbon gallery item tooltip, escape the special character designation with a double ampersand (&&).
        /// </summary>
        public static string EscapeAmpersands(string text)
        {
            return text.Replace("&", "&&");
        }

        /// <summary>
        /// "\\r\\n" --> "\r\n"
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string UnescapeNewlines(string text)
        {
            if (String.IsNullOrEmpty(text))
                return text;

            return text.Replace("\\r\\n", "\r\n");
        }

        /// <summary>
        /// To display a string as part of a title or tooltip text, remove hotkey traces
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string StripHotkey(string text)
        {
            string hotkey = @"\(&\w\)";
            Regex regexHotKey = new Regex(hotkey);
            text = Regex.Replace(text, hotkey, "");
            return text.Replace("&", "");
        }

        /// <summary>
        /// To display a string in a ribbon command tooltip, strip out any ampersands.
        /// </summary>
        public static string StripAmpersands(string text)
        {
            return text.Replace("&", "");
        }

        /// <summary>
        /// Strips indentation, adds &amp;nbsp; for multiple whitespaces and ensures all lines end with a CRLF.
        /// </summary>
        private static string NormalizeText(string text)
        {
            string normalized = StripIndentation(text);
            normalized = AddNbsp(normalized);
            normalized = LineEndingHelper.Normalize(normalized, LineEnding.CRLF);

            return normalized;
        }

        /// <summary>
        /// Escapes HTML entities and optionally replaces any plain-text HTTP, HTTPS and FTP URLs with a HTML link to
        /// that URL. These must be done at the same time to avoid escaping URL characters.
        /// </summary>
        private static string EscapeHtmlEntitiesAndAddLinks(string text, bool addLinks)
        {
            StringBuilder output = new StringBuilder();
            int pos = 0;

            if (addLinks)
            {
                // Links start with http://, https://, or ftp:// at a word break, and continue until < or > is encountered, or whitespace.
                // If whitespace, then a single "." or "," character may also be removed from the link.
                Match match = Regex.Match(text, @"\b(http://|https://|ftp://).+?(?=[\<\>]|(([.,])?[\s$]))", RegexOptions.IgnoreCase);

                for (; match.Success; match = match.NextMatch())
                {
                    if (match.Index > pos)
                    {
                        output.Append(HtmlUtils.EscapeEntities(text.Substring(pos, match.Index - pos)));
                    }
                    pos = match.Index + match.Length;
                    output.AppendFormat("<a href=\"{0}\">{0}</a>", HtmlUtils.EscapeEntities(match.Value));
                }
            }

            if (pos < text.Length)
            {
                output.Append(HtmlUtils.EscapeEntities(text.Substring(pos)));
            }

            return output.ToString();
        }

        /// <summary>
        /// Gets HTML from a text string without wrapping it in block elements.
        /// </summary>
        /// <param name="text">The plain text to convert into HTML</param>
        /// <returns>The HTML</returns>
        public static string GetHTMLFromText(string text, bool addLinks)
        {
            return GetHTMLFromText(text, addLinks, null);
        }

        /// <summary>
        /// Gets HTML from a text string, wrapping lines in the given default block element.
        /// </summary>
        /// <param name="text">The plain text to convert into HTML</param>
        /// <returns>The HTML</returns>
        public static string GetHTMLFromText(string text, bool addLinks, DefaultBlockElement defaultBlockElement)
        {
            text = EscapeHtmlEntitiesAndAddLinks(text, addLinks);
            text = NormalizeText(text);

            return ConvertTextToHtml(text, defaultBlockElement);
        }

        private static string ConvertTextToHtml(string text, DefaultBlockElement defaultBlockElement)
        {
            if (defaultBlockElement == null)
            {
                return text.Replace("\r\n", "<br />");
            }

            if (!text.Contains("\r\n"))
            {
                return text;
            }

            bool insideBlockElement = false;
            bool blockElementIsEmpty = true;

            // Replace each occurrence of a sequence of non-CRLF characters followed by 0 or more CRLFs with the
            // non-CRLF characters wrapped in an HTML block element:
            //      line1\r\nline2\r\n\r\nline3 => <p>line1<br />line2</p><p>line3</p>
            //      (the <br /> is added because ParagraphDefaultBlockElement.NumberOfNewLinesToReplace == 2)
            string htmlFromText = Regex.Replace(text, @"(?<plainText>[^\r\n]+)?(?<succeedingNewLines>(\r\n){0," + defaultBlockElement.NumberOfNewLinesToReplace + "})",
                delegate (Match match)
                {
                    Group plainText = match.Groups["plainText"];
                    Group succeedingNewLines = match.Groups["succeedingNewLines"];

                    if (plainText.Length == 0 && succeedingNewLines.Length == 0)
                    {
                        return string.Empty;
                    }

                    StringBuilder html = new StringBuilder();

                    if (!insideBlockElement)
                    {
                        html.Append(defaultBlockElement.BeginTag);
                        insideBlockElement = true;
                        blockElementIsEmpty = true;
                    }

                    if (plainText.Length > 0)
                    {
                        html.Append(plainText.Value);
                        blockElementIsEmpty = false;
                    }

                    // The length of succeedingNewLines is the number of characters captured and each newline is two characters: \r\n.
                    int numberOfSucceedingNewLines = succeedingNewLines.Length / 2;

                    if (numberOfSucceedingNewLines == defaultBlockElement.NumberOfNewLinesToReplace)
                    {
                        if (blockElementIsEmpty)
                        {
                            // Manually inflate the element, otherwise it won't be rendered.
                            html.Append("&nbsp;");
                        }

                        html.AppendLine(defaultBlockElement.EndTag);
                        insideBlockElement = false;
                    }
                    else
                    {
                        for (int i = 0; i < numberOfSucceedingNewLines; i++)
                        {
                            html.AppendLine("<br />");
                            blockElementIsEmpty = false;
                        }
                    }

                    return html.ToString();
                });

            return insideBlockElement ? htmlFromText + defaultBlockElement.EndTag : htmlFromText;
        }

        private static string AddNbsp(string orig)
        {
            orig = orig.Replace("\t", "    ");
            orig = Regex.Replace(orig, " (?= )", "&nbsp;");
            orig = Regex.Replace(orig, "^ ", "&nbsp;", RegexOptions.Multiline);
            return orig;
        }

        public static string ConvertNewLinesToBr(string html)
        {
            SimpleHtmlParser parser = new SimpleHtmlParser(html);
            StringBuilder sb = new StringBuilder();
            Element ele = parser.Next();
            while (ele != null)
            {
                if (ele is Text)
                    sb.Append(ele.RawText.Replace("\r\n", "<br/>"));
                else
                    sb.Append(ele.RawText);
                ele = parser.Next();
            }

            return sb.ToString();
        }

        public static string StripIndentation(string strVal)
        {
            if (strVal == null)
                return null;

            if (strVal == String.Empty)
                return String.Empty;

            string[] lines = strVal.Split('\n');

            if (lines.Length <= 1)
                return strVal;

            string common = null;
            foreach (string line in lines)
            {
                if (line.Trim().Length == 0)
                    continue;
                string thisIndent = GetIndent(line);
                if (common == null)
                    common = thisIndent;
                else
                    common = CommonPrefix(thisIndent, common);
            }

            if (string.IsNullOrEmpty(common))
                return strVal;

            StringBuilder sb = new StringBuilder(strVal.Length);
            foreach (string line in lines)
            {
                if (line.StartsWith(common, StringComparison.OrdinalIgnoreCase))
                    sb.Append(line.Substring(common.Length)).Append('\n');
                else
                    sb.Append(line).Append('\n');
            }
            return sb.ToString();
        }

        private static string CommonPrefix(string s1, string s2)
        {
            int i;
            for (i = 0; i < s1.Length && i < s2.Length; i++)
            {
                if (s1[i] != s2[i])
                    return s1.Substring(0, i);
            }
            return s1.Substring(0, i);
        }

        private static string GetIndent(string strVal)
        {
            int pos;
            for (pos = 0; pos < strVal.Length; pos++)
            {
                switch (strVal[pos])
                {
                    case ' ':
                    case '\t':
                        break;
                    default:
                        return strVal.Substring(0, pos);
                }
            }
            return strVal;  // all whitespace
        }

        public static string RtfEscape(string str)
        {
            str = LineEndingHelper.Normalize(str, LineEnding.LF);
            StringBuilder output = new StringBuilder((int)(str.Length * 1.1));
            foreach (char c in str)
            {
                output.Append(RtfEscape(c));
            }
            return output.ToString();
        }

        public static string RtfEscape(char c)
        {
            switch (c)
            {
                case '\\':
                    return @"\\";
                case '{':
                    return @"\{";
                case '}':
                    return @"\}";
                case '\t':
                    return @"\tab ";
                case '\n':
                    return @"\line ";
                case '\r':
                    return @"";
            }

            if (c <= 255)
                return c.ToString();
            else
                return @"\u" + (int)c + "?";
        }

        /// <summary>
        /// Helper method to replace a simple text string within a file (creates or uses
        /// a destination file and leaves the original file untouched)
        /// </summary>
        /// <param name="sourceFile">source file</param>
        /// <param name="sourceText">source text</param>
        /// <param name="destFile">destination file</param>
        /// <param name="destText">destination text</param>
        public static void ReplaceInFile(string sourceFile, string sourceText, string destFile, string destText)
        {
            using (StreamReader source = new StreamReader(new FileStream(sourceFile, FileMode.Open, FileAccess.Read), Encoding.UTF8))
            {
                using (StreamWriter destination = new StreamWriter(new FileStream(destFile, FileMode.Create, FileAccess.ReadWrite), Encoding.UTF8))
                {
                    // setup buffers/index for match detection
                    char[] sourceBuffer = sourceText.ToCharArray();
                    char[] potentialMatch = new char[sourceBuffer.Length];
                    int matchPosition = -1;

                    // setup buffer for reading characters
                    char[] readBuff = new char[1];

                    // read the file character-by-character
                    while (source.Read(readBuff, 0, 1) > 0)
                    {
                        // copy the character into the potential match buffer
                        potentialMatch[++matchPosition] = readBuff[0];
                        int matchedChars = matchPosition + 1;

                        // see if we have a full or partial match
                        if (CompareCharArrays(potentialMatch, sourceBuffer, matchedChars))
                        {
                            // if this is a full match, output destText and reset buffer
                            if (matchedChars == sourceBuffer.Length)
                            {
                                destination.Write(destText);
                                matchPosition = -1;
                            }

                            // otherwise is is a partial match, just keep going trying to find
                            // a potential match....
                        }
                        // char arrays not equal, output contents of potential match to destination
                        // and reset match buffer
                        else
                        {
                            destination.Write(potentialMatch, 0, matchedChars);
                            matchPosition = -1;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Compare the first 'charsToCompare' characters of the two passed character arrays.
        /// </summary>
        /// <param name="chars1">First character array</param>
        /// <param name="chars2">Second character array</param>
        /// <param name="charsToCompare">Number of characters to compare</param>
        /// <returns>true if the first 'n' characters match, else false</returns>
        public static bool CompareCharArrays(char[] chars1, char[] chars2, int charsToCompare)
        {
            // if even one character does not match, return false
            for (int i = 0; i < charsToCompare; i++)
                if (chars1[i] != chars2[i])
                    return false;

            // got through all of the characters, they match
            return true;
        }

        /// <summary>
        /// Helper to format the specified tool tip text.
        /// </summary>
        /// <returns>The formatted tool tip text, or null.</returns>
        public static string FormatTooltipText(string text, int maxLineLength, int maxLines)
        {
            //	Ensure that the text contains something.
            if (text == null || text.Length == 0)
                return null;
            text = text.Trim();
            if (text.Length == 0)
                return null;

            //	Break the text into lines.
            int lines = 0;
            int lineLength = 0;
            bool inWhitespace = false;
            StringBuilder stringBuilder = new StringBuilder();
            foreach (char ch in text)
            {
                if (Char.IsControl(ch) || Char.IsWhiteSpace(ch) || Char.IsSeparator(ch))
                {
                    if (!inWhitespace)
                    {
                        inWhitespace = true;
                        if (lineLength > maxLineLength)
                        {
                            if (++lines > maxLines)
                                break;
                            stringBuilder.Append('\r');
                            lineLength = 0;
                        }
                        else if (stringBuilder.Length != 0)
                        {
                            stringBuilder.Append('\x20');
                            lineLength++;
                        }
                    }
                }
                else
                {
                    if (inWhitespace)
                        inWhitespace = false;
                    stringBuilder.Append(ch);
                    lineLength++;
                }
            }
            return stringBuilder.Length == 0 ? null : stringBuilder.ToString();
        }

        public static string CompactWhiteSpace(string description)
        {
            // compress multiple consecutive whitespace chars into one space
            description = Regex.Replace(description, @"\s+", " ");

            return description;
        }
    }
}
