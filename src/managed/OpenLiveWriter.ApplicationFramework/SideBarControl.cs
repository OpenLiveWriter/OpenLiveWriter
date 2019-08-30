// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// A side-bar control similar to the one appearing in FireBird's preferences dialog.  We use
    /// this control to build user interfaces similar to a TabControl, but without all the bugs
    /// from Microsoft.
    /// </summary>
    public class SideBarControl : BorderControl
    {
        #region Static & Constant Declarations

        /// <summary>
        /// The pad constant.  Used to provide a bit of air around visual components.
        /// </summary>
        private const int PAD = 2;

        #endregion Static & Constant Declarations

        #region Private Member Variables

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        /// <summary>
        /// BitmapButton list of the entries in the SideBarControl.
        /// </summary>
        private ArrayList bitmapButtonList = new ArrayList();

        /// <summary>
        /// The selected index of the BitmapButton that is selected.
        /// </summary>
        private int selectedIndex = -1;

        #endregion Private Member Variables

        #region Public Events

        /// <summary>
        /// Occurs when the SelectedIndex property changes.
        /// </summary>
        [
            Category("Property Changed"),
                Description("Occurs when the SelectedIndex property changes.")
        ]
        public event EventHandler SelectedIndexChanged;

        #endregion

        #region Class Initialization & Termination

        /// <summary>
        /// Initializes a new instance of the SideBarControl class.
        /// </summary>
        public SideBarControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            //	Set the control to a simple UserControl that will contain the BitmapButtons.
            Control = new UserControl();
            Control.GotFocus += new EventHandler(Control_GotFocus);
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
            //
            // SideBarControl
            //
            this.Name = "SideBarControl";

        }
        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the selected index.
        /// </summary>
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }
            set
            {
                if (selectedIndex != value)
                {
                    //	Ensure that the value is valid.
                    Debug.Assert(value == -1 || (value >= 0 && value < bitmapButtonList.Count), "SelectedIndex out of range");
                    if (!(value == -1 || (value >= 0 && value < bitmapButtonList.Count)))
                        throw new ArgumentOutOfRangeException("value");

                    //	Deselect the currently selected BitmapButton, if there is one.
                    if (selectedIndex >= 0 && selectedIndex < bitmapButtonList.Count)
                    {
                        BitmapButton bitmapButton = (BitmapButton)bitmapButtonList[selectedIndex];
                        bitmapButton.Latched = false;
                    }

                    //	Set the new selected index.
                    selectedIndex = value;

                    //	Select the new BitmapButton.
                    if (selectedIndex != -1)
                    {
                        BitmapButton bitmapButton = (BitmapButton)bitmapButtonList[selectedIndex];
                        bitmapButton.Latched = true;
                    }

                    //	Raise the SelectedIndexChanged event.
                    OnSelectedIndexChanged(EventArgs.Empty);
                }
            }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Sets a SideBarControl entry.
        /// </summary>
        /// <param name="index">Index of the entry to set; zero based.</param>
        /// <param name="bitmap">Bitmap of the entry to set.</param>
        /// <param name="text">Text of the entry to set.</param>
        public void SetEntry(int index, Bitmap bitmap, string text, string name)
        {
            //	Instantiate and initialize the BitmapButton.
            BitmapButton bitmapButton = new BitmapButton();
            bitmapButton.Tag = index;
            bitmapButton.Click += new EventHandler(bitmapButton_Click);
            bitmapButton.AutoSizeHeight = false;
            bitmapButton.AutoSizeWidth = false;
            bitmapButton.ButtonStyle = ButtonStyle.Flat;
            bitmapButton.TextAlignment = TextAlignment.Right;
            bitmapButton.ButtonText = text;
            bitmapButton.BitmapEnabled = bitmap;
            bitmapButton.BitmapSelected = bitmap;
            bitmapButton.ClickSetsFocus = true;
            bitmapButton.Size = new Size(Control.Width - (PAD * 2), 52);
            bitmapButton.TabStop = false;
            bitmapButton.AccessibleName = text;
            bitmapButton.Name = name;
            Control.Controls.Add(bitmapButton);

            //	Replace and existing BitmapButton.
            if (index < bitmapButtonList.Count)
            {
                //	Remove the existing BitmapButton.
                if (bitmapButtonList[index] != null)
                {
                    BitmapButton oldBitmapButton = (BitmapButton)bitmapButtonList[index];
                    oldBitmapButton.Click -= new EventHandler(bitmapButton_Click);
                }

                //	Set the new BitmapButton.
                bitmapButtonList[index] = bitmapButton;
            }
            //	Add a new BitmapButton.
            else
            {
                //	Ensure that there are entries up to the index position (make them null).  This
                //	allows the user of this control to add his entries out of order or with gaps.
                for (int i = bitmapButtonList.Count; i < index; i++)
                    bitmapButtonList.Add(null);

                //	Add the BitmapButton.
                bitmapButtonList.Add(bitmapButton);
            }
        }

        public void AdjustSize()
        {
            int maxWidth = Width;
            foreach (BitmapButton button in bitmapButtonList)
            {
                button.AutoSizeWidth = true;
                button.AutoSizeHeight = true;
                // HACK: AutoSizeWidth doesn't quite work right; it doesn't
                // take effect until SetBoundsCore gets called, so I have
                // to "change" the width to force the SetBoundsCore call.
                // Yuck!!
                button.Width = button.Width + 1;
                button.Height = button.Height + 1;
                maxWidth = Math.Max(maxWidth, button.Width);
            }

            foreach (BitmapButton button in bitmapButtonList)
            {
                button.AutoSizeWidth = false;
                button.AutoSizeHeight = false;
                button.Width = maxWidth;
                button.Height = 
                    (int)Math.Ceiling(button.Height / (DisplayHelper.PixelsPerLogicalInchY / 96f))
                  + (button.BitmapEnabled == null ? DisplayHelper.ScaleYCeil(10) : 0); // Add a 10 pixel vertical padding when text-only
            }

            Width = maxWidth + PAD * 2;
        }

        #endregion Public Methods

        #region Protected Events

        /// <summary>
        /// Raises the SelectedIndexChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnSelectedIndexChanged(EventArgs e)
        {
            if (SelectedIndexChanged != null)
                SelectedIndexChanged(this, e);
        }

        #endregion Protected Events

        #region Protected Event Overrides

        /// <summary>
        /// Raises the Layout event.
        /// </summary>
        /// <param name="e">A LayoutEventArgs that contains the event data.</param>
        protected override void OnLayout(LayoutEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLayout(e);

            //	Layout each of the bitmap buttons in the entry list.
            int yOffset = PAD;
            foreach (BitmapButton bitmapButton in bitmapButtonList)
            {
                if (bitmapButton != null)
                {
                    bitmapButton.Location = new Point(Utility.CenterMinZero(bitmapButton.Width, Control.Width), yOffset);
                    yOffset += bitmapButton.Height + PAD;
                }
            }
        }

        #endregion Protected Event Overrides

        #region Private Event Handlers

        /// <summary>
        /// bitmapButton_Click event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void bitmapButton_Click(object sender, EventArgs e)
        {
            //	Get the sending BitmapButton.
            BitmapButton bitmapButton = sender as BitmapButton;
            Debug.Assert(bitmapButton != null, "What??");
            if (bitmapButton == null)
                return;

            //	Set the SelectedIndex.
            SelectedIndex = (int)bitmapButton.Tag;
        }

        /// <summary>
        /// Control_GotFocus event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void Control_GotFocus(object sender, EventArgs e)
        {
            //	Drive focus to the correct BitmapButton.
            if (selectedIndex != -1)
            {
                BitmapButton bitmapButton = (BitmapButton)bitmapButtonList[selectedIndex];
                bitmapButton.Focus();
            }
        }

        #endregion Private Event Handlers
    }
}
