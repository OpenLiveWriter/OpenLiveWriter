// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace OpenLiveWriter.CoreServices.Layout
{
    public class AutoGrow : IDisposable
    {
        private Control _container;
        private Size _margin;
        private AnchorStyles _direction;
        private bool _allowShrink;
        private bool _allowAnchoring = false;

        private bool canceled = false;

        public AutoGrow(Control container, AnchorStyles direction, bool allowShrink)
        {
            Debug.Assert(container.Dock != DockStyle.Fill, "Can't autogrow a docked container");

            switch (direction)
            {
                case AnchorStyles.None:
                case AnchorStyles.Right:
                case AnchorStyles.Bottom:
                case AnchorStyles.Right | AnchorStyles.Bottom:
                    break;
                default:
                    throw new ArgumentException("Only combinations of Right and Bottom are allowed");
            }

            _direction = direction;
            _container = container;
            Point maxChildPoint = GetMaxChildPoint(container);
            _margin = new Size(container.Width - maxChildPoint.X, container.Height - maxChildPoint.Y);
            _allowShrink = allowShrink;
        }

        private static Point GetMaxChildPoint(Control container)
        {
            if (container.Controls.Count == 0)
                return Point.Empty;

            int right = int.MinValue;
            int bottom = int.MinValue;
            foreach (Control c in container.Controls)
            {
                right = Math.Max(right, c.Right);
                bottom = Math.Max(bottom, c.Bottom);
            }
            return new Point(right, bottom);
        }

        public bool IsCanceled
        {
            get { return canceled; }
        }

        public bool AllowAnchoring
        {
            get { return _allowAnchoring; }
            set { _allowAnchoring = value; }
        }

        public void Cancel()
        {
            canceled = true;
        }

        public void Dispose()
        {
            if (!canceled)
            {
                Point newPoint = GetMaxChildPoint(_container);

                int newWidth = _container.Width;
                int newHeight = _container.Height;

                if ((_direction & AnchorStyles.Right) == AnchorStyles.Right)
                {
                    newWidth = newPoint.X + _margin.Width;
                    if (!_allowShrink && newWidth < _container.Width)
                        newWidth = _container.Width;
                }

                if ((_direction & AnchorStyles.Bottom) == AnchorStyles.Bottom)
                {
                    newHeight = newPoint.Y + _margin.Height;
                    if (!_allowShrink && newHeight < _container.Height)
                        newHeight = _container.Height;
                }

                if (newWidth != _container.Width || newHeight != _container.Height)
                {
                    Control[] controls = (Control[])new ArrayList(_container.Controls).ToArray(typeof(Control));
                    using (_allowAnchoring ? null : LayoutHelper.SuspendAnchoring(controls))
                    {
                        _container.Size = new Size(newWidth, newHeight);
                    }
                }
            }
        }
    }
}
