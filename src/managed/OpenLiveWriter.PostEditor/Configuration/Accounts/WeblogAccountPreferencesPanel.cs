// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.ApplicationFramework.Preferences;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.Configuration.Accounts
{
    public class WeblogAccountPreferencesPanel : PreferencesPanel, IBlogPostEditingSitePreferences
    {
        private OpenLiveWriter.PostEditor.Configuration.Accounts.WeblogAccountManagementControl weblogAccountManagementControl1;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;
        private System.Windows.Forms.GroupBox groupBoxOptions;
        private System.Windows.Forms.CheckBox checkBoxAllowAutoUpdate;
        private System.Windows.Forms.CheckBox checkBoxAllowProviderButtons;

        private WeblogAccountPreferences _weblogAccountPreferences;

        public WeblogAccountPreferencesPanel()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            groupBoxOptions.Text = Res.Get(StringId.Options);
            checkBoxAllowAutoUpdate.Text = Res.Get(StringId.AllowAutoUpdate);
            checkBoxAllowProviderButtons.Text = Res.Get(StringId.AllowProviderExtensions);
            PanelName = Res.Get(StringId.PanelNameAccounts);

            PanelBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Configuration.Accounts.Images.AccountPanelBitmap.png");

            // initialize preferences
            _weblogAccountPreferences = new WeblogAccountPreferences();
            _weblogAccountPreferences.PreferencesModified += new EventHandler(_weblogAccountPreferences_PreferencesModified);

            checkBoxAllowAutoUpdate.Checked = _weblogAccountPreferences.AllowSettingsAutoUpdate;
            checkBoxAllowAutoUpdate.CheckedChanged += new EventHandler(checkBoxAllowAutoUpdate_CheckedChanged);

            checkBoxAllowProviderButtons.Checked = _weblogAccountPreferences.AllowProviderButtons;
            checkBoxAllowProviderButtons.CheckedChanged += new EventHandler(checkBoxAllowProviderButtons_CheckedChanged);

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!DesignMode)
            {
                LayoutHelper.FixupGroupBox(groupBoxOptions);
                checkBoxAllowAutoUpdate.Height += 1;
                checkBoxAllowProviderButtons.Height += 1;
            }
        }

        private void checkBoxAllowAutoUpdate_CheckedChanged(object sender, EventArgs e)
        {
            _weblogAccountPreferences.AllowSettingsAutoUpdate = checkBoxAllowAutoUpdate.Checked;
        }

        private void checkBoxAllowProviderButtons_CheckedChanged(object sender, EventArgs e)
        {
            _weblogAccountPreferences.AllowProviderButtons = checkBoxAllowProviderButtons.Checked;
        }

        public override void Save()
        {
            if (_weblogAccountPreferences.IsModified())
            {
                _weblogAccountPreferences.Save();

                if (_editingSite != null)
                    _editingSite.NotifyWeblogSettingsChanged(false);
            }
        }

        private void _weblogAccountPreferences_PreferencesModified(object sender, EventArgs e)
        {
            OnModified(EventArgs.Empty);
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

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.weblogAccountManagementControl1 = new OpenLiveWriter.PostEditor.Configuration.Accounts.WeblogAccountManagementControl();
            this.groupBoxOptions = new System.Windows.Forms.GroupBox();
            this.checkBoxAllowAutoUpdate = new System.Windows.Forms.CheckBox();
            this.checkBoxAllowProviderButtons = new System.Windows.Forms.CheckBox();
            this.groupBoxOptions.SuspendLayout();
            this.SuspendLayout();
            //
            // weblogAccountManagementControl1
            //
            this.weblogAccountManagementControl1.BlogSettingsEditors = null;
            this.weblogAccountManagementControl1.EditingSite = null;
            this.weblogAccountManagementControl1.Location = new System.Drawing.Point(8, 32);
            this.weblogAccountManagementControl1.Name = "weblogAccountManagementControl1";
            this.weblogAccountManagementControl1.Size = new System.Drawing.Size(345, 214);
            this.weblogAccountManagementControl1.TabIndex = 1;
            //
            // groupBoxOptions
            //
            this.groupBoxOptions.Controls.Add(this.checkBoxAllowAutoUpdate);
            this.groupBoxOptions.Controls.Add(this.checkBoxAllowProviderButtons);
            this.groupBoxOptions.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxOptions.Location = new System.Drawing.Point(8, 256);
            this.groupBoxOptions.Name = "groupBoxOptions";
            this.groupBoxOptions.Size = new System.Drawing.Size(342, 102);
            this.groupBoxOptions.TabIndex = 2;
            this.groupBoxOptions.TabStop = false;
            this.groupBoxOptions.Text = "Options";
            //
            // checkBoxAllowAutoUpdate
            //
            this.checkBoxAllowAutoUpdate.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.checkBoxAllowAutoUpdate.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxAllowAutoUpdate.Location = new System.Drawing.Point(16, 24);
            this.checkBoxAllowAutoUpdate.Name = "checkBoxAllowAutoUpdate";
            this.checkBoxAllowAutoUpdate.Size = new System.Drawing.Size(320, 32);
            this.checkBoxAllowAutoUpdate.TabIndex = 0;
            this.checkBoxAllowAutoUpdate.Text = "Automatically &update account information (categories, links, capabilities, and p" +
                "rovider extensions)";
            this.checkBoxAllowAutoUpdate.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            //
            // checkBoxAllowProviderButtons
            //
            this.checkBoxAllowProviderButtons.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.checkBoxAllowProviderButtons.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxAllowProviderButtons.Location = new System.Drawing.Point(16, 56);
            this.checkBoxAllowProviderButtons.Name = "checkBoxAllowProviderButtons";
            this.checkBoxAllowProviderButtons.Size = new System.Drawing.Size(315, 32);
            this.checkBoxAllowProviderButtons.TabIndex = 3;
            this.checkBoxAllowProviderButtons.Text = "Allow &weblog provider extensions (custom buttons which appear in the sidebar)";
            this.checkBoxAllowProviderButtons.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            //
            // WeblogAccountPreferencesPanel
            //
            this.Controls.Add(this.groupBoxOptions);
            this.Controls.Add(this.weblogAccountManagementControl1);
            this.Name = "WeblogAccountPreferencesPanel";
            this.PanelName = "Accounts";
            this.Controls.SetChildIndex(this.weblogAccountManagementControl1, 0);
            this.Controls.SetChildIndex(this.groupBoxOptions, 0);
            this.groupBoxOptions.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        #region IBlogPostEditingSitePreferences Members

        public IBlogPostEditingSite EditingSite
        {
            get
            {
                return _editingSite;
            }
            set
            {
                _editingSite = value;
                weblogAccountManagementControl1.EditingSite = value;
            }
        }

        private IBlogPostEditingSite _editingSite;

        #endregion

    }
}
