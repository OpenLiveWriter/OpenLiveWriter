// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using OpenLiveWriter.Controls;

namespace OpenLiveWriter.ApplicationFramework
{
    #region Public Enumeration Declarations

    /// <summary>
    /// The vertical splitter style.
    /// </summary>
    internal enum VerticalSplitterStyle
    {
        None,   //	No vertical splitter.
        Left,   //	Vertical splitter on the left edge of the column.
        Right   //	Vertical splitter on the right edge of the column.
    }

    #endregion Public Enumeration Declarations

    /// <summary>
    /// Provides the workspace column control.
    /// </summary>
    public class WorkspaceColumnLightweightControl : LightweightControl
    {
        #region Private Member Variables & Declarations

        /// <summary>
        /// The minimum horizontal splitter position.
        /// </summary>
        private static readonly double MINIMUM_HORIZONTAL_SPLITTER_POSITION = 0.20;

        /// <summary>
        /// The default maximum horizontal splitter position.
        /// </summary>
        private static readonly double MAXIMUM_HORIZONTAL_SPLITTER_POSITION = 0.80;

        /// <summary>
        /// Required designer cruft.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// The vertical splitter lightweight control.
        /// </summary>
        private SplitterLightweightControl splitterLightweightControlVertical;

        /// <summary>
        /// The horizontal splitter lightweight control.
        /// </summary>
        private SplitterLightweightControl splitterLightweightControlHorizontal;

        /// <summary>
        /// The upper pane WorkspaceColumnPaneLightweightControl.
        /// </summary>
        private WorkspaceColumnPaneLightweightControl workspaceColumnPaneUpper;

        /// <summary>
        /// The lower pane WorkspaceColumnPaneLightweightControl.
        /// </summary>
        private WorkspaceColumnPaneLightweightControl workspaceColumnPaneLower;

        /// <summary>
        /// The vertical splitter style.
        /// </summary>
        private VerticalSplitterStyle verticalSplitterStyle = VerticalSplitterStyle.None;

        /// <summary>
        /// The vertical splitter width.
        /// </summary>
        private int verticalSplitterWidth = 5;

        /// <summary>
        /// The horizontal splitter height.
        /// </summary>
        private int horizontalSplitterHeight = 5;

        /// <summary>
        /// The maximum horizontal splitter position.
        /// </summary>
        private double maximumHorizontalSplitterPosition = MAXIMUM_HORIZONTAL_SPLITTER_POSITION;

        /// <summary>
        /// The starting preferred column width.
        /// </summary>
        private double startingPreferredColumnWidth;

        /// <summary>
        /// The preferred column width.
        /// </summary>
        private int preferredColumnWidth = 0;

        /// <summary>
        /// The minimum column width.
        /// </summary>
        private int minimumColumnWidth = 0;

        /// <summary>
        /// The maximum column width.
        /// </summary>
        private int maximumColumnWidth = 0;

        /// <summary>
        /// The starting horizontal splitter position.
        /// </summary>
        private double startingHorizontalSplitterPosition;

        /// <summary>
        /// The horizontal splitter position.
        /// </summary>
        private double horizontalSplitterPosition = 0.50;

        #endregion Private Member Variables & Declarations

        #region Public Events

        /// <summary>
        /// Occurs when the PreferredColumnWidth changes.
        /// </summary>
        [
            Category("Property Changed"),
                Description("Occurs when the PreferredColumnWidth property is changed.")
        ]
        public event EventHandler PreferredColumnWidthChanged;

        /// <summary>
        /// Occurs when the MinimumColumnWidth changes.
        /// </summary>
        [
            Category("Property Changed"),
                Description("Occurs when the MinimumColumnWidth property is changed.")
        ]
        public event EventHandler MinimumColumnWidthChanged;

        /// <summary>
        /// Occurs when the MaximumColumnWidth changes.
        /// </summary>
        [
            Category("Property Changed"),
                Description("Occurs when the MaximumColumnWidth property is changed.")
        ]
        public event EventHandler MaximumColumnWidthChanged;

        /// <summary>
        /// Occurs when the HorizontalSplitterPosition changes.
        /// </summary>
        [
            Category("Property Changed"),
                Description("Occurs when the HorizontalSplitterPosition property is changed.")
        ]
        public event EventHandler HorizontalSplitterPositionChanged;

        /// <summary>
        /// Occurs when the horizontal splitter begins moving.
        /// </summary>
        [
            Category("Action"),
                Description("Occurs when the horizontal splitter begins moving.")
        ]
        public event EventHandler HorizontalSplitterBeginMove;

        /// <summary>
        /// Occurs when the horizontal splitter is ends moving.
        /// </summary>
        [
            Category("Action"),
                Description("Occurs when the horizontal splitter ends moving.")
        ]
        public event EventHandler HorizontalSplitterEndMove;

        /// <summary>
        /// Occurs when the horizontal splitter is moving.
        /// </summary>
        [
            Category("Action"),
                Description("Occurs when the horizontal splitter is moving.")
        ]
        public event EventHandler HorizontalSplitterMoving;

        /// <summary>
        /// Occurs when the vertical splitter begins moving.
        /// </summary>
        [
            Category("Action"),
                Description("Occurs when the vertical splitter begins moving.")
        ]
        public event EventHandler VerticalSplitterBeginMove;

        /// <summary>
        /// Occurs when the vertical splitter is ends moving.
        /// </summary>
        [
            Category("Action"),
                Description("Occurs when the vertical splitter ends moving.")
        ]
        public event EventHandler VerticalSplitterEndMove;

        /// <summary>
        /// Occurs when the vertical splitter is moving.
        /// </summary>
        [
            Category("Action"),
                Description("Occurs when the vertical splitter is moving.")
        ]
        public event EventHandler VerticalSplitterMoving;

        #endregion Public Events

        #region Class Initialization & Termination

        /// <summary>
        /// Initializes a new instance of the WorkspaceColumnLightweightControl class.
        /// </summary>
        public WorkspaceColumnLightweightControl(IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the WorkspaceColumnLightweightControl class.
        /// </summary>
        public WorkspaceColumnLightweightControl()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();
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

        #endregion Class Initialization & Termination

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.splitterLightweightControlVertical = new OpenLiveWriter.ApplicationFramework.SplitterLightweightControl(this.components);
            this.splitterLightweightControlHorizontal = new OpenLiveWriter.ApplicationFramework.SplitterLightweightControl(this.components);
            this.workspaceColumnPaneUpper = new OpenLiveWriter.ApplicationFramework.WorkspaceColumnPaneLightweightControl(this.components);
            this.workspaceColumnPaneLower = new OpenLiveWriter.ApplicationFramework.WorkspaceColumnPaneLightweightControl(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitterLightweightControlVertical)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitterLightweightControlHorizontal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.workspaceColumnPaneUpper)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.workspaceColumnPaneLower)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            //
            // splitterLightweightControlVertical
            //
            this.splitterLightweightControlVertical.LightweightControlContainerControl = this;
            this.splitterLightweightControlVertical.Orientation = OpenLiveWriter.ApplicationFramework.SplitterLightweightControl.SplitterOrientation.Vertical;
            this.splitterLightweightControlVertical.SplitterEndMove += new OpenLiveWriter.ApplicationFramework.LightweightSplitterEventHandler(this.splitterLightweightControlVertical_SplitterEndMove);
            this.splitterLightweightControlVertical.SplitterBeginMove += new System.EventHandler(this.splitterLightweightControlVertical_SplitterBeginMove);
            this.splitterLightweightControlVertical.SplitterMoving += new OpenLiveWriter.ApplicationFramework.LightweightSplitterEventHandler(this.splitterLightweightControlVertical_SplitterMoving);
            //
            // splitterLightweightControlHorizontal
            //
            this.splitterLightweightControlHorizontal.LightweightControlContainerControl = this;
            this.splitterLightweightControlHorizontal.SplitterEndMove += new OpenLiveWriter.ApplicationFramework.LightweightSplitterEventHandler(this.splitterLightweightControlHorizontal_SplitterEndMove);
            this.splitterLightweightControlHorizontal.SplitterBeginMove += new System.EventHandler(this.splitterLightweightControlHorizontal_SplitterBeginMove);
            this.splitterLightweightControlHorizontal.SplitterMoving += new OpenLiveWriter.ApplicationFramework.LightweightSplitterEventHandler(this.splitterLightweightControlHorizontal_SplitterMoving);
            //
            // workspaceColumnPaneUpper
            //
            this.workspaceColumnPaneUpper.Border = false;
            this.workspaceColumnPaneUpper.Control = null;
            this.workspaceColumnPaneUpper.FixedHeight = 0;
            this.workspaceColumnPaneUpper.FixedHeightLayout = false;
            this.workspaceColumnPaneUpper.LightweightControl = null;
            this.workspaceColumnPaneUpper.LightweightControlContainerControl = this;
            this.workspaceColumnPaneUpper.Visible = false;
            this.workspaceColumnPaneUpper.VisibleChanged += new System.EventHandler(this.workspaceColumnPaneUpper_VisibleChanged);
            //
            // workspaceColumnPaneLower
            //
            this.workspaceColumnPaneLower.Border = false;
            this.workspaceColumnPaneLower.Control = null;
            this.workspaceColumnPaneLower.FixedHeight = 0;
            this.workspaceColumnPaneLower.FixedHeightLayout = false;
            this.workspaceColumnPaneLower.LightweightControl = null;
            this.workspaceColumnPaneLower.LightweightControlContainerControl = this;
            this.workspaceColumnPaneLower.Visible = false;
            this.workspaceColumnPaneLower.VisibleChanged += new System.EventHandler(this.workspaceColumnPaneLower_VisibleChanged);
            //
            // WorkspaceColumnLightweightControl
            //
            this.AllowDrop = true;
            ((System.ComponentModel.ISupportInitialize)(this.splitterLightweightControlVertical)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitterLightweightControlHorizontal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.workspaceColumnPaneUpper)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.workspaceColumnPaneLower)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }
        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the upper pane WorkspaceColumnPaneLightweightControl.
        /// </summary>
        public WorkspaceColumnPaneLightweightControl UpperPane
        {
            get
            {
                return workspaceColumnPaneUpper;
            }
        }

        /// <summary>
        /// Gets or sets the lower pane WorkspaceColumnPaneLightweightControl.
        /// </summary>
        public WorkspaceColumnPaneLightweightControl LowerPane
        {
            get
            {
                return workspaceColumnPaneLower;
            }
        }

        /// <summary>
        /// Gets or sets the control that is attached to the splitter control.
        /// </summary>
        public LightweightControl HorizontalSplitterAttachedControl
        {
            get
            {
                return splitterLightweightControlHorizontal.AttachedControl;
            }

            set
            {
                splitterLightweightControlHorizontal.AttachedControl = value;
            }
        }

        /// <summary>
        /// Gets or sets the vertical splitter style.
        /// </summary>
        [
            Browsable(false)
        ]
        internal VerticalSplitterStyle VerticalSplitterStyle
        {
            get
            {
                return verticalSplitterStyle;
            }
            set
            {
                if (verticalSplitterStyle != value)
                {
                    verticalSplitterStyle = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the vertical splitter width.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(5),
                Description("Specifies the initial vertical splitter width.")
        ]
        public int VerticalSplitterWidth
        {
            get
            {
                return verticalSplitterWidth;
            }
            set
            {
                if (verticalSplitterWidth != value)
                {
                    verticalSplitterWidth = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the horizontal splitter height.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(5),
                Description("Specifies the initial horizontal splitter height.")
        ]
        public int HorizontalSplitterHeight
        {
            get
            {
                return horizontalSplitterHeight;
            }
            set
            {
                if (horizontalSplitterHeight != value)
                {
                    horizontalSplitterHeight = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the preferred column width.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(0),
                Description("Specifies the preferred column width.")
        ]
        public int PreferredColumnWidth
        {
            get
            {
                return preferredColumnWidth;
            }
            set
            {
                if (preferredColumnWidth != value)
                {
                    preferredColumnWidth = value;
                    OnPreferredColumnWidthChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the preferred column width.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(0),
                Description("Specifies the minimum column width.")
        ]
        public int MinimumColumnWidth
        {
            get
            {
                return minimumColumnWidth;
            }
            set
            {
                if (minimumColumnWidth != value)
                {
                    minimumColumnWidth = value;
                    OnMinimumColumnWidthChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum column width.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(0),
                Description("Specifies the maximum column width.")
        ]
        public int MaximumColumnWidth
        {
            get
            {
                return maximumColumnWidth;
            }
            set
            {
                if (maximumColumnWidth != value)
                {
                    maximumColumnWidth = value;
                    OnMaximumColumnWidthChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the preferred horizontal splitter position.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(0.50),
                Description("Specifies the horizontal splitter position.")
        ]
        public double HorizontalSplitterPosition
        {
            get
            {
                return horizontalSplitterPosition;
            }
            set
            {
                //	Check the new horizontal splitter position.
                if (value < MINIMUM_HORIZONTAL_SPLITTER_POSITION)
                    value = MINIMUM_HORIZONTAL_SPLITTER_POSITION;
                else if (value > MaximumHorizontalSplitterPosition)
                    value = MaximumHorizontalSplitterPosition;

                //	If the horizontal splitter position is changing, change it.
                if (horizontalSplitterPosition != value)
                {
                    //	Change it.
                    horizontalSplitterPosition = value;

                    //	Raise the HorizontalSplitterPositionChanged event.
                    OnHorizontalSplitterPositionChanged(EventArgs.Empty);

                    //	Layout and invalidate.
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Specifies the maximum horizontal splitter position (as a percentage of the overall column size).
        /// </summary>
        [
            Category("Behavior"),
                DefaultValue(0.80),
                Description("Specifies the maximum horizontal splitter position (as a percentage of the overall column size).")
        ]
        public double MaximumHorizontalSplitterPosition
        {
            get
            {
                return maximumHorizontalSplitterPosition;
            }
            set
            {
                maximumHorizontalSplitterPosition = value;

                //update the current horizontal  position in case it exceeds the new limit.
                HorizontalSplitterPosition = HorizontalSplitterPosition;
            }
        }

        /// <summary>
        /// Gets or Sets the layout position of the horizontal splitter (in Y pixels).
        /// </summary>
        public int HorizontalSplitterLayoutPosition
        {
            set
            {
                if (value == 0 || PaneLayoutHeight == 0)
                    HorizontalSplitterPosition = MinimumHorizontalSplitterLayoutPosition;
                else
                    HorizontalSplitterPosition = ((double)value) / PaneLayoutHeight;
            }
            get
            {
                return (int)(PaneLayoutHeight * HorizontalSplitterPosition);
            }
        }

        #endregion Public Properties

        #region Protected Events

        /// <summary>
        /// Raises the MaximumColumnWidthChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnMaximumColumnWidthChanged(EventArgs e)
        {
            if (MaximumColumnWidthChanged != null)
                MaximumColumnWidthChanged(this, e);
        }

        /// <summary>
        /// Raises the MinimumColumnWidthChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnMinimumColumnWidthChanged(EventArgs e)
        {
            if (MinimumColumnWidthChanged != null)
                MinimumColumnWidthChanged(this, e);
        }

        /// <summary>
        /// Raises the PreferredColumnWidthChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnPreferredColumnWidthChanged(EventArgs e)
        {
            if (PreferredColumnWidthChanged != null)
                PreferredColumnWidthChanged(this, e);
        }

        /// <summary>
        /// Raises the HorizontalSplitterPositionChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnHorizontalSplitterPositionChanged(EventArgs e)
        {
            if (HorizontalSplitterPositionChanged != null)
                HorizontalSplitterPositionChanged(this, e);
        }

        /// <summary>
        /// Raises the HorizontalSplitterBeginMove event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnHorizontalSplitterBeginMove(EventArgs e)
        {
            if (HorizontalSplitterBeginMove != null)
                HorizontalSplitterBeginMove(this, e);
        }

        /// <summary>
        /// Raises the HorizontalSplitterEndMove event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnHorizontalSplitterEndMove(EventArgs e)
        {
            if (HorizontalSplitterEndMove != null)
                HorizontalSplitterEndMove(this, e);
        }

        /// <summary>
        /// Raises the HorizontalSplitterMoving event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnHorizontalSplitterMoving(EventArgs e)
        {
            if (HorizontalSplitterMoving != null)
                HorizontalSplitterMoving(this, e);
        }

        /// <summary>
        /// Raises the VerticalSplitterBeginMove event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnVerticalSplitterBeginMove(EventArgs e)
        {
            if (VerticalSplitterBeginMove != null)
                VerticalSplitterBeginMove(this, e);
        }

        /// <summary>
        /// Raises the VerticalSplitterEndMove event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnVerticalSplitterEndMove(EventArgs e)
        {
            if (VerticalSplitterEndMove != null)
                VerticalSplitterEndMove(this, e);
        }

        /// <summary>
        /// Raises the VerticalSplitterMoving event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnVerticalSplitterMoving(EventArgs e)
        {
            if (VerticalSplitterMoving != null)
                VerticalSplitterMoving(this, e);
        }

        #endregion Protected Events

        #region Protected Event Overrides

        /// <summary>
        /// Raises the Layout event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLayout(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLayout(e);

            //	Layout the vertical splitter.
            Rectangle layoutRectangle = LayoutVerticalSplitter();

            //	Layout the upper and lower panes if they are both visible.
            if (workspaceColumnPaneUpper.Visible && workspaceColumnPaneLower.Visible)
            {
                //	Calculate the pane layout height (the area available for layout of the upper
                //	and lower panes).
                int paneLayoutHeight = layoutRectangle.Height - horizontalSplitterHeight;

                //	If the upper pane is fixed height, layout the column this way.
                if (workspaceColumnPaneUpper.FixedHeightLayout)
                {
                    //	If the upper pane's fixed height is larger than the layout rectangle height,
                    //	layout just the upper pane.
                    if (workspaceColumnPaneUpper.FixedHeight > layoutRectangle.Height)
                    {
                        workspaceColumnPaneUpper.VirtualBounds = layoutRectangle;
                        splitterLightweightControlHorizontal.Visible = false;
                        workspaceColumnPaneLower.VirtualBounds = Rectangle.Empty;
                        workspaceColumnPaneUpper.PerformLayout();
                    }
                    //	Layout both the upper and lower panes.
                    else
                    {
                        //	Layout the upper pane lightweight control.
                        workspaceColumnPaneUpper.VirtualBounds = new Rectangle(layoutRectangle.X,
                                                                                layoutRectangle.Y,
                                                                                layoutRectangle.Width,
                                                                                workspaceColumnPaneUpper.FixedHeight);
                        workspaceColumnPaneUpper.PerformLayout();

                        //	Layout the horizontal splitter lightweight control and disable it.
                        splitterLightweightControlHorizontal.Visible = true;
                        splitterLightweightControlHorizontal.Enabled = false;
                        splitterLightweightControlHorizontal.VirtualBounds = new Rectangle(layoutRectangle.X,
                                                                                            workspaceColumnPaneUpper.VirtualBounds.Bottom,
                                                                                            layoutRectangle.Width,
                                                                                            horizontalSplitterHeight);

                        //	Layout the lower pane lightweight control.
                        workspaceColumnPaneLower.VirtualBounds = new Rectangle(layoutRectangle.X,
                                                                                splitterLightweightControlHorizontal.VirtualBounds.Bottom,
                                                                                layoutRectangle.Width,
                                                                                layoutRectangle.Height - (workspaceColumnPaneUpper.VirtualHeight + horizontalSplitterHeight));
                        workspaceColumnPaneLower.PerformLayout();
                    }
                }
                //	If the lower pane is fixed height, layout the column this way.
                else if (workspaceColumnPaneLower.FixedHeightLayout)
                {
                    //	If the upper pane's fixed height is larger than the layout rectangle height,
                    //	layout just the upper pane.
                    if (workspaceColumnPaneLower.FixedHeight > layoutRectangle.Height)
                    {
                        workspaceColumnPaneLower.VirtualBounds = layoutRectangle;
                        splitterLightweightControlHorizontal.Visible = false;
                        workspaceColumnPaneLower.VirtualBounds = Rectangle.Empty;
                        workspaceColumnPaneLower.PerformLayout();
                    }
                    //	Layout both the upper and lower panes.
                    else
                    {
                        //	Layout the lower pane lightweight control.
                        workspaceColumnPaneLower.VirtualBounds = new Rectangle(layoutRectangle.X,
                                                                                layoutRectangle.Bottom - workspaceColumnPaneLower.FixedHeight,
                                                                                layoutRectangle.Width,
                                                                                workspaceColumnPaneLower.FixedHeight);
                        workspaceColumnPaneLower.PerformLayout();

                        //	Layout the horizontal splitter lightweight control and disable it.
                        splitterLightweightControlHorizontal.Visible = true;
                        splitterLightweightControlHorizontal.Enabled = false;
                        splitterLightweightControlHorizontal.VirtualBounds = new Rectangle(layoutRectangle.X,
                                                                                            workspaceColumnPaneLower.VirtualBounds.Top - horizontalSplitterHeight,
                                                                                            layoutRectangle.Width,
                                                                                            horizontalSplitterHeight);

                        //	Layout the upper pane lightweight control.
                        workspaceColumnPaneUpper.VirtualBounds = new Rectangle(layoutRectangle.X,
                                                                                layoutRectangle.Y,
                                                                                layoutRectangle.Width,
                                                                                splitterLightweightControlHorizontal.VirtualBounds.Top);
                        workspaceColumnPaneUpper.PerformLayout();
                    }
                }
                //	If the pane layout height is too small (i.e. there isn't enough room to show
                //	both panes), err on the side of showing just the top pane.  This is an extreme
                //	edge condition.
                else if (paneLayoutHeight < 10 * horizontalSplitterHeight)
                {
                    //	Only the upper pane lightweight control is visible.
                    workspaceColumnPaneUpper.VirtualBounds = layoutRectangle;
                    workspaceColumnPaneLower.VirtualBounds = Rectangle.Empty;
                    splitterLightweightControlHorizontal.Visible = false;
                    splitterLightweightControlHorizontal.VirtualBounds = Rectangle.Empty;
                }
                else
                {
                    //	Get the horizontal splitter layout position.
                    int horizontalSplitterLayoutPosition = HorizontalSplitterLayoutPosition;

                    //	Layout the upper pane lightweight control.
                    workspaceColumnPaneUpper.VirtualBounds = new Rectangle(layoutRectangle.X,
                                                                            layoutRectangle.Y,
                                                                            layoutRectangle.Width,
                                                                            horizontalSplitterLayoutPosition);
                    workspaceColumnPaneUpper.PerformLayout();

                    //	Layout the horizontal splitter lightweight control and enable it.
                    splitterLightweightControlHorizontal.Visible = true;
                    splitterLightweightControlHorizontal.Enabled = true;
                    splitterLightweightControlHorizontal.VirtualBounds = new Rectangle(layoutRectangle.X,
                                                                                        workspaceColumnPaneUpper.VirtualBounds.Bottom,
                                                                                        layoutRectangle.Width,
                                                                                        horizontalSplitterHeight);

                    //	Layout the lower pane lightweight control.
                    workspaceColumnPaneLower.VirtualBounds = new Rectangle(layoutRectangle.X,
                                                                                                splitterLightweightControlHorizontal.VirtualBounds.Bottom,
                                                                                                layoutRectangle.Width,
                                                                                                layoutRectangle.Height - (workspaceColumnPaneUpper.VirtualHeight + horizontalSplitterHeight));
                    workspaceColumnPaneLower.PerformLayout();
                }
            }
            //	Layout the upper pane, if it's visible.  Note that we ignore the FixedHeight
            //	property of the pane in this case because it doesn't make sense to layout a
            //	pane that doesn't fill the entire height of the column.
            else if (workspaceColumnPaneUpper.Visible)
            {
                //	Only the upper pane lightweight control is visible.
                workspaceColumnPaneUpper.VirtualBounds = layoutRectangle;
                splitterLightweightControlHorizontal.Visible = false;
                workspaceColumnPaneUpper.PerformLayout();
            }
            //	Layout the lower pane, if it's visible.  Note that we ignore the FixedHeight
            //	property of the pane in this case because it doesn't make sense to layout a
            //	pane that doesn't fill the entire height of the column.
            else if (workspaceColumnPaneLower.Visible)
            {
                //	Only the lower pane lightweight control is visible.
                workspaceColumnPaneLower.VirtualBounds = layoutRectangle;
                splitterLightweightControlHorizontal.Visible = false;
                workspaceColumnPaneLower.PerformLayout();
            }
        }

        #endregion Protected Event Overrides

        #region Private Properties

        /// <summary>
        /// Gets the maximum width increase.
        /// </summary>
        private int MaximumWidthIncrease
        {
            get
            {
                Debug.Assert(VirtualWidth <= MaximumColumnWidth, "The column is wider than it's maximum width.");
                return Math.Max(0, MaximumColumnWidth - VirtualWidth);
            }
        }

        /// <summary>
        /// Gets the maximum width decrease.
        /// </summary>
        private int MaximumWidthDecrease
        {
            get
            {
                Debug.Assert(VirtualWidth >= MinimumColumnWidth, "The column is narrower than it's minimum width.");
                return Math.Max(0, VirtualWidth - MinimumColumnWidth);
            }
        }

        /// <summary>
        /// Gets the pane layout height.
        /// </summary>
        private int PaneLayoutHeight
        {
            get
            {
                return Math.Max(0, VirtualClientRectangle.Height - horizontalSplitterHeight);
            }
        }

        /// <summary>
        /// Gets the minimum layout position of the horizontal splitter.
        /// </summary>
        private int MinimumHorizontalSplitterLayoutPosition
        {
            get
            {
                return (int)(PaneLayoutHeight * MINIMUM_HORIZONTAL_SPLITTER_POSITION);
            }
        }

        /// <summary>
        /// Gets the maximum layout position of the horizontal splitter.
        /// </summary>
        private int MaximumHorizontalSplitterLayoutPosition
        {
            get
            {
                return (int)(PaneLayoutHeight * MaximumHorizontalSplitterPosition);
            }
        }

        #endregion Private Properties

        #region Private Metods

        /// <summary>
        /// Helper that performs layout logic for the vertical splitter.
        /// </summary>
        /// <returns>The layout rectangle for the next phases of layout.</returns>
        private Rectangle LayoutVerticalSplitter()
        {
            //	Obtain the layout rectangle.
            Rectangle layoutRectangle = VirtualClientRectangle;

            //	Layout the vertical splitter lightweight control and adjust the layout rectangle
            //	as needed.
            if (verticalSplitterStyle == VerticalSplitterStyle.None)
            {
                //	No vertical splitter lightweight control.
                splitterLightweightControlVertical.Visible = false;
                splitterLightweightControlVertical.VirtualBounds = Rectangle.Empty;
            }
            else if (verticalSplitterStyle == VerticalSplitterStyle.Left)
            {
                //	Left vertical splitter lightweight control.
                splitterLightweightControlVertical.Visible = true;
                splitterLightweightControlVertical.VirtualBounds = new Rectangle(layoutRectangle.X,
                                                                                    layoutRectangle.Y,
                                                                                    verticalSplitterWidth,
                                                                                    layoutRectangle.Height);

                //	Adjust the layout rectangle.
                layoutRectangle.X += verticalSplitterWidth;
                layoutRectangle.Width -= verticalSplitterWidth;
            }
            else if (verticalSplitterStyle == VerticalSplitterStyle.Right)
            {
                //	Right vertical splitter lightweight control.
                splitterLightweightControlVertical.Visible = true;
                splitterLightweightControlVertical.VirtualBounds = new Rectangle(layoutRectangle.Right - verticalSplitterWidth,
                                                                                    layoutRectangle.Top,
                                                                                    verticalSplitterWidth,
                                                                                    layoutRectangle.Height);

                //	Adjust the layout rectangle.
                layoutRectangle.Width -= verticalSplitterWidth;
            }

            //	Done!  Return the layout rectangle.
            return layoutRectangle;
        }

        private void LayoutWithFixedHeightUpperPane(Rectangle layoutRectangle)
        {
        }

        private void LayoutWithFixedHeightLowerPane(Rectangle layoutRectangle)
        {

        }

        /// <summary>
        /// Helper to adjust the Position in the LightweightSplitterEventArgs for the vertical splitter.
        /// </summary>
        /// <param name="e">LightweightSplitterEventArgs to adjust.</param>
        private void AdjustVerticalLightweightSplitterEventArgsPosition(ref LightweightSplitterEventArgs e)
        {
            //	If the vertical splitter style is non, we shouldn't receive this event.
            Debug.Assert(verticalSplitterStyle != VerticalSplitterStyle.None);
            if (verticalSplitterStyle == VerticalSplitterStyle.None)
                return;

            //	Left or right splitter style.
            if (verticalSplitterStyle == VerticalSplitterStyle.Left)
            {
                if (e.Position < 0)
                {
                    if (Math.Abs(e.Position) > MaximumWidthIncrease)
                        e.Position = MaximumWidthIncrease * -1;
                }
                else
                {
                    if (e.Position > MaximumWidthDecrease)
                        e.Position = MaximumWidthDecrease;
                }
            }
            else if (verticalSplitterStyle == VerticalSplitterStyle.Right)
            {
                if (e.Position > 0)
                {
                    if (e.Position > MaximumWidthIncrease)
                        e.Position = MaximumWidthIncrease;
                }
                else
                {
                    if (Math.Abs(e.Position) > MaximumWidthDecrease)
                        e.Position = MaximumWidthDecrease * -1;
                }
            }
        }

        /// <summary>
        /// Helper to adjust the Position in the LightweightSplitterEventArgs for the horizontal splitter.
        /// </summary>
        /// <param name="e">LightweightSplitterEventArgs to adjust.</param>
        private void AdjustHorizontalLightweightSplitterEventArgsPosition(ref LightweightSplitterEventArgs e)
        {
            int horizontalSplitterLayoutPosition = HorizontalSplitterLayoutPosition;
            if (e.Position < 0)
            {
                if (HorizontalSplitterLayoutPosition + e.Position < MinimumHorizontalSplitterLayoutPosition)
                    e.Position = MinimumHorizontalSplitterLayoutPosition - horizontalSplitterLayoutPosition;
            }
            else
            {
                if (HorizontalSplitterLayoutPosition + e.Position > MaximumHorizontalSplitterLayoutPosition)
                    e.Position = MaximumHorizontalSplitterLayoutPosition - horizontalSplitterLayoutPosition;
            }
        }

        #endregion Private Metods

        #region Private Event Handler

        /// <summary>
        /// splitterLightweightControlVertical_SplitterBeginMove event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void splitterLightweightControlVertical_SplitterBeginMove(object sender, EventArgs e)
        {
            startingPreferredColumnWidth = PreferredColumnWidth;

            //	Raise the VerticalSplitterBeginMove event.
            OnVerticalSplitterBeginMove(EventArgs.Empty);
        }

        /// <summary>
        /// splitterLightweightControlVertical_SplitterEndMove event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void splitterLightweightControlVertical_SplitterEndMove(object sender, LightweightSplitterEventArgs e)
        {
            //	If the splitter has moved.
            if (e.Position != 0)
            {
                //	Adjust the vertical splitter position.
                AdjustVerticalLightweightSplitterEventArgsPosition(ref e);

                //	Adjust the preferred column width.
                if (verticalSplitterStyle == VerticalSplitterStyle.Left)
                    PreferredColumnWidth -= e.Position;
                else if (verticalSplitterStyle == VerticalSplitterStyle.Right)
                    PreferredColumnWidth += e.Position;
            }

            //	Raise the VerticalSplitterEndMove event.
            OnVerticalSplitterEndMove(EventArgs.Empty);
        }

        /// <summary>
        /// splitterLightweightControlVertical_SplitterMoving event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void splitterLightweightControlVertical_SplitterMoving(object sender, LightweightSplitterEventArgs e)
        {
            //	If the splitter has moved.
            if (e.Position != 0)
            {
                //	Adjust the splitter position.
                AdjustVerticalLightweightSplitterEventArgsPosition(ref e);

                //	Adjust the preferred column width - in real time.
                if (verticalSplitterStyle == VerticalSplitterStyle.Left)
                    PreferredColumnWidth -= e.Position;
                else if (verticalSplitterStyle == VerticalSplitterStyle.Right)
                    PreferredColumnWidth += e.Position;

                //	Update manually to keep the screen as up to date as possible.
                Update();
            }

            //	Raise the VerticalSplitterMoving event.
            OnVerticalSplitterMoving(EventArgs.Empty);
        }

        /// <summary>
        /// splitterLightweightControlHorizontal_SplitterBeginMove event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void splitterLightweightControlHorizontal_SplitterBeginMove(object sender, EventArgs e)
        {
            startingHorizontalSplitterPosition = HorizontalSplitterPosition;

            //	Raise the HorizontalSplitterBeginMove event.
            OnHorizontalSplitterBeginMove(EventArgs.Empty);
        }

        /// <summary>
        /// splitterLightweightControlHorizontal_SplitterEndMove event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void splitterLightweightControlHorizontal_SplitterEndMove(object sender, LightweightSplitterEventArgs e)
        {
            //	If the splitter has moved.
            if (e.Position != 0)
            {
                //	Adjust the horizontal splitter position.
                AdjustHorizontalLightweightSplitterEventArgsPosition(ref e);

                //	Adjust the horizontal splitter position.
                HorizontalSplitterPosition += (double)e.Position / PaneLayoutHeight;
            }

            //	Raise the HorizontalSplitterEndMove event.
            OnHorizontalSplitterEndMove(EventArgs.Empty);
        }

        /// <summary>
        /// splitterLightweightControlHorizontal_SplitterMoving event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void splitterLightweightControlHorizontal_SplitterMoving(object sender, LightweightSplitterEventArgs e)
        {
            //	If the splitter has moved.
            if (e.Position != 0)
            {
                AdjustHorizontalLightweightSplitterEventArgsPosition(ref e);

                //	Adjust the horizontal splitter position.
                HorizontalSplitterPosition += (double)e.Position / PaneLayoutHeight;

                //	Update manually to keep the screen as up to date as possible.
                Update();
            }

            //	Raise the HorizontalSplitterMoving event.
            OnHorizontalSplitterMoving(EventArgs.Empty);
        }

        /// <summary>
        /// workspaceColumnPaneUpper_VisibleChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void workspaceColumnPaneUpper_VisibleChanged(object sender, EventArgs e)
        {
            PerformLayout();
            Invalidate();
        }

        /// <summary>
        /// workspaceColumnPaneLower_VisibleChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void workspaceColumnPaneLower_VisibleChanged(object sender, EventArgs e)
        {
            PerformLayout();
            Invalidate();
        }

        #endregion Private Event Handler
    }
}
