// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.InternalWriterPlugin.Controls
{
    internal class MapBirdsEyeDirectionControl : System.Windows.Forms.UserControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private Bitmap _backgroundBitmap;
        private const int BUTTON_INSET = 6;

        private class DirectionalButton : MapBitmapButton
        {
            public DirectionalButton(VEOrientation direction, string directionName)
                : base(String.Format(CultureInfo.InvariantCulture, "BEV{0}", directionName))
            {
                _direction = direction;
            }

            public VEOrientation Direction { get { return _direction; } }
            private VEOrientation _direction;
        }
        private class NorthButton : DirectionalButton { public NorthButton() : base(VEOrientation.North, "North") { } }
        private class EastButton : DirectionalButton { public EastButton() : base(VEOrientation.East, "East") { } }
        private class SouthButton : DirectionalButton { public SouthButton() : base(VEOrientation.South, "South") { } }
        private class WestButton : DirectionalButton { public WestButton() : base(VEOrientation.West, "West") { } }

        private NorthButton _northButton = new NorthButton();
        private SouthButton _southButton = new SouthButton();
        private EastButton _eastButton = new EastButton();
        private WestButton _westButton = new WestButton();

        private Bitmap _centerArrowBitmap;

        public MapBirdsEyeDirectionControl()
        {
            //	Turn on double buffered painting.
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            //set accessible names
            _northButton.AccessibleName = Res.Get(StringId.MapDirectionNorth);
            _eastButton.AccessibleName = Res.Get(StringId.MapDirectionEast);
            _southButton.AccessibleName = Res.Get(StringId.MapDirectionSouth);
            _westButton.AccessibleName = Res.Get(StringId.MapDirectionWest);

            _arrowBitmaps[0] = ResourceHelper.LoadAssemblyResourceBitmap("Images.PointNorth.png");
            _arrowBitmaps[1] = ResourceHelper.LoadAssemblyResourceBitmap("Images.PointEast.png");
            _arrowBitmaps[2] = ResourceHelper.LoadAssemblyResourceBitmap("Images.PointSouth.png");
            _arrowBitmaps[3] = ResourceHelper.LoadAssemblyResourceBitmap("Images.PointWest.png");

            // initialize the background bitmap
            _backgroundBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.BEVNSEWFrame.png");

            Width = _backgroundBitmap.Width + (BUTTON_INSET * 2);
            Height = _backgroundBitmap.Height + (BUTTON_INSET * 2);

            _buttons[0] = _northButton;
            _buttons[1] = _eastButton;
            _buttons[2] = _southButton;
            _buttons[3] = _westButton;

            RecalcPositions();

            // add the directional buttons and subscribe to their click event
            Controls.Add(_northButton);
            _northButton.Click += new EventHandler(_directionalButton_Click);
            Controls.Add(_eastButton);
            _eastButton.Click += new EventHandler(_directionalButton_Click);
            Controls.Add(_southButton);
            _southButton.Click += new EventHandler(_directionalButton_Click);
            Controls.Add(_westButton);
            _westButton.Click += new EventHandler(_directionalButton_Click);

            // handle mouse events that control 'hot-tracking'
            this.MouseLeave += new EventHandler(arrowHotTrack_MouseLeave);
            _northButton.MouseLeave += new EventHandler(arrowHotTrack_MouseLeave);
            _northButton.MouseEnter += new EventHandler(arrowHotTrack_MouseEnter);
            _eastButton.MouseLeave += new EventHandler(arrowHotTrack_MouseLeave);
            _eastButton.MouseEnter += new EventHandler(arrowHotTrack_MouseEnter);
            _southButton.MouseLeave += new EventHandler(arrowHotTrack_MouseLeave);
            _southButton.MouseEnter += new EventHandler(arrowHotTrack_MouseEnter);
            _westButton.MouseLeave += new EventHandler(arrowHotTrack_MouseLeave);
            _westButton.MouseEnter += new EventHandler(arrowHotTrack_MouseEnter);

            // default to North
            Direction = VEOrientation.North;
        }

        private void RecalcPositions()
        {
            int centerX = (Width / 2) - (_northButton.Width / 2);
            int centerY = (Height / 2) - (_eastButton.Height / 2);

            _buttonLocations[0] = new Point(centerX, 0);
            _buttonLocations[1] = new Point(Width - _eastButton.Width, centerY);
            _buttonLocations[2] = new Point(centerX, Height - _southButton.Height);
            _buttonLocations[3] = new Point(0, centerY);
        }

        private Point[] _buttonLocations = new Point[4];
        private DirectionalButton[] _buttons = new DirectionalButton[4];
        private Bitmap[] _arrowBitmaps = new Bitmap[4];

        private void PositionButtons(int iTopButton)
        {
            int iLocation = 0;
            for (int i = iTopButton; i < _buttons.Length; i++)
                _buttons[i].Location = _buttonLocations[iLocation++];

            for (int i = 0; i < iTopButton; i++)
                _buttons[i].Location = _buttonLocations[iLocation++];
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public VEOrientation Direction
        {
            get
            {
                return _mapDirection;
            }
            set
            {
                _mapDirection = value;

                RepositionButtons();

                if (DirectionChanged != null)
                    DirectionChanged(this, EventArgs.Empty);
            }
        }

        private void RepositionButtons()
        {
            // flip the buttons around
            switch (_mapDirection)
            {
                case VEOrientation.North:
                    PositionButtons(0);
                    break;
                case VEOrientation.East:
                    PositionButtons(1);
                    break;
                case VEOrientation.South:
                    PositionButtons(2);
                    break;
                case VEOrientation.West:
                    PositionButtons(3);
                    break;
            }

            // always go back to north facing button
            CenterArrowBitmap = _arrowBitmaps[0];

            Invalidate();
        }

        private VEOrientation _mapDirection;

        public event EventHandler DirectionChanged;

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, ScaleX(_backgroundBitmap.Width + (BUTTON_INSET * 2)), ScaleY(_backgroundBitmap.Height + (BUTTON_INSET * 2)), specified);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.DrawImage(_backgroundBitmap, ScaleX(BUTTON_INSET), ScaleY(BUTTON_INSET), ScaleX(_backgroundBitmap.Width), ScaleY(_backgroundBitmap.Height));

            if (CenterArrowBitmap != null)
            {
                e.Graphics.DrawImage(
                    CenterArrowBitmap,
                    (Width / 2) - (ScaleX(CenterArrowBitmap.Width) / 2),
                    (Height / 2) - (ScaleY(CenterArrowBitmap.Height) / 2),
                    ScaleX(CenterArrowBitmap.Width),
                    ScaleY(CenterArrowBitmap.Height));
            }
        }

        private Bitmap CenterArrowBitmap
        {
            get
            {
                return _centerArrowBitmap;
            }
            set
            {
                _centerArrowBitmap = value;
                Invalidate();
            }
        }

        private void _directionalButton_Click(object sender, EventArgs e)
        {
            _supressNextEnter = true;
            Direction = (sender as DirectionalButton).Direction;
        }

        private void arrowHotTrack_MouseEnter(object sender, EventArgs e)
        {
            if (!_supressNextEnter)
            {
                DirectionalButton button = sender as DirectionalButton;
                for (int i = 0; i < _buttonLocations.Length; i++)
                {
                    if (button.Location == _buttonLocations[i])
                    {
                        CenterArrowBitmap = _arrowBitmaps[i];
                        break;
                    }
                }
            }
            else
            {
                _supressNextEnter = false;
            }
        }

        private void arrowHotTrack_MouseLeave(object sender, EventArgs e)
        {
            CenterArrowBitmap = _arrowBitmaps[0];
        }

        private bool _supressNextEnter = false;

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

        #region High DPI Scaling
        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            SaveScaleState(factor.Width, factor.Height);
            base.ScaleControl(factor, specified);
            RecalcPositions();
            RepositionButtons();
        }

        protected override void ScaleCore(float dx, float dy)
        {
            SaveScaleState(dx, dy);
            base.ScaleCore(dx, dy);
            RecalcPositions();
            RepositionButtons();
        }

        protected override bool ScaleChildren
        {
            get
            {
                return false;
            }
        }

        private void SaveScaleState(float dx, float dy)
        {
            scale = new PointF(scale.X * dx, scale.Y * dy);
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

        protected Point ScaleLocation(Point p, float dx, float dy)
        {
            p.X = (int)(p.X * dx);
            p.Y = (int)(p.Y * dy);
            return p;
        }
        #endregion
    }
}
