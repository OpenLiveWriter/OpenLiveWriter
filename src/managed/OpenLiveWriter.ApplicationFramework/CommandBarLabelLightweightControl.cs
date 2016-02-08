// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// CommandBar label lightweight control.
    /// </summary>
    public class CommandBarLabelLightweightControl : LightweightControl
    {
        /// <summary>
        ///	The maximum text with of text on a label.
        /// </summary>
        private const int MAX_TEXT_WIDTH = 300;

        /// <summary>
        /// The top margin to leave around the command bar label bitmap.
        /// </summary>
        private const int TOP_MARGIN = 4;

        /// <summary>
        /// The left margin to leave around the command bar label image and text.
        /// </summary>
        private const int LEFT_MARGIN = 2;

        /// <summary>
        /// The bottom margin to leave around the command bar label image and text.
        /// </summary>
        private const int BOTTOM_MARGIN = 4;

        /// <summary>
        /// The right margin to leave around the command bar label image and text.
        /// </summary>
        private const int RIGHT_MARGIN = 2;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        /// <summary>
        /// The label text.
        /// </summary>
        private readonly string text;

        /// <summary>
        /// The string format used to format text.
        /// </summary>
        private readonly TextFormatFlags textFormatFlags = TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.PreserveGraphicsTranslateTransform;

        /// <summary>
        /// The text layout rectangle.  This is the rectangle into which the text is measured and
        /// drawn.  It is not the actual text rectangle.
        /// </summary>
        private Rectangle textLayoutRectangle;

        /// <summary>
        /// Initializes a new instance of the CommandBarLabelLightweightControl class.
        /// </summary>
        /// <param name="container"></param>
        public CommandBarLabelLightweightControl(IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the CommandBarLabelLightweightControl class.
        /// </summary>
        public CommandBarLabelLightweightControl()
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            InitializeComponent();
            SetAccesibleInfo();
        }

        /// <summary>
        /// Initializes a new instance of the CommandBarLabelLightweightControl class.
        /// </summary>
        /// <param name="text">The text for the label.</param>
        public CommandBarLabelLightweightControl(string text)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            InitializeComponent();

            //	Set the text.
            this.text = text;

            SetAccesibleInfo();
        }

        private void SetAccesibleInfo()
        {
            AccessibleName = ControlHelper.ToAccessibleName(text);
            AccessibleRole = AccessibleRole.StaticText;
        }

        /// <summary>
        /// Required designer variable.
        /// </summary>
        public Container Components
        {
            get { return components; }
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
        /// Raises the Layout event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLayout(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLayout(e);

            if (Parent == null)
                return;

            //	If there is no text, we're done.
            if (text == null || text.Length == 0)
            {
                VirtualSize = Size.Empty;
                return;
            }

            //	Obtain the font.
            Font font = ApplicationManager.ApplicationStyle.NormalApplicationFont;

            //	The label width and height.
            int labelWidth = LEFT_MARGIN, labelHeight = TOP_MARGIN + font.Height + BOTTOM_MARGIN;

            using (Graphics graphics = Parent.CreateGraphics())
            {
                //	Initialize the text layout rectangle.
                textLayoutRectangle = new Rectangle(labelWidth,
                                                    TOP_MARGIN,
                                                    MAX_TEXT_WIDTH,
                                                    font.Height);

                Size textSize = TextRenderer.MeasureText(graphics, text, font, textLayoutRectangle.Size, textFormatFlags);
                textLayoutRectangle.Size = textSize;

                //	Increase the label width to account for the text, plus a bit of extra space.
                labelWidth += textSize.Width + FontHelper.WidthOfSpace(graphics, font);
            }

            //	Increase the label width for the right margin.
            labelWidth += RIGHT_MARGIN;

            //	Set the new virtual size.
            VirtualSize = new Size(labelWidth, labelHeight);
        }

        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnPaint(e);

            if (!string.IsNullOrEmpty(text))
            {
                BidiGraphics bidiGraphics = new BidiGraphics(e.Graphics, VirtualSize);
                bidiGraphics.DrawText(text,
                                              ApplicationManager.ApplicationStyle.NormalApplicationFont,
                                            textLayoutRectangle,
                                            Color.Black,
                                            textFormatFlags | TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.NoPadding);
            }
        }
    }
}
