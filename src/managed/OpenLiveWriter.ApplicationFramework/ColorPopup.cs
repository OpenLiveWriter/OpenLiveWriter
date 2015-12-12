// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Summary description for ColorPopup.
    /// </summary>
    public class ColorPopup : System.Windows.Forms.UserControl
    {
        private Color m_color = Color.Empty;
        private Bitmap m_dropDownArrow = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.BlackDropArrow.png");
        private Bitmap m_buttonOutlineHover = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.ToolbarButtonHover.png");
        private Bitmap m_buttonOutlinePressed = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.ToolbarButtonPressed.png");
        private bool m_hover = false;
        private bool m_pressed = false;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public ColorPopup()
        {
            // enable double buffered painting.
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
        }

        public Color Color
        {
            get { return m_color; }
            set
            {
                m_color = value;
                if (ColorSelected != null)
                    ColorSelected(this, new ColorSelectedEventArgs(value));
                Invalidate();
            }
        }

        public Color EffectiveColor
        {
            get
            {
                if (m_color == Color.Empty)
                    return Color.FromArgb(86, 150, 172);
                else
                    return m_color;
            }
        }

        public event ColorSelectedEventHandler ColorSelected;

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
            //
            // ColorPopup
            //
            this.Name = "ColorPopup";
            this.Size = new System.Drawing.Size(150, 40);
            this.Text = "&Color Scheme";

        }
        #endregion

        public void AutoSizeForm()
        {
            using (Graphics g = Graphics.FromHwnd(User32.GetDesktopWindow()))
            {
                StringFormat sf = new StringFormat(StringFormat.GenericDefault);
                sf.HotkeyPrefix = ShowKeyboardCues ? HotkeyPrefix.Show : HotkeyPrefix.Hide;
                SizeF size = g.MeasureString(Text, Font, new PointF(0, 0), sf);
                Width = PADDING * 2 + GUTTER_SIZE * 2 + COLOR_SIZE + (int)Math.Ceiling(size.Width) + m_dropDownArrow.Width;
                Height = PADDING * 2 + COLOR_SIZE;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            m_hover = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            m_hover = false;
            Invalidate();
        }

        const int PADDING = 6;
        const int GUTTER_SIZE = 3;
        const int COLOR_SIZE = 14;

        protected override void OnPaint(PaintEventArgs e)
        {
            BidiGraphics g = new BidiGraphics(e.Graphics, ClientRectangle);
            if (m_pressed)
            {
                SystemButtonHelper.DrawSystemButtonFacePushed(g, false, ClientRectangle, false);
            }
            else if (m_hover)
            {
                SystemButtonHelper.DrawSystemButtonFace(g, false, false, ClientRectangle, false);
            }

            Rectangle colorRect = new Rectangle(PADDING, PADDING, COLOR_SIZE, COLOR_SIZE);
            Rectangle dropDownArrowRect = new Rectangle(Width - PADDING - m_dropDownArrow.Width,
                                                        PADDING,
                                                        m_dropDownArrow.Width,
                                                        colorRect.Height);
            Rectangle textRect = new Rectangle(PADDING + GUTTER_SIZE + colorRect.Width,
                PADDING,
                Width - (PADDING + GUTTER_SIZE + colorRect.Width) - (PADDING + GUTTER_SIZE + dropDownArrowRect.Width),
                colorRect.Height);

            using (Brush b = new SolidBrush(EffectiveColor))
                g.FillRectangle(b, colorRect);
            using (Pen p = new Pen(SystemColors.Highlight, 1))
                g.DrawRectangle(p, colorRect);

            g.DrawText(Text, Font, textRect, SystemColors.ControlText, ShowKeyboardCues ? TextFormatFlags.Default : TextFormatFlags.NoPrefix);

            g.DrawImage(false,
                        m_dropDownArrow,
                        RectangleHelper.Center(m_dropDownArrow.Size, dropDownArrowRect, false),
                        0,
                        0,
                        m_dropDownArrow.Width,
                        m_dropDownArrow.Height,
                        GraphicsUnit.Pixel);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            ColorPickerForm form = new ColorPickerForm();
            form.Color = Color;
            form.ColorSelected += new ColorSelectedEventHandler(form_ColorSelected);
            form.Closed += new EventHandler(form_Closed);
            form.TopMost = true;

            form.StartPosition = FormStartPosition.Manual;
            Point p = PointToScreen(new Point(0, Height));
            form.Location = p;
            form.Show();
            m_pressed = true;
            Invalidate();
        }

        private void form_ColorSelected(object sender, ColorSelectedEventArgs args)
        {
            Color = args.SelectedColor;
        }

        private void form_Closed(object sender, EventArgs e)
        {
            m_pressed = false;
            Invalidate();
        }
    }
}
