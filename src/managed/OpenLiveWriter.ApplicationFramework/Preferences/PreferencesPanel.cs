// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.ApplicationFramework.Preferences
{
    /// <summary>
    /// Preferences panel base class.
    /// </summary>
    public class PreferencesPanel : UserControl, IRtlAware
    {
        public delegate void SwitchToPanel();

        #region Component Designer generated code

        private System.Windows.Forms.Label labelPanelName;
        private System.ComponentModel.Container components = null;

        #endregion Component Designer generated code

        #region Private Member Variables

        /// <summary>
        /// The panel bitmap.
        /// </summary>
        private Bitmap panelBitmap;

        #endregion Private Member Variables

        #region Public Events

        /// <summary>
        /// Occurs when one or more preferences in the PreferencesPanel have been modified.
        /// </summary>
        public event EventHandler Modified;

        #endregion

        #region Class Initialization & Termination

        /// <summary>
        /// Initializes a new instance of the PreferencesPanel class.
        /// </summary>
        public PreferencesPanel()
        {
            // Fixes multiple bugs in right-to-left builds, that
            // occur when OnLoad gets called more than once, with different .Font values
            // depending on whether it is parented at the time.
            Font = Res.DefaultFont;

            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            this.labelPanelName.Font = Res.GetFont(FontSize.XXLarge, FontStyle.Regular);
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
            this.labelPanelName = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // labelPanelName
            //
            this.labelPanelName.BackColor = System.Drawing.SystemColors.Control;
            this.labelPanelName.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this.labelPanelName.Font = Res.GetFont(FontSize.XXLarge, FontStyle.Regular);
            this.labelPanelName.Location = new System.Drawing.Point(8, 8);
            this.labelPanelName.Name = "labelPanelName";
            this.labelPanelName.Size = new System.Drawing.Size(354, 23);
            this.labelPanelName.TabIndex = 0;
            this.labelPanelName.Text = "Options";
            //
            // PreferencesPanel
            //
            this.Controls.Add(this.labelPanelName);
            this.Name = "PreferencesPanel";
            this.Size = new System.Drawing.Size(370, 370);
            this.ResumeLayout(false);

        }
        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the panel name.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the panel name.")
        ]
        public string PanelName
        {
            get
            {
                return labelPanelName.Text;
            }
            set
            {
                labelPanelName.Text = value;
                AccessibleName = value;
            }
        }

        /// <summary>
        /// Gets or sets the panel bitmap.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the panel bitmap.")
        ]
        public Bitmap PanelBitmap
        {
            get
            {
                return panelBitmap;
            }
            set
            {
                panelBitmap = value;
            }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Tells the preferences panel to prepare for save.
        /// Any values should be validated, and an error message
        /// displayed to the user if necessary.
        ///
        /// The SwitchToPanel delegate can be called to force
        /// the containing control to display the panel.
        ///
        /// Returns true if validation was successful, false if
        /// not (i.e. the save operation should be aborted).
        /// </summary>
        public virtual bool PrepareSave(SwitchToPanel switchToPanel)
        {
            return true;
        }

        /// <summary>
        /// Saves the PreferencesPanel.
        /// </summary>
        public virtual void Save()
        {
        }

        #endregion Public Methods

        #region Protected Events

        /// <summary>
        /// Raises the Modified event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnModified(EventArgs e)
        {
            if (Modified != null)
                Modified(this, e);
        }

        #endregion Protected Events

        #region Private Event Handlers

        /// <summary>
        /// preferences_PreferencesModified event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void preferences_PreferencesModified(object sender, EventArgs e)
        {
            //	Raise the Modified event.
            OnModified(EventArgs.Empty);
        }

        #endregion Private Event Handlers

        private bool reversed = false;
        void IRtlAware.Layout()
        {
            if (!reversed)
            {
                reversed = true;
                BidiHelper.RtlLayoutFixup(this, true, true, Controls);
            }
        }
    }
}
