// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Diagnostics;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Controls;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.Mshtml;
using mshtml;

namespace OpenLiveWriter.PostEditor.Tables
{

    public class TableEditor
    {
        public static void InsertTable(IHtmlEditor htmlEditor, TableCreationParameters parameters)
        {
            InsertTable(htmlEditor, null, parameters);
        }

        public static void InsertTable(IHtmlEditor htmlEditor, IHtmlEditorComponentContext editorContext, TableCreationParameters parameters)
        {
            // note whether we are inserting into mshtml
            bool insertingIntoMshtml = editorContext != null;

            // build table html
            StringBuilder tableHtml = new StringBuilder();

            // clip the width if necessary (if inserting a table within another table cell)
            if (insertingIntoMshtml)
            {
                IHTMLElement2 parentCellBlock = editorContext.Selection.SelectedMarkupRange.Start.GetParentElement(ElementFilters.BLOCK_OR_TABLE_CELL_ELEMENTS) as IHTMLElement2;
                if (parentCellBlock is IHTMLTableCell)
                {
                    int parentCellWidth = parentCellBlock.clientWidth != 0 ? parentCellBlock.clientWidth : parentCellBlock.scrollWidth;
                    parameters.Properties.Width = Math.Min(parentCellWidth, parameters.Properties.Width);
                }
            }

            // table properties
            TableProperties properties = parameters.Properties;
            StringBuilder propertiesString = new StringBuilder();
            if (properties.Width.Units != PixelPercentUnits.Undefined)
                propertiesString.AppendFormat("width=\"{0}\"", properties.Width);
            if (properties.BorderSize != String.Empty)
                propertiesString.AppendFormat(" border=\"{0}\"", properties.BorderSize);
            if (properties.CellPadding != String.Empty)
                propertiesString.AppendFormat(" cellpadding=\"{0}\"", properties.CellPadding);
            if (properties.CellSpacing != String.Empty)
                propertiesString.AppendFormat(" cellspacing=\"{0}\"", properties.CellSpacing);

            // begin table
            tableHtml.AppendFormat("<table {0} unselectable=\"on\">\r\n", propertiesString.ToString());
            tableHtml.Append("<tbody>\r\n");

            // write cells
            string columnWidth = String.Empty;

            switch (parameters.Properties.Width.Units)
            {
                case PixelPercentUnits.Pixels:
                    int width = parameters.Properties.Width / parameters.Columns;
                    columnWidth = string.Format(" width=\"{0}\"", width);
                    break;
                case PixelPercentUnits.Percentage:
                    columnWidth = string.Format(" width=\"{0}%\"", 100 / parameters.Columns);
                    break;
            }

            for (int r = 0; r < parameters.Rows; r++)
            {
                tableHtml.Append("<tr>\r\n");

                for (int c = 0; c < parameters.Columns; c++)
                {
                    // add default alignment and width to each cell
                    string valign = " valign=\"top\""; //    (more natural/expected behavior than middle)
                    tableHtml.AppendFormat("<td {0}{1}></td>\r\n", valign, columnWidth);
                }

                tableHtml.Append("</tr>\r\n");
            }
            

            // end table
            tableHtml.Append("</tbody>\r\n");
            tableHtml.Append("</table>\r\n");

            // if we have an mshml selection, save it before inserting (so we can locate the table after insert)
            MarkupRange targetMarkupRange = null;
            if (insertingIntoMshtml)
            {
                targetMarkupRange = editorContext.Selection.SelectedMarkupRange.Clone();
                targetMarkupRange.Start.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Left;
                targetMarkupRange.End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;
            }

            IDisposable insertionNotification = null;
            if (insertingIntoMshtml)
            {
                insertionNotification = new HtmlEditorControl.InitialInsertionNotify((HtmlEditorControl)editorContext);
            }

            using (insertionNotification)
            {
                // insert the table
                htmlEditor.InsertHtml(tableHtml.ToString(), false);

                // select the first cell for editing if we have an mshtml selection
                if (insertingIntoMshtml)
                {
                    IHTMLElement[] cells = targetMarkupRange.GetElements(ElementFilters.TABLE_CELL_ELEMENT, true);
                    if (cells.Length > 0)
                    {
                        SelectCell(editorContext, cells[0] as IHTMLTableCell);
                    }
                }
            }
        }

        public static TableProperties GetTableProperties(IHtmlEditorComponentContext editorContext)
        {
            return GetTableProperties(editorContext, editorContext.Selection.SelectedMarkupRange);
        }

        public static TableProperties GetTableProperties(IHtmlEditorComponentContext editorContext, MarkupRange markupRange)
        {
            TableEditor tableEditor = new TableEditor(editorContext, markupRange);
            return tableEditor.TableProperties;
        }

        public static void SetTableProperties(IHtmlEditorComponentContext editorContext, TableProperties tableProperties)
        {
            SetTableProperties(editorContext, editorContext.Selection.SelectedMarkupRange, tableProperties);
        }

        public static void SetTableProperties(IHtmlEditorComponentContext editorContext, MarkupRange markupRange, TableProperties tableProperties)
        {
            TableEditor tableEditor = new TableEditor(editorContext, markupRange);
            tableEditor.TableProperties = tableProperties;
        }

        public static void DeleteTable(IHtmlEditorComponentContext editorContext)
        {
            DeleteTable(editorContext, editorContext.Selection.SelectedMarkupRange);
        }

        public static void DeleteTable(IHtmlEditorComponentContext editorContext, MarkupRange markupRange)
        {
            TableEditor tableEditor = new TableEditor(editorContext, markupRange);
            tableEditor.DeleteTable();
        }

        public static RowProperties GetRowProperties(IHtmlEditorComponentContext editorContext)
        {
            return GetRowProperties(editorContext, editorContext.Selection.SelectedMarkupRange);
        }

        public static RowProperties GetRowProperties(IHtmlEditorComponentContext editorContext, MarkupRange markupRange)
        {
            TableEditor tableEditor = new TableEditor(editorContext, markupRange);
            return tableEditor.RowProperties;
        }

        public static void SetRowProperties(IHtmlEditorComponentContext editorContext, RowProperties rowProperties)
        {
            SetRowProperties(editorContext, editorContext.Selection.SelectedMarkupRange, rowProperties);
        }

        public static void SetRowProperties(IHtmlEditorComponentContext editorContext, MarkupRange markupRange, RowProperties rowProperties)
        {
            TableEditor tableEditor = new TableEditor(editorContext, markupRange);
            tableEditor.RowProperties = rowProperties;
        }

        public static IHTMLTableRow InsertRowAbove(IHtmlEditorComponentContext editorContext)
        {
            return InsertRowAbove(editorContext, editorContext.Selection.SelectedMarkupRange);
        }

        public static IHTMLTableRow InsertRowAbove(IHtmlEditorComponentContext editorContext, MarkupRange markupRange)
        {
            TableEditor tableEditor = new TableEditor(editorContext, markupRange);
            return tableEditor.InsertRowAbove();
        }

        public static IHTMLTableRow InsertRowBelow(IHtmlEditorComponentContext editorContext)
        {
            return InsertRowBelow(editorContext, editorContext.Selection.SelectedMarkupRange);
        }

        public static IHTMLTableRow InsertRowBelow(IHtmlEditorComponentContext editorContext, MarkupRange markupRange)
        {
            TableEditor tableEditor = new TableEditor(editorContext, markupRange);
            return tableEditor.InsertRowBelow();
        }

        public static void MoveRowUp(IHtmlEditorComponentContext editorContext)
        {
            MarkupRange selectedMarkupRange = editorContext.Selection.SelectedMarkupRange;
            using (new SelectionPreserver(selectedMarkupRange))
                MoveRowUp(editorContext, selectedMarkupRange);
        }

        public static void MoveRowUp(IHtmlEditorComponentContext editorContext, MarkupRange markupRange)
        {
            TableEditor tableEditor = new TableEditor(editorContext, markupRange);
            tableEditor.MoveRowUp();
        }

        public static void MoveRowDown(IHtmlEditorComponentContext editorContext)
        {
            MarkupRange selectedMarkupRange = editorContext.Selection.SelectedMarkupRange;
            using (new SelectionPreserver(selectedMarkupRange))
                MoveRowDown(editorContext, selectedMarkupRange);
        }

        public static void MoveRowDown(IHtmlEditorComponentContext editorContext, MarkupRange markupRange)
        {
            TableEditor tableEditor = new TableEditor(editorContext, markupRange);
            tableEditor.MoveRowDown();
        }

        public static void DeleteRows(IHtmlEditorComponentContext editorContext)
        {
            DeleteRows(editorContext, editorContext.Selection.SelectedMarkupRange);
        }

        public static void DeleteRows(IHtmlEditorComponentContext editorContext, MarkupRange markupRange)
        {
            TableEditor tableEditor = new TableEditor(editorContext, markupRange);
            tableEditor.DeleteRows();
        }

        public static ColumnProperties GetColumnProperties(IHtmlEditorComponentContext editorContext)
        {
            return GetColumnProperties(editorContext, editorContext.Selection.SelectedMarkupRange);
        }

        public static ColumnProperties GetColumnProperties(IHtmlEditorComponentContext editorContext, MarkupRange markupRange)
        {
            TableEditor tableEditor = new TableEditor(editorContext, markupRange);
            return tableEditor.ColumnProperties;
        }

        public static void SetColumnProperties(IHtmlEditorComponentContext editorContext, ColumnProperties columnProperties)
        {
            SetColumnProperties(editorContext, editorContext.Selection.SelectedMarkupRange, columnProperties);
        }

        public static void SetColumnProperties(IHtmlEditorComponentContext editorContext, MarkupRange markupRange, ColumnProperties columnProperties)
        {
            TableEditor tableEditor = new TableEditor(editorContext, markupRange);
            tableEditor.ColumnProperties = columnProperties;
        }

        public static void InsertColumnLeft(IHtmlEditorComponentContext editorContext)
        {
            InsertColumnLeft(editorContext, editorContext.Selection.SelectedMarkupRange);
        }

        public static void InsertColumnLeft(IHtmlEditorComponentContext editorContext, MarkupRange markupRange)
        {
            TableEditor tableEditor = new TableEditor(editorContext, markupRange);
            tableEditor.InsertColumnLeft();
        }

        public static void InsertColumnRight(IHtmlEditorComponentContext editorContext)
        {
            InsertColumnRight(editorContext, editorContext.Selection.SelectedMarkupRange);
        }

        public static void InsertColumnRight(IHtmlEditorComponentContext editorContext, MarkupRange markupRange)
        {
            TableEditor tableEditor = new TableEditor(editorContext, markupRange);
            tableEditor.InsertColumnRight();
        }

        public static void MoveColumnLeft(IHtmlEditorComponentContext editorContext)
        {
            MarkupRange selectedMarkupRange = editorContext.Selection.SelectedMarkupRange;
            using (new SelectionPreserver(selectedMarkupRange))
                MoveColumnLeft(editorContext, selectedMarkupRange);
        }

        public static void MoveColumnLeft(IHtmlEditorComponentContext editorContext, MarkupRange markupRange)
        {
            TableEditor tableEditor = new TableEditor(editorContext, markupRange);
            tableEditor.MoveColumnLeft();
        }

        public static void MoveColumnRight(IHtmlEditorComponentContext editorContext)
        {
            MarkupRange selectedMarkupRange = editorContext.Selection.SelectedMarkupRange;
            using (new SelectionPreserver(selectedMarkupRange))
                MoveColumnRight(editorContext, selectedMarkupRange);
        }

        public static void MoveColumnRight(IHtmlEditorComponentContext editorContext, MarkupRange markupRange)
        {
            TableEditor tableEditor = new TableEditor(editorContext, markupRange);
            tableEditor.MoveColumnRight();
        }

        public static void DeleteColumns(IHtmlEditorComponentContext editorContext)
        {
            DeleteColumns(editorContext, editorContext.Selection.SelectedMarkupRange);
        }

        public static void DeleteColumns(IHtmlEditorComponentContext editorContext, MarkupRange markupRange)
        {
            TableEditor tableEditor = new TableEditor(editorContext, markupRange);
            tableEditor.DeleteColumns();
        }

        public static CellProperties GetCellProperties(IHtmlEditorComponentContext editorContext)
        {
            return GetCellProperties(editorContext, editorContext.Selection.SelectedMarkupRange);
        }

        public static CellProperties GetCellProperties(IHtmlEditorComponentContext editorContext, MarkupRange markupRange)
        {
            TableEditor tableEditor = new TableEditor(editorContext, markupRange);
            return tableEditor.CellProperties;
        }

        public static void SetCellProperties(IHtmlEditorComponentContext editorContext, CellProperties cellProperties)
        {
            SetCellProperties(editorContext, editorContext.Selection.SelectedMarkupRange, cellProperties);
        }

        public static void SetCellProperties(IHtmlEditorComponentContext editorContext, MarkupRange markupRange, CellProperties cellProperties)
        {
            TableEditor tableEditor = new TableEditor(editorContext, markupRange);
            tableEditor.CellProperties = cellProperties;
        }

        public static void ClearCells(IHtmlEditorComponentContext editorContext)
        {
            ClearCells(editorContext, editorContext.Selection.SelectedMarkupRange);
        }

        public static void ClearCells(IHtmlEditorComponentContext editorContext, MarkupRange markupRange)
        {
            TableEditor tableEditor = new TableEditor(editorContext, markupRange);
            tableEditor.ClearCells();
        }

        public static void InsertLineBreak(IHtmlEditorComponentContext editorContext)
        {
            TableEditor tableEditor = new TableEditor(editorContext);
            tableEditor.InsertLineBreak();
        }

        public static void SelectNextCell(IHtmlEditorComponentContext editorContext)
        {
            TableEditor tableEditor = new TableEditor(editorContext);
            tableEditor.SelectNextCell();
        }

        public static void SelectPreviousCell(IHtmlEditorComponentContext editorContext)
        {
            TableEditor tableEditor = new TableEditor(editorContext);
            tableEditor.SelectPreviousCell();
        }

        public static void SelectCell(IHtmlEditorComponentContext editorContext, IHTMLTableCell cell)
        {
            TableEditor tableEditor = new TableEditor(editorContext);
            tableEditor.SelectCell(cell);
        }

        public static void MakeEmptyCellsNbsp(IHtmlEditorComponentContext editorContext, IHTMLElement tableElement)
        {
            MarkupRange tableMarkupRange = editorContext.MarkupServices.CreateMarkupRange(tableElement, false);
            TableEditor tableEditor = new TableEditor(editorContext, tableMarkupRange);
            tableEditor.MakeEmptyCellsNbsp();
        }

        public static void MakeEmptyCellsNull(IHtmlEditorComponentContext editorContext, IHTMLElement tableElement)
        {
            MarkupRange tableMarkupRange = editorContext.MarkupServices.CreateMarkupRange(tableElement, false);
            TableEditor tableEditor = new TableEditor(editorContext, tableMarkupRange);
            tableEditor.MakeEmptyCellsNull();
        }

        #region Construction and Initialization

        private TableEditor(IHtmlEditorComponentContext editorContext)
            : this(editorContext, editorContext.Selection.SelectedMarkupRange)
        {
        }

        private TableEditor(IHtmlEditorComponentContext editorContext, MarkupRange markupRange)
        {
            _editorContext = editorContext;
            _markupRange = markupRange;
        }

        #endregion

        #region Table Level Commands

        private TableProperties TableProperties
        {
            get
            {
                TableProperties tableProperties = new TableProperties();

                // read cell padding
                if (TableSelection.Table.cellPadding != null)
                    tableProperties.CellPadding = TableSelection.Table.cellPadding.ToString();
                else
                    tableProperties.CellPadding = String.Empty;

                // read cell spacing
                if (TableSelection.Table.cellSpacing != null)
                    tableProperties.CellSpacing = TableSelection.Table.cellSpacing.ToString();
                else
                    tableProperties.CellSpacing = String.Empty;

                // read border
                if (TableSelection.Table.border != null)
                    tableProperties.BorderSize = TableSelection.Table.border.ToString();
                else
                    tableProperties.BorderSize = String.Empty;

                // read width
                tableProperties.Width = TableHelper.GetTableWidth(TableSelection.Table);

                // return
                return tableProperties;
            }
            set
            {
                // save properties to table
                using (IUndoUnit undoUnit = _editorContext.CreateUndoUnit())
                {
                    // cell padding
                    if (value.CellPadding != String.Empty)
                        TableSelection.Table.cellPadding = value.CellPadding;
                    else
                        (TableSelection.Table as IHTMLElement).removeAttribute("cellpadding", 0);

                    // cell spacing
                    if (value.CellSpacing != String.Empty)
                        TableSelection.Table.cellSpacing = value.CellSpacing;
                    else
                        (TableSelection.Table as IHTMLElement).removeAttribute("cellspacing", 0);

                    // border
                    if (value.BorderSize != String.Empty)
                        TableSelection.Table.border = value.BorderSize;
                    else
                        (TableSelection.Table as IHTMLElement).removeAttribute("border", 0);

                    // get the existing width, calculate the delta, then spread
                    // the delta across all of the columns
                    var existingWidth = TableHelper.GetTableWidth(TableSelection.Table);

                    if (existingWidth.Units == PixelPercentUnits.Pixels)
                    {
                        int changeInWidth = value.Width - existingWidth;
                        IHTMLTableRow firstRow = TableSelection.Table.rows.item(0, 0) as IHTMLTableRow;
                        if (firstRow.cells.length > 0)
                        {
                            int changePerColumn = changeInWidth / firstRow.cells.length;
                            int leftoverChange = changeInWidth % firstRow.cells.length;
                            foreach (IHTMLTableCell cell in firstRow.cells)
                            {
                                HTMLTableColumn column = new HTMLTableColumn(TableSelection.Table, cell);
                                column.Width = column.Width + changePerColumn + leftoverChange;
                                leftoverChange = 0; // allocate only once
                            }
                        }
                    }

                    // also set the width of the whole table to match the columns
                    TableSelection.Table.width = value.Width.ToString();

                    // update borders
                    TableHelper.UpdateDesignTimeBorders(TableSelection.Table);

                    // sync widths
                    TableHelper.SynchronizeCellAndTableWidthsForEditing(TableSelection.Table);

                    undoUnit.Commit();
                }

            }
        }

        public void DeleteTable()
        {
            using (IUndoUnit undoUnit = _editorContext.CreateUndoUnit())
            {
                HTMLElementHelper.RemoveElement(TableSelection.Table as IHTMLElement);
                _editorContext.FireSelectionChanged();
                undoUnit.Commit();
            }
        }

        #endregion

        #region Row Level Commands

        private RowProperties RowProperties
        {
            get
            {
                IHTMLTableRow row = TableSelection.BeginRow;

                RowProperties rowProperties = new RowProperties();
                rowProperties.Height = TableHelper.GetRowHeight(row);
                rowProperties.CellProperties.BackgroundColor = GetBackgroundColorForRow(row);
                rowProperties.CellProperties.HorizontalAlignment = GetAlignmentForRow(row);
                rowProperties.CellProperties.VerticalAlignment = GetVAlignmentForRow(row);

                return rowProperties;
            }
            set
            {
                using (IUndoUnit undoUnit = _editorContext.CreateUndoUnit())
                {
                    RowProperties existingRowProperties = RowProperties;

                    IHTMLTableRow row = TableSelection.BeginRow;

                    // height
                    if (value.Height > 0)
                    {
                        (row as IHTMLTableRow2).height = value.Height;
                    }
                    else
                    {
                        (row as IHTMLElement).removeAttribute("height", 0);
                    }

                    foreach (IHTMLTableCell cell in row.cells)
                    {
                        // background color
                        if (!value.CellProperties.BackgroundColor.IsMixed) // mixed means hands ooff
                        {
                            if (value.CellProperties.BackgroundColor.Color != Color.Empty)
                            {
                                cell.bgColor = ColorHelper.ColorToString(value.CellProperties.BackgroundColor.Color);
                            }
                            else
                            {
                                (cell as IHTMLElement).removeAttribute("bgcolor", 0);
                            }
                        }

                        // horizontal alignment
                        if (value.CellProperties.HorizontalAlignment != HorizontalAlignment.Mixed) // mixed means hands off
                        {
                            if (value.CellProperties.HorizontalAlignment != HorizontalAlignment.Left)
                            {
                                cell.align = TableHelper.GetHtmlAlignmentForAlignment(value.CellProperties.HorizontalAlignment);
                            }
                            else
                            {
                                (cell as IHTMLElement).removeAttribute("align", 0);
                            }
                        }

                        // vertical alignment
                        if (value.CellProperties.VerticalAlignment != VerticalAlignment.Mixed) // mixed means hands off
                        {
                            if (value.CellProperties.VerticalAlignment != VerticalAlignment.Middle)
                            {
                                cell.vAlign = TableHelper.GetHtmlAlignmentForVAlignment(value.CellProperties.VerticalAlignment);
                            }
                            else
                            {
                                (cell as IHTMLElement).removeAttribute("valign", 0);
                            }
                        }
                    }

                    undoUnit.Commit();
                }
            }
        }

        private IHTMLTableRow InsertRowAbove()
        {
            return InsertRow(false);
        }

        private IHTMLTableRow InsertRowBelow()
        {
            return InsertRow(true);
        }

        private void MoveRowUp()
        {
            // no-op if this is the first row or the selection spans more than one row
            if ((TableSelection.BeginRow.rowIndex == 0) || !TableSelection.SingleRowSelected)
                return;

            using (_editorContext.DamageServices.CreateDamageTracker(_editorContext.MarkupServices.CreateMarkupRange(TableSelection.Table as IHTMLElement, true), false))
            {
                // determine the source row and target row
                IHTMLTableRow sourceRow = TableSelection.BeginRow;
                IHTMLTableRow targetRow = TableSelection.Table.rows.item(sourceRow.rowIndex - 1, sourceRow.rowIndex - 1) as IHTMLTableRow;

                using (IUndoUnit undoUnit = _editorContext.CreateUndoUnit())
                {
                    // swap them
                    HTMLElementHelper.SwapElements(sourceRow as IHTMLElement, targetRow as IHTMLElement);

                    undoUnit.Commit();
                }
            }

        }

        private void MoveRowDown()
        {
            // no-op if this is the last row or the selection spans more than one row
            if ((TableSelection.BeginRow.rowIndex >= (TableSelection.Table.rows.length - 1)) || !TableSelection.SingleRowSelected)
                return;

            using (_editorContext.DamageServices.CreateDamageTracker(_editorContext.MarkupServices.CreateMarkupRange(TableSelection.Table as IHTMLElement, true), false))
            {
                // determine the source row and target row
                IHTMLTableRow sourceRow = TableSelection.BeginRow;
                IHTMLTableRow targetRow = TableSelection.Table.rows.item(sourceRow.rowIndex + 1, sourceRow.rowIndex + 1) as IHTMLTableRow;

                using (IUndoUnit undoUnit = _editorContext.CreateUndoUnit())
                {
                    // swap them
                    HTMLElementHelper.SwapElements(sourceRow as IHTMLElement, targetRow as IHTMLElement);

                    undoUnit.Commit();
                }
            }
        }

        private void DeleteRows()
        {
            using (IUndoUnit undoUnit = _editorContext.CreateUndoUnit())
            {
                int endRowIndex = TableSelection.EndRow.rowIndex;
                int endColumnIndex = TableSelection.EndColumn.Index;
                // collect up the rows to remove
                ArrayList rowsToRemove = new ArrayList();
                for (int i = TableSelection.BeginRow.rowIndex; i <= endRowIndex; i++)
                    rowsToRemove.Add(TableSelection.Table.rows.item(i, i));

                // The selection gets into a very bad state if we allow HTMLElementHelper.RemoveElement below
                // to remove the element(s) that are selected.
                // To avoid this, we move the selection before deleting the rows.
                MarkupRange newSelection;
                if (endRowIndex == TableSelection.Table.rows.length - 1)
                {
                    // Deleting bottom-most row. Move selection below the table
                    newSelection = _editorContext.MarkupServices.CreateMarkupRange(TableSelection.Table as IHTMLElement, true);
                    newSelection.Collapse(false);
                }
                else
                {
                    // Move selection into next row
                    int nextRowIndex = endRowIndex + 1;
                    newSelection = _editorContext.MarkupServices.CreateMarkupRange((IHTMLElement)((IHTMLTableRow)TableSelection.Table.rows.item(nextRowIndex, nextRowIndex)).cells.item(endColumnIndex, endColumnIndex), false);
                    newSelection.Collapse(true);
                }
                newSelection.ToTextRange().select(); ;

                // delete the rows
                foreach (IHTMLElement row in rowsToRemove)
                    HTMLElementHelper.RemoveElement(row);

                // delete the entire table if this action left it empty
                DeleteTableIfEmpty();

                // commit the changes
                undoUnit.Commit();
            }
        }

        #endregion

        #region Column Oriented Commands

        private ColumnProperties ColumnProperties
        {
            get
            {
                HTMLTableColumn tableColumn = TableSelection.BeginColumn;
                ColumnProperties columnProperties = new ColumnProperties();
                columnProperties.Width = tableColumn.Width;
                columnProperties.CellProperties.BackgroundColor = tableColumn.BackgroundColor;
                columnProperties.CellProperties.HorizontalAlignment = tableColumn.HorizontalAlignment;
                columnProperties.CellProperties.VerticalAlignment = tableColumn.VerticalAlignment;
                return columnProperties;
            }
            set
            {
                using (IUndoUnit undoUnit = _editorContext.CreateUndoUnit())
                {
                    HTMLTableColumn tableColumn = TableSelection.BeginColumn;
                    tableColumn.Width = value.Width;
                    tableColumn.BackgroundColor = value.CellProperties.BackgroundColor;
                    tableColumn.HorizontalAlignment = value.CellProperties.HorizontalAlignment;
                    tableColumn.VerticalAlignment = value.CellProperties.VerticalAlignment;

                    TableHelper.SynchronizeCellAndTableWidthsForEditing(TableSelection.Table);

                    undoUnit.Commit();
                }
            }
        }

        private void InsertColumnLeft()
        {

            using (IUndoUnit undoUnit = _editorContext.CreateUndoUnit())
            {
                InsertAdjacentColumn(TableSelection.BeginColumn, false);

                TableHelper.SynchronizeCellAndTableWidthsForEditing(TableSelection.Table);

                undoUnit.Commit();
            }

        }

        private void InsertColumnRight()
        {

            using (IUndoUnit undoUnit = _editorContext.CreateUndoUnit())
            {
                InsertAdjacentColumn(TableSelection.EndColumn, true);

                TableHelper.SynchronizeCellAndTableWidthsForEditing(TableSelection.Table);

                undoUnit.Commit();
            }

        }

        private void MoveColumnLeft()
        {
            // no-op if this is the first column or the selection spans more than one column
            if ((TableSelection.BeginColumn.Index == 0) || !TableSelection.SingleColumnSelected)
                return;

            // determine the source and target column indexes
            int sourceIndex = TableSelection.BeginColumn.Index;
            int targetIndex = sourceIndex - 1;

            using (_editorContext.DamageServices.CreateDamageTracker(_editorContext.MarkupServices.CreateMarkupRange(TableSelection.Table as IHTMLElement, true), false))
            {
                using (IUndoUnit undoUnit = _editorContext.CreateUndoUnit())
                {
                    // swap the cells in the respective columns
                    foreach (IHTMLTableRow row in TableSelection.Table.rows)
                    {
                        if (row.cells.length > sourceIndex)
                        {
                            HTMLElementHelper.SwapElements(
                                row.cells.item(sourceIndex, sourceIndex) as IHTMLElement,
                                row.cells.item(targetIndex, targetIndex) as IHTMLElement);
                        }
                    }

                    undoUnit.Commit();
                }
            }

        }

        private void MoveColumnRight()
        {
            // no-op if this is the last column or the selection spans more than one column
            if ((TableSelection.BeginColumn.Index >= (TableSelection.BeginRow.cells.length - 1)) || !TableSelection.SingleColumnSelected)
                return;

            // determine the source and target column indexes
            int sourceIndex = TableSelection.BeginColumn.Index;
            int targetIndex = sourceIndex + 1;

            using (_editorContext.DamageServices.CreateDamageTracker(_editorContext.MarkupServices.CreateMarkupRange(TableSelection.Table as IHTMLElement, true), false))
            {
                using (IUndoUnit undoUnit = _editorContext.CreateUndoUnit())
                {
                    // swap the cells in the respective columns
                    foreach (IHTMLTableRow row in TableSelection.Table.rows)
                    {
                        if (row.cells.length > targetIndex)
                        {
                            HTMLElementHelper.SwapElements(
                                row.cells.item(sourceIndex, sourceIndex) as IHTMLElement,
                                row.cells.item(targetIndex, targetIndex) as IHTMLElement);
                        }
                    }

                    undoUnit.Commit();
                }
            }

        }

        private void DeleteColumns()
        {
            // index to delete
            int endRowIndex = TableSelection.EndRow.rowIndex;
            int beginColumnIndex = TableSelection.BeginColumn.Index;
            int endColumnIndex = TableSelection.EndColumn.Index;

            // accumulate a list of cells in the columns
            ArrayList columnCells = new ArrayList();
            foreach (IHTMLTableRow row in TableSelection.Table.rows)
            {
                // if the row contains a cell in this column
                if (row.cells.length > beginColumnIndex)
                {
                    for (int i = beginColumnIndex; i <= endColumnIndex; i++)
                        columnCells.Add(row.cells.item(i, i));
                }
            }

            using (IUndoUnit undoUnit = _editorContext.CreateUndoUnit())
            {
                // The selection gets into a very bad state if we allow HTMLElementHelper.RemoveElement below
                // to remove the element(s) that are selected.
                // To avoid this, we move the selection before deleting the columns.
                MarkupRange newSelection;
                if (endColumnIndex == ((IHTMLTableRow)TableSelection.Table.rows.item(endRowIndex, endRowIndex)).cells.length - 1)
                {
                    // Deleting rightmost column. Move selection outside of table
                    newSelection = _editorContext.MarkupServices.CreateMarkupRange(TableSelection.Table as IHTMLElement, true);
                    newSelection.Collapse(false);
                }
                else
                {
                    // Move selection into next column
                    int nextColumnIndex = endColumnIndex + 1;
                    newSelection = _editorContext.MarkupServices.CreateMarkupRange((IHTMLElement)TableSelection.EndRow.cells.item(nextColumnIndex, nextColumnIndex), false);
                    newSelection.Collapse(true);
                }

                newSelection.ToTextRange().select();

                // delete each cell
                foreach (IHTMLTableCell cell in columnCells)
                    HTMLElementHelper.RemoveElement(cell as IHTMLElement);

                TableHelper.SynchronizeCellAndTableWidthsForEditing(TableSelection.Table);

                DeleteTableIfEmpty();

                undoUnit.Commit();
            }
        }

        #endregion

        #region Cell Editing Oriented Commands

        private CellProperties CellProperties
        {
            get
            {
                CellProperties cellProperties = new CellProperties();
                cellProperties.BackgroundColor = new CellColor(TableHelper.GetColorForHtmlColor(TableSelection.BeginCell.bgColor));
                cellProperties.HorizontalAlignment = TableHelper.GetAlignmentForHtmlAlignment(TableSelection.BeginCell.align);
                cellProperties.VerticalAlignment = TableHelper.GetVAlignmentForHtmlAlignment(TableSelection.BeginCell.vAlign);
                return cellProperties;
            }
            set
            {
                using (IUndoUnit undoUnit = _editorContext.CreateUndoUnit())
                {
                    if (!value.BackgroundColor.IsMixed)
                    {
                        if (value.BackgroundColor.Color != Color.Empty)
                        {
                            TableSelection.BeginCell.bgColor = ColorHelper.ColorToString(value.BackgroundColor.Color);
                        }
                        else
                        {
                            (TableSelection.BeginCell as IHTMLElement).removeAttribute("bgcolor", 0);
                        }
                    }

                    if (value.HorizontalAlignment != HorizontalAlignment.Mixed)
                    {
                        if (value.HorizontalAlignment != HorizontalAlignment.Left)
                        {
                            TableSelection.BeginCell.align = TableHelper.GetHtmlAlignmentForAlignment(value.HorizontalAlignment);
                        }
                        else
                        {
                            (TableSelection.BeginCell as IHTMLElement).removeAttribute("align", 0);
                        }
                    }

                    if (value.VerticalAlignment != VerticalAlignment.Mixed)
                    {
                        if (value.VerticalAlignment != VerticalAlignment.Middle)
                        {
                            TableSelection.BeginCell.vAlign = TableHelper.GetHtmlAlignmentForVAlignment(value.VerticalAlignment);
                        }
                        else
                        {
                            (TableSelection.BeginCell as IHTMLElement).removeAttribute("valign", 0);
                        }
                    }

                    undoUnit.Commit();
                }
            }
        }

        private void ClearCells()
        {
            using (IUndoUnit undoUnit = _editorContext.CreateUndoUnit())
            {
                // clear the contents of the selected cells
                foreach (IHTMLElement cellElement in TableSelection.SelectedCells)
                {
                    cellElement.innerHTML = null;
                }

                // select the begin cell
                SelectCell(TableSelection.BeginCell);

                // commit
                undoUnit.Commit();
            }

        }

        /// <summary>
        /// Routine to automatically add/remove/restore &nbsp; to empty cells so that
        /// these empty cells do not "collapse" when published.
        /// </summary>
        private void MakeEmptyCellsNbsp()
        {
            IHTMLElement tableElement = TableSelection.Table as IHTMLElement;
            if (tableElement != null)
            {
                // modify document but "merge" this edit with any previously undoable action
                using (IUndoUnit undoUnit = _editorContext.CreateInvisibleUndoUnit())
                {
                    MarkupRange tableMarkupRange = _editorContext.MarkupServices.CreateMarkupRange(tableElement);

                    foreach (IHTMLElement cellElement in tableMarkupRange.GetElements(ElementFilters.TABLE_CELL_ELEMENT, true))
                    {
                        if ((cellElement.innerHTML == null) || cellElement.innerHTML == String.Empty)
                        {
                            cellElement.innerHTML = EMPTY_CELL;
                        }
                    }

                    undoUnit.Commit();
                }
            }
        }

        private void MakeEmptyCellsNull()
        {
            IHTMLElement tableElement = TableSelection.Table as IHTMLElement;
            if (tableElement != null)
            {
                // modify document but "merge" this edit with any previously undoable action
                using (IUndoUnit undoUnit = _editorContext.CreateInvisibleUndoUnit())
                {
                    MarkupRange tableMarkupRange = _editorContext.MarkupServices.CreateMarkupRange(tableElement);

                    foreach (IHTMLElement cellElement in tableMarkupRange.GetElements(ElementFilters.TABLE_CELL_ELEMENT, true))
                    {
                        string innerHTML = cellElement.innerHTML;
                        if ((innerHTML == EMPTY_CELL) || innerHTML == String.Empty)
                        {
                            cellElement.innerHTML = null;
                        }
                    }

                    undoUnit.Commit();
                }
            }
        }

        #endregion

        #region Selection Mutating Operations

        private void InsertLineBreak()
        {
            // modify document but "merge" this edit with any previously undoable action
            using (IUndoUnit undoUnit = _editorContext.CreateInvisibleUndoUnit())
            {
                // alias MarkupServices
                MshtmlMarkupServices markupServices = _editorContext.MarkupServices;

                // create and insert a BR element
                IHTMLElement brElement = markupServices.CreateElement(_ELEMENT_TAG_ID.TAGID_BR, null);
                markupServices.InsertElement(brElement, MarkupRange.Start, MarkupRange.End);

                // move the selection to be just after the BR
                MarkupRange range = markupServices.CreateMarkupRange();
                range.Start.MoveAdjacentToElement(brElement, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
                range.End.MoveToPointer(range.Start);
                range.ToTextRange().select();

                // commit the edit
                undoUnit.Commit();
            }
        }

        private void SelectNextCell()
        {
            // determine the target cell (if the selection spans more than one cell then
            // the target cell is the very first cell)
            IHTMLElement targetCell = null;
            if (!TableSelection.HasContiguousSelection)
            {
                MarkupPointer afterCurrentCellPointer = _editorContext.MarkupServices.CreateMarkupPointer(TableSelection.BeginCell as IHTMLElement, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
                MarkupPointer endOfTablePointer = _editorContext.MarkupServices.CreateMarkupPointer(TableSelection.Table as IHTMLElement, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeEnd);
                targetCell = afterCurrentCellPointer.SeekElementRight(ElementFilters.TABLE_CELL_ELEMENT, endOfTablePointer);
            }
            else
            {
                targetCell = TableSelection.BeginCell as IHTMLElement;
            }

            // if there is no target cell then we need to insert a row and use its
            // first cell as the target cell
            if (targetCell == null)
            {
                IHTMLTableRow newRow = InsertRowBelow();
                if (newRow != null)
                {
                    // record the target cell
                    targetCell = newRow.cells.item(0, 0) as IHTMLElement;
                }
            }

            // move the selection to the target cell and select the contents of the cell
            if (targetCell != null)
            {
                SelectCell(targetCell as IHTMLTableCell);
            }
        }

        private void SelectPreviousCell()
        {
            // determine the target cell (if the selection spans more than one cell then
            // the target cell is the very first cell)
            IHTMLElement targetCell = null;
            if (!TableSelection.HasContiguousSelection)
            {
                MarkupPointer beforeCurrentCellPointer = _editorContext.MarkupServices.CreateMarkupPointer(TableSelection.BeginCell as IHTMLElement, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
                MarkupPointer beginningOfTablePointer = _editorContext.MarkupServices.CreateMarkupPointer(TableSelection.Table as IHTMLElement, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin);
                targetCell = beforeCurrentCellPointer.SeekElementLeft(ElementFilters.TABLE_CELL_ELEMENT, beginningOfTablePointer);
            }
            else
            {
                targetCell = TableSelection.BeginCell as IHTMLElement;
            }

            if (targetCell != null)
            {
                SelectCell(targetCell as IHTMLTableCell);
            }
        }

        private void SelectCell(IHTMLTableCell cell)
        {
            IHTMLElement cellElement = cell as IHTMLElement;

            // move the selection to the beginning of the cell
            MarkupRange markupRange = _editorContext.MarkupServices.CreateMarkupRange(cellElement);

            //  if the cell is empty then collapse the selection
            if (cellElement.innerHTML == null)
                markupRange.End.MoveToPointer(markupRange.Start);

            IHTMLTxtRange textRange = markupRange.ToTextRange();
            textRange.select();
        }

        #endregion

        #region Private Helpers

        private MarkupRange MarkupRange
        {
            get
            {
                return _markupRange;
            }
        }

        private TableSelection TableSelection
        {
            get
            {
                if (_tableSelection == null)
                    _tableSelection = new TableSelection(MarkupRange);
                return _tableSelection;
            }
        }

        private IHTMLTableRow InsertRow(bool below)
        {
            // determine the html element relative position and base row
            IHTMLTableRow selectedRow = below ? TableSelection.EndRow : TableSelection.BeginRow;

            // screen out no selected row
            if (selectedRow == null)
                return null;

            using (IUndoUnit undoUnit = _editorContext.CreateUndoUnit())
            {
                // create a new row, copy the source row's attributes to it, and insert it
                IHTMLTableRow newRow = TableSelection.Table.insertRow(below ? selectedRow.rowIndex + 1 : selectedRow.rowIndex) as IHTMLTableRow;
                HTMLElementHelper.CopyAttributes(selectedRow as IHTMLElement, newRow as IHTMLElement);

                // insert a like number of cells into the new row, cloning the attributes of the
                // corresponding cells from the source row
                for (int i = 0; i < selectedRow.cells.length; i++)
                {
                    IHTMLTableCell newCell = InsertCell(newRow);
                    HTMLElementHelper.CopyAttributes(selectedRow.cells.item(i, i) as IHTMLElement, newCell as IHTMLElement);
                }

                // commit the changes
                undoUnit.Commit();

                // return the row for further manipulation
                return newRow;
            }
        }

        private CellColor GetBackgroundColorForRow(IHTMLTableRow row)
        {
            CellColor cellColor = new CellColor();
            bool firstCellProcessed = false;
            foreach (IHTMLTableCell cell in row.cells)
            {
                // for the first cell processed, note its color
                if (!firstCellProcessed)
                {
                    cellColor.Color = TableHelper.GetColorForHtmlColor(cell.bgColor);
                    firstCellProcessed = true;
                }
                // for subsequent cells, if any of them differ from the first cell
                // then the background color is mixed
                else
                {
                    if (cellColor.Color != TableHelper.GetColorForHtmlColor(cell.bgColor))
                    {
                        cellColor.IsMixed = true;
                        break;
                    }
                }
            }
            return cellColor;
        }

        private HorizontalAlignment GetAlignmentForRow(IHTMLTableRow row)
        {
            HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left;
            bool firstCellProcessed = false;
            foreach (IHTMLTableCell cell in row.cells)
            {
                // for the first cell processed, note its alignment
                if (!firstCellProcessed)
                {
                    horizontalAlignment = TableHelper.GetAlignmentForHtmlAlignment(cell.align);
                    firstCellProcessed = true;
                }
                // for subsequent cells, if any of them differ from the first cell
                // then the alignment is mixed
                else
                {
                    if (horizontalAlignment != TableHelper.GetAlignmentForHtmlAlignment(cell.align))
                    {
                        horizontalAlignment = HorizontalAlignment.Mixed;
                        break;
                    }
                }
            }

            return horizontalAlignment;
        }

        private VerticalAlignment GetVAlignmentForRow(IHTMLTableRow row)
        {
            VerticalAlignment verticalAlignment = VerticalAlignment.Middle;
            bool firstCellProcessed = false;

            foreach (IHTMLTableCell cell in row.cells)
            {
                // for the first cell processed, note its alignment
                if (!firstCellProcessed)
                {
                    verticalAlignment = TableHelper.GetVAlignmentForHtmlAlignment(cell.vAlign);
                    firstCellProcessed = true;
                }
                // for subsequent cells, if any of them differ from the first cell
                // then the alignment is mixed
                else
                {
                    if (verticalAlignment != TableHelper.GetVAlignmentForHtmlAlignment(cell.vAlign))
                    {
                        verticalAlignment = VerticalAlignment.Mixed;
                        break;
                    }
                }

            }

            return verticalAlignment;
        }

        private bool TableParentElementFilter(IHTMLElement e)
        {
            if (ElementFilters.BLOCK_ELEMENTS(e))
                return true;
            else if (e is IHTMLTableCell)
                return true;
            else
                return false;
        }

        private IHTMLTableCell InsertCell(IHTMLTableRow row)
        {
            return InsertCell(row, -1);
        }

        private IHTMLTableCell InsertCell(IHTMLTableRow row, int index)
        {
            IHTMLElement cell = (IHTMLElement)row.insertCell(index);
            return cell as IHTMLTableCell;
        }

        private void DeleteTableIfEmpty()
        {
            IHTMLElement tableElement = TableSelection.Table as IHTMLElement;
            MarkupRange tableMarkupRange = _editorContext.MarkupServices.CreateMarkupRange(tableElement);
            if (tableMarkupRange.GetElements(ElementFilters.TABLE_CELL_ELEMENT, true).Length == 0)
            {
                HTMLElementHelper.RemoveElement(tableElement);
                _editorContext.FireSelectionChanged();
            }

        }

        private void InsertAdjacentColumn(HTMLTableColumn column, bool after)
        {
            // set the specified alignment for each cell in the column
            foreach (IHTMLTableRow row in TableSelection.Table.rows)
            {
                if (row.cells.length > column.Index)
                {
                    // insert the cell
                    IHTMLTableCell newCell = InsertCell(row, after ? column.Index + 1 : column.Index);

                    // copy the attributes of the source cell for this column
                    HTMLElementHelper.CopyAttributes(column.BaseCell as IHTMLElement, newCell as IHTMLElement);
                }
            }
        }

        private class SelectionPreserver : IDisposable
        {
            public SelectionPreserver(MarkupRange selectedMarkupRange)
            {
                _preservedMarkupRange = selectedMarkupRange.Clone();
                _preservedMarkupRange.Start.Cling = true;
                _preservedMarkupRange.End.Cling = true;
            }

            public void Dispose()
            {
                _preservedMarkupRange.ToTextRange().select();
            }

            private MarkupRange _preservedMarkupRange = null;
        }

        #endregion

        #region Private Data and Constants

        private IHtmlEditorComponentContext _editorContext;
        private MarkupRange _markupRange;
        private TableSelection _tableSelection;

        private const string EMPTY_CELL = "&nbsp;";

        #endregion

    }

    public class TableCreationParameters
    {
        public TableCreationParameters(int rows, int columns, TableProperties properties)
        {
            _rows = rows;
            _columns = columns;
            _properties = properties;
        }

        public int Rows { get { return _rows; } }
        private int _rows;
        public int Columns { get { return _columns; } }
        private int _columns;
        public TableProperties Properties { get { return _properties; } }
        private TableProperties _properties;

    }

    public class TableProperties
    {
        public string CellPadding
        {
            get { return _cellPadding; }
            set { _cellPadding = value; }
        }
        private string _cellPadding = String.Empty;

        public string CellSpacing
        {
            get { return _cellSpacing; }
            set { _cellSpacing = value; }
        }
        private string _cellSpacing = String.Empty;

        public string BorderSize
        {
            get { return _borderSize; }
            set { _borderSize = value; }
        }
        private string _borderSize = String.Empty;

        public PixelPercent Width { get; set; }
    }

    public class CellProperties
    {
        public CellColor BackgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; }
        }
        private CellColor _backgroundColor = new CellColor();

        public HorizontalAlignment HorizontalAlignment
        {
            get { return _horizontalAlignment; }
            set { _horizontalAlignment = value; }
        }
        private HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Left;

        public VerticalAlignment VerticalAlignment
        {
            get { return _verticalAlignment; }
            set { _verticalAlignment = value; }
        }
        private VerticalAlignment _verticalAlignment = VerticalAlignment.Middle;
    }

    public class RowProperties
    {
        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }
        private int _height = 0;

        public CellProperties CellProperties
        {
            get { return _cellProperties; }
            set { _cellProperties = value; }
        }
        private CellProperties _cellProperties = new CellProperties();
    }

    public class ColumnProperties
    {
        public PixelPercent Width { get; set; }


        public CellProperties CellProperties
        {
            get { return _cellProperties; }
            set { _cellProperties = value; }
        }
        private CellProperties _cellProperties = new CellProperties();
    }

    public class CellColor
    {
        public CellColor()
        {
        }

        public CellColor(Color color)
        {
            Color = color;
        }

        public bool IsMixed
        {
            get
            {
                return _isMixed;
            }
            set
            {
                _isMixed = value;
                if (_isMixed)
                    Color = Color.Empty;
            }
        }
        private bool _isMixed = false;

        public Color Color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
            }
        }
        private Color _color = Color.Empty;
    }

    public enum HorizontalAlignment
    {
        Mixed,
        Left,
        Center,
        Right
    }

    public enum VerticalAlignment
    {
        Mixed,
        Top,
        Middle,
        Bottom
    }

}
