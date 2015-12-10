// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.HtmlEditor.Marshalling.Data_Handlers
{
    /// <summary>
    /// Data format handler for mindshare entities
    /// </summary>
    internal class FileHandler : FreeTextHandler
    {
        public FileHandler(DataObjectMeister dataObject, DataFormatHandlerContext handlerContext, IHtmlMarshallingTarget editorContext)
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
            // see if there are files at the top-level of the file data
            FileData fileData = data.FileData;
            if (fileData != null && fileData.Files.Length > 0)
            {
                // if there are any directories in the file data then we can't create from
                foreach (FileItem file in fileData.Files)
                {
                    if (file.IsDirectory)
                        return false;
                }

                // no directories found, we can create
                return true;
            }
            else  // no file data
            {
                return false;
            }
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
                    // get the list of files from the data meister
                    FileItem[] files = DataMeister.FileData.Files;

                    string[] filePaths = new string[files.Length];
                    // create an array of file entities to insert
                    for (int i = 0; i < files.Length; i++)
                    {
                        filePaths[i] = files[i].ContentsPath;
                    }

                    string html = EditorContext.HtmlGenerationService.GenerateHtmlFromFiles(filePaths);
                    EditorContext.InsertHtml(begin, end, html, null);

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
    }
}
