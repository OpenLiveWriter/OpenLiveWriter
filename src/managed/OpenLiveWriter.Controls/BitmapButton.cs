// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Security.Permissions;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.UI;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.Controls
{
    #region Public Enumerations

    /// <summary>
    /// The text alignment.
    /// </summary>
    public enum TextAlignment
    {
        /// <summary>
        /// Button text appears to the right of the button bitmap.
        /// </summary>
        Right,

        /// <summary>
        /// Button text appears to the bottom of the button bitmap.
        /// </summary>
        Bottom
    }

    /// <summary>
    /// The button style.
    /// </summary>
    public enum ButtonStyle
    {
        /// <summary>
        /// The button face is drawn by the control with a three-dimensional appearance.
        /// </summary>
        Standard,

        /// <summary>
        /// The button face is drawn by the control with a flat appearance.
        /// </summary>
        Flat,

        /// <summary>
        /// The button face is not drawn by the control.
        /// </summary>
        Bitmap
    }

    #endregion Public Enumerations

    /// <summary>
    /// CommandBar button lightweight control.
    /// </summary>
    public class BitmapButton : UserControl
    {
        #region Private Enumerations

        /// <summary>
        /// The button draw state.
        /// </summary>
        private enum DrawState
        {
            Disabled,
            Enabled,
            Selected,
            Latched,
            Pushed
        }

        #endregion Private Enumerations

        #region Static & Constant Declarations

        /// <summary>
        /// The darken inset.
        /// </summary>
        private const int DARKEN_INSET = 2;

        /// <summary>
        /// The bitmap pad.
        /// </summary>
        private const int BITMAP_PAD = 3;

        /// <summary>
        /// The horizontal text pad.
        /// </summary>
        private const int HORIZONTAL_TEXT_PAD = 3;

        /// <summary>
        /// The vertical text pad.
        /// </summary>
        private const int VERTICAL_TEXT_PAD = -1;

        /// <summary>
        /// The pushed offset.
        /// </summary>
        private const int PUSHED_OFFSET = 1;

        /// <summary>
        /// The size of a piece of a fave bitmap.
        /// </summary>
        private const int PIECE_SIZE = 8;

        /// <summary>
        /// The face color.
        /// </summary>
        private static readonly Color faceColor = Color.FromArgb(GraphicsHelper.Opacity(66), 246, 246, 246);

        [ThreadStatic]
        private static BorderPaint buttonFaceBorder;
        [ThreadStatic]
        private static BorderPaint pushedButtonFaceBorder;

        #endregion Static & Constant Declarations

        #region Private Member Variables & Declarations

        /// <summary>
        /// Required designer interface.
        /// </summary>
        private IContainer components;

        /// <summary>
        /// A value which indicates how the BitmapButton should automatically determine its
        /// width.
        /// </summary>
        private bool autoSizeWidth;

        /// <summary>
        /// A value which indicates how the BitmapButton should automatically determine its
        /// height.
        /// </summary>
        private bool autoSizeHeight;

        /// <summary>
        /// A value which how the button is drawn by the control.
        /// </summary>
        private ButtonStyle buttonStyle = ButtonStyle.Standard;

        /// <summary>
        /// A value which indicates whether clicking on the control shifts focus to it.
        /// </summary>
        private bool clickSetsFocus;

        /// <summary>
        /// The button bitmap for the enabled state.
        /// </summary>
        private Bitmap bitmapEnabled;

        /// <summary>
        /// The text alignment for the button.
        /// </summary>
        private TextAlignment textAlignment = TextAlignment.Right;

        /// <summary>
        /// The text for the button.
        /// </summary>
        private string buttonText;

        /// <summary>
        /// The ToolTip.
        /// </summary>
        private ToolTip2 toolTip;

        /// <summary>
        /// The bitmap layout rectangle.
        /// </summary>
        private Rectangle bitmapRectangle;

        /// <summary>
        /// The text layout rectangle.
        /// </summary>
        private Rectangle textRectangle;

        /// <summary>
        /// A value indicating whether the button is latched.
        /// </summary>
        private bool latched;

        /// <summary>
        /// A value indicating whether a key press has pushed the button.
        /// </summary>
        private bool pushedByKeystroke;

        /// <summary>
        /// A value indicating whether the mouse is inside the control.
        /// </summary>
        private bool mouseInside;

        /// <summary>
        /// A value indicating whether the left mouse button is down.
        /// </summary>
        private bool leftMouseDown;

        /// <summary>
        /// A value indicating whether the right mouse button is down.
        /// </summary>
        private bool rightMouseDown;

        private bool useVirtualTransparency;

        private bool allowMirroring;

        #endregion Private Member Variables & Declarations

        #region Class Initialization & Termination

        /// <summary>
        /// Static initialization of the BitmapButton class.
        /// </summary>
        static BitmapButton()
        {
            //	Initialize the string format.

        }

        /// <summary>
        /// Initializes a new instance of the BitmapButton class.
        /// </summary>
        /// <param name="container"></param>
        public BitmapButton(IContainer container)
        {
            // Required for Windows.Forms Class Composition Designer support
            container.Add(this);
            InitializeComponent();

            //	Do common initialization.
            InitializeObject();
        }

        /// <summary>
        /// Initializes a new instance of the BitmapButton class.
        /// </summary>
        public BitmapButton()
        {
            // Required for Windows.Forms Class Composition Designer support
            InitializeComponent();

            //	Do common initialization.
            InitializeObject();
        }

        /// <summary>
        /// Common initialization.
        /// </summary>
        private void InitializeObject()
        {
            //	Turn on double buffered painting.
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            //	Redraw if resized (mainly for the designer).
            SetStyle(ControlStyles.ResizeRedraw, true);

            //  Allow transparent background color
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            AccessibleRole = AccessibleRole.PushButton;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
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
            //
            // BitmapButton
            //
            this.Name = "BitmapButton";
            this.Size = new System.Drawing.Size(72, 64);

        }
        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a value which indicates whether the BitmapButton should automatically
        /// determine its size.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(false),
                Description("Specifies whether the BitmapButton should automcatically determine its width.")
        ]
        public bool AutoSizeWidth
        {
            get
            {
                return autoSizeWidth;
            }
            set
            {
                if (autoSizeWidth != value)
                {
                    autoSizeWidth = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value which indicates whether the BitmapButton should automatically
        /// determine its size.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(false),
                Description("Specifies whether the BitmapButton should automcatically determine its height.")
        ]
        public bool AutoSizeHeight
        {
            get
            {
                return autoSizeHeight;
            }
            set
            {
                if (autoSizeHeight != value)
                {
                    autoSizeHeight = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// A value which indicates how the button is drawn by the control.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(ButtonStyle.Standard),
                Description("Specifies how the button is drawn by the control.")
        ]
        public ButtonStyle ButtonStyle
        {
            get
            {
                return buttonStyle;
            }
            set
            {
                if (buttonStyle != value)
                {
                    buttonStyle = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the button bitmap for the disabled state.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(true),
                DefaultValue(null),
                Description("Specifies the button bitmap for the disabled state.")
        ]
        public Bitmap BitmapDisabled { get; set; }

        /// <summary>
        /// Gets or sets the bitmap for the enabled state.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(true),
                DefaultValue(null),
                Description("Specifies the button bitmap for the enabled state.")
        ]
        public Bitmap BitmapEnabled
        {
            get
            {
                return bitmapEnabled;
            }

            set
            {
                bitmapEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets the bitmap for the selected state.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(true),
                DefaultValue(null),
                Description("Specifies the bitmap for the selected state.")
        ]
        public Bitmap BitmapSelected { get; set; }

        /// <summary>
        /// Gets or sets the bitmap for the pushed state.
        /// </summary>
        [
            Category("Appearance.OwnerDraw"),
                Localizable(true),
                DefaultValue(null),
                Description("Specifies the bitmap for the pushed state.  Used only when ButtonStyle.Bitmap is specified.")
        ]

        public Bitmap BitmapPushed { get; set; }

        /// <summary>
        /// Gets or sets the text alignment for the button.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(TextAlignment.Right),
                Description("Specifies the default text alignment for the button.")
        ]
        public TextAlignment TextAlignment
        {
            get
            {
                return textAlignment;
            }
            set
            {
                if (textAlignment != value)
                {
                    textAlignment = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the button text.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(true),
                DefaultValue(null),
                Description("Specifies the button text.  Used only when ButtonStyle.Bitmap is not specified.")
        ]
        public string ButtonText
        {
            get
            {
                return buttonText;
            }
            set
            {
                if (buttonText != value)
                {
                    buttonText = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the ToolTip.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(true),
                Description("Specifies the ToolTip text.")
        ]
        public string ToolTip
        {
            get
            {
                return toolTip?.GetToolTip(this);
            }

            set
            {
                if (toolTip == null)
                {
                    // we need to instantiate this tooltip lazily because it
                    // causes memory leaks if the form it lives on is not collected,
                    // which is the case for the property shelf.
                    toolTip = new ToolTip2(components);
                }

                toolTip.SetToolTip(this, value);
            }
        }

        /// <summary>
        /// Gets or sets the ToolTip.
        /// </summary>
        [
            Category("Behavior"),
                Localizable(false),
                DefaultValue(false),
                Description("Specifies whether clicking on the BitmapButton causes it it receive keyboard focus.")
        ]
        public bool ClickSetsFocus
        {
            get
            {
                return clickSetsFocus;
            }

            set
            {
                clickSetsFocus = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the button is latched.
        /// </summary>
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public bool Latched
        {
            get
            {
                return latched;
            }

            set
            {
                if (latched != value)
                {
                    latched = value;
                    Invalidate();
                }
            }
        }

        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public bool UseVirtualTransparency
        {
            get
            {
                return useVirtualTransparency;
            }
            set
            {
                if (useVirtualTransparency != value)
                {
                    useVirtualTransparency = value;
                    Invalidate();
                }
            }
        }

        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public bool AllowMirroring
        {
            get
            {
                return allowMirroring;
            }
            set
            {
                if (allowMirroring != value)
                {
                    allowMirroring = value;
                    Invalidate();
                }
            }
        }

        #endregion

        #region Protected Event Overrides

        /// <summary>
        /// Raises the ChangeUICues event.
        /// </summary>
        /// <param name="e">A UICuesEventArgs that contains the event data.</param>
        protected override void OnChangeUICues(UICuesEventArgs e)
        {
            //	Track whether focus is being shown.
            if (e.ChangeFocus)
            {
                Invalidate();
            }

            //	Call the base class's method so that registered delegates receive the event.
            base.OnChangeUICues(e);
        }

        /// <summary>
        /// Raises the Click event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnClick(EventArgs e)
        {
            //	If we're pushed, raise the event; otherwise, suppress it.
            if (Pushed)
                base.OnClick(e);
        }

        /// <summary>
        /// Raises the EnabledChanged event,
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnEnabledChanged(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnEnabledChanged(e);

            //	Update the control.
            Invalidate();
        }

        /// <summary>
        /// Raises the GotFocus event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnGotFocus(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnGotFocus(e);

            //	Update the control.
            Invalidate();
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLayout(e);

            //	If the ButtonStyle is Bitmap, layout the bitmap button.
            if (ButtonStyle == ButtonStyle.Bitmap)
            {
                //	Set the bitmap rectangle.
                if (bitmapEnabled == null)
                    bitmapRectangle = Rectangle.Empty;
                else
                    bitmapRectangle = new Rectangle(Utility.CenterMinZero(ScaleX(bitmapEnabled.Width), Width),
                        Utility.CenterMinZero(ScaleY(bitmapEnabled.Height), Height),
                        ScaleX(bitmapEnabled.Width),
                        ScaleY(bitmapEnabled.Height));

                //	Set the text rectangle to be empty (no text is drawn for a ButtonStyle of Bitmap).
                textRectangle = Rectangle.Empty;
            }
            else
            {
                //	Handle right aligned text.
                if (TextAlignment == TextAlignment.Right)
                {
                    //	Set the bitmap rectangle.
                    if (bitmapEnabled == null)
                        bitmapRectangle = Rectangle.Empty;
                    else if (buttonText == null)
                        bitmapRectangle = new Rectangle(Utility.CenterMinZero(ScaleX(bitmapEnabled.Width), Width),
                            Utility.CenterMinZero(ScaleY(bitmapEnabled.Height), Height),
                            ScaleX(bitmapEnabled.Width),
                            ScaleY(bitmapEnabled.Height));
                    else
                        bitmapRectangle = new Rectangle(BITMAP_PAD,
                            Utility.CenterMinZero(ScaleY(bitmapEnabled.Height), Height),
                            ScaleX(bitmapEnabled.Width),
                            ScaleY(bitmapEnabled.Height));

                    //	Set the text rectangle.
                    if (buttonText == null)
                        textRectangle = Rectangle.Empty;
                    else
                        textRectangle = new Rectangle(bitmapRectangle.Right,
                            Utility.CenterMinZero(Font.Height, Height) + 1,
                            Width - bitmapRectangle.Right,
                            Font.Height);
                }
                //	Handle bottom aligned text.
                else if (TextAlignment == TextAlignment.Bottom)
                {
                    //	Set the bitmap rectangle.
                    if (bitmapEnabled == null)
                        bitmapRectangle = Rectangle.Empty;
                    else
                        bitmapRectangle = new Rectangle(Utility.CenterMinZero(ScaleX(bitmapEnabled.Width), Width),
                            Utility.CenterMinZero(ScaleY(bitmapEnabled.Height), Height - Font.Height),
                            ScaleX(bitmapEnabled.Width),
                            ScaleY(bitmapEnabled.Height));

                    //	Set the text rectangle.
                    if (buttonText == null)
                        textRectangle = Rectangle.Empty;
                    else
                    {
                        textRectangle = new Rectangle(ScaleX(HORIZONTAL_TEXT_PAD),
                            bitmapRectangle.Bottom + ScaleY(VERTICAL_TEXT_PAD),
                            Width - ScaleX((HORIZONTAL_TEXT_PAD * 2)),
                            Font.Height);
                    }
                }
            }
        }

        /// <summary>
        /// Raises the LostFocus event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLostFocus(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLostFocus(e);

            //	Update the control.
            Invalidate();
        }

        /// <summary>
        /// Raises the MouseEnter event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseEnter(e);

            //	Set the MouseInside property.
            MouseInside = true;
        }

        /// <summary>
        /// Raises the MouseDown event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseDown(e);

            //	Get focus, if should and can and don't have it already.
            if (clickSetsFocus && CanFocus && !Focused)
                Focus();

            //	Left or right down.
            if (e.Button == MouseButtons.Left)
                LeftMouseDown = true;
            else if (e.Button == MouseButtons.Right)
                RightMouseDown = true;
        }

        /// <summary>
        /// Raises the MouseLeave event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseLeave(e);

            //	Set the MouseInside property.
            MouseInside = false;
            LeftMouseDown = RightMouseDown = false;
        }

        /// <summary>
        /// Raises the MouseMove event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseMove(e);

            //	Update the state of the MouseInside property if the LeftMouseButtonDown property
            //	is true.
            if (Pushed)
                MouseInside = ClientRectangle.Contains(e.X, e.Y);
        }

        /// <summary>
        /// Raises the MouseUp event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseUp(e);

            //	Left or right up.
            if (e.Button == MouseButtons.Left)
                LeftMouseDown = false;
            else if (e.Button == MouseButtons.Right)
                RightMouseDown = false;
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (useVirtualTransparency && BackColor == Color.Transparent)
            {
                VirtualTransparency.VirtualPaint(this, pevent);
                return;
            }
            base.OnPaintBackground(pevent);
        }

        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnPaint(e);

            BidiGraphics g = new BidiGraphics(e.Graphics, ClientRectangle);

            //	Determine the draw state of the button.
            DrawState drawState;
            if (!Enabled)
                drawState = DrawState.Disabled;
            else if (Latched)
                drawState = DrawState.Latched;
            else if (Pushed && MouseInside)
                drawState = DrawState.Pushed;
            else if (MouseInside)
                drawState = DrawState.Selected;
            else
                drawState = DrawState.Enabled;

            //	Draw the button face, as needed, and select the button bitmap to draw below.
            Bitmap buttonBitmap = null;
            switch (drawState)
            {
                case DrawState.Disabled:
                    buttonBitmap = BitmapDisabled;
                    break;

                case DrawState.Enabled:
                    if (Focused)
                    {
                        if (ButtonStyle == ButtonStyle.Standard)
                            DrawStandardButtonFace(e.Graphics);
                        else if (ButtonStyle == ButtonStyle.Flat)
                            DrawFlatButtonFace(e.Graphics);
                    }

                    buttonBitmap = BitmapEnabled;
                    break;

                case DrawState.Selected:
                    if (ButtonStyle == ButtonStyle.Standard)
                        DrawStandardButtonFace(e.Graphics);
                    else if (ButtonStyle == ButtonStyle.Flat)
                        DrawFlatButtonFace(e.Graphics);
                    buttonBitmap = BitmapSelected;
                    break;

                case DrawState.Latched:
                case DrawState.Pushed:
                    if (ButtonStyle == ButtonStyle.Standard)
                    {
                        DrawStandardButtonFacePushed(e.Graphics);
                        buttonBitmap = BitmapSelected;
                    }
                    else if (ButtonStyle == ButtonStyle.Flat)
                    {
                        DrawFlatButtonFacePushed(e.Graphics);
                        buttonBitmap = BitmapSelected;
                    }
                    else if (ButtonStyle == ButtonStyle.Bitmap)
                        buttonBitmap = BitmapPushed;
                    break;
            }

            //	Draw the button bitmap, if there is one.
            if (buttonBitmap != null)
            {
                //	Set the rectangle into which we'll draw the bitmap.
                Rectangle drawBitmapRectangle = bitmapRectangle;
                //				if (ButtonStyle != ButtonStyle.Bitmap && drawState == DrawState.Pushed)
                //					drawBitmapRectangle.Offset(ScaleX(PUSHED_OFFSET), ScaleY(PUSHED_OFFSET));

                //	Draw the bitmap.
                var destRect = new Rectangle(drawBitmapRectangle.X, drawBitmapRectangle.Y, ScaleX(buttonBitmap.Width), ScaleY(buttonBitmap.Height));
                g.DrawImage(AllowMirroring, buttonBitmap, destRect);
            }

            //	If focus is being shown, and we're focused, draw the focus rectangle.
            if (ShowFocusCues && Focused)
            {
                Rectangle rectangle = ClientRectangle;
                if (ButtonStyle == ButtonStyle.Flat)
                {
                    //offset the focus a bit to ensure it draws directly over the flat border
                    rectangle = new Rectangle(rectangle.Location, new Size(ScaleX(rectangle.Width - 1), ScaleY(rectangle.Height - 1)));

                    //clear the background of the rectangle so it contrasts well with the flat selection
                    using (Pen p = new Pen(BackColor))
                        g.DrawRectangle(p, rectangle);
                }

                ControlPaint.DrawFocusRectangle(e.Graphics, rectangle);
            }

            //	Draw the button text, if there is some.
            if (ButtonStyle != ButtonStyle.Bitmap && buttonText != null)
            {
                //	Set the rectangle into which we'll draw the text.
                Rectangle drawTextRectangle = textRectangle;
                drawTextRectangle.Offset(HORIZONTAL_TEXT_PAD, VERTICAL_TEXT_PAD);
                if (drawState == DrawState.Pushed)
                    drawTextRectangle.Offset(ScaleX(PUSHED_OFFSET), ScaleY(PUSHED_OFFSET));

                TextFormatFlags textFormat;
                if (ShowKeyboardCues)
                    textFormat = TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.WordBreak | TextFormatFlags.ExpandTabs | TextFormatFlags.EndEllipsis;
                else
                    textFormat = TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.WordBreak | TextFormatFlags.ExpandTabs | TextFormatFlags.EndEllipsis | TextFormatFlags.HidePrefix;

                //	Set the text color.
                Color textColor = ForeColor;
                if (!Enabled)
                    textColor = Color.FromArgb(GraphicsHelper.Opacity(50), textColor);

                //	Draw the text.
                g.DrawText(buttonText,
                                Font,
                                drawTextRectangle,
                                textColor,
                                textFormat);
            }
        }

        #endregion Protected Event Overrides

        #region Protected Methods

        /// <summary>
        /// Processes a command key.
        /// </summary>
        /// <param name="msg">A Message, passed by reference, that represents the window message to process.</param>
        /// <param name="keyData">One of the Keys values that represents the key to process.</param>
        /// <returns>true if the character was processed by the control; otherwise, false.</returns>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys key = keyData & ~(Keys.Control | Keys.Shift);
            if (IsPushKey(key))
            {
                PushByKeystroke();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected virtual bool IsPushKey(Keys key)
        {
            return key == Keys.Space;
        }

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
            //	If AutoSizeHeight is true, adjust height value as needed.
            if (AutoSizeHeight && (specified & BoundsSpecified.Height) != 0)
            {
                if (ButtonStyle == ButtonStyle.Bitmap)
                {
                    //	Adjust for a ButtonStyle of Bitmap.
                    height = bitmapEnabled == null ? 0 : ScaleY(bitmapEnabled.Height);
                }
                else
                {
                    //	Adjust for a ButtonStyle of Standard or Flat.
                    if (TextAlignment == TextAlignment.Right || TextAlignment == TextAlignment.Bottom)
                    {
                        height = bitmapEnabled == null ? 0 : ScaleY((BITMAP_PAD * 2) + bitmapEnabled.Height);

                        if (!String.IsNullOrEmpty(buttonText))
                        {
                            // The Font.Height is automatically scaled.
                            int paddedTextHeight = ScaleY((VERTICAL_TEXT_PAD * 2)) + Font.Height;

                            if (TextAlignment == TextAlignment.Right && paddedTextHeight > height)
                            {
                                height = paddedTextHeight;
                            }
                            else if (TextAlignment == TextAlignment.Bottom)
                            {
                                height += paddedTextHeight;
                            }
                        }
                    }
                }

                height = ScaleY(height);
            }

            //	If AutoSizeHeight is true, adjust height value as needed.
            if (AutoSizeWidth && (specified & BoundsSpecified.Width) != 0)
            {
                if (ButtonStyle == ButtonStyle.Bitmap)
                {
                    width = bitmapEnabled == null ? 0 : ScaleX(bitmapEnabled.Width);
                }
                else
                {
                    //	Adjust for a ButtonStyle of Standard or Flat.
                    if (TextAlignment == TextAlignment.Right || TextAlignment == TextAlignment.Bottom)
                    {
                        width = bitmapEnabled == null ? 0 : ScaleX(BITMAP_PAD + bitmapEnabled.Width);

                        if (String.IsNullOrEmpty(buttonText) && width != 0)
                        {
                            width += ScaleX(BITMAP_PAD);
                        }
                        else if (!String.IsNullOrEmpty(buttonText))
                        {
                            using (Graphics graphics = CreateGraphics())
                            {
                                // TextRenderer.MeasureText is automatically scaled.
                                Size buttonTextSize = TextRenderer.MeasureText(graphics, buttonText, Font);
                                int buttonTextWidth = buttonTextSize.Width + ScaleX(HORIZONTAL_TEXT_PAD * 2);

                                if (TextAlignment == TextAlignment.Right)
                                {
                                    width += buttonTextWidth;
                                }
                                else if (TextAlignment == TextAlignment.Bottom && buttonTextWidth > width)
                                {
                                    width = buttonTextWidth;
                                }
                            }
                        }
                    }
                }
            }

            //	Call the base class's method.
            base.SetBoundsCore(x, y, width, height, specified);
        }

        #endregion

        #region Private Properties

        private BorderPaint ButtonFaceBorder
        {
            get
            {
                if (buttonFaceBorder == null)
                {
                    buttonFaceBorder = new BorderPaint(
                        ResourceHelper.LoadAssemblyResourceBitmap("Images.Application.BitmapButtonFace.png"),
                        false,
                        BorderPaintMode.Cached | BorderPaintMode.StretchToFill | BorderPaintMode.PaintMiddleCenter,
                        3, 4, 3, 71);
                }
                return buttonFaceBorder;
            }
        }

        private BorderPaint PushedButtonFaceBorder
        {
            get
            {
                if (pushedButtonFaceBorder == null)
                {
                    pushedButtonFaceBorder = new BorderPaint(
                        ResourceHelper.LoadAssemblyResourceBitmap("Images.Application.BitmapButtonFacePushed.png"),
                        false,
                        BorderPaintMode.Cached | BorderPaintMode.StretchToFill | BorderPaintMode.PaintMiddleCenter,
                        3, 4, 3, 71);
                }
                return pushedButtonFaceBorder;
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
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the left mouse button is down.
        /// </summary>
        private bool LeftMouseDown
        {
            get
            {
                return leftMouseDown;
            }
            set
            {
                if (leftMouseDown != value)
                {
                    leftMouseDown = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the right mouse button is down.
        /// </summary>
        private bool RightMouseDown
        {
            get
            {
                return rightMouseDown;
            }
            set
            {
                if (rightMouseDown != value)
                {
                    rightMouseDown = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the button is pushed.
        /// </summary>
        private bool Pushed
        {
            get
            {
                return pushedByKeystroke || (LeftMouseDown && !RightMouseDown);
            }
        }

        #endregion Private Properties

        #region Private Methods

        /// <summary>
        /// Helper method to push the button by keystroke.
        /// </summary>
        private void PushByKeystroke()
        {
            pushedByKeystroke = true;
            Invalidate();
            Update();

            OnClick(EventArgs.Empty);

            pushedByKeystroke = false;
            Invalidate();
            Update();
        }

        /// <summary>
        /// Draw the standard button face.
        /// </summary>
        /// <param name="graphics">Graphics context in which the standard button face is to be drawn.</param>
        private void DrawStandardButtonFace(Graphics graphics)
        {
            //	Draw the border.
            ButtonFaceBorder.DrawBorder(graphics, ClientRectangle);

            //	Fill the button face.
            FillStandardButtonFace(graphics);
        }

        /// <summary>
        /// Draw standard button face in the pushed state.
        /// </summary>
        /// <param name="graphics">Graphics context in which the standard button face is to be drawn in the pushed state.</param>
        private void DrawStandardButtonFacePushed(Graphics graphics)
        {
            //	Draw the border.
            PushedButtonFaceBorder.DrawBorder(graphics, ClientRectangle);

            //	Fill the button face.
            FillStandardButtonFace(graphics);
        }

        /// <summary>
        /// Helper to fill the button face.
        /// </summary>
        /// <param name="graphics">Graphics context in which the button face is to be filled.</param>
        private void FillStandardButtonFace(Graphics graphics)
        {
            using (SolidBrush solidBrush = new SolidBrush(faceColor))
                graphics.FillRectangle(solidBrush, PIECE_SIZE, PIECE_SIZE, Width - (PIECE_SIZE * 2), Height - (PIECE_SIZE * 2));
        }

        /// <summary>
        /// Draw the flat button face.
        /// </summary>
        /// <param name="graphics">Graphics context in which the flat button face is to be drawn.</param>
        private void DrawFlatButtonFace(Graphics graphics)
        {
            //	Fill the bounds with a 25% opaque version of the selection color.
            using (SolidBrush solidBrush = new SolidBrush(Color.FromArgb(GraphicsHelper.Opacity(15), SystemColors.Highlight)))
                graphics.FillRectangle(solidBrush, ClientRectangle);

            //	Draw a rectangle around the section area using.
            using (Pen pen = new Pen(Color.FromArgb(GraphicsHelper.Opacity(35), SystemColors.Highlight)))
                graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }

        /// <summary>
        /// Draw the flat button face in the pushed state.
        /// </summary>
        /// <param name="graphics">Graphics context in which the flat button face is to be drawn in the pushed.</param>
        private void DrawFlatButtonFacePushed(Graphics graphics)
        {
            //	Fill the bounds with a 25% opaque version of the selection color.
            using (SolidBrush solidBrush = new SolidBrush(Color.FromArgb(GraphicsHelper.Opacity(30), SystemColors.Highlight)))
                graphics.FillRectangle(solidBrush, ClientRectangle);

            //	Draw a rectangle around the section area using.
            using (Pen pen = new Pen(Color.FromArgb(GraphicsHelper.Opacity(70), SystemColors.Highlight)))
                graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }

        #endregion Private Methods

        #region High DPI Scaling
        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            SaveScale(factor.Width, factor.Height);
            base.ScaleControl(factor, specified);
        }

        protected override void ScaleCore(float dx, float dy)
        {
            SaveScale(dx, dy);
            base.ScaleCore(dx, dy);
        }

        private void SaveScale(float dx, float dy)
        {
            scale = new PointF(scale.X * dx, scale.Y * dy);
        }

        private PointF scale = new PointF(1f, 1f);

        protected int ScaleX(int x)
        {
            return (int)(x * scale.X);
        }

        protected int ScaleY(int y)
        {
            return (int)(y * scale.Y);
        }
        #endregion
    }
}
