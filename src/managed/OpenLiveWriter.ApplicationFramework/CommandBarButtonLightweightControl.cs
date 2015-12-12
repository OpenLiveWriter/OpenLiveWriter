// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// CommandBar button lightweight control.
    /// </summary>
    public class CommandBarButtonLightweightControl : LightweightControl
    {
        #region Private Enumerations

        /// <summary>
        /// The button draw state.
        /// </summary>
        public enum DrawState
        {
            Disabled,
            Enabled,
            Selected,
            Pushed
        }

        #endregion Private Enumerations

        #region Static & Constant Declaration

        /// <summary>
        /// The string format used to format text.
        /// </summary>
        private static StringFormat stringFormat;

        /// <summary>
        ///	The maximum text with of text on a button.
        /// </summary>
        private const int MAX_TEXT_WIDTH = 300;

        /// <summary>
        /// The top margin to leave around the command bar button bitmap.
        /// </summary>
        public const int TOP_MARGIN = 4;

        /// <summary>
        /// The bottom margin to leave around the command bar button image and text.
        /// </summary>
        public const int BOTTOM_MARGIN = 4;

        /// <summary>
        /// The drop down button width.
        /// </summary>
        private const int DROP_DOWN_BUTTON_WIDTH = 17;

        /// <summary>
        /// The context menu indicator width.
        /// </summary>
        private const int CONTEXT_MENU_INDICATOR_WIDTH = 12;

        /// <summary>
        /// The offset at which to paint the context menu arrow.
        /// </summary>
        private const int CONTEXT_MENU_ARROW_OFFSET = 3;

        /// <summary>
        /// Horizontal padding between provider buttons
        /// </summary>
        private const int PROVIDER_HORIZONTAL_PAD = 1;

        /// <summary>
        /// The context menu arrow bitmap.
        /// </summary>
        private Bitmap contextMenuArrowBitmap;

        /// <summary>
        /// The disabled context menu arrow bitmap.
        /// </summary>
        private Bitmap contextMenuArrowBitmapDisabled;

        #endregion Static & Constant Declaration

        #region Private Member Variables & Declarations

        /// <summary>
        /// The command bar lightweight control that this CommandBarButtonLightweightControl is
        /// associated with.
        /// </summary>
        private readonly CommandBarLightweightControl commandBarLightweightControl;

        /// <summary>
        /// The command identifier of the command that is associated with this
        /// CommandBarButtonLightweightControl.
        /// </summary>
        private readonly string commandIdentifier;

        private readonly bool rightAligned;

        /// <summary>
        /// The command that is associated with this CommandBarButtonLightweightControl.
        /// </summary>
        private Command command;

        /// <summary>
        /// Used to avoid extra layouts and paints.
        /// </summary>
        private CommandState lastLayoutCommandState;

        /// <summary>
        /// A value indicating whether the mouse is inside the control.
        /// </summary>
        private bool mouseInside = false;

        private bool mouseInsideContextMenu = false;

        /// <summary>
        /// A value indicating whether the button is pushed.  (Note that "pushed" applies to
        /// the button and not to the context menu.)
        /// </summary>
        private bool buttonPushed = false;

        /// <summary>
        /// A value indicating whether the context menu is showing.
        /// </summary>
        private bool contextMenuShowing;

        private Rectangle rImage = Rectangle.Empty;
        private Rectangle rText = Rectangle.Empty;
        private Rectangle rArrow = Rectangle.Empty;

        private int marginLeft, marginRight;

        #endregion Private Member Variables & Declarations

        #region Class Initialization & Termination

        /// <summary>
        /// Static initialization of the CommandBarButtonLightweightControl class.
        /// </summary>
        static CommandBarButtonLightweightControl()
        {
            //	Initialize the string format.
            stringFormat = new StringFormat();
            stringFormat.LineAlignment = StringAlignment.Center;
            stringFormat.Trimming = StringTrimming.EllipsisCharacter;
        }

        public CommandBarButtonLightweightControl(CommandBarLightweightControl commandBarLightweightControl, string commandIdentifier, bool rightAligned)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            InitializeComponent();
            InitializeObject();

            //	Set the command bar lightweight control.
            this.commandBarLightweightControl = commandBarLightweightControl;
            commandBarLightweightControl.CommandManagerChanged += new EventHandler(commandBarLightweightControl_CommandManagerChanged);

            //	Set the command identifier.
            this.commandIdentifier = commandIdentifier;

            this.rightAligned = rightAligned;

            this.contextMenuArrowBitmap = commandBarLightweightControl.ContextMenuArrowBitmap;
            this.contextMenuArrowBitmapDisabled = commandBarLightweightControl.ContextMenuArrowBitmapDisabled;

            //	Update the command.
            UpdateCommand();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Command = null;
                if (commandBarLightweightControl != null)
                    commandBarLightweightControl.CommandManagerChanged -= new EventHandler(commandBarLightweightControl_CommandManagerChanged);
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
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            //
            // CommandBarButtonLightweightControl
            //
            this.Visible = false;
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }
        #endregion

        /// <summary>
        /// Common object initialization.
        /// </summary>
        private void InitializeObject()
        {
            AccessibleRole = AccessibleRole.PushButton;
            AccessibleDefaultAction = "Press";
            TabStop = true;
        }

        #region Protected Event Overrides

        /// <summary>
        /// Encapsulates the relevant state from a Command that determines
        /// whether a re-layout is needed.
        /// </summary>
        private class CommandState
        {
            private readonly CommandBarButtonStyle style;
            private readonly string text;
            private readonly bool on;
            private readonly bool visibleOnCommandBar;
            private readonly Size imageSize;

            public CommandState(Command command, Size imageSize)
            {
                style = command.CommandBarButtonStyle;
                text = command.Text;
                on = command.On;
                visibleOnCommandBar = command.VisibleOnCommandBar;
                this.imageSize = imageSize;
            }

            public bool NeedsLayout(Command command, Size imageSize)
            {
                return style != command.CommandBarButtonStyle
                       || on != command.On
                       || visibleOnCommandBar != command.VisibleOnCommandBar
                       || command.Text != text
                       || !this.imageSize.Equals(imageSize);
            }
        }

        public int MarginLeft
        {
            get { return marginLeft; }
        }

        public int MarginRight
        {
            get { return marginRight; }
        }

        /// <summary>
        /// Raises the Layout event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLayout(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLayout(e);

            //	No layout required if there is no parent or no command.
            if (Parent == null || Command == null)
            {
                lastLayoutCommandState = null;

                VirtualSize = Size.Empty;
                return;
            }

            lastLayoutCommandState = new CommandState(Command, ImageSize);

            //	The button width and height.
            int buttonWidth = 0, buttonHeight = 0;
            marginLeft = 0;
            marginRight = 0;

            //	If the command bar button style is "System", then do the layout.
            if (Command.CommandBarButtonStyle == CommandBarButtonStyle.System)
            {
                rImage = Rectangle.Empty;
                rText = Rectangle.Empty;
                rArrow = Rectangle.Empty;

                buttonHeight = 24;

                //	Obtain the font.
                Font font = ApplicationManager.ApplicationStyle.NormalApplicationFont;

                ButtonFeatures features = ButtonFeatures.None;

                //	Adjust the button width and height for the command bar button bitmap, if there is one.
                if (HasImage && !Command.SuppressCommandBarBitmap)
                {
                    features |= ButtonFeatures.Image;
                    rImage.Size = ImageSize;
                }

                //	Adjust the button width and height for the command bar button text, if there is text.
                if (!string.IsNullOrEmpty(Command.CommandBarButtonText))
                {
                    features |= ButtonFeatures.Text;
                    using (Graphics graphics = Parent.CreateGraphics())
                        rText.Size = TextRenderer.MeasureText(graphics, Command.CommandBarButtonText, font, Size.Empty, TextFormatFlags.NoPadding);
                }

                //	Enlarge the width for the drop-down button or context menu indicator, as needed.
                if (DropDownContextMenuUserInterface)
                {
                    features |= ButtonFeatures.SplitMenu;
                    rArrow.Size = contextMenuArrowBitmap.Size;
                }
                else if (ContextMenuUserInterface)
                {
                    features |= ButtonFeatures.Menu;
                    rArrow.Size = contextMenuArrowBitmap.Size;
                }

                ButtonMargins? margins = commandBarLightweightControl.GetButtonMargins(features, rightAligned);
                if (margins == null)
                {
                    Trace.Fail("Don't know how to layout these features: " + features + ", " + commandBarLightweightControl.GetType().Name);
                    VirtualSize = Size.Empty;
                    return;
                }
                ButtonMargins margin = margins.Value;
                marginRight = margin.RightMargin;

                rImage.Y = Utility.CenterMinZero(rImage.Height, buttonHeight);
                rText.Y = Utility.CenterMinZero(rText.Height, buttonHeight);
                rArrow.Y = Utility.CenterMinZero(rArrow.Height, buttonHeight);

                rImage.X = margin.LeftOfImage;
                rText.X = rImage.Right + margin.LeftOfText;
                rArrow.X = rText.Right + margin.LeftOfArrow;
                buttonWidth = rArrow.Right + margin.RightPadding;
            }
            else if (Command.CommandBarButtonStyle == CommandBarButtonStyle.Bitmap)
            {
                buttonWidth += HorizontalMargin + Command.CommandBarButtonBitmapEnabled.Width + HorizontalMargin;
                buttonHeight += TOP_MARGIN + Command.CommandBarButtonBitmapEnabled.Height + BOTTOM_MARGIN;
            }
            else if (Command.CommandBarButtonStyle == CommandBarButtonStyle.Provider)
            {
                if (HasImage)
                {
                    buttonHeight = ProviderButtonFaceLeftEnabled.Height;
                    if (DropDownContextMenuUserInterface)
                    {
                        buttonWidth = ProviderButtonFaceLeftEnabled.Width + ProviderButtonFaceRightEnabled.Width;
                    }
                    else if (ContextMenuUserInterface)
                    {
                        buttonWidth = ProviderButtonFaceDropDownEnabled.Width;
                    }
                    else
                    {
                        buttonWidth = ProviderButtonFaceEnabled.Width;
                    }
                    buttonWidth += (2 * PROVIDER_HORIZONTAL_PAD);
                }
            }

            //	Set the new virtual size.
            VirtualSize = new Size(buttonWidth, buttonHeight + 2 * VerticalPadding);
        }

        private int HorizontalMargin
        {
            get
            {
                if (IsLargeButton)
                    return 6;
                else
                    return 4;
            }
        }

        private int VerticalPadding
        {
            get
            {
                return 0;
            }
        }

        private int SnapButtonHeight(int height)
        {
            // large buttons always occupy 42 pixels
            if (IsLargeButton)
                return SystemButtonHelper.LARGE_BUTTON_TOTAL_SIZE;
            else
                return TOP_MARGIN + height + BOTTOM_MARGIN;
        }

        private bool IsLargeButton
        {
            get
            {
                return ImageSize.Height > SystemButtonHelper.SMALL_BUTTON_IMAGE_SIZE;
            }
        }

        private bool HasImage
        {
            get
            {
                Size size = ImageSize;
                return size.Width > 0 && size.Height > 0;
            }
        }

        private Size ImageSize
        {
            get
            {
                if (Command is ICustomButtonBitmapPaint)
                    return new Size(((ICustomButtonBitmapPaint)Command).Width, ((ICustomButtonBitmapPaint)Command).Height);
                if (Command.CommandBarButtonBitmapEnabled != null)
                    return Command.CommandBarButtonBitmapEnabled.Size;
                return new Size(0, 0);
            }
        }

        /// <summary>
        /// Raises the MouseEnter event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseEnter(e);

            //	Ignore the event if input events are disabled.
            if (InputEventsDisabled)
                return;

            //	Set the MouseInside property.
            MouseInside = true;
        }

        /// <summary>
        /// Raises the MouseDown event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            //	Ignore the event if input events are disabled.
            if (InputEventsDisabled)
                return;

            //	Process left mouse button.
            if (e.Button == MouseButtons.Left)
            {
                //	If the drop down context menu user interface is being displayed, hit-test and
                //	do the dropdown if the mouse was pressed in the right area.  Otherwise, if the
                if (DropDownContextMenuUserInterface)
                {
                    if (InContextMenuArrow(e))
                        DoShowContextMenu();
                    else
                        ButtonPushed = true;
                }
                else if (ContextMenuUserInterface)
                    DoShowContextMenu();
                else
                    ButtonPushed = true;
            }

            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseDown(e);
        }

        private bool InContextMenuArrow(MouseEventArgs e)
        {
            if (!DropDownContextMenuUserInterface)
                return false;

            int x = e.X;

            if (BidiHelper.IsRightToLeft)
                x = VirtualWidth - x;

            if (Command.CommandBarButtonStyle == CommandBarButtonStyle.System)
                return x >= VirtualWidth - DROP_DOWN_BUTTON_WIDTH;
            else if (Command.CommandBarButtonStyle == CommandBarButtonStyle.Provider)
                return x >= VirtualWidth - ProviderButtonFaceRightEnabled.Width;
            else
                return false;
        }

        /// <summary>
        /// Raises the MouseLeave event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseLeave(e);

            //	Ignore the event if input events are disabled.
            if (InputEventsDisabled)
                return;

            //	Clear the ButtonPushed and MouseInside properties.
            ButtonPushed = MouseInside = MouseInsideContextMenu = false;
        }

        /// <summary>
        /// Raises the MouseMove event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseMove(e);

            //	Ignore the event if input events are disabled.
            if (InputEventsDisabled)
                return;

            MouseInsideContextMenu = this.InContextMenuArrow(e);

            //	Update the state of the MouseInside property if the LeftMouseButtonDown property
            //	is true.
            if (ButtonPushed)
                MouseInside = VirtualClientRectangle.Contains(e.X, e.Y);
        }

        /// <summary>
        /// Raises the MouseUp event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseUp(e);

            //	Ignore the event if input events are disabled.
            if (InputEventsDisabled)
                return;

            //	If the button is the left button, set the LeftMouseButtonDown property.
            if (e.Button == MouseButtons.Left && ButtonPushed)
            {
                //	Note that the button is not pushed.
                ButtonPushed = false;

                //	If the mouse was inside the lightweight control, execute the event.
                if (VirtualClientRectangle.Contains(e.X, e.Y))
                {
                    //	Execute the event.
                    if (Command != null && Command.On && Command.Enabled)
                        Command.PerformExecute();
                }
            }
        }

        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs args)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnPaint(args);

            // Fix bug 602576: The 'Remove Effects' button is truncated in right-to-left builds
            if (BidiHelper.IsRightToLeft)
                args.Graphics.ResetClip();

            // Take int account vertical padding
            Rectangle rect = new Rectangle(VirtualClientRectangle.X, VirtualClientRectangle.Y + VerticalPadding, VirtualClientRectangle.Width, VirtualClientRectangle.Height);

            BidiGraphics g = new BidiGraphics(args.Graphics, rect);

            //	No painting required if there is no parent or no command.
            if (Parent == null || Command == null)
                return;

            //	Determine the draw state of the button.
            DrawState drawState;
            if (!Command.Enabled)
                drawState = DrawState.Disabled;
            else if (Command.Latched || (ButtonPushed && MouseInside) || (ContextMenuUserInterface && ContextMenuShowing))
                drawState = DrawState.Pushed;
            else if (MouseInside || (DropDownContextMenuUserInterface && ContextMenuShowing))
                drawState = DrawState.Selected;
            else
                drawState = DrawState.Enabled;

            //	Draw the button.
            if (Command.CommandBarButtonStyle == CommandBarButtonStyle.System)
            {
                Size imageSize = ImageSize;

                // is this a large button?
                bool isLargeButton = false;
                if (imageSize.Height > SystemButtonHelper.SMALL_BUTTON_IMAGE_SIZE)
                    isLargeButton = true;

                if (Command is ICustomButtonBitmapPaint)
                {
                    switch (drawState)
                    {
                        case DrawState.Selected:
                            SystemButtonHelper.DrawSystemButtonFace(g, DropDownContextMenuUserInterface, contextMenuShowing, VirtualClientRectangle, isLargeButton);
                            break;
                        case DrawState.Pushed:
                            SystemButtonHelper.DrawSystemButtonFacePushed(g, DropDownContextMenuUserInterface, VirtualClientRectangle, isLargeButton);
                            break;
                    }

                    ((ICustomButtonBitmapPaint)Command).Paint(g, rImage, drawState);
                }
                else
                {
                    Bitmap buttonBitmap = null;
                    switch (drawState)
                    {
                        case DrawState.Disabled:
                            buttonBitmap = Command.CommandBarButtonBitmapDisabled;
                            break;
                        case DrawState.Enabled:
                            buttonBitmap = Command.CommandBarButtonBitmapEnabled;
                            break;
                        case DrawState.Selected:
                            SystemButtonHelper.DrawSystemButtonFace(g, DropDownContextMenuUserInterface, contextMenuShowing, VirtualClientRectangle, isLargeButton);
                            buttonBitmap = Command.CommandBarButtonBitmapSelected;
                            break;
                        case DrawState.Pushed:
                            SystemButtonHelper.DrawSystemButtonFacePushed(g, DropDownContextMenuUserInterface, VirtualClientRectangle, isLargeButton);
                            buttonBitmap = Command.CommandBarButtonBitmapSelected;
                            break;
                    }

                    //	Draw the button bitmap.
                    if (buttonBitmap != null && !Command.SuppressCommandBarBitmap)
                    {
                        if (!SystemInformation.HighContrast)
                            g.DrawImage(false, buttonBitmap, rImage);
                        else
                        {
                            //apply a high contrast image matrix
                            ImageAttributes ia = new ImageAttributes();
                            ia.SetColorMatrix(HighContrastColorMatrix);
                            ColorMap cm = new ColorMap();
                            cm.OldColor = Color.White;
                            cm.NewColor = SystemColors.Control;
                            ia.SetRemapTable(new ColorMap[] { cm });
                            g.DrawImage(false, buttonBitmap, rImage, 0, 0, buttonBitmap.Width, buttonBitmap.Height, GraphicsUnit.Pixel, ia);
                        }
                    }
                }

                //	Draw the text.
                if (Command.CommandBarButtonText != null && Command.CommandBarButtonText.Length != 0)
                {
                    Color textColor;
                    if (drawState == DrawState.Disabled)
                        textColor = commandBarLightweightControl.DisabledTextColor;
                    else
                        textColor = commandBarLightweightControl.TextColor;

                    g.DrawText(
                        Command.CommandBarButtonText,
                        ApplicationManager.ApplicationStyle.NormalApplicationFont,
                        rText,
                        textColor,
                        TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.NoPadding | TextFormatFlags.NoClipping);
                }

                //	Draw the context menu arrow, if needed.
                if (DropDownContextMenuUserInterface || ContextMenuUserInterface)
                {
                    Bitmap contextMenuArrowBitmapToDraw;
                    if (drawState == DrawState.Disabled)
                        contextMenuArrowBitmapToDraw = contextMenuArrowBitmapDisabled;
                    else
                        contextMenuArrowBitmapToDraw = contextMenuArrowBitmap;

                    int height = isLargeButton ? SystemButtonHelper.LARGE_BUTTON_TOTAL_SIZE : VirtualHeight;
                    g.DrawImage(false, contextMenuArrowBitmapToDraw, rArrow);
                }
            }
            else if (Command.CommandBarButtonStyle == CommandBarButtonStyle.Bitmap)
            {
                switch (drawState)
                {
                    case DrawState.Disabled:
                        g.DrawImage(true, Command.CommandBarButtonBitmapDisabled, HorizontalMargin, TOP_MARGIN);
                        break;
                    case DrawState.Enabled:
                        g.DrawImage(true, Command.CommandBarButtonBitmapEnabled, HorizontalMargin, TOP_MARGIN);
                        break;
                    case DrawState.Selected:
                        g.DrawImage(true, Command.CommandBarButtonBitmapSelected, HorizontalMargin, TOP_MARGIN);
                        break;
                    case DrawState.Pushed:
                        g.DrawImage(true, Command.CommandBarButtonBitmapPushed, HorizontalMargin, TOP_MARGIN);
                        break;
                }
            }
            else if (Command.CommandBarButtonStyle == CommandBarButtonStyle.Provider)
            {
                DrawProviderButtonFace(g, drawState);
                DrawProviderButton(g, drawState);
            }

            if (Focused)
                g.DrawFocusRectangle(new Rectangle(0, 0, VirtualWidth, VirtualHeight), Parent.ForeColor, Parent.BackColor);
        }

        private void DrawProviderButtonFace(BidiGraphics g, DrawState drawState)
        {
            if (DropDownContextMenuUserInterface)
            {
                if (drawState == DrawState.Selected)
                {
                    g.DrawImage(true, ProviderButtonFaceLeftEnabled, PROVIDER_HORIZONTAL_PAD, 0);
                    if (ContextMenuShowing)
                        g.DrawImage(true, ProviderButtonFaceRightPressed, PROVIDER_HORIZONTAL_PAD + ProviderButtonFaceLeftEnabled.Width, 0);
                    else
                        g.DrawImage(true, ProviderButtonFaceRightEnabled, PROVIDER_HORIZONTAL_PAD + ProviderButtonFaceLeftEnabled.Width, 0);
                }
                else if (drawState == DrawState.Pushed)
                {
                    if (ContextMenuShowing)
                    {
                        g.DrawImage(true, ProviderButtonFaceLeftEnabled, PROVIDER_HORIZONTAL_PAD, 0);
                        g.DrawImage(true, ProviderButtonFaceRightPressed, PROVIDER_HORIZONTAL_PAD + ProviderButtonFaceLeftEnabled.Width, 0);
                    }
                    else
                    {
                        g.DrawImage(true, ProviderButtonFaceLeftPressed, PROVIDER_HORIZONTAL_PAD, 0);
                        g.DrawImage(true, ProviderButtonFaceRightPressed, PROVIDER_HORIZONTAL_PAD + ProviderButtonFaceLeftEnabled.Width, 0);
                    }
                }
            }
            else if (ContextMenuUserInterface)
            {
                if (drawState == DrawState.Selected)
                    g.DrawImage(true, ProviderButtonFaceDropDownEnabled, PROVIDER_HORIZONTAL_PAD, 0);
                else if (drawState == DrawState.Pushed)
                    g.DrawImage(true, ProviderButtonFaceDropDownPressed, PROVIDER_HORIZONTAL_PAD, 0);
            }
            else
            {
                if (drawState == DrawState.Selected)
                    g.DrawImage(true, ProviderButtonFaceEnabled, PROVIDER_HORIZONTAL_PAD, 0);
                else if (drawState == DrawState.Pushed)
                    g.DrawImage(true, ProviderButtonFacePressed, PROVIDER_HORIZONTAL_PAD, 0);
            }
        }

        private void DrawProviderButton(BidiGraphics g, DrawState drawState)
        {
            // determine the bitmap to draw
            Bitmap buttonBitmap;
            switch (drawState)
            {
                case DrawState.Disabled:
                    buttonBitmap = Command.CommandBarButtonBitmapDisabled;
                    break;
                case DrawState.Enabled:
                    buttonBitmap = Command.CommandBarButtonBitmapEnabled;
                    break;
                case DrawState.Pushed:
                    buttonBitmap = Command.CommandBarButtonBitmapPushed;
                    break;
                case DrawState.Selected:
                    buttonBitmap = Command.CommandBarButtonBitmapSelected;
                    break;
                default:
                    Debug.Fail("Unexpected DrawState!");
                    buttonBitmap = Command.CommandBarButtonBitmapEnabled;
                    break;
            }

            // draw the button
            Rectangle centerButtonInRect;
            if (DropDownContextMenuUserInterface)
                centerButtonInRect = new Rectangle(PROVIDER_HORIZONTAL_PAD, 0, ProviderButtonFaceLeftEnabled.Width, ProviderButtonFaceRightEnabled.Height);
            else if (ContextMenuUserInterface)
                centerButtonInRect = new Rectangle(PROVIDER_HORIZONTAL_PAD, 0, ProviderButtonFaceEnabled.Width, ProviderButtonFaceEnabled.Height);
            else
                centerButtonInRect = new Rectangle(PROVIDER_HORIZONTAL_PAD, 0, ProviderButtonFaceEnabled.Width, ProviderButtonFaceEnabled.Height);

            int left = centerButtonInRect.Left + (centerButtonInRect.Width / 2) - (buttonBitmap.Width / 2) + (drawState == DrawState.Pushed ? 1 : 0);
            int top = centerButtonInRect.Top + (centerButtonInRect.Height / 2) - (buttonBitmap.Height / 2) + (drawState == DrawState.Pushed ? 1 : 0);
            g.DrawImage(false, buttonBitmap, left, top);

            // draw the accompanying arrow if necessary
            Rectangle centerArrowInRect = Rectangle.Empty;
            if (DropDownContextMenuUserInterface)
                centerArrowInRect = new Rectangle(centerButtonInRect.Width, 0, ProviderButtonFaceRightEnabled.Width, ProviderButtonFaceRightEnabled.Height);
            else if (ContextMenuUserInterface)
                centerArrowInRect = new Rectangle(centerButtonInRect.Width - 1, 0, ProviderButtonFaceDropDownEnabled.Width - ProviderButtonFaceEnabled.Width, ProviderButtonFaceDropDownEnabled.Height);
            if (centerArrowInRect != Rectangle.Empty)
            {
                int arrowLeft = centerArrowInRect.Left + (centerArrowInRect.Width / 2) - (ProviderDownArrow.Width / 2) + (ContextMenuShowing ? 1 : 0);
                int arrowTop = centerArrowInRect.Top + (centerArrowInRect.Height / 2) - (ProviderDownArrow.Height / 2) + (ContextMenuShowing ? 1 : 0);
                g.DrawImage(true, ProviderDownArrow, arrowLeft, arrowTop);
            }
        }

        #endregion Protected Event Overrides

        #region Private Properties

        /// <summary>
        /// Gets or sets command that is associated with this CommandBarButtonLightweightControl.
        /// </summary>
        private Command Command
        {
            get
            {
                return command;
            }
            set
            {
                //	If the command is changing, change it.
                if (command != value)
                {
                    //	Remove event handlers.
                    if (command != null)
                    {
                        command.ShowCommandBarButtonContextMenu -= new EventHandler(command_ShowCommandBarButtonContextMenu);
                        command.StateChanged -= new EventHandler(command_StateChanged);
                        command.CommandBarButtonTextChanged -= new EventHandler(command_StateChanged);
                        command.VisibleOnCommandBarChanged -= new EventHandler(command_StateChanged);
                        command.CommandBarButtonContextMenuDefinitionChanged -= new EventHandler(command_CommandBarButtonContextMenuDefinitionChanged);
                        Visible = false;
                    }

                    //	Set the new command.
                    command = value;

                    //	Add event handlers.
                    if (command != null)
                    {
                        command.ShowCommandBarButtonContextMenu += new EventHandler(command_ShowCommandBarButtonContextMenu);
                        command.StateChanged += new EventHandler(command_StateChanged);
                        command.CommandBarButtonTextChanged += new EventHandler(command_StateChanged);
                        command.VisibleOnCommandBarChanged += new EventHandler(command_StateChanged);
                        Visible = command.On && command.VisibleOnCommandBar;
                        command.CommandBarButtonContextMenuDefinitionChanged += new EventHandler(command_CommandBarButtonContextMenuDefinitionChanged);
                    }

                    SetAccessibleInfo();
                }
            }
        }

        void command_CommandBarButtonContextMenuDefinitionChanged(object sender, EventArgs e)
        {
            SetAccessibleInfo();
        }

        public void SetAccessibleInfo()
        {
            if (ContextMenuUserInterface || DropDownContextMenuUserInterface)
                AccessibleRole = AccessibleRole.ButtonMenu;
            else
                AccessibleRole = AccessibleRole.PushButton;

            //update the accessibility name of this control
            AccessibleName = command != null ? command.Text : null;

            AccessibleKeyboardShortcut = command != null && command.Shortcut != Shortcut.None ? KeyboardHelper.FormatShortcutString(command.Shortcut) : null;
        }

        /// <summary>
        /// Returns a value indicating whether input events are disabled.
        /// </summary>
        private bool InputEventsDisabled
        {
            get
            {
                //	Disable input events if there is no command, or if there is a command but it is
                //	not active and enabled.
                return Command == null || !(Command.On && Command.Enabled);
            }
        }

        /// <summary>
        /// Gets the tool tip text.
        /// </summary>
        private string ToolTipText
        {
            get
            {
                if (Command == null || !Command.On)
                    return null;
                else
                    return Command.Text;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the mouse is inside the control.
        /// </summary>
        private bool MouseInside
        {
            get
            {
                return mouseInside;
            }
            set
            {
                //	Ensure that the property is actually changing.
                if (mouseInside != value)
                {
                    //	Update the value.
                    mouseInside = value;

                    Invalidate();

                    //	Update the tool tip text, if the parent implements IToolTipDisplay.  Note
                    //	that we set the tool tip text when the parent implements IToolTipDisplay so
                    //	an older tool tip will be erased if it was being displayed.
                    if (Parent is IToolTipDisplay)
                    {
                        IToolTipDisplay toolTipDisplay = (IToolTipDisplay)Parent;
                        if (mouseInside && !ButtonPushed)
                            toolTipDisplay.SetToolTip(ToolTipText);
                        else
                            toolTipDisplay.SetToolTip(null);
                    }
                }
            }
        }

        private bool MouseInsideContextMenu
        {
            get { return mouseInsideContextMenu; }
            set
            {
                if (mouseInsideContextMenu != value)
                {
                    mouseInsideContextMenu = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the button is pushed.
        /// </summary>
        private bool ButtonPushed
        {
            get
            {
                return buttonPushed;
            }
            set
            {
                if (buttonPushed != value)
                {
                    buttonPushed = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the dropdown context menu user interface should be displayed.
        /// </summary>
        public bool DropDownContextMenuUserInterface
        {
            get
            {
                return Command != null &&
                        (Command.CommandBarButtonContextMenu != null
                            || Command.CommandBarButtonContextMenuDefinition != null
                            || Command.CommandBarButtonContextMenuControlHandler != null
                            || Command.CommandBarButtonContextMenuHandler != null) &&
                        Command.CommandBarButtonContextMenuDropDown;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the context menu user interface should be displayed.
        /// </summary>
        private bool ContextMenuUserInterface
        {
            get
            {
                return Command != null &&
                        (Command.CommandBarButtonContextMenu != null
                            || Command.CommandBarButtonContextMenuDefinition != null
                            || Command.CommandBarButtonContextMenuControlHandler != null
                            || Command.CommandBarButtonContextMenuHandler != null) &&
                        !Command.CommandBarButtonContextMenuDropDown;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the context menu is showing.
        /// </summary>
        private bool ContextMenuShowing
        {
            get
            {
                return contextMenuShowing;
            }
            set
            {
                if (contextMenuShowing != value)
                {
                    contextMenuShowing = value;
                    Invalidate();
                    Update();
                }
            }
        }

        private static Bitmap ProviderButtonFaceEnabled { get { return ResourceHelper.LoadAssemblyResourceBitmap("Images.CommandBar.ProviderButtonFaceEnabled.png"); } }
        private static Bitmap ProviderButtonFacePressed { get { return ResourceHelper.LoadAssemblyResourceBitmap("Images.CommandBar.ProviderButtonFacePressed.png"); } }
        private static Bitmap ProviderButtonFaceDropDownEnabled { get { return ResourceHelper.LoadAssemblyResourceBitmap("Images.CommandBar.ProviderButtonFaceDropDownEnabled.png"); } }
        private static Bitmap ProviderButtonFaceDropDownPressed { get { return ResourceHelper.LoadAssemblyResourceBitmap("Images.CommandBar.ProviderButtonFaceDropDownPressed.png"); } }
        private static Bitmap ProviderButtonFaceLeftEnabled { get { return ResourceHelper.LoadAssemblyResourceBitmap("Images.CommandBar.ProviderButtonFaceLeftEnabled.png"); } }
        private static Bitmap ProviderButtonFaceLeftPressed { get { return ResourceHelper.LoadAssemblyResourceBitmap("Images.CommandBar.ProviderButtonFaceLeftPressed.png"); } }
        private static Bitmap ProviderButtonFaceRightEnabled { get { return ResourceHelper.LoadAssemblyResourceBitmap("Images.CommandBar.ProviderButtonFaceRightEnabled.png"); } }
        private static Bitmap ProviderButtonFaceRightPressed { get { return ResourceHelper.LoadAssemblyResourceBitmap("Images.CommandBar.ProviderButtonFaceRightPressed.png"); } }
        private static Bitmap ProviderDownArrow { get { return ResourceHelper.LoadAssemblyResourceBitmap("Images.CommandBar.ProviderDownArrow.png"); } }

        #endregion Private Properties

        #region Private Methods

        /// <summary>
        /// Updates the command.
        /// </summary>
        private void UpdateCommand()
        {
            //	Locate the command.
            Command updateCommand = commandBarLightweightControl.CommandManager.Get(commandIdentifier);

            //	Set the command.
            Command = updateCommand;
        }

        /// <summary>
        /// Shows the context menu.  This is a modal operation -- it does not return until the user
        /// dismisses the context menu.
        /// </summary>
        private void DoShowContextMenu()
        {
            if (Command != null)
            {
                // calcualte point to show context menu at as well as alternative point
                // in the case where the menu might go off the right edge of the screen
                Point menuLocation = VirtualClientPointToScreen(new Point(0, VirtualHeight));
                int alternativeLocation = VirtualClientPointToScreen(new Point(VirtualWidth, VirtualHeight)).X;

                if (Command.CommandBarButtonContextMenuDefinition != null)
                {
                    //	Note that the context menu is showing.
                    StartShowContextMenu();

                    //	Show the context menu.
                    Command command = CommandContextMenu.ShowModal(commandBarLightweightControl.CommandManager, Parent, menuLocation, alternativeLocation, Command.CommandBarButtonContextMenuDefinition);

                    // cleanup state/ui
                    EndShowContextMenu();

                    //	If a command was selected, execute it.
                    if (command != null)
                        command.PerformExecute();
                }
                else if (Command.CommandBarButtonContextMenuControlHandler != null)
                {
                    //	Note that the context menu is showing.
                    StartShowContextMenu();

                    // create the mini-form
                    CommandContextMenuMiniForm miniForm = new CommandContextMenuMiniForm(Win32WindowImpl.ForegroundWin32Window, Command);

                    miniForm.Location = PositionMenu(menuLocation, alternativeLocation, miniForm.Size);
                    miniForm.Closed += new EventHandler(miniForm_Closed);
                    miniForm.Show();
                }
                else if (Command.CommandBarButtonContextMenuHandler != null)
                {
                    StartShowContextMenu();
                    Command.CommandBarButtonContextMenuHandler(Parent, menuLocation, alternativeLocation, new EndShowContextMenuDisposable(this));
                }
            }

        }

        public static Point PositionMenu(Point menuLocation, int alternativeLocation, Size menuSize)
        {
            if (!BidiHelper.IsRightToLeft)
            {
                int top = menuLocation.Y;
                int left = menuLocation.X;
                if (!Screen.FromPoint(new Point(alternativeLocation, menuLocation.Y)).Bounds.Contains(new Point(left + menuSize.Width, top)))
                    left = alternativeLocation - menuSize.Width;
                return new Point(left, top);
            }
            else
            {
                int top = menuLocation.Y;
                int left = alternativeLocation - menuSize.Width;
                if (!Screen.FromPoint(new Point(alternativeLocation, menuLocation.Y)).Bounds.Contains(new Point(left, top)))
                    left = menuLocation.X;
                return new Point(left, top);
            }
        }

        private class EndShowContextMenuDisposable : IDisposable
        {
            private readonly CommandBarButtonLightweightControl control;
            private bool disposed = false;

            public EndShowContextMenuDisposable(CommandBarButtonLightweightControl control)
            {
                this.control = control;
            }

            public void Dispose()
            {
                if (!disposed)
                {
                    disposed = true;
                    control.EndShowContextMenu();
                }
            }
        }

        // cleanup when the mini form closes
        private void miniForm_Closed(object sender, EventArgs e)
        {
            // cleanup form
            CommandContextMenuMiniForm miniForm = sender as CommandContextMenuMiniForm;
            miniForm.Closed -= new EventHandler(miniForm_Closed);
            miniForm.Dispose();

            // cleanup ui
            EndShowContextMenu();
        }

        /// <summary>
        /// Initialize state/UI for context menu
        /// </summary>
        private void StartShowContextMenu()
        {
            ContextMenuShowing = true;
        }

        /// <summary>
        /// Make sure the state and UI is cleaned up after showing the context menu
        /// </summary>
        private void EndShowContextMenu()
        {
            //	Note that the context menu is not showing and ensure that the mouse is not inside.
            ContextMenuShowing = false;
            MouseInside = false;

            // if requested, invalidate the parent so that the command bar repaints
            // correctly (this is necessary for the IE ToolBand which won't paint
            // its background correctly unless the entire control is invalidated)
            if (Command != null)
            {
                if (Command.CommandBarButtonContextMenuInvalidateParent)
                    Parent.Invalidate(true);
            }
        }

        /// <summary>
        /// Returns a color matrix that will draw buttons in a high-contrast-friendly mode.
        /// </summary>
        private static ColorMatrix HighContrastColorMatrix
        {
            get
            {
                if (_highContrastColorMatrix == null)
                    _highContrastColorMatrix = ImageHelper.GetHighContrastImageMatrix();
                return _highContrastColorMatrix;
            }
        }
        private static ColorMatrix _highContrastColorMatrix;

        #endregion Private Methods

        #region Private Event Handlers

        /// <summary>
        /// commandBarLightweightControl_CommandManagerChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void commandBarLightweightControl_CommandManagerChanged(object sender, EventArgs e)
        {
            UpdateCommand();
        }

        /// <summary>
        /// command_CommandBarButtonTextChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void command_CommandBarButtonTextChanged(object sender, EventArgs e)
        {
            commandBarLightweightControl.PerformLayout();
            Parent.Invalidate();
        }

        /// <summary>
        /// command_ShowCommandBarButtonContextMenu event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void command_ShowCommandBarButtonContextMenu(object sender, EventArgs e)
        {
            DoShowContextMenu();
        }

        /// <summary>
        /// command_StateChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void command_StateChanged(object sender, EventArgs e)
        {
            AccessibleName = command != null ? command.Text : null;
            Visible = command.On && command.VisibleOnCommandBar;

            if (lastLayoutCommandState == null || lastLayoutCommandState.NeedsLayout(Command, ImageSize))
                commandBarLightweightControl.PerformLayout();
            Parent.Invalidate();
        }

        #endregion Private Event Handlers

        /// <summary>
        /// Commands can implement this interface to provide
        /// dynamic command bar button images.
        /// </summary>
        public interface ICustomButtonBitmapPaint
        {
            int Width { get; }
            int Height { get; }
            void Paint(BidiGraphics g, Rectangle bounds, DrawState drawState);
        }

        public override bool DoDefaultAction()
        {
            if (ContextMenuUserInterface || DropDownContextMenuUserInterface)
            {
                DoShowContextMenu();
                return true;
            }
            //	Execute the event.
            if (Command != null && Command.On && Command.Enabled)
            {
                Command.PerformExecute();
            }
            return true;
        }

        public override bool Focus()
        {
            return base.Focus();
        }

        public override bool Unfocus()
        {
            return base.Unfocus();
        }
    }
}
