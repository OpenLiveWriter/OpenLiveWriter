// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.HtmlEditor.Marshalling.Data_Handlers
{
    /// <summary>
    /// Data format handler for mindshare entities
    /// </summary>
    public class UrlHandler : FreeTextHandler
    {
        public UrlHandler(DataObjectMeister dataObject, DataFormatHandlerContext handlerContext, IHtmlMarshallingTarget editorContext)
            : base(dataObject, handlerContext, editorContext)
        {
            _url = ExtractUrl(dataObject);
            _title = ExtractTitle(dataObject);
        }

        /// <summary>
        /// Is there URL data in the passed data object?
        /// </summary>
        /// <param name="data">data</param>
        /// <returns>true if there is url data, else false</returns>
        public static bool CanCreateFrom(DataObjectMeister data)
        {
            return HasUrl(data);
        }

        protected static bool HasUrl(DataObjectMeister data)
        {
            // Firefox places Urldata on the clipboard when copying HTML selections. As a result
            // we need to ignore the urldata if there is html data and let the htmldata fall through
            if (data.HTMLData != null)
                return false;

            return (data.URLData != null || IsTextUrl(GetText(data)));
        }

        protected static string ExtractUrl(DataObjectMeister data)
        {
            if (data.URLData != null)
                return data.URLData.URL;
            else if (IsTextUrl(GetText(data)))
                return data.TextData.Text;
            else
                return null;
        }

        protected static string ExtractTitle(DataObjectMeister data)
        {
            if (data.URLData != null)
                return data.URLData.Title;
            else if (IsTextUrl(GetText(data)))
                return data.TextData.Text; //note: don't use TextData.Title as it returns a truncated string (bug 328059)
            else
                return null;
        }

        private static bool IsTextUrl(string text)
        {
            return !Regex.IsMatch(text.Trim(), "\\s") && UrlHelper.IsUrl(text);
        }

        private static string GetText(DataObjectMeister data)
        {
            string text = null;
            if (data != null && data.TextData != null)
                text = data.TextData.Text;

            if (text == null)
                text = string.Empty;

            return text;
        }

        /// <summary>
        /// Ask the data format handler to provide drag over feedback (copy, move, etc.)
        /// </summary>
        /// <param name="supportedEffects">Effects supported by the drag source</param>
        /// <returns>Effets allowed by the drop target</returns>
        public override DragDropEffects ProvideDragFeedback(Point screenPoint, int keyState, DragDropEffects supportedEffects)
        {
            base.ProvideDragFeedback(screenPoint, keyState, supportedEffects);
            if ((supportedEffects & DragDropEffects.Copy) > 0)
                return DragDropEffects.Copy;
            if ((supportedEffects & DragDropEffects.Link) > 0)
                return DragDropEffects.Link;
            else
                return DragDropEffects.None;
        }

        protected string Url
        {
            get
            {
                return _url;
            }
        }
        private string _url;

        protected string Title
        {
            get
            {
                return _title;
            }
        }
        private string _title;

        /// <summary>
        /// Grabs HTML copied in the clipboard and pastes it into the document (pulls in a copy of embedded content too)
        /// </summary>
        protected override bool DoInsertData(DataAction action, MarkupPointer begin, MarkupPointer end)
        {
            using (new WaitCursor())
            {
                try
                {
                    //StringBuilder html = new StringBuilder();
                    //html.AppendFormat("<a href=\"{0}\">{1}</a>", DataMeister.URLData.URL, DataMeister.URLData.Title);
                    string html = EditorContext.HtmlGenerationService.GenerateHtmlFromLink(Url, Title, Title, String.Empty, false);
                    EditorContext.InsertHtml(begin, end, html, null);

                    //place the caret at the end of the inserted content
                    //EditorContext.MoveCaretToMarkupPointer(end, true);
                    return true;
                }
                catch (Exception e)
                {
                    //bugfix 1696, put exceptions into the trace log.
                    Trace.Fail("Exception while inserting URL: " + e.Message, e.StackTrace);
                    return false;
                }
            }
        }
    }
}
