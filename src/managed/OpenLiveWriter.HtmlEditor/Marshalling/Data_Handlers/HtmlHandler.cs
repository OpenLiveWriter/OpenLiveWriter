// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.HTML;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor.Marshalling.Data_Handlers
{
    public class HtmlHandler : FreeTextHandler
    {
        public HtmlHandler(DataObjectMeister dataObject, DataFormatHandlerContext handlerContext, IHtmlMarshallingTarget editorContext)
            : base(dataObject, handlerContext, editorContext)
        {
        }
        /// <summary>
        /// Is there URL data in the passed data object?
        /// </summary>
        /// <param name="data">data</param>
        /// <returns>true if there is url data, else false</returns>
        public static bool CanCreateFrom(DataObjectMeister data)
        {
            return data.HTMLData != null;
        }

        private static bool IsOfficeHtml(HTMLData data)
        {
            string generator = data.HTMLMetaData.Generator;
            if (String.IsNullOrEmpty(generator))
                return false;
            return
                (generator.StartsWith("Microsoft Word") || generator.StartsWith("Microsoft Excel") ||
                 generator.StartsWith("Microsoft PowerPoint"));
        }

        /// <summary>
        /// Grabs HTML copied in the clipboard and pastes it into the document (pulls in a copy of embedded content too)
        /// </summary>
        protected override bool DoInsertData(DataAction action, MarkupPointer begin, MarkupPointer end)
        {
            using (new WaitCursor())
            {
                try
                {
                    string baseUrl = UrlHelper.GetBasePathUrl(DataMeister.HTMLData.SourceURL);
                    string html = DataMeister.HTMLData.HTMLSelection;

                    //Check to see if the selection has an incomplete unordered list
                    var finder = new IncompleteListFinder(html);
                    finder.Parse();

                    if ((!EditorContext.CleanHtmlOnPaste) || finder.HasIncompleteList)
                    {
                        using (IUndoUnit undoUnit = EditorContext.CreateInvisibleUndoUnit())
                        {
                            // Create a new MarkupContainer off of EditorContext's document that contains the source HTML
                            // with comments marking the start and end selection.
                            MarkupContainer sourceContainer = EditorContext.MarkupServices.ParseString(DataMeister.HTMLData.HTMLWithMarkers);

                            // MSHTML's ParseString implementation clears all the attributes on the <body> element, so we
                            // have to manually add them back in.
                            CopyBodyAttributes(DataMeister.HTMLData.HTMLWithMarkers, sourceContainer.Document.body);

                            MarkupRange sourceRange = FindMarkedFragment(sourceContainer.Document,
                                HTMLDataObject.START_FRAGMENT_MARKER, HTMLDataObject.END_FRAGMENT_MARKER);
                            MshtmlMarkupServices sourceContainerMarkupServices =
                                new MshtmlMarkupServices((IMarkupServicesRaw)sourceContainer.Document);

                            // Some applications may not add the correct fragment markers (e.g. copying from Fiddler from
                            // the Web Sessions view). We'll just select the entire <body> of the clipboard in this case.
                            if (sourceRange == null)
                            {
                                sourceRange = sourceContainerMarkupServices.CreateMarkupRange(sourceContainer.Document.body, false);
                            }
                            else
                            {
                                // Make sure that we don't try to copy just parts of a table/list. We need to include the
                                // parent table/list.
                                if (!EditorContext.CleanHtmlOnPaste)
                                {
                                    ExpandToIncludeTables(sourceRange, sourceContainerMarkupServices);
                                }
                                ExpandToIncludeLists(sourceRange, sourceContainerMarkupServices);
                            }

                            if (sourceRange != null)
                            {
                                if (!EditorContext.CleanHtmlOnPaste)
                                {
                                    // WinLive 273280: Alignment on a table acts like a float, which can throw off the layout of the rest of
                                    // the document. If there is nothing before or after the table, then we can safely remove the alignment.
                                    RemoveAlignmentIfSingleTable(sourceRange);

                                    // Serialize the source HTML to a string while keeping the source formatting.
                                    MarkupRange destinationRange = EditorContext.MarkupServices.CreateMarkupRange(begin.Clone(), end.Clone());
                                    html = KeepSourceFormatting(sourceRange, destinationRange);
                                }
                                else
                                {
                                    // Normalize the HTML to ensure elements with optional end tags have explicit closing tags.
                                    // MSHTML may serialize HTML without closing tags for elements like <p>, which can cause
                                    // issues when the HTML is processed further or displayed in other contexts.
                                    html = HtmlUtils.NormalizeHtmlClosingTags(sourceRange.HtmlText);
                                }
                            }

                            undoUnit.Commit();
                        }

                        Trace.Assert(html != null, "Inline source CSS failed!");
                    }

                    if (html == null)
                    {
                        html = DataMeister.HTMLData.HTMLSelection;
                    }

                    if (IsPasteFromSharedCanvas(DataMeister))
                    {
                        if (action == DataAction.Copy)
                        {
                            // WinLive 96840 - Copying and pasting images within shared canvas should persist source
                            // decorator settings. "wlCopySrcUrl" is inserted while copy/pasting within canvas.
                            html = EditorContext.FixImageReferences(ImageCopyFixupHelper.FixupSourceUrlForCopy(html),
                                                                    DataMeister.HTMLData.SourceURL);
                        }
                    }
                    else
                    {
                        html = EditorContext.FixImageReferences(html, DataMeister.HTMLData.SourceURL);

                        HtmlCleanupRule cleanupRule = HtmlCleanupRule.Normal;
                        if (IsOfficeHtml(DataMeister.HTMLData))
                            cleanupRule = HtmlCleanupRule.PreserveTables;

                        // In Mail, we want to preserve the style of the html that is on the clipboard
                        // Whereas in Writer we by default want to remove formatting so it looks like your blog theme
                        if (EditorContext.CleanHtmlOnPaste)
                        {
                            // optionally cleanup the html
                            html = EditorContext.HtmlGenerationService.CleanupHtml(html, baseUrl, cleanupRule);
                        }
                        else
                        {
                            html = HtmlCleaner.StripNamespacedTagsAndCommentsAndMarkupDirectives(html);
                        }

                        // standard fixups
                        html = EditorContext.HtmlGenerationService.GenerateHtmlFromHtmlFragment(html, baseUrl);
                    }

                    // insert the content
                    if (EditorContext.MarshalHtmlSupported)
                        EditorContext.InsertHtml(begin, end, html, DataMeister.HTMLData.SourceURL);
                    else if (EditorContext.MarshalTextSupported)
                    {
                        // This is called only in the case that we're attempting to marshal HTML, but only
                        // text is supported. In this case, we should down convert to text and provide that.
                        html = HTMLDocumentHelper.HTMLToPlainText(html);
                        EditorContext.InsertHtml(begin, end, html, DataMeister.HTMLData.SourceURL);
                    }
                    else
                        Debug.Assert(false, "Html being inserted when text or html isn't supported.");

                    // Now select what was just inserted
                    EditorContext.MarkupServices.CreateMarkupRange(begin, end).ToTextRange().select();

                    //place the caret at the end of the inserted content
                    //EditorContext.MoveCaretToMarkupPointer(end, true);
                    return true;
                }
                catch (Exception e)
                {
                    //bugfix 1696, put exceptions into the trace log.
                    Trace.Fail("Exception while inserting HTML: " + e.Message, e.StackTrace);
                    return false;
                }
            }
        }

        private void CopyBodyAttributes(string sourceHtml, IHTMLElement destinationBody)
        {
            Debug.Assert(destinationBody != null, "destinationBody should not be null!");
            if (destinationBody == null)
            {
                return;
            }

            var finder = new BodyTagFinder(sourceHtml);
            finder.Parse();
            if (finder.BodyBeginTag != null)
            {
                StringBuilder bodyAttributes = new StringBuilder();
                foreach (Attr attr in finder.BodyBeginTag.Attributes)
                {
                    bodyAttributes.AppendFormat(CultureInfo.InvariantCulture, "{0} ", attr);
                }
                IHTMLElement sourceBodyElement = EditorContext.MarkupServices.CreateElement(_ELEMENT_TAG_ID.TAGID_BODY, bodyAttributes.ToString());
                HTMLElementHelper.CopyAttributes(sourceBodyElement, destinationBody);
            }
        }

        /// <summary>
        /// Searches through the provided document for a start and end comment marker and then returns the fragment as
        /// a MarkupRange.
        /// </summary>
        /// <param name="document">The document to search.</param>
        /// <param name="startMarker">The comment text that marks the start of the fragment
        /// (e.g. &lt;!--StartFragment--&gt; ).</param>
        /// <param name="endMarker">The comment text that marks the end of the fragment
        /// (e.g. &lt;!--EndFragment--&gt; ).</param>
        /// <returns>The fragment as a MarkupRange or null if no valid fragment was found.</returns>
        private MarkupRange FindMarkedFragment(IHTMLDocument2 document, string startMarker, string endMarker)
        {
            MarkupPointer startFragment = null;
            MarkupPointer endFragment = null;
            MshtmlMarkupServices markupServices = new MshtmlMarkupServices((IMarkupServicesRaw)document);

            // Look for the markers in the document.
            foreach (IHTMLElement element in document.all)
            {
                if (element is IHTMLCommentElement && ((IHTMLCommentElement)element).text == startMarker)
                {
                    startFragment = markupServices.CreateMarkupPointer(element, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
                }
                else if (element is IHTMLCommentElement && ((IHTMLCommentElement)element).text == endMarker)
                {
                    endFragment = markupServices.CreateMarkupPointer(element, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
                }
            }

            if (startFragment == null || endFragment == null || !startFragment.Positioned || !endFragment.Positioned ||
                startFragment.IsRightOf(endFragment))
            {
                Trace.WriteLine("Unable to find fragment or invalid fragment!");
                return null;
            }

            // WinLive 251786: IE (and most other browsers) allow HTML like the following:
            //  <p>This is a paragraph[cursor]
            //  <p>This is a paragraph
            // However, when we use MarkupPointers to walk through this HTML, IE pretends there is a </p> at the end
            // of each of the above lines. This can cause issues when we copy part of this HTML somewhere else (e.g
            // everything after the [cursor]) and attempt to walk through both copies (e.g. during paste with keep
            // source formatting) at the same time. This holds true for some other elements, such as <li>s and <td>s.
            MarkupContext startContext = startFragment.Right(false);
            if (startFragment.IsLeftOf(endFragment) &&
                startContext.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_ExitScope &&
                startContext.Element != null &&
                ElementFilters.IsEndTagOptional(startContext.Element) &&
                !Regex.IsMatch(startContext.Element.outerHTML,
                               String.Format(CultureInfo.InvariantCulture, @"</{0}(\s[^>]*)?>\s*$", startContext.Element.tagName),
                               RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                startFragment.Right(true);
            }

            return markupServices.CreateMarkupRange(startFragment, endFragment);
        }

        /// <summary>
        /// Takes the source HTML and makes necessary modifications to keep the source formatting as if it were to be
        /// pasted into the destination range.
        /// </summary>
        /// <param name="sourceRange">The range containing the HTML that is being copied.</param>
        /// <param name="destinationRange">The range that the source HTML will be copied to.</param>
        /// <returns>A serialized string of the source HTML with necessary modifications to keep the source formatting
        /// or null if unsuccessful.</returns>
        private string KeepSourceFormatting(MarkupRange sourceRange, MarkupRange destinationRange)
        {
            Debug.Assert(sourceRange.Start.Container.GetOwningDoc() == destinationRange.Start.Container.GetOwningDoc(),
                "Ranges must share an owning document!");

            // We will temporarily add comments to the destination document to mark the destinationRange.
            IHTMLElement startComment = null;
            IHTMLElement endComment = null;

            try
            {
                // This is our true destination document.
                IHTMLDocument2 destinationDocument = destinationRange.Start.Container.Document;
                MshtmlMarkupServices destinationMarkupServices = new MshtmlMarkupServices((IMarkupServicesRaw)destinationDocument);

                // However, we'll use a temp destination because we don't want to paste anything into the real
                // document yet as it could fail, it would fire events, images would start loading, etc.
                MarkupContainer temporaryDestinationContainer = destinationMarkupServices.CreateMarkupContainer();
                MarkupPointer temporaryDestinationPointer = destinationMarkupServices.CreateMarkupPointer();
                temporaryDestinationPointer.MoveToContainer(temporaryDestinationContainer, true);

                // We add in comments to the destination document so that when we copy this range over to the fake
                // destination we'll be able to find the range again.
                destinationRange.Start.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Left;
                destinationRange.End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;

                string startMarker = string.Format(CultureInfo.InvariantCulture, "<!--{0}-->", Guid.NewGuid());
                destinationMarkupServices.InsertHtml(startMarker, destinationRange.Start);
                startComment = destinationRange.Start.Right(false).Element;

                string endMarker = string.Format(CultureInfo.InvariantCulture, "<!--{0}-->", Guid.NewGuid());
                destinationMarkupServices.InsertHtml(endMarker, destinationRange.End);
                endComment = destinationRange.End.Left(false).Element;

                try
                {
                    // Copy over the entire destination document into the fake destination document.
                    MarkupRange destinationAll = SelectAll(destinationDocument);
                    destinationMarkupServices.Copy(destinationAll.Start, destinationAll.End, temporaryDestinationPointer);

                    // Find the original destination range in this copy.
                    MarkupRange temporaryDestinationRange = FindMarkedFragment(temporaryDestinationContainer.Document, startMarker, endMarker);

                    if (temporaryDestinationRange != null)
                    {
                        // Do the work to keep the source formatting.
                        MarkupRange inlinedRange = new KeepSourceFormatting(sourceRange, temporaryDestinationRange).Execute();
                        if (inlinedRange != null)
                        {
                            // Normalize the HTML to ensure elements with optional end tags have explicit closing tags.
                            // MSHTML may serialize HTML without closing tags for elements like <p>, which can cause
                            // issues when the HTML is processed further or displayed in other contexts.
                            return HtmlUtils.NormalizeHtmlClosingTags(inlinedRange.HtmlText);
                        }
                    }
                }
                finally
                {
                    // WinLive 249077: Clear the temporary destination container, otherwise behaviors may
                    // inadvertently attach to elements in the MarkupContainer.
                    temporaryDestinationContainer.Document.body.innerHTML = String.Empty;
                }
            }
            catch (Exception e)
            {
                // I really dont want some funky html on the clipboard that causes a problem with this code
                // to prevent a paste from going through.
                Trace.Fail("Failed to get inline css for selection: " + e);
            }
            finally
            {
                Debug.Assert(startComment is IHTMLCommentElement, "Didn't find start comment or it wasn't created.");
                if (startComment is IHTMLCommentElement)
                {
                    HTMLElementHelper.RemoveElement(startComment);
                }

                Debug.Assert(endComment is IHTMLCommentElement, "Didn't find end comment or it wasn't created.");
                if (endComment is IHTMLCommentElement)
                {
                    HTMLElementHelper.RemoveElement(endComment);
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a MarkupRange that contains the entire provided document.
        /// </summary>
        /// <param name="document">The document to select.</param>
        /// <returns>A MarkupRange that contains the entire document.</returns>
        private MarkupRange SelectAll(IHTMLDocument2 document)
        {
            MshtmlMarkupServices markupServices = new MshtmlMarkupServices((IMarkupServicesRaw)document);
            MarkupRange entireDocument = markupServices.CreateMarkupRange(((IHTMLDocument3)document).documentElement, true);

            // Make sure the doctype and anything else outside the root element is selected too.
            MarkupContext context = entireDocument.Start.Left(true);
            while (context.Context != _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_None)
            {
                context = entireDocument.Start.Left(true);
            }

            context = entireDocument.End.Right(true);
            while (context.Context != _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_None)
            {
                context = entireDocument.End.Right(true);
            }

            return entireDocument;
        }

        /// <summary>
        /// Makes sure that whole (not parts of) tables are included in the source of a paste.
        /// </summary>
        /// <param name="range">The original source range. The range may be modified.</param>
        /// <param name="markupServices">MarkupServices for the range.</param>
        private void ExpandToIncludeTables(MarkupRange range, MshtmlMarkupServices markupServices)
        {
            MarkupPointer pointer = markupServices.CreateMarkupPointer();

            IHTMLElement[] tableElements = range.GetElements(ElementFilters.TABLE_ELEMENTS, false);
            foreach (IHTMLElement element in tableElements)
            {
                IHTMLElement parentTable = element;
                while (parentTable != null && markupServices.GetElementTagId(parentTable) != _ELEMENT_TAG_ID.TAGID_TABLE)
                {
                    parentTable = parentTable.parentElement;
                }

                if (parentTable != null)
                {
                    pointer.MoveAdjacentToElement(parentTable, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
                    if (range.Start.IsRightOf(pointer))
                    {
                        range.Start.MoveToPointer(pointer);
                    }

                    pointer.MoveAdjacentToElement(parentTable, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
                    if (range.End.IsLeftOf(pointer))
                    {
                        range.End.MoveToPointer(pointer);
                    }
                }
            }
        }

        /// <summary>
        /// Makes sure that whole (not parts of) lists are included in the source of a paste.
        /// </summary>
        /// <param name="range">The original source range. The range may be modified.</param>
        /// <param name="markupServices">MarkupServices for the range.</param>
        private void ExpandToIncludeLists(MarkupRange range, MshtmlMarkupServices markupServices)
        {
            MarkupPointer pointer = markupServices.CreateMarkupPointer();
            IHTMLElementFilter listFilter =
                ElementFilters.CreateCompoundElementFilter(ElementFilters.LIST_ELEMENTS, ElementFilters.LIST_ITEM_ELEMENTS);

            IHTMLElement[] listElements = range.GetElements(listFilter, false);
            foreach (IHTMLElement element in listElements)
            {
                IHTMLElement parentList = element;
                while (parentList != null && !ElementFilters.IsListElement(parentList))
                {
                    parentList = parentList.parentElement;
                }

                if (parentList != null)
                {
                    pointer.MoveAdjacentToElement(parentList, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
                    if (range.Start.IsRightOf(pointer))
                    {
                        range.Start.MoveToPointer(pointer);
                    }

                    pointer.MoveAdjacentToElement(parentList, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
                    if (range.End.IsLeftOf(pointer))
                    {
                        range.End.MoveToPointer(pointer);
                    }
                }
            }
        }

        /// <summary>
        /// If this element is a table, and there's nothing else before or after it and it's aligned left or right
        /// then removes the alignment.
        /// </summary>
        /// <param name="range">The original source range.</param>
        private void RemoveAlignmentIfSingleTable(MarkupRange range)
        {
            // WinLive 273280: Alignment on a table acts like a float, which can throw off the layout of the rest of
            // the document. If there is nothing before or after the table, then we can safely remove the alignment.
            IHTMLElement[] topLevelElements = range.GetTopLevelElements(e => !(e is IHTMLCommentElement));
            if (topLevelElements.Length == 1 &&
                topLevelElements[0] is IHTMLTable &&
                (String.Compare(topLevelElements[0].getAttribute("align", 2) as string, "left", StringComparison.OrdinalIgnoreCase) == 0 ||
                 String.Compare(topLevelElements[0].getAttribute("align", 2) as string, "right", StringComparison.OrdinalIgnoreCase) == 0))
            {
                topLevelElements[0].removeAttribute("align", 0);
            }
        }

        public static bool IsPasteFromSharedCanvas(DataObjectMeister dataMeister)
        {
            return (dataMeister.HTMLData != null) && IsSharedCanvasTempUrl(dataMeister.HTMLData.SourceURL);
        }

        public static bool IsSharedCanvasTempUrl(string url)
        {
            try
            {
                return UrlHelper.IsFileUrl(url) && TempFileManager.Instance.IsPathContained(new Uri(url).LocalPath);
            }
            catch
            {
                // maybe URL was not valid or something... who knows
                return false;
            }
        }
    }
}
