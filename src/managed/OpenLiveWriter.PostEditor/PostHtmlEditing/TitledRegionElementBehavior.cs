// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using mshtml;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    internal class TitledRegionElementBehavior : EditableRegionElementBehavior
    {
        public TitledRegionElementBehavior(IHtmlEditorComponentContext editorContext, IHTMLElement prevEditableRegion, IHTMLElement nextEditableRegion)
            : base(editorContext, prevEditableRegion, nextEditableRegion)
        {

        }

        protected override void OnElementAttached()
        {
            SetPaintColors(HTMLElement);

            base.OnElementAttached();
        }

        protected override void OnSelectedChanged()
        {
            base.OnSelectedChanged();
            RegionBorderVisible = Selected;
        }

        /// <summary>
        /// Select all of the text in the region.
        /// </summary>
        protected void SelectAll()
        {
            MarkupRange selectRange = ElementRange.Clone();
            selectRange.MoveToElement(HTMLElement, false);
            selectRange.ToTextRange().select();
        }

        public override void GetPainterInfo(ref _HTML_PAINTER_INFO pInfo)
        {
            // expand the paint region enough to make room for the painted tab.
            pInfo.rcExpand.top = Math.Max(BORDER_MARGIN_Y, pInfo.rcExpand.top);
            pInfo.rcExpand.bottom = Math.Max(BORDER_MARGIN_Y, pInfo.rcExpand.bottom);
            pInfo.rcExpand.left = Math.Max(BORDER_MARGIN_X, pInfo.rcExpand.left);
            pInfo.rcExpand.right = Math.Max(BORDER_MARGIN_X, pInfo.rcExpand.right);

            base.GetPainterInfo(ref pInfo);
        }

        protected bool RegionBorderVisible
        {
            get
            {
                return _regionBorderVisible;
            }
            set
            {
                if (_regionBorderVisible != value)
                {
                    _regionBorderVisible = value;
                    Invalidate();
                }
            }
        }
        protected bool _regionBorderVisible;

        private const int BORDER_MARGIN_X = 4; //padding for the border drawn around the region.
        private const int BORDER_MARGIN_Y = 3; //padding for the border drawn around the region.

        public override void OnDraw(Graphics g, Rectangle drawBounds, RECT rcBounds, RECT rcUpdate, int lDrawFlags, IntPtr hdc, IntPtr pvDrawObject)
        {
            base.OnDraw(g, drawBounds, rcBounds, rcUpdate, lDrawFlags, hdc, pvDrawObject);

            if (RegionBorderVisible)
            {
                //Create region around content region of the element, and add a little padding
                Rectangle bounds = GetPaintRectangle(GetClientRectangle(ELEMENT_REGION.CONTENT), rcBounds);
                int heightPadding = BORDER_MARGIN_Y;
                int widthPadding = BORDER_MARGIN_X;
                bounds.X -= widthPadding;
                bounds.Y -= heightPadding;
                bounds.Width += widthPadding * 2;
                bounds.Height += heightPadding * 2;

                Rectangle borderRectangle = bounds;
                ControlHelper.DrawRoundedRectangle(g, Pens.Gray, borderRectangle, 2);
            }
        }
    }
}
