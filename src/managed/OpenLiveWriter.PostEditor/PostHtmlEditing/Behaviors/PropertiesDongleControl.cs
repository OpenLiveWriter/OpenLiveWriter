// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors
{
    /// <summary>
    /// Summary description for PropertiesDongleControl.
    /// </summary>
    public class PropertiesDongleControl : BehaviorControl
    {
        private readonly Bitmap _widgetEnabled;
        private readonly Bitmap _widgetSelected;
        public PropertiesDongleControl()
        {
            _widgetEnabled = ResourceHelper.LoadAssemblyResourceBitmap("PostHtmlEditing.Images.PropertiesHandleEnabled.png", true);
            _widgetSelected = ResourceHelper.LoadAssemblyResourceBitmap("PostHtmlEditing.Images.PropertiesHandleSelected.png", true);
        }

        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    Invalidate();
                }
            }
        }
        private bool _selected;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.DrawImage(Selected ? _widgetSelected : _widgetEnabled, new Rectangle(new Point(0, 0), VirtualSize));
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            oldCursor = Cursor.Current;
            Parent.SetCursor(Cursors.Arrow);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (oldCursor != null)
            {
                Parent.SetCursor(oldCursor);
                oldCursor = null;
            }
        }
        Cursor oldCursor;

        public override Size MinimumVirtualSize
        {
            get { return _widgetSelected.Size; }
        }

        public override Size DefaultVirtualSize
        {
            get { return _widgetSelected.Size; }
        }
    }
}
