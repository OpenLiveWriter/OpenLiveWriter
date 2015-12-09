// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using OpenLiveWriter.ApplicationFramework.Preferences;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Autoreplace
{
    class AutoreplacePreferencesPanel : PreferencesPanel
    {

        public AutoreplacePreferencesPanel()
        {
            InitializeComponent();
            PanelName = Res.Get(StringId.AutoreplacePrefererencesPanel);
            PanelBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.PreferencesAutoreplace.png");
            autoreplaceManagementControl1.Preferences = _autoreplacePreferences;
            _autoreplacePreferences.PreferencesModified += _autoreplacePreferences_PreferencesModified;
        }

        void _autoreplacePreferences_PreferencesModified(object sender, EventArgs e)
        {
            OnModified(EventArgs.Empty);
        }

        private AutoreplaceManagementControl autoreplaceManagementControl1;
        private readonly AutoreplacePreferences _autoreplacePreferences = new AutoreplacePreferences();

        public override bool PrepareSave(SwitchToPanel switchToPanel)
        {
            return true;
        }

        public override void Save()
        {
            _autoreplacePreferences.Save();
            base.Save();
        }

        private void InitializeComponent()
        {
            this.autoreplaceManagementControl1 = new OpenLiveWriter.PostEditor.Autoreplace.AutoreplaceManagementControl();
            this.SuspendLayout();
            //
            // autoreplaceManagementControl1
            //
            this.autoreplaceManagementControl1.Location = new System.Drawing.Point(8, 32);
            this.autoreplaceManagementControl1.Name = "autoreplaceManagementControl1";
            this.autoreplaceManagementControl1.Preferences = null;
            this.autoreplaceManagementControl1.Size = new System.Drawing.Size(345, 245);
            this.autoreplaceManagementControl1.TabIndex = 1;
            //
            // AutoreplacePreferencesPanel
            //
            this.Controls.Add(this.autoreplaceManagementControl1);
            this.Name = "AutoreplacePreferencesPanel";
            this.Controls.SetChildIndex(this.autoreplaceManagementControl1, 0);
            this.ResumeLayout(false);

        }

    }
}
