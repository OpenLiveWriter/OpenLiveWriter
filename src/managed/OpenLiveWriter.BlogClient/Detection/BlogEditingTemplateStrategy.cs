// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.BlogClient.Detection
{
    internal class BlogEditingTemplateStrategies
    {
        internal enum StrategyType { FontsAndPaddingOnly, NoSiblings, FramedWysiwyg, Site, NoStyle }

        internal static BlogEditingTemplateStrategy GetTemplateStrategy(StrategyType strategyType)
        {
            switch (strategyType)
            {
                case StrategyType.NoStyle:
                    return new NoStyleEditingTemplate();
                case StrategyType.FontsAndPaddingOnly:
                    return new WordStyleEditingTemplate();
                case StrategyType.NoSiblings:
                    return new ParentElementsOnlyEditingTemplate();
                case StrategyType.Site:
                    return new WebsiteEditingTemplate();
                case StrategyType.FramedWysiwyg:
                    return new FramedWordStyleEditingTemplate();
                default:
                    throw new ArgumentException("Unknown template strategy: " + strategyType.ToString());
            }
        }
    }
    /// <summary>
    /// Summary description for BlogEditingTemplateStrategy.
    /// </summary>
    internal abstract class BlogEditingTemplateStrategy
    {
        public BlogEditingTemplateStrategy()
        {
        }

        protected internal virtual void FixupDownloadedFiles(string blogTemplateFile, FileBasedSiteStorage storage, string supportingFilesDir)
        {
        }

        protected internal abstract BlogEditingTemplate GenerateBlogTemplate(IHTMLDocument3 doc, IHTMLElement titleElement, IHTMLElement[] allTitleElements, IHTMLElement bodyElement);

        protected static void CleanupContainingAnchorTag(IHTMLElement titleElement)
        {
            try
            {
                IHTMLAnchorElement containingAnchor = HTMLElementHelper.GetContainingAnchorElement(titleElement);
                if (containingAnchor != null)
                {
                    IHTMLElement anchor = containingAnchor as IHTMLElement;

                    //set to empty so link styles are still applied (bug 297187)
                    anchor.setAttribute("href", "", 0);

                    anchor.removeAttribute("title", 0);
                    anchor.removeAttribute("rel", 0);
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected error cleaning anchor tag: " + ex.ToString());
            }
        }

        protected void SetDefaultTitleText(string titleText, params IHTMLElement[] titleElements)
        {
            foreach (IHTMLElement titleElement in titleElements)
            {
                titleElement.innerText = titleText;
            }
        }
    }

    internal class FramedWordStyleEditingTemplate : WordStyleEditingTemplate
    {
        protected internal override BlogEditingTemplate GenerateBlogTemplate(IHTMLDocument3 doc, IHTMLElement titleElement, IHTMLElement[] allTitleElements, IHTMLElement bodyElement)
        {
            // if title is containing with a link then strip the link
            CleanupContainingAnchorTag(titleElement);
            string templateHtml = "";
            StyleBuilder styleBuilder = new StyleBuilder();
            IMarkupServicesRaw rawMarkupServices = doc as IMarkupServicesRaw;
            MshtmlMarkupServices markupServices = new MshtmlMarkupServices(rawMarkupServices);
            MarkupPointer startPointer = markupServices.CreateMarkupPointer(titleElement, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin);
            MarkupPointer endPointer = markupServices.CreateMarkupPointer(bodyElement, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
            MarkupRange range = markupServices.CreateMarkupRange(startPointer, endPointer);

            IHTMLElement stopElement = range.ParentElement();
            IHTMLElement currElement;

            string titleTemplateText = WrapInHiddenHtml(postTitleClass, BlogEditingTemplate.POST_TITLE_MARKER);
            AddTitleStyles(titleElement, styleBuilder);

            currElement = titleElement;
            while (currElement != null && currElement.sourceIndex != stopElement.sourceIndex)
            {
                string className = currElement.tagName + currElement.sourceIndex;
                titleTemplateText = WriteStartTag(currElement, className) + titleTemplateText + WriteEndTag(currElement);
                AddFrameStyles(currElement, "." + className, styleBuilder);
                currElement = currElement.parentElement;
            }

            string bodyTemplateText = WrapInHiddenHtml(postBodyClass, BlogEditingTemplate.POST_BODY_MARKER);
            AddBodyStyles(bodyElement, styleBuilder);

            currElement = bodyElement;
            while (currElement != null && currElement.sourceIndex != stopElement.sourceIndex)
            {
                string className = currElement.tagName + currElement.sourceIndex;
                bodyTemplateText = WriteStartTag(currElement, className) + bodyTemplateText + WriteEndTag(currElement);
                AddFrameStyles(currElement, "." + className, styleBuilder);
                currElement = currElement.parentElement;
            }

            templateHtml = titleTemplateText + bodyTemplateText;
            currElement = range.ParentElement();
            while (currElement != null)
            {
                string className = null;
                if (currElement.tagName == "HTML")
                {
                    MarkupPointer bodyPointer = markupServices.CreateMarkupPointer(((IHTMLDocument2)doc).body, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
                    MarkupPointer docPointer = markupServices.CreateMarkupPointer(currElement, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin);
                    MarkupRange headRange = markupServices.CreateMarkupRange(docPointer, bodyPointer);
                    IHTMLElement[] elements = headRange.GetTopLevelElements(new IHTMLElementFilter(IsHeadElement));
                    if (elements.Length > 0)
                    {
                        //string head = elements[0].innerHTML;
                        string head = "";
                        //string defaultStyles = "<style>p, h1, h2, h3, h4, h5, h6, blockquote, pre{ padding-top: 1px; }</style>";
                        styleBuilder.Dispose();
                        head = String.Format(CultureInfo.InvariantCulture, "<head>{0}<style>{1}</style></head>", head, styleBuilder.ToString());
                        templateHtml = head + templateHtml;
                    }
                }
                else
                {
                    className = currElement.tagName + currElement.sourceIndex;
                    AddFrameStyles(currElement, "." + className, styleBuilder);
                }
                templateHtml = WriteStartTag(currElement, className) + templateHtml + WriteEndTag(currElement);
                currElement = currElement.parentElement;
            }

            //prepend the doctype of the document - this prevents styles in the document from rendering improperly
            string docType = HTMLDocumentHelper.GetSpecialHeaders((IHTMLDocument2)doc).DocType;
            //string docType = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\"[]>";
            if (docType != null)
                templateHtml = docType + "\r\n" + templateHtml;

            return new BlogEditingTemplate(templateHtml);
        }

        private void AddFrameStyles(IHTMLElement e, string selector, StyleBuilder styleBuilder)
        {
            IHTMLElement2 e2 = (IHTMLElement2)e;
            IHTMLCurrentStyle currStyle = e2.currentStyle;
            styleBuilder.Selector = selector;

            //styleBuilder.Append("font-size", currStyle.fontSize);
            styleBuilder.Append("display", currStyle.display);
            styleBuilder.Append("padding", currStyle.padding);
            styleBuilder.Append("margin", currStyle.margin);
            styleBuilder.Append("width", currStyle.width);
            styleBuilder.Append("height", currStyle.height);
            styleBuilder.Append("display", currStyle.display);

            styleBuilder.Append("background-color", currStyle.backgroundColor);
            styleBuilder.Append("background-repeat", currStyle.backgroundRepeat);
            styleBuilder.Append("background-image", currStyle.backgroundImage);
            styleBuilder.Append("background-position", currStyle.backgroundPositionY + " " + currStyle.backgroundPositionX);

            styleBuilder.Append("border-top", CreateBorderStyle(currStyle.borderTopWidth, currStyle.borderTopStyle, currStyle.borderTopColor));
            styleBuilder.Append("border-right", CreateBorderStyle(currStyle.borderRightWidth, currStyle.borderRightStyle, currStyle.borderRightColor));
            styleBuilder.Append("border-bottom", CreateBorderStyle(currStyle.borderBottomWidth, currStyle.borderBottomStyle, currStyle.borderBottomColor));
            styleBuilder.Append("border-left", CreateBorderStyle(currStyle.borderLeftWidth, currStyle.borderLeftStyle, currStyle.borderLeftColor));
        }

        private string WriteStartTag(IHTMLElement element, string className)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<");
            sb.Append(element.tagName);

            if (className != null)
                sb.Append(" class=" + className);

            sb.Append(">");
            string startTag = sb.ToString();
            return startTag;
        }

        private string WriteStartTag(IHTMLElement element)
        {
            return WriteStartTag(element, null);
        }

        private string WriteEndTag(IHTMLElement element)
        {
            return String.Format(CultureInfo.InvariantCulture, "</{0}>", element.tagName);
        }

        private string WrapInHiddenHtml(string className, string html)
        {
            return String.Format(CultureInfo.InvariantCulture, "<div class='{0}' style='border: 0px; margin 0px; padding: 0px;'>{1}</div>", className, html);
        }
    }
    internal class NoStyleEditingTemplate : BlogEditingTemplateStrategy
    {
        protected internal override BlogEditingTemplate GenerateBlogTemplate(IHTMLDocument3 doc, IHTMLElement titleElement, IHTMLElement[] allTitleElements, IHTMLElement bodyElement)
        {
            return new BlogEditingTemplate(true);
        }
    }

    internal class WordStyleEditingTemplate : BlogEditingTemplateStrategy
    {
        protected internal override BlogEditingTemplate GenerateBlogTemplate(IHTMLDocument3 doc, IHTMLElement titleElement, IHTMLElement[] allTitleElements, IHTMLElement bodyElement)
        {
            CleanupContainingAnchorTag(titleElement);
            StyleBuilder styleBuilder = new StyleBuilder();
            string templateHtml;
            using (styleBuilder)
            {
                string titleTemplateText = WrapInStyledDiv(postTitleClass, BlogEditingTemplate.POST_TITLE_MARKER);
                AddTitleStyles(titleElement, styleBuilder);

                string bodyTemplateText = WrapInStyledDiv(postBodyClass, BlogEditingTemplate.POST_BODY_MARKER);
                AddBodyStyles(bodyElement, styleBuilder);

                templateHtml = titleTemplateText + bodyTemplateText;
            }

            string headHtml = String.Format(CultureInfo.InvariantCulture, "<head><style>{0}</style></head>", styleBuilder.ToString());
            templateHtml = String.Format(CultureInfo.InvariantCulture, "<html>{0}<body>{1}</body></html>", headHtml, templateHtml);

            //prepend the doctype of the document - this prevents styles in the document from rendering improperly
            string docType = HTMLDocumentHelper.GetSpecialHeaders((IHTMLDocument2)doc).DocType;
            if (docType != null)
                templateHtml = docType + "\r\n" + templateHtml;

            return new BlogEditingTemplate(templateHtml);
        }

        protected const string postTitleClass = "formattedPostTitle";
        protected const string postBodyClass = "formattedPostBody";

        private BlogEditingTemplate CreateTemplateHTML(string docType, string styleData, string titleTag, string bodyTag)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(docType);
            sb.Append("\n");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<html><head><style>{0}</style></head><body>", styleData);
            sb.AppendFormat(CultureInfo.InvariantCulture, "<{0} class='{1}'>{2}</{0}>", titleTag, postTitleClass, BlogEditingTemplate.POST_TITLE_MARKER);
            sb.AppendFormat(CultureInfo.InvariantCulture, "<{0} class='{1}'>{2}</{0}>", bodyTag, postBodyClass, BlogEditingTemplate.POST_BODY_MARKER);
            sb.AppendFormat(CultureInfo.InvariantCulture, "</body></html>");

            return new BlogEditingTemplate(sb.ToString());
        }

        private string WrapInStyledDiv(string className, string html)
        {
            return String.Format(CultureInfo.InvariantCulture, "<div class='{0}'>{1}</div>", className, html);
        }

        protected void AddTitleStyles(IHTMLElement titleElement, StyleBuilder styleBuilder)
        {
            string fontSize = (string)((IHTMLElement2)titleElement).currentStyle.fontSize;
            AddStyles(titleElement, "." + postTitleClass, styleBuilder, true, false, false, fontSize);

            //explicitly preserve the background color of the title element just in case the title has
            //a different background than the postBody element.  If we don't do this, we open ourselves
            //to the possibility of having a title font color that is the same as the body background color,
            //in which case the title would be invisible while editing.
            //styleBuilder.Selector = "." + postTitleClass;
            //AppendBackgroundStyles(titleElement, styleBuilder, false);
        }

        protected void AddBodyStyles(IHTMLElement postBodyElement, StyleBuilder styleBuilder)
        {
            string selector = "." + postBodyClass;
            styleBuilder.Selector = selector;
            string fontSize = (string)((IHTMLElement2)postBodyElement).currentStyle.fontSize;
            AddStyles(postBodyElement, selector, styleBuilder, false, false, false, fontSize);

            //set the background color of the post body at the background of the entire body
            styleBuilder.Selector = "body";
            AppendBackgroundStyles(postBodyElement, styleBuilder, false);

            //set the default font size since this is the size that all %-based font-sizes are rooted from
            IHTMLElement2 bodyElement2 = (IHTMLElement2)((IHTMLDocument2)postBodyElement.document).body;
            styleBuilder.Append("font-size", bodyElement2.currentStyle.fontSize);

            //set the default styles for inline elements for the post body
            AddInlineElementStyles(postBodyElement, "." + postBodyClass + " ", styleBuilder, fontSize);

            AddPostBodyBlockElementStyle(postBodyElement, "p", "", styleBuilder, fontSize);
            AddPostBodyBlockElementStyle(postBodyElement, "h1", "", styleBuilder, fontSize);
            AddPostBodyBlockElementStyle(postBodyElement, "h2", "", styleBuilder, fontSize);
            AddPostBodyBlockElementStyle(postBodyElement, "h3", "", styleBuilder, fontSize);
            AddPostBodyBlockElementStyle(postBodyElement, "h4", "", styleBuilder, fontSize);
            AddPostBodyBlockElementStyle(postBodyElement, "h5", "", styleBuilder, fontSize);
            AddPostBodyBlockElementStyle(postBodyElement, "h6", "", styleBuilder, fontSize);
            AddPostBodyBlockElementStyle(postBodyElement, "blockquote", "", styleBuilder, fontSize);

            AddPostBodyImageElementStyle(postBodyElement, "", styleBuilder, fontSize);

            AddPostBodyListStyles(postBodyElement, "ol", "", styleBuilder, fontSize, 3);
            AddPostBodyListStyles(postBodyElement, "ul", "", styleBuilder, fontSize, 3);
        }

        private void AddPostBodyImageElementStyle(IHTMLElement parentElement, string selectorPrefix, StyleBuilder styleBuilder, string defaultFontSize)
        {
            IHTMLDocument2 doc2 = (parentElement.document as IHTMLDocument2);
            IHTMLElement newElement = doc2.createElement("img");
            (parentElement as IHTMLDOMNode).appendChild((IHTMLDOMNode)newElement);

            IHTMLElement2 e2 = (IHTMLElement2)newElement;

            styleBuilder.Selector = selectorPrefix + " img";
            IHTMLCurrentStyle currStyle = e2.currentStyle;

            styleBuilder.Append("display", currStyle.display);

            styleBuilder.Append("padding", currStyle.padding);
            styleBuilder.Append("margin", currStyle.margin);

            styleBuilder.Append("border-top", CreateBorderStyle(currStyle.borderTopWidth, currStyle.borderTopStyle, currStyle.borderTopColor));
            styleBuilder.Append("border-right", CreateBorderStyle(currStyle.borderRightWidth, currStyle.borderRightStyle, currStyle.borderRightColor));
            styleBuilder.Append("border-bottom", CreateBorderStyle(currStyle.borderBottomWidth, currStyle.borderBottomStyle, currStyle.borderBottomColor));
            styleBuilder.Append("border-left", CreateBorderStyle(currStyle.borderLeftWidth, currStyle.borderLeftStyle, currStyle.borderLeftColor));
        }

        private void AddPostBodyListStyles(IHTMLElement parentElement, string listTag, string selectorPrefix, StyleBuilder styleBuilder, string defaultFontSize, int stopDepth)
        {
            IHTMLDocument2 doc2 = (parentElement.document as IHTMLDocument2);
            IHTMLElement listElement = doc2.createElement(listTag);
            (parentElement as IHTMLDOMNode).appendChild((IHTMLDOMNode)listElement);

            styleBuilder.Selector = selectorPrefix + " " + listTag;
            IHTMLCurrentStyle currStyle = ((IHTMLElement2)listElement).currentStyle;

            styleBuilder.Append("display", currStyle.display);
            styleBuilder.Append("padding", currStyle.padding);
            styleBuilder.Append("margin", currStyle.margin);

            IHTMLElement listItem = AddPostBodyElementStyle(listElement, "li", selectorPrefix + " " + listTag + " ", styleBuilder, defaultFontSize, false);
            currStyle = (listItem as IHTMLElement2).currentStyle;
            styleBuilder.Append("list-style", currStyle.listStyleType);
            styleBuilder.Append("list-style-image", currStyle.listStyleImage);
            styleBuilder.Append("list-style-position", currStyle.listStylePosition);

            if (stopDepth > 1)
                AddPostBodyListStyles(listItem, listTag, selectorPrefix + " " + listTag + " ", styleBuilder, defaultFontSize, stopDepth - 1);
        }

        protected string CreateBorderStyle(object width, object style, object color)
        {
            StringBuilder sb = new StringBuilder();
            if (width != null)
                sb.Append(width);
            if (style != null)
            {
                if (sb.Length > 0)
                    sb.Append(" ");
                sb.Append(style);
            }
            if (color != null)
            {
                if (sb.Length > 0)
                    sb.Append(" ");
                sb.Append(color);
            }
            return sb.ToString();
        }

        private void AddPostBodyBlockElementStyle(IHTMLElement parentElement, string elementName, string selectorPrefix, StyleBuilder styleBuilder, string defaultFontSize)
        {
            IHTMLElement blockElement = AddPostBodyElementStyle(parentElement, elementName, selectorPrefix, styleBuilder, defaultFontSize, false);

            string inlineSelector = selectorPrefix != "" ? selectorPrefix + " " + elementName : elementName;
            AddInlineElementStyles(blockElement, inlineSelector + " ", styleBuilder, defaultFontSize);
        }

        private void AddInlineElementStyles(IHTMLElement parentElement, string selectorPrefix, StyleBuilder styleBuilder, string defaultFontSize)
        {
            AddPostBodyElementStyle(parentElement, "a", selectorPrefix, styleBuilder, defaultFontSize, true);
            AddPostBodyElementStyle(parentElement, "b", selectorPrefix, styleBuilder, defaultFontSize, true);
            AddPostBodyElementStyle(parentElement, "i", selectorPrefix, styleBuilder, defaultFontSize, true);
            AddPostBodyElementStyle(parentElement, "strong", selectorPrefix, styleBuilder, defaultFontSize, true);
            AddPostBodyElementStyle(parentElement, "em", selectorPrefix, styleBuilder, defaultFontSize, true);
            AddPostBodyElementStyle(parentElement, "span", selectorPrefix, styleBuilder, defaultFontSize, true);
            AddPostBodyElementStyle(parentElement, "strike", selectorPrefix, styleBuilder, defaultFontSize, true);
            AddPostBodyElementStyle(parentElement, "u", selectorPrefix, styleBuilder, defaultFontSize, true);
            AddPostBodyElementStyle(parentElement, "img", selectorPrefix, styleBuilder, defaultFontSize, true);
        }

        private IHTMLElement AddPostBodyElementStyle(IHTMLElement parentElement, string elementName, string selectorPrefix, StyleBuilder styleBuilder, string defaultFontSize, bool includeBorders)
        {
            IHTMLDocument2 doc2 = (parentElement.document as IHTMLDocument2);
            IHTMLElement newElement = doc2.createElement(elementName);
            (parentElement as IHTMLDOMNode).appendChild((IHTMLDOMNode)newElement);
            IHTMLElement2 newElement2 = (IHTMLElement2)newElement;

            try
            {
                newElement.innerText = "Hello";
            }
            catch (Exception) { } // some tags don't allow inner text, so just ignore

            //bug fix: anchors will not properly set their current styles if they do not have an href attribute
            if (newElement.tagName == "A")
                newElement.setAttribute("href", "http://www.notarealdomain.com", 0);

            string fontSize = (string)newElement2.currentStyle.fontSize;
            if (fontSize == defaultFontSize)
            {
                //if the fontSize is the same as the default font, then don't set the font size as
                //this would cause the font to get reduced/increased by an unexpected %.
                //This fixes a bug where template editing styles get mucked up by
                //% values that IE returns as the font size if no explicit font-size is set.

                //WARNING Be very careful about changing font-size logic in this
                //class as it needs to be tested against many blog templates.
                fontSize = null;
            }
            AddStyles(newElement, selectorPrefix + elementName.ToLower(CultureInfo.InvariantCulture), styleBuilder, false, true, includeBorders, fontSize);
            return newElement;
        }

        private void AppendBackgroundStyles(IHTMLElement e, StyleBuilder styleBuilder, bool allowImages)
        {
            IHTMLElement2 e2 = (IHTMLElement2)e;
            try
            {
                Color c = GetBackgroundColorFromPixel(e);
                string defaultBackgroundColor = ColorHelper.ColorToString(c);
                styleBuilder.Append("background-color", defaultBackgroundColor);
            }
            catch (Exception)
            {
                while (e2 != null)
                {
                    string bgColor = (string)e2.currentStyle.backgroundColor;
                    if (bgColor != "transparent")
                    {
                        styleBuilder.Append("background-color", bgColor);
                        return;
                    }
                    e2 = (IHTMLElement2)((IHTMLElement)e2).parentElement;
                }
            }
        }

        private static Color GetBackgroundColorFromPixel(IHTMLElement e)
        {
            IHTMLElement divElement = (e.document as IHTMLDocument2).createElement("div");
            divElement.id = Guid.NewGuid().ToString();
            divElement.style.padding = "10px 10px 10px 10px";
            divElement.style.width = "100px";
            divElement.style.height = "100px";
            (e as IHTMLDOMNode).appendChild((IHTMLDOMNode)divElement);
            string documentHtml = HTMLDocumentHelper.HTMLDocToString(e.document as IHTMLDocument2);
            (e as IHTMLDOMNode).removeChild((IHTMLDOMNode)divElement);

            HtmlScreenCaptureCore screenCapture = new HtmlScreenCaptureCore(documentHtml, 800);
            DocumentSnapshotHandler snapshotHandler = new DocumentSnapshotHandler(divElement.id);
            screenCapture.HtmlDocumentAvailable += new HtmlDocumentAvailableHandlerCore(snapshotHandler.HtmlDocumentAvailable);
            Bitmap docImage = screenCapture.CaptureHtml(3000);
            using (Graphics g = Graphics.FromImage(docImage))
            {
                Rectangle rect = snapshotHandler.rect;
                g.DrawRectangle(new Pen(Color.Blue, 1), rect);
            }
            using (FileStream fs = new FileStream(@"c:\temp\docImage.png", FileMode.Create))
            {
                docImage.Save(fs, ImageFormat.Png);
            }

            Color c = docImage.GetPixel(snapshotHandler.rect.X + 5, snapshotHandler.rect.Y + 5);
            return c;
        }

        private class DocumentSnapshotHandler
        {
            string _elementId;
            public Rectangle rect;
            public DocumentSnapshotHandler(string elementId)
            {
                _elementId = elementId;
            }
            public void HtmlDocumentAvailable(object sender, HtmlDocumentAvailableEventArgsCore e)
            {
                IHTMLDocument3 doc3 = e.Document as IHTMLDocument3;
                IHTMLElement2 element = (IHTMLElement2)doc3.getElementById(_elementId);
                IHTMLRect hRect = element.getBoundingClientRect();
                rect = new Rectangle(hRect.left, hRect.top, hRect.right - hRect.left, hRect.bottom - hRect.top);
                e.DocumentReady = true;
            }
        }

        protected void AddStyles(IHTMLElement e, string selector, StyleBuilder styleBuilder, bool invisible, bool includeBackgroundColor, bool includeBorders, string fontSize)
        {
            IHTMLElement2 e2 = (IHTMLElement2)e;
            IHTMLCurrentStyle currStyle = e2.currentStyle;
            styleBuilder.Selector = selector;

            if (fontSize != null)
                styleBuilder.Append("font-size", fontSize);
            styleBuilder.Append("font-family", currStyle.fontFamily);
            styleBuilder.Append("font-weight", currStyle.fontWeight);
            styleBuilder.Append("font-variant", currStyle.fontVariant);
            styleBuilder.Append("line-height", currStyle.lineHeight);
            styleBuilder.Append("text-transform", currStyle.textTransform);
            styleBuilder.Append("text-decoration", currStyle.textDecoration);
            styleBuilder.Append("color", currStyle.color);

            if (includeBackgroundColor)
                styleBuilder.Append("background-color", currStyle.backgroundColor);

            styleBuilder.Append("display", currStyle.display);

            string padding = e2.currentStyle.padding;
            if (invisible || padding != "auto")
                styleBuilder.Append("padding", invisible ? "0px" : padding);

            string margin = e2.currentStyle.margin;
            if (invisible || margin != "auto")
                styleBuilder.Append("margin", invisible ? "0px" : margin);

            if (includeBorders)
            {
                styleBuilder.Append("border-top", CreateBorderStyle(currStyle.borderTopWidth, currStyle.borderTopStyle, currStyle.borderTopColor));
                styleBuilder.Append("border-right", CreateBorderStyle(currStyle.borderRightWidth, currStyle.borderRightStyle, currStyle.borderRightColor));
                styleBuilder.Append("border-bottom", CreateBorderStyle(currStyle.borderBottomWidth, currStyle.borderBottomStyle, currStyle.borderBottomColor));
                styleBuilder.Append("border-left", CreateBorderStyle(currStyle.borderLeftWidth, currStyle.borderLeftStyle, currStyle.borderLeftColor));
            }
        }

        protected bool IsHeadElement(IHTMLElement e)
        {
            return e.tagName == "HEAD";
        }

        protected class StyleBuilder : IDisposable
        {
            private string _currentSelector;
            private StringBuilder sb = new StringBuilder();

            public string Selector
            {
                get { return _currentSelector; }
                set
                {
                    if (_currentSelector != value)
                    {
                        CloseSelector();
                        _currentSelector = value;
                        if (_currentSelector != null)
                        {
                            sb.Append(_currentSelector);
                            sb.Append("{");
                        }
                    }
                }
            }

            public void Append(string styleName, object styleValue)
            {
                if (styleValue != null)
                    sb.AppendFormat("\r\n    {0}: {1}; ", styleName, styleValue.ToString());
            }

            public override string ToString()
            {
                return sb.ToString();
            }

            private void CloseSelector()
            {
                if (_currentSelector != null)
                {
                    sb.Append("\r\n}\r\n");
                    _currentSelector = null;
                }
            }

            public void Dispose()
            {
                CloseSelector();
            }
        }
    }

    /// <summary>
    /// Strategy that attempts to preserve the website, but clears out all content.
    /// </summary>
    internal class WebLayoutEditingTemplate2 : BlogEditingTemplateStrategy
    {
        /// <summary>
        /// Generates an editing template the almost perfectly matches the existing website (include sidebars, titles, etc).
        /// </summary>
        protected internal override BlogEditingTemplate GenerateBlogTemplate(IHTMLDocument3 doc, IHTMLElement titleElement, IHTMLElement[] allTitleElements, IHTMLElement bodyElement)
        {
            // if title is containing with a link then strip the link
            CleanupContainingAnchorTag(titleElement);

            ClearDocumentContent(doc);

            //update primary titleElement to contain the editable title marker.
            string titleTemplateText = BlogEditingTemplate.POST_TITLE_MARKER;
            titleElement.innerText = titleTemplateText;

            //update the bodyElement to contain the editable body marker.
            string bodyTemplateText = BlogEditingTemplate.POST_BODY_MARKER;
            bodyElement.innerText = bodyTemplateText;

            string templateHtml = HTMLDocumentHelper.HTMLDocToString((IHTMLDocument2)doc);

            //prepend the doctype of the document - this prevents styles in the document from rendering improperly
            string docType = HTMLDocumentHelper.GetSpecialHeaders((IHTMLDocument2)doc).DocType;
            if (docType != null)
                templateHtml = docType + "\r\n" + templateHtml;

            return new BlogEditingTemplate(templateHtml);
        }

        private void ClearDocumentContent(IHTMLDocument3 doc)
        {
            IHTMLElement body = (doc as IHTMLDocument2).body;
            IHTMLDOMNode bodyNode = body as IHTMLDOMNode;
            ClearNodeContent(bodyNode);
        }

        private void ClearNodeContent(IHTMLDOMNode node)
        {
            foreach (IHTMLDOMNode childNode in node.childNodes as IHTMLDOMChildrenCollection)
            {
                switch (childNode.nodeType)
                {
                    case HTMLDocumentHelper.HTMLDOMNodeTypes.ElementNode:
                        ClearNodeContent(childNode);
                        break;
                    case HTMLDocumentHelper.HTMLDOMNodeTypes.TextNode:
                        ReplaceNodeTextWithNBSP(childNode);
                        break;
                }
            }
        }

        private void ReplaceNodeTextWithNBSP(IHTMLDOMNode node)
        {
            const string NBSP = " ";
            string nodeText = (string)node.nodeValue;
            int length = nodeText.Length;
            StringBuilder sb = new StringBuilder(length * NBSP.Length);
            for (int i = 0; i < length; i++)
            {
                sb.Append(NBSP);
            }
            node.nodeValue = sb.ToString();
        }
    }

    internal class WebsiteEditingTemplate : BlogEditingTemplateStrategy
    {
        /// <summary>
        /// Generates an editing template the almost perfectly matches the existing website (include sidebars, titles, etc).
        /// </summary>
        protected internal override BlogEditingTemplate GenerateBlogTemplate(IHTMLDocument3 doc, IHTMLElement titleElement, IHTMLElement[] allTitleElements, IHTMLElement bodyElement)
        {
            // if title is containing with a link then strip the link
            CleanupContainingAnchorTag(titleElement);

            // Remove <noscript> and <noembed> tags from the preview.
            RemoveNoShowElements((IHTMLDocument2)doc);

            //create read-only placeholder for document elements that contain the post title text
            //(including archive and recently posted links). Fixes bug 297877.
            SetDefaultTitleText(BlogEditingTemplate.POST_TITLE_READONLY_MARKER, allTitleElements);

            //update primary titleElement to contain the editable title marker.
            string titleTemplateText = BlogEditingTemplate.POST_TITLE_MARKER;
            titleElement.innerText = titleTemplateText;

            //if the title exists inside the body (which is about to be reset with the body marker),
            //preserve its parent tree so it doesn't get deleted
            IHTMLDOMNode preservedTitleNodes = MaybePreserveTitleTree(titleElement, bodyElement) as IHTMLDOMNode;

            //update the bodyElement to contain the editable body marker.
            string bodyTemplateText = BlogEditingTemplate.POST_BODY_MARKER;
            bodyElement.innerText = bodyTemplateText;

            //if the title parents where preserved, prepend them back into the bodyElement
            if (preservedTitleNodes != null)
            {
                IHTMLDOMNode bodyNode = (IHTMLDOMNode)bodyElement;
                IHTMLDOMNode firstNode = bodyNode.firstChild;
                if (firstNode == null)
                    bodyNode.appendChild(preservedTitleNodes);
                else
                    bodyNode.insertBefore(preservedTitleNodes, firstNode);
            }

            string templateHtml = HTMLDocumentHelper.HTMLDocToString((IHTMLDocument2)doc);
            if (templateHtml == null || templateHtml.IndexOf(BlogEditingTemplate.POST_TITLE_MARKER, StringComparison.OrdinalIgnoreCase) == -1 ||
                templateHtml.IndexOf(BlogEditingTemplate.POST_BODY_MARKER, StringComparison.OrdinalIgnoreCase) == -1)
            {
                Debug.Fail("Failed to insert blog post editing markers");
                throw new Exception("Error inserting template markers");
            }

            //prepend the doctype of the document - this prevents styles in the document from rendering improperly
            string docType = HTMLDocumentHelper.GetSpecialHeaders((IHTMLDocument2)doc).DocType;
            if (docType != null)
                templateHtml = docType + "\r\n" + templateHtml;

            return new BlogEditingTemplate(templateHtml);
        }

        /// <summary>
        /// WinLive 211555: To avoid confusing our users with warning messages, we strip any &lt;noscript&gt; and
        /// &lt;noembed&gt; elements.
        /// </summary>
        private void RemoveNoShowElements(IHTMLDocument2 doc)
        {
            IMarkupServicesRaw rawMarkupServices = (IMarkupServicesRaw)doc;
            MshtmlMarkupServices markupServices = new MshtmlMarkupServices(rawMarkupServices);
            MarkupRange bodyRange = markupServices.CreateMarkupRange(doc.body, false);
            foreach (IHTMLElement noShowElement in bodyRange.GetElements(e => e is IHTMLNoShowElement, true))
            {
                HTMLElementHelper.RemoveElement(noShowElement);
            }
        }

        /// <summary>
        /// Returns the top-most title parent node between the title element and the body element,
        /// if the specified titleElement exists within the bodyElement.
        /// </summary>
        /// <param name="titleElement"></param>
        /// <param name="bodyElement"></param>
        /// <returns></returns>
        IHTMLDOMNode MaybePreserveTitleTree(IHTMLElement titleElement, IHTMLElement bodyElement)
        {
            IHTMLElement preservedParents = titleElement;
            IHTMLElement parent = titleElement.parentElement;
            while (parent != null && bodyElement.sourceIndex != parent.sourceIndex)
            {
                preservedParents = parent;
                parent = parent.parentElement;
            }
            if (parent != null && bodyElement.sourceIndex == parent.sourceIndex)
            {
                //the title element is a child of the body element, so extract the title's parent tree
                //from the DOM and return it.
                IHTMLDOMNode preservedTitleTreeNode = (preservedParents as IHTMLDOMNode).removeNode(true);
                return preservedTitleTreeNode;
            }
            else
            {
                //the title element is not a child of the body element
                return null;
            }
        }
    }

    internal class ParentElementsOnlyEditingTemplate : BlogEditingTemplateStrategy
    {
        /// <summary>
        /// Generates a blog editing template based on the HTML in a document.
        /// </summary>
        /// <param name="doc">The full HTML document</param>
        /// <param name="titleElement">the element in the document that surrounds the post title text</param>
        /// <param name="bodyElement">the element in the document that surrounds the post body text</param>
        /// <returns></returns>
        protected internal override BlogEditingTemplate GenerateBlogTemplate(IHTMLDocument3 doc, IHTMLElement titleElement, IHTMLElement[] allTitleElements, IHTMLElement bodyElement)
        {
            // if title is containing with a link then strip the link
            CleanupContainingAnchorTag(titleElement);

            IMarkupServicesRaw rawMarkupServices = doc as IMarkupServicesRaw;
            MshtmlMarkupServices markupServices = new MshtmlMarkupServices(rawMarkupServices);
            MarkupPointer startPointer = markupServices.CreateMarkupPointer(titleElement, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin);
            MarkupPointer endPointer = markupServices.CreateMarkupPointer(bodyElement, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
            MarkupRange range = markupServices.CreateMarkupRange(startPointer, endPointer);

            IHTMLElement stopElement = range.ParentElement();
            IHTMLElement currElement;

            string titleTemplateText = BlogEditingTemplate.POST_TITLE_MARKER;
            MarkupPointer siblingPointer = markupServices.CreateMarkupPointer();

            bool preserveClear = false;
            currElement = titleElement;
            IHTMLElement2 currElement2 = (IHTMLElement2)currElement;
            while (currElement != null && currElement.sourceIndex != stopElement.sourceIndex)
            {
                titleTemplateText = WriteStartTag(currElement, null) + titleTemplateText + WriteEndTag(currElement);

                currElement2 = (IHTMLElement2)currElement;
                string styleFloat = currElement2.currentStyle.styleFloat;
                if (!String.IsNullOrEmpty(styleFloat) && !String.IsNullOrEmpty((string)currElement2.currentStyle.width))
                {
                    if (String.Compare(styleFloat, "LEFT", StringComparison.OrdinalIgnoreCase) == 0 ||
                        String.Compare(styleFloat, "RIGHT", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        preserveClear = true;
                    }
                }

                currElement = currElement.parentElement;
            }

            string bodyTemplateText = BlogEditingTemplate.POST_BODY_MARKER;

            currElement = bodyElement;

            MarkupRange currElementRange = markupServices.CreateMarkupRange();
            while (currElement != null && currElement.sourceIndex != stopElement.sourceIndex)
            {
                // Then we need to look for and preserve siblings with "clear" attribute...
                IHTMLElement parentElement = currElement.parentElement;
                if (preserveClear && parentElement != null)
                {
                    IHTMLElementCollection siblings = (IHTMLElementCollection)parentElement.children;
                    foreach (IHTMLElement sibling in siblings)
                    {
                        if (sibling.sourceIndex == currElement.sourceIndex)
                            continue;

                        siblingPointer.MoveAdjacentToElement(sibling, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);

                        // Does this sibling end before the current element starts?
                        currElementRange.MoveToElement(currElement, true);
                        if (siblingPointer.IsLeftOfOrEqualTo(currElementRange.Start))
                        {
                            IHTMLElement2 sibling2 = (IHTMLElement2)sibling;
                            string styleClear = sibling2.currentStyle.clear;
                            if (!String.IsNullOrEmpty(styleClear) && String.Compare(styleClear, "NONE", StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                // Then preserve the clear...
                                titleTemplateText = titleTemplateText +
                                                    WriteStartTag(sibling, String.Format(@"clear: {0}", styleClear)) +
                                                    WriteEndTag(sibling);
                            }
                        }
                    }
                }

                bodyTemplateText = WriteStartTag(currElement, null) + bodyTemplateText + WriteEndTag(currElement);
                currElement = currElement.parentElement;
            }

            string templateHtml = titleTemplateText + bodyTemplateText;
            currElement = range.ParentElement();
            while (currElement != null)
            {
                if (currElement.tagName == "HTML")
                {
                    MarkupPointer bodyPointer = markupServices.CreateMarkupPointer(((IHTMLDocument2)doc).body, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
                    MarkupPointer docPointer = markupServices.CreateMarkupPointer(currElement, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin);
                    MarkupRange headRange = markupServices.CreateMarkupRange(docPointer, bodyPointer);
                    IHTMLElement[] elements = headRange.GetTopLevelElements(new IHTMLElementFilter(IsHeadElement));
                    if (elements.Length > 0)
                    {
                        string head = elements[0].innerHTML;
                        //string defaultStyles = "<style>p, h1, h2, h3, h4, h5, h6, blockquote, pre{ padding-top: 1px; }</style>";
                        head = String.Format(CultureInfo.InvariantCulture, "<head>{0}</head>", head);
                        templateHtml = head + templateHtml;
                    }
                }
                templateHtml = WriteStartTag(currElement, null) + templateHtml + WriteEndTag(currElement);
                currElement = currElement.parentElement;
            }

            //prepend the doctype of the document - this prevents styles in the document from rendering improperly
            string docType = HTMLDocumentHelper.GetSpecialHeaders((IHTMLDocument2)doc).DocType;
            //string docType = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\"[]>";
            if (docType != null)
                templateHtml = docType + "\r\n" + templateHtml;

            return new BlogEditingTemplate(templateHtml);
        }

        private string WriteStartTag(IHTMLElement element, string style)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<");
            sb.Append(element.tagName);

            if (style != null && style != String.Empty)
            {
                sb.AppendFormat(" style=\"{0}\"", style);
            }

            IHTMLDOMNode node = (IHTMLDOMNode)element;
            IHTMLAttributeCollection attrs = node.attributes as IHTMLAttributeCollection;
            if (attrs != null)
            {
                foreach (IHTMLDOMAttribute attr in attrs)
                {
                    string attrName = attr.nodeName as string;
                    if (attr.specified)
                    {
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
                                string upperAttrName = attrName.ToUpperInvariant();
                                if (upperAttrName == "COLSPAN")
                                {
                                    //avoid bug with getting the attr value for column span elements
                                    attrValue = (element as IHTMLTableCell).colSpan.ToString(CultureInfo.InvariantCulture);
                                }
                                else if (upperAttrName == "STYLE")
                                {
                                    //the style attribute value cannot be extracted as a string, so grab its CSS text instead
                                    attrValue = element.style.cssText;
                                }
                            }
                        }

                        if (attrName != null && attrValue != null)
                        {
                            //write out this attribute.
                            sb.AppendFormat(" {0}=\"{1}\"", attrName, attrValue);
                        }
                    }
                }
            }
            sb.Append(">");
            string startTag = sb.ToString();
            return startTag;
        }

        private string WriteEndTag(IHTMLElement element)
        {
            return String.Format(CultureInfo.InvariantCulture, "</{0}>", element.tagName);
        }

        private bool IsHeadElement(IHTMLElement e)
        {
            return e.tagName == "HEAD";
        }

        /// <summary>
        /// Fixes up the CSS styles of in the template to make them editor safe.
        /// </summary>
        /// <param name="blogTemplateFile"></param>
        /// <param name="storage"></param>
        /// <param name="supportingFilesDir"></param>
        protected internal override void FixupDownloadedFiles(string blogTemplateFile, FileBasedSiteStorage storage, string supportingFilesDir)
        {
            //Fix up the downloaded files to remove position:relative CSS settings (fixes bugs 244951, 287589, 287556)
            string supportingFilesPath = Path.Combine(storage.BasePath, supportingFilesDir);
            if (Directory.Exists(supportingFilesPath))
            {
                string[] files = Directory.GetFiles(supportingFilesPath);
                foreach (string file in files)
                {
                    if (file.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
                    {
                        string cssPath = Path.Combine(supportingFilesPath, file);
                        string newCssContent = "";
                        using (StreamReader sr = new StreamReader(cssPath))
                        {
                            string cssContent = sr.ReadToEnd();
                            newCssContent = new LightWeightCSSReplacer(cssContent).DoReplace();
                        }

                        using (StreamWriter sw = new StreamWriter(cssPath, false, Encoding.UTF8))
                        {
                            sw.Write(newCssContent);
                        }
                    }
                }
            }

            //fix up the styles in the HTML document
            string templateFilePath = Path.Combine(supportingFilesPath, blogTemplateFile);
            string newTemplateContent = "";
            using (StreamReader sr = new StreamReader(templateFilePath))
            {
                string docContent = sr.ReadToEnd();
                newTemplateContent = new LightWeightDocumentCssReplacer(docContent).DoReplace();
            }

            using (StreamWriter sw = new StreamWriter(templateFilePath, false, Encoding.UTF8))
            {
                sw.Write(newTemplateContent);
            }
        }
    }

    /// <summary>
    /// Summary description for LightWeightDocumentCssReplacer.
    /// </summary>
    internal class LightWeightDocumentCssReplacer : LightWeightHTMLDocumentIterator
    {
        public LightWeightDocumentCssReplacer(string html)
            : base(html)
        {

        }

        public string DoReplace()
        {
            Parse();
            return _docBuilder.ToString();
        }

        protected override void DefaultAction(Element el)
        {
            base.DefaultAction(el);
            _docBuilder.Append(el.ToString());
        }

        protected override void OnStyleText(StyleText styleText)
        {
            string text = styleText.ToString();
            text = LightWeightCSSReplacer.ReplacePositionStyle(text);
            _docBuilder.Append(text);
        }

        private StringBuilder _docBuilder = new StringBuilder();
    }

    /// <summary>
    /// Summary description for LightWeightCSSReplacer.
    /// </summary>
    internal class LightWeightCSSReplacer : LightweightCSSIterator
    {
        public LightWeightCSSReplacer(string css)
            : base(css)
        {
        }

        public string DoReplace()
        {
            Parse();
            return _cssBuilder.ToString();
        }
        private StringBuilder _cssBuilder = new StringBuilder();

        protected override void OnStyleComment(StyleComment styleComment)
        {
            Emit(styleComment);
            base.OnStyleComment(styleComment);
        }

        protected override void OnStyleLiteral(StyleLiteral styleLiteral)
        {
            Emit(styleLiteral);
            base.OnStyleLiteral(styleLiteral);
        }

        protected override void OnStyleText(StyleText styleText)
        {
            string text = styleText.ToString();
            text = ReplacePositionStyle(text);
            _cssBuilder.Append(text);
        }

        /// <summary>
        /// Replaces style positioning rules with "inherit".
        /// This is required to avoid mouse offset issues that occur with blocks that use
        /// relative positioning.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        internal static string ReplacePositionStyle(string text)
        {
            // TODO: Stop messing with the CSS and instead override the position attribute at runtime so we can
            // apply this fix to only block elements.

            StringBuilder sb = new StringBuilder();
            int startIndex = 0;
            foreach (Match match in position.Matches(text))
            {
                Group g = match.Groups["position"];
                if (g.Success)
                {
                    foreach (Capture c in g.Captures)
                    {
                        string textUpToCapture = text.Substring(startIndex, c.Index - startIndex);
                        if (ShouldReplacePositionStyle(text, startIndex, textUpToCapture))
                        {
                            sb.Append(textUpToCapture);
                            sb.Append("inherit");
                            startIndex = c.Index + c.Length;
                        }
                    }
                }
            }
            sb.Append(text.Substring(startIndex));
            string newText = sb.ToString();
            return newText;
        }

        private static bool ShouldReplacePositionStyle(string text, int startIndex, string textUpToCapture)
        {
            // WinLive 269001: The default theme for Wordpress 3.0 (TwentyTen) has the following CSS rule:
            //
            //      ...
            //  }
            //
            //  sup,
            //  sub {
            //      height: 0;
            //      line-height: 1;
            //      position: relative;
            //      vertical-align: baseline;
            //  }
            //
            // But when we replace "position: relative;" with "position: inherit", superscript doesn't
            // display correctly. We'll check to make sure we don't replace the position in this case.

            int currentOpeningParenthesis = textUpToCapture.LastIndexOf('{');
            if (currentOpeningParenthesis > -1)
            {
                int previousClosingParenthesis = textUpToCapture.LastIndexOf('}');
                if (previousClosingParenthesis < 0)
                {
                    // There may not be a closing parenthesis included in this text so fall back to the
                    // start index. We'll add back in the 1 that we subtract here.
                    previousClosingParenthesis = startIndex - 1;
                }

                //      ...
                //  } /* <-- previousClosingParenthesis */
                //
                //  sup,
                //  sub { /* <-- currentOpeningParenthesis */
                //      ...
                //      position: relative;
                //      ...
                //  }
                if (previousClosingParenthesis < currentOpeningParenthesis)
                {
                    int afterPreviousClosingParenthesis = previousClosingParenthesis + 1;
                    int untilCurrentOpeningParenthesis = currentOpeningParenthesis - afterPreviousClosingParenthesis;

                    Match superScriptMatch = superScriptCssSelector.Match(text, afterPreviousClosingParenthesis, untilCurrentOpeningParenthesis);
                    if (superScriptMatch.Success &&
                        superScriptMatch.Index == afterPreviousClosingParenthesis &&
                        superScriptMatch.Length == untilCurrentOpeningParenthesis)
                    {
                        // Allow this position style to stay the same.
                        return false;
                    }
                }
            }

            return true;
        }

        private static Regex position = new Regex(@"(\s|{|;)position\s*\:\s*(?<position>[a-z]*)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        private static Regex superScriptCssSelector = new Regex(@"(\s*(sub,?|sup,?)\s*)+", RegexOptions.Compiled);

        protected override void OnStyleImport(StyleImport styleImport)
        {
            Emit(styleImport);
            base.OnStyleImport(styleImport);
        }

        protected override void OnStyleUrl(StyleUrl styleUrl)
        {
            Emit(styleUrl);
            base.OnStyleUrl(styleUrl);
        }

        private void Emit(StyleElement styleElement)
        {
            if (styleElement != null)
                _cssBuilder.Append(styleElement.ToString());
        }
    }
}
