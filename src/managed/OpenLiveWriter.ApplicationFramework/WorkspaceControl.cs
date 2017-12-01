// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using OpenLiveWriter.Controls;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// The WorkspaceControl control provides a multi-pane workspace, with an optional
    /// command bar.
    /// </summary>
    public class WorkspaceControl : LightweightControlContainerControl
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
        private IContainer components = null;

        /// <summary>
        /// The CommandManager for this WorkspaceControl.
        /// </summary>
        private CommandManager commandManager;

        /// <summary>
        /// Gets the CommandManager for this WorkspaceControl.
        /// </summary>
        public CommandManager CommandManager
        {
            get
            {
                return commandManager;
            }
        }

        /// <summary>
        /// The top layout margin.
        /// </summary>
        private int topLayoutMargin;

        /// <summary>
        /// Gets or sets the top layout margin.
        /// </summary>
        [
            Category("Appearance"),
                DefaultValue(0),
                Description("Specifies the top layout margin.")
        ]
        public int TopLayoutMargin
        {
            get
            {
                return topLayoutMargin;
            }
            set
            {
                topLayoutMargin = value;
            }
        }

        /// <summary>
        /// The left layout margin.
        /// </summary>
        private int leftLayoutMargin;

        /// <summary>
        /// Gets or sets the left layout margin.
        /// </summary>
        [
            Category("Appearance"),
                DefaultValue(0),
                Description("Specifies the left layout margin.")
        ]
        public int LeftLayoutMargin
        {
            get
            {
                return leftLayoutMargin;
            }
            set
            {
                leftLayoutMargin = value;
            }
        }

        /// <summary>
        /// The bottom layout margin.
        /// </summary>
        private int bottomLayoutMargin;

        /// <summary>
        /// Gets or sets the bottom layout margin.
        /// </summary>
        [
            Category("Appearance"),
                DefaultValue(0),
                Description("Specifies the bottom layout margin.")
        ]
        public int BottomLayoutMargin
        {
            get
            {
                return bottomLayoutMargin;
            }
            set
            {
                bottomLayoutMargin = value;
            }
        }

        /// <summary>
        /// The right layout margin.
        /// </summary>
        private int rightLayoutMargin;

        /// <summary>
        /// Gets or sets the right layout margin.
        /// </summary>
        [
            Category("Appearance"),
                DefaultValue(0),
                Description("Specifies the right layout margin.")
        ]
        public int RightLayoutMargin
        {
            get
            {
                return rightLayoutMargin;
            }
            set
            {
                rightLayoutMargin = value;
            }
        }

        /// <summary>
        ///	Gets the top color.
        /// </summary>
        public virtual Color TopColor
        {
            get
            {
                return SystemColors.Control;
            }
        }

        /// <summary>
        ///	Gets the bottom color.
        /// </summary>
        public virtual Color BottomColor
        {
            get
            {
                return SystemColors.Control;
            }
        }

        /// <summary>
        /// Gets the first command bar lightweight control.
        /// </summary>
        public virtual CommandBarLightweightControl FirstCommandBarLightweightControl
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the second command bar lightweight control.
        /// </summary>
        public virtual CommandBarLightweightControl SecondCommandBarLightweightControl
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// The left WorkspaceColumnLightweightControl.
        /// </summary>
        private WorkspaceColumnLightweightControl leftColumn;

        /// <summary>
        /// Gets the left WorkspaceColumnLightweightControl.
        /// </summary>
        public WorkspaceColumnLightweightControl LeftColumn
        {
            get
            {
                return leftColumn;
            }
        }

        /// <summary>
        /// The center WorkspaceColumnLightweightControl.
        /// </summary>
        private WorkspaceColumnLightweightControl centerColumn;

        /// <summary>
        /// Gets the center WorkspaceColumnLightweightControl.
        /// </summary>
        public WorkspaceColumnLightweightControl CenterColumn
        {
            get
            {
                return centerColumn;
            }
        }

        /// <summary>
        /// The right WorkspaceColumnLightweightControl.
        /// </summary>
        private WorkspaceColumnLightweightControl rightColumn;

        /// <summary>
        /// Gets the right WorkspaceColumnLightweightControl.
        /// </summary>
        public WorkspaceColumnLightweightControl RightColumn
        {
            get
            {
                return rightColumn;
            }
        }

        /// <summary>
        /// Initializes a new instance of the WorkspaceControl class.
        /// </summary>
        public WorkspaceControl(CommandManager commandManager)
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            //	Set the CommandManager.
            this.commandManager = commandManager;

            //	Turn on double buffered painting.
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
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

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.leftColumn = new OpenLiveWriter.ApplicationFramework.WorkspaceColumnLightweightControl(this.components);
            this.centerColumn = new OpenLiveWriter.ApplicationFramework.WorkspaceColumnLightweightControl(this.components);
            this.rightColumn = new OpenLiveWriter.ApplicationFramework.WorkspaceColumnLightweightControl(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.leftColumn)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.centerColumn)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rightColumn)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            //
            // leftColumn
            //
            this.leftColumn.HorizontalSplitterHeight = 4;
            this.leftColumn.LightweightControlContainerControl = this;
            this.leftColumn.MinimumColumnWidth = 30;
            this.leftColumn.PreferredColumnWidth = 190;
            this.leftColumn.VerticalSplitterWidth = 4;
            this.leftColumn.MaximumColumnWidthChanged += new System.EventHandler(this.leftColumn_MaximumColumnWidthChanged);
            this.leftColumn.PreferredColumnWidthChanged += new System.EventHandler(this.leftColumn_PreferredColumnWidthChanged);
            this.leftColumn.MinimumColumnWidthChanged += new System.EventHandler(this.leftColumn_MinimumColumnWidthChanged);
            //
            // centerColumn
            //
            this.centerColumn.HorizontalSplitterHeight = 4;
            this.centerColumn.LightweightControlContainerControl = this;
            this.centerColumn.MinimumColumnWidth = 30;
            this.centerColumn.PreferredColumnWidth = 30;
            this.centerColumn.VerticalSplitterWidth = 4;
            //
            // rightColumn
            //
            this.rightColumn.HorizontalSplitterHeight = 4;
            this.rightColumn.LightweightControlContainerControl = this;
            this.rightColumn.MinimumColumnWidth = 30;
            this.rightColumn.PreferredColumnWidth = 150;
            this.rightColumn.VerticalSplitterWidth = 4;
            this.rightColumn.MaximumColumnWidthChanged += new System.EventHandler(this.rightColumn_MaximumColumnWidthChanged);
            this.rightColumn.PreferredColumnWidthChanged += new System.EventHandler(this.rightColumn_PreferredColumnWidthChanged);
            this.rightColumn.MinimumColumnWidthChanged += new System.EventHandler(this.rightColumn_MinimumColumnWidthChanged);
            //
            // WorkspaceControl
            //
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Name = "WorkspaceControl";
            this.Size = new System.Drawing.Size(294, 286);
            ((System.ComponentModel.ISupportInitialize)(this.leftColumn)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.centerColumn)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rightColumn)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }
        #endregion

        /// <summary>
        /// Raises the DragEnter event.
        /// </summary>
        /// <param name="e">A DragEventArgs that contains the event data.</param>
        protected override void OnDragEnter(DragEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnDragEnter(e);

            //	None.
            e.Effect = DragDropEffects.None;
        }

        /// <summary>
        /// Raises the Layout event.
        /// </summary>
        /// <param name="e">A LayoutEventArgs that contains the event data.</param>
        protected override void OnLayout(LayoutEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLayout(e);

            //	If we have no parent, no layout is required.
            if (Parent == null)
                return;

            //	Layout the command bar lightweight controls.  The return is the column layout rectangle.
            Rectangle columnLayoutRectangle = LayoutCommandBars();

            //	Set the initial (to be adjusted below) column widths.
            int leftColumnWidth = LeftColumnPreferredWidth;
            int centerColumnWidth = CenterColumnMinimumWidth;
            int rightColumnWidth = RightColumnPreferredWidth;

            //	Adjust the column widths as needed for the layout width.
            if (leftColumnWidth + centerColumnWidth + rightColumnWidth > columnLayoutRectangle.Width)
            {
                //	Calculate the width that is available to the left and right columns.
                int availableWidth = columnLayoutRectangle.Width - centerColumnWidth;

                //	Adjust the left and right column widths.
                if (LeftColumnVisible && RightColumnVisible)
                {
                    //	Calculate the relative width of the left column.
                    double leftColumnRelativeWidth = ((double)leftColumnWidth) / (leftColumnWidth + rightColumnWidth);

                    //	Adjust the left and right column widths.
                    leftColumnWidth = Math.Max((int)(leftColumnRelativeWidth * availableWidth), LeftColumnMinimumWidth);
                    rightColumnWidth = Math.Max(availableWidth - leftColumnWidth, RightColumnMinimumWidth);
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
                    centerColumnWidth = columnLayoutRectangle.Width - (leftColumnWidth + rightColumnWidth);
                else
                    leftColumnWidth = columnLayoutRectangle.Width - rightColumnWidth;
            }

            //	Set the layout X offset.
            int layoutX = columnLayoutRectangle.X;

            //	Layout the left column, if it is visible.
            if (LeftColumnVisible)
            {
                //	Set the virtual bounds of the left column.
                leftColumn.VirtualBounds = new Rectangle(layoutX,
                                                            columnLayoutRectangle.Y,
                                                            leftColumnWidth,
                                                            columnLayoutRectangle.Height);
                leftColumn.PerformLayout();

                //	Adjust the layout X to account for the left column.
                layoutX += leftColumnWidth;

                //	Update the left column vertical splitter and maximum column width.
                if (CenterColumnVisible)
                {
                    //	Turn on the left column vertical splitter on the right side.
                    leftColumn.VerticalSplitterStyle = VerticalSplitterStyle.Right;

                    //	Set the left column's maximum width.
                    leftColumn.MaximumColumnWidth = columnLayoutRectangle.Width -
                                                    (CenterColumnMinimumWidth + this.RightColumnPreferredWidth);
                }
                else
                {
                    leftColumn.VerticalSplitterStyle = VerticalSplitterStyle.None;
                    leftColumn.MaximumColumnWidth = 0;
                }
            }

            //	Layout the center column.
            if (CenterColumnVisible)
            {
                //	Set the virtual bounds of the center column.
                centerColumn.VirtualBounds = new Rectangle(layoutX,
                                                            columnLayoutRectangle.Y,
                                                            centerColumnWidth,
                                                            columnLayoutRectangle.Height);
                centerColumn.PerformLayout();

                //	Adjust the layout X to account for the center column.
                layoutX += centerColumnWidth;

                //	The center column never has a vertical splitter or a maximum column width.
                centerColumn.VerticalSplitterStyle = VerticalSplitterStyle.None;
                centerColumn.MaximumColumnWidth = 0;
            }

            //	Layout the right column.
            if (RightColumnVisible)
            {
                //	Set the virtual bounds of the right column.
                rightColumn.VirtualBounds = new Rectangle(layoutX,
                                                            columnLayoutRectangle.Y,
                                                            rightColumnWidth,
                                                            columnLayoutRectangle.Height);
                rightColumn.PerformLayout();

                //	Update the right column vertical splitter and maximum column width.
                if (CenterColumnVisible || LeftColumnVisible)
                {
                    //	Turn on the right column's vertical splitter on the left side.
                    rightColumn.VerticalSplitterStyle = VerticalSplitterStyle.Left;

                    //	Set the right column's maximum width.
                    rightColumn.MaximumColumnWidth = columnLayoutRectangle.Width -
                                                        (LeftColumnPreferredWidth + CenterColumnMinimumWidth);
                }
                else
                {
                    rightColumn.VerticalSplitterStyle = VerticalSplitterStyle.None;
                    rightColumn.MaximumColumnWidth = 0;
                }
            }
        }

        /// <summary>
        /// Override background painting.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //	Fill the background.
            if (TopColor == BottomColor)
                using (SolidBrush solidBrush = new SolidBrush(TopColor))
                    e.Graphics.FillRectangle(solidBrush, ClientRectangle);
            else
                using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(ClientRectangle, TopColor, BottomColor, LinearGradientMode.Vertical))
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
        /// Layout the command bars.
        /// </summary>
        /// <returns>Column layout rectangle.</returns>
        private Rectangle LayoutCommandBars()
        {
            //	The command bar area height (set below).
            int commandBarAreaHeight = 0;

            //	If we have a "first" command bar lightweight control, lay it out.
            if (FirstCommandBarLightweightControl != null)
            {
                //	If a command bar definition has been supplied, layout the "first" command bar
                //	lightweight control. Otherwise, hide it.
                if (FirstCommandBarLightweightControl.CommandBarDefinition != null)
                {
                    //	Make the command bar visible before getting height.
                    FirstCommandBarLightweightControl.Visible = true;

                    //	Obtain the height and lay it out.
                    int firstCommandBarHeight = FirstCommandBarLightweightControl.DefaultVirtualSize.Height;
                    FirstCommandBarLightweightControl.VirtualBounds = new Rectangle(0, commandBarAreaHeight, Width, firstCommandBarHeight);

                    //	Increment the command bar area height.
                    commandBarAreaHeight += firstCommandBarHeight;
                }
                else
                {
                    //	Hide the command bar lightweight control.
                    FirstCommandBarLightweightControl.Visible = false;
                    FirstCommandBarLightweightControl.VirtualBounds = Rectangle.Empty;
                }
            }

            //	If we have a "second" command bar lightweight control, lay it out.
            if (SecondCommandBarLightweightControl != null)
            {
                //	If a command bar definition has been supplied, layout the "second" command bar
                //	lightweight control. Otherwise, hide it.
                if (SecondCommandBarLightweightControl.CommandBarDefinition != null)
                {
                    //	Make the command bar visible before getting height.
                    SecondCommandBarLightweightControl.Visible = true;

                    //	Obtain the height and lay it out.
                    int secondCommandBarHeight = SecondCommandBarLightweightControl.DefaultVirtualSize.Height;
                    SecondCommandBarLightweightControl.VirtualBounds = new Rectangle(0, commandBarAreaHeight, Width, secondCommandBarHeight);

                    //	Increment the command bar area height.
                    commandBarAreaHeight += secondCommandBarHeight;
                }
                else
                {
                    //	Hide the command bar lightweight control.
                    SecondCommandBarLightweightControl.Visible = false;
                    SecondCommandBarLightweightControl.VirtualBounds = Rectangle.Empty;
                }
            }

            //	Return the column layout rectangle.
            return new Rectangle(leftLayoutMargin,
                                    commandBarAreaHeight + topLayoutMargin,
                                    Width - (leftLayoutMargin + rightLayoutMargin),
                                    Height - commandBarAreaHeight - (topLayoutMargin + bottomLayoutMargin));
        }

        /// <summary>
        /// leftColumn_MaximumColumnWidthChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void leftColumn_MaximumColumnWidthChanged(object sender, EventArgs e)
        {
            PerformLayout();
            Invalidate();
        }

        /// <summary>
        /// leftColumn_MinimumColumnWidthChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void leftColumn_MinimumColumnWidthChanged(object sender, EventArgs e)
        {
            PerformLayout();
            Invalidate();
        }

        /// <summary>
        /// leftColumn_PreferredColumnWidthChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void leftColumn_PreferredColumnWidthChanged(object sender, EventArgs e)
        {
            PerformLayout();
            Invalidate();
        }

        /// <summary>
        /// rightColumn_MaximumColumnWidthChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void rightColumn_MaximumColumnWidthChanged(object sender, EventArgs e)
        {
            PerformLayout();
            Invalidate();
        }

        /// <summary>
        /// rightColumn_MinimumColumnWidthChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void rightColumn_MinimumColumnWidthChanged(object sender, EventArgs e)
        {
            PerformLayout();
            Invalidate();
        }

        /// <summary>
        /// rightColumn_PreferredColumnWidthChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void rightColumn_PreferredColumnWidthChanged(object sender, EventArgs e)
        {
            PerformLayout();
            Invalidate();
        }
    }
}

