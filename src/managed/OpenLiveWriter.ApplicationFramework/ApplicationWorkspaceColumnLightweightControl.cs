// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using Project31.CoreServices;
using Project31.Controls;

namespace Project31.ApplicationFramework
{
	/// <summary>
	/// The vertical splitter style.
	/// </summary>
	public enum VerticalSplitterStyle
	{
		None,	//	No vertical splitter.
		Left,	//	Vertical splitter on the left edge of the column.
		Right	//	Vertical splitter on the right edge of the column.
	}

	/// <summary>
	///
	/// </summary>
	public class ApplicationWorkspaceColumnLightweightControl : Project31.Controls.LightweightControl
	{
		/// <summary>
		/// The minimum horizontal splitter position.
		/// </summary>
		private static double MINIMUM_HORIZONTAL_SPLITTER_POSITION = 0.20;

		/// <summary>
		/// The maximum horizontal splitter position.
		/// </summary>
		private static double MAXIMUM_HORIZONTAL_SPLITTER_POSITION = 0.80;

		/// <summary>
		/// Required designer cruft.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// The vertical splitter lightweight control.
		/// </summary>
		private Project31.ApplicationFramework.SplitterLightweightControl splitterLightweightControlVertical;

		/// <summary>
		/// The horizontal splitter lightweight control.
		/// </summary>
		private Project31.ApplicationFramework.SplitterLightweightControl splitterLightweightControlHorizontal;

		/// <summary>
		/// The upper pane lightweight control.
		/// </summary>
		private LightweightControl upperPaneLightweightControl;

		/// <summary>
		/// Gets or sets the upper pane lightweight control.
		/// </summary>
		[
		Category("Design"),
		Localizable(false),
		DefaultValue(null),
		Description("Specifies the upper pane lightweight control.")
		]
		public LightweightControl UpperPaneLightweightControl
		{
			get
			{
				return upperPaneLightweightControl;
			}
			set
			{
				ChangePaneLightweightControl(value, ref upperPaneLightweightControl);
			}
		}

		/// <summary>
		/// Gets or sets the upper pane control.
		/// </summary>
		[
		Category("Design"),
		Localizable(false),
		DefaultValue(null),
		Description("Specifies the upper pane control.")
		]
		public Control UpperPaneControl
		{
			get
			{
				if (upperPaneLightweightControl is ApplicationWorkspaceColumnPaneLightweightControl)
					return ((ApplicationWorkspaceColumnPaneLightweightControl)upperPaneLightweightControl).Control;
				else
					return null;
			}
			set
			{
				ApplicationWorkspaceColumnPaneLightweightControl applicationWorkspaceColumnPaneLightweightControl = new ApplicationWorkspaceColumnPaneLightweightControl();
				applicationWorkspaceColumnPaneLightweightControl.Control = value;
				ChangePaneLightweightControl(applicationWorkspaceColumnPaneLightweightControl, ref upperPaneLightweightControl);
			}
		}

		/// <summary>
		/// The lower pane lightweight control.
		/// </summary>
		private LightweightControl lowerPaneLightweightControl;

		/// <summary>
		/// Gets or sets the lower pane lightweight control.
		/// </summary>
		[
		Category("Design"),
		Localizable(false),
		DefaultValue(null),
		Description("Specifies the lower pane lightweight control.")
		]
		public LightweightControl LowerPaneLightweightControl
		{
			get
			{
				return lowerPaneLightweightControl;
			}
			set
			{
				ChangePaneLightweightControl(value, ref lowerPaneLightweightControl);
			}
		}

		/// <summary>
		/// Gets or sets the lower pane control.
		/// </summary>
		[
		Category("Design"),
		Localizable(false),
		DefaultValue(null),
		Description("Specifies the lower pane control.")
		]
		public Control LowerPaneControl
		{
			get
			{
				if (lowerPaneLightweightControl is ApplicationWorkspaceColumnPaneLightweightControl)
					return ((ApplicationWorkspaceColumnPaneLightweightControl)lowerPaneLightweightControl).Control;
				else
					return null;
			}
			set
			{
				ApplicationWorkspaceColumnPaneLightweightControl applicationWorkspaceColumnPaneLightweightControl = new ApplicationWorkspaceColumnPaneLightweightControl();
				applicationWorkspaceColumnPaneLightweightControl.Control = value;
				ChangePaneLightweightControl(applicationWorkspaceColumnPaneLightweightControl, ref lowerPaneLightweightControl);
			}
		}

		/// <summary>
		/// The vertical splitter style.
		/// </summary>
		private VerticalSplitterStyle verticalSplitterStyle = VerticalSplitterStyle.None;

		/// <summary>
		/// Gets or sets the vertical splitter style.
		/// </summary>
		[
		Category("Appearance"),
		Localizable(false),
		DefaultValue(VerticalSplitterStyle.None),
		Description("Specifies the initial vertical splitter style.")
		]
		public VerticalSplitterStyle VerticalSplitter
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
		/// The vertical splitter width.
		/// </summary>
		private int verticalSplitterWidth = 5;

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
		/// The horizontal splitter height.
		/// </summary>
		private int horizontalSplitterHeight = 5;

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
		/// The preferred column width.
		/// </summary>
		private int preferredColumnWidth = 0;

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
		/// The minimum column width.
		/// </summary>
		private int minimumColumnWidth = 0;

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
		/// The maximum column width.
		/// </summary>
		private int maximumColumnWidth = 0;

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
		/// The horizontal splitter position.
		/// </summary>
		private double horizontalSplitterPosition = 0.60;

		/// <summary>
		/// Gets or sets the preferred horizontal splitter position.
		/// </summary>
		[
		Category("Appearance"),
		Localizable(false),
		DefaultValue(0.60),
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
				else if (value > MAXIMUM_HORIZONTAL_SPLITTER_POSITION)
					value = MAXIMUM_HORIZONTAL_SPLITTER_POSITION;

				//	If the horizontal splitter position is changing, change it.
				if (horizontalSplitterPosition != value)
				{
					horizontalSplitterPosition = value;
					OnHorizontalSplitterPositionChanged(EventArgs.Empty);
				}
			}
		}

		/// <summary>
		/// Occurs when the PreferredColumnWidth changes.
		/// </summary>
		public event EventHandler PreferredColumnWidthChanged;

		/// <summary>
		/// Occurs when the MinimumColumnWidth changes.
		/// </summary>
		public event EventHandler MinimumColumnWidthChanged;

		/// <summary>
		/// Occurs when the MaximumColumnWidth changes.
		/// </summary>
		public event EventHandler MaximumColumnWidthChanged;

		/// <summary>
		/// Occurs when the HorizontalSplitterPosition changes.
		/// </summary>
		public event EventHandler HorizontalSplitterPositionChanged;

		/// <summary>
		/// Initializes a new instance of the ApplicationWorkspaceColumnLightweightControl class.
		/// </summary>
		public ApplicationWorkspaceColumnLightweightControl()
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();
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
			this.splitterLightweightControlVertical = new Project31.ApplicationFramework.SplitterLightweightControl(this.components);
			this.splitterLightweightControlHorizontal = new Project31.ApplicationFramework.SplitterLightweightControl(this.components);
			//
			// splitterLightweightControlVertical
			//
			this.splitterLightweightControlVertical.LightweightControlContainerControl = this;
			this.splitterLightweightControlVertical.Orientation = Project31.ApplicationFramework.SplitterLightweightControl.SplitterOrientation.Vertical;
			this.splitterLightweightControlVertical.SplitterEndMove += new Project31.ApplicationFramework.LightweightSplitterEventHandler(this.splitterLightweightControlVertical_SplitterEndMove);
			this.splitterLightweightControlVertical.SplitterMoving += new Project31.ApplicationFramework.LightweightSplitterEventHandler(this.splitterLightweightControlVertical_SplitterMoving);
			//
			// splitterLightweightControlHorizontal
			//
			this.splitterLightweightControlHorizontal.LightweightControlContainerControl = this;
			this.splitterLightweightControlHorizontal.Orientation = Project31.ApplicationFramework.SplitterLightweightControl.SplitterOrientation.Horizontal;
			this.splitterLightweightControlHorizontal.SplitterEndMove += new Project31.ApplicationFramework.LightweightSplitterEventHandler(this.splitterLightweightControlHorizontal_SplitterEndMove);
			this.splitterLightweightControlHorizontal.SplitterMoving += new Project31.ApplicationFramework.LightweightSplitterEventHandler(this.splitterLightweightControlHorizontal_SplitterMoving);
		}
		#endregion

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
		/// Raises the Layout event.
		/// </summary>
		/// <param name="e">An EventArgs that contains the event data.</param>
		protected override void OnLayout(EventArgs e)
		{
			//	Call the base class's method so that registered delegates receive the event.
			base.OnLayout(e);

			//	Obtain the layout rectangle.
			Rectangle layoutRectangle = VirtualClientRectangle;

			//	Layout the vertical splitter lightweight control and adjust the layout rectangle.
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
				splitterLightweightControlVertical.VirtualBounds = new Rectangle(	layoutRectangle.X,
																					layoutRectangle.Y,
																					verticalSplitterWidth,
																					layoutRectangle.Height);
				layoutRectangle.X += verticalSplitterWidth;
				layoutRectangle.Width -= verticalSplitterWidth;
			}
			else if (verticalSplitterStyle == VerticalSplitterStyle.Right)
			{
				//	Right vertical splitter lightweight control.
				splitterLightweightControlVertical.Visible = true;
				splitterLightweightControlVertical.VirtualBounds = new Rectangle(	layoutRectangle.Right-verticalSplitterWidth,
																					layoutRectangle.Top,
																					verticalSplitterWidth,
																					layoutRectangle.Height);
				layoutRectangle.Width -= verticalSplitterWidth;
			}

			//	Layout the upper and lower pane lightweight controls.
			if (upperPaneLightweightControl != null && upperPaneLightweightControl.Visible &&
				lowerPaneLightweightControl != null && lowerPaneLightweightControl.Visible)
			{
				//	Calculate the pane layout height (the area available for layout of the upper
				//	and lower panes).
				int paneLayoutHeight = layoutRectangle.Height-horizontalSplitterHeight;

				//	If the pane layout height is too small (i.e. there isn't enough room to show
				//	both panes), err on the side of showing just the top pane.  This is an extreme
				//	edge condition.
				if (paneLayoutHeight < 10*horizontalSplitterHeight)
				{
					//	Only the upper pane lightweight control is visible.
					upperPaneLightweightControl.VirtualBounds = layoutRectangle;
					lowerPaneLightweightControl.VirtualBounds = Rectangle.Empty;
					splitterLightweightControlHorizontal.Visible = false;
					splitterLightweightControlHorizontal.VirtualBounds = Rectangle.Empty;
				}
				else
				{
					//	Get the horizontal splitter layout position.
					int horizontalSplitterLayoutPosition = HorizontalSplitterLayoutPosition;

					//	Layout the upper pane lightweight control.
					upperPaneLightweightControl.VirtualBounds = new Rectangle(	layoutRectangle.X,
																				layoutRectangle.Y,
																				layoutRectangle.Width,
																				horizontalSplitterLayoutPosition);

					//	Layout the horizontal splitter lightweight control.
					splitterLightweightControlHorizontal.Visible = true;
					splitterLightweightControlHorizontal.VirtualBounds = new Rectangle(	layoutRectangle.X,
																						upperPaneLightweightControl.VirtualBounds.Bottom,
																						layoutRectangle.Width,
																						horizontalSplitterHeight);

					//	Layout the lower pane lightweight control.
					lowerPaneLightweightControl.VirtualBounds = new Rectangle(	layoutRectangle.X,
																				splitterLightweightControlHorizontal.VirtualBounds.Bottom,
																				layoutRectangle.Width,
																				layoutRectangle.Height-(upperPaneLightweightControl.VirtualHeight+horizontalSplitterHeight));
				}
			}
			else if (upperPaneLightweightControl != null && upperPaneLightweightControl.Visible)
			{
				//	Only the upper pane lightweight control is visible.
				upperPaneLightweightControl.VirtualBounds = layoutRectangle;
			}
			else if (lowerPaneLightweightControl != null && lowerPaneLightweightControl.Visible)
			{
				//	Only the lower pane lightweight control is visible.
				lowerPaneLightweightControl.VirtualBounds = layoutRectangle;
			}
		}

		/// <summary>
		/// Private helper to change one of the pane lightweight controls.
		/// </summary>
		/// <param name="lightweightControl">The new lightweight control.</param>
		/// <param name="paneLightweightControl">The pane lightweight control to change.</param>
		private void ChangePaneLightweightControl(LightweightControl lightweightControl, ref LightweightControl paneLightweightControl)
		{
			//	If the pane lightweight control is changing, change it.
			if (paneLightweightControl != lightweightControl)
			{
				//	If we have a current lightweight control in the pane, remove it.
				if (paneLightweightControl != null)
					paneLightweightControl.LightweightControlContainerControl = null;

				//	Set the pane lightweight control.
				paneLightweightControl = lightweightControl;

				//	If we have a new lightweight control in the pane, add it.
				if (paneLightweightControl != null)
					paneLightweightControl.LightweightControlContainerControl = this;

				//	Layout and invalidate.
				PerformLayout();
				Invalidate();
			}
		}

		/// <summary>
		/// Gets the maximum width increase.
		/// </summary>
		private int MaximumWidthIncrease
		{
			get
			{
				Debug.Assert(VirtualWidth <= MaximumColumnWidth, "The column is wider than its maximum width.");
				return Math.Max(0, MaximumColumnWidth-VirtualWidth);
			}
		}

		/// <summary>
		/// Gets the maximum width decrease.
		/// </summary>
		private int MaximumWidthDecrease
		{
			get
			{
				Debug.Assert(VirtualWidth >= MinimumColumnWidth, "The column is narrower than its minimum width.");
				return Math.Max(0, VirtualWidth-MinimumColumnWidth);
			}
		}

		/// <summary>
		/// Gets the pane layout height.
		/// </summary>
		private int PaneLayoutHeight
		{
			get
			{
				return Math.Max(0, VirtualClientRectangle.Height-horizontalSplitterHeight);
			}
		}

		/// <summary>
		/// Gets the layout position of the horizontal splitter.
		/// </summary>
		private int HorizontalSplitterLayoutPosition
		{
			get
			{
				return (int)(PaneLayoutHeight*horizontalSplitterPosition);
			}
		}

		/// <summary>
		/// Gets the minimum layout position of the horizontal splitter.
		/// </summary>
		private int MinimumHorizontalSplitterLayoutPosition
		{
			get
			{
				return (int)(PaneLayoutHeight*MINIMUM_HORIZONTAL_SPLITTER_POSITION);
			}
		}

		/// <summary>
		/// Gets the maximum layout position of the horizontal splitter.
		/// </summary>
		private int MaximumHorizontalSplitterLayoutPosition
		{
			get
		{
				return (int)(PaneLayoutHeight*MAXIMUM_HORIZONTAL_SPLITTER_POSITION);
			}
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
						e.Position = MaximumWidthIncrease*-1;
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
						e.Position = MaximumWidthDecrease*-1;
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
				if (HorizontalSplitterLayoutPosition+e.Position < MinimumHorizontalSplitterLayoutPosition)
					e.Position = MinimumHorizontalSplitterLayoutPosition-horizontalSplitterLayoutPosition;
			}
			else
			{
				if (HorizontalSplitterLayoutPosition+e.Position > MaximumHorizontalSplitterLayoutPosition)
					e.Position = MaximumHorizontalSplitterLayoutPosition-horizontalSplitterLayoutPosition;
			}
		}

		/// <summary>
		/// splitterLightweightControlVertical_SplitterEndMove event handler.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void splitterLightweightControlVertical_SplitterEndMove(object sender, Project31.ApplicationFramework.LightweightSplitterEventArgs e)
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
		}

		/// <summary>
		/// splitterLightweightControlVertical_SplitterMoving event handler.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void splitterLightweightControlVertical_SplitterMoving(object sender, Project31.ApplicationFramework.LightweightSplitterEventArgs e)
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
		}

		/// <summary>
		/// splitterLightweightControlHorizontal_SplitterEndMove event handler.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void splitterLightweightControlHorizontal_SplitterEndMove(object sender, Project31.ApplicationFramework.LightweightSplitterEventArgs e)
		{
			//	If the splitter has moved.
			if (e.Position != 0)
			{
				//	Adjust the horizontal splitter position.
				AdjustHorizontalLightweightSplitterEventArgsPosition(ref e);

				//	Adjust the horizontal splitter position.
				HorizontalSplitterPosition += (double)e.Position/PaneLayoutHeight;

				//	Layout and invalidate.
				PerformLayout();
				Invalidate();
			}
		}

		/// <summary>
		/// splitterLightweightControlHorizontal_SplitterMoving event handler.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void splitterLightweightControlHorizontal_SplitterMoving(object sender, Project31.ApplicationFramework.LightweightSplitterEventArgs e)
		{
			//	If the splitter has moved.
			if (e.Position != 0)
			{
				AdjustHorizontalLightweightSplitterEventArgsPosition(ref e);

				//	Adjust the horizontal splitter position.
				HorizontalSplitterPosition += (double)e.Position/PaneLayoutHeight;

				//	Layout and invalidate.
				PerformLayout();
				Invalidate();

				//	Update manually to keep the screen as up to date as possible.
				Update();
			}
		}
	}
}

