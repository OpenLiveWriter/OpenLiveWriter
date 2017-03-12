// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace OpenLiveWriter.CoreServices.Layout
{
    internal abstract class ControlAdapter
    {
        public abstract object AdaptedObject { get; }

        public abstract bool Visible { get; }
        public abstract int Left { get; set; }
        public abstract int Top { get; set; }
        public abstract int Right { get; }
        public abstract int Bottom { get; }
        public abstract Rectangle Bounds { get; }
        public abstract Point Location { get; set; }
        public abstract Size Size { get; }

        public static ControlAdapter Create(object o)
        {
            if (o is ControlGroup)
                return new ControlGroupControlAdapter((ControlGroup)o);
            else
                return new ControlControlAdapter((Control)o);
        }
    }

    internal class ControlControlAdapter : ControlAdapter
    {
        private Control c;

        public ControlControlAdapter(Control c)
        {
            this.c = c;
        }

        public override object AdaptedObject
        {
            get { return c; }
        }

        public override bool Visible
        {
            get { return c.Visible; }
        }

        public override int Bottom
        {
            get { return c.Bottom; }
        }

        public override Rectangle Bounds
        {
            get { return c.Bounds; }
        }

        public override int Left
        {
            get { return c.Left; }
            set { c.Left = value; }
        }

        public override Point Location
        {
            get { return c.Location; }
            set { c.Location = value; }
        }

        public override int Right
        {
            get { return c.Right; }
        }

        public override Size Size
        {
            get { return c.Size; }
        }

        public override int Top
        {
            get { return c.Top; }
            set { c.Top = value; }
        }
    }

    internal class ControlGroupControlAdapter : ControlAdapter
    {
        private ControlGroup c;

        public ControlGroupControlAdapter(ControlGroup c)
        {
            this.c = c;
        }

        public override object AdaptedObject
        {
            get { return c; }
        }

        public override bool Visible
        {
            get { return true; }
        }

        public override int Bottom
        {
            get { return c.Bottom; }
        }

        public override Rectangle Bounds
        {
            get { return c.Bounds; }
        }

        public override int Left
        {
            get { return c.Left; }
            set { Location = new Point(value, Top); }
        }

        public override Point Location
        {
            get { return c.Location; }
            set { c.Location = value; }
        }

        public override int Right
        {
            get { return c.Right; }
        }

        public override Size Size
        {
            get { return c.Size; }
        }

        public override int Top
        {
            get { return c.Top; }
            set { Location = new Point(Left, value); }
        }
    }
}
