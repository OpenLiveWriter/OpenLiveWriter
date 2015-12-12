// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.Controls
{
    /// <summary>
    /// Subclass of the System.Windows.Forms.Label control that paints itself in the disabled state
    /// using opacity as opposed to simply calling ControlPaint.DrawStringDisabled using the system
    /// control color.
    /// </summary>
    public class LabelControl : Label
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        /// <summary>
        /// String format we use to format text.
        /// </summary>
        private TextFormatFlags stringFormat;

        /// <summary>
        /// A value which indicates whether the label control is multi-line.
        /// </summary>
        private bool multiLine;

        /// <summary>
        /// The actual measured size of the text after drawing.
        /// </summary>
        private SizeF measuredTextSize = SizeF.Empty;

        /// <summary>
        /// Initializes a new instance of the LabelControl class.
        /// </summary>
        public LabelControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            //	Set the initial string format.
            SetStringFormat();
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

        /// <summary>
        /// Disable the FlatStyle property and force it to be standard.
        /// </summary>
        [
            Category("Appearence")
        ]
        public bool MultiLine
        {
            get
            {
                return multiLine;
            }
            set
            {
                multiLine = value;
                SetStringFormat();
                Invalidate();
            }
        }

        /// <summary>
        /// Disable the FlatStyle property and force it to be standard.
        /// </summary>
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public new FlatStyle FlatStyle
        {
            get
            {
                return FlatStyle.Standard;
            }
        }

        /// <summary>
        /// Gets or sets the alignment of text in the label.
        /// </summary>
        public override ContentAlignment TextAlign
        {
            get
            {
                return base.TextAlign;
            }
            set
            {
                if (base.TextAlign != value)
                {
                    base.TextAlign = value;
                    SetStringFormat();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            BidiGraphics g = new BidiGraphics(e.Graphics, ClientRectangle);
            //	Paint.
            g.DrawText(Text, Font, ClientRectangle, Color.FromArgb(GraphicsHelper.Opacity(Enabled ? 100.0 : 50.0), ForeColor), stringFormat);

            Size size = new Size(ClientSize.Width, ClientSize.Height);
            measuredTextSize = g.MeasureText(Text, Font, size, stringFormat);
        }

        /// <summary>
        ///	Set the string format.
        /// </summary>
        private void SetStringFormat()
        {
            //	Instantiate the string format.
            stringFormat = new TextFormatFlags();

            //	No wrapping and ellipsis on truncation.
            if (!multiLine)
                stringFormat |= TextFormatFlags.SingleLine;
            stringFormat |= TextFormatFlags.ExpandTabs;
            stringFormat |= TextFormatFlags.EndEllipsis;

            if (!this.UseMnemonic)
                stringFormat |= TextFormatFlags.NoPrefix;
            else if (!ShowKeyboardCues)
                stringFormat |= TextFormatFlags.HidePrefix;

            //	Map ContentAlignment to the StringFormat we use to draw the string.
            switch (TextAlign)
            {
                case ContentAlignment.BottomCenter:
                    stringFormat |= TextFormatFlags.Right;		            //	Vertical
                    stringFormat |= TextFormatFlags.HorizontalCenter;       //	Horizontal
                    break;

                case ContentAlignment.BottomLeft:
                    stringFormat |= TextFormatFlags.Right;		            //	Vertical
                    break;

                case ContentAlignment.BottomRight:
                    stringFormat |= TextFormatFlags.Right;                  //	Vertical
                    stringFormat |= TextFormatFlags.Bottom;                 //	Horizontal
                    break;

                case ContentAlignment.MiddleCenter:
                    stringFormat |= TextFormatFlags.VerticalCenter;	        //	Vertical
                    stringFormat |= TextFormatFlags.HorizontalCenter;		//	Horizontal
                    break;

                case ContentAlignment.MiddleLeft:
                    stringFormat |= TextFormatFlags.VerticalCenter;	        //	Vertical
                    break;

                case ContentAlignment.MiddleRight:
                    stringFormat |= TextFormatFlags.VerticalCenter;	        //	Vertical
                    stringFormat |= TextFormatFlags.Bottom;			        //	Horizontal
                    break;

                case ContentAlignment.TopCenter:
                    stringFormat |= TextFormatFlags.HorizontalCenter;		//	Horizontal
                    break;

                case ContentAlignment.TopLeft:
                    break;

                case ContentAlignment.TopRight:
                    stringFormat |= TextFormatFlags.Bottom;			        //	Horizontal
                    break;
            }
        }

        protected bool IsInText(Point point)
        {
            return IsWithin(point.X, Width, measuredTextSize.Width, InvertIfRightToLeft(stringFormat))
                && IsWithin(point.Y, Height, measuredTextSize.Height, stringFormat);
        }

        /// <summary>
        /// Returns the rectangle region occupied by the painted label text.
        /// </summary>
        /// <returns></returns>
        protected Rectangle GetMeasuredTextRectangle()
        {
            Rectangle textRect = new Rectangle(Point.Empty, measuredTextSize.ToSize());
            //shift the X position based on the alignment
            if (TextFormatFlags.HorizontalCenter == (stringFormat & TextFormatFlags.HorizontalCenter))
            {
                textRect.X = Utility.CenterInRectangle(textRect.Size, ClientRectangle).X;
            }
            else if (TextFormatFlags.Right == (stringFormat & TextFormatFlags.Right))
            {
                textRect.Offset(ClientRectangle.Width - textRect.Width, 0);
            }

            //shift the Y position based on the lineAlignment
            if (TextFormatFlags.VerticalCenter == (stringFormat & TextFormatFlags.VerticalCenter))
            {
                textRect.Y = Utility.CenterInRectangle(textRect.Size, ClientRectangle).Y;
            }
            else if (TextFormatFlags.Bottom == (stringFormat & TextFormatFlags.Bottom))
            {
                textRect.Offset(0, ClientRectangle.Height - textRect.Height);
            }

            return textRect;
        }

        /// <summary>
        /// Given a point, a region, a text size, and alignment, determines if the point
        /// is located within the text.
        /// </summary>
        private bool IsWithin(double testVal, double regionSize, float textVal, TextFormatFlags alignment)
        {
            double min = 0;
            double max = regionSize;

            if ((TextFormatFlags.VerticalCenter == (stringFormat & TextFormatFlags.VerticalCenter)) ||
                    (TextFormatFlags.HorizontalCenter == (stringFormat & TextFormatFlags.HorizontalCenter)))
            {
                min = (max - textVal) / 2.0;
                max -= min;
            }
            else if ((TextFormatFlags.Bottom == (stringFormat & TextFormatFlags.Bottom)) ||
                (TextFormatFlags.Right == (stringFormat & TextFormatFlags.Right)))
            {
                min = max - textVal;
            }
            else
            {
                max = textVal;
            }

            return !(testVal < min || testVal > max);
        }

        /// <summary>
        /// Used for calculations in IsInText.
        ///
        /// </summary>
        private TextFormatFlags InvertIfRightToLeft(TextFormatFlags sa)
        {
            RightToLeft rtl = RightToLeft;
            Control c = this;
            while (rtl == RightToLeft.Inherit)
            {
                c = c.Parent;
                if (c == null)
                    rtl = RightToLeft.No;
                else
                    rtl = c.RightToLeft;
            }

            if (rtl != RightToLeft.Yes)
                return sa;

            if ((TextFormatFlags.VerticalCenter == (sa & TextFormatFlags.VerticalCenter)) ||
                (TextFormatFlags.HorizontalCenter == (sa & TextFormatFlags.HorizontalCenter)))
            {
                return sa;
            }
            else if ((TextFormatFlags.Bottom == (sa & TextFormatFlags.Bottom)) ||
                (TextFormatFlags.Right == (sa & TextFormatFlags.Right)))
            {
                return new TextFormatFlags();
            }
            else
            {
                return (TextFormatFlags.Right | TextFormatFlags.Bottom);
            }
        }
    }
}
