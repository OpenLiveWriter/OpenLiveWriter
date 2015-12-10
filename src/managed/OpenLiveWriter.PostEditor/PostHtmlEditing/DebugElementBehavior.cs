// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#if DEBUG_BEHAVIORS
using System;
using System.Drawing;
using mshtml;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    internal class DebugElementBehavior : EditableRegionElementBehavior
    {
        public DebugElementBehavior(IHtmlEditorComponentContext editorContext, IHTMLElement prevEditableRegion, IHTMLElement nextEditableRegion)
            : base(editorContext, prevEditableRegion, nextEditableRegion)
        {
        }

        protected override void OnElementAttached()
        {
            //PaintRegionBorder = Color.Blue;
            base.OnElementAttached ();
        }

        protected override void OnElementDetached()
        {
            base.OnElementDetached ();
        }

        public override void GetPainterInfo(ref _HTML_PAINTER_INFO pInfo)
        {
            base.GetPainterInfo(ref pInfo);
        }

        public override void OnDraw(Graphics g, Rectangle drawBounds, RECT rcBounds, RECT rcUpdate, int lDrawFlags, IntPtr hdc, IntPtr pvDrawObject)
        {
            IHTMLElement2 element2 = HTMLElement as IHTMLElement2;
            //g.DrawRectangle(new Pen(Color.Red, 1), GetPaintRectangle(GetClientRectangle(ELEMENT_REGION.CONTENT), rcBounds));
            //g.DrawRectangle(new Pen(Color.Orange, 1), GetPaintRectangle(GetClientRectangle(ELEMENT_REGION.BORDER), rcBounds));
            //g.DrawRectangle(new Pen(Color.Blue, 1), GetPaintRectangle(GetClientRectangle(ELEMENT_REGION.PADDING), rcBounds));
            //g.DrawRectangle(new Pen(Color.Gold, 1), GetPaintRectangle(GetClientRectangle(ELEMENT_REGION.MARGIN), rcBounds));

            //highlight the line that the caret is currently placed on.
            //Rectangle lineRect = GetPaintRectangle(GetLineClientRectangle(EditorContext.SelectedMarkupRange.Start), rcBounds);
            //g.DrawRectangle(new Pen(Color.Violet, 1), lineRect);
        }
    }
}
#endif
