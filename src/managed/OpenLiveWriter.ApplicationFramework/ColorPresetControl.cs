// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.Localization;
using System.Security.Permissions;

namespace OpenLiveWriter.ApplicationFramework
{
    public class GridNavigateEventArgs : EventArgs
    {
        public enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }

        private readonly Direction direction;

        public GridNavigateEventArgs(Direction dir)
        {
            this.direction = dir;
        }

        public Direction Navigate
        {
            get { return direction; }
        }
    }

    public delegate void GridNavigateEventHandler(object sender, GridNavigateEventArgs args);

    public class ColorButton : System.Windows.Forms.Control
    {
        public event ColorSelectedEventHandler ColorSelected;
        public event GridNavigateEventHandler Navigate;

        private Color _color;
        public Color Color
        {
            get { return _color; }
        }

        private bool m_selected;
        public bool Selected
        {
            get { return m_selected; }
            set { m_selected = value; }
        }

        private const int SQUARE_SIZE = 24;
        private const int BORDER_SIZE = 20;
        private const int COLOR_WELL_SIZE = 18;

        private bool _highlighted;
        private bool highlighted
        {
            get { return _highlighted; }
            set
            {
                if (_highlighted != value)
                {
                    _highlighted = value;
                    Invalidate();
                }
            }

        }

        public ColorButton(Color color) : base()
        {
            this.Width = SQUARE_SIZE;
            this.Height = SQUARE_SIZE;
            this.SetStyle(ControlStyles.Selectable | ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true);
            this._highlighted = false;
            this._color = color;
            this.KeyPress += new KeyPressEventHandler(ColorButton_KeyPress);
            this.Visible = true;
            this.TabStop = true;
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            highlighted = true;
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            highlighted = false;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Up:
                    highlighted = false;
                    this.Navigate(this, new GridNavigateEventArgs(GridNavigateEventArgs.Direction.Up));
                    break;
                case Keys.Down:
                    highlighted = false;
                    this.Navigate(this, new GridNavigateEventArgs(GridNavigateEventArgs.Direction.Down));
                    break;
                case Keys.Left:
                    highlighted = false;
                    this.Navigate(this, new GridNavigateEventArgs(GridNavigateEventArgs.Direction.Left));
                    break;
                case Keys.Right:
                    highlighted = false;
                    this.Navigate(this, new GridNavigateEventArgs(GridNavigateEventArgs.Direction.Right));
                    break;
                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }

            return true;
        }

        private void ColorButton_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r' || e.KeyChar == ' ')
            {
                ColorSelected(this, new ColorSelectedEventArgs(this._color));
                return;
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            highlighted = true;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            highlighted = false;
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            this.ColorSelected(this, new ColorSelectedEventArgs(this._color));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Rectangle outerRect = new Rectangle(0, 0, this.Width, this.Height);
            Rectangle innerRect = RectangleHelper.Center(new Size(COLOR_WELL_SIZE, COLOR_WELL_SIZE), outerRect, false);

            if (Selected)
                outerRect.Inflate(1, 1);

            if (!highlighted)
                outerRect.Inflate(COLOR_WELL_SIZE - BORDER_SIZE, COLOR_WELL_SIZE - BORDER_SIZE);

            using (Brush b = new SolidBrush(SystemColors.Highlight))
                e.Graphics.FillRectangle(b, outerRect);

            using (Brush b = new SolidBrush(this._color))
                e.Graphics.FillRectangle(b, innerRect);
        }
    }

    /// <summary>
    /// Summary description for ColorPresetControl.
    /// </summary>
    public class ColorPresetControl : IColorPickerSubControl
    {
        private TableLayoutPanel tableLayout;

        private int NUM_ROWS = 3;
        private int NUM_COLUMNS = 4;

        private ColorButton[] colors = {
                                     new ColorButton(Color.FromArgb(58, 177, 222)),
                                     new ColorButton(Color.FromArgb(166, 166, 166)),
                                     new ColorButton(Color.FromArgb(110, 158, 255)),
                                     new ColorButton(Color.FromArgb(112, 217, 235)),

                                     new ColorButton(Color.FromArgb(242, 57, 57)),
                                     new ColorButton(Color.FromArgb(255, 123, 4)),
                                     new ColorButton(Color.FromArgb(255, 172, 247)),
                                     new ColorButton(Color.FromArgb(145, 226, 71)),

                                     new ColorButton(Color.FromArgb(102, 117, 140)),
                                     new ColorButton(Color.FromArgb(132, 117, 217)),
                                     new ColorButton(Color.FromArgb(201, 198, 184)),
                                     new ColorButton(Color.FromArgb(244, 62, 131))
                                 };

        public ColorPresetControl(ColorSelectedEventHandler colorSelected, NavigateEventHandler navigate) : base(colorSelected, navigate)
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            AccessibleName = Res.Get(StringId.ColorPickerColor);

            System.Diagnostics.Debug.Assert(colors.Length == 12, "Unexpected number of buttons.  If you change the number of buttons, then you'll need to ensure that the navigation code is updated as well.");

            int i = 0;
            foreach (ColorButton button in colors)
            {
                button.ColorSelected += colorSelected;
                button.Navigate += new GridNavigateEventHandler(ColorPresetControl_Navigate);
                button.TabIndex = i;
                button.Margin = new System.Windows.Forms.Padding(0);
                tableLayout.Controls.Add(button);
                tableLayout.SetCellPosition(button, new TableLayoutPanelCellPosition(i % (NUM_ROWS + 1), i / (NUM_COLUMNS)));
                i++;
            }

            foreach (RowStyle style in tableLayout.RowStyles)
            {
                style.SizeType = SizeType.Percent;
                style.Height = 1 / (float)tableLayout.RowCount;
            }
            foreach (ColumnStyle style in tableLayout.ColumnStyles)
            {
                style.SizeType = SizeType.Percent;
                style.Width = 1 / (float)tableLayout.ColumnCount;
            }

            this.Controls.Add(tableLayout);
        }

        public override Color Color
        {
            get
            {
                return _color;
            }

            set
            {
                foreach (ColorButton button in colors)
                {
                    _color = value;
                    button.Selected = (button.Color == value);
                }
            }
        }

        void ColorPresetControl_Navigate(object sender, GridNavigateEventArgs args)
        {
            Control c = (Control)sender;

            TableLayoutPanelCellPosition navFrom = tableLayout.GetCellPosition(c);
            TableLayoutPanelCellPosition navTo = navFrom;

            bool navigateOffPresets = false;
            bool forward = false;

            switch (args.Navigate)
            {
                case GridNavigateEventArgs.Direction.Up:
                    if (navFrom.Row <= 0)
                    {
                        navigateOffPresets = true;
                    }
                    else
                    {
                        navTo.Row--;
                    }

                    break;
                case GridNavigateEventArgs.Direction.Down:
                    if (navFrom.Row >= tableLayout.RowCount - 1)
                    {
                        navigateOffPresets = true;
                        forward = true;
                    }
                    else
                    {
                        navTo.Row++;
                    }

                    forward = true;
                    break;
                case GridNavigateEventArgs.Direction.Left:
                    if (navFrom.Column <= 0)
                    {
                        if (navFrom.Row <= 0)
                        {
                            navigateOffPresets = true;
                        }
                        else
                        {
                            navTo.Row--;
                            navTo.Column = tableLayout.ColumnCount - 1;
                        }
                    }
                    else
                    {
                        navTo.Column--;
                    }

                    break;
                case GridNavigateEventArgs.Direction.Right:
                    if (navFrom.Column >= tableLayout.ColumnCount - 1)
                    {
                        if (navFrom.Row >= tableLayout.RowCount - 1)
                        {
                            forward = true;
                            navigateOffPresets = true;
                        }
                        else
                        {
                            navTo.Column = 0;
                            navTo.Row++;
                        }
                    }
                    else
                    {
                        navTo.Column++;
                    }
                    break;
                default:
                    break;
            }

            if (navigateOffPresets)
            {
                Navigate(new NavigateEventArgs(forward));
            }
            else
            {
                tableLayout.GetControlFromPosition(navTo.Column, navTo.Row).Select();
            }
        }

        public override void SelectControl(bool forward)
        {
            if (forward)
            {
                tableLayout.GetControlFromPosition(0, 0).Select();
            }
            else
            {
                tableLayout.GetControlFromPosition(tableLayout.ColumnCount - 1, tableLayout.RowCount - 1).Select();
            }
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            //
            // ColorPresetControl
            //
            this.Name = "ColorPresetControl";
            this.Size = new System.Drawing.Size(96, 72);
            this.tableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.SuspendLayout();

            //
            // tableLayout
            //
            this.tableLayout.RowCount = 3;
            this.tableLayout.ColumnCount = 4;
            this.tableLayout.Location = new System.Drawing.Point(0, 0);
            this.tableLayout.Name = "tableLayout";
            this.tableLayout.TabIndex = 0;
            this.tableLayout.TabStop = true;
            this.tableLayout.Size = new System.Drawing.Size(96, 72);

            this.ResumeLayout(false);

        }
        #endregion
    }
}
