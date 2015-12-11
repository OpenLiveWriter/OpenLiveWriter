// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Controls;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Provides a tab scroller lightweight control.
    /// </summary>
    internal class TabScrollerButtonLightweightControl : ButtonBaseLightweightControl
    {
        private const int DEFAULT_WIDTH = 11;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components;

        /// <summary>
        /// The auto-scroll timer.  Used to send AutoScroll events.
        /// </summary>
        private Timer timerAutoScroll;

        /// <summary>
        /// A value indicating whether auto-scroll occurred.  This value is used to suppress the
        /// Scroll event, which we do not want to fire if we auto-scrolled.
        /// </summary>
        private bool autoScrollOccurred = false;

        /// <summary>
        /// Occurs when the TabScrollerButtonLightweightControl is clicked.
        /// </summary>
        public event EventHandler Scroll;

        /// <summary>
        /// Occurs when the TabScrollerButtonLightweightControl is held down.
        /// </summary>
        public event EventHandler AutoScroll;

        /// <summary>
        /// Initializes a new instance of the TabScrollerButtonLightweightControl class.
        /// </summary>
        public TabScrollerButtonLightweightControl(IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the TabScrollerButtonLightweightControl class.
        /// </summary>
        public TabScrollerButtonLightweightControl()
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            InitializeComponent();
        }

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.timerAutoScroll = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            //
            // timerAutoScroll
            //
            this.timerAutoScroll.Tick += new System.EventHandler(this.timerAutoScroll_Tick);
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }
        #endregion

        /// <summary>
        /// Gets the default virtual size of the lightweight control.
        /// </summary>
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public override Size DefaultVirtualSize
        {
            get
            {
                return new Size(DEFAULT_WIDTH, 0);
            }
        }

        /// <summary>
        /// Gets the minimum virtual size of the lightweight control.
        /// </summary>
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public override Size MinimumVirtualSize
        {
            get
            {
                return DefaultVirtualSize;
            }
        }

        /// <summary>
        /// Raises the Click event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnClick(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnClick(e);

            //	Raise the Scroll event if auto-scroll has not occurred.
            if (!autoScrollOccurred)
                OnScroll(EventArgs.Empty);
        }

        /// <summary>
        /// Raises the DoubleClick event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnDoubleClick(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnDoubleClick(e);

            //	Raise the Scroll event if Continuous scroll has not occurred.
            if (!autoScrollOccurred)
                OnScroll(EventArgs.Empty);
        }

        /// <summary>
        /// Raises the MouseDown event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseDown(e);

            //	Start the auto-scroll timer.
            autoScrollOccurred = false;
            timerAutoScroll.Interval = 200;
            timerAutoScroll.Start();
        }

        /// <summary>
        /// Raises the MouseUp event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseUp(e);

            //	Stop the mouse down timer.
            timerAutoScroll.Stop();
        }

        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnPaint(e);

            //	Obtain the face rectangle.
            Rectangle faceRectangle = VirtualClientRectangle;

            //	Draw the edges of the scroller.
            using (SolidBrush solidBrush = new SolidBrush(ApplicationManager.ApplicationStyle.BorderColor))
            {
                //	Draw the top edge.
                e.Graphics.FillRectangle(solidBrush,
                                            faceRectangle.X,
                                            faceRectangle.Y,
                                            faceRectangle.Width,
                                            1);

                //	Draw the left edge.
                e.Graphics.FillRectangle(solidBrush,
                                            faceRectangle.X,
                                            faceRectangle.Y + 1,
                                            1,
                                            faceRectangle.Height - 1);

                //	Draw the right edge.
                e.Graphics.FillRectangle(solidBrush,
                                            faceRectangle.Right - 1,
                                            faceRectangle.Y + 1,
                                            1,
                                            faceRectangle.Height - 1);

                //	Draw the bottom edge.
                e.Graphics.FillRectangle(solidBrush,
                                            faceRectangle.X,
                                            faceRectangle.Bottom - 1,
                                            faceRectangle.Width,
                                            1);
            }

            //	Adjust the face rectangle.
            faceRectangle.Inflate(-1, -1);

            //	Fill face of the scroller.
            using (SolidBrush solidBrush = new SolidBrush(Color.Red))
                e.Graphics.FillRectangle(solidBrush, faceRectangle);

            Color topLeftColor, bottomRightColor;
            if (LeftMouseButtonDown)
            {
                topLeftColor = Color.FromArgb(128, ApplicationManager.ApplicationStyle.ActiveTabLowlightColor);
                bottomRightColor = Color.FromArgb(128, ApplicationManager.ApplicationStyle.ActiveTabHighlightColor);
            }
            else
            {
                topLeftColor = Color.FromArgb(128, ApplicationManager.ApplicationStyle.ActiveTabHighlightColor);
                bottomRightColor = Color.FromArgb(10, ApplicationManager.ApplicationStyle.ActiveTabLowlightColor);
            }

            //	Draw the top/left inside the scroller.
            using (SolidBrush solidBrush = new SolidBrush(topLeftColor))
            {
                //	Draw the top edge.
                e.Graphics.FillRectangle(solidBrush,
                                            faceRectangle.X,
                                            faceRectangle.Y,
                                            faceRectangle.Width,
                                            1);

                //	Draw the left edge.
                e.Graphics.FillRectangle(solidBrush,
                                            faceRectangle.X,
                                            faceRectangle.Y + 1,
                                            1,
                                            faceRectangle.Height - 1);
            }

            //	Draw the bottom/right inside the scroller.
            using (SolidBrush solidBrush = new SolidBrush(bottomRightColor))
            {
                //	Draw the bottom edge.
                e.Graphics.FillRectangle(solidBrush,
                                            faceRectangle.X + 1,
                                            faceRectangle.Bottom - 1,
                                            faceRectangle.Width - 1,
                                            1);

                //	Draw the right edge.
                e.Graphics.FillRectangle(solidBrush,
                                            faceRectangle.Right - 1,
                                            faceRectangle.Y + 1,
                                            1,
                                            faceRectangle.Height - 2);
            }
        }

        /// <summary>
        /// Raises the Scroll event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected internal virtual void OnScroll(EventArgs e)
        {
            if (Scroll != null)
                Scroll(this, e);
        }

        /// <summary>
        /// Raises the AutoScroll event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected internal virtual void OnAutoScroll(EventArgs e)
        {
            if (AutoScroll != null)
            {
                autoScrollOccurred = true;
                AutoScroll(this, e);
            }
        }

        /// <summary>
        /// timerAutoScroll_Tick event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void timerAutoScroll_Tick(object sender, EventArgs e)
        {
            //	Speed up the timer.
            timerAutoScroll.Interval = 15;

            //	Scroll.
            OnAutoScroll(EventArgs.Empty);
        }
    }
}
