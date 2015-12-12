// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using mshtml;
using OpenLiveWriter.Api;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators;
using OpenLiveWriter.PostEditor.PostHtmlEditing.Sidebar;
using OpenLiveWriter.PostEditor.Video;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.PostEditor.ContentSources
{

    class SmartContentInsertionHelper
    {
        private SmartContentInsertionHelper()
        {
        }

        /// <summary>
        /// Clones active smart content contained in the provided HTML, and disables unknown smart content.
        /// </summary>
        public static string PrepareSmartContentHtmlForEditorInsertion(string html, IContentSourceSidebarContext sourceContext)
        {
            StringBuilder output = new StringBuilder();
            ContentSourceManager.SmartContentPredicate predicate = new ContentSourceManager.SmartContentPredicate();
            SimpleHtmlParser p = new SimpleHtmlParser(html);
            for (Element el; null != (el = p.Next());)
            {
                if (predicate.IsMatch(el))
                {
                    BeginTag bt = el as BeginTag;
                    Attr idAttr = bt.GetAttribute("id");

                    String contentSourceId, contentItemId;
                    ContentSourceManager.ParseContainingElementId(idAttr.Value, out contentSourceId, out contentItemId);
                    ISmartContent smartContent = sourceContext.FindSmartContent(contentItemId);
                    if (smartContent != null)
                    {
                        String newId = Guid.NewGuid().ToString();
                        sourceContext.CloneSmartContent(contentItemId, newId);

                        if (RefreshableContentManager.ContentSourcesWithRefreshableContent.Contains(contentSourceId))
                        {
                            IExtensionData extensionData = sourceContext.FindExtentsionData(newId);
                            Debug.Assert(extensionData != null);

                            // Since we just made a new id for the smart content just about to be inserted
                            // we want to give it a chance to get a callback because its callback might have happened while
                            // it was on the clipboard(in the event of cut).  This means the refreshable content manager doesnt know
                            // to watch out for this smart content on paste, it only knows to look out for who created it.   Thus
                            // we just force the callback, and if it didnt need it, nothing will happen.
                            if (extensionData.RefreshCallBack == null)
                            {
                                extensionData.RefreshCallBack = DateTime.UtcNow;
                            }
                        }

                        idAttr.Value = ContentSourceManager.MakeContainingElementId(contentSourceId, newId);
                    }
                    else
                    {
                        ContentSourceManager.RemoveSmartContentAttributes(bt);
                    }
                }
                output.Append(el.ToString());
            }
            return output.ToString();
        }

        public static void InsertEditorHtmlIntoElement(IContentSourceSidebarContext contentSourceContext, SmartContentSource source, ISmartContent sContent, IHTMLElement element)
        {
            string content = source.GenerateEditorHtml(sContent, contentSourceContext);

            // If the plugin returned null has the HTML it would like to insert, remove the element from the editor
            if (content == null)
                HTMLElementHelper.RemoveElement(element);
            else
                InsertContentIntoElement(content, sContent, contentSourceContext, element);
        }

        public static void InsertContentIntoElement(string content, ISmartContent sContent, IContentSourceSidebarContext contentSourceContext, IHTMLElement element)
        {
            MshtmlMarkupServices MarkupServices = new MshtmlMarkupServices((IMarkupServicesRaw)element.document);

            //Note: undo/redo disabled for smart content since undo causes the HTML to get out of sync
            //with the inserter's settings state, so undo changes will be blown away the next time the
            //the inserter's HTML is regenerated.  Also note that making this insertion without wrapping it
            //in an undo clears the undo/redo stack, which is what we want for beta.
            //string undoId = Guid.NewGuid().ToString();

            MarkupRange htmlRange = MarkupServices.CreateMarkupRange(element, false);
            htmlRange.Start.PushCling(true);
            htmlRange.End.PushCling(true);
            MarkupServices.Remove(htmlRange.Start, htmlRange.End);
            htmlRange.Start.PopCling();
            htmlRange.End.PopCling();

            element.style.padding = ToPaddingString(sContent.Layout);

            if (sContent.Layout.Alignment == Alignment.None
                || sContent.Layout.Alignment == Alignment.Right
                || sContent.Layout.Alignment == Alignment.Left)
            {
                element.style.display = "inline";
                element.style.marginLeft = "0px";
                element.style.marginRight = "0px";
                element.style.styleFloat = sContent.Layout.Alignment.ToString().ToLower(CultureInfo.InvariantCulture);
            }
            else if (sContent.Layout.Alignment == Alignment.Center)
            {
                element.style.styleFloat = Alignment.None.ToString().ToLower(CultureInfo.InvariantCulture);
                element.style.display = "block";
                element.style.marginLeft = "auto";
                element.style.marginRight = "auto";
            }

            // Clear out any width on the overall smart content block, if the element is centered, we will add the width back in later
            // after we calcuate it from the childern, the current width value is stale.
            element.style.width = "";

            //Note: we use MarkupServices to insert the content so that IE doesn't try to fix up URLs.
            //Element.insertAdjacentHTML() is a no-no because it rewrites relaive URLs to include
            //the fullpath from the local filesytem.

            //MarkupServices.ParseString() doesn't attempt to fix up URLs, so its safe to use.
            //We will now stage the new content into a MarkupContainer, and then move it into
            //the working document.
            MarkupPointer sc1 = MarkupServices.CreateMarkupPointer();
            MarkupPointer sc2 = MarkupServices.CreateMarkupPointer();

            //Create a temporary document from the html and set the start/end pointers to the
            //start and end of the document.
            MarkupServices.ParseString(content, sc1, sc2);
            IHTMLDocument2 doc = sc1.GetDocument();
            MarkupRange stagingRange = MarkupServices.CreateMarkupRange(sc1, sc2);
            stagingRange.MoveToElement(doc.body, false);

            //IE7 hack: fixes bug 305512.  Note that this will destroy the inner content of the element,
            //so make sure it is called before the refreshed content is inserted.
            BeforeInsertInvalidateHackForIE7(element);

            //move the content from the staging area into the actual insertion point.
            MarkupServices.Move(stagingRange.Start, stagingRange.End, htmlRange.End);

            if (sContent.Layout.Alignment == Alignment.Center)
            {
                MarkupContext mc = htmlRange.End.Right(false);
                MarkupRange range = MarkupServices.CreateMarkupRange(mc.Element, false);

                IHTMLElement[] childern = range.GetTopLevelElements(MarkupRange.FilterNone);

                int maxWidth = 0;
                foreach (IHTMLElement child in childern)
                    maxWidth = Math.Max(maxWidth, child.offsetWidth);

                if (maxWidth != 0)
                    mc.Element.style.width = maxWidth;
            }

            // Let the context provider know the smart content was edited.
            string contentSourceId, contentId;
            ContentSourceManager.ParseContainingElementId(element.id, out contentSourceId, out contentId);
            contentSourceContext.OnSmartContentEdited(contentId);
        }

        public static string GenerateContentBlock(string contentSourceId, string blockId, string content, IExtensionData exData)
        {
            return SmartContentInsertionHelper.GenerateContentBlock(contentSourceId, blockId, content, (ISmartContent)new SmartContent(exData), null);
        }
        public static string GenerateContentBlock(string contentSourceId, string blockId, string content, ISmartContent sContent)
        {
            return SmartContentInsertionHelper.GenerateContentBlock(contentSourceId, blockId, content, sContent, null);
        }
        public static string GenerateContentBlock(string contentSourceId, string blockId, string content, ISmartContent sContent, IHTMLElement element)
        {
            string className = ContentSourceManager.EDITABLE_SMART_CONTENT;
            string elementId = ContentSourceManager.MakeContainingElementId(contentSourceId, blockId);
            bool inline = true;

            return GenerateBlock(className, elementId, sContent, inline, content, false, element);
        }

        public static string GenerateBlock(string className, string elementId, ISmartContent sContent, bool displayInline, string content, bool noFloat, IHTMLElement element)
        {
            if (string.IsNullOrEmpty(content))
                return "";

            // generate the html to insert
            StringBuilder htmlBuilder = new StringBuilder();

            htmlBuilder.AppendFormat("<div class=\"{0}\"", className);
            if (!string.IsNullOrEmpty(elementId))
                htmlBuilder.AppendFormat(" id=\"{0}\"", elementId);

            if (element != null)
            {
                // Persist the language direction of the smart content if it's explicitly set.
                string currentDirection = element.getAttribute("dir", 2) as string;
                if (!String.IsNullOrEmpty(currentDirection))
                {
                    htmlBuilder.AppendFormat(" dir=\"{0}\"", currentDirection);
                }
            }

            StringBuilder styleBuilder = new StringBuilder();

            if (sContent.Layout.Alignment == Alignment.None
               || sContent.Layout.Alignment == Alignment.Right
               || sContent.Layout.Alignment == Alignment.Left)
            {
                // If the smart content is none/right/left we just use float
                if (!noFloat || sContent.Layout.Alignment != Alignment.None)
                {
                    AppendStyle(noFloat ? "text-align" : "float",
                                sContent.Layout.Alignment.ToString().ToLower(CultureInfo.InvariantCulture),
                                styleBuilder);
                }
            }
            else if (element != null && sContent.Layout.Alignment == Alignment.Center)
            {
                // If the alignment is centered then it needs to make sure float is set to none
                AppendStyle("float", "none", styleBuilder);
                AppendStyle("display", "block", styleBuilder);
                AppendStyle("margin-left", "auto", styleBuilder);
                AppendStyle("margin-right", "auto", styleBuilder);
                AppendStyle("width", element.offsetWidth.ToString(CultureInfo.InvariantCulture), styleBuilder);
            }

            if (displayInline && sContent.Layout.Alignment != Alignment.Center)
                AppendStyle("display", "inline", styleBuilder);

            if (sContent.Layout.Alignment != Alignment.Center)
                AppendStyle("margin", "0px", styleBuilder);

            AppendStyle("padding", ToPaddingString(sContent.Layout), styleBuilder);

            if (styleBuilder.Length > 0)
                htmlBuilder.AppendFormat(" style=\"{0}\"", styleBuilder.ToString());

            htmlBuilder.AppendFormat(">{0}</div>", content);

            return htmlBuilder.ToString();
        }

        public static bool ContainsUnbalancedDivs(string html)
        {
            int tags = 0;
            SimpleHtmlParser p = new SimpleHtmlParser(html);
            for (Element e; (e = p.Next()) != null;)
            {
                if (e is Tag && ((Tag)e).NameEquals("div"))
                {
                    if (e is BeginTag)
                        ++tags;
                    else
                        --tags;
                }
            }

            return tags != 0;
        }

        private static string ToPaddingString(ILayoutStyle layoutStyle)
        {
            return String.Format(CultureInfo.InvariantCulture,
                                 "{0}px {1}px {2}px {3}px",
                                 layoutStyle.TopMargin,
                                 layoutStyle.RightMargin,
                                 layoutStyle.BottomMargin,
                                 layoutStyle.LeftMargin);
        }

        private static void AppendStyle(string name, string val, StringBuilder sb)
        {
            if (sb.Length > 0)
                sb.Append(" ");
            sb.AppendFormat("{0}:{1};", name, val);
        }

        /// <summary>
        /// Forces IE 7 to redraw the new contents of the element.
        /// </summary>
        /// <param name="e"></param>
        private static void BeforeInsertInvalidateHackForIE7(IHTMLElement e)
        {
            //Fixes bug 305512.
            //IE 7 (beta3) has a bad habit of not redrawing the updated HTML if the width
            //of the content box has increased while the editor is not focused. Investigation
            //has found that setting the innerText is at least one way to force the editor
            //to refresh the painting of the element.
            //TODO: after IE7 goes final, check to see if this hack is still necessary.
            e.innerText = "";
        }

        // Warning: Does not deal with escaping properly. This is fine as long as
        // we're only using it for content we generate and there are no security
        // impliciations.
        public static string StripDivsWithClass(string html, string cssClass)
        {
            if (html.IndexOf(cssClass) < 0)
                return html;

            StringBuilder sb = new StringBuilder();
            HtmlExtractor ex = new HtmlExtractor(html);
            int pos = 0;
            while (ex.Seek("<div class='" + cssClass + "'>").Success)
            {
                sb.Append(html, pos, ex.Element.Offset - pos);
                ex.Parser.CollectHtmlUntil("div");
                pos = ex.Parser.Position;
            }
            sb.Append(html, pos, html.Length - pos);
            return sb.ToString();
        }
    }
}
