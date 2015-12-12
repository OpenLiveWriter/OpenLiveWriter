// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using OpenLiveWriter.CoreServices.HTML;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.HtmlEditor.Marshalling.Data_Handlers
{
    /// <summary>
    /// Data format handler for plain text
    /// </summary>
    internal class PlainTextHandler : FreeTextHandler
    {
        public PlainTextHandler(DataObjectMeister dataObject, DataFormatHandlerContext handlerContext, IHtmlMarshallingTarget editorContext)
            : base(dataObject, handlerContext, editorContext)
        {
        }
        /// <summary>
        /// Is there text data in the passed data object?
        /// </summary>
        /// <param name="data">data</param>
        /// <returns>true if there is text data, else false</returns>
        public static bool CanCreateFrom(DataObjectMeister data)
        {
            return data.TextData != null;
        }

        /// <summary>
        /// Grabs text copied in the clipboard and pastes it into the document
        /// </summary>
        protected override bool DoInsertData(DataAction action, MarkupPointer begin, MarkupPointer end)
        {
            try
            {
                // get the text data as a string
                string textData = DataMeister.TextData.Text;
                string html = EditorContext.HtmlGenerationService.GenerateHtmlFromPlainText(textData);

                //insert captured content into the document
                EditorContext.InsertHtml(begin, end, html, null);
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
