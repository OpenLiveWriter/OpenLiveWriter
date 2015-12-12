// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlEditor;
using mshtml;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.PostEditor.Tables
{

    internal class TableEditingElementBehavior : HtmlEditorElementBehavior
    {
        public TableEditingElementBehavior(IHtmlEditorComponentContext editorContext, TableEditingManager tableEditingManager)
            : base(editorContext)
        {
            _tableEditingManager = tableEditingManager;
        }

        protected override void OnElementAttached()
        {
            base.OnElementAttached();

            // If this is a "rectangular" table then automatically add the "unselectable"
            // attribute to it because we know we can support editing of it without
            // erratic behavior
            TableHelper.MakeTableWriterEditableIfRectangular(HTMLElement as IHTMLTable);

            // Is the table editable?
            _tableIsEditable = TableHelper.TableElementIsEditable(HTMLElement, ElementRange);

            if (_tableIsEditable)
            {
                // hookup column size editor and cell selection tracker
                _columnSizeEditor = new TableColumnSizeEditor(HTMLElement as IHTMLTable, EditorContext, HTMLPaintSite);
                _cellSelectionTracker = new TableCellSelectionTracker(this, TableEditingContext);

                HookEvents();

                // make it unselecteable
                HTMLElement.setAttribute("unselectable", "on", 0);

                // if the table has no borders then set a runtime style that lets the user see the borders
                TableHelper.UpdateDesignTimeBorders(HTMLElement as IHTMLTable, HTMLElement as IHTMLElement2);

                _tableEditingInitialized = true;
            }
        }

        protected void HookEvents()
        {
            // subscribe to events
            EditorContext.KeyDown += new HtmlEventHandler(EditorContext_KeyDown);
            EditorContext.SelectionChanged += new EventHandler(EditorContext_SelectionChanged);
            EditorContext.DocumentEvents.DoubleClick += new HtmlEventHandler(EditorContext_DoubleClick);
            EditorContext.HandleClear += new HtmlEditorSelectionOperationEventHandler(EditorContext_HandleClear);
            EditorContext.HandleCut += new HtmlEditorSelectionOperationEventHandler(EditorContext_HandleCut);
            EditorContext.PerformTemporaryFixupsToEditedHtml += new TemporaryFixupHandler(EditorContext_PerformTemporaryFixupsToEditedHtml);
        }

        protected void UnhookEvents()
        {
            // subscribe to events
            EditorContext.KeyDown -= new HtmlEventHandler(EditorContext_KeyDown);
            EditorContext.SelectionChanged -= new EventHandler(EditorContext_SelectionChanged);
            EditorContext.DocumentEvents.DoubleClick -= new HtmlEventHandler(EditorContext_DoubleClick);
            EditorContext.HandleClear -= new HtmlEditorSelectionOperationEventHandler(EditorContext_HandleClear);
            EditorContext.HandleCut -= new HtmlEditorSelectionOperationEventHandler(EditorContext_HandleCut);
            EditorContext.PerformTemporaryFixupsToEditedHtml -= new TemporaryFixupHandler(EditorContext_PerformTemporaryFixupsToEditedHtml);
        }

        protected override bool QueryElementSelected()
        {
            if (_tableIsEditable)
            {
                TableSelection tableSelection = new TableSelection(EditorContext.Selection.SelectedMarkupRange);
                if ((tableSelection.Table != null) && HTMLElementHelper.ElementsAreEqual(HTMLElement, tableSelection.Table as IHTMLElement))
                {
                    _currentTableSelection = tableSelection;
                    return true;
                }
                else
                {
                    _currentTableSelection = null;
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        protected override void OnSelectedChanged()
        {
            if (Selected)
            {
                // make empty cells null for cleaner editing experience
                TableEditor.MakeEmptyCellsNull(EditorContext, HTMLElement);
            }
        }

        private void EditorContext_DoubleClick(object sender, HtmlEventArgs e)
        {
            if (Selected)
                EditorContext.CommandManager.Execute(CommandId.ActivateContextualTab);
        }

        private void EditorContext_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (Attached && _tableIsEditable)
                {
                    // possible for this code to execute when HTMLElement is null, not sure why
                    if (HTMLElement == null)
                        return;

                    if (Selected)
                    {
                        // auto-expand selection to entire table if we have selected every cell
                        if (!_currentTableSelection.EntireTableSelected &&
                            (_currentTableSelection.SelectedCells.Count > 1) &&
                            _currentTableSelection.SelectionSpansAllCells)
                        {
                            // auto-expand the selection but do so asynchronously so we don't screw up other
                            // selection-changed processing or recurse back into this method
                            EditorContext.MainFrameWindow.BeginInvoke(
                                new InvokeInUIThreadDelegate(SelectEntireTable), new object[] { });
                        }

                        DrawSelectionBorder = _currentTableSelection.EntireTableSelected;
                    }
                    else
                    {
                        if (HTMLElement.parentElement != null) //  If the table is being deleted (parentless), don't bother (Bug 548926)
                            DrawSelectionBorder = false;
                    }

                    _cellSelectionTracker.EditorSelectionChanged(_currentTableSelection);
                }
            }
            catch (Exception ex)
            {
                Debug.Fail("Unexpected exception in EditorContext_SelectionChanged: " + ex.ToString());
            }
        }

        private void EditorContext_KeyDown(object o, HtmlEventArgs e)
        {
            if (Selected)
            {
                // Enter key
                if ((e.htmlEvt.keyCode == (int)Keys.Enter) &&
                    !e.htmlEvt.shiftKey && !e.htmlEvt.ctrlKey && !e.htmlEvt.altKey)
                {

                    // insert a linebreak if we are not parented by a block element
                    IHTMLElement blockScope = EditorContext.Selection.SelectedMarkupRange.Start.GetParentElement(ElementFilters.BLOCK_OR_TABLE_CELL_ELEMENTS);
                    if (blockScope is IHTMLTableCell)
                    {
                        LastChanceKeyboardHook.OnBeforeKeyHandled(this, GetKeyEventArgs(e.htmlEvt));
                        TableEditor.InsertLineBreak(EditorContext);
                        e.Cancel();
                    }
                }

                // tab key
                if ((e.htmlEvt.keyCode == (int)Keys.Tab) &&
                    !e.htmlEvt.ctrlKey && !e.htmlEvt.altKey)
                {
                    // only hijack tab and shift-tab if we are not in a list
                    IHTMLElement listItemScope = EditorContext.Selection.SelectedMarkupRange.Start.GetParentElement(ElementFilters.LIST_ITEM_ELEMENTS);
                    if (listItemScope == null)
                    {
                        if (e.htmlEvt.shiftKey)
                        {
                            LastChanceKeyboardHook.OnBeforeKeyHandled(this, GetKeyEventArgs(e.htmlEvt));
                            TableEditor.SelectPreviousCell(EditorContext);
                            e.Cancel();
                        }
                        else
                        {
                            LastChanceKeyboardHook.OnBeforeKeyHandled(this, GetKeyEventArgs(e.htmlEvt));
                            TableEditor.SelectNextCell(EditorContext);
                            e.Cancel();
                        }
                    }
                }
            }
        }

        private static KeyEventArgs GetKeyEventArgs(IHTMLEventObj htmlEvt)
        {
            return new KeyEventArgs((Keys)htmlEvt.keyCode |
                        (htmlEvt.ctrlKey ? Keys.Control : Keys.None) |
                        (htmlEvt.shiftKey ? Keys.Shift : Keys.None) |
                        (htmlEvt.altKey ? Keys.Alt : Keys.None));

        }

        private void EditorContext_HandleClear(HtmlEditorSelectionOperationEventArgs ea)
        {
            if (Selected && MultipleCellsSelected && !EntireTableSelected)
            {
                using (IUndoUnit undoUnit = EditorContext.CreateUndoUnit())
                {
                    TableEditor.ClearCells(EditorContext);
                    undoUnit.Commit();
                }

                ea.Handled = true;
            }
        }

        private void EditorContext_HandleCut(HtmlEditorSelectionOperationEventArgs ea)
        {
            if (Selected && MultipleCellsSelected && !EntireTableSelected)
            {
                EditorContext.ExecuteCommand(IDM.COPY);

                using (IUndoUnit undoUnit = EditorContext.CreateUndoUnit())
                {
                    TableEditor.ClearCells(EditorContext);
                    undoUnit.Commit();
                }

                ea.Handled = true;
            }
        }

        private static void EditorContext_PerformTemporaryFixupsToEditedHtml(TemporaryFixupArgs args)
        {
            string html = args.Html;
            if (html.Contains("table"))
            {
                StringBuilder output = new StringBuilder(html.Length);
                SimpleHtmlParser parser = new SimpleHtmlParser(html);
                for (Element el; null != (el = parser.Next());)
                {
                    output.Append(html, el.Offset, el.Length);
                    if (el is BeginTag &&
                        ((BeginTag)el).NameEquals("td"))
                    {
                        Element e = parser.Peek(0);
                        if (e is EndTag && ((EndTag)e).NameEquals("td"))
                            output.Append("&nbsp;");
                    }

                }
                args.Html = output.ToString();
            }
        }

        public override void GetPainterInfo(ref mshtml._HTML_PAINTER_INFO pInfo)
        {
            // ensure we paint above everything (including selection handles)
            pInfo.lFlags = (int)_HTML_PAINTER.HTMLPAINTER_OPAQUE;
            pInfo.lZOrder = (int)_HTML_PAINT_ZORDER.HTMLPAINT_ZORDER_WINDOW_TOP;
            pInfo.rcExpand.top = 1;
            pInfo.rcExpand.bottom = 1;
            pInfo.rcExpand.left = 1;
            pInfo.rcExpand.right = 1;
        }

        public override void Draw(RECT rcBounds, RECT rcUpdate, int lDrawFlags, IntPtr hdc, IntPtr pvDrawObject)
        {
            if (_tableIsEditable)
            {
                if (DrawSelectionBorder)
                {
                    using (Graphics g = Graphics.FromHdc(hdc))
                    {
                        Rectangle rcBoundsRect = new Rectangle(rcBounds.left, rcBounds.top, rcBounds.right - rcBounds.left - 1, rcBounds.bottom - rcBounds.top - 1);
                        g.DrawRectangle(SystemPens.Highlight, rcBoundsRect);

                        rcBoundsRect.Inflate(-1, -1);
                        g.DrawRectangle(SystemPens.Highlight, rcBoundsRect);
                    }
                }
            }
        }

        private bool MultipleCellsSelected
        {
            get
            {
                return _currentTableSelection.HasContiguousSelection;
            }
        }

        private bool EntireTableSelected
        {
            get
            {
                return _currentTableSelection.EntireTableSelected;
            }
        }

        private TableEditingContext TableEditingContext
        {
            get
            {
                if (_tableEditingContext == null)
                    _tableEditingContext = new TableEditingContext(EditorContext);
                return _tableEditingContext;
            }
        }
        private TableEditingContext _tableEditingContext;

        private void SelectEntireTable()
        {
            try
            {
                MarkupRange tableMarkupRange = EditorContext.MarkupServices.CreateMarkupRange(HTMLElement, true);
                tableMarkupRange.ToTextRange().select();
                DrawSelectionBorder = true;
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected error attempting to select entire table: " + ex.ToString());
            }
        }

        private bool DrawSelectionBorder
        {
            get
            {
                return _drawSelectionBorder;
            }
            set
            {
                if (_drawSelectionBorder != value)
                {
                    _drawSelectionBorder = value;
                    Invalidate();
                }
            }
        }
        private bool _drawSelectionBorder = false;

        protected override void Dispose(bool disposeManagedResources)
        {
            if (!_disposed)
            {
                if (disposeManagedResources)
                {
                    if (_tableEditingInitialized)
                    {
                        // dispose column size editor and cell selection tracker
                        _columnSizeEditor.Dispose();
                        _columnSizeEditor = null;
                        _cellSelectionTracker = null;

                        UnhookEvents();

                        _tableEditingManager.NotifyTableDetached();
                    }
                }

                _disposed = true;
            }

            base.Dispose(disposeManagedResources);
        }
        private bool _disposed;

        // assume true until proven otherwise (this it to cover the case where
        // QueryElementSelected is called prior to OnElementAttached)
        private bool _tableIsEditable = true;

        private bool _tableEditingInitialized;

        private TableSelection _currentTableSelection;

        private TableColumnSizeEditor _columnSizeEditor;
        private TableCellSelectionTracker _cellSelectionTracker;
        private TableEditingManager _tableEditingManager;

    }

}
