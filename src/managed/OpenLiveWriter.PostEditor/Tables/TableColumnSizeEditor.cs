// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Mshtml;
using mshtml;

namespace OpenLiveWriter.PostEditor.Tables
{

    internal class TableColumnSizeEditor : IDisposable
    {
        public TableColumnSizeEditor(IHTMLTable table, IHtmlEditorComponentContext editorContext, IHTMLPaintSiteRaw paintSite)
        {
            Debug.Assert(((IHTMLElement)table).offsetHeight > 0 && ((IHTMLElement)table).offsetWidth > 0,
                         "TableColumnSizeEditor unexpectedly attached to a table with no height and/or width!");

            // save references
            _table = table;
            _editorContext = editorContext;
            _paintSite = paintSite;

            // initialize sizing
            _sizingOperation = new SizingOperation(_editorContext, _table);

            // initialize table editing context
            _tableEditingContext = new TableEditingContext(editorContext);

            // subscribe to events
            _editorContext.PreHandleEvent += new OpenLiveWriter.Mshtml.HtmlEditDesignerEventHandler(_editorContext_PreHandleEvent);
        }

        public void Dispose()
        {
            _editorContext.PreHandleEvent -= new OpenLiveWriter.Mshtml.HtmlEditDesignerEventHandler(_editorContext_PreHandleEvent);
        }

        private int _editorContext_PreHandleEvent(int inEvtDispId, IHTMLEventObj pIEventObj)
        {
            try
            {
                switch (inEvtDispId)
                {
                    case DISPID_HTMLELEMENTEVENTS2.ONMOUSEMOVE:
                    case DISPID_HTMLELEMENTEVENTS2.ONMOUSEDOWN:
                    case DISPID_HTMLELEMENTEVENTS2.ONMOUSEUP:
                        return HandleMouseEvent(inEvtDispId, pIEventObj);

                    default:
                        return HRESULT.S_FALSE;
                }
            }
            catch (Exception ex)
            {
                // log error
                Trace.Fail("Unexpected error during TableColumnSizeEditor PreHandleEvent: " + ex.ToString());

                // reset state
                _sizingOperation.EndSizing();

                // event not handled
                return HRESULT.S_FALSE;
            }
        }

        private int HandleMouseEvent(int inEvtDispId, IHTMLEventObj pIEventObj)
        {
            // WinLive 160252: MSHTML throws a COMException with HRESULT 0x8000FFFF (E_UNEXPECTED) when calling
            // IHTMLPaintSite.TransformGlobalToLocal if the table has no height.
            IHTMLElement tableElement = (IHTMLElement)_table;
            if (tableElement.offsetHeight <= 0 || tableElement.offsetWidth <= 0)
            {
                return HRESULT.S_FALSE;
            }

            // compute the element local coordinates of the point
            POINT clientMouseLocation = new POINT();
            clientMouseLocation.x = pIEventObj.clientX;
            clientMouseLocation.y = pIEventObj.clientY;
            POINT localMouseLocation = new POINT();
            _paintSite.TransformGlobalToLocal(clientMouseLocation, ref localMouseLocation);

            // determine if the point is within our bounds
            int tableWidth = tableElement.offsetWidth + 4; // extra padding for mouse handling at right edge
            Rectangle elementBounds = new Rectangle(-1, -1, tableWidth, tableElement.offsetHeight + 1);
            bool mouseInElement = elementBounds.Contains(localMouseLocation.x, localMouseLocation.y);

            if (mouseInElement || _sizingOperation.InProgress)
            {
                // create args
                TableColumnMouseEventArgs mouseEventArgs = new TableColumnMouseEventArgs(
                    new Point(clientMouseLocation.x, clientMouseLocation.y),
                    new Point(localMouseLocation.x, localMouseLocation.y));

                // fire the event
                switch (inEvtDispId)
                {
                    case DISPID_HTMLELEMENTEVENTS2.ONMOUSEMOVE:
                        OnMouseMove(mouseEventArgs);
                        break;
                    case DISPID_HTMLELEMENTEVENTS2.ONMOUSEDOWN:
                        OnMouseDown(mouseEventArgs);
                        break;
                    case DISPID_HTMLELEMENTEVENTS2.ONMOUSEUP:
                        OnMouseUp(mouseEventArgs);
                        break;
                    default:
                        Trace.Fail("unexpected event id");
                        break;
                }

                // indicate whether we should mask the event from the editor
                return mouseEventArgs.Handled ? HRESULT.S_OK : HRESULT.S_FALSE;

            }
            else
            {
                // if the mouse is not inside the element the end sizing
                _sizingOperation.EndSizing();
            }

            // event not handled
            return HRESULT.S_FALSE;
        }

        private IHTMLElement GetTargetCell(Point clientPoint)
        {
            // maximum amount of scanning buffer is based on cell spacing
            int maxScanningRange = Math.Max(TableHelper.GetAttributeAsInteger(_table.cellSpacing), 2);

            // copy client point so we can modify the x-coordinate while scanning
            Point targetPoint = new Point(clientPoint.X, clientPoint.Y);

            // if we go past the end of the table allow the cell closest to the cursor
            // to become the target cell (necessary for sizing the table larger)
            Point xTargetPoint = new Point(targetPoint.X, targetPoint.Y);
            IHTMLTableCell targetCell = null;
            while (targetCell == null && xTargetPoint.X >= (targetPoint.X - maxScanningRange))   // 0 )
            {
                // determine the cell we are over
                targetCell = _editorContext.ElementFromClientPoint(xTargetPoint) as IHTMLTableCell;

                // screen cells that don't belong to us
                if (!HTMLElementHelper.ElementsAreEqual(_table as IHTMLElement, TableHelper.GetContainingTableElement(targetCell as IHTMLElement) as IHTMLElement))
                {
                    targetCell = null;
                }

                xTargetPoint.X--;
            }

            // if we got a target cell then ensure that the point is over the document area
            if (targetCell != null)
            {
                if (_editorContext.PointIsOverDocumentArea(clientPoint))
                {
                    return targetCell as IHTMLElement;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

        }

        private void OnMouseMove(TableColumnMouseEventArgs ea)
        {
            if (_sizingOperation.InProgress)
            {
                // mouse move when we are in an active sizing state
                _sizingOperation.ContinueSizing(ea.ClientPoint.X);
                ea.Handled = true;
            }
            else
            {
                // mouse move when we are just tracking the cursor (not sizing)
                HandleMouseMove(ea);
            }
        }

        private void OnMouseDown(TableColumnMouseEventArgs ea)
        {
            if (_sizingOperation.Pending)
            {
                _sizingOperation.BeginSizing(GetTargetCell(ea.ClientPoint) as IHTMLTableCell);

                ea.Handled = true;
            }
        }

        private void OnMouseUp(TableColumnMouseEventArgs ea)
        {
            if (_sizingOperation.InProgress)
            {
                _sizingOperation.EndSizing();

                ea.Handled = true;
            }
        }

        private void HandleMouseMove(TableColumnMouseEventArgs ea)
        {
            // cell element we are over
            IHTMLElement targetCell = GetTargetCell(ea.ClientPoint);

            // if there is no element then we are done
            if (targetCell == null)
            {
                // reset state
                _sizingOperation.EndSizing();
                return;
            }

            // get the cell and row
            IHTMLTableCell cell = targetCell as IHTMLTableCell;
            IHTMLTableRow row = TableHelper.GetContainingRowElement(cell);

            // convert the client point to cell-local coordinates & calcualte our comparison x values
            TableCellEditingElementBehavior cellBehavior = _tableEditingContext.GetCellBehavior(targetCell);
            if (cellBehavior == null)
            {
                _sizingOperation.ClearPending();
                return;
            }

            Point cellLocalMousePt = cellBehavior.TransformGlobalToLocal(ea.ClientPoint);
            int cellSpacing = TableHelper.GetAttributeAsInteger(_table.cellSpacing);
            int cellSpacingOffset = cellSpacing / 2;
            int compareX = cellLocalMousePt.X;
            int cellStartX = 0 - cellSpacingOffset;
            int cellEndX = targetCell.offsetWidth + cellSpacingOffset;

            // if the mouse is near the edge of the cell then update the pending sizing action
            // (unless the mouse is near the edge of the first cell where no sizing is supported)
            if (MouseNearCellEdge(compareX, cellStartX, cellSpacing) || MouseNearCellEdge(compareX, cellEndX, cellSpacing))
            {
                if (MouseNearCellEdge(compareX, cellStartX, cellSpacing))
                {
                    if (cell.cellIndex > 0)
                    {
                        int leftIndex = cell.cellIndex - 1;
                        int rightIndex = cell.cellIndex;
                        _sizingOperation.TrackPending(ea.ClientPoint.X, leftIndex, rightIndex);
                        ea.Handled = true;
                    }
                    else
                    {
                        _sizingOperation.ClearPending();
                    }

                }
                else if (MouseNearCellEdge(compareX, cellEndX, cellSpacing))
                {
                    int leftIndex = cell.cellIndex;
                    int rightIndex = cell.cellIndex < (row.cells.length - 1) ? cell.cellIndex + 1 : -1;

                    _sizingOperation.TrackPending(ea.ClientPoint.X, leftIndex, rightIndex);
                    ea.Handled = true;
                }
            }
            else // mouse is not near the edge of the cell, reset pending action
            {
                _sizingOperation.ClearPending();
            }
        }

        private bool MouseNearCellEdge(int mouseX, int cellEdgeX, int cellSpacing)
        {
            int hotRegion = Math.Max(cellSpacing / 2, 2);
            return Math.Abs(mouseX - cellEdgeX) <= hotRegion;
        }

        private IHTMLTable _table;
        private IHtmlEditorComponentContext _editorContext;
        private TableEditingContext _tableEditingContext;

        private IHTMLPaintSiteRaw _paintSite;

        private SizingOperation _sizingOperation;
    }

    internal class SizingOperation
    {
        public SizingOperation(IHtmlEditorComponentContext editorContext, IHTMLTable table)
        {
            _editorContext = editorContext;
            _table = table;
        }

        public void TrackPending(int clientX, int leftColumnIndex, int rightColumnIndex)
        {
            _pending = true;
            _lastClientX = clientX;
            _pendingLeftColumnIndex = leftColumnIndex;
            _pendingRightColumnIndex = rightColumnIndex;

            ShowSizingCursor();
        }

        public void ClearPending()
        {
            if (_pending)
                ShowDefaultCursor(Cursors.IBeam);

            _pending = false;
        }

        public void BeginSizing(IHTMLTableCell targetCell)
        {
            // set pending state to false
            _pending = false;

            // sanity check
            if (_sizingUndoUnit != null)
            {
                Trace.Fail("Never call BeginSizing twice consecutively (must call EndSizing first)");
                _sizingUndoUnit.Dispose();
                _sizingUndoUnit = null;
            }

            // create new undo unit
            _sizingUndoUnit = _editorContext.CreateUndoUnit();

            // calculate left and right columns
            InitializeSizingContext(targetCell);

            // make sure we show the sizing cursor
            ShowSizingCursor();
        }

        public void ContinueSizing(int clientX)
        {
            // calculate offset
            int offset = clientX - _lastClientX;

            // no-op for zero offset
            if (offset == 0)
                return;

            if (!_cellWidthsFixed)
            {
                // do fixups once during each size operation
                // This actually causes the cells to change size in many cases, which is incredibly annoying.
                // TableHelper.SynchronizeCellWidthsForEditing(_table);
                _cellWidthsFixed = true;
            }

            // check for abort condition
            if (AbortOnMinimumColumnWidth(offset))
                return;

            // perform the sizing (middle of the table mode)
            if (_rightColumn != null)
            {
                _leftColumn.Width = Math.Max(_leftColumn.Width + offset, MINIMUM_COLUMN_WIDTH);
                _rightColumn.Width = Math.Max(_rightColumn.Width - offset, MINIMUM_COLUMN_WIDTH);
            }
            // perform the sizing (end of the table mode)
            else
            {
                // change left column
                _leftColumn.Width = Math.Max(_leftColumn.Width + offset, MINIMUM_COLUMN_WIDTH);

                // set the table width to prevent table wierdness
                TableHelper.SynchronizeTableWidthForEditing(_table);
            }

            // update last client x
            _lastClientX = clientX;

            // make sure we continue showing the sizing cursor
            ShowSizingCursor();
        }

        public void EndSizing()
        {
            try
            {
                if (InProgress)
                {
                    // finish sizing operation
                    _sizingUndoUnit.Commit();
                    _sizingUndoUnit.Dispose();

                    // show the default cursor (force ibeam)
                    ShowDefaultCursor(Cursors.IBeam);

                    // For some reason, changing the size doesn't cause the
                    // document to become dirty. (Bug 776385)
                    _editorContext.ForceDirty();
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception ending column sizing: " + ex.ToString());
            }
            finally
            {
                // reset state
                _sizingUndoUnit = null;
                _leftColumn = null;
                _rightColumn = null;
                _cellWidthsFixed = true;

                // make sure pending flag is cleared
                _pending = false;
            }
        }

        public bool Pending
        {
            get { return _pending; }
        }

        public bool InProgress
        {
            get { return _sizingUndoUnit != null; }
        }

        public void ShowCursor(Cursor cursor)
        {
            _editorContext.OverrideCursor = true;
            Cursor.Current = cursor;
        }

        public void ShowSizingCursor()
        {
            ShowCursor(Cursors.VSplit);
        }

        public void ShowDefaultCursor()
        {
            ShowDefaultCursor(Cursors.Default);
        }

        public void ShowDefaultCursor(Cursor forceCursor)
        {
            if (forceCursor != Cursors.Default)
                Cursor.Current = forceCursor;

            _editorContext.OverrideCursor = false;
        }

        private void InitializeSizingContext(IHTMLTableCell targetCell)
        {
            IHTMLTableRow row = TableHelper.GetContainingRowElement(targetCell as IHTMLTableCell);
            IHTMLTableCell leftCell = row.cells.item(_pendingLeftColumnIndex, _pendingLeftColumnIndex) as IHTMLTableCell;
            _leftColumn = new HTMLTableColumn(_table, leftCell);

            if (_pendingRightColumnIndex != -1)
            {
                IHTMLTableCell rightCell = row.cells.item(_pendingRightColumnIndex, _pendingRightColumnIndex) as IHTMLTableCell;
                _rightColumn = new HTMLTableColumn(_table, rightCell);
            }
            else
            {
                _rightColumn = null;
            }

            // force a fixup of cell widths on the next call to ContinueSizing
            // (we do this during ContinueSizing so that table column borders don't
            // visible "jump" on MouseDown)

            _cellWidthsFixed = false;
        }

        private bool AbortOnMinimumColumnWidth(int offset)
        {
            if (offset < 0 && _leftColumn.Width <= MINIMUM_COLUMN_WIDTH)
            {
                EndSizing();
                return true;
            }
            else if (offset > 0 && _rightColumn != null && _rightColumn.Width <= MINIMUM_COLUMN_WIDTH)
            {
                EndSizing();
                return true;
            }
            else
            {
                return false;
            }
        }
        private const int MINIMUM_COLUMN_WIDTH = 10;

        private IHTMLTable _table;
        private IHtmlEditorComponentContext _editorContext;

        private bool _pending = false;
        private int _lastClientX;
        private int _pendingLeftColumnIndex;
        private int _pendingRightColumnIndex;

        private IUndoUnit _sizingUndoUnit = null;
        private HTMLTableColumn _leftColumn = null;
        private HTMLTableColumn _rightColumn = null;
        private bool _cellWidthsFixed = true;

    }

    internal class TableColumnMouseEventArgs : EventArgs
    {
        public TableColumnMouseEventArgs(Point clientPoint, Point localPoint)
        {
            _clientPoint = clientPoint;
            _localPoint = localPoint;
        }

        public Point ClientPoint
        {
            get { return _clientPoint; }
        }
        private Point _clientPoint;

        public Point LocalPoint
        {
            get { return _localPoint; }
        }
        private Point _localPoint;

        public bool Handled
        {
            get { return _handled; }
            set { _handled = value; }
        }
        private bool _handled = false;
    }
}
