// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Web;
using mshtml;
using OpenLiveWriter.CoreServices.Progress;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for HTMLThinner.
    /// </summary>
    public class HTMLThinner
    {
        private class TickableProgressTick : ProgressTick
        {
            public TickableProgressTick(IProgressHost progress, int totalTicks) : base(progress, 100, 100)
            {
                _totalTicks = totalTicks;
            }
            private int _totalTicks = -1;
            private int _currentTicks = 0;

            public void Tick()
            {
                this.UpdateProgress(Math.Min(_currentTicks++,_totalTicks), _totalTicks);
            }
        }

        public static string Thin(IHTMLElement startElement)
        {
            return Thin(startElement, false, SilentProgressHost.Instance);
        }

        public static string Thin(IHTMLElement startElement, IProgressHost progressHost)
        {
            return Thin(startElement, false, progressHost);
        }

        public static string Thin(IHTMLElement startElement, bool preserveImages)
        {
            return Thin(startElement, preserveImages, SilentProgressHost.Instance);
        }

        public static string Thin(IHTMLElement startElement, bool preserveImages, IProgressHost progressHost)
        {
            StringBuilder escapedText = new StringBuilder();
            if (startElement != null)
            {
                IHTMLElementCollection elements = (IHTMLElementCollection)startElement.all;
                TickableProgressTick progress = new TickableProgressTick(progressHost, elements.length + 1);
                IHTMLDOMNode startNode = (IHTMLDOMNode) startElement;
                StripChildNodes(startNode, escapedText, preserveImages, progress);
            }
            return escapedText.ToString();
        }

        /// <summary>
        /// Used as a part of HTML thinning to remove extraneous child nodes from an HTMLDOMNode
        /// </summary>
        /// <param name="node">The node whose children should be stripped</param>
        /// <returns>An HTML string with the DOMNodes cleaned out</returns>
        private static void StripChildNodes(IHTMLDOMNode node, StringBuilder escapedText, bool preserveImages, TickableProgressTick progress)
        {

            // is this a text node?  If so, just get the text and return it
            if (node.nodeType == HTMLDocumentHelper.HTMLDOMNodeTypes.TextNode)
                escapedText.Append(HttpUtility.HtmlEncode(node.nodeValue.ToString()));
            else
            {
                progress.Tick();
                bool tagStillOpen = false;
                ArrayList preserveTags = PreserveTags;
                if (preserveImages)
                    preserveTags = PreserveTagsWithImages;

                // if we're in an element node (a tag) and we should preserve the tag,
                // append it to the returned text
                if (preserveTags.Contains(node.nodeName))
                {
                    // Append the opening tag element, with any extraneous
                    // attributes stripped
                    escapedText.Append("<" + node.nodeName);
                    StripAttributes((IHTMLElement)node, escapedText);

                    // if the element has no children, we can simply close out the tag
                    if (!node.hasChildNodes())
                    {
                        if (node.nodeName == HTMLTokens.IFrame)
                            escapedText.Append("></" + node.nodeName + ">");
                        else
                            escapedText.Append("/>");
                    }
                    else // the element has children, leave the tag open
                    {
                        escapedText.Append(">");
                        tagStillOpen = true;
                    }
                }
                else if (ReplaceTags.Contains(node.nodeName))
                {
                    // If there are no children, just emit the replacement tag
                    if (!node.hasChildNodes())
                    {
                        // Replace the tag
                        escapedText.Append("<" + (string)ReplaceTags[node.nodeName] + "/>");
                    }
                    else
                    {
                        if (!IsChildlessTag((string)ReplaceTags[node.nodeName]))
                        {
                            escapedText.Append("<" + (string)ReplaceTags[node.nodeName] + ">");
                        }
                        // Since there are children, we're going to emit the replacement
                        // tag at the end of this node
                        tagStillOpen = true;
                    }
                }

                if (node.firstChild != null)
                {
                    StripChildNodes(node.firstChild, escapedText, preserveImages, progress);
                }

                // put a closing tag in for the current element (because we left it open in case of children)
                if (tagStillOpen)
                {
                    if (PreserveTags.Contains(node.nodeName))
                        escapedText.Append("</" + node.nodeName + ">");
                    else if (ReplaceTags.Contains(node.nodeName))
                    {
                        if (!IsChildlessTag((string)ReplaceTags[node.nodeName]))
                            escapedText.Append("</" + (string)ReplaceTags[node.nodeName] + ">");
                        else
                            escapedText.Append("<" + (string)ReplaceTags[node.nodeName] + "/>");
                    }
                }
            }

            if (node.nextSibling != null)
            {
                StripChildNodes(node.nextSibling, escapedText, preserveImages, progress);
            }
        }

        /// <summary>
        /// Remove any extraneous attributes from an Attribute Collection
        /// </summary>
        /// <param name="attributes">The attribute collection</param>
        /// <returns>a string representing the attributes with extraneous attributes removed</returns>
        public static void StripAttributes(IHTMLElement element, StringBuilder escapedText)
        {
            foreach (string attr in PreserveAttributes)
            {

                object attrObject = element.getAttribute(attr, 2); //note: use 2 as param to get pure attr value
                string attrValue = null;
                if (attrObject != null)
                    attrValue = attrObject.ToString();

                if (attrValue != null && attrValue != string.Empty)
                {
                    escapedText.Append(" " + attr.ToUpper(CultureInfo.InvariantCulture) + "=\"" + attrValue + "\"");
                }
            }
        }

        private static bool IsChildlessTag(string tagName)
        {
            return tagName == HTMLTokens.Br;
        }

        /// <summary>
        /// The list of tags that will be preserved when the HTML is thinned.
        /// </summary>
        private static ArrayList PreserveTags
        {
            get
            {
                if (m_preserveTags == null)
                {
                    m_preserveTags = new ArrayList();
                    m_preserveTags.Add(HTMLTokens.A);
                    m_preserveTags.Add(HTMLTokens.P);
                    m_preserveTags.Add(HTMLTokens.Br);
                    m_preserveTags.Add(HTMLTokens.Form);
                    m_preserveTags.Add(HTMLTokens.Input);
                    m_preserveTags.Add(HTMLTokens.Select);
                    m_preserveTags.Add(HTMLTokens.Option);
                    m_preserveTags.Add(HTMLTokens.Ilayer);
                    m_preserveTags.Add(HTMLTokens.Meta);
                    m_preserveTags.Add(HTMLTokens.Head);
                    m_preserveTags.Add(HTMLTokens.Body);
                    m_preserveTags.Add(HTMLTokens.Div);
                    m_preserveTags.Add(HTMLTokens.IFrame);
                    m_preserveTags.Add(HTMLTokens.Pre);
                }
                return m_preserveTags;
            }
        }
        private static ArrayList m_preserveTags;

        private static ArrayList PreserveTagsWithImages
        {
            get
            {
                if (m_preserveTagsWithImages == null)
                {
                    m_preserveTagsWithImages = new ArrayList();
                    foreach (string s in PreserveTags)
                        m_preserveTagsWithImages.Add(s);
                    m_preserveTagsWithImages.Add(HTMLTokens.Img);
                }
                return m_preserveTagsWithImages;
            }
        }
        private static ArrayList m_preserveTagsWithImages;

        /// <summary>
        /// A table of tags and their replacements when thinning
        /// </summary>
        private static Hashtable ReplaceTags
        {
            get
            {
                if (m_replaceTags == null)
                {
                    m_replaceTags = new Hashtable();
                    m_replaceTags.Add(HTMLTokens.H1, HTMLTokens.P);
                    m_replaceTags.Add(HTMLTokens.H2, HTMLTokens.P);
                    m_replaceTags.Add(HTMLTokens.H3, HTMLTokens.P);
                    m_replaceTags.Add(HTMLTokens.H4, HTMLTokens.P);
                    m_replaceTags.Add(HTMLTokens.H5, HTMLTokens.P);
                    m_replaceTags.Add(HTMLTokens.H6, HTMLTokens.P);
                    m_replaceTags.Add(HTMLTokens.Hr, HTMLTokens.P);
                    m_replaceTags.Add(HTMLTokens.Li, HTMLTokens.Br);
                    m_replaceTags.Add(HTMLTokens.Dt, HTMLTokens.Br);
                    m_replaceTags.Add(HTMLTokens.Dd, HTMLTokens.Br);
                    m_replaceTags.Add(HTMLTokens.Menu, HTMLTokens.P);
                    m_replaceTags.Add(HTMLTokens.Ul, HTMLTokens.P);
                    m_replaceTags.Add(HTMLTokens.Ol, HTMLTokens.P);
                    m_replaceTags.Add(HTMLTokens.Dir, HTMLTokens.P);
                    m_replaceTags.Add(HTMLTokens.Dl, HTMLTokens.P);
                    m_replaceTags.Add(HTMLTokens.Blockquote, HTMLTokens.P);

                }
                return m_replaceTags;
            }
        }
        private static Hashtable m_replaceTags;

        /// <summary>
        /// The list of attributes that should be preserved
        /// </summary>
        private static ArrayList PreserveAttributes
        {
            get
            {
                if (m_preserveAttributes == null)
                {
                     m_preserveAttributes = new ArrayList(
                        new string[]{HTMLTokens.Href,HTMLTokens.Name,HTMLTokens.Value,HTMLTokens.Action,HTMLTokens.Method,
                                        HTMLTokens.Enctype,HTMLTokens.Size,HTMLTokens.Type,HTMLTokens.Src, HTMLTokens.Content, HTMLTokens.HttpEquiv,
                                        HTMLTokens.Height, HTMLTokens.Width, HTMLTokens.Alt});
                }
                return m_preserveAttributes;
            }
        }
        private static ArrayList m_preserveAttributes;

    }
}
