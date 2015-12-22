// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;
using OpenLiveWriter.InternalWriterPlugin.Controls;

namespace OpenLiveWriter.InternalWriterPlugin.Controls
{

    public class MapScrollControl : System.Windows.Forms.UserControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private Bitmap _backgroundBitmap;
        private const int BUTTON_INSET = 5;

        private class DirectionalButton : MapBitmapButton
        {
            public DirectionalButton(VEOrientation direction, string resourceName)
                : base(resourceName)
            {
                _direction = direction;
            }

            public VEOrientation Direction { get { return _direction; } }
            private VEOrientation _direction;
        }
        private class NorthButton : DirectionalButton { public NorthButton() : base(VEOrientation.North, "RoadNorth") { } }
        private class EastButton : DirectionalButton { public EastButton() : base(VEOrientation.East, "RoadEast") { } }
        private class SouthButton : DirectionalButton { public SouthButton() : base(VEOrientation.South, "RoadSouth") { } }
        private class WestButton : DirectionalButton { public WestButton() : base(VEOrientation.West, "RoadWest") { } }

        private NorthButton _northButton = new NorthButton();
        private SouthButton _southButton = new SouthButton();
        private EastButton _eastButton = new EastButton();
        private WestButton _westButton = new WestButton();

        public MapScrollControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            //	Turn on double buffered painting.
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            //set accessible names
            _northButton.AccessibleName = Res.Get(StringId.MapDirectionNorth);
            _eastButton.AccessibleName = Res.Get(StringId.MapDirectionEast);
            _southButton.AccessibleName = Res.Get(StringId.MapDirectionSouth);
            _westButton.AccessibleName = Res.Get(StringId.MapDirectionWest);

            // initialize the background bitmap
            _backgroundBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.RoadNSEWFrame.png");

            Size = GetPreferredSize();

            int centerX = Utility.Center(ScaleX(_northButton.Width), Width);
            int centerY = Utility.Center(ScaleY(_eastButton.Height), Height);
            _northButton.Location = new Point(centerX, 0); ;
            _eastButton.Location = new Point(Width - ScaleX(_eastButton.Width) - ScaleX(1), centerY);
            _southButton.Location = new Point(centerX, Height - ScaleY(_southButton.Height) - ScaleY(1));
            _westButton.Location = new Point(0, centerY);

            // add the directional buttons and subscribe to their click event
            Controls.Add(_northButton);
            _northButton.Click += new EventHandler(_directionalButton_Click);
            Controls.Add(_eastButton);
            _eastButton.Click += new EventHandler(_directionalButton_Click);
            Controls.Add(_southButton);
            _southButton.Click += new EventHandler(_directionalButton_Click);
            Controls.Add(_westButton);
            _westButton.Click += new EventHandler(_directionalButton_Click);
        }

        private Size GetPreferredSize()
        {
            return new Size((int)((_backgroundBitmap.Width + (BUTTON_INSET * 2)) * scale.X),
            (int)((_backgroundBitmap.Height + (BUTTON_INSET * 2)) * scale.Y));
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            SaveScale(factor.Width, factor.Height);
            base.ScaleControl(factor, specified);
        }

        protected override void ScaleCore(float dx, float dy)
        {
            SaveScale(dx, dy);
            base.ScaleCore(dx, dy);
        }

        private void SaveScale(float dx, float dy)
        {
            scale = new PointF(scale.X * dx, scale.Y * dy);
        }
        private PointF scale = new PointF(1f, 1f);

        private int ScaleX(int x)
        {
            return (int)(x * scale.X);
        }

        private int ScaleY(int y)
        {
            return (int)(y * scale.Y);
        }

        public event DirectionalButtonClickedHandler DirectionalButtonClicked;

        protected void OnDirectionalButtonClicked(DirectionalButtonClickedEventArgs ea)
        {
            if (DirectionalButtonClicked != null)
                DirectionalButtonClicked(this, ea);
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            Size preferredSize = GetPreferredSize();
            base.SetBoundsCore(x, y, preferredSize.Width, preferredSize.Height, specified);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.DrawImage(_backgroundBitmap, ScaleX(BUTTON_INSET), ScaleY(BUTTON_INSET), ScaleX(_backgroundBitmap.Width), ScaleY(_backgroundBitmap.Height));
        }

        private void _directionalButton_Click(object sender, EventArgs e)
        {
            OnDirectionalButtonClicked(new DirectionalButtonClickedEventArgs((sender as DirectionalButton).Direction));
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            this.RightToLeft = RightToLeft.No;
        }
        #endregion

    }

    public class DirectionalButtonClickedEventArgs : EventArgs
    {
        public DirectionalButtonClickedEventArgs(VEOrientation direction)
        {
            _direction = direction;
        }

        public VEOrientation Direction { get { return _direction; } }

        private VEOrientation _direction;
    }

    public delegate void DirectionalButtonClickedHandler(object sender, DirectionalButtonClickedEventArgs ea);

}
