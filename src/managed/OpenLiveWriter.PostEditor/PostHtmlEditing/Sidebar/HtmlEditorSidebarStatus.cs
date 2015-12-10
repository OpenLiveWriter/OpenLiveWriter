// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.ApplicationFramework.ApplicationStyles;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Sidebar
{
    /// <summary>
    /// Summary description for ImageEditingPropertyStatusbar.
    /// </summary>
    internal class HtmlEditorSidebarStatus : Panel
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public HtmlEditorSidebarStatus()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // enable double buffered painting.
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            // set height
            Height = 20;

            // does not accept tab
            TabStop = false;

            _stringFormat = TextFormatFlags.SingleLine | TextFormatFlags.ExpandTabs |
                TextFormatFlags.PathEllipsis;
        }

        public void UpdateStatus(Image image, string statusText)
        {
            _image = image;
            Text = statusText;
            UpdateAppearance();
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

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            // calculate the rectangle we will paint within
            _controlRectangle = new Rectangle(0, 1, Width, Height - 1);

            // calculate the image rectanagle (if we have an image)
            _imageRectangle = Rectangle.Empty;
            if (_image != null)
            {
                int IMAGE_TOP_OFFSET = 3;
                int IMAGE_LEFT_OFFSET = 2;
                _imageRectangle = new Rectangle(IMAGE_LEFT_OFFSET, IMAGE_TOP_OFFSET, _image.Width, _image.Height);
            }

            //calculate the text rectangle
            int TEXT_TOP_OFFSET = 4;
            int TEXT_LEFT_MARGIN = 2;
            int TEXT_LEFT_OFFSET = _imageRectangle.Right + TEXT_LEFT_MARGIN;

            _textRectangle = new Rectangle(TEXT_LEFT_OFFSET, TEXT_TOP_OFFSET, _controlRectangle.Width - TEXT_LEFT_OFFSET, _controlRectangle.Height - TEXT_TOP_OFFSET);

            // make sure the control is repainted
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            BidiGraphics g = new BidiGraphics(e.Graphics, _controlRectangle);

            Color backgroundColor = ColorizedResources.Instance.SidebarGradientBottomColor;
            using (Brush brush = new SolidBrush(backgroundColor))
                g.FillRectangle(brush, _controlRectangle);

            if (!(_image == null && (Text == null || Text == string.Empty)))
            {
                // draw the border
                using (Pen pen = new Pen(ColorizedResources.Instance.BorderLightColor))
                {
                    g.DrawLine(pen, _controlRectangle.Left, _controlRectangle.Top, _controlRectangle.Width, _controlRectangle.Top);
                    g.DrawLine(pen, _controlRectangle.Left, _controlRectangle.Bottom, _controlRectangle.Width, _controlRectangle.Bottom);
                }

                // draw the image
                if (_image != null)
                    g.DrawImage(true, _image, new Rectangle(_imageRectangle.Left, _imageRectangle.Top, _image.Width, _image.Height));

                // draw the text
                g.DrawText(Text, ApplicationManager.ApplicationStyle.NormalApplicationFont,
                                       new Rectangle(_textRectangle.X, _textRectangle.Y, _textRectangle.Width, _textRectangle.Height),
                                       ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarTextColor,
                                       _stringFormat);
            }
        }
        private TextFormatFlags _stringFormat;
        private Rectangle _controlRectangle;
        private Rectangle _imageRectangle;
        private Rectangle _textRectangle;
        private Image _image;
        private static readonly int TOP_OFFSET = SatelliteApplicationForm.WorkspaceInset;

        private void UpdateAppearance()
        {
            PerformLayout();
            Invalidate();
        }

    }
}
