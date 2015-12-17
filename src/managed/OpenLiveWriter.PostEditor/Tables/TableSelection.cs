// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.PostEditor.Tables
{
    internal class TableSelection
    {
        public TableSelection(MarkupRange markupRange)
        {
            // calculate the begin and end cells
            IHTMLTableCell beginCell;
            IHTMLTableCell endCell;
            ArrayList selectedCells;
            FindCellRange(markupRange, out selectedCells, out beginCell, out endCell);

            // see if the two cells have a single containing table
            IHTMLTable table = GetSelectedTable(beginCell, endCell, markupRange) as IHTMLTable;

            // if we have a table then calculate the rest of our states
            if (table != null)
            {
                // validate the table selection
                if (ValidateTableSelection(table, markupRange, out _entireTableSelected))
                {
                    _table = table;

                    _beginCell = beginCell;
                    _endCell = endCell;

                    // filter selected cells to only include direct descendents of this table (no
                    // cells from nested tables)
                    _selectedCells = new ArrayList();
                    foreach (IHTMLElement cell in selectedCells)
                        if (HTMLElementHelper.ElementsAreEqual(TableHelper.GetContainingTableElement(cell) as IHTMLElement, _table as IHTMLElement))
                            _selectedCells.Add(cell);

                    _hasContiguousSelection = !HTMLElementHelper.ElementsAreEqual(_beginCell as IHTMLElement, _endCell as IHTMLElement);

                    _beginRow = GetContainingRowForCell(beginCell);
                    _endRow = GetContainingRowForCell(endCell);

                    _beginColumn = new HTMLTableColumn(_table, beginCell);
                    _endColumn = new HTMLTableColumn(_table, endCell);
                }
            }
        }

        public bool HasContiguousSelection
        {
            get { return _hasContiguousSelection; }
        }
        private bool _hasContiguousSelection = false;

        public IHTMLTable Table
        {
            get { return _table; }
        }
        private IHTMLTable _table = null;

        public bool EntireTableSelected
        {
            get { return _entireTableSelected; }
        }
        private bool _entireTableSelected;

        public IHTMLTableCell BeginCell
        {
            get { return _beginCell; }
        }
        private IHTMLTableCell _beginCell;

        public IHTMLTableCell EndCell
        {
            get { return _endCell; }
        }
        private IHTMLTableCell _endCell;

        public ArrayList SelectedCells
        {
            get { return _selectedCells; }
        }
        private ArrayList _selectedCells = new ArrayList();

        public bool SelectionSpansAllCells
        {
            get
            {
                return (_table as IHTMLTable2).cells.length == SelectedCells.Count;
            }
        }

        public IHTMLTableRow BeginRow
        {
            get { return _beginRow; }
        }
        private IHTMLTableRow _beginRow = null;

        public IHTMLTableRow EndRow
        {
            get { return _endRow; }
        }
        private IHTMLTableRow _endRow = null;

        public bool SingleRowSelected
        {
            get { return HTMLElementHelper.ElementsAreEqual(_beginRow as IHTMLElement, _endRow as IHTMLElement); }
        }

        public HTMLTableColumn BeginColumn
        {
            get { return _beginColumn; }
        }
        private HTMLTableColumn _beginColumn = null;

        public HTMLTableColumn EndColumn
        {
            get { return _endColumn; }
        }
        private HTMLTableColumn _endColumn = null;

        public bool SingleColumnSelected
        {
            get { return !HasContiguousSelection; }
        }

        private void FindCellRange(MarkupRange selectedRange, out ArrayList selectedCells, out IHTMLTableCell beginCell, out IHTMLTableCell endCell)
        {
            // default to null
            beginCell = null;
            endCell = null;
            selectedCells = new ArrayList();

            // JJA: fix bug #476623 -- at document initialization the selected markup range
            // may not yet be positioned so protect ourselves in this case
            if (!selectedRange.Positioned)
                return;

            // query for all of the table cells within the range
            selectedCells.AddRange(selectedRange.GetElements(ElementFilters.TABLE_CELL_ELEMENT, false));

            // extract the begin and end cells
            if (selectedCells.Count == 0)
            {
                // see if the selection is contained within a single cell
                beginCell = selectedRange.Start.GetParentElement(ElementFilters.TABLE_CELL_ELEMENT) as IHTMLTableCell;
                if (beginCell != null)
                {
                    // make sure the cell is content editable (it would not be in the case
                    // where the call to GetParentElement went all the way out of the body
                    // and found a cell that was part of the containing template)
                    if (!(beginCell as IHTMLElement3).isContentEditable)
                        beginCell = null;
                }

                endCell = beginCell;

                selectedCells.Add(beginCell);
            }
            else if (selectedCells.Count == 1)
            {
                beginCell = selectedCells[0] as IHTMLTableCell;
                endCell = selectedCells[0] as IHTMLTableCell;
            }
            else
            {
                beginCell = selectedCells[0] as IHTMLTableCell;
                endCell = selectedCells[selectedCells.Count - 1] as IHTMLTableCell;
            }
        }

        private IHTMLElement GetSelectedTable(IHTMLTableCell beginCell, IHTMLTableCell endCell, MarkupRange selectionMarkupRange)
        {
            // screen null cases
            if (beginCell == null || endCell == null)
                return null;

            // get containing tables
            IHTMLTable beginTable = TableHelper.GetContainingTableElement(beginCell as IHTMLElement);
            IHTMLTable endTable = TableHelper.GetContainingTableElement(endCell as IHTMLElement);

            // see if they are from the same table
            if (HTMLElementHelper.ElementsAreEqual(beginTable as IHTMLElement, endTable as IHTMLElement))
            {
                return beginTable as IHTMLElement;
            }
            else
                return null;
        }

        private IHTMLTableRow GetContainingRowForCell(IHTMLTableCell tableCell)
        {
            // search up the parent hierarchy
            IHTMLElement element = tableCell as IHTMLElement;
            while (element != null)
            {
                if (element is IHTMLTableRow)
                {
                    return element as IHTMLTableRow;
                }

                // search parent
                element = element.parentElement;
            }

            // didn't find a row
            return null;
        }

        private bool ValidateTableSelection(IHTMLTable table, MarkupRange selectionMarkupRange, out bool tableFullySelected)
        {
            // assume table is not fully selected
            tableFullySelected = false;

            // first check to see that this is a "Writer" editable table
            if (!TableHelper.TableElementContainsWriterEditingMark(table as IHTMLElement))
                return false;

            // get elemental objects we need to analyze the table
            IHTMLElement tableElement = table as IHTMLElement;
            MarkupRange tableMarkupRange = selectionMarkupRange.Clone();
            tableMarkupRange.MoveToElement(table as IHTMLElement, true);

            // analyze selection
            bool selectionAtTableStart = tableMarkupRange.Start.IsEqualTo(selectionMarkupRange.Start);
            bool selectionAtTableEnd = tableMarkupRange.End.IsEqualTo(selectionMarkupRange.End);

            // is the table fully selected?
            if (selectionAtTableStart && selectionAtTableEnd)
            {
                tableFullySelected = true;
                return true;
            }
            else
            {
                MarkupRange selectionMarkupRange2 = selectionMarkupRange.Clone();
                // is the selection bounded by the table
                IHTMLElement beginParentTable = selectionMarkupRange2.Start.SeekElementLeft(ElementFilters.CreateEqualFilter(tableElement));
                IHTMLElement endParentTable = selectionMarkupRange2.End.SeekElementRight(ElementFilters.CreateEqualFilter(tableElement));
                return beginParentTable != null && endParentTable != null;
            }
        }

    }
}
