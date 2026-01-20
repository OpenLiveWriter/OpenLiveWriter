// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Diagnostics;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// The ApplicationWorkspace control provides a multi-pane workspace.
    /// </summary>
    public class ApplicationWorkspace : OpenLiveWriter.Controls.LightweightControlContainerControl
    {
        /// <summary>
        /// The default left column preferred width.
        /// </summary>
        private const int LEFT_COLUMN_DEFAULT_PREFERRED_WIDTH = 150;

        /// <summary>
        /// The default center column preferred width.
        /// </summary>
        private const int CENTER_COLUMN_DEFAULT_PREFERRED_WIDTH = 20;

        /// <summary>
        /// The default right column preferred width.
        /// </summary>
        private const int RIGHT_COLUMN_DEFAULT_PREFERRED_WIDTH = 150;

        /// <summary>
        /// The default left column minimum width.
        /// </summary>
        private const int LEFT_COLUMN_DEFAULT_MINIMUM_WIDTH = 10;

        /// <summary>
        /// The default center column minimum width.
        /// </summary>
        private const int CENTER_COLUMN_DEFAULT_MINIMUM_WIDTH = 10;

        /// <summary>
        /// The default right column minimum width.
        /// </summary>
        private const int RIGHT_COLUMN_DEFAULT_MINIMUM_WIDTH = 10;

        /// <summary>
        /// Required designer cruft.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// The column layout margin.
        /// </summary>
        private Size columnLayoutMargin = new Size(5, 5);

        /// <summary>
        /// Gets or sets the column layout margin.
        /// </summary>
        [
        Category("Appearance"),
        DefaultValue(typeof(Size), "5, 5"),
        Description("Specifies the column layout margin.")
        ]
        public Size ColumnLayoutMargin
        {
            get
            {
                return columnLayoutMargin;
            }
            set
            {
                columnLayoutMargin = value;
            }
        }

        /// <summary>
        /// Gets or sets the command bar definition.
        /// </summary>
        public CommandBarDefinition CommandBarDefinition
        {
            get
            {
                return applicationCommandBarLightweightControl.CommandBarDefinition;
            }
            set
            {
                applicationCommandBarLightweightControl.CommandBarDefinition = value;
                PerformLayout();
                Invalidate();
            }
        }

        /// <summary>
        /// The application workspace command bar lightweight control.
        /// </summary>
        private OpenLiveWriter.ApplicationFramework.ApplicationWorkspaceCommandBarLightweightControl applicationCommandBarLightweightControl;

        /// <summary>
        /// The left ApplicationWorkspaceColumnLightweightControl.
        /// </summary>
        private OpenLiveWriter.ApplicationFramework.ApplicationWorkspaceColumnLightweightControl leftColumn;

        /// <summary>
        /// Gets the left ApplicationWorkspaceColumnLightweightControl.
        /// </summary>
        public ApplicationWorkspaceColumnLightweightControl LeftColumn
        {
            get
            {
                return leftColumn;
            }
        }

        /// <summary>
        /// The center ApplicationWorkspaceColumnLightweightControl.
        /// </summary>
        private OpenLiveWriter.ApplicationFramework.ApplicationWorkspaceColumnLightweightControl centerColumn;

        /// <summary>
        /// Gets the center ApplicationWorkspaceColumnLightweightControl.
        /// </summary>
        public ApplicationWorkspaceColumnLightweightControl CenterColumn
        {
            get
            {
                return centerColumn;
            }
        }

        /// <summary>
        /// The right ApplicationWorkspaceColumnLightweightControl.
        /// </summary>
        private OpenLiveWriter.ApplicationFramework.ApplicationWorkspaceColumnLightweightControl rightColumn;

        /// <summary>
        /// Gets the right ApplicationWorkspaceColumnLightweightControl.
        /// </summary>
        public ApplicationWorkspaceColumnLightweightControl RightColumn
        {
            get
            {
                return rightColumn;
            }
        }

        /// <summary>
        /// Initializes a new instance of the ApplicationWorkspace class.
        /// </summary>
        public ApplicationWorkspace()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            //	Turn on double buffered painting.
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.applicationCommandBarLightweightControl = new OpenLiveWriter.ApplicationFramework.ApplicationWorkspaceCommandBarLightweightControl(this.components);
            this.leftColumn = new OpenLiveWriter.ApplicationFramework.ApplicationWorkspaceColumnLightweightControl();
            this.centerColumn = new OpenLiveWriter.ApplicationFramework.ApplicationWorkspaceColumnLightweightControl();
            this.rightColumn = new OpenLiveWriter.ApplicationFramework.ApplicationWorkspaceColumnLightweightControl();
            ((System.ComponentModel.ISupportInitialize)(this.applicationCommandBarLightweightControl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.leftColumn)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.centerColumn)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rightColumn)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            //
            // applicationCommandBarLightweightControl
            //
            this.applicationCommandBarLightweightControl.LayoutMargin = new System.Drawing.Size(2, 2);
            this.applicationCommandBarLightweightControl.LightweightControlContainerControl = this;
            this.applicationCommandBarLightweightControl.Visible = false;
            //
            // leftColumn
            //
            this.leftColumn.LightweightControlContainerControl = this;
            this.leftColumn.MinimumColumnWidth = 30;
            this.leftColumn.PreferredColumnWidth = 150;
            this.leftColumn.MaximumColumnWidthChanged += new System.EventHandler(this.leftColumn_MaximumColumnWidthChanged);
            this.leftColumn.PreferredColumnWidthChanged += new System.EventHandler(this.leftColumn_PreferredColumnWidthChanged);
            this.leftColumn.MinimumColumnWidthChanged += new System.EventHandler(this.leftColumn_MinimumColumnWidthChanged);
            //
            // centerColumn
            //
            this.centerColumn.LightweightControlContainerControl = this;
            this.centerColumn.MinimumColumnWidth = 30;
            this.centerColumn.PreferredColumnWidth = 30;
            //
            // rightColumn
            //
            this.rightColumn.LightweightControlContainerControl = this;
            this.rightColumn.MinimumColumnWidth = 30;
            this.rightColumn.PreferredColumnWidth = 150;
            this.rightColumn.MaximumColumnWidthChanged += new System.EventHandler(this.rightColumn_MaximumColumnWidthChanged);
            this.rightColumn.PreferredColumnWidthChanged += new System.EventHandler(this.rightColumn_PreferredColumnWidthChanged);
            this.rightColumn.MinimumColumnWidthChanged += new System.EventHandler(this.rightColumn_MinimumColumnWidthChanged);
            //
            // ApplicationWorkspace
            //
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Name = "ApplicationWorkspace";
            this.Size = new System.Drawing.Size(294, 286);
            ((System.ComponentModel.ISupportInitialize)(this.applicationCommandBarLightweightControl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.leftColumn)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.centerColumn)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rightColumn)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }
        #endregion

        /// <summary>
        /// Raises the Layout event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnLayout(LayoutEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLayout(e);

            //	Layout the application command bar lightweight control.  The return is the column
            //	layout rectangle.
            Rectangle columnLayoutRectangle = LayoutApplicationCommandBarLightweightControl();

            //	Set the initial (to be adjusted below) column widths.
            int leftColumnWidth = LeftColumnPreferredWidth;
            int centerColumnWidth = CenterColumnMinimumWidth;
            int rightColumnWidth = RightColumnPreferredWidth;

            //	Adjust the column widths as needed for the layout width.
            if (leftColumnWidth+centerColumnWidth+rightColumnWidth > columnLayoutRectangle.Width)
            {
                //	Calculate the width that is available to the left and right columns.
                int availableWidth = columnLayoutRectangle.Width-centerColumnWidth;

                //	Adjust the left and right column widths.
                if (LeftColumnVisible && RightColumnVisible)
                {
                    //	Calculate the relative width of the left column.
                    double leftColumnRelativeWidth = ((double)leftColumnWidth)/(leftColumnWidth+rightColumnWidth);

                    //	Adjust the left and right column widths.
                    leftColumnWidth = Math.Max((int)(leftColumnRelativeWidth*availableWidth), LeftColumnMinimumWidth);
                    rightColumnWidth = Math.Max(availableWidth-leftColumnWidth, RightColumnMinimumWidth);
                }
                else if (LeftColumnVisible)
                {
                    //	Only the left column is visible, so it gets all the available width.
                    leftColumnWidth = Math.Max(availableWidth, LeftColumnMinimumWidth);
                }
                else if (RightColumnVisible)
                {
                    //	Only the right column is visible, so it gets all the available width.
                    rightColumnWidth = Math.Max(availableWidth, RightColumnMinimumWidth);
                }
            }
            else
            {
                //	We have a surplus of room.  Allocate additional space to the center column, if
                //	if is visible, or the left column if it is not.
                if (CenterColumnVisible)
                    centerColumnWidth = columnLayoutRectangle.Width-(leftColumnWidth+rightColumnWidth);
                else
                    leftColumnWidth = columnLayoutRectangle.Width-rightColumnWidth;
            }

            //	Set the layout X offset.
            int layoutX = columnLayoutRectangle.X;

            //	Layout the left column, if it is visible.
            if (LeftColumnVisible)
            {
                //	Set the virtual bounds of the left column.
                leftColumn.VirtualBounds = new Rectangle(	layoutX,
                                                            columnLayoutRectangle.Y,
                                                            leftColumnWidth,
                                                            columnLayoutRectangle.Height);

                //	Adjust the layout X to account for the left column.
                layoutX += leftColumnWidth;

                //	Update the left column vertical splitter and maximum column width.
                if (CenterColumnVisible)
                {
                    //	Turn on the left column vertical splitter on the right side.
                    leftColumn.VerticalSplitter = VerticalSplitterStyle.Right;

                    //	Set the left column's maximum width.
                    leftColumn.MaximumColumnWidth = columnLayoutRectangle.Width-
                                                    (CenterColumnMinimumWidth+this.RightColumnPreferredWidth);
                }
                else
                {
                    leftColumn.VerticalSplitter = VerticalSplitterStyle.None;
                    leftColumn.MaximumColumnWidth = 0;
                }
            }

            //	Layout the center column.
            if (CenterColumnVisible)
            {
                //	Set the virtual bounds of the center column.
                centerColumn.VirtualBounds = new Rectangle(	layoutX,
                                                            columnLayoutRectangle.Y,
                                                            centerColumnWidth,
                                                            columnLayoutRectangle.Height);

                //	Adjust the layout X to account for the center column.
                layoutX += centerColumnWidth;

                //	The center column never has a vertical splitter or a maximum column width.
                centerColumn.VerticalSplitter = VerticalSplitterStyle.None;
                centerColumn.MaximumColumnWidth = 0;
            }

            //	Layout the right column.
            if (RightColumnVisible)
            {
                //	Set the virtual bounds of the right column.
                rightColumn.VirtualBounds = new Rectangle(	layoutX,
                                                            columnLayoutRectangle.Y,
                                                            rightColumnWidth,
                                                            columnLayoutRectangle.Height);

                //	Update the right column vertical splitter and maximum column width.
                if (CenterColumnVisible || LeftColumnVisible)
                {
                    //	Turn on the right column's vertical splitter on the left side.
                    rightColumn.VerticalSplitter = VerticalSplitterStyle.Left;

                    //	Set the right column's maximum width.
                    rightColumn.MaximumColumnWidth =	columnLayoutRectangle.Width-
                                                        (LeftColumnPreferredWidth+CenterColumnMinimumWidth);
                }
                else
                {
                    rightColumn.VerticalSplitter = VerticalSplitterStyle.None;
                    rightColumn.MaximumColumnWidth = 0;
                }
            }
        }

        /// <summary>
        /// Override background painting.
        /// </summary>
        /// <param name="e">Event parameters.</param>
        protected override void OnPaintBackground(System.Windows.Forms.PaintEventArgs e)
        {
            //	Fill the background.
            using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(ClientRectangle, ApplicationManager.ApplicationStyle.ApplicationWorkspaceTopColor,  ApplicationManager.ApplicationStyle.ApplicationWorkspaceBottomColor, LinearGradientMode.ForwardDiagonal))
                e.Graphics.FillRectangle(linearGradientBrush, ClientRectangle);
        }

        /// <summary>
        /// Gets a value indicating whether the left column is visible.
        /// </summary>
        private bool LeftColumnVisible
        {
            get
            {
                return leftColumn != null && leftColumn.Visible;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the center column is visible.
        /// </summary>
        private bool CenterColumnVisible
        {
            get
            {
                return centerColumn != null && centerColumn.Visible;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the right column is visible.
        /// </summary>
        private bool RightColumnVisible
        {
            get
            {
                return rightColumn != null && rightColumn.Visible;
            }
        }

        /// <summary>
        /// Gets the preferred width of the left column.
        /// </summary>
        private int LeftColumnPreferredWidth
        {
            get
            {
                if (!LeftColumnVisible)
                    return 0;
                else
                {
                    if (leftColumn.PreferredColumnWidth == 0)
                        leftColumn.PreferredColumnWidth = LEFT_COLUMN_DEFAULT_PREFERRED_WIDTH;
                    return leftColumn.PreferredColumnWidth;
                }
            }
        }

        /// <summary>
        /// Gets the preferred width of the center column.
        /// </summary>
        private int CenterColumnPreferredWidth
        {
            get
            {
                if (!CenterColumnVisible)
                    return 0;
                else
                {
                    if (centerColumn.PreferredColumnWidth == 0)
                        centerColumn.PreferredColumnWidth = CENTER_COLUMN_DEFAULT_PREFERRED_WIDTH;
                    return centerColumn.PreferredColumnWidth;
                }
            }
        }

        /// <summary>
        /// Gets the preferred width of the right column.
        /// </summary>
        private int RightColumnPreferredWidth
        {
            get
            {
                if (!RightColumnVisible)
                    return 0;
                else
                {
                    if (rightColumn.PreferredColumnWidth == 0)
                        rightColumn.PreferredColumnWidth = RIGHT_COLUMN_DEFAULT_PREFERRED_WIDTH;
                    return rightColumn.PreferredColumnWidth;
                }
            }
        }

        /// <summary>
        /// Gets the minimum width of the left column.
        /// </summary>
        private int LeftColumnMinimumWidth
        {
            get
            {
                if (!LeftColumnVisible)
                    return 0;
                else
                {
                    if (leftColumn.MinimumColumnWidth == 0)
                        leftColumn.MinimumColumnWidth = LEFT_COLUMN_DEFAULT_MINIMUM_WIDTH;
                    return leftColumn.MinimumColumnWidth;
                }
            }
        }

        /// <summary>
        /// Gets the minimum width of the center column.
        /// </summary>
        private int CenterColumnMinimumWidth
        {
            get
            {
                if (!CenterColumnVisible)
                    return 0;
                else
                {
                    if (centerColumn.MinimumColumnWidth == 0)
                        centerColumn.MinimumColumnWidth = CENTER_COLUMN_DEFAULT_MINIMUM_WIDTH;
                    return centerColumn.MinimumColumnWidth;
                }
            }
        }

        /// <summary>
        /// Gets the minimum width of the right column.
        /// </summary>
        private int RightColumnMinimumWidth
        {
            get
            {
                if (!RightColumnVisible)
                    return 0;
                else
                {
                    if (rightColumn.MinimumColumnWidth == 0)
                        rightColumn.MinimumColumnWidth = RIGHT_COLUMN_DEFAULT_MINIMUM_WIDTH;
                    return rightColumn.MinimumColumnWidth;
                }
            }
        }

        /// <summary>
        /// Layout the application command bar lightweight control.
        /// </summary>
        /// <returns>Column layout rectangle.</returns>
        private Rectangle LayoutApplicationCommandBarLightweightControl()
        {
            //	The command bar height (set below if ).
            int applicationCommandBarHeight = 0;

            //	If we have am application command bar lightweight control, lay it out.
            if (applicationCommandBarLightweightControl != null)
            {
                //	If a command bar definition has been supplied, layout the application command
                //	bar lightweight control. Otherwise, hide it.
                if (CommandBarDefinition != null)
                {
                    //	Set the application command bar height.
                    applicationCommandBarHeight = applicationCommandBarLightweightControl.DefaultVirtualSize.Height;

                    //	Layout the application command bar lightweight control.
                    applicationCommandBarLightweightControl.Visible = true;
                    applicationCommandBarLightweightControl.VirtualBounds = new Rectangle(0, 0, Width, applicationCommandBarHeight);
                }
                else
                {
                    //	Layout the application command bar lightweight control.
                    applicationCommandBarLightweightControl.Visible = false;
                    applicationCommandBarLightweightControl.VirtualBounds = Rectangle.Empty;
                }
            }

            //	Return the column layout rectangle.
            return new Rectangle(	columnLayoutMargin.Width,
                                    applicationCommandBarHeight+columnLayoutMargin.Height,
                                    Width-(columnLayoutMargin.Width*2),
                                    Height-applicationCommandBarHeight-(columnLayoutMargin.Height*2));
        }

        /// <summary>
        /// leftColumn_MaximumColumnWidthChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void leftColumn_MaximumColumnWidthChanged(object sender, System.EventArgs e)
        {
            PerformLayout();
            Invalidate();
        }

        /// <summary>
        /// leftColumn_MinimumColumnWidthChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void leftColumn_MinimumColumnWidthChanged(object sender, System.EventArgs e)
        {
            PerformLayout();
            Invalidate();
        }

        /// <summary>
        /// leftColumn_PreferredColumnWidthChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void leftColumn_PreferredColumnWidthChanged(object sender, System.EventArgs e)
        {
            PerformLayout();
            Invalidate();
        }

        /// <summary>
        /// rightColumn_MaximumColumnWidthChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void rightColumn_MaximumColumnWidthChanged(object sender, System.EventArgs e)
        {
            PerformLayout();
            Invalidate();
        }

        /// <summary>
        /// rightColumn_MinimumColumnWidthChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void rightColumn_MinimumColumnWidthChanged(object sender, System.EventArgs e)
        {
            PerformLayout();
            Invalidate();
        }

        /// <summary>
        /// rightColumn_PreferredColumnWidthChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void rightColumn_PreferredColumnWidthChanged(object sender, System.EventArgs e)
        {
            PerformLayout();
            Invalidate();
        }
    }
}

