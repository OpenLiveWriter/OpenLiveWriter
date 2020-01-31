// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.ApplicationFramework.Preferences
{

    public class PreferencesForm : BaseForm
    {
        #region Static & Constant Declarations

        #endregion Static & Constant Declarations

        #region Private Member Variables

        /// <summary>
        /// The SideBarControl that provides our TabControl-like user interface.
        /// </summary>
        private SideBarControl sideBarControl;

        /// <summary>
        /// The PreferencesPanel list.
        /// </summary>
        protected ArrayList preferencesPanelList = new ArrayList();

        /// <summary>
        /// A value which indicates whether the form is initialized.
        /// </summary>
        private bool initialized;

        #endregion Private Member Variables

        #region Windows Form Designer generated code

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.Panel panelPreferences;
        private System.ComponentModel.IContainer components;

        #endregion Windows Form Designer generated code

        #region Class Initialization & Termination

        public PreferencesForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.buttonOK.Text = Res.Get(StringId.OKButtonText);
            this.buttonCancel.Text = Res.Get(StringId.CancelButton);
            this.buttonApply.Text = Res.Get(StringId.ApplyButton);
            //	Set the title of the form.
            Text = Res.Get(StringId.Options);

            //	Instantiate and initialize the SideBarControl.
            sideBarControl = new SideBarControl();
            sideBarControl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
            sideBarControl.TabStop = true;
            sideBarControl.TabIndex = 0;
            sideBarControl.SelectedIndexChanged += new EventHandler(sideBarControl_SelectedIndexChanged);
            sideBarControl.Location = new Point(10, 10);
            sideBarControl.Size = new Size(151, ClientSize.Height - 20);
            Controls.Add(sideBarControl);

        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (PreferencesPanel panel in preferencesPanelList)
                    panel.Dispose();

                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #endregion Class Initialization & Termination

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(PreferencesForm));
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonApply = new System.Windows.Forms.Button();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.panelPreferences = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            //
            // buttonOK
            //
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOK.Location = new System.Drawing.Point(288, 568);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "OK";
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.Location = new System.Drawing.Point(368, 568);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            //
            // buttonApply
            //
            this.buttonApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonApply.Enabled = false;
            this.buttonApply.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonApply.Location = new System.Drawing.Point(448, 568);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(75, 23);
            this.buttonApply.TabIndex = 4;
            this.buttonApply.Text = "&Apply";
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            //
            // imageList
            //
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageList.ImageSize = new System.Drawing.Size(32, 32);
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.White;
            //
            // panelPreferences
            //
            this.panelPreferences.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panelPreferences.BackColor = System.Drawing.SystemColors.Control;
            this.panelPreferences.Location = new System.Drawing.Point(162, 0);
            this.panelPreferences.Name = "panelPreferences";
            this.panelPreferences.Size = new System.Drawing.Size(370, 567);
            this.panelPreferences.TabIndex = 1;
            //
            // PreferencesForm
            //
            this.AcceptButton = this.buttonOK;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(534, 600);
            this.Controls.Add(this.panelPreferences);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PreferencesForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Options";
            this.ResumeLayout(false);

        }
        #endregion

        #region Events

        /// <summary>
        /// Event triggered when the user saves a new set of preferences (by pressing OK, or Apply).
        /// </summary>
        public event EventHandler PreferencesSaved;

        public void OnPreferencesSaved(EventArgs evt)
        {
            if (PreferencesSaved != null)
                PreferencesSaved(this, evt);
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
                return sideBarControl.SelectedIndex;
            }
            set
            {
                sideBarControl.SelectedIndex = value;
            }
        }

        public IWin32Window Win32Owner
        {
            get
            {
                if (_win32Owner != null)
                    return _win32Owner;
                else
                    return Owner;
            }
            set
            {
                _win32Owner = value;
            }
        }
        private IWin32Window _win32Owner;

        #endregion Public Properties

        #region Public Methods

        public void HideApplyButton()
        {
            buttonApply.Visible = false;
            int shift = buttonApply.Right - buttonCancel.Right;
            buttonOK.Left += shift;
            buttonCancel.Left += shift;
        }

        public void SelectEntry(Type panel)
        {

            for (int i = 0; i < preferencesPanelList.Count; i++)
            {
                if (preferencesPanelList[i].GetType() == panel)
                {
                    SelectedIndex = i;
                    return;
                }
            }
        }

        /// <summary>
        /// Sets a PreferencesPanel.
        /// </summary>
        /// <param name="index">Index of the entry to set; zero based.</param>
        /// <param name="preferencesPanel">The PreferencesPanel to set.</param>
        public void SetEntry(int index, PreferencesPanel preferencesPanel)
        {
            //	Set the SideBarControl entry.
            sideBarControl.SetEntry(index, preferencesPanel.PanelBitmap, preferencesPanel.PanelName, "btn" + preferencesPanel.Name);

            //	Set our PreferencesPanel event handlers.
            preferencesPanel.Modified += new EventHandler(preferencesPanel_Modified);

            //	Replace and existing PreferencesPanel.
            if (index < preferencesPanelList.Count)
            {
                //	Remove the existing PreferencesPanel.
                if (preferencesPanelList[index] != null)
                {
                    PreferencesPanel oldPreferencesPanel = (PreferencesPanel)preferencesPanelList[index];
                    oldPreferencesPanel.Modified -= new EventHandler(preferencesPanel_Modified);
                    if (sideBarControl.SelectedIndex == index)
                        panelPreferences.Controls.Remove(oldPreferencesPanel);
                }

                //	Set the new PreferencesPabel.
                preferencesPanelList[index] = preferencesPanel;
            }
            //	Add a new PreferencesPanel.
            else
            {
                //	Ensure that there are entries up to the index position (make them null).  This
                //	allows the user of this control to add his entries out of order or with gaps.
                for (int i = preferencesPanelList.Count; i < index; i++)
                    preferencesPanelList.Add(null);

                //	Add the BitmapButton.
                preferencesPanelList.Add(preferencesPanel);
            }

            //	Add the Preferences panel.
            preferencesPanel.Dock = DockStyle.Fill;
            panelPreferences.Controls.Add(preferencesPanel);
        }

        #endregion Public Methods

        #region Protected Event Overrides

        /// <summary>
        /// Raises the Load event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLoad(e);

            //  The Collection Settings dialog looks weird when it comes up with Fields
            //  selected but Flags focused... both boxes are blue. This makes sure that
            //  doesn't happen.
            this.buttonCancel.Focus();

            AdjustHeightToFit();

            using (new AutoGrow(this, AnchorStyles.Right, true))
            {
                LayoutHelper.EqualizeButtonWidthsHoriz(AnchorStyles.Right, buttonCancel.Width, int.MaxValue,
                    buttonOK, buttonCancel, buttonApply);

                int oldSize = sideBarControl.Width;
                sideBarControl.AdjustSize();
                int deltaX = sideBarControl.Width - oldSize;
                new ControlGroup(panelPreferences, buttonOK, buttonCancel, buttonApply).Left += deltaX;

                if (buttonOK.Left < sideBarControl.Right)
                {
                    int right = buttonApply.Right;
                    DisplayHelper.AutoFitSystemButton(buttonOK);
                    DisplayHelper.AutoFitSystemButton(buttonCancel);
                    DisplayHelper.AutoFitSystemButton(buttonApply);
                    LayoutHelper.DistributeHorizontally(8, buttonOK, buttonCancel, buttonApply);
                    new ControlGroup(buttonOK, buttonCancel, buttonApply).Left += right - buttonApply.Right;
                }
            }

            //	We're initialized, so remove all unselected panels.  This allows AutoScale to
            //	work.
            initialized = true;
            RemoveUnselectedPanels();

            // protect against being shown directly on top of an identically sized owner
            OffsetFromIdenticalOwner();
        }

        private void AdjustHeightToFit()
        {
            int maxPanelHeight = 0;
            foreach (PreferencesPanel panel in preferencesPanelList)
            {
                maxPanelHeight = Math.Max(maxPanelHeight, GetPanelHeightRequired(panel));
            }
            panelPreferences.Height = maxPanelHeight;
            Height = maxPanelHeight + (int)Math.Ceiling(DisplayHelper.ScaleY(100));
        }

        private int GetPanelHeightRequired(PreferencesPanel preferencesPanel)
        {
            int maxBottom = 0;
            foreach (Control c in preferencesPanel.Controls)
                maxBottom = Math.Max(maxBottom, c.Bottom);
            return maxBottom;
        }

        #endregion Protected Event Overrides

        #region Private Properties

        #endregion

        #region Private Methods

        /// <summary>
        /// Helper method to save Preferences.
        /// Returns true if saved successfully.
        /// </summary>
        protected virtual bool SavePreferences()
        {
            TabSwitcher tabSwitcher = new TabSwitcher(sideBarControl);

            for (int i = 0; i < preferencesPanelList.Count; i++)
            {
                PreferencesPanel preferencesPanel = (PreferencesPanel)preferencesPanelList[i];
                tabSwitcher.Tab = i;
                if (!preferencesPanel.PrepareSave(new PreferencesPanel.SwitchToPanel(tabSwitcher.Switch)))
                {
                    return false;
                }
            }

            //	Save every PreferencesPanel.
            for (int i = 0; i < preferencesPanelList.Count; i++)
            {
                PreferencesPanel preferencesPanel = (PreferencesPanel)preferencesPanelList[i];
                if (preferencesPanel != null)
                    preferencesPanel.Save();
            }

            //	Disable the Apply button.
            buttonApply.Enabled = false;

            //notify listeners that the preferences where saved.
            OnPreferencesSaved(EventArgs.Empty);

            return true;
        }

        internal class TabSwitcher
        {
            private SideBarControl control;
            internal TabSwitcher(SideBarControl control)
            {
                this.control = control;
            }

            public int Tab;

            public void Switch()
            {
                control.SelectedIndex = Tab;
            }
        }

        /// <summary>
        /// Removes all unselected panels.
        /// </summary>
        private void RemoveUnselectedPanels()
        {
            if (!initialized)
                return;

            for (int i = 0; i < preferencesPanelList.Count; i++)
            {
                if (i != sideBarControl.SelectedIndex && preferencesPanelList[i] != null)
                {
                    PreferencesPanel preferencesPanel = (PreferencesPanel)preferencesPanelList[i];
                    if (panelPreferences.Controls.Contains(preferencesPanel))
                        panelPreferences.Controls.Remove(preferencesPanel);
                }
            }
        }

        private void OffsetFromIdenticalOwner()
        {
            if (Win32Owner != null)
            {
                RECT ownerRect = new RECT();
                RECT prefsRect = new RECT();
                if (User32.GetWindowRect(Win32Owner.Handle, ref ownerRect) && User32.GetWindowRect(Handle, ref prefsRect))
                {
                    if ((ownerRect.right - ownerRect.left) == (prefsRect.right - prefsRect.left))
                    {
                        // adjust location
                        StartPosition = FormStartPosition.Manual;
                        Location = new Point(ownerRect.left - SystemInformation.CaptionHeight, ownerRect.top - SystemInformation.CaptionHeight);
                    }
                }
            }
        }

        #endregion Private Methods

        #region Private Event Handlers

        /// <summary>
        /// sideBarControl_SelectedIndexChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void sideBarControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            //	Make the selected PreferencesPanel visible.
            if (sideBarControl.SelectedIndex < preferencesPanelList.Count && preferencesPanelList[sideBarControl.SelectedIndex] != null)
            {
                PreferencesPanel preferencesPanel = (PreferencesPanel)preferencesPanelList[sideBarControl.SelectedIndex];
                if (BidiHelper.IsRightToLeft && preferencesPanel.RightToLeft != RightToLeft.Yes)
                    preferencesPanel.RightToLeft = RightToLeft.Yes;
                BidiHelper.RtlLayoutFixup(preferencesPanel);
                panelPreferences.Controls.Add(preferencesPanel);
                if (ShowKeyboardCues)
                {
                    //fix bug 406441, if the show cues window messages have been sent to the form
                    //resend them to force the new control to show them
                    ControlHelper.HideAccelerators(this);
                    ControlHelper.ShowAccelerators(this);
                }

                if (ShowFocusCues)
                {
                    //fix bug 406420, if the show cues window messages have been sent to the form
                    //resend them to force the new control to show them
                    ControlHelper.HideFocus(this);
                    ControlHelper.ShowFocus(this);
                }
                preferencesPanel.BringToFront();
            }

            //	Remove unselected panels.
            RemoveUnselectedPanels();
        }

        /// <summary>
        /// preferencesPanel_Modified event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void preferencesPanel_Modified(object sender, EventArgs e)
        {
            buttonApply.Enabled = true;
        }

        /// <summary>
        /// buttonOK_Click event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (SavePreferences())
                DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// buttonCancel_Click event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// buttonApply_Click event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void buttonApply_Click(object sender, EventArgs e)
        {
            SavePreferences();
        }

        #endregion Event Handlers
    }
}
