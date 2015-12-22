// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.ApplicationFramework
{
    #region Public Enumerations

    /// <summary>
    /// Bevel styles.
    /// </summary>
    public enum BevelStyle
    {
        None,
        SingleLine,
        DoubleLine
    }

    #endregion Public Enumerations

    /// <summary>
    /// CommandBar lightweight control.
    /// </summary>
    public class CommandBarLightweightControl : LightweightControl
    {
        #region Private Memeber Variables & Declarations

        /// <summary>
        /// The CommandManager for the CommandBarLightweightControl.
        /// </summary>
        private CommandManager commandManager;

        /// <summary>
        /// The left command bar container lightweight control.
        /// </summary>
        private CommandBarContainerLightweightControl leftContainer;

        /// <summary>
        /// The right command bar container lightweight control.
        /// </summary>
        private CommandBarContainerLightweightControl rightContainer;

        /// <summary>
        /// The command bar definition for this command bar lightweight control.
        /// </summary>
        private CommandBarDefinition commandBarDefinition;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components;

        /// <summary>
        /// The top bevel style.
        /// </summary>
        private BevelStyle topBevelStyle = BevelStyle.None;

        /// <summary>
        /// The bottom bevel style.
        /// </summary>
        private BevelStyle bottomBevelStyle = BevelStyle.None;

        #endregion Private Memeber Variables & Declarations

        #region Public Events

        /// <summary>
        /// The CommandManagerChanged event.
        /// </summary>
        public event EventHandler CommandManagerChanged;

        #endregion Public Events

        #region Class Initialization & Termination

        /// <summary>
        /// Initializes a new instance of the CommandBarLightweightControl class.
        /// </summary>
        /// <param name="container"></param>
        public CommandBarLightweightControl(IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            InitializeComponent();

            //	Common object initialization.
            InitializeObject();
        }

        /// <summary>
        /// Initializes a new instance of the CommandBarLightweightControl class.
        /// </summary>
        public CommandBarLightweightControl()
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            InitializeComponent();

            //	Common object initialization.
            InitializeObject();
        }

        /// <summary>
        /// Common object initialization.
        /// </summary>
        private void InitializeObject()
        {
            AccessibleRole = AccessibleRole.ToolBar;
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
            this.components = new System.ComponentModel.Container();
            this.leftContainer = new OpenLiveWriter.ApplicationFramework.CommandBarContainerLightweightControl(this.components);
            this.rightContainer = new OpenLiveWriter.ApplicationFramework.CommandBarContainerLightweightControl(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.leftContainer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rightContainer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            //
            // leftContainer
            //
            this.leftContainer.LightweightControlContainerControl = this;
            //
            // rightContainer
            //
            this.rightContainer.LightweightControlContainerControl = this;
            ((System.ComponentModel.ISupportInitialize)(this.leftContainer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rightContainer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }
        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the CommandManager for the CommandBarLightweightControl.
        /// </summary>
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public CommandManager CommandManager
        {
            get
            {
                return commandManager;
            }
            set
            {
                if (commandManager != null)
                    commandManager.Changed -= new EventHandler(commandManager_Changed);

                commandManager = value;

                if (commandManager != null)
                    commandManager.Changed += new EventHandler(commandManager_Changed);
            }
        }

        /// <summary>
        /// Gets or sets the top bevel style.
        /// </summary>
        [
            Category("Appearance"),
                DefaultValue(BevelStyle.None),
                Description("Specifies the top bevel style.")
        ]
        public BevelStyle TopBevelStyle
        {
            get
            {
                return topBevelStyle;
            }
            set
            {
                if (topBevelStyle != value)
                {
                    topBevelStyle = value;
                    PerformLayout();
                }
            }
        }

        /// <summary>
        /// Gets or sets the bottom bevel style.
        /// </summary>
        ///
        [
            Category("Appearance"),
                DefaultValue(BevelStyle.DoubleLine),
                Description("Specifies the bottom bevel style.")
        ]
        public BevelStyle BottomBevelStyle
        {
            get
            {
                return bottomBevelStyle;
            }
            set
            {
                if (bottomBevelStyle != value)
                {
                    bottomBevelStyle = value;
                    PerformLayout();
                }
            }
        }

        /// <summary>
        /// Gets the top layout margin.
        /// </summary>
        public virtual int TopLayoutMargin
        {
            get
            {
                return -1;
            }
        }

        /// <summary>
        /// Gets the left layout margin.
        /// </summary>
        public virtual int LeftLayoutMargin
        {
            get
            {
                return 2;
            }
        }

        /// <summary>
        /// Gets the bottom layout margin.
        /// </summary>
        public virtual int BottomLayoutMargin
        {
            get
            {
                return -2;
            }
        }

        /// <summary>
        /// Gets the right layout margin.
        /// </summary>
        public virtual int RightLayoutMargin
        {
            get
            {
                return 2;
            }
        }

        /// <summary>
        /// Gets the separator layout margin.
        /// </summary>
        public virtual int SeparatorLayoutMargin
        {
            get
            {
                return 2;
            }
        }

        /// <summary>
        ///	Gets the top command bar color.
        /// </summary>
        public virtual Color TopColor
        {
            get
            {
                return SystemColors.Control;
            }
        }

        /// <summary>
        ///	Gets the bottom command bar color.
        /// </summary>
        public virtual Color BottomColor
        {
            get
            {
                return SystemColors.Control;
            }
        }

        /// <summary>
        ///	Gets the top bevel first line color.
        /// </summary>
        public virtual Color TopBevelFirstLineColor
        {
            get
            {
                return !UseHighContrastMode ? SystemColors.Control : SystemColors.ControlLight;
            }
        }

        /// <summary>
        ///	Gets the top bevel second line color.
        /// </summary>
        public virtual Color TopBevelSecondLineColor
        {
            get
            {
                return !UseHighContrastMode ? SystemColors.Control : SystemColors.ControlLight;
            }
        }

        /// <summary>
        ///	Gets the bottom bevel first line color.
        /// </summary>
        public virtual Color BottomBevelFirstLineColor
        {
            get
            {
                return !UseHighContrastMode ? SystemColors.Control : SystemColors.ControlLight;
            }
        }

        /// <summary>
        ///	Gets the bottom bevel second line color.
        /// </summary>
        public virtual Color BottomBevelSecondLineColor
        {
            get
            {
                return !UseHighContrastMode ? SystemColors.Control : SystemColors.ControlLight;
            }
        }

        /// <summary>
        ///	Gets the text color.
        /// </summary>
        public virtual Color TextColor
        {
            get
            {
                return !UseHighContrastMode ? SystemColors.WindowText : SystemColors.ControlText;
            }
        }

        /// <summary>
        ///	Gets the text color.
        /// </summary>
        public virtual Color DisabledTextColor
        {
            get
            {
                return SystemColors.GrayText;
            }
        }

        public int LeftContainerOffSetSpacing
        {
            get
            {
                return leftContainer.OffsetSpacing;
            }
            set
            {
                leftContainer.OffsetSpacing = value;
            }
        }

        public int RightContainerOffSetSpacing
        {
            get
            {
                return rightContainer.OffsetSpacing;
            }
            set
            {
                rightContainer.OffsetSpacing = value;
            }
        }

        protected bool UseHighContrastMode
        {
            get { return SystemInformation.HighContrast; }
        }

        /// <summary>
        /// Gets or sets the command bar definition for this command bar lightweight control.
        /// </summary>
        [
            Category("Design"),
                DefaultValue(null),
                Description("Specifies the command bar definition for this command bar lightweight control.")
        ]
        public CommandBarDefinition CommandBarDefinition
        {
            get
            {
                return commandBarDefinition;
            }
            set
            {
                //	If we have a current command bar definition, clear all the current command bar
                //	lightweight controls.
                if (commandBarDefinition != null)
                {
                    leftContainer.LightweightControls.Clear();
                    rightContainer.LightweightControls.Clear();
                }

                //	Se the new command bar definition.
                commandBarDefinition = value;

                //	If we have a new command bar definition, add the new command bar lightweight
                //	controls.
                if (commandBarDefinition != null)
                {
                    //	Enumerate the left command bar entries.
                    foreach (CommandBarEntry commandBarEntry in commandBarDefinition.LeftCommandBarEntries)
                    {
                        //	Get the lightweight control for this command bar entry.
                        LightweightControl lightweightControl = commandBarEntry.GetLightweightControl(this, false);

                        //	Add the lightweight control to the left command bar container lightweight control.
                        if (lightweightControl != null)
                            leftContainer.LightweightControls.Add(lightweightControl);
                    }

                    //	Enumerate the right command bar entries.
                    foreach (CommandBarEntry commandBarEntry in commandBarDefinition.RightCommandBarEntries)
                    {
                        //	Get the lightweight control for this command bar entry.
                        LightweightControl lightweightControl = commandBarEntry.GetLightweightControl(this, true);

                        //	Add the lightweight control to the left command bar container lightweight control.
                        if (lightweightControl != null)
                            rightContainer.LightweightControls.Add(lightweightControl);
                    }
                }

                //	Layout the command bar lightweight controls.
                PerformLayout();
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
                //	The minimum virtual size (adjust this for expansion joint).
                return new Size(0, DefaultVirtualSize.Height);
            }
        }

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
                //	If we are not initialized, just return.
                if (leftContainer == null && rightContainer == null)
                    return Size.Empty;

                //	Obtain the left container size and the right container size.
                Size leftContainerSize = leftContainer.DefaultVirtualSize;
                Size rightContainerSize = rightContainer.DefaultVirtualSize;

                //	If left and and right containers are empty, the command bar is empty, so it has no size.
                if (leftContainerSize == Size.Empty && rightContainerSize == Size.Empty)
                    return Size.Empty;

                //	If right container is empty, the command bar is sized for the left container.
                if (rightContainerSize == Size.Empty)
                    return new Size(LeftLayoutMargin + leftContainerSize.Width + RightLayoutMargin,
                        TopLayoutMargin + leftContainerSize.Height + BottomLayoutMargin);

                //	If left container is empty, the command bar is sized for the right container.
                if (leftContainerSize == Size.Empty)
                    return new Size(LeftLayoutMargin + rightContainerSize.Width + RightLayoutMargin,
                        TopLayoutMargin + rightContainerSize.Height + BottomLayoutMargin);

                //	Size the command bar for both left and right containers.
                return new Size(LeftLayoutMargin + leftContainerSize.Width + LeftLayoutMargin + rightContainerSize.Width + RightLayoutMargin,
                    TopLayoutMargin + Math.Max(leftContainerSize.Height, rightContainerSize.Height) + BottomLayoutMargin);
            }
        }

        public virtual Bitmap ContextMenuArrowBitmap
        {
            get
            {
                return ResourceHelper.LoadAssemblyResourceBitmap("Images.CommandBar.ContextMenuArrow.png");
            }
        }

        public virtual Bitmap ContextMenuArrowBitmapDisabled
        {
            get
            {
                return ResourceHelper.LoadAssemblyResourceBitmap("Images.CommandBar.ContextMenuArrowDisabled.png");
            }
        }

        public virtual ButtonMargins? GetButtonMargins(ButtonFeatures features, bool rightAligned)
        {
            switch (features)
            {
                case ButtonFeatures.Image:
                    return new ButtonMargins(5, 0, 0, 5, 0);
                case ButtonFeatures.Image | ButtonFeatures.Menu:
                    return new ButtonMargins(5, 0, 3, 5, 0);
                case ButtonFeatures.Text:
                    return new ButtonMargins(0, 8, 0, 8, 0);
                case ButtonFeatures.Text | ButtonFeatures.Menu:
                    return new ButtonMargins(0, 8, 5, 8, 0);
                case ButtonFeatures.Image | ButtonFeatures.Text:
                    return new ButtonMargins(8, 4, 0, 8, 0);
                default:
                    return null;
            }
        }

        #endregion Public Properties

        #region Protected Methods

        /// <summary>
        /// Raises the CommandManagerChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnCommandManagerChanged(EventArgs e)
        {
            if (CommandManagerChanged != null)
                CommandManagerChanged(null, e);
        }

        #endregion Protected Methods

        #region Protected Event Overrides

        /// <summary>
        /// Raises the Layout event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLayout(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLayout(e);

            //	Have the containers perform their layout.
            leftContainer.PerformLayout();
            rightContainer.PerformLayout();

            //	Calculate the layout height.
            int layoutHeight = this.VirtualHeight - (TopLayoutMargin + BottomLayoutMargin);

            //	Set the left container height to the layout hight, and have it layout.
            leftContainer.VirtualHeight = layoutHeight;

            //	Set the right container height to the layout hight, and have it layout.
            rightContainer.VirtualHeight = layoutHeight;

            //	Place the left container control (this is easy).
            leftContainer.VirtualLocation = new Point(LeftLayoutMargin, TopLayoutMargin);
            leftContainer.PerformLayout();

            //	Place the right container (either immediately following the left container, or
            //	right aligned, if possible).
            if (leftContainer.VirtualBounds.Right + LeftLayoutMargin + rightContainer.VirtualWidth + RightLayoutMargin > VirtualWidth)
                rightContainer.VirtualLocation = new Point(leftContainer.VirtualBounds.Right + LeftLayoutMargin, TopLayoutMargin);
            else
                rightContainer.VirtualLocation = new Point(VirtualClientRectangle.Right - (rightContainer.VirtualWidth + RightLayoutMargin), TopLayoutMargin);
            rightContainer.PerformLayout();

            RtlLayoutFixup(false);
        }

        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            BidiGraphics g = new BidiGraphics(e.Graphics, VirtualClientRectangle);
            //	Fill the background.
            if (TopColor == BottomColor)
                using (SolidBrush solidBrush = new SolidBrush(TopColor))
                    g.FillRectangle(solidBrush, VirtualClientRectangle);
            else
                using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(VirtualClientRectangle, TopColor, BottomColor, LinearGradientMode.Vertical))
                    g.FillRectangle(linearGradientBrush, VirtualClientRectangle);

            //	Draw the first line of the top bevel, if we should.
            if (TopBevelStyle != BevelStyle.None)
                using (SolidBrush solidBrush = new SolidBrush(TopBevelFirstLineColor))
                    g.FillRectangle(solidBrush, 0, 0, VirtualWidth, 1);

            if (TopBevelStyle == BevelStyle.DoubleLine)
                using (SolidBrush solidBrush = new SolidBrush(TopBevelSecondLineColor))
                    g.FillRectangle(solidBrush, 0, 1, VirtualWidth, 1);

            //	Draw the first line of the bottom bevel.
            if (BottomBevelStyle == BevelStyle.DoubleLine)
            {
                using (SolidBrush solidBrush = new SolidBrush(BottomBevelFirstLineColor))
                    g.FillRectangle(solidBrush, 0, VirtualHeight - 2, VirtualWidth, 1);
                using (SolidBrush solidBrush = new SolidBrush(BottomBevelSecondLineColor))
                    g.FillRectangle(solidBrush, 0, VirtualHeight - 1, VirtualWidth, 1);
            }
            else if (BottomBevelStyle == BevelStyle.SingleLine)
            {
                using (SolidBrush solidBrush = new SolidBrush(BottomBevelFirstLineColor))
                    g.FillRectangle(solidBrush, 0, VirtualHeight - 1, VirtualWidth, 1);
            }

            //	Call the base class's method so that registered delegates receive the event.
            base.OnPaint(e);
        }

        #endregion Protected Event Overrides

        #region Private Event Handlers

        /// <summary>
        /// commandManager_Changed event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void commandManager_Changed(object sender, EventArgs e)
        {
            //	Raise the CommandManagerChanged event.
            OnCommandManagerChanged(EventArgs.Empty);

            //	Layout.
            LightweightControlContainerControl.PerformLayout();
            Invalidate();
        }

        #endregion Private Event Handlers

        #region Accessibility
        protected override void AddAccessibleControlsToList(ArrayList list)
        {
            AddCommandBarEntries(commandBarDefinition.LeftCommandBarEntries, list, false);
            AddCommandBarEntries(commandBarDefinition.RightCommandBarEntries, list, true);
        }
        private void AddCommandBarEntries(CommandBarEntryCollection entries, ArrayList list, bool rightAligned)
        {
            foreach (CommandBarEntry entry in entries)
            {
                LightweightControl control = entry.GetLightweightControl(this, rightAligned);
                list.Add(control);
            }
        }
        #endregion
    }

    public struct ButtonMargins
    {
        public readonly int LeftOfImage;
        public readonly int LeftOfText;
        public readonly int LeftOfArrow;
        public readonly int RightPadding;
        public readonly int RightMargin;

        public ButtonMargins(int leftOfImage, int leftOfText, int leftOfArrow, int rightPadding, int rightMargin)
        {
            LeftOfImage = leftOfImage;
            LeftOfText = leftOfText;
            LeftOfArrow = leftOfArrow;
            RightPadding = rightPadding;
            RightMargin = rightMargin;
        }
    }

    [Flags]
    public enum ButtonFeatures
    {
        None = 0,
        Image = 1,
        Text = 2,
        Menu = 4,
        SplitMenu = 8
    };

}
