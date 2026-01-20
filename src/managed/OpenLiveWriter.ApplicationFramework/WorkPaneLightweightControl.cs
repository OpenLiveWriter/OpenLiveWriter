// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using System.Diagnostics;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Lightweight work pane control.
    /// </summary>
    internal class WorkPaneLightweightControl : LightweightControl
    {
        /// <summary>
        /// Top left bitmap.
        /// </summary>
        private static Bitmap topLeftBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.Application.WorkPaneTopLeft.png");

        /// <summary>
        /// Top right bitmap.
        /// </summary>
        private static Bitmap topRightBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.Application.WorkPaneTopRight.png");

        /// <summary>
        /// Bottom left bitmap.
        /// </summary>
        private static Bitmap bottomLeftBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.Application.WorkPaneBottomLeft.png");

        /// <summary>
        /// Bottom right bitmap.
        /// </summary>
        private static Bitmap bottomRightBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.Application.WorkPaneBottomRight.png");

        /// <summary>
        /// Top bitmap.
        /// </summary>
        private static Bitmap topBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.Application.WorkPaneTop.png");

        /// <summary>
        /// Bottom bitmap.
        /// </summary>
        private static Bitmap bottomBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.Application.WorkPaneBottom.png");

        /// <summary>
        /// Left bitmap.
        /// </summary>
        private static Bitmap leftBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.Application.WorkPaneLeft.png");

        /// <summary>
        /// Right bitmap.
        /// </summary>
        private static Bitmap rightBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.Application.WorkPaneRight.png");

        /// <summary>
        /// The command bar lightweight control.
        /// </summary>
        private OpenLiveWriter.ApplicationFramework.CommandBarLightweightControl commandBarLightweightControl;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components;

        /// <summary>
        /// The control.
        /// </summary>
        private Control control;

        /// <summary>
        /// Gets or sets the control.
        /// </summary>
        public Control Control
        {
            get
            {
                return control;
            }
            set
            {
                if (control != null && Parent != null && Parent.Controls.Contains(control))
                    Parent.Controls.Remove(control);
                control = value;
                PerformLayout();
                Invalidate();
            }
        }

        /// <summary>
        /// Initializes a new instance of the WorkPaneLightweightControl class.
        /// </summary>
        public WorkPaneLightweightControl(System.ComponentModel.IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the WorkPaneLightweightControl class.
        /// </summary>
        public WorkPaneLightweightControl()
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            InitializeComponent();
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.commandBarLightweightControl = new OpenLiveWriter.ApplicationFramework.CommandBarLightweightControl(this.components);
            //
            // commandBarLightweightControl
            //
            this.commandBarLightweightControl.LightweightControlContainerControl = this;

        }

        /// <summary>
        /// Gets the command bar rectangle.
        /// </summary>
        private Rectangle CommandBarRectangle
        {
            get
            {
                return new Rectangle(	topLeftBitmap.Width,
                    topBitmap.Height,
                    VirtualWidth-(topLeftBitmap.Width+topRightBitmap.Width),
                    topLeftBitmap.Height-topBitmap.Height);
            }
        }

        /// <summary>
        /// Gets the control rectangle.
        /// </summary>
        private Rectangle ControlRectangle
        {
            get
            {
                return new Rectangle(	leftBitmap.Width,
                    topLeftBitmap.Height,
                    VirtualWidth-(leftBitmap.Width+rightBitmap.Width+1),
                    VirtualHeight-(topLeftBitmap.Height+bottomBitmap.Height+1));
            }
        }

        /// <summary>
        /// Raises the Layout event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLayout(System.EventArgs e)
        {
            //	Call the base class's method so registered delegates receive the event.
            base.OnLayout(e);

            //	Set the bounds of the command bar lightweight control.
            commandBarLightweightControl.VirtualBounds = CommandBarRectangle;

            if (control != null)
            {
                if (Parent != null && !Parent.Controls.Contains(control))
                {
                    Parent.Controls.Add(control);
                    if (control is ICommandBarProvider)
                        commandBarLightweightControl.CommandBarDefinition = ((ICommandBarProvider)control).CommandBarDefinition;
                }
                control.Bounds = VirtualClientRectangleToParent(ControlRectangle);
            }
        }

        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            //	Create a rectangle representing the header area to be filled.
            Rectangle headerRectangle = new Rectangle(	0,
                0,
                VirtualWidth-(topLeftBitmap.Width+topRightBitmap.Width),
                topLeftBitmap.Height-1);

            if (!headerRectangle.IsEmpty)
            {
                //	Construct an offscreen bitmap.
                Bitmap bitmap = new Bitmap(headerRectangle.Width, headerRectangle.Height);
                Graphics bitmapGraphics = Graphics.FromImage(bitmap);
                LinearGradientBrush linearGradientBrush = new LinearGradientBrush(	headerRectangle,
                    Color.FromArgb(231, 231, 232),
                    Color.FromArgb(185, 218, 233),
                    LinearGradientMode.Horizontal);
                bitmapGraphics.FillRectangle(linearGradientBrush, headerRectangle);
                linearGradientBrush.Dispose();
                bitmapGraphics.Dispose();
                e.Graphics.DrawImageUnscaled(bitmap, topLeftBitmap.Width, 1);
            }

            //	Fill in the client rectangle.
            Rectangle clientArea = new Rectangle(	leftBitmap.Width-1,
                topLeftBitmap.Height,
                VirtualWidth-((leftBitmap.Width-1)+rightBitmap.Width),
                VirtualHeight-(topLeftBitmap.Height+bottomBitmap.Height));
            e.Graphics.FillRectangle(System.Drawing.Brushes.White, clientArea);

            //	Draw the border.
            DrawBorder(e.Graphics);

            //	Call the base class's method so that registered delegates receive the event.
            base.OnPaint(e);
        }

        /// <summary>
        /// Helper to draw the work pane border.
        /// </summary>
        /// <param name="graphics">Graphics context into which the border is drawn.</param>
        private void DrawBorder(Graphics graphics)
        {
            //	Draw the top left corner of the border.
            if (topLeftBitmap != null)
                graphics.DrawImageUnscaled(topLeftBitmap, 0, 0);

            //	Draw the top right corner of the border.
            if (topRightBitmap != null)
                graphics.DrawImageUnscaled(topRightBitmap, VirtualWidth-topRightBitmap.Width, 0);

            //	Draw the bottom left corner of the border.
            if (bottomLeftBitmap != null)
                graphics.DrawImageUnscaled(bottomLeftBitmap, 0, VirtualHeight-bottomLeftBitmap.Height);

            //	Draw the bottom right corner of the border.
            if (bottomRightBitmap != null)
                graphics.DrawImageUnscaled(bottomRightBitmap, VirtualWidth-bottomRightBitmap.Width, VirtualHeight-bottomRightBitmap.Height);

            //	Fill the top.
            if (topBitmap != null)
            {
                Rectangle fillRectangle = new Rectangle(topLeftBitmap.Width,
                    0,
                    VirtualWidth-(topLeftBitmap.Width+topRightBitmap.Width),
                    topBitmap.Height);
                GraphicsHelper.TileFillUnscaledImageHorizontally(graphics, topBitmap, fillRectangle);
            }

            //	Fill the left.
            if (leftBitmap != null)
            {
                Rectangle fillRectangle = new Rectangle(0,
                    topLeftBitmap.Height,
                    leftBitmap.Width,
                    VirtualHeight-(topLeftBitmap.Height+bottomLeftBitmap.Height));
                GraphicsHelper.TileFillUnscaledImageVertically(graphics, leftBitmap, fillRectangle);
            }

            //	Fill the right.
            if (rightBitmap != null)
            {
                Rectangle fillRectangle = new Rectangle(VirtualWidth-rightBitmap.Width,
                    topRightBitmap.Height,
                    rightBitmap.Width,
                    VirtualHeight-(topRightBitmap.Height+bottomRightBitmap.Height));
                GraphicsHelper.TileFillUnscaledImageVertically(graphics, rightBitmap, fillRectangle);
            }

            //	Fill the bottom.
            if (bottomBitmap != null)
            {
                Rectangle fillRectangle = new Rectangle(bottomLeftBitmap.Width,
                    VirtualHeight-bottomBitmap.Height,
                    VirtualWidth-(bottomLeftBitmap.Width+bottomRightBitmap.Width),
                    bottomBitmap.Height);
                GraphicsHelper.TileFillUnscaledImageHorizontally(graphics, bottomBitmap, fillRectangle);
            }
        }
    }
}
