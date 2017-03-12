// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using OpenLiveWriter.Api;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.PostEditor.ContentSources;
using OpenLiveWriter.PostEditor.PostHtmlEditing.Sidebar;

namespace OpenLiveWriter.PostEditor
{
    internal class SmartContentWorker
    {
        /// <summary>
        /// Walks the current contents to find smart content areas.  When one is found, it calls the operation on the smart content.  The operation has a chance
        /// to return new content.  If the content is non-null it will replace the current content.
        /// </summary>
        /// <param name="contents">the raw HTML string whose structured blocks will be replaced.</param>
        /// <param name="operation">Delegate for generating replacement content.</param>
        /// <param name="editMode">If true, then the element's stylename will be activated for editing</param>
        /// <param name="continueOnError">
        /// true - if the plugin throws an exception, it keeps crawling the DOM
        /// false - if a plugin throws an exception, it stops processing the DOM and return empty string
        /// null - if a plugin throws an exception, this function will rethrow it
        /// </param
        /// <returns>the contents with structured blocks replaced.</returns>
        internal static string PerformOperation(string contents, SmartContentOperation operation, bool editMode, IContentSourceSidebarContext sourceContext, bool? continueOnError)
        {
            //replace all structured content blocks with their editor HTML
            //string html = PostBodyPreprocessor.Preprocess(contents);
            StringBuilder sb = new StringBuilder();
            SimpleHtmlParser parser = new SimpleHtmlParser(contents);
            for (Element e = parser.Next(); e != null; e = parser.Next())
            {

                if (e is BeginTag)
                {
                    BeginTag beginTag = (BeginTag)e;
                    string elementClassName = beginTag.GetAttributeValue("class");
                    if (ContentSourceManager.IsSmartContentClass(elementClassName))
                    {
                        ISmartContent sContent = null;
                        try
                        {
                            string contentSourceId, contentItemId;
                            string blockId = beginTag.GetAttributeValue("id");
                            if (blockId != null)
                            {
                                ContentSourceManager.ParseContainingElementId(blockId, out contentSourceId, out contentItemId);

                                ContentSourceInfo contentSource = sourceContext.FindContentSource(contentSourceId);
                                if (contentSource != null && contentSource.Instance is SmartContentSource)
                                {
                                    SmartContentSource sSource = (SmartContentSource)contentSource.Instance;
                                    sContent = sourceContext.FindSmartContent(contentItemId);
                                    if (sContent != null)
                                    {

                                        //write the div with the appropriate className
                                        string newClassName = editMode ? ContentSourceManager.EDITABLE_SMART_CONTENT : ContentSourceManager.SMART_CONTENT;
                                        beginTag.GetAttribute("class").Value = newClassName;

                                        //replace the inner HTML of the div with the source's editor HTML
                                        string content = parser.CollectHtmlUntil("div");

                                        sb.Append(e.ToString());

                                        operation(sourceContext, sSource, sContent, ref content);

                                        sb.Append(content);

                                        sb.Append("</div>");
                                        continue;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(String.Format(CultureInfo.InvariantCulture, "Error loading smart content item\r\n{0}", ex));
                            sContent = null;

                            if (continueOnError == null)
                                throw;

                            if (!continueOnError.Value)
                                return String.Empty;
                        }

                        if (sContent == null)
                        {
                            //this element references an unknown smart content, so it should not be editable
                            Attr classAttr = beginTag.GetAttribute("class");
                            classAttr.Value = ContentSourceManager.SMART_CONTENT;
                        }
                    }
                }
                sb.Append(e.ToString());
            }

            return sb.ToString();
        }

        internal delegate void SmartContentOperation(IPublishingContext site, SmartContentSource source, ISmartContent sContent, ref string content);
    }
}
