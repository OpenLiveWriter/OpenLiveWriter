// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Provides a tab pane lightweight control similar to (but cooler than) TabControl.
    /// </summary>
    public class TabLightweightControl : LightweightControl
    {
        #region Private Member Variables & Declarations

        /// <summary>
        /// The number of pixels to scroll for each auto scroll.
        /// </summary>
        private const int AUTO_SCROLL_DELTA = 3;

        /// <summary>
        /// WHEEL_DELTA, as specified in WinUser.h.  Somehow Microsoft forgot this in the
        /// SystemInformation class in .NET...
        /// </summary>
        private const int WHEEL_DELTA = 120;

        /// <summary>
        /// The minimum tab width, below which the tab selector area will not be displayed.
        /// </summary>
        private const int MINIMUM_TAB_WIDTH = 12;

        /// <summary>
        /// Pad space, in pixels.  This value is used to provide a bit of "air" around the visual
        /// elements of the tab lightweight control.
        /// </summary>
        internal const int PAD = 2;

        /// <summary>
        /// The tab inset, in pixels.
        /// </summary>
        private const int TAB_INSET = 4;

        /// <summary>
        /// The drag-and-drop tab selection delay.
        /// </summary>
        private const int DRAG_DROP_SELECTION_DELAY = 500;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components;

        /// <summary>
        /// A value indicating whether tabs should be small.
        /// </summary>
        private bool smallTabs = false;

        /// <summary>
        /// A value which indicates whether side and bottom tab page borders will be drawn.
        /// </summary>
        private bool drawSideAndBottomTabPageBorders = true;

        /// <summary>
        /// A value indicating whether the tab selecter area is scrollable.
        /// </summary>
        private bool scrollableTabSelectorArea = false;

        /// <summary>
        /// A value indicating whether the tab selecter area will allow tab text/bitmaps to be clipped.
        /// </summary>
        private bool allowTabClipping = false;

        /// <summary>
        /// The tab entry list, sorted by tab number.
        /// </summary>
        private SortedList tabEntryList = new SortedList();

        /// <summary>
        /// The selected tab entry.
        /// </summary>
        private TabEntry selectedTabEntry = null;

        /// <summary>
        /// The tab page container control.  This control is used to contain all the TabPageControls
        /// that are added by calls to the SetTab method.  Note that it's important that each
        /// TabPageControl is properly sized and contained in the TabPageContainerControl.  We use
        /// Z order to display the right TabPageControl.
        /// </summary>
        private TabPageContainerControl tabPageContainerControl;

        /// <summary>
        /// The left tab scroller button.
        /// </summary>
        private TabScrollerButtonLightweightControl tabScrollerButtonLightweightControlLeft;

        /// <summary>
        /// The right tab scroller button.
        /// </summary>
        private TabScrollerButtonLightweightControl tabScrollerButtonLightweightControlRight;

        /// <summary>
        /// The tab selector area size.
        /// </summary>
        private Size tabSelectorAreaSize;

        /// <summary>
        /// The tab scroller position.
        /// </summary>
        private int tabScrollerPosition = 0;

        /// <summary>
        /// The DateTime of the last DragInside event.
        /// </summary>
        private DateTime dragInsideTime;

        #endregion Private Member Variables & Declarations

        #region Public Events

        /// <summary>
        /// Occurs when the SelectedTabNumber property changes.
        /// </summary>
        [
            Category("Property Changed"),
                Description("Occurs when the SelectedTabNumber property changes.")
        ]
        public event EventHandler SelectedTabNumberChanged;

        #endregion Public Events

        #region Class Initialization & Termination

        /// <summary>
        /// Initializes a new instance of the TabLightweightControl class.
        /// </summary>
        public TabLightweightControl(IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the TabLightweightControl class.
        /// </summary>
        public TabLightweightControl()
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
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
            this.tabScrollerButtonLightweightControlLeft = new OpenLiveWriter.ApplicationFramework.TabScrollerButtonLightweightControl(this.components);
            this.tabScrollerButtonLightweightControlRight = new OpenLiveWriter.ApplicationFramework.TabScrollerButtonLightweightControl(this.components);
            this.tabPageContainerControl = new OpenLiveWriter.ApplicationFramework.TabPageContainerControl();
            ((System.ComponentModel.ISupportInitialize)(this.tabScrollerButtonLightweightControlLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tabScrollerButtonLightweightControlRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            //
            // tabScrollerButtonLightweightControlLeft
            //
            this.tabScrollerButtonLightweightControlLeft.LightweightControlContainerControl = this;
            this.tabScrollerButtonLightweightControlLeft.Scroll += new System.EventHandler(this.tabScrollerButtonLightweightControlLeft_Scroll);
            this.tabScrollerButtonLightweightControlLeft.AutoScroll += new System.EventHandler(this.tabScrollerButtonLightweightControlLeft_AutoScroll);
            //
            // tabScrollerButtonLightweightControlRight
            //
            this.tabScrollerButtonLightweightControlRight.LightweightControlContainerControl = this;
            this.tabScrollerButtonLightweightControlRight.Scroll += new System.EventHandler(this.tabScrollerButtonLightweightControlRight_Scroll);
            this.tabScrollerButtonLightweightControlRight.AutoScroll += new System.EventHandler(this.tabScrollerButtonLightweightControlRight_AutoScroll);
            //
            // tabPageContainerControl
            //
            this.tabPageContainerControl.Location = new System.Drawing.Point(524, 17);
            this.tabPageContainerControl.Name = "tabPageContainerControl";
            this.tabPageContainerControl.TabIndex = 0;
            //
            // TabLightweightControl
            //
            this.AllowMouseWheel = true;
            ((System.ComponentModel.ISupportInitialize)(this.tabScrollerButtonLightweightControlLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tabScrollerButtonLightweightControlRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }
        #endregion

        #region Public Methods & Properties

        public bool ColorizeBorder
        {
            get
            {
                return _colorizeBorder;
            }
            set
            {
                _colorizeBorder = value;
            }
        }

        private bool _colorizeBorder = true;

        /// <summary>
        /// Gets or sets a value indicating whether tabs should be small.
        /// </summary>
        [
            Category("Appearance"),
                DefaultValue(false),
                Description("Specifies whether whether side and bottom tab page borders will be drawn.")
        ]
        public bool SmallTabs
        {
            get
            {
                return smallTabs;
            }
            set
            {
                if (smallTabs != value)
                {
                    smallTabs = value;

                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value which indicates whether side and bottom tab page borders will be drawn.
        /// </summary>
        [
            Category("Appearance"),
                DefaultValue(true),
                Description("Specifies whether whether side and bottom tab page borders will be drawn.")
        ]
        public bool DrawSideAndBottomTabPageBorders
        {
            get
            {
                return drawSideAndBottomTabPageBorders;
            }
            set
            {
                if (drawSideAndBottomTabPageBorders != value)
                {
                    drawSideAndBottomTabPageBorders = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        public override Size MinimumVirtualSize
        {
            get
            {
                // Not bothering to calculate minimum width, YAGNI.
                return new Size(0, tabPageContainerControl.Location.Y + SelectedTab.MinimumSize.Height);
            }
        }

        /// <summary>
        ///	Gets or sets a value indicating whether the tab selecter area is scrollable.
        /// </summary>
        [
            Category("Appearance"),
                DefaultValue(false),
                Description("Specifies whether whether whether the tab selecter area is scrollable.")
        ]
        public bool ScrollableTabSelectorArea
        {
            get
            {
                return scrollableTabSelectorArea;
            }
            set
            {
                Debug.Assert(!value, "'Scrollable tab selectors may not work correctly with bidi'");

                if (scrollableTabSelectorArea != value)
                {
                    scrollableTabSelectorArea = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        ///	Gets or sets whether the tab selecter area will allow tab text/bitmaps to be clipped.
        /// If false, text or bitmaps will be dropped to shrink the tab size.
        /// </summary>
        [
            Category("Appearance"),
                DefaultValue(false),
                Description("Specifies whether whether whether the tab selecter area is scrollable.")
        ]
        public bool AllowTabClipping
        {
            get
            {
                return allowTabClipping;
            }
            set
            {
                if (allowTabClipping != value)
                {
                    allowTabClipping = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets the tab count.
        /// </summary>
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public int TabCount
        {
            get
            {
                return tabEntryList.Count;
            }
        }

        /// <summary>
        /// Gets or sets the index of the currently-selected tab page.
        /// </summary>
        public int SelectedTabNumber
        {
            get
            {
                return tabEntryList.IndexOfValue(SelectedTabEntry);
            }
            set
            {
                TabEntry tabEntry = (TabEntry)tabEntryList[value];
                if (tabEntry != null)
                    SelectedTabEntry = tabEntry;
            }
        }

        public TabPageControl GetTab(int i)
        {
            return ((TabEntry)tabEntryList[i]).TabPageControl;
        }

        /// <summary>
        /// Gets the selected TabPageControl that was used to create the tab.
        /// </summary>
        public TabPageControl SelectedTab
        {
            get
            {
                return SelectedTabEntry.TabPageControl;
            }
        }

        private void IncrementTab()
        {
            int currentIndex = SelectedTabNumber + 1;
            if (currentIndex >= tabEntryList.Count)
                currentIndex = 0;
            TabEntry tabEntry = (TabEntry)tabEntryList[currentIndex];
            if (tabEntry != null)
                SelectedTabEntry = tabEntry;

        }

        private void DecrementTab()
        {
            int currentIndex = SelectedTabNumber - 1;
            if (currentIndex < 0)
                currentIndex = tabEntryList.Count - 1;
            TabEntry tabEntry = (TabEntry)tabEntryList[currentIndex];
            if (tabEntry != null)
                SelectedTabEntry = tabEntry;

        }

        public bool CheckForTabSwitch(Keys keyData)
        {
            if ((Control.ModifierKeys & Keys.Control) != 0)
            {
                Keys keys1 = keyData & Keys.KeyCode;
                if (keys1 == Keys.Tab)
                {
                    if ((Control.ModifierKeys & Keys.Shift) != 0)
                    {
                        DecrementTab();
                    }
                    else
                    {
                        IncrementTab();
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Sets a tab control.
        /// </summary>
        /// <param name="index">The tab index to set.</param>
        /// <param name="tabPageControl">The tab page control to set.</param>
        public void SetTab(int tabNumber, TabPageControl tabPageControl)
        {
            //	If there already is a tab entry for the specified tab number, remove it.
            RemoveTabEntry(tabNumber);

            //	Instantiate the new tab entry.
            TabEntry tabEntry = new TabEntry(this, tabPageControl);
            tabEntryList.Add(tabNumber, tabEntry);
            tabPageContainerControl.Controls.Add(tabEntry.TabPageControl);
            LightweightControls.Add(tabEntry.TabSelectorLightweightControl);
            tabEntry.TabSelectorLightweightControl.DragInside += new DragEventHandler(TabSelectorLightweightControl_DragInside);
            tabEntry.TabSelectorLightweightControl.DragOver += new DragEventHandler(TabSelectorLightweightControl_DragOver);
            tabEntry.TabSelectorLightweightControl.Selected += new EventHandler(TabSelectorLightweightControl_Selected);
            tabEntry.TabPageControl.VisibleChanged += new EventHandler(TabPageControl_VisibleChanged);

            //	Layout and invalidate.
            PerformLayout();
            Invalidate();
        }

        /// <summary>
        /// Removes a tab control.
        /// </summary>
        /// <param name="tabNumber">The tab number to remove.</param>
        public void RemoveTab(int tabNumber)
        {
            //	Remove it.
            RemoveTabEntry(tabNumber);

            //	Layout and invalidate.
            PerformLayout();
            Invalidate();
        }

        /// <summary>
        /// Determines whether the specified tab number has been added.
        /// </summary>
        /// <param name="tabNumber">The tab number to look for.</param>
        /// <returns>True if a tab has been added with the specified tab number; false otherwise.</returns>
        public bool HasTab(int tabNumber)
        {
            return tabEntryList.Contains(tabNumber);
        }

        #endregion Public Methods & Properties

        #region Internal Methods & Properties

        /// <summary>
        /// Gets the selected tab entry.
        /// </summary>
        internal TabEntry SelectedTabEntry
        {
            get
            {
                //	Select the first tab entry, if there isn't a selected tab entry.
                if (selectedTabEntry == null)
                    SelectedTabEntry = FirstTabEntry;
                return selectedTabEntry;
            }
            set
            {
                if (selectedTabEntry != value)
                {
                    if (selectedTabEntry != null)
                        selectedTabEntry.Unselected();

                    //	Select the new tab entry.
                    selectedTabEntry = value;

                    if (selectedTabEntry != null)
                        selectedTabEntry.Selected();

                    PerformLayout();
                    Invalidate();

                    //	Raise the SelectedTabNumberChanged event.
                    OnSelectedTabNumberChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets the first tab entry.
        /// </summary>
        internal TabEntry FirstTabEntry
        {
            get
            {
                return (tabEntryList.Count == 0) ? null : (TabEntry)tabEntryList.GetByIndex(0);
            }
        }

        #endregion

        #region Protected Event Overrides

        /// <summary>
        /// Raises the Layout event.  The mother of all layout processing.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLayout(EventArgs e)
        {
            //	Call the base class's method so registered delegates receive the event.
            base.OnLayout(e);

            //	No layout required if this control is not visible.
            if (Parent == null)
                return;

            //	If there are not tabs, layout processing is simple.
            if (tabEntryList.Count == 0)
            {
                HideTabScrollerInterface();
                tabSelectorAreaSize = new Size(0, 1);
                return;
            }

            //	Pre-process the layout of tab selectors.
            int width = (TAB_INSET * 2) + (PAD * 4) + (PAD * (tabEntryList.Count - 1)), height = 0;
            foreach (TabEntry tabEntry in tabEntryList.Values)
            {
                if (!tabEntry.Hidden)
                {
                    //	Reset the tab selector and have it perform its layout logic so that it is at
                    //	its natural size.
                    tabEntry.TabSelectorLightweightControl.SmallTabs = SmallTabs;
                    tabEntry.TabSelectorLightweightControl.ShowTabBitmap = true;
                    tabEntry.TabSelectorLightweightControl.ShowTabText = true;
                    tabEntry.TabSelectorLightweightControl.PerformLayout();

                    //	If this tab selector is the tallest one we've seen, it sets a new high-
                    //	water mark.
                    if (tabEntry.TabSelectorLightweightControl.UnselectedVirtualSize.Height > height)
                        height = tabEntry.TabSelectorLightweightControl.UnselectedVirtualSize.Height;

                    //	Adjust the width to account for this tab selector.
                    width += tabEntry.TabSelectorLightweightControl.UnselectedVirtualSize.Width;

                    // Push the tabs closer together
                    width -= 6;
                }
            }
            width += 6;

            //	Pad height.
            height += PAD;

            //	If the tab selection area is scrollable, enable/disable the scroller interface.
            if (scrollableTabSelectorArea)
            {
                //	Set the tab selector area size.
                tabSelectorAreaSize = new Size(width, height);

                //	If the entire tab selector area is visible, hide the tab scroller interface.
                if (width <= VirtualWidth)
                    HideTabScrollerInterface();
                else
                {
                    //	If the tab scroller position is zero, the left tab scroller button is not
                    //	shown.
                    if (tabScrollerPosition == 0)
                        tabScrollerButtonLightweightControlLeft.Visible = false;
                    else
                    {
                        //	Layout the left tab scroller button.
                        tabScrollerButtonLightweightControlLeft.VirtualBounds = new Rectangle(0,
                                                                                                0,
                                                                                                tabScrollerButtonLightweightControlLeft.DefaultVirtualSize.Width,
                                                                                                tabSelectorAreaSize.Height);
                        tabScrollerButtonLightweightControlLeft.Visible = true;
                        tabScrollerButtonLightweightControlLeft.BringToFront();
                    }

                    //	Layout the right tab scroller button.
                    if (tabScrollerPosition == MaximumTabScrollerPosition)
                        tabScrollerButtonLightweightControlRight.Visible = false;
                    else
                    {
                        int rightScrollerButtonWidth = tabScrollerButtonLightweightControlRight.DefaultVirtualSize.Width;
                        tabScrollerButtonLightweightControlRight.VirtualBounds = new Rectangle(TabAreaRectangle.Right - rightScrollerButtonWidth,
                                                                                                0,
                                                                                                rightScrollerButtonWidth,
                                                                                                tabSelectorAreaSize.Height);
                        tabScrollerButtonLightweightControlRight.Visible = true;
                        tabScrollerButtonLightweightControlRight.BringToFront();
                    }
                }
            }
            else
            {
                //	Hide the tab scroller interface.
                HideTabScrollerInterface();

                if (!allowTabClipping)
                {
                    //	If the entire tab selector area is not visible, switch into "no bitmap" mode.
                    if (width > VirtualWidth)
                    {
                        //	Reset the width.
                        width = (TAB_INSET * 2) + (PAD * 4) + (PAD * (tabEntryList.Count - 1));

                        //	Re-process the layout of tab selectors.
                        foreach (TabEntry tabEntry in tabEntryList.Values)
                        {
                            if (!tabEntry.Hidden)
                            {
                                //	Hide tab bitmap.
                                tabEntry.TabSelectorLightweightControl.ShowTabBitmap = false;
                                tabEntry.TabSelectorLightweightControl.ShowTabText = true;
                                tabEntry.TabSelectorLightweightControl.PerformLayout();

                                //	Adjust the width to account for this tab selector.
                                width += tabEntry.TabSelectorLightweightControl.UnselectedVirtualSize.Width;
                            }
                        }
                    }

                    //	If the entire tab selector area is not visible, switch into "no text except selected" mode.
                    if (width > VirtualWidth)
                    {
                        //	Reset the width.
                        width = (TAB_INSET * 2) + (PAD * 4) + (PAD * (tabEntryList.Count - 1));

                        //	Re-process the layout of tab selectors.
                        foreach (TabEntry tabEntry in tabEntryList.Values)
                        {
                            if (!tabEntry.Hidden)
                            {
                                //	Hide tab text.
                                tabEntry.TabSelectorLightweightControl.ShowTabBitmap = true;
                                tabEntry.TabSelectorLightweightControl.ShowTabText = tabEntry.IsSelected;
                                tabEntry.TabSelectorLightweightControl.PerformLayout();

                                //	Adjust the width to account for this tab selector.
                                width += tabEntry.TabSelectorLightweightControl.UnselectedVirtualSize.Width;
                            }
                        }
                    }

                    //	If the entire tab selector area is not visible, switch into "no text" mode.
                    if (width > VirtualWidth)
                    {
                        //	Reset the width.
                        width = (TAB_INSET * 2) + (PAD * 4) + (PAD * (tabEntryList.Count - 1));

                        //	Re-process the layout of tab selectors.
                        foreach (TabEntry tabEntry in tabEntryList.Values)
                        {
                            if (!tabEntry.Hidden)
                            {
                                //	Hide tab text.
                                tabEntry.TabSelectorLightweightControl.ShowTabBitmap = true;
                                tabEntry.TabSelectorLightweightControl.ShowTabText = false;
                                tabEntry.TabSelectorLightweightControl.PerformLayout();

                                //	Adjust the width to account for this tab selector.
                                width += tabEntry.TabSelectorLightweightControl.UnselectedVirtualSize.Width;
                            }
                        }
                    }

                    //	If the entire tab selector area is not visible, hide it.
                    if (width > VirtualWidth)
                    {
                        //	Reset the width.
                        width = (TAB_INSET * 2) + (PAD * 4) + (PAD * (tabEntryList.Count - 1));

                        //	Re-process the layout of tab selectors.
                        foreach (TabEntry tabEntry in tabEntryList.Values)
                        {
                            if (!tabEntry.Hidden)
                            {
                                //	Hide tab text.
                                tabEntry.TabSelectorLightweightControl.ShowTabBitmap = false;
                                tabEntry.TabSelectorLightweightControl.PerformLayout();

                                //	Adjust the width to account for this tab selector.
                                width += tabEntry.TabSelectorLightweightControl.UnselectedVirtualSize.Width;
                            }
                        }

                        if (width > VirtualWidth)
                            height = 1;
                    }
                }

                //	Set the tab selector layout size.
                tabSelectorAreaSize = new Size(width, height);
            }

            //	Finally, actually layout the tab entries.
            int x = TAB_INSET + (PAD * 2);
            TabEntry previousTabEntry = null;
            foreach (TabEntry tabEntry in tabEntryList.Values)
            {
                if (!tabEntry.Hidden)
                {
                    //	Adjust the x offset for proper positioning of this tab and set the y offset,
                    //	too.  Now we know WHERE the tab will be laid out in the tab area.
                    if (previousTabEntry != null)
                        x += previousTabEntry.IsSelected ? -PAD + 1 : -1;
                    int y = Math.Max(0, tabSelectorAreaSize.Height - tabEntry.TabSelectorLightweightControl.VirtualBounds.Height);

                    //	Latout the tab entry.
                    tabEntry.TabSelectorLightweightControl.VirtualLocation = new Point(x - tabScrollerPosition, y);

                    //	Adjust the x offset to account for the tab entry.
                    x += tabEntry.TabSelectorLightweightControl.VirtualBounds.Width;

                    //	Set the previous tab entry for the next loop iteration.
                    previousTabEntry = tabEntry;
                }
            }

            //	Set the bounds of the tab page control.
            Rectangle tabPageControlBounds = VirtualClientRectangleToParent(TabPageRectangle);
            if (tabPageContainerControl.Bounds != tabPageControlBounds)
                tabPageContainerControl.Bounds = tabPageControlBounds;

            //	Make sure the selected tab entry and its TabPageControl are visible and at the top
            //	of the Z order and that, if there is one, the previously selected tab entry's
            //	TabPageControl is not visible.
            if (SelectedTabEntry != null)
            {
                SelectedTabEntry.TabSelectorLightweightControl.BringToFront();
                SelectedTabEntry.TabPageControl.BringToFront();
            }

            RtlLayoutFixup(true);
        }

        /// <summary>
        /// Raises the LightweightControlContainerControlChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLightweightControlContainerControlChanged(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLightweightControlContainerControlChanged(e);

            //	Set the tab page control's parent.
            if (tabPageContainerControl.Parent != Parent)
                tabPageContainerControl.Parent = Parent;

            PerformLayout();
            Invalidate();
        }

        /// <summary>
        /// Raises the LightweightControlContainerControlVirtualLocationChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLightweightControlContainerControlVirtualLocationChanged(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLightweightControlContainerControlVirtualLocationChanged(e);

            //	Layout and invalidate.
            PerformLayout();
            Invalidate();
        }

        /// <summary>
        /// Raises the MouseWheel event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseWheel(e);

            //	Scroll the tab layout area.
            ScrollTabLayoutArea(-(e.Delta / WHEEL_DELTA) * ScrollDelta);
        }

        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            //	Draw the tab page border.
            DrawTabPageBorders(e.Graphics);

            //	Call the base class's method so that registered delegates receive the event.
            base.OnPaint(e);
        }

        /// <summary>
        /// Raises the VirtualLocationChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnVirtualLocationChanged(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnVirtualLocationChanged(e);

            //	Layout and invalidate.
            PerformLayout();
            Invalidate();
        }

        #endregion Protected Event Overrides

        #region Protected Events

        /// <summary>
        /// Raises the SelectedTabNumberChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnSelectedTabNumberChanged(EventArgs e)
        {
            if (SelectedTabNumberChanged != null)
                SelectedTabNumberChanged(this, e);
        }

        #endregion

        #region Private Methods & Properties

        /// <summary>
        /// Helper to remove a tab entry.
        /// </summary>
        /// <param name="tabNumber">The tab number of the tab entry to remove.</param>
        private void RemoveTabEntry(int tabNumber)
        {
            //	Locate the tab entry for the specified tab number.  It it's found, remove it.
            TabEntry tabEntry = (TabEntry)tabEntryList[tabNumber];
            if (tabEntry != null)
            {
                if (SelectedTabEntry == tabEntry)
                    SelectedTabEntry = FirstTabEntry;
                tabPageContainerControl.Controls.Remove(tabEntry.TabPageControl);
                LightweightControls.Remove(tabEntry.TabSelectorLightweightControl);
                tabEntryList.Remove(tabNumber);
            }
        }

        /// <summary>
        ///	Gets or sets the tab scroller position.
        /// </summary>
        private int TabScrollerPosition
        {
            get
            {
                return tabScrollerPosition;
            }
            set
            {
                tabScrollerPosition = MathHelper.Clip(value, 0, MaximumTabScrollerPosition);
            }
        }

        /// <summary>
        /// Gets the tab area rectangle.  This is the entire area available for tab and tab
        /// scroller button layout.
        /// </summary>
        private Rectangle TabAreaRectangle
        {
            get
            {
                return new Rectangle(0,
                                        0,
                                        VirtualWidth,
                                        tabSelectorAreaSize.Height);
            }
        }

        /// <summary>
        /// Gets the tab page border rectangle.
        /// </summary>
        private Rectangle TabPageBorderRectangle
        {
            get
            {
                return new Rectangle(0,
                                        tabSelectorAreaSize.Height - 1,
                                        VirtualWidth,
                                        VirtualHeight - (tabSelectorAreaSize.Height - 1));
            }
        }

        /// <summary>
        /// Gets the tab page rectangle.
        /// </summary>
        private Rectangle TabPageRectangle
        {
            get
            {
                int border = drawSideAndBottomTabPageBorders ? 1 : 0;
                return new Rectangle(border,
                                        tabSelectorAreaSize.Height + border,
                                        VirtualWidth - (border * 2),
                                        VirtualHeight - (tabSelectorAreaSize.Height + (border * 2)));
            }
        }

        /// <summary>
        /// Gets the maximum tab scroller position.
        /// </summary>
        private int MaximumTabScrollerPosition
        {
            get
            {
                return Math.Max(0, tabSelectorAreaSize.Width - TabAreaRectangle.Width);
            }
        }

        /// <summary>
        /// Gets the scroll delta, or how many pixels to scroll the tab area.
        /// </summary>
        private int ScrollDelta
        {
            get
            {
                return TabAreaRectangle.Width / 8;
            }
        }

        /// <summary>
        /// Hides the tab scroller interface.
        /// </summary>
        private void HideTabScrollerInterface()
        {
            tabScrollerPosition = 0;
            tabScrollerButtonLightweightControlLeft.Visible = false;
            tabScrollerButtonLightweightControlRight.Visible = false;
        }

        /// <summary>
        /// Draws the tab page border.
        /// </summary>
        /// <param name="graphics">Graphics context where the tab page border is to be drawn.</param>
        private void DrawTabPageBorders(Graphics graphics)
        {
            Color c = SystemColors.ControlDark;
            if (_colorizeBorder)
                c = ColorizedResources.Instance.BorderLightColor;

            //	Draw tab page borders.
            using (SolidBrush borderBrush = new SolidBrush(c))
            {
                //	Obtain the tab page border rectangle.
                Rectangle tabPageBorderRectangle = TabPageBorderRectangle;

                //	Draw the top edge.
                graphics.FillRectangle(borderBrush,
                                        tabPageBorderRectangle.X,
                                        tabPageBorderRectangle.Y,
                                        tabPageBorderRectangle.Width,
                                        1);

                //	Draw the highlight under the top edge.
                using (SolidBrush highlightBrush = new SolidBrush(Color.FromArgb(206, 219, 248)))
                {
                    graphics.FillRectangle(highlightBrush,
                                            tabPageBorderRectangle.X,
                                            tabPageBorderRectangle.Y + 1,
                                            tabPageBorderRectangle.Width,
                                            1);
                }

                //	Draw tab page borders, if we should.
                if (drawSideAndBottomTabPageBorders)
                {
                    //	Draw the left edge.
                    graphics.FillRectangle(borderBrush,
                                            tabPageBorderRectangle.X,
                                            tabPageBorderRectangle.Y + 1,
                                            1,
                                            tabPageBorderRectangle.Height - 1);

                    //	Draw the right edge.
                    graphics.FillRectangle(borderBrush,
                                            tabPageBorderRectangle.Right - 1,
                                            tabPageBorderRectangle.Y + 1,
                                            1,
                                            tabPageBorderRectangle.Height - 1);

                    //	Draw the bottom edge.
                    graphics.FillRectangle(borderBrush,
                                            tabPageBorderRectangle.X + 1,
                                            tabPageBorderRectangle.Bottom - 1,
                                            tabPageBorderRectangle.Width - 2,
                                            1);
                }
            }
        }

        /// <summary>
        /// TabSelectorLightweightControl_DragInside event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void TabSelectorLightweightControl_DragInside(object sender, DragEventArgs e)
        {
            //	Note the DateTime of the last DragInside event.
            dragInsideTime = DateTime.Now;
        }

        /// <summary>
        /// TabSelectorLightweightControl_DragInside event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void TabSelectorLightweightControl_DragOver(object sender, DragEventArgs e)
        {
            //	Wait an amount of time before selecting the tab page.
            if (DateTime.Now.Subtract(dragInsideTime).Milliseconds < DRAG_DROP_SELECTION_DELAY)
                return;

            //	Ensure that the sender is who we think it is.
            Debug.Assert(sender is TabSelectorLightweightControl, "Doh!", "Bad event wiring is the leading cause of code decay.");
            if (sender is TabSelectorLightweightControl)
            {

                //	Set the selected tab entry, if we should.
                TabSelectorLightweightControl tabSelectorLightweightControl = (TabSelectorLightweightControl)sender;
                if (tabSelectorLightweightControl.TabEntry.TabPageControl.DragDropSelectable && SelectedTabEntry != tabSelectorLightweightControl.TabEntry)
                    SelectedTabEntry = tabSelectorLightweightControl.TabEntry;
            }
        }

        /// <summary>
        /// TabSelectorLightweightControl_Selected event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void TabSelectorLightweightControl_Selected(object sender, EventArgs e)
        {
            //	Ensure that the sender is who we think it is.
            Debug.Assert(sender is TabSelectorLightweightControl, "Doh!", "Bad event wiring is the leading cause of code decay.");
            if (sender is TabSelectorLightweightControl)
            {
                //	Set the selected tab entry.
                TabSelectorLightweightControl tabSelectorLightweightControl = (TabSelectorLightweightControl)sender;
                SelectedTabEntry = tabSelectorLightweightControl.TabEntry;
            }
        }

        private void TabPageControl_VisibleChanged(object sender, EventArgs e)
        {
            PerformLayout();
            Invalidate();
        }

        /// <summary>
        /// tabScrollerButtonLightweightControlLeft_Scroll event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void tabScrollerButtonLightweightControlLeft_Scroll(object sender, EventArgs e)
        {
            ScrollTabLayoutArea(-ScrollDelta);
        }

        /// <summary>
        /// tabScrollerButtonLightweightControlRight_Scroll event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void tabScrollerButtonLightweightControlRight_Scroll(object sender, EventArgs e)
        {
            ScrollTabLayoutArea(ScrollDelta);
        }

        /// <summary>
        /// tabScrollerButtonLightweightControlLeft_AutoScroll event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void tabScrollerButtonLightweightControlLeft_AutoScroll(object sender, EventArgs e)
        {
            ScrollTabLayoutArea(-AUTO_SCROLL_DELTA);
        }

        /// <summary>
        /// tabScrollerButtonLightweightControlRight_AutoScroll event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void tabScrollerButtonLightweightControlRight_AutoScroll(object sender, EventArgs e)
        {
            ScrollTabLayoutArea(AUTO_SCROLL_DELTA);
        }

        /// <summary>
        /// Scrolls the tab layout area.
        /// </summary>
        /// <param name="delta">Scroll delta.  This value does not have to take scroll limits into
        /// account as the TabScrollerPosition property handles this.</param>
        private void ScrollTabLayoutArea(int delta)
        {
            //	If we have a scrollable tab layout area, scroll it.
            if (ScrollableTabSelectorArea)
            {
                //	Adjust the tab scroller position.
                TabScrollerPosition += delta;

                //	Get the screen up to date.
                PerformLayout();
                Invalidate();
                Update();
            }
        }

        #endregion Private Methods & Properties

        #region Accessibility
        protected override void AddAccessibleControlsToList(ArrayList list)
        {
            for (int i = 0; i < TabCount; i++)
            {
                TabEntry tabEntry = (TabEntry)tabEntryList[i];
                list.Add(tabEntry.TabSelectorLightweightControl);
            }
            base.AddAccessibleControlsToList(list);
        }
        #endregion
    }
}
