// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Provides a control which is useful for providing a border for, or replacing the border of,
    /// another control.
    /// </summary>
    public class BorderControl : UserControl
    {
        #region Private Member Variables

        /// <summary>
        /// The border size.
        /// </summary>
        private const int BORDER_SIZE = 1;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        /// <summary>
        /// The theme border color color.
        /// </summary>
        private Color themeBorderColor;

        /// <summary>
        /// The FocusWatchingUserControl that contains the control.  This allows us to hide borders
        /// or add borders to any control.
        /// </summary>
        private FocusWatchingUserControl focusWatchingUserControl;

        /// <summary>
        /// The control that this BorderControl is providing a border for.
        /// </summary>
        private Control control;

        /// <summary>
        ///
        /// </summary>
        private bool themeBorder = false;

        /// <summary>
        /// A value indicating whether the height of the BorderControl is automatically determined
        /// based on the height of the control.
        /// </summary>
        private bool autoHeight = false;

        /// <summary>
        /// The top inset.
        /// </summary>
        private int topInset;

        /// <summary>
        /// The left inset.
        /// </summary>
        private int leftInset;

        /// <summary>
        /// The bottom inset.
        /// </summary>
        private int bottomInset;

        /// <summary>
        /// True if bottom border should not be used.
        /// </summary>
        private bool suppressBottomBorder;

        /// <summary>
        /// The right inset.
        /// </summary>
        private int rightInset;

        #endregion Private Member Variables

        #region Class Initialization & Termination

        /// <summary>
        /// Initializes a new instance of the BorderObscuringControl class.
        /// </summary>
        public BorderControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            //	Set the theme border color.
            themeBorderColor = SystemColors.ControlDark;
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

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.focusWatchingUserControl = new OpenLiveWriter.Controls.FocusWatchingUserControl();
            this.SuspendLayout();
            //
            // focusWatchingUserControl
            //
            this.focusWatchingUserControl.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.focusWatchingUserControl.BackColor = System.Drawing.SystemColors.Window;
            this.focusWatchingUserControl.Location = new System.Drawing.Point(1, 1);
            this.focusWatchingUserControl.Name = "focusWatchingUserControl";
            this.focusWatchingUserControl.Size = new System.Drawing.Size(148, 148);
            this.focusWatchingUserControl.TabStop = false;
            //this.focusWatchingUserControl.TabIndex = 0;
            //
            // BorderControl
            //
            this.Controls.Add(this.focusWatchingUserControl);
            this.Name = "BorderControl";
            this.ResumeLayout(false);

        }
        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets
        /// </summary>
        public Control Control
        {
            get
            {
                return control;
            }
            set
            {
                if (control != value)
                {
                    if (control != null)
                    {
                        control.EnabledChanged -= new EventHandler(control_EnabledChanged);
                        control.Parent = null;
                    }

                    control = value;

                    if (control != null)
                    {
                        control.EnabledChanged += new EventHandler(control_EnabledChanged);
                        control.Parent = focusWatchingUserControl;
                        focusWatchingUserControl.BackColor = control.Enabled ? control.BackColor : SystemColors.Control;
                    }
                }
            }
        }

        /// <summary>
        /// </summary>
        public bool ThemeBorder
        {
            get
            {
                return themeBorder;
            }
            set
            {
                if (themeBorder != value)
                {
                    themeBorder = value;
                    PerformLayout();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the height of the BorderControl is
        /// automatically determined based on the height of the control.
        /// </summary>
        public bool AutoHeight
        {
            get
            {
                return autoHeight;
            }
            set
            {
                autoHeight = value;
            }
        }

        /// <summary>
        /// Gets or sets the top inset.
        /// </summary>
        public int TopInset
        {
            get
            {
                return topInset;
            }
            set
            {
                if (topInset != value)
                {
                    topInset = value;
                    PerformLayout();
                }
            }
        }

        /// <summary>
        /// Gets or sets the left inset.
        /// </summary>
        public int LeftInset
        {
            get
            {
                return leftInset;
            }
            set
            {
                if (leftInset != value)
                {
                    leftInset = value;
                    PerformLayout();
                }
            }
        }

        /// <summary>
        /// Gets or sets the bottom inset.
        /// </summary>
        public int BottomInset
        {
            get
            {
                return bottomInset;
            }
            set
            {
                if (bottomInset != value)
                {
                    bottomInset = value;
                    PerformLayout();
                }
            }
        }

        public bool SuppressBottomBorder
        {
            get
            {
                return suppressBottomBorder;
            }
            set
            {
                if (suppressBottomBorder != value)
                {
                    suppressBottomBorder = value;
                    PerformLayout();
                }
            }
        }

        /// <summary>
        /// Gets or sets the right inset.
        /// </summary>
        public int RightInset
        {
            get
            {
                return rightInset;
            }
            set
            {
                if (rightInset != value)
                {
                    rightInset = value;
                    PerformLayout();
                }
            }
        }

        /// <summary>
        /// Gets or sets the background color for the control.
        /// </summary>
        public override Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                base.BackColor = value;
                focusWatchingUserControl.BackColor = BackColor;
            }
        }

        #endregion Public Properties

        #region Protected Methods

        /// <summary>
        /// Performs the work of setting the specified bounds of this control.
        /// </summary>
        /// <param name="x">The new Left property value of the control.</param>
        /// <param name="y">The new Right property value of the control.</param>
        /// <param name="width">The new Width property value of the control.</param>
        /// <param name="height">The new Height property value of the control.</param>
        /// <param name="specified">A bitwise combination of the BoundsSpecified values.</param>
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            //	If this is an auto-height BorderControl, and we have a control, constrain the height
            //	of the BorderControl based on the height of the control.
            if (autoHeight && control != null)
                height = control.Size.Height + (topInset + bottomInset) + (BORDER_SIZE * 2);

            //	Call the base class's method.
            base.SetBoundsCore(x, y, width, height, specified);
        }

        #endregion Protected Methods

        #region Protected Event Overrides

        /// <summary>
        /// Raises the SystemColorsChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnSystemColorsChanged(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnSystemColorsChanged(e);

            //	Obtain the theme border color again.
            themeBorderColor = ColorHelper.GetThemeBorderColor(SystemColors.ControlDark);

            //	Invalidate.
            Invalidate();
        }

        /// <summary>
        /// Raises the Layout event.
        /// </summary>
        /// <param name="e">A LayoutEventArgs that contains the event data.</param>
        protected override void OnLayout(LayoutEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLayout(e);

            //	Layout the focusWatchingUserControl.
            if (themeBorder)
                focusWatchingUserControl.Bounds = new Rectangle(1, 1, Width - 2, suppressBottomBorder ? Height - 1 : Height - 2);
            else
                focusWatchingUserControl.Bounds = new Rectangle(2, 2, Width - 4, suppressBottomBorder ? Height - 2 : Height - 4);

            //	Layout the control.
            if (control != null)
                control.Bounds = new Rectangle(leftInset,
                                                topInset,
                                                focusWatchingUserControl.Width - rightInset,
                                                autoHeight ? control.Height : focusWatchingUserControl.Height - (topInset + bottomInset));

            //	Make sure the control gets repainted.
            Invalidate();
        }

        /// <summary>
        /// Raises the PaintBackground event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnPaintBackground(e);

            if (themeBorder)
            {
                //	Draw the border.
                using (Pen pen = new Pen(themeBorderColor))
                    e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
            else
                ControlPaint.DrawBorder3D(e.Graphics, ClientRectangle, Border3DStyle.Sunken);
        }

        /// <summary>
        /// control_EnabledChanged event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void control_EnabledChanged(object sender, EventArgs e)
        {
            focusWatchingUserControl.BackColor = control.Enabled ? control.BackColor : SystemColors.Control;
            focusWatchingUserControl.Invalidate();
        }

        #endregion Protected Event Overrides
    }
}
