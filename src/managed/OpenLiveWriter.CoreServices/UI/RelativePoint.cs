// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;

namespace OpenLiveWriter.CoreServices.UI
{
    public struct RelativePoint
    {
        private readonly Anchor _anchor;
        private readonly Point _point;

        public enum Anchor
        {
            TopLeft,
            TopCenter,
            TopRight,
            MiddleLeft,
            MiddleCenter,
            MiddleRight,
            BottomLeft,
            BottomCenter,
            BottomRight
        }

        public RelativePoint(Anchor anchor, Point point)
        {
            _anchor = anchor;
            _point = point;
        }

        public RelativePoint(Anchor anchor, int x, int y)
        {
            _anchor = anchor;
            _point = new Point(x, y);
        }

        public Point ToAbsolute(Rectangle rect)
        {
            return ToAbsolute(_anchor, _point, rect);
        }

        public static Point ToAbsolute(Anchor anchor, Point point, Rectangle rect)
        {
            switch (anchor)
            {
                case Anchor.TopLeft:
                    point.Offset(rect.Left, rect.Top);
                    break;
                case Anchor.TopCenter:
                    point.Offset(rect.Left + (rect.Width / 2), rect.Top);
                    break;
                case Anchor.TopRight:
                    point.Offset(rect.Right, rect.Top);
                    break;
                case Anchor.MiddleLeft:
                    point.Offset(rect.Left, rect.Top + (rect.Height / 2));
                    break;
                case Anchor.MiddleCenter:
                    point.Offset(rect.Left + (rect.Width / 2), rect.Top + (rect.Height / 2));
                    break;
                case Anchor.MiddleRight:
                    point.Offset(rect.Right, rect.Top + (rect.Height / 2));
                    break;
                case Anchor.BottomLeft:
                    point.Offset(rect.Left, rect.Bottom);
                    break;
                case Anchor.BottomCenter:
                    point.Offset(rect.Left + (rect.Width / 2), rect.Bottom);
                    break;
                case Anchor.BottomRight:
                    point.Offset(rect.Right, rect.Bottom);
                    break;
            }
            return point;
        }
    }
}
