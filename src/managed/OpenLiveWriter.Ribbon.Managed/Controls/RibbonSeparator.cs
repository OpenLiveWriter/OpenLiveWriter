// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Ribbon.Managed.Rendering;

namespace OpenLiveWriter.Ribbon.Managed.Controls
{
    /// <summary>
    /// Separator control for ribbon groups.
    /// </summary>
    public class RibbonSeparator : RibbonControlBase
    {
        private bool _isVertical = true;

        /// <summary>
        /// Gets or sets whether this separator is vertical.
        /// </summary>
        public bool IsVertical
        {
            get => _isVertical;
            set
            {
                _isVertical = value;
                UpdateSize();
                Invalidate();
            }
        }

        public RibbonSeparator()
        {
            UpdateSize();
        }

        protected override void UpdateSize()
        {
            if (_isVertical)
            {
                Size = new Size(6, Height);
                MinimumSize = new Size(6, 0);
            }
            else
            {
                Size = new Size(Width, 6);
                MinimumSize = new Size(0, 6);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            RibbonRenderer.Instance.DrawSeparator(e.Graphics, ClientRectangle, _isVertical);
        }
    }
}
