// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenLiveWriter.HtmlParser.Parser
{
    public class HtmlUtils
    {
        private HtmlUtils()
        {
        }

        public static string NormalizeWhitespace(string html)
        {
            return Regex.Replace(html, @"\s+", " ");
        }

        /// <summary>
        /// Normalizes HTML by ensuring elements with optional end tags have explicit closing tags.
        /// This fixes issues where MSHTML serializes HTML without closing tags for elements like &lt;p&gt;.
        /// </summary>
        /// <param name="html">The HTML string to normalize.</param>
        /// <returns>HTML with explicit closing tags added for elements with optional end tags.</returns>
        public static string NormalizeHtmlClosingTags(string html)
        {
            if (string.IsNullOrEmpty(html))
                return html;

            StringBuilder result = new StringBuilder(html.Length + 100);
            ArrayList openTags = new ArrayList(); // Stack of tag names (optional end tags + containers)
            SimpleHtmlParser parser = new SimpleHtmlParser(html);

            Element el;
            while ((el = parser.Next()) != null)
            {
                if (el is BeginTag)
                {
                    BeginTag beginTag = (BeginTag)el;
                    string tagName = beginTag.Name;

                    // Close any implicitly closed tags before adding this new tag
                    CloseImplicitlyClosedTags(result, openTags, tagName);

                    result.Append(el.RawText);

                    // Track tags with optional end tags and container elements (but not self-closing)
                    if ((HasOptionalEndTag(tagName) || IsContainerElement(tagName)) && !beginTag.Complete)
                    {
                        openTags.Add(tagName);
                    }
                }
                else if (el is EndTag)
                {
                    EndTag endTag = (EndTag)el;
                    CloseTagsUntilMatch(result, openTags, endTag.Name);
                    result.Append(el.RawText);
                }
                else
                {
                    result.Append(el.RawText);
                }
            }

            // Close any remaining open tags at end of input (only optional-end-tag elements)
            for (int i = openTags.Count - 1; i >= 0; i--)
            {
                string tag = (string)openTags[i];
                if (HasOptionalEndTag(tag))
                {
                    result.Append("</").Append(tag).Append(">");
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Returns true if the element has an optional end tag per HTML spec.
        /// </summary>
        private static bool HasOptionalEndTag(string tagName)
        {
            switch (tagName.ToUpper(CultureInfo.InvariantCulture))
            {
                case "P":
                case "LI":
                case "DT":
                case "DD":
                case "TR":
                case "TH":
                case "TD":
                case "THEAD":
                case "TBODY":
                case "TFOOT":
                case "COLGROUP":
                case "OPTION":
                case "OPTGROUP":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns true if the new tag implicitly closes the open tag.
        /// Based on HTML5 optional tag omission rules.
        /// </summary>
        private static bool ImplicitlyCloses(string newTag, string openTag)
        {
            string newUpper = newTag.ToUpper(CultureInfo.InvariantCulture);
            string openUpper = openTag.ToUpper(CultureInfo.InvariantCulture);

            // Block-level elements implicitly close an open P tag per HTML5 spec
            if (openUpper == "P" && IsBlockElement(newUpper))
            {
                return true;
            }

            switch (newUpper)
            {
                case "P":
                    return openUpper == "P";
                case "LI":
                    return openUpper == "LI";
                case "DT":
                case "DD":
                    return openUpper == "DT" || openUpper == "DD";
                case "TR":
                    return openUpper == "TR" || openUpper == "TH" || openUpper == "TD";
                case "TH":
                case "TD":
                    return openUpper == "TH" || openUpper == "TD";
                case "THEAD":
                case "TBODY":
                case "TFOOT":
                    return openUpper == "THEAD" || openUpper == "TBODY" || openUpper == "TFOOT";
                case "OPTION":
                    return openUpper == "OPTION";
                case "OPTGROUP":
                    return openUpper == "OPTGROUP" || openUpper == "OPTION";
                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns true if the element is a block-level element that closes an open P tag.
        /// Per HTML5, a P element's end tag can be omitted if immediately followed by these elements.
        /// </summary>
        private static bool IsBlockElement(string tagNameUpper)
        {
            switch (tagNameUpper)
            {
                case "ADDRESS":
                case "ARTICLE":
                case "ASIDE":
                case "BLOCKQUOTE":
                case "DIV":
                case "DL":
                case "FIELDSET":
                case "FIGCAPTION":
                case "FIGURE":
                case "FOOTER":
                case "FORM":
                case "H1":
                case "H2":
                case "H3":
                case "H4":
                case "H5":
                case "H6":
                case "HEADER":
                case "HR":
                case "MAIN":
                case "NAV":
                case "OL":
                case "P":
                case "PRE":
                case "SECTION":
                case "TABLE":
                case "UL":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns true if the element is a container that closes all inner optional-end-tag elements.
        /// </summary>
        private static bool IsContainerElement(string tagName)
        {
            switch (tagName.ToUpper(CultureInfo.InvariantCulture))
            {
                case "TABLE":
                case "UL":
                case "OL":
                case "DL":
                case "SELECT":
                case "BODY":
                case "HTML":
                case "DIV":
                case "FORM":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Closes tags that are implicitly closed when a new tag opens.
        /// </summary>
        private static void CloseImplicitlyClosedTags(StringBuilder result, ArrayList openTags, string newTagName)
        {
            // Close all matching tags from the top of the stack
            while (openTags.Count > 0)
            {
                string openTag = (string)openTags[openTags.Count - 1];
                if (ImplicitlyCloses(newTagName, openTag))
                {
                    openTags.RemoveAt(openTags.Count - 1);
                    result.Append("</").Append(openTag).Append(">");
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Closes tags until we find a matching open tag for the given end tag.
        /// </summary>
        private static void CloseTagsUntilMatch(StringBuilder result, ArrayList openTags, string endTagName)
        {
            string endUpper = endTagName.ToUpper(CultureInfo.InvariantCulture);

            // Container elements close their inner optional-end-tag elements until the matching container
            if (IsContainerElement(endTagName))
            {
                while (openTags.Count > 0)
                {
                    string openTag = (string)openTags[openTags.Count - 1];
                    string openUpper = openTag.ToUpper(CultureInfo.InvariantCulture);

                    if (openUpper == endUpper)
                    {
                        // Found the matching container - remove it from stack
                        openTags.RemoveAt(openTags.Count - 1);
                        break;
                    }
                    else if (HasOptionalEndTag(openTag))
                    {
                        // Close this optional-end-tag element
                        openTags.RemoveAt(openTags.Count - 1);
                        result.Append("</").Append(openTag).Append(">");
                    }
                    else
                    {
                        // Hit another container - stop here (malformed HTML)
                        break;
                    }
                }
                return;
            }

            // For optional end tag elements, close until we find a match
            if (HasOptionalEndTag(endTagName))
            {
                while (openTags.Count > 0)
                {
                    string openTag = (string)openTags[openTags.Count - 1];
                    string openUpper = openTag.ToUpper(CultureInfo.InvariantCulture);

                    if (openUpper == endUpper)
                    {
                        // Found the match - remove from stack, source end tag will close it
                        openTags.RemoveAt(openTags.Count - 1);
                        break;
                    }
                    else if (HasOptionalEndTag(openTag))
                    {
                        // Close this tag implicitly
                        openTags.RemoveAt(openTags.Count - 1);
                        result.Append("</").Append(openTag).Append(">");
                    }
                    else
                    {
                        // Hit a container element - stop here
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Fixes invalid nested list HTML where MSHTML places nested &lt;ul&gt;/&lt;ol&gt; elements
        /// as siblings of &lt;li&gt; rather than inside the preceding &lt;li&gt;.
        /// For example, converts:
        ///   &lt;ul&gt;&lt;li&gt;item&lt;/li&gt;&lt;ul&gt;&lt;li&gt;nested&lt;/li&gt;&lt;/ul&gt;&lt;/ul&gt;
        /// To:
        ///   &lt;ul&gt;&lt;li&gt;item&lt;ul&gt;&lt;li&gt;nested&lt;/li&gt;&lt;/ul&gt;&lt;/li&gt;&lt;/ul&gt;
        /// </summary>
        public static string FixNestedListHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return html;

            StringBuilder result = new StringBuilder(html.Length + 50);
            SimpleHtmlParser parser = new SimpleHtmlParser(html);
            ArrayList tokenBuffer = new ArrayList();

            // Buffer all tokens first so we can look ahead
            Element el;
            while ((el = parser.Next()) != null)
            {
                tokenBuffer.Add(el);
            }

            // Track whether we're inside a list context using a stack
            // Each entry is the tag name of the list element (UL or OL)
            ArrayList listStack = new ArrayList();
            // Track whether the last meaningful event was a </li> close tag
            // If so, and the next thing is a <ul>/<ol>, we need to reopen that <li>
            bool lastWasLiClose = false;
            string pendingLiCloseTag = null;

            for (int i = 0; i < tokenBuffer.Count; i++)
            {
                Element token = (Element)tokenBuffer[i];

                if (token is BeginTag)
                {
                    BeginTag bt = (BeginTag)token;
                    string nameUpper = bt.Name.ToUpper(CultureInfo.InvariantCulture);

                    if (nameUpper == "UL" || nameUpper == "OL")
                    {
                        if (lastWasLiClose && listStack.Count > 0 && pendingLiCloseTag != null)
                        {
                            // Remove the </li> we already wrote - the nested list should be inside the <li>
                            // We need to remove the trailing </li> from the result
                            string closeLi = "</" + pendingLiCloseTag + ">";
                            string resultStr = result.ToString();
                            int lastIndex = resultStr.LastIndexOf(closeLi, StringComparison.OrdinalIgnoreCase);
                            if (lastIndex >= 0)
                            {
                                result.Remove(lastIndex, closeLi.Length);
                            }
                            // We'll need to re-close the <li> after the nested list closes
                            listStack.Add(nameUpper + "|" + pendingLiCloseTag);
                        }
                        else
                        {
                            listStack.Add(nameUpper);
                        }
                        result.Append(token.RawText);
                        lastWasLiClose = false;
                        pendingLiCloseTag = null;
                    }
                    else if (nameUpper == "LI")
                    {
                        result.Append(token.RawText);
                        lastWasLiClose = false;
                        pendingLiCloseTag = null;
                    }
                    else
                    {
                        result.Append(token.RawText);
                        lastWasLiClose = false;
                        pendingLiCloseTag = null;
                    }
                }
                else if (token is EndTag)
                {
                    EndTag et = (EndTag)token;
                    string nameUpper = et.Name.ToUpper(CultureInfo.InvariantCulture);

                    if (nameUpper == "LI")
                    {
                        result.Append(token.RawText);
                        lastWasLiClose = true;
                        pendingLiCloseTag = et.Name;
                    }
                    else if (nameUpper == "UL" || nameUpper == "OL")
                    {
                        result.Append(token.RawText);

                        // Check if we need to re-close an <li> that was opened for nesting
                        if (listStack.Count > 0)
                        {
                            string stackEntry = (string)listStack[listStack.Count - 1];
                            listStack.RemoveAt(listStack.Count - 1);
                            int pipeIndex = stackEntry.IndexOf('|');
                            if (pipeIndex >= 0)
                            {
                                // This list was nested inside an <li> - re-close the <li>
                                string liTagName = stackEntry.Substring(pipeIndex + 1);
                                result.Append("</").Append(liTagName).Append(">");
                                lastWasLiClose = true;
                                pendingLiCloseTag = liTagName;
                            }
                            else
                            {
                                lastWasLiClose = false;
                                pendingLiCloseTag = null;
                            }
                        }
                        else
                        {
                            lastWasLiClose = false;
                            pendingLiCloseTag = null;
                        }
                    }
                    else
                    {
                        result.Append(token.RawText);
                        lastWasLiClose = false;
                        pendingLiCloseTag = null;
                    }
                }
                else
                {
                    result.Append(token.RawText);
                    // Don't reset lastWasLiClose for whitespace-only text nodes
                    if (token is Text)
                    {
                        string text = token.RawText;
                        if (!string.IsNullOrEmpty(text) && text.Trim().Length > 0)
                        {
                            lastWasLiClose = false;
                            pendingLiCloseTag = null;
                        }
                    }
                }
            }

            return result.ToString();
        }

        public static string HTMLToPlainText(string html)
        {
            return HTMLToPlainText(html, false);
        }

        public static string HTMLToPlainText(string html, bool forIndexing)
        {
            html = HTMLToPlainTextNoTrim(html, forIndexing);

            if (html == null)
                return null;

            // Finally, trim any additional whitespace
            return html.Trim();
        }

        public static string HTMLToPlainTextNoTrim(string html, bool forIndexing)
        {
            if (html == null)
                return null;

            // Clean out any already existing newlines
            html = Regex.Replace(html, @"[\r\n]", " ");

            // Remove the head
            html = Regex.Replace(html, @"<head.*?</head>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Remove any javascript
            html = Regex.Replace(html, @"<script.*?</script>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            // Remove any CSS
            html = Regex.Replace(html, @"<style.*?</style>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // compress whitespace
            html = Regex.Replace(html, @"\s+", " ");

            // remove any smart content
            // because the smart content might have <divs> nested in it, we use regex balancing groups to make sure to parse the nesting correctly.
            //html = Regex.Replace(html, @"<div(\s[^>]*)?id(\s*)?=(\s*)?""scid:([^""]*)?""([^>]*)?>(?>(?!<div|</div>).|<div(?<Depth>)|</div>(?<-Depth>))*(?(Depth)(?!))</div>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            html = Regex.Replace(html, @"<div(\s[^>]*)?id(\s*)?=(\s*)?[""]?scid:([^""]*)?[""]?([^>]*)?>(?>(?!<div|<(/|\\/)div>).|<div(?<Depth>)|<(/|\\/)div>(?<-Depth>))*(?(Depth)(?!))</div>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // turn heading tags into <p>
            html = Regex.Replace(html, @"<(/?)h[1-7](\s[^>]*)?>", "<$1p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            // turn ul/ol tags into <p>
            html = Regex.Replace(html, @"<(/?)[uo]l(\s[^>]*)?>", "<$1p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            // separate <li>s with newlines
            html = Regex.Replace(html, @"(?<!<p(\s[^>]*)?>\s*)<li(\s[^>]*)?>", "\r\n", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // clean out the <p> tags
            // Adjacent (or whitespace-separated) </p><p> tags should be treated as <p>.
            html = Regex.Replace(html, @"</p(\s[^>]*)?>\s*(<p(\s[^>]*)?>)", "<p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            // Whitespace-separated <div>&nbsp;</div> tags should be treated as just one <div></div>.
            html = Regex.Replace(html, @"<p(\s[^>]*)?>\s*&nbsp;\s*</p(\s[^>]*)?>", "<p></p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // clean out the <div> tags
            // Adjacent (or whitespace-separated) <div><div> tags should be treated as just one <div>.
            html = Regex.Replace(html, @"<div(\s[^>]*)?>(\s*<div(\s[^>]*)?>)+", "<div>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            // Adjacent (or whitespace-separated) </div></div> tags should be treated as just one </div>.
            html = Regex.Replace(html, @"</div(\s[^>]*)?>(\s*</div(\s[^>]*)?>)+", "</div>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            // Adjacent (or whitespace-separated) <br></div> tags should be treated as just one </div>.
            html = Regex.Replace(html, @"<br(\s[^>]*)?>\s*</div(\s[^>]*)?>", "</div>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            // Whitespace-separated <div>&nbsp;</div> tags should be treated as just one <div></div>.
            html = Regex.Replace(html, @"<div(\s[^>]*)?>\s*&nbsp;\s*</div(\s[^>]*)?>", "<div></div>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            // Adjacent (or whitespace-separated) </div><div> tags should be treated as <div>.
            html = Regex.Replace(html, @"</div(\s[^>]*)?>\s*<div(\s[^>]*)?>", "<div>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // <p><div> and <div><p> (or corresponding end tags) should all be collapsed into <p>
            html = Regex.Replace(html, @"</?p(\s[^>]*)?>\s*</?div(\s[^>]*)?>", "</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            html = Regex.Replace(html, @"</?div(\s[^>]*)?>\s*</?p(\s[^>]*)?>", "<p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // <p> becomes 2 newlines
            html = Regex.Replace(html, @"</?p(\s[^>]*)?>", "\r\n\r\n", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            // <br> becomes a newline
            html = Regex.Replace(html, @"<br(\s[^>]*)?>", "\r\n", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            // <div> becomes a newline
            html = Regex.Replace(html, @"</?div(\s[^>]*)?>", "\r\n", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            // null characters ("\0") become two newlines (Mail includes one of these at the end of plain-text reply headers)
            html = Regex.Replace(html, @"\x00", "\r\n\r\n");

            // Clean out all the other tags
            html = Regex.Replace(html, @"</?[^>]+>", forIndexing ? " " : string.Empty);

            // Unescape all entities
            html = UnEscapeEntities(html, UnEscapeMode.Default);

            // remove leading whitespace
            html = Regex.Replace(html, @"^[ \t]*(.+?)[ \t]*$", "$1", RegexOptions.Multiline);

            return html;
        }

        public static string EscapeEntity(char c)
        {
            return EntityEscaper.Char(c);
        }

        /// <summary>
        /// In general you can't put named entities directly in XML PCDATA.
        /// Most entities must use numeric instead.
        /// </summary>
        /// <param name="attribute">If true, escape \r and \n to their numeric equivalents</param>
        public static string EscapeEntitiesForXml(string plaintext, bool attribute)
        {
            if (plaintext == null)
                return null;

            StringBuilder output = new StringBuilder();
            foreach (char c in plaintext)
            {
                switch (c)
                {
                    case '"':
                    case '&':
                    case '<':
                    case '>':
                        output.Append(EntityEscaper.Char(c));
                        break;
                    case '\r':
                    case '\n':
                        if (attribute)
                            AppendNumericEntity(c, output);
                        else
                            output.Append(c);
                        break;
                    case '\'':
                        if (attribute)
                            AppendNumericEntity(c, output);
                        else
                            output.Append(c);
                        break;
                    case (char)160:
                        AppendNumericEntity(c, output);
                        break;
                    default:
                        output.Append(c);
                        break;
                }

            }
            return output.ToString();

        }

        private static void AppendNumericEntity(char c, StringBuilder output)
        {
            output.Append('&').Append('#');
            output.Append(((int)c).ToString(CultureInfo.InvariantCulture));
            output.Append(';');
        }

        public static string EscapeEntities(string plaintext)
        {
            if (plaintext == null)
                return null;

            StringBuilder output = new StringBuilder();
            foreach (char c in plaintext)
            {
                output.Append(EntityEscaper.Char(c));
            }
            return output.ToString();
        }

        public static int DecodeEntityReference(string charref)
        {
            // most common case--entity reference
            int charCode = EntityEscaper.Code(charref, false);
            if (charCode != -1)
            {
                return (char)charCode;
            }

            // no?  maybe it's a numeric reference
            if (charref[0] == '#')
            {
                charref = charref.Substring(1);

                // maybe it's a decimal character reference
                if (Regex.IsMatch(charref, @"^[0-9]+$"))
                {
                    try
                    {
                        int decCode = int.Parse(charref, CultureInfo.InvariantCulture);
                        if (decCode < 0xFFFF)
                        {
                            return (char)decCode;
                        }
                    }
                    catch (FormatException) { }  // parsing error
                    catch (OverflowException) { }  // number too big
                }

                // if not, maybe it's a hex character reference
                if (charref[0] == 'x')
                {
                    try
                    {
                        int decCode = int.Parse(charref.Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        if (decCode < 0xFFFF)
                        {
                            return (char)decCode;
                        }
                    }
                    catch (FormatException) { }  // parsing error
                    catch (OverflowException) { }  // number too big
                }
            }

            return -1;
        }

        [Obsolete("Use overload with UnEscapeMode")]
        public static string UnEscapeEntities(string html)
        {
            return UnEscapeEntities(html, UnEscapeMode.Default);
        }

        public enum UnEscapeMode { Default, Attribute, NonMarkupText = Default }

        /// <summary>
        /// The unsafeForAttributesMode tells the method to perform more aggressive
        /// matching of "basic" entities, like IE does on non-markup HTML text.
        /// However we can't do this kind of matching for attributes, since it
        /// breaks URLs.  When in doubt, use false.
        ///
        /// Example:
        ///
        /// UnEscapeEntities("&pounda", true) => "£a"
        /// UnEscapeEntities("&pounda", false) => "&pounda"
        /// </summary>
        public static string UnEscapeEntities(string html, UnEscapeMode unEscapeMode)
        {
            if (html == null)
                return null;

            StringBuilder output = new StringBuilder(html.Length);
            int len = html.Length;
            for (int i = 0; i < len; i++)
            {
                char c0 = html[i];
                if (c0 == '&')
                {
                    if (i + 1 < len)
                    {
                        char c1 = html[i + 1];
                        switch (c1)
                        {
                            case '#':
                                {
                                    if (i + 2 < len)
                                    {
                                        char c2 = html[i + 2];
                                        switch (c2)
                                        {
                                            case 'x':
                                            case 'X':
                                                {
                                                    // do hexadecimal match

                                                    bool semicolonTerminated = false;
                                                    int charVal = 0;
                                                    int j;
                                                    for (j = i + 3; j < len; j++)
                                                    {
                                                        int hexVal = ToHexValue(html[j]);
                                                        if (hexVal == -1)
                                                        {
                                                            // skip one more char if currently on semicolon
                                                            if (html[j] == ';')
                                                                semicolonTerminated = true;
                                                            break;
                                                        }
                                                        charVal *= 16;
                                                        charVal += hexVal;
                                                    }
                                                    if (semicolonTerminated && charVal != 0)
                                                    {
                                                        i = j;
                                                        output.Append((char)charVal);
                                                        continue;
                                                    }
                                                    // if total is 0, continue
                                                    break;
                                                }
                                            case '0':
                                            case '1':
                                            case '2':
                                            case '3':
                                            case '4':
                                            case '5':
                                            case '6':
                                            case '7':
                                            case '8':
                                            case '9':
                                                {
                                                    // do decimal match

                                                    int charVal = 0;
                                                    int j;
                                                    for (j = i + 2; j < len; j++)
                                                    {
                                                        char c = html[j];
                                                        if (c < '0' || c > '9')
                                                        {
                                                            if (c == ';')
                                                                ++j;
                                                            break;
                                                        }

                                                        int cVal = c - '0';
                                                        charVal *= 10;
                                                        charVal += cVal;
                                                    }
                                                    if (charVal != 0)
                                                    {
                                                        i = j - 1;
                                                        output.Append((char)charVal);
                                                        continue;
                                                    }
                                                    // if total is 0, continue
                                                    break;
                                                }
                                        }
                                    }
                                    break;
                                }
                            default:
                                {
                                    int j;
                                    int end = Math.Min(len, i + 12);
                                    for (j = i + 1; j < end; j++)
                                    {
                                        char c = html[j];
                                        if (c == ';' || (!(c >= 'a' && c <= 'z') && !(c >= 'A' && c <= 'Z') && !(c >= '0' && c <= '9')))
                                        {
                                            break;
                                        }
                                    }

                                    string entityRef = html.Substring(i + 1, j - (i + 1));

                                    if (unEscapeMode != UnEscapeMode.Attribute)
                                    {
                                        // k = number of characters in entityRef that we are using
                                        int k, code = -1;
                                        for (k = 1; k < entityRef.Length; k++)
                                        {
                                            if (-1 != (code = EntityEscaper.Code(entityRef.Substring(0, k), true)))
                                                break;
                                        }

                                        if (code == -1)
                                        {
                                            code = EntityEscaper.Code(entityRef, false);
                                        }

                                        if (code != -1)
                                        {
                                            output.Append((char)code);
                                            i += 1 + k;
                                            if (i < end && html[i] == ';')
                                                ++i;
                                            --i;
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        int code = EntityEscaper.Code(entityRef, false);
                                        if (code != -1)
                                        {
                                            output.Append((char)code);
                                            i += 1 + entityRef.Length;
                                            if (i < end && html[i] == ';')
                                                ++i;
                                            --i;
                                            continue;
                                        }
                                    }

                                    break;
                                }
                        }
                    }
                }
                output.Append(c0);
            }
            return output.ToString();
        }

        private static int ToHexValue(char c)
        {
            return
                (c >= '0' && c <= '9') ? c - '0' :
                (c >= 'A' && c <= 'F') ? c - 'A' + 10 :
                (c >= 'a' && c <= 'f') ? c - 'a' + 10 :
                -1;
        }

        /// <summary>
        /// Removes all &nbsp;
        /// </summary>
        public static string TidyNbsps(string html)
        {
            // watch out for special case: <p>&nbsp;</p>, <td>&nbsp;</td>, etc.
            if (html == "&nbsp;")
                return html;

            //return Regex.Replace(html, @"(?<!(\s|&nbsp;))&nbsp;(?!(\s|&nbsp;))", " ");
            return Regex.Replace(
                html,
                @"(&nbsp;|(?>\s+))*&nbsp;(&nbsp;|(?>\s+))*",
                new MatchEvaluator(new TidyNbspsHelper(html).Evaluator)
                );
        }

        private class TidyNbspsHelper
        {
            private string _html;

            public TidyNbspsHelper(string html)
            {
                _html = html;
            }

            public string Evaluator(Match match)
            {
                int count = match.Groups[1].Captures.Count + match.Groups[2].Captures.Count + 1;

                if (count == 1) // special case for standalone &nbsp;
                {
                    // watch out for special case: <p>&nbsp;</p>, <td>&nbsp;</td>, etc.
                    if (match.Index > 0 && _html[match.Index - 1] == '>'
                        && match.Index + match.Length < _html.Length - 1 && _html[match.Index + match.Length] == '<')
                    {
                        return "&nbsp;";
                    }
                    else
                    {
                        return " ";
                    }
                }

                int strLen = ("&nbsp;".Length * (count - 1)) + 1;
                StringBuilder sb = new StringBuilder(strLen);
                for (int i = 0; i < count - 1; i++)
                    sb.Append("&nbsp;");
                sb.Append(" ");
                return sb.ToString();
            }
        }
    }

    /// <summary>
    /// Escapes all character entity references from HTML 4.01 spec.
    /// Data is parsed from:
    /// http://www.w3.org/TR/REC-html40/sgml/entities.html
    /// </summary>
    public class EntityEscaper
    {
        private readonly static Dictionary<string, int> basicCodes;
        private readonly static Dictionary<string, int> codes;
        private readonly static Dictionary<int, string> rBasicCodes;
        private readonly static Dictionary<int, string> rcodes;

        static EntityEscaper()
        {
            basicCodes = new Dictionary<string, int>((int)(96 * 1.3));
            #region ISO 8859-1 characters
            basicCodes.Add("nbsp", 160);
            basicCodes.Add("iexcl", 161);
            basicCodes.Add("cent", 162);
            basicCodes.Add("pound", 163);
            basicCodes.Add("curren", 164);
            basicCodes.Add("yen", 165);
            basicCodes.Add("brvbar", 166);
            basicCodes.Add("sect", 167);
            basicCodes.Add("uml", 168);
            basicCodes.Add("copy", 169);
            basicCodes.Add("ordf", 170);
            basicCodes.Add("laquo", 171);
            basicCodes.Add("not", 172);
            basicCodes.Add("shy", 173);
            basicCodes.Add("reg", 174);
            basicCodes.Add("macr", 175);
            basicCodes.Add("deg", 176);
            basicCodes.Add("plusmn", 177);
            basicCodes.Add("sup2", 178);
            basicCodes.Add("sup3", 179);
            basicCodes.Add("acute", 180);
            basicCodes.Add("micro", 181);
            basicCodes.Add("para", 182);
            basicCodes.Add("middot", 183);
            basicCodes.Add("cedil", 184);
            basicCodes.Add("sup1", 185);
            basicCodes.Add("ordm", 186);
            basicCodes.Add("raquo", 187);
            basicCodes.Add("frac14", 188);
            basicCodes.Add("frac12", 189);
            basicCodes.Add("frac34", 190);
            basicCodes.Add("iquest", 191);
            basicCodes.Add("Agrave", 192);
            basicCodes.Add("Aacute", 193);
            basicCodes.Add("Acirc", 194);
            basicCodes.Add("Atilde", 195);
            basicCodes.Add("Auml", 196);
            basicCodes.Add("Aring", 197);
            basicCodes.Add("AElig", 198);
            basicCodes.Add("Ccedil", 199);
            basicCodes.Add("Egrave", 200);
            basicCodes.Add("Eacute", 201);
            basicCodes.Add("Ecirc", 202);
            basicCodes.Add("Euml", 203);
            basicCodes.Add("Igrave", 204);
            basicCodes.Add("Iacute", 205);
            basicCodes.Add("Icirc", 206);
            basicCodes.Add("Iuml", 207);
            basicCodes.Add("ETH", 208);
            basicCodes.Add("Ntilde", 209);
            basicCodes.Add("Ograve", 210);
            basicCodes.Add("Oacute", 211);
            basicCodes.Add("Ocirc", 212);
            basicCodes.Add("Otilde", 213);
            basicCodes.Add("Ouml", 214);
            basicCodes.Add("times", 215);
            basicCodes.Add("Oslash", 216);
            basicCodes.Add("Ugrave", 217);
            basicCodes.Add("Uacute", 218);
            basicCodes.Add("Ucirc", 219);
            basicCodes.Add("Uuml", 220);
            basicCodes.Add("Yacute", 221);
            basicCodes.Add("THORN", 222);
            basicCodes.Add("szlig", 223);
            basicCodes.Add("agrave", 224);
            basicCodes.Add("aacute", 225);
            basicCodes.Add("acirc", 226);
            basicCodes.Add("atilde", 227);
            basicCodes.Add("auml", 228);
            basicCodes.Add("aring", 229);
            basicCodes.Add("aelig", 230);
            basicCodes.Add("ccedil", 231);
            basicCodes.Add("egrave", 232);
            basicCodes.Add("eacute", 233);
            basicCodes.Add("ecirc", 234);
            basicCodes.Add("euml", 235);
            basicCodes.Add("igrave", 236);
            basicCodes.Add("iacute", 237);
            basicCodes.Add("icirc", 238);
            basicCodes.Add("iuml", 239);
            basicCodes.Add("eth", 240);
            basicCodes.Add("ntilde", 241);
            basicCodes.Add("ograve", 242);
            basicCodes.Add("oacute", 243);
            basicCodes.Add("ocirc", 244);
            basicCodes.Add("otilde", 245);
            basicCodes.Add("ouml", 246);
            basicCodes.Add("divide", 247);
            basicCodes.Add("oslash", 248);
            basicCodes.Add("ugrave", 249);
            basicCodes.Add("uacute", 250);
            basicCodes.Add("ucirc", 251);
            basicCodes.Add("uuml", 252);
            basicCodes.Add("yacute", 253);
            basicCodes.Add("thorn", 254);
            basicCodes.Add("yuml", 255);
            #endregion

            codes = new Dictionary<string, int>(basicCodes);

            #region Symbols, mathematical symbols, and Greek letters
            codes.Add("fnof", 402);
            codes.Add("Alpha", 913);
            codes.Add("Beta", 914);
            codes.Add("Gamma", 915);
            codes.Add("Delta", 916);
            codes.Add("Epsilon", 917);
            codes.Add("Zeta", 918);
            codes.Add("Eta", 919);
            codes.Add("Theta", 920);
            codes.Add("Iota", 921);
            codes.Add("Kappa", 922);
            codes.Add("Lambda", 923);
            codes.Add("Mu", 924);
            codes.Add("Nu", 925);
            codes.Add("Xi", 926);
            codes.Add("Omicron", 927);
            codes.Add("Pi", 928);
            codes.Add("Rho", 929);
            codes.Add("Sigma", 931);
            codes.Add("Tau", 932);
            codes.Add("Upsilon", 933);
            codes.Add("Phi", 934);
            codes.Add("Chi", 935);
            codes.Add("Psi", 936);
            codes.Add("Omega", 937);
            codes.Add("alpha", 945);
            codes.Add("beta", 946);
            codes.Add("gamma", 947);
            codes.Add("delta", 948);
            codes.Add("epsilon", 949);
            codes.Add("zeta", 950);
            codes.Add("eta", 951);
            codes.Add("theta", 952);
            codes.Add("iota", 953);
            codes.Add("kappa", 954);
            codes.Add("lambda", 955);
            codes.Add("mu", 956);
            codes.Add("nu", 957);
            codes.Add("xi", 958);
            codes.Add("omicron", 959);
            codes.Add("pi", 960);
            codes.Add("rho", 961);
            codes.Add("sigmaf", 962);
            codes.Add("sigma", 963);
            codes.Add("tau", 964);
            codes.Add("upsilon", 965);
            codes.Add("phi", 966);
            codes.Add("chi", 967);
            codes.Add("psi", 968);
            codes.Add("omega", 969);
            codes.Add("thetasym", 977);
            codes.Add("upsih", 978);
            codes.Add("piv", 982);
            codes.Add("bull", 8226);
            codes.Add("hellip", 8230);
            codes.Add("prime", 8242);
            codes.Add("Prime", 8243);
            codes.Add("oline", 8254);
            codes.Add("frasl", 8260);
            codes.Add("weierp", 8472);
            codes.Add("image", 8465);
            codes.Add("real", 8476);
            codes.Add("trade", 8482);
            codes.Add("alefsym", 8501);
            codes.Add("larr", 8592);
            codes.Add("uarr", 8593);
            codes.Add("rarr", 8594);
            codes.Add("darr", 8595);
            codes.Add("harr", 8596);
            codes.Add("crarr", 8629);
            codes.Add("lArr", 8656);
            codes.Add("uArr", 8657);
            codes.Add("rArr", 8658);
            codes.Add("dArr", 8659);
            codes.Add("hArr", 8660);
            codes.Add("forall", 8704);
            codes.Add("part", 8706);
            codes.Add("exist", 8707);
            codes.Add("empty", 8709);
            codes.Add("nabla", 8711);
            codes.Add("isin", 8712);
            codes.Add("notin", 8713);
            codes.Add("ni", 8715);
            codes.Add("prod", 8719);
            codes.Add("sum", 8721);
            codes.Add("minus", 8722);
            codes.Add("lowast", 8727);
            codes.Add("radic", 8730);
            codes.Add("prop", 8733);
            codes.Add("infin", 8734);
            codes.Add("ang", 8736);
            codes.Add("and", 8743);
            codes.Add("or", 8744);
            codes.Add("cap", 8745);
            codes.Add("cup", 8746);
            codes.Add("int", 8747);
            codes.Add("there4", 8756);
            codes.Add("sim", 8764);
            codes.Add("cong", 8773);
            codes.Add("asymp", 8776);
            codes.Add("ne", 8800);
            codes.Add("equiv", 8801);
            codes.Add("le", 8804);
            codes.Add("ge", 8805);
            codes.Add("sub", 8834);
            codes.Add("sup", 8835);
            codes.Add("nsub", 8836);
            codes.Add("sube", 8838);
            codes.Add("supe", 8839);
            codes.Add("oplus", 8853);
            codes.Add("otimes", 8855);
            codes.Add("perp", 8869);
            codes.Add("sdot", 8901);
            codes.Add("lceil", 8968);
            codes.Add("rceil", 8969);
            codes.Add("lfloor", 8970);
            codes.Add("rfloor", 8971);
            codes.Add("lang", 9001);
            codes.Add("rang", 9002);
            codes.Add("loz", 9674);
            codes.Add("spades", 9824);
            codes.Add("clubs", 9827);
            codes.Add("hearts", 9829);
            codes.Add("diams", 9830);
            #endregion
            #region Markup-significant and internationalization characters
            codes.Add("quot", 34);
            codes.Add("amp", 38);
            codes.Add("lt", 60);
            codes.Add("gt", 62);
            codes.Add("OElig", 338);
            codes.Add("oelig", 339);
            codes.Add("Scaron", 352);
            codes.Add("scaron", 353);
            codes.Add("Yuml", 376);
            codes.Add("circ", 710);
            codes.Add("tilde", 732);
            codes.Add("ensp", 8194);
            codes.Add("emsp", 8195);
            codes.Add("thinsp", 8201);
            codes.Add("zwnj", 8204);
            codes.Add("zwj", 8205);
            codes.Add("lrm", 8206);
            codes.Add("rlm", 8207);
            codes.Add("ndash", 8211);
            codes.Add("mdash", 8212);
            codes.Add("lsquo", 8216);
            codes.Add("rsquo", 8217);
            codes.Add("sbquo", 8218);
            codes.Add("ldquo", 8220);
            codes.Add("rdquo", 8221);
            codes.Add("bdquo", 8222);
            codes.Add("dagger", 8224);
            codes.Add("Dagger", 8225);
            codes.Add("permil", 8240);
            codes.Add("lsaquo", 8249);
            codes.Add("rsaquo", 8250);
            codes.Add("euro", 8364);
            #endregion

            PopulateReverse(codes, ref rcodes);
            PopulateReverse(basicCodes, ref rBasicCodes);
        }

        private static void PopulateReverse(Dictionary<string, int> fwd, ref Dictionary<int, string> rev)
        {
            rev = new Dictionary<int, string>((int)(fwd.Count * 1.3));
            foreach (KeyValuePair<string, int> entry in fwd)
            {
                rev[(char)entry.Value] = entry.Key;
            }
        }

        /// <summary>
        /// Returns -1 if not found
        /// </summary>
        public static int Code(string name, bool unterminated)
        {
            Dictionary<string, int> codesToUse = unterminated ? basicCodes : codes;
            int retVal;
            if (codesToUse.TryGetValue(name, out retVal))
                return retVal;
            else
                return -1;
        }

        public static bool HasChar(char c)
        {
            return rcodes.ContainsKey(c);
        }

        public static string Char(char c)
        {
            if (rcodes.ContainsKey(c))
                return "&" + rcodes[c] + ";";
            else
                return c.ToString();
        }
    }

}
