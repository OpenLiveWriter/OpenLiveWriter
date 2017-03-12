// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors
{
    /// <summary>
    /// Provides a property dongle for opening the side bar.
    /// </summary>
    public class SidebarDongleController
    {
        private const int WIDGET_HORIZONTAL_OVERLAY = 10;
        private const int WIDGET_VERTICAL_OFFSET = 5;
        private ElementControlBehavior _parent;
        private PropertiesDongleControl _dongle;
        private IBlogPostSidebarContext _sidebarContext;
        private SidebarDongleController(ElementControlBehavior parent, IBlogPostSidebarContext sidebarContext)
        {
            _parent = parent;
            _sidebarContext = sidebarContext;
            _sidebarContext.SidebarVisibleChanged += new EventHandler(_sidebarContext_SidebarVisibleChanged);

            _dongle = new PropertiesDongleControl();
            _parent.Controls.Add(_dongle);
            _dongle.Click += new EventHandler(_dongle_Click);
            _dongle.Visible = !_sidebarContext.SidebarVisible;

            AttachPropertiesDongle();
        }

        public static SidebarDongleController AttachPropertiesDongle(ElementControlBehavior parent, IBlogPostSidebarContext sidebarContext)
        {
            return new SidebarDongleController(parent, sidebarContext);
        }

        private void AttachPropertiesDongle()
        {
            _parent.SelectedChanged += new EventHandler(_parent_SelectedChanged);
            _parent.ElementSizeChanged += new EventHandler(_parent_ElementSizeChanged);
        }

        public void DetachPropertiesDongle()
        {
            _parent.SelectedChanged -= new EventHandler(_parent_SelectedChanged);
            _parent.ElementSizeChanged -= new EventHandler(_parent_ElementSizeChanged);
        }

        private void _parent_ElementSizeChanged(object sender, EventArgs e)
        {
            SetDonglePosition();
        }

        private void SetDonglePosition()
        {
            _dongle.VirtualSize = _dongle.MinimumVirtualSize;
            _dongle.VirtualLocation = new Point(_parent.ElementRectangle.Width - _dongle.VirtualWidth + WIDGET_HORIZONTAL_OVERLAY, 0 + WIDGET_VERTICAL_OFFSET);
        }

        private void _dongle_Click(object sender, EventArgs e)
        {
            _sidebarContext.SidebarVisible = true;
        }

        private void _sidebarContext_SidebarVisibleChanged(object sender, EventArgs e)
        {
            updateVisibility();
        }

        private void _parent_SelectedChanged(object sender, EventArgs e)
        {
            updateVisibility();
        }

        private void updateVisibility()
        {
            _dongle.Visible = _parent.Selected && !_sidebarContext.SidebarVisible;
            _parent.Invalidate();
        }
    }
}
