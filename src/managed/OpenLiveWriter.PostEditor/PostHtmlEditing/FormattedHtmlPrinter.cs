// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using mshtml;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Mshtml;
using System.Diagnostics;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Utility for pretty printing and range of HTML content from the DOM.
    /// </summary>
    public class FormattedHtmlPrinter
    {
        private FormattedHtmlPrinter()
        {
        }

        public static String ToFormattedHtml(MshtmlMarkupServices markupServices, MarkupRange bounds)
        {
            StringBuilder sb = new StringBuilder();
            HtmlWriter xmlWriter = new HtmlWriter(sb);
            PrintHtml(xmlWriter, markupServices, bounds);
            return sb.ToString();
        }

        private static void PrintHtml(HtmlWriter writer, MshtmlMarkupServices MarkupServices, MarkupRange bounds)
        {
            //create a range to span a single position while walking the doc
            MarkupRange range = MarkupServices.CreateMarkupRange();
            range.Start.MoveToPointer(bounds.Start);
            range.End.MoveToPointer(bounds.Start);

            //create a context that can be reused while walking the document.
            MarkupContext context = new MarkupContext();

            //move the range.End to the right and print out each element along the way
            range.End.Right(true, context);
            while (context.Context != _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_None && range.Start.IsLeftOf(bounds.End))
            {
                string text = null;
                if (context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_Text)
                {
                    //if this is a text context, then get the text that is between the start and end points.
                    text = range.HtmlText;

                    //the range.HtmlText operation sometimes returns the outer tags for a text node,
                    //so we need to strip the tags.
                    //FIXME: if the Right/Left operations returned the available text value, this wouldn't be necessary.
                    if (text != null)
                        text = StripSurroundingTags(text);
                }
                else if (context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_ExitScope)
                {
                    string htmlText = range.HtmlText;
                    if (context.Element.innerHTML == null && htmlText != null && htmlText.IndexOf("&nbsp;") != -1)
                    {
                        //HACK: Under these conditions, there was a was an invisible NBSP char in the
                        //document that is not detectable by walking through the document with MarkupServices.
                        //So, we force the text of the element to be the &nbsp; char to ensure that the
                        //whitespace that was visible in the editor is visible in the final document.
                        text = "&nbsp;";
                    }
                }

                //print the context.
                printContext(writer, context, text, range);

                //move the start element to the spot where the end currently is so tht there is
                //only ever a single difference in position
                range.Start.MoveToPointer(range.End);

                //move the end to the next position
                range.End.Right(true, context);
            }
        }

        private static string trimHtmlText(string html)
        {
            return Regex.Replace(html, @"\s+", " ");
        }

        /// <summary>
        /// Utility for printing the correct XHTML for a given MarkupContext.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="context"></param>
        /// <param name="text"></param>
        private static void printContext(HtmlWriter writer, MarkupContext context, string text, MarkupRange range)
        {
            switch (context.Context)
            {
                case _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope:
                    printElementStart(writer, context.Element);
                    if (HtmlLinebreakStripper.IsPreserveWhitespaceTag(context.Element.tagName))
                    {
                        // <pre> was losing whitespace using the normal markup pointer traversal method

                        writer.WriteString(BalanceHtml(context.Element.innerHTML));
                        printElementEnd(writer, context.Element);
                        range.End.MoveAdjacentToElement(context.Element, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
                        break;
                    }
                    else
                    {
                        if (text != null)
                            writer.WriteString(trimHtmlText(text));
                        break;
                    }
                case _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_ExitScope:
                    if (text != null)
                        writer.WriteString(trimHtmlText(text));
                    printElementEnd(writer, context.Element);
                    break;
                case _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_None:
                    break;
                case _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_NoScope:
                    if (context.Element is IHTMLCommentElement
                       || context.Element is IHTMLUnknownElement)
                    {
                        //bugfix: 1777 - comments should just be inserted raw.
                        string html = context.Element.outerHTML;
                        // bugfix: 534222 - embed tag markup generation issues
                        if (html != null && html.ToUpper(CultureInfo.InvariantCulture) != "</EMBED>")
                            writer.WriteString(html);
                    }
                    else
                    {
                        printElementStart(writer, context.Element);
                        if (text == null && context.Element.innerHTML != null)
                        {
                            //Avoid MSHTML bug: in some cases (like title or script elements), MSHTML improperly
                            //reports a tag as being NoScope, even through it clearly has a start and end tag with
                            //text in between. To cover this case, we look for text in a noscope element, and add
                            //it to the XML stream if it is detected.
                            writer.WriteString(context.Element.innerHTML);
                        }
                        printElementEnd(writer, context.Element);
                    }
                    break;
                case _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_Text:
                    if (text != null)
                        writer.WriteString(trimHtmlText(text));
                    break;
                default:
                    break;
            }
        }

        private static string BalanceHtml(string html)
        {
            StringBuilder sb = new StringBuilder(html.Length + 10);

            SimpleHtmlParser parser = new SimpleHtmlParser(html);
            Element el;
            while (null != (el = parser.Next()))
            {
                if (el is BeginTag)
                {
                    BeginTag bt = (BeginTag)el;
                    if (!ElementFilters.RequiresEndTag(bt.Name))
                        bt.Complete = true;
                }
                sb.Append(el.ToString());
            }

            return sb.ToString();
        }

        /// <summary>
        /// Utility for properly printing the end tag for an element.
        /// This utility takes care of including/suppressing end tags for empty nodes properly.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="element"></param>
        private static void printElementEnd(HtmlWriter writer, IHTMLElement element)
        {
            // No tagName, no end tag.
            if (string.IsNullOrEmpty(element.tagName))
            {
                return;
            }

            if (ElementFilters.RequiresEndTag(element))
            {
                writer.WriteEndElement(true);
            }
            else
            {
                writer.WriteEndElement(false);
            }
        }

        /// <summary>
        /// Utility for properly printing the start tag for an element.
        /// This utility takes care of including/suppressing attributes and namespaces properly.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="element"></param>
        private static void printElementStart(HtmlWriter writer, IHTMLElement element)
        {
            string tagName = element.tagName;

            // If there is no tag name, this is mostly an artificial tag reported by mshtml,
            // and not really present in the markup
            // (e.g HTMLTableCaptionClass)
            if (string.IsNullOrEmpty(tagName))
            {
                return;
            }

            //XHTML tags are all lowercase
            tagName = tagName.ToLower(CultureInfo.InvariantCulture);
            //this is a standard HTML tag, so just write it out.
            writer.WriteStartElement(tagName);

            IHTMLDOMNode node = element as IHTMLDOMNode;
            IHTMLAttributeCollection attrs = node.attributes as IHTMLAttributeCollection;
            if (attrs != null)
            {
                foreach (IHTMLDOMAttribute attr in attrs)
                {
                    string attrName = attr.nodeName as string;
                    if (attr.specified)
                    {

                        string attrNameLower = attrName.ToLower(CultureInfo.InvariantCulture);

                        //get the raw attribute value (so that IE doesn't try to expand out paths in the value).
                        string attrValue = element.getAttribute(attrName, 2) as string;
                        if (attrValue == null)
                        {
                            //IE won't return some attributes (like class) using IHTMLElement.getAttribute(),
                            //so if the value is null, try to get the value directly from the DOM Attribute.
                            //Note: we can't use the DOM value by default, because IE will rewrite the value
                            //to contain a fully-qualified path on some attributes (like src and href).
                            attrValue = attr.nodeValue as string;

                            if (attrValue == null)
                            {
                                if ((attrNameLower == "hspace" || attrNameLower == "vspace") && attr.nodeValue is int)
                                {
                                    attrValue = ((int)attr.nodeValue).ToString(CultureInfo.InvariantCulture);
                                }
                                else if (attrNameLower == "style")
                                {
                                    //Avoid bug: Images that are resized with the editor insert a STYLE attribute.
                                    //IE won't return the style attribute using the standard API, so we have to grab
                                    //it from the style object
                                    attrValue = element.style.cssText;
                                }
                                else if (attrNameLower == "colspan")
                                {
                                    attrValue = (element as IHTMLTableCell).colSpan.ToString(CultureInfo.InvariantCulture);
                                }
                                else if (attrNameLower == "rowspan")
                                {
                                    attrValue = (element as IHTMLTableCell).rowSpan.ToString(CultureInfo.InvariantCulture);
                                }
                                else if (attrNameLower == "align" && attr.nodeValue is int)
                                {
                                    // This is not documented anywhere. Just discovered the values empirically on IE7 (Vista).
                                    switch ((int)attr.nodeValue)
                                    {
                                        case 1:
                                            attrValue = "left";
                                            break;
                                        case 2:
                                            attrValue = "center";
                                            break;
                                        case 3:
                                            attrValue = "right";
                                            break;
                                        case 4:
                                            attrValue = "texttop";
                                            break;
                                        case 5:
                                            attrValue = "absmiddle";
                                            break;
                                        case 6:
                                            attrValue = "baseline";
                                            break;
                                        case 7:
                                            attrValue = "absbottom";
                                            break;
                                        case 8:
                                            attrValue = "bottom";
                                            break;
                                        case 9:
                                            attrValue = "middle";
                                            break;
                                        case 10:
                                            attrValue = "top";
                                            break;
                                    }
                                }
                            }
                            Debug.WriteLineIf(attrValue != null && attrName != "id", String.Format(CultureInfo.InvariantCulture, "{0}.{1} attribute value not retreived", tagName, attrName), element.outerHTML);
                        }

                        // Minimized attributes are not allowed, according
                        // to section 4.5 of XHTML 1.0 specification.
                        // TODO: Deal with simple values that are not strings
                        if (attrValue == null && attrNameLower != "id")
                            attrValue = attrName;

                        if (attrName != null && attrValue != null)
                        {
                            //write out this attribute.
                            writer.WriteAttributeString(attrName, attrValue);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Strips the tags that a surrounding a string (ex: <b><i>text</i></b> becomes text)
        /// </summary>
        /// <param name="text">a string of text zero or one surrounding tags</param>
        /// <returns></returns>
        private static string StripSurroundingTags(string text)
        {
            int index = text.IndexOf('<');
            if (index != -1)
            {
                //this HTML text fragment is surrounded by tags, but shouldn't be
                //fix bug 486877 by trimming whitespace before/after the tags, so it doesn't creep in.
                text = text.Trim();
                index = text.IndexOf('<'); //reset the index of the tag
            }
            while (index != -1)
            {
                int index2 = text.IndexOf('>', index);
                text = text.Remove(index, index2 - index + 1);
                index = text.IndexOf('<');
            }
            return text;
        }
    }

    class HtmlWriter
    {
        private StringBuilder sb;
        private Stack openElements = new Stack();

        public HtmlWriter(StringBuilder sb)
        {
            this.sb = sb;
        }

        public void WriteAttributeString(String name, String val)
        {
            GetCurrentElementPrinter().WriteElementAttribute(sb, name, val);
        }

        public void WriteStartElement(String name)
        {
            HtmlElementPrinter currentPrinter = null;
            if (openElements.Count > 0)
            {
                currentPrinter = GetCurrentElementPrinter();
            }

            HtmlElementPrinter elementPrinter = new HtmlElementPrinter(name, openElements.Count, currentPrinter);

            if (currentPrinter != null)
                currentPrinter.BeforeAddChildElement(sb, name);

            elementPrinter.WriteElementStart(sb);
            openElements.Push(elementPrinter);
        }

        private HtmlElementPrinter GetCurrentElementPrinter()
        {
            return (HtmlElementPrinter)openElements.Peek();
        }

        public void WriteString(String str)
        {
            if (openElements.Count > 0)
                GetCurrentElementPrinter().WriteString(sb, str);
            else
                sb.Append(str);
        }

        public void WriteEndElement(bool requiresEndTag)
        {
            HtmlElementPrinter elementPrinter = (HtmlElementPrinter)openElements.Pop();
            elementPrinter.WriteElementEnd(sb, requiresEndTag);
        }

        /// <summary>
        /// Look up table holding the names of all block-formatted tags.
        /// </summary>
        private static Hashtable BlockTagNames
        {
            get
            {
                if (_blockTagNames == null)
                {
                    _blockTagNames = new Hashtable();
                    _blockTagNames["div"] = "div";
                    _blockTagNames["p"] = "p";
                    _blockTagNames["pre"] = "pre";
                    _blockTagNames["br"] = "br";
                    _blockTagNames["h1"] = "h1";
                    _blockTagNames["h2"] = "h2";
                    _blockTagNames["h3"] = "h3";
                    _blockTagNames["h4"] = "h4";
                    _blockTagNames["h5"] = "h5";
                    _blockTagNames["h6"] = "h6";
                    _blockTagNames["hr"] = "hr";
                    _blockTagNames["blockquote"] = "blockquote";
                    _blockTagNames["table"] = "table";
                    _blockTagNames["tr"] = "tr";
                    _blockTagNames["td"] = "td";
                    _blockTagNames["th"] = "th";
                    _blockTagNames["ul"] = "ul";
                    _blockTagNames["ol"] = "ol";
                    _blockTagNames["li"] = "li";
                }
                return _blockTagNames;
            }
        }
        private static Hashtable _blockTagNames;

        /// <summary>
        /// Manages the incremental printing of an HTML element.
        /// </summary>
        private class HtmlElementPrinter
        {
            private enum START_TAG_STATE { NONE, OPENED, CLOSED };
            private START_TAG_STATE startTagState = START_TAG_STATE.NONE;

            private String tagName;
            private int stackDepth;
            private HtmlElementPrinter parentElementPrinter;
            private ElementIndentStrategy indentStrategy;
            private int childCount;
            public HtmlElementPrinter(String tagName, int stackDepth, HtmlElementPrinter parentElementPrinter)
            {
                this.tagName = tagName;
                this.stackDepth = stackDepth;
                this.parentElementPrinter = parentElementPrinter;

                //create the appropriate indentation strategy for this element
                this.indentStrategy = CreateIndentStrategy(tagName, this);
            }

            public HtmlElementPrinter getParent()
            {
                return parentElementPrinter;
            }

            public void WriteElementStart(StringBuilder sb)
            {
                startTagState = START_TAG_STATE.OPENED;
                ApplyStartIndentStrategy(sb, stackDepth);
                sb.Append("<");
                sb.Append(tagName);
            }

            public void WriteElementAttribute(StringBuilder sb, String attrName, String attrValue)
            {
                Debug.Assert(Regex.IsMatch(attrName, "^[a-zA-Z-]+$"), "Illegal attribute name: " + attrName);
                sb.Append(" ");
                sb.Append(attrName);
                sb.Append("=\"");
                sb.Append(HtmlUtils.EscapeEntities(attrValue));
                sb.Append("\"");
            }

            public void WriteElementEnd(StringBuilder sb, bool requiresEndTag)
            {
                if (startTagState == START_TAG_STATE.OPENED && !requiresEndTag)
                {
                    sb.Append(" /");
                    CloseStartElement(sb);
                }
                else
                {
                    CloseStartElement(sb);
                    ApplyEndIndentStrategy(sb, stackDepth);
                    sb.Append("</");
                    sb.Append(tagName);
                    sb.Append(">");
                    if (BlockTagNames.ContainsKey(tagName))
                    {
                        sb.Append("\r\n");
                    }
                }
            }

            public void BeforeAddChildElement(StringBuilder sb, String tagName)
            {
                CloseStartElement(sb);
                childCount++;
            }

            public void WriteString(StringBuilder sb, String str)
            {
                CloseStartElement(sb);
                sb.Append(str);
            }

            public int ChildCount
            {
                get { return childCount; }
            }

            private void CloseStartElement(StringBuilder sb)
            {
                if (startTagState == START_TAG_STATE.OPENED)
                {
                    sb.Append(">");
                    startTagState = START_TAG_STATE.CLOSED;
                }
            }

            protected virtual void ApplyStartIndentStrategy(StringBuilder sb, int depth)
            {
                indentStrategy.ApplyStartIndent(sb, depth);
            }

            protected virtual void ApplyEndIndentStrategy(StringBuilder sb, int depth)
            {
                indentStrategy.ApplyEndIndent(sb, depth);
            }

            private static ElementIndentStrategy CreateIndentStrategy(String tagName, HtmlElementPrinter printer)
            {
                if (BlockTagNames.ContainsKey(tagName))
                    return new BlockElementIndentStrategy(printer);
                else
                    return new ElementIndentStrategy(printer);
            }
        }

        /// <summary>
        /// Standard indentation strategy that only indents when a tag starts on a newline
        /// </summary>
        private class ElementIndentStrategy
        {
            protected HtmlElementPrinter elementPrinter;
            public ElementIndentStrategy(HtmlElementPrinter elementPrinter)
            {
                this.elementPrinter = elementPrinter;
            }

            public virtual void ApplyStartIndent(StringBuilder sb, int depth)
            {
                bool isStartOfLine = sb.Length == 0 || sb[sb.Length - 1] == '\n';

                if (isStartOfLine)
                    WriteIndent(sb, depth);
            }

            public virtual void ApplyEndIndent(StringBuilder sb, int depth)
            {
                bool isStartOfLine = sb.Length == 0 || sb[sb.Length - 1] == '\n';

                if (isStartOfLine)
                    WriteIndent(sb, depth);
            }

            protected void WriteIndent(StringBuilder sb, int depth)
            {
                for (int i = 0; i < depth; i++)
                {
                    sb.Append("  ");
                }
            }
        }

        /// <summary>
        /// Indentation strategy that forces a line break and indent before the element starts.
        /// </summary>
        private class BlockElementIndentStrategy : ElementIndentStrategy
        {
            private bool forceVisualLeadingLineBreak = true;
            public BlockElementIndentStrategy(HtmlElementPrinter elementPrinter)
                : base(elementPrinter)
            {
                //if this element is is the very first content in the parent element, then suppress
                //adding the visual line break so that we don't end up with extra whitespace.
                HtmlElementPrinter parent = elementPrinter.getParent();
                if (parent != null)
                    forceVisualLeadingLineBreak = parent.ChildCount > 0;
            }

            public override void ApplyStartIndent(StringBuilder sb, int depth)
            {
                bool isStartOfLine = sb.Length == 0 || sb[sb.Length - 1] == '\n';

                //force a line break so this tag can be indented.
                if (!isStartOfLine)
                {
                    sb.Append("\r\n");
                }

                if (forceVisualLeadingLineBreak && sb.Length > 0)
                {
                    //add an additional line break to mirror the visual breaks
                    //shown when the HTML is rendered in a browser
                    sb.Append("\r\n");
                }

                WriteIndent(sb, depth);
            }
        }
    }
}
