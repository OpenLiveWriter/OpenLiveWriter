// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Multipane lightweight control.
    /// </summary>
    public class MultiPaneLightweightControl : LightweightControl
    {
        /// <summary>
        /// The height of the gutter area.
        /// </summary>
        private const int GUTTER_HEIGHT = 22;

        /// <summary>
        /// The border size.
        /// </summary>
        private const int BORDER_SIZE = 4;

        /// <summary>
        /// The splitter width.
        /// </summary>
        private int SPLITTER_WIDTH = 4;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components;

        /// <summary>
        /// The left splitter position.  This is the offset of the left splitter from the left edge
        /// of the multi pane lightweight control.  A value of -1 indicates that the left splitter
        /// position has not been set.
        /// </summary>
        private int leftSplitterPosition = -1;
        private int newLeftSplitterPosition = -1;

        /// <summary>
        /// The right splitter position.  This is the offset of the right splitter from the right
        /// edge of the multi pane lightweight control.  A value of -1 indicates that the right
        /// splitter position has not been set.
        /// </summary>
        private int rightSplitterPosition = -1;
        private int newRightSplitterPosition = -1;

        /// <summary>
        ///
        /// </summary>
        private OpenLiveWriter.ApplicationFramework.WorkPaneLightweightControl workPaneLightweightControlLeft;

        /// <summary>
        ///
        /// </summary>
        private OpenLiveWriter.ApplicationFramework.WorkPaneLightweightControl workPaneLightweightControlCenter;

        /// <summary>
        ///
        /// </summary>
        private OpenLiveWriter.ApplicationFramework.WorkPaneLightweightControl workPaneLightweightControlRight;

        /// <summary>
        /// The left splitter lightweight control.
        /// </summary>
        private OpenLiveWriter.ApplicationFramework.SplitterLightweightControl leftSplitter;

        /// <summary>
        /// The right splitter lightweight control.
        /// </summary>
        private OpenLiveWriter.ApplicationFramework.SplitterLightweightControl rightSplitter;

        /// <summary>
        /// The gutter lightweight control.
        /// </summary>
        private OpenLiveWriter.Controls.GutterLightweightControl gutter;

        /// <summary>
        /// Gets or sets the left pane control.
        /// </summary>
        [
        Category("Layout"),
        Localizable(false),
        DefaultValue(null),
        Description("Specifies the left pane control.")
        ]
        public Control LeftPaneControl
        {
            get
            {
                return workPaneLightweightControlLeft.Control;
            }
            set
            {
                workPaneLightweightControlLeft.Control = value;
            }
        }

        /// <summary>
        /// Gets or sets the center pane control.
        /// </summary>
        [
        Category("Layout"),
        Localizable(false),
        DefaultValue(null),
        Description("Specifies the center pane control.")
        ]
        public Control CenterPaneControl
        {
            get
            {
                return workPaneLightweightControlCenter.Control;
            }
            set
            {
                workPaneLightweightControlCenter.Control = value;
            }
        }

        /// <summary>
        /// Gets or sets the right pane control.
        /// </summary>
        [
        Category("Layout"),
        Localizable(false),
        DefaultValue(null),
        Description("Specifies the right pane control.")
        ]
        public Control RightPaneControl
        {
            get
            {
                return workPaneLightweightControlRight.Control;
            }
            set
            {
                workPaneLightweightControlRight.Control = value;
            }
        }

        /// <summary>
        /// The vertical tracking indicator.  This helper class displays the splitter position when
        /// a splitter is being resized.
        /// </summary>
        VerticalTrackingIndicator verticalTrackingIdicator = new VerticalTrackingIndicator();

        /// <summary>
        /// Initializes a new instance of the MultiPaneLightweightControl class.
        /// </summary>
        /// <param name="container"></param>
        public MultiPaneLightweightControl(System.ComponentModel.IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the MultiPaneLightweightControl class.
        /// </summary>
        public MultiPaneLightweightControl()
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            InitializeComponent();
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.leftSplitter = new OpenLiveWriter.ApplicationFramework.SplitterLightweightControl(this.components);
            this.rightSplitter = new OpenLiveWriter.ApplicationFramework.SplitterLightweightControl(this.components);
            this.gutter = new OpenLiveWriter.Controls.GutterLightweightControl(this.components);
            this.workPaneLightweightControlLeft = new OpenLiveWriter.ApplicationFramework.WorkPaneLightweightControl(this.components);
            this.workPaneLightweightControlCenter = new OpenLiveWriter.ApplicationFramework.WorkPaneLightweightControl(this.components);
            this.workPaneLightweightControlRight = new OpenLiveWriter.ApplicationFramework.WorkPaneLightweightControl(this.components);
            //
            // leftSplitter
            //
            this.leftSplitter.LightweightControlContainerControl = this;
            this.leftSplitter.SplitterEndMove += new OpenLiveWriter.ApplicationFramework.LightweightSplitterEventHandler(this.leftSplitter_SplitterEndMove);
            this.leftSplitter.SplitterBeginMove += new System.EventHandler(this.leftSplitter_SplitterBeginMove);
            this.leftSplitter.SplitterMoving += new OpenLiveWriter.ApplicationFramework.LightweightSplitterEventHandler(this.leftSplitter_SplitterMoving);
            //
            // rightSplitter
            //
            this.rightSplitter.LightweightControlContainerControl = this;
            this.rightSplitter.SplitterEndMove += new OpenLiveWriter.ApplicationFramework.LightweightSplitterEventHandler(this.rightSplitter_SplitterEndMove);
            this.rightSplitter.SplitterBeginMove += new System.EventHandler(this.rightSplitter_SplitterBeginMove);
            this.rightSplitter.SplitterMoving += new OpenLiveWriter.ApplicationFramework.LightweightSplitterEventHandler(this.rightSplitter_SplitterMoving);
            //
            // gutter
            //
            this.gutter.LightweightControlContainerControl = this;
            //
            // workPaneLightweightControlLeft
            //
            this.workPaneLightweightControlLeft.Control = null;
            this.workPaneLightweightControlLeft.LightweightControlContainerControl = this;
            //
            // workPaneLightweightControlCenter
            //
            this.workPaneLightweightControlCenter.Control = null;
            this.workPaneLightweightControlCenter.LightweightControlContainerControl = this;
            //
            // workPaneLightweightControlRight
            //
            this.workPaneLightweightControlRight.Control = null;
            this.workPaneLightweightControlRight.LightweightControlContainerControl = this;

        }
        #endregion

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnLayout(System.EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLayout(e);

            //	Initialize the left splitter position, if it is uninitialized.
            if (leftSplitterPosition == -1)
                InitializeLeftSplitterPosition();

            //	Initialize the right splitter position, if it is uninitialized.
            if (rightSplitterPosition == -1)
                InitializeRightSplitterPosition();

            //	Layout the gutter.
            gutter.VirtualBounds = CalculateGutterBounds();

            //	Layout the left pane lightweight control.
            workPaneLightweightControlLeft.VirtualBounds = CalculateLeftPaneControlBounds();

            //	Layout the left splitter.
            leftSplitter.VirtualBounds = new Rectangle(	leftSplitterPosition,
                                                        0,
                                                        SPLITTER_WIDTH,
                                                        VirtualHeight-GUTTER_HEIGHT);

            //	Layout the center pane lightweight control.
            workPaneLightweightControlCenter.VirtualBounds = CalculateCenterPaneLightweightControlBounds();

            //	Layout the right splitter.
            rightSplitter.VirtualBounds = new Rectangle(VirtualWidth-rightSplitterPosition,
                                                        0,
                                                        SPLITTER_WIDTH,
                                                        VirtualHeight-GUTTER_HEIGHT);

            //	Layout the right pane lightweight control.
            workPaneLightweightControlRight.VirtualBounds = CalculateRightPaneControlBounds();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            //	Paint the background.
            using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(VirtualClientRectangle, Color.FromArgb(214, 232, 239), Color.FromArgb(148, 197, 217), LinearGradientMode.Horizontal))
                e.Graphics.FillRectangle(linearGradientBrush, VirtualClientRectangle);

            //	Call the base class's method so that registered delegates receive the event and
            //	child lightweight controls are painted.
            base.OnPaint(e);
        }

        /// <summary>
        ///
        /// </summary>
        private void InitializeLeftSplitterPosition()
        {
            leftSplitterPosition = 260;//Math.Max(MinimumLeftSplitterPosition(), VirtualWidth/5);
        }

        /// <summary>
        ///
        /// </summary>
        private void InitializeRightSplitterPosition()
        {
            rightSplitterPosition = Math.Max(MinimumRightSplitterPosition(), VirtualWidth/5);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private Rectangle CalculateVerticalTrackingIndicatorRectangle(int position)
        {
            return new Rectangle(	position,
                                    0,
                                    SPLITTER_WIDTH-1,
                                    VirtualHeight-GUTTER_HEIGHT);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private Rectangle CalculateNewLeftSplitterBounds()
        {
            return new Rectangle(	newLeftSplitterPosition,
                                    -1,
                                    SPLITTER_WIDTH+1,
                                    VirtualHeight-GUTTER_HEIGHT);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private Rectangle CalculateLeftSplitterBounds()
        {
            return new Rectangle(	leftSplitterPosition,
                                    0,
                                    SPLITTER_WIDTH,
                                    VirtualHeight-GUTTER_HEIGHT);
        }

        /// <summary>
        /// Calculates the bounds of the gutter control.
        /// </summary>
        /// <returns>The bounds of the gutter control.</returns>
        private Rectangle CalculateGutterBounds()
        {
            return new Rectangle(	0,
                                    VirtualHeight-GUTTER_HEIGHT,
                                    VirtualWidth,
                                    GUTTER_HEIGHT);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private Rectangle CalculateLeftPaneControlBounds()
        {
            return new Rectangle(	BORDER_SIZE,
                                    BORDER_SIZE,
                                    leftSplitterPosition-BORDER_SIZE,
                                    PaneHeight());
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private Rectangle CalculateCenterPaneLightweightControlBounds()
        {
            return new Rectangle(	leftSplitterPosition+SPLITTER_WIDTH,
                                    BORDER_SIZE,
                                    VirtualWidth-(rightSplitterPosition+leftSplitterPosition+SPLITTER_WIDTH),
                                    PaneHeight());
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private Rectangle CalculateRightPaneControlBounds()
        {
            int rightControlX = VirtualWidth-rightSplitterPosition+SPLITTER_WIDTH;
            return new Rectangle(	rightControlX,
                                    BORDER_SIZE,
                                    VirtualWidth-(rightControlX+BORDER_SIZE),
                                    PaneHeight());
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private int PaneHeight()
        {
            return VirtualHeight-GUTTER_HEIGHT-(BORDER_SIZE*2);
        }

        /// <summary>
        /// Calculates the minimum left splitter position.
        /// </summary>
        /// <returns></returns>
        private int MinimumLeftSplitterPosition()
        {
            return 0;
        }

        /// <summary>
        /// Calculates the maximum left splitter position.
        /// </summary>
        /// <returns></returns>
        private int MaximumLeftSplitterPosition()
        {
            return VirtualWidth-10;
        }

        /// <summary>
        /// Calculates the minimum right splitter position.
        /// </summary>
        /// <returns></returns>
        private int MinimumRightSplitterPosition()
        {
            return 100;
        }

        /// <summary>
        /// Calculates the maximum right splitter position.
        /// </summary>
        /// <returns></returns>
        private int MaximumRightSplitterPosition()
        {
            return VirtualWidth-10;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void leftSplitter_SplitterBeginMove(object sender, System.EventArgs e)
        {
            //	Calculate.
            Rectangle trackingIndicatorRectangle = CalculateVerticalTrackingIndicatorRectangle(leftSplitterPosition);

            verticalTrackingIdicator.Begin(Parent, VirtualClientRectangleToParent(trackingIndicatorRectangle));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void leftSplitter_SplitterEndMove(object sender, OpenLiveWriter.ApplicationFramework.LightweightSplitterEventArgs e)
        {
            verticalTrackingIdicator.End();
            leftSplitterPosition = newLeftSplitterPosition;
            PerformLayout();
            Invalidate();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void leftSplitter_SplitterMoving(object sender, OpenLiveWriter.ApplicationFramework.LightweightSplitterEventArgs e)
        {
            //	Obtain the minimum and maximum left splitter positions.
            int minimumLeftSplitterPosition = MinimumLeftSplitterPosition();
            int maximumLeftSplitterPosition = MaximumLeftSplitterPosition();

            //	Calculate the new left splitter position.
            newLeftSplitterPosition = leftSplitterPosition+e.Position;

            //	Validate the new left splitter position.  Adjust it as needed.
            if (newLeftSplitterPosition < minimumLeftSplitterPosition)
                newLeftSplitterPosition = minimumLeftSplitterPosition;
            else if (newLeftSplitterPosition > maximumLeftSplitterPosition)
                newLeftSplitterPosition = maximumLeftSplitterPosition;

            //	Calculate.
            Rectangle trackingIndicatorRectangle = CalculateVerticalTrackingIndicatorRectangle(newLeftSplitterPosition);

            //	Set the new left splitter position, if it has changed.
            verticalTrackingIdicator.Update(VirtualClientRectangleToParent(trackingIndicatorRectangle).Location);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rightSplitter_SplitterBeginMove(object sender, System.EventArgs e)
        {
            //	Calculate.
            Rectangle trackingIndicatorRectangle = CalculateVerticalTrackingIndicatorRectangle(VirtualWidth-rightSplitterPosition);

            verticalTrackingIdicator.Begin(Parent, VirtualClientRectangleToParent(trackingIndicatorRectangle));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rightSplitter_SplitterEndMove(object sender, OpenLiveWriter.ApplicationFramework.LightweightSplitterEventArgs e)
        {
            verticalTrackingIdicator.End();
            rightSplitterPosition = newRightSplitterPosition;
            PerformLayout();
            Invalidate();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rightSplitter_SplitterMoving(object sender, OpenLiveWriter.ApplicationFramework.LightweightSplitterEventArgs e)
        {
            //	Obtain the minimum and maximum right splitter positions.
            int minimumRightSplitterPosition = MinimumRightSplitterPosition();
            int maximumRightSplitterPosition = MaximumRightSplitterPosition();

            //	Calculate the new right splitter position.
            newRightSplitterPosition = rightSplitterPosition-e.Position;

            //	Validate the new right splitter position.  Adjust it as needed.
            if (newRightSplitterPosition < minimumRightSplitterPosition)
                newRightSplitterPosition = minimumRightSplitterPosition;
            else if (newRightSplitterPosition > maximumRightSplitterPosition)
                newRightSplitterPosition = maximumRightSplitterPosition;

            //	Calculate.
            Rectangle trackingIndicatorRectangle = CalculateVerticalTrackingIndicatorRectangle(VirtualWidth-newRightSplitterPosition);

            //	Set the new right splitter position, if it has changed.
            verticalTrackingIdicator.Update(VirtualClientRectangleToParent(trackingIndicatorRectangle).Location);
        }
    }
}
