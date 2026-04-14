// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using mshtml;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.CoreServices
{
    public class HTMLTrimmer
    {
        private enum Visibility
        {
            Whitespace,  // textual whitespace, &nbsp; and <br>
            Invisible,   // <h1></h1>, <p></p>, <strong></strong>, and other tags that could potentially have meaning if they contain visible elements
            Visible      // any tag with attributes, anything that isn't whitespace or invisible
        }

        public static string Trim(string html)
        {
            return Trim(html, false);
        }

        public static string Trim(string html, bool onlyTrimParagraphs)
        {
            Element[] els = Elements(html);

            int pos;
            // First, go backwards over the list, deleting
            // all <br> and whitespace.  Stop as soon as
            // significant content is encountered.
            if (onlyTrimParagraphs)
            {
                pos = 1 + FindCleanupIndexForParagraphTrim(els);
            }
            else
            {
                pos = 1 + FindLastVisibleElementAndRemoveWhitespace(els);
            }

            // pos now points to the index where whitespace cleanup should begin

            // Remove empty pairs of invisible tags, e.g. <b></b>.  Each time
            // a pair is removed, start over, because the removal
            // of an empty pair may create another empty pair, e.g. <p><i></i></p>
            while (FindAndRemoveEmptyTag(pos, els))
            {
            }

            // Remove extra unmatched <p> begin tags.
            for (int i = pos; i < els.Length; i++)
            {
                BeginTag bt = els[i] as BeginTag;
                if (bt != null && bt.NameEquals("p"))
                    els[i] = null;
            }

            // Concatenate all the elements that are left.
            StringBuilder output = new StringBuilder(html.Length);
            foreach (Element el in els)
                if (el != null)
                    output.Append(el.RawText);
            return output.ToString();
        }

        private static int FindCleanupIndexForParagraphTrim(Element[] els)
        {
            int i;
            for (i = els.Length - 1; i >= 0; i--)
            {
                Element el = els[i];

                if (el is Tag)
                {
                    if (!((Tag)el).NameEquals("p"))
                    {
                        return i;
                    }
                }
                else if (el is Text)
                {
                    if (HtmlUtils.UnEscapeEntities(el.RawText, HtmlUtils.UnEscapeMode.NonMarkupText).Trim().Length == 0)
                    {
                        els[i] = null;
                    }
                    else
                    {
                        return i;
                    }
                }
                else
                {
                    return i;
                }
            }
            return i;
        }

        /// <summary>
        /// Returns the index of the last visible element, or -1 if none found.
        /// </summary>
        private static int FindLastVisibleElementAndRemoveWhitespace(Element[] els)
        {
            int i;
            for (i = els.Length - 1; i >= 0; i--)
            {
                Element el = els[i];

                switch (DetermineVisibility(el))
                {
                    case Visibility.Whitespace:
                        // delete and keep going
                        els[i] = null;
                        continue;
                    case Visibility.Invisible:
                        // keep going
                        continue;
                    case Visibility.Visible:
                        // we're done
                        return i;
                }
            }
            return i;
        }

        private static bool FindAndRemoveEmptyTag(int startIndex, Element[] els)
        {
            for (int j = startIndex; j < els.Length - 1; j++)
            {
                BeginTag begin = els[j] as BeginTag;
                if (begin != null)
                {
                    for (int k = j + 1; k < els.Length; k++)
                    {
                        if (els[k] == null)
                            continue; // keep looking
                        if (!(els[k] is EndTag) || !((EndTag)els[k]).NameEquals(begin.Name))
                            break; // no match

                        // It's a match, delete start and end tags and restart
                        els[j] = null;
                        els[k] = null;

                        return true;
                    }
                }
            }
            return false;
        }

        private static Visibility DetermineVisibility(Element el)
        {
            if (el is Tag)
            {
                if (el is BeginTag && ((BeginTag)el).Attributes.Length > 0)
                    return Visibility.Visible;

                switch (((Tag)el).Name.ToUpperInvariant())
                {
                    case "P":
                    case "BLOCKQUOTE":
                    case "H1":
                    case "H2":
                    case "H3":
                    case "H4":
                    case "H5":
                    case "H6":
                    case "TT":
                    case "I":
                    case "B":
                    case "U":
                    case "S":
                    case "STRIKE":
                    case "BIG":
                    case "SMALL":
                    case "EM":
                    case "STRONG":
                    case "DFN":
                    case "CODE":
                    case "SAMP":
                    case "KBD":
                    case "VAR":
                    case "CITE":
                    case "ABBR":
                    case "ACRONYM":
                    case "SUB":
                    case "SUP":
                    case "Q":
                    case "INS":
                    case "DEL":
                        return Visibility.Invisible;
                    case "BR":
                        return Visibility.Whitespace;
                    // anything else is significant
                    default:
                        return Visibility.Visible;
                }
            }
            else if (el is Text)
            {
                string text = HtmlUtils.UnEscapeEntities(el.RawText, HtmlUtils.UnEscapeMode.NonMarkupText);
                if (Regex.IsMatch(text, @"^(\s|&nbsp;)*$", RegexOptions.ExplicitCapture))
                    return Visibility.Whitespace;
                else
                    return Visibility.Visible;

            }

            return Visibility.Visible;
        }

        private static Element[] Elements(string html)
        {
            ArrayList elements = new ArrayList();
            SimpleHtmlParser parser = new SimpleHtmlParser(html);
            Element el;
            while (null != (el = parser.Next()))
                elements.Add(el);
            return (Element[])elements.ToArray(typeof(Element));
        }
    }

    /// <summary>
    /// Removes whitespace from the beginning and/or end of a node.
    /// </summary>
    public class DOMBasedHTMLTrimmer
    {
        public static void TrimWhitespace(IHTMLDocument2 doc, bool trimBegin, bool trimEnd)
        {
            TrimWhitespace((IHTMLDOMNode)doc.body, trimBegin, trimEnd);
        }

        /// <summary>
        /// Returns true if the node itself was pruned.
        /// </summary>
        public static bool TrimWhitespace(IHTMLDOMNode node, bool trimBegin, bool trimEnd)
        {
            if (node.nodeType == HTMLDocumentHelper.HTMLDOMNodeTypes.ElementNode)
            {
                if (trimBegin)
                {
                    // Recurse in a forward depth-first direction,
                    // until either all the children are pruned
                    // or one of them isn't pruned.
                    IHTMLDOMNode child;
                    while (null != (child = node.firstChild))
                    {
                        if (!TrimWhitespace(child, true, false))
                            break;
                    }
                }

                if (trimEnd)
                {
                    // Recurse in a backward depth-first direction.
                    // Again, stop as soon as a child is not pruned.
                    IHTMLDOMNode child;
                    while (null != (child = LastChild(node)))
                    {
                        if (!TrimWhitespace(child, false, true))
                            break;
                    }
                }
            }

            bool prune = IsWhitespaceNode(node);
            if (prune)
                node.parentNode.removeChild(node);
            return prune;
        }

        /// <summary>
        /// Get the last child of a node.  Would be great if there was
        /// a cheaper way to do this.
        /// </summary>
        private static IHTMLDOMNode LastChild(IHTMLDOMNode parent)
        {
            IHTMLDOMNode child = parent.firstChild;
            while (child != null && child.nextSibling != null)
                child = child.nextSibling;
            return child;
        }

        /// <summary>
        /// Determine whether a node can be considered whitespace.
        ///
        /// A node can be whitespace if:
        /// - It is a text node that does not contain non-whitespace chars
        /// - It is a tag that, if empty, is invisible; and does not
        ///   have a class, style, or id
        ///
        /// But not if:
        /// - It is a tag that has children
        /// </summary>
        private static bool IsWhitespaceNode(IHTMLDOMNode node)
        {
            switch (node.nodeType)
            {
                case HTMLDocumentHelper.HTMLDOMNodeTypes.ElementNode:
                    {
                        // No element with descendants should be trimmed.
                        // (If the descendants are all whitespace, they need to
                        // have been pruned by now.)
                        if (node.firstChild != null)
                            return false;

                        switch (node.nodeName.ToUpperInvariant())
                        {
                            // These are all the tags that count as whitespace.
                            // We might want to back off on some of these tags,
                            // and just look for ones that are commonly generated
                            // by our WYSIWYG editor.
                            case "BR":
                            case "P":
                            case "DIV":
                            case "BLOCKQUOTE":
                            case "UL":
                            case "OL":
                            case "TABLE":
                            case "FORM":
                            case "SPAN":
                            case "TT":
                            case "I":
                            case "B":
                            case "U":
                            case "S":
                            case "STRIKE":
                            case "BIG":
                            case "SMALL":
                            case "EM":
                            case "STRONG":
                            case "DFN":
                            case "CODE":
                            case "SAMP":
                            case "KBD":
                            case "VAR":
                            case "CITE":
                            case "ABBR":
                            case "ACRONYM":
                            case "SUB":
                            case "SUP":
                                return !HasInterestingAttributes(node);
                            default:
                                return false;
                        }
                    }
                case HTMLDocumentHelper.HTMLDOMNodeTypes.TextNode:
                    {
                        string s = node.nodeValue.ToString();
                        s = HtmlUtils.UnEscapeEntities(s, HtmlUtils.UnEscapeMode.NonMarkupText);
                        foreach (char c in s)
                        {
                            if (!char.IsWhiteSpace(c))
                                return false;
                        }
                        return true;
                    }
                default:
                    // Any non-text non-element nodes will be kept
                    return false;
            }
        }

        /// <summary>
        /// We don't want to prune elements that have class, style, id, or event attributes.
        /// It seems like if the author went through the trouble to put these attributes
        /// on, we shouldn't trim.  (Maybe we should even keep any element with any attributes?)
        /// </summary>
        private static bool HasInterestingAttributes(IHTMLDOMNode node)
        {
            IHTMLAttributeCollection attrs = node.attributes as IHTMLAttributeCollection;
            if (attrs != null)
            {
                foreach (IHTMLDOMAttribute attr in attrs)
                {
                    if (attr.specified)
                    {
                        string attrName = attr.nodeName as string;
                        if (attrName != null)
                        {
                            attrName = attrName.ToUpperInvariant();
                            switch (attrName)
                            {
                                case "CLASSNAME":
                                case "CLASS":
                                case "STYLE":
                                case "ID":
                                    return true;
                            }
                            return attrName.StartsWith("on", StringComparison.OrdinalIgnoreCase);
                        }
                    }
                }
            }
            return false;
        }
    }
}
