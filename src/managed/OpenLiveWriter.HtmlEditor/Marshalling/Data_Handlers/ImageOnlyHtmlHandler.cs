// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor.Marshalling.Data_Handlers
{
    /// <summary>
    // WinLive 96840 - Copying and pasting images within shared canvas should persist source
    // decorator settings.
    /// Helper function to add "wlCopySrcUrl" attribute from "src" attribute
    /// </summary>
    internal class ImageCopyFixupHelper
    {
        public static string FixupSourceUrlForCopy(string html)
        {
            // Note: This is not perfect but does the job. The search string could potentially
            // match 'mysrc="hello"', and this will cause it to insert wlCopySrcUrl even for that.
            // But that is ok, it should be rare, worst case is an extra dummy attribute.
            Regex rxSrc = new Regex(@"\s*src\s*=\s*([""'])(.*?)\1",
                                 RegexOptions.CultureInvariant | RegexOptions.Compiled |
                                 RegexOptions.IgnoreCase | RegexOptions.Singleline);
            // Looks for an IMG tag
            Regex rxImg = new Regex(@"<IMG\s+(.*?)/?>",
                                 RegexOptions.CultureInvariant | RegexOptions.Compiled |
                                 RegexOptions.IgnoreCase | RegexOptions.Singleline);

            return rxImg.Replace(html, new MatchEvaluator(match =>
                rxSrc.Replace(match.Value, new MatchEvaluator(match2 =>
                    string.Format(CultureInfo.InvariantCulture, " src=\"{0}\" wlCopySrcUrl=\"{0}\" ", match2.Groups[2].Value)))));
        }
    }

    /// <summary>
    /// Data format handler for Image-only HTML snippets.
    /// </summary>
    internal class ImageOnlyHtmlHandler : FreeTextHandler
    {
        public ImageOnlyHtmlHandler(DataObjectMeister dataObject, DataFormatHandlerContext handlerContext, IHtmlMarshallingTarget editorContext)
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
            return data.HTMLData != null && data.HTMLData.OnlyImagePath != null;
        }

        private string GrowToAnchorParent(HTMLData htmlData)
        {
            if (htmlData.OnlyImageElement == null)
                return null;

            string html;
            // Load up the html document from the clipboard to a document to examine the html about to be inserted
            MshtmlMarkupServices markupServices = new MshtmlMarkupServices(htmlData.HTMLDocument as IMarkupServicesRaw);
            MarkupRange range = markupServices.CreateMarkupRange(htmlData.OnlyImageElement, true);

            // look to see if this is a case where the inserted html is <a>|<img>|</a>
            MarkupContext markupContextStart = range.Start.Left(true);
            MarkupContext markupContextEnd = range.End.Right(true);

            // if that is the cause, change the html about to be inserted to |<a><img></a>|
            if (markupContextStart.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_ExitScope &&
                markupContextEnd.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_ExitScope &&
                markupContextStart.Element.tagName == "A" &&
                markupContextEnd.Element.tagName == "A")
            {
                html = markupContextStart.Element.outerHTML;
            }
            else
            {
                html = htmlData.HTMLSelection;
            }

            return html;
        }

        /// <summary>
        /// Grabs an HTML img copied in the clipboard and pastes it into the document.
        /// </summary>
        protected override bool DoInsertData(DataAction action, MarkupPointer begin, MarkupPointer end)
        {
            using (new WaitCursor())
            {
                try
                {
                    HTMLData htmlData = DataMeister.HTMLData;
                    string baseUrl = UrlHelper.GetBaseUrl(htmlData.SourceURL);
                    string html = htmlData.HTMLSelection;

                    if (HtmlHandler.IsPasteFromSharedCanvas(DataMeister))
                    {
                        if (action == DataAction.Move)
                        {
                            // if we are dragging and dropping the image, we need to grow to find the anchor for an image
                            html = GrowToAnchorParent(htmlData);
                        }
                        else
                        {
                            // if we are copying and pasting an image from writer, we need to change temp path references the original file path
                            if (htmlData.OnlyImageElement != null)
                            {
                                // WinLive 96840 - Copying and pasting images within shared canvas should persist source
                                // decorator settings. "wlCopySrcUrl" is inserted while copy/pasting within canvas.
                                // Insert wlCopySrcUrl attribute
                                html = ImageCopyFixupHelper.FixupSourceUrlForCopy(html);
                            }
                            html = EditorContext.FixImageReferences(html, htmlData.SourceURL);
                        }
                    }
                    else
                    {
                        html = EditorContext.FixImageReferences(html, htmlData.SourceURL);
                        html = EditorContext.HtmlGenerationService.CleanupHtml(html, baseUrl, HtmlCleanupRule.Normal);
                    }

                    html = EditorContext.HtmlGenerationService.GenerateHtmlFromHtmlFragment(html, baseUrl);
                    EditorContext.InsertHtml(begin, end, html, DataMeister.HTMLData.SourceURL);

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
    }
}
