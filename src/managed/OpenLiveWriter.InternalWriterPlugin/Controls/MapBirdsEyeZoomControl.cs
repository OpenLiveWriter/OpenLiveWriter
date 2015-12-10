// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.InternalWriterPlugin.Controls
{

    public enum BirdsEyeZoomLevel
    {
        Small,
        Large
    }

    /// <summary>
    /// Summary description for BirdsEyeZoomControl.
    /// </summary>
    public class MapBirdsEyeZoomControl : System.Windows.Forms.UserControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private Bitmap _backgroundBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.BEVZoomFrame.png");
        private Bitmap _smallZoomBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.BEVZoomSmallEnabled.png");
        private Bitmap _smallZoomCheckedBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.BEVZoomSmallSelected.png");
        private Bitmap _largeZoomBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.BEVZoomLargeEnabled.png");
        private Bitmap _largeZoomCheckedBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.BEVZoomLargeSelected.png");

        private Rectangle _smallZoomBounds;
        private Rectangle _largeZoomBounds;

        public MapBirdsEyeZoomControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            //	Turn on double buffered painting.
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            Width = _backgroundBitmap.Width;
            Height = _backgroundBitmap.Height;

            _smallZoomBounds = new Rectangle(
                new Point((Width / 2) - ((_smallZoomBitmap.Width + _largeZoomBitmap.Width) / 2) - 3, 1),
                _smallZoomBitmap.Size);

            _largeZoomBounds = new Rectangle(
                new Point(_smallZoomBounds.Right, 1), _largeZoomBitmap.Size);

            // force initial paint
            _zoomLevel = BirdsEyeZoomLevel.Small;
        }

        public BirdsEyeZoomLevel ZoomLevel
        {
            get
            {
                return _zoomLevel;
            }
            set
            {
                if (value != ZoomLevel)
                {
                    _zoomLevel = value;

                    if (ZoomLevelChanged != null)
                        ZoomLevelChanged(this, EventArgs.Empty);

                    Invalidate();
                }
            }

        }
        private BirdsEyeZoomLevel _zoomLevel;

        public event EventHandler ZoomLevelChanged;

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, ScaleX(_backgroundBitmap.Width), ScaleY(_backgroundBitmap.Height), specified);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // draw border
            e.Graphics.DrawImage(_backgroundBitmap, 0, 0, ScaleX(_backgroundBitmap.Width), ScaleY(_backgroundBitmap.Height));

            // draw zoom bitmaps
            Bitmap smallZoomBitmap = (ZoomLevel == BirdsEyeZoomLevel.Small) ? _smallZoomCheckedBitmap : _smallZoomBitmap;
            Bitmap largeZoomBitmap = (ZoomLevel == BirdsEyeZoomLevel.Large) ? _largeZoomCheckedBitmap : _largeZoomBitmap;
            e.Graphics.DrawImage(smallZoomBitmap, _smallZoomBounds.X, _smallZoomBounds.Y, _smallZoomBounds.Width, _smallZoomBounds.Height);
            e.Graphics.DrawImage(largeZoomBitmap, _largeZoomBounds.X, _largeZoomBounds.Y, _largeZoomBounds.Width, _largeZoomBounds.Height);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            Point clientMousePoint = PointToClient(MousePosition);

            if (_smallZoomBounds.Contains(clientMousePoint))
                ZoomLevel = BirdsEyeZoomLevel.Small;
            else if (_largeZoomBounds.Contains(clientMousePoint))
                ZoomLevel = BirdsEyeZoomLevel.Large;
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
        }
        #endregion

        #region High DPI Scaling
        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            SaveScaleState(factor.Width, factor.Height);
            base.ScaleControl(factor, specified);
        }

        protected override void ScaleCore(float dx, float dy)
        {
            SaveScaleState(dx, dy);
            base.ScaleCore(dx, dy);
        }

        private void SaveScaleState(float dx, float dy)
        {
            scale = new PointF(scale.X * dx, scale.Y * dy);
            _smallZoomBounds = ScaleRectangle(_smallZoomBounds, dx, dy);
            _largeZoomBounds = ScaleRectangle(_largeZoomBounds, dx, dy);
        }

        private PointF scale = new PointF(1f, 1f);

        protected int ScaleX(int x)
        {
            return (int)(x * scale.X);
        }

        protected int ScaleY(int y)
        {
            return (int)(y * scale.Y);
        }

        private int ScaleValue(int val, float scaleFactor)
        {
            return (int)(val * scaleFactor);
        }

        private Rectangle ScaleRectangle(Rectangle rect, float scaleX, float scaleY)
        {
            rect.Width = ScaleValue(rect.Width, scaleX);
            rect.Height = ScaleValue(rect.Height, scaleY);
            rect.X = ScaleValue(rect.X, scaleX);
            rect.Y = ScaleValue(rect.Y, scaleY);
            return rect;
        }
        #endregion
    }
}

