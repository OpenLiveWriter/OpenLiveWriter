// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    public abstract class PropertiesEditingElementBehavior : HtmlEditorElementBehavior
    {

        public PropertiesEditingElementBehavior(IHtmlEditorComponentContext editorContext)
            : base(editorContext)
        {
        }

        protected override void OnElementAttached()
        {
            base.OnElementAttached ();

            EditorContext.PreHandleEvent +=new HtmlEditDesignerEventHandler(EditorContext_PreHandleEvent);
        }

        protected override void OnSelectedChanged()
        {
            // make sure we paint the dongle
            Invalidate(_widgetArea) ;
        }

        public override void GetPainterInfo(ref _HTML_PAINTER_INFO pInfo)
        {
            // ensure we paint above everything (including selection handles)
            pInfo.lFlags = (int)_HTML_PAINTER.HTMLPAINTER_OPAQUE ;
            pInfo.lZOrder = (int)_HTML_PAINT_ZORDER.HTMLPAINT_ZORDER_WINDOW_TOP ;

            // expand to the right to accomodate our widget
            pInfo.rcExpand.top = 0 ;
            pInfo.rcExpand.bottom = 0 ;
            pInfo.rcExpand.left = 0 ;
            pInfo.rcExpand.right = _widgetEnabled.Width - WIDGET_HORIZONTAL_OVERLAY ;
        }

        protected virtual bool WidgetActive
        {
            get
            {
                return Selected;
            }
        }

        private int EditorContext_PreHandleEvent(int inEvtDispId, IHTMLEventObj pIEventObj)
        {
            if ( Selected )
            {
                if ( inEvtDispId == DISPID_HTMLELEMENTEVENTS2.ONMOUSEMOVE )
                {
                    MouseInWidget = ClientPointInWidget(pIEventObj.clientX, pIEventObj.clientY) ;
                }

                else if ( inEvtDispId == DISPID_HTMLELEMENTEVENTS2.ONMOUSEDOWN )
                {
                    if ( WidgetActive && MouseInWidget )
                    {
                        // show the properties form
                        ShowProperties() ;

                        // eat the click
                        return HRESULT.S_OK ;
                    }
                }
            }

            return HRESULT.S_FALSE;
        }

        protected virtual void ShowProperties()
        {
            Invalidate(_widgetArea);
        }

        private Rectangle CalculateWidgetScreenBounds()
        {
            // translate local location to client location
            POINT localLocation = new POINT();
            localLocation.x = _widgetLocation.X ;
            localLocation.y = _widgetLocation.Y ;
            POINT clientLocation = new POINT();
            HTMLPaintSite.TransformLocalToGlobal(localLocation, ref clientLocation);

            Point screenLocation = EditorContext.PointToScreen(new Point(clientLocation.x, clientLocation.y)) ;
            return new Rectangle(screenLocation, new Size(_widgetArea.Width, _widgetArea.Height) );
        }

        private bool MouseInWidget
        {
            get
            {
                return _mouseInWidget ;
            }
            set
            {
                if ( _mouseInWidget != value )
                {
                    _mouseInWidget = value ;

                    Invalidate(_widgetArea) ;
                }

                // set the arrow if the mouse in the widget
                if ( _mouseInWidget )
                    Cursor.Current = Cursors.Arrow ;

                // toggle cursor override appropriately
                EditorContext.OverrideCursor = _mouseInWidget ;
            }
        }
        private bool _mouseInWidget ;

        private bool ClientPointInWidget( int x, int y )
        {
            // calculate mouse position in local coordinates
            POINT clientMouseLocation = new POINT();
            clientMouseLocation.x = x ;
            clientMouseLocation.y = y ;
            POINT localMouseLocation = new POINT();
            HTMLPaintSite.TransformGlobalToLocal( clientMouseLocation, ref localMouseLocation ) ;

            // is the mouse in the widget?
            return _widgetArea.Contains( localMouseLocation.x, localMouseLocation.y ) ;
        }

        public override void OnResize(SIZE size)
        {
            _elementSize = new Size(size.cx, size.cy) ;
            _widgetLocation = new Point(_elementSize.Width - _widgetEnabled.Width, WIDGET_VERTICAL_OFFSET) ;
            _widgetArea = new Rectangle( _widgetLocation, _widgetEnabled.Size );
        }

        public override void Draw(RECT rcBounds, RECT rcUpdate, int lDrawFlags, IntPtr hdc, IntPtr pvDrawObject)
        {
            if ( WidgetActive )
            {
                using ( Graphics g = Graphics.FromHdc(hdc) )
                {
                    g.DrawImage( MouseInWidget ? _widgetSelected : _widgetEnabled,
                                 rcBounds.right-_widgetEnabled.Width, rcBounds.top + WIDGET_VERTICAL_OFFSET );
                }
            }
        }

        protected void InvalidateWidget()
        {
            Invalidate(_widgetArea) ;
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            if (!_disposed)
            {
                if (disposeManagedResources)
                {
                    Debug.Assert(EditorContext != null);
                    EditorContext.PreHandleEvent -= new HtmlEditDesignerEventHandler(EditorContext_PreHandleEvent);
                }

                _disposed = true;
            }

            base.Dispose(disposeManagedResources);
        }

        private Size _elementSize ;
        private Point _widgetLocation ;
        private Rectangle _widgetArea ;
        private bool _disposed;

        private const int WIDGET_HORIZONTAL_OVERLAY = 5 ;
        private const int WIDGET_VERTICAL_OFFSET = 5 ;
        private Bitmap _widgetEnabled = ResourceHelper.LoadAssemblyResourceBitmap("PostHtmlEditing.Images.PropertiesHandleEnabled.png", true) ;
        private Bitmap _widgetSelected = ResourceHelper.LoadAssemblyResourceBitmap("PostHtmlEditing.Images.PropertiesHandleSelected.png", true) ;
    }
}
