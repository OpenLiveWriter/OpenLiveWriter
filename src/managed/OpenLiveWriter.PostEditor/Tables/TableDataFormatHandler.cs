// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using OpenLiveWriter.Api;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.HtmlEditor.Marshalling;
using OpenLiveWriter.HtmlEditor.Marshalling.Data_Handlers;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.HTML;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.PostEditor.ContentSources;
using OpenLiveWriter.PostEditor.PostHtmlEditing;
using mshtml;

namespace OpenLiveWriter.PostEditor.Tables
{

    internal class TableDataFormatHandler : HtmlHandler
    {

        public TableDataFormatHandler(DataObjectMeister dataObject, DataFormatHandlerContext handlerContext, IHtmlMarshallingTarget editorContext)
            : base(dataObject, handlerContext, editorContext)
        {

        }

        public new static bool CanCreateFrom(DataObjectMeister data)
        {
            return IsPasteFromSharedCanvas(data) && (GetSourceTable(data) != null);
        }

        protected override bool DoInsertData(DataAction action, MarkupPointer begin, MarkupPointer end)
        {
            // get the table and its cells
            IHTMLTable sourceTable = GetSourceTable(DataMeister);
            IHTMLElementCollection cells = (sourceTable as IHTMLElement2).getElementsByTagName("td");

            // single-cell tables just get the innerHTML of the cell pasted at the selection
            if (cells.length == 1)
            {
                IHTMLElement cell = cells.item(0, 0) as IHTMLElement;
                EditorContext.InsertHtml(begin, end, cell.innerHTML, UrlHelper.GetBaseUrl(DataMeister.HTMLData.SourceURL));
            }
            else
            {
                // if we are inside a table
                TableSelection tableSelection = new TableSelection(EditorContext.MarkupServices.CreateMarkupRange(begin, end));
                if (tableSelection.Table != null)
                {
                    // paste the source cells into the table
                    PasteCellsIntoTable(sourceTable, tableSelection);
                }
                else
                {
                    // get table html (make sure width matches # of rows selected)
                    TableHelper.SynchronizeTableWidthForEditing(sourceTable);
                    string html = (sourceTable as IHTMLElement).outerHTML;

                    // insert the html (nests within an undo unit)
                    EditorContext.InsertHtml(begin, end, html, UrlHelper.GetBaseUrl(DataMeister.HTMLData.SourceURL));
                }
            }

            return true;
        }

        private void PasteCellsIntoTable(IHTMLTable sourceTable, TableSelection tableSelection)
        {
            // collect up the top level cells we are pasting
            ArrayList cellsToPaste = new ArrayList();
            foreach (IHTMLTableRow row in sourceTable.rows)
                foreach (IHTMLElement cell in row.cells)
                    cellsToPaste.Add(cell);

            // get the list of cells we are pasting into
            int appendRows = 0;
            int cellsPerRow = 0;
            ArrayList targetCells;
            if (tableSelection.SelectedCells.Count > 1)
            {
                targetCells = tableSelection.SelectedCells;
            }
            else
            {
                targetCells = new ArrayList();
                bool accumulatingCells = false;
                foreach (IHTMLTableRow row in tableSelection.Table.rows)
                {
                    cellsPerRow = row.cells.length;
                    foreach (IHTMLElement cell in row.cells)
                    {
                        if (!accumulatingCells && HTMLElementHelper.ElementsAreEqual(cell, tableSelection.BeginCell as IHTMLElement))
                            accumulatingCells = true;

                        if (accumulatingCells)
                            targetCells.Add(cell);
                    }
                }

                // if the target cells aren't enough to paste all of the cells, then
                // calculate the number of rows we need to append to fit all of the
                // cells being pasted
                int cellGap = cellsToPaste.Count - targetCells.Count;
                if (cellGap > 0 && cellsPerRow > 0)
                {
                    appendRows = cellGap / cellsPerRow + (cellGap % cellsPerRow == 0 ? 0 : 1);
                }
            }

            // perform the paste
            using (IUndoUnit undoUnit = EditorContext.CreateUndoUnit())
            {
                // append rows if needed
                if (appendRows > 0)
                {
                    // see if we can cast our editor context to the one required
                    // by the table editor
                    IHtmlEditorComponentContext editorContext = EditorContext as IHtmlEditorComponentContext;
                    if (editorContext != null)
                    {
                        // markup range based on last target cell
                        IHTMLElement lastCell = targetCells[targetCells.Count - 1] as IHTMLElement;
                        MarkupRange lastCellRange = EditorContext.MarkupServices.CreateMarkupRange(lastCell);
                        for (int i = 0; i < appendRows; i++)
                        {
                            IHTMLTableRow row = TableEditor.InsertRowBelow(editorContext, lastCellRange);
                            foreach (IHTMLElement cell in row.cells)
                                targetCells.Add(cell);
                            lastCellRange = EditorContext.MarkupServices.CreateMarkupRange(row as IHTMLElement);
                        }
                    }
                    else
                    {
                        Debug.Fail("Couldn't cast EditorContext!");
                    }
                }

                // do the paste
                for (int i = 0; i < cellsToPaste.Count && i < targetCells.Count; i++)
                {
                    (targetCells[i] as IHTMLElement).innerHTML = (cellsToPaste[i] as IHTMLElement).innerHTML;
                }
                undoUnit.Commit();
            }

        }

        private static IHTMLTable GetSourceTable(DataObjectMeister dataMeister)
        {
            // find our post body div
            IHTMLElement body = GetPostBodyElement(dataMeister.HTMLData.HTMLDocument);

            // look for a single table which constitutes the entire selection
            if (body != null)
            {
                IHTMLElementCollection tables = (body as IHTMLElement2).getElementsByTagName("table");
                if (tables.length >= 1)
                {
                    IHTMLElement table = tables.item(0, 0) as IHTMLElement;
                    if (table.innerText == (body as IHTMLElement).innerText)
                    {
                        if (TableHelper.TableElementIsEditable(table))
                        {
                            return table as IHTMLTable;
                        }
                    }
                }
            }

            // didn't find a single table
            return null;
        }

        /// <summary>
        /// This function will search the document for a contenteditable element that also
        /// has a postBody class name.  This is because we don't have access to the editor in a static context.
        /// Before using this function again in the future, consider finding a way to access BlogPostHtmlEditorControl.PostBodyElement
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        private static IHTMLElement GetPostBodyElement(IHTMLDocument2 document)
        {
            // find our post body div
            IHTMLElementCollection elementCollection = document.all;
            foreach (IHTMLElement3 element in elementCollection)
            {
                if ((element as IHTMLElement).className == "postBody" && element.contentEditable == "true")
                {
                    return element as IHTMLElement;
                }
            }

            // didn't find it
            return null;
        }

    }
}
