// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Provides a tab selector lightweight control (i.e. the tab itself).
    /// </summary>
    internal class TabSelectorLightweightControl : LightweightControl
    {
        /// <summary>
        /// Pad space, in pixels.  This value is used to provide a bit of "air" around the visual
        /// elements of the tab selected lightweight control.
        /// </summary>
        private const int PAD = TabLightweightControl.PAD;

        /// <summary>
        ///	The maximum tab text width.
        /// </summary>
        private const int MAXIMUM_TAB_TEXT_WIDTH = 200;

        /// <summary>
        ///	The minimum tab interior width.
        /// </summary>
        private const int MINIMUM_TAB_INTERIOR_WIDTH = 4;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components;

        /// <summary>
        /// The tab entry.
        /// </summary>
        private TabEntry tabEntry;

        /// <summary>
        /// Gets the tab entry.
        /// </summary>
        public TabEntry TabEntry
        {
            get
            {
                return tabEntry;
            }
        }

        /// <summary>
        /// A value indicating whether tabs should be small.
        /// </summary>
        private bool smallTabs = false;

        /// <summary>
        /// Gets or sets a value indicating whether tabs should be small.
        /// </summary>
        public bool SmallTabs
        {
            get
            {
                return smallTabs;
            }
            set
            {
                smallTabs = value;
            }
        }

        /// <summary>
        /// A value indicating whether the tab text is shown.
        /// </summary>
        private bool showTabText = true;

        /// <summary>
        /// Gets or sets a value indicating whether the tab text is shown.
        /// </summary>
        public bool ShowTabText
        {
            get
            {
                return showTabText;
            }
            set
            {
                showTabText = value;
            }
        }

        /// <summary>
        /// A value indicating whether the tab bitmap is shown.
        /// </summary>
        private bool showTabBitmap = true;

        /// <summary>
        /// Gets or sets a value indicating whether the tab bitmap is shown.
        /// </summary>
        public bool ShowTabBitmap
        {
            get
            {
                return showTabBitmap;
            }
            set
            {
                showTabBitmap = value;
            }
        }

        /// <summary>
        /// The unselected virtual size.
        /// </summary>
        private Size unselectedVirtualSize;

        /// <summary>
        /// Gets or sets the unselected virtual size.
        /// </summary>
        public Size UnselectedVirtualSize
        {
            get
            {
                return unselectedVirtualSize;
            }
            set
            {
                unselectedVirtualSize = value;
            }
        }

        private TextFormatFlags textFormatFlags;

        /// <summary>
        /// The bitmap rectangle.
        /// </summary>
        private Rectangle bitmapRectangle;

        /// <summary>
        /// The text layout rectangle.  This is the rectangle into which the text is measured and
        /// drawn.  It is not the actual text rectangle.
        /// </summary>
        private Rectangle textLayoutRectangle;

        /// <summary>
        /// The selected event.
        /// </summary>
        public event EventHandler Selected;

        /// <summary>
        /// Initializes a new instance of the TabSelectorLightweightControl class.
        /// </summary>
        public TabSelectorLightweightControl(IContainer container)
        {
            //	Shut up!
            if (components == null)
                components = null;

            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            InitializeComponent();

            //	Initialize the object.
            InitializeObject();
        }

        /// <summary>
        /// Initializes a new instance of the TabSelectorLightweightControl class.
        /// </summary>
        public TabSelectorLightweightControl()
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            InitializeComponent();

            //	Initialize the object.
            InitializeObject();
        }

        /// <summary>
        /// Initializes a new instance of the TabSelectorLightweightControl class.
        /// </summary>
        public TabSelectorLightweightControl(TabEntry tabEntry)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            InitializeComponent();

            //	Set the tab entry.
            this.tabEntry = tabEntry;

            //	Initialize the object.
            InitializeObject();
        }

        /// <summary>
        /// Object initialization.
        /// </summary>
        private void InitializeObject()
        {
            //	Initialize the string format.
            textFormatFlags = TextFormatFlags.EndEllipsis | TextFormatFlags.PreserveGraphicsTranslateTransform;

            //set the custom accessibility values for this control
            AccessibleName = TabEntry.TabPageControl.TabText;
            AccessibleRole = AccessibleRole.PageTab;

            TabStop = true;
        }

        private LightweightControlAccessibleObject _accessibleObject;
        protected override AccessibleObject CreateAccessibilityInstance()
        {
            if (_accessibleObject == null)
            {
                _accessibleObject = (LightweightControlAccessibleObject)base.CreateAccessibilityInstance();
                // tabs are selectable role
                _accessibleObject.SetAccessibleStateOverride(TabEntry.IsSelected ? AccessibleStates.Selectable | AccessibleStates.Selected : AccessibleStates.Selectable);
            }

            return _accessibleObject;
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
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            //
            // TabSelectorLightweightControl
            //
            this.AllowDrop = true;
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }
        #endregion

        /// <summary>
        /// Raises the MouseDown event.
        /// </summary>
        /// <param name="e">An MouseEventArgs that contains the event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            //	Call the base class's method so registered delegates receive the event.
            base.OnMouseDown(e);

            //	Raise the Selected event.
            if (e.Button == MouseButtons.Left)
                OnSelected(EventArgs.Empty);
        }

        /// <summary>
        /// Raises the Layout event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLayout(EventArgs e)
        {
            //	Call the base class's method so registered delegates receive the event.
            base.OnLayout(e);

            //	No layout required if this control is not visible.
            if (Parent == null)// || Parent.Parent == null)
                return;

            //	Obtain the font we'll use to draw the tab selector lightweight control.
            Font font = TabEntry.TabPageControl.ApplicationStyle.NormalApplicationFont;

            //	Obtain the tab bitmap and tab text.
            Bitmap tabBitmap = TabEntry.TabPageControl.TabBitmap;
            string tabText = TabEntry.TabPageControl.TabText;

            //	The tab height.
            int tabHeight = 0;

            //	Adjust the tab height for the bitmap.
            if (tabBitmap != null && tabBitmap.Height > tabHeight)
                tabHeight = tabBitmap.Height;

            //	Adjust the tab height for the font.
            if (font.Height > tabHeight)
                tabHeight = font.Height;

            //	Pad the tab height.
            tabHeight += smallTabs ? PAD * 2 : PAD * 4;

            //	Set the initial tab width (padded).
            int tabWidth = PAD * 2;

            //	Reset the bitmap and text layout rectangles for the layout code below.
            bitmapRectangle = textLayoutRectangle = Rectangle.Empty;

            //	Layout the tab bitmap.
            if (showTabBitmap && tabBitmap != null)
            {
                tabWidth += PAD;
                //	Set the bitmap rectangle.
                bitmapRectangle = new Rectangle(tabWidth,
                                                Utility.CenterMinZero(tabBitmap.Height, tabHeight),
                                                tabBitmap.Width,
                                                tabBitmap.Height);

                //	Update the tab width.
                tabWidth += bitmapRectangle.Width;
                tabWidth += PAD;
            }

            //	Layout the tab text.
            if (showTabText && tabText != null && tabText.Length != 0)
            {
                //	Initialize the text layout rectangle.
                textLayoutRectangle = new Rectangle(tabWidth,
                                                    Utility.CenterMinZero(font.Height, tabHeight + 2),
                                                    MAXIMUM_TAB_TEXT_WIDTH,
                                                    font.Height);

                Size textSize = TextRenderer.MeasureText(
                    tabText,
                    font,
                    textLayoutRectangle.Size,
                    textFormatFlags);

                textLayoutRectangle.Size = textSize;

                //	Update the tab width.
                tabWidth = textLayoutRectangle.Right + PAD;
            }

            //	If the tab is black (neither the bitmap or name will fit, or neither is present),
            //	assure that it is laid out with a minimum interior width.
            if (bitmapRectangle.IsEmpty && textLayoutRectangle.IsEmpty)
                tabWidth += MINIMUM_TAB_INTERIOR_WIDTH;

            //	Pad the tab width.
            tabWidth += PAD * 2;

            //	Note the unselected virtual size (used by layout logic by TabLightweightControl).
            unselectedVirtualSize = new Size(tabWidth, tabHeight);

            //	If the tab entry is selected, make necessary adjustments to have it occlude
            //	surrounding tabs and miter itself into the tab page border.
            if (tabEntry.IsSelected)
            {
                tabHeight += smallTabs ? 1 : PAD * 2;

                // With localized tab text, we can't afford to waste any space
                /*
                tabWidth += PAD*4;
                if (!bitmapRectangle.IsEmpty)
                    bitmapRectangle.X += PAD*2;
                if (!textLayoutRectangle.IsEmpty)
                    textLayoutRectangle.X += PAD*2;
                */
            }

            //	Set the virtual size.
            VirtualSize = new Size(tabWidth, tabHeight);
        }

        #region Protected Event Overrides

        /// <summary>
        /// Raises the MouseEnter event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseEnter(e);

            //	Set the ToolTip.
            if (tabEntry.TabPageControl.TabToolTipText != null && tabEntry.TabPageControl.TabToolTipText.Length != 0)
                LightweightControlContainerControl.SetToolTip(tabEntry.TabPageControl.TabToolTipText);
            else if (!ShowTabText)
                LightweightControlContainerControl.SetToolTip(tabEntry.TabPageControl.TabText);
        }

        /// <summary>
        /// Raises the MouseLeave event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseLeave(e);

            //	Clear the ToolTip.
            if ((tabEntry.TabPageControl.TabToolTipText != null && tabEntry.TabPageControl.TabToolTipText.Length != 0)
                || !ShowTabText)
                LightweightControlContainerControl.SetToolTip(null);
        }

        #endregion

        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs args)
        {
            if (!TabEntry.Hidden)
            {
                //	Call the base class's method so that registered delegates receive the event.
                base.OnPaint(args);

                BidiGraphics g = new BidiGraphics(args.Graphics, VirtualClientRectangle);

                //	Draw the tab.
                DrawTab(g);

                //	Draw the tab bitmap.
                if (!bitmapRectangle.IsEmpty)
                    g.DrawImage(false, TabEntry.TabPageControl.TabBitmap, bitmapRectangle);

                //	Draw tab text.
                if (!textLayoutRectangle.IsEmpty)
                {
                    //	Select the text color to use.
                    Color textColor;
                    if (tabEntry.IsSelected)
                        textColor = TabEntry.TabPageControl.ApplicationStyle.ActiveTabTextColor;
                    else
                        textColor = TabEntry.TabPageControl.ApplicationStyle.InactiveTabTextColor;

                    Rectangle tempRect = textLayoutRectangle;
                    //	Draw the tab text.
                    g.DrawText(TabEntry.TabPageControl.TabText,
                        TabEntry.TabPageControl.ApplicationStyle.NormalApplicationFont,
                        tempRect,
                        textColor,
                        textFormatFlags);
                }
            }

            if (Focused)
                ControlPaint.DrawFocusRectangle(args.Graphics, VirtualClientRectangle, Parent.ForeColor, Parent.BackColor);
        }

        /// <summary>
        /// Raises the Selected event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnSelected(EventArgs e)
        {
            if (Selected != null)
                Selected(this, e);
        }

        /// <summary>
        /// Draws the tab.
        /// </summary>
        /// <param name="graphics">Graphics context where the tab page border is to be drawn.</param>
        private void DrawTab(BidiGraphics graphics)
        {
            //	Obtain the rectangle.
            Rectangle virtualClientRectangle = VirtualClientRectangle;

            //	Compute the face rectangle.
            Rectangle faceRectangle = new Rectangle(virtualClientRectangle.X + 1,
                                                    virtualClientRectangle.Y + 1,
                                                    virtualClientRectangle.Width - 2,
                                                    virtualClientRectangle.Height - (tabEntry.IsSelected ? 1 : 2));

            //	Fill face of the tab.
            Color topColor, bottomColor;
            if (tabEntry.IsSelected)
            {
                topColor = TabEntry.TabPageControl.ApplicationStyle.ActiveTabTopColor;
                bottomColor = TabEntry.TabPageControl.ApplicationStyle.ActiveTabBottomColor;
            }
            else
            {
                topColor = TabEntry.TabPageControl.ApplicationStyle.InactiveTabTopColor;
                bottomColor = TabEntry.TabPageControl.ApplicationStyle.InactiveTabBottomColor;
            }
            if (topColor == bottomColor)
                using (SolidBrush solidBrush = new SolidBrush(topColor))
                    graphics.FillRectangle(solidBrush, faceRectangle);
            else
                using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(VirtualClientRectangle, topColor, bottomColor, LinearGradientMode.Vertical))
                    graphics.FillRectangle(linearGradientBrush, faceRectangle);

#if THREEDEE
            //	Draw the highlight inside the tab selector.
            Color highlightColor;
            if (tabEntry.IsSelected)
                highlightColor = TabEntry.TabPageControl.ApplicationStyle.ActiveTabHighlightColor;
            else
                highlightColor = TabEntry.TabPageControl.ApplicationStyle.InactiveTabHighlightColor;
            using (SolidBrush solidBrush = new SolidBrush(highlightColor))
            {
                //	Draw the top edge.
                graphics.FillRectangle(	solidBrush,
                                        faceRectangle.X,
                                        faceRectangle.Y,
                                        faceRectangle.Width-1,
                                        1);

                //	Draw the left edge.
                graphics.FillRectangle(	solidBrush,
                                        faceRectangle.X,
                                        faceRectangle.Y+1,
                                        1,
                                        faceRectangle.Height-(tabEntry.IsSelected ? 2 : 1));
            }

            //	Draw the lowlight inside the tab selector.
            Color lowlightColor;
            if (tabEntry.IsSelected)
                lowlightColor = TabEntry.TabPageControl.ApplicationStyle.ActiveTabLowlightColor;
            else
                lowlightColor = TabEntry.TabPageControl.ApplicationStyle.InactiveTabLowlightColor;
            using (SolidBrush solidBrush = new SolidBrush(lowlightColor))
            {
                //	Draw the right edge.
                graphics.FillRectangle(	solidBrush,
                                        faceRectangle.Right-1,
                                        faceRectangle.Y+1,
                                        1,
                                        faceRectangle.Height-(tabEntry.IsSelected ? 2 : 1));
            }
#endif

            //	Draw the edges of the tab selector.
            using (SolidBrush solidBrush = new SolidBrush(TabEntry.TabPageControl.ApplicationStyle.BorderColor))
            {
                //	Draw the top edge.
                graphics.FillRectangle(solidBrush,
                    virtualClientRectangle.X + 2,
                    virtualClientRectangle.Y,
                    virtualClientRectangle.Width - 4,
                    1);

                //	Draw the left edge.
                graphics.FillRectangle(solidBrush,
                    virtualClientRectangle.X,
                    virtualClientRectangle.Y + 2,
                    1,
                    virtualClientRectangle.Height - (tabEntry.IsSelected ? 1 : 2));

                //	Draw the right edge.
                graphics.FillRectangle(solidBrush,
                    virtualClientRectangle.Right - 1,
                    virtualClientRectangle.Y + 2,
                    1,
                    virtualClientRectangle.Height - (tabEntry.IsSelected ? 1 : 2));

                //  Draw the corners.
                graphics.FillRectangle(solidBrush,
                    virtualClientRectangle.X + 1,
                    virtualClientRectangle.Y + 1,
                    1, 1);
                graphics.FillRectangle(solidBrush,
                    virtualClientRectangle.Right - 2,
                    virtualClientRectangle.Y + 1,
                    1, 1);
            }
        }

        #region Accessibility
        public override bool DoDefaultAction()
        {
            OnSelected(EventArgs.Empty);
            return true;
        }
        #endregion
    }
}
