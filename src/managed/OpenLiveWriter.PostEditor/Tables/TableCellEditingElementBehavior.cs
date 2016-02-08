// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using OpenLiveWriter.HtmlEditor;
using mshtml;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.PostEditor.Tables
{

    internal class TableCellEditingElementBehavior : HtmlEditorElementBehavior
    {
        public TableCellEditingElementBehavior(IHtmlEditorComponentContext editorContext)
            : base(editorContext)
        {
        }

        protected override void OnElementAttached()
        {
            // call base
            base.OnElementAttached();

            // determine whether the cell is editable
            _cellIsEditable = TableHelper.TableElementIsEditable(HTMLElement, ElementRange);

            // if editable then do our thing
            if (_cellIsEditable)
            {
                // add ourselves to the list of cell element behaviors
                TableEditingContext.AddCellBehavior(this);

                // if the table has no borders then set a runtime style that lets the user see the borders
                TableHelper.UpdateDesignTimeBorders(
                    TableHelper.GetContainingTableElement(HTMLElement),
                    HTMLElement as IHTMLElement2);
            }
        }

        protected override bool QueryElementSelected()
        {
            return false;
        }

        protected override void OnSelectedChanged()
        {
        }

        public Point TransformGlobalToLocal(Point ptGlobal)
        {
            POINT pointLocal = new POINT();
            POINT pointGlobal = new POINT();
            pointGlobal.x = ptGlobal.X;
            pointGlobal.y = ptGlobal.Y;
            HTMLPaintSite.TransformGlobalToLocal(pointGlobal, ref pointLocal);
            return new Point(pointLocal.x, pointLocal.y);
        }

        public bool DrawSelectionBorder
        {
            get
            {
                return _drawSelectionBorder;
            }
            set
            {
                if (value != _drawSelectionBorder)
                {
                    _drawSelectionBorder = value;

                    Invalidate();
                }
            }
        }
        private bool _drawSelectionBorder = false;

        public override void GetPainterInfo(ref mshtml._HTML_PAINTER_INFO pInfo)
        {
            // ensure we paint above everything (including selection handles)
            pInfo.lFlags = (int)_HTML_PAINTER.HTMLPAINTER_OPAQUE;
            pInfo.lZOrder = (int)_HTML_PAINT_ZORDER.HTMLPAINT_ZORDER_WINDOW_TOP;
        }

        public override void Draw(RECT rcBounds, RECT rcUpdate, int lDrawFlags, IntPtr hdc, IntPtr pvDrawObject)
        {
            if (_cellIsEditable)
            {
                if (DrawSelectionBorder)
                {
                    using (Graphics g = Graphics.FromHdc(hdc))
                    {
                        Rectangle rcBoundsRect = new Rectangle(rcBounds.left, rcBounds.top, rcBounds.right - rcBounds.left - 1, rcBounds.bottom - rcBounds.top - 1);
                        g.DrawRectangle(SystemPens.Highlight, rcBoundsRect);
                    }
                }
            }
        }

        private TableEditingContext TableEditingContext
        {
            get
            {
                Debug.Assert(EditorContext != null);
                if (_tableEditingContext == null)
                    _tableEditingContext = new TableEditingContext(EditorContext);
                return _tableEditingContext;
            }
        }
        private TableEditingContext _tableEditingContext;

        private bool LeftMouseButtonIsDown
        {
            get
            {
                return (Control.MouseButtons & MouseButtons.Left) > 0;
            }
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            if (!_disposed)
            {
                if (disposeManagedResources)
                {
                    if (_cellIsEditable)
                    {
                        TableEditingContext.RemoveCellBehavior(this);
                    }
                }

                _disposed = true;
            }

            base.Dispose(disposeManagedResources);
        }
        private bool _disposed;

        // assume true so initial query about our selection state executes
        private bool _cellIsEditable = true;
    }
}
