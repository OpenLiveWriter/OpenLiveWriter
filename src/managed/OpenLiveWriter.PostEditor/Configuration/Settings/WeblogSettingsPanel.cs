// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.ApplicationFramework.Preferences;

namespace OpenLiveWriter.PostEditor.Configuration.Settings
{
    /// <summary>
    /// Summary description for WeblogSettingsPanel.
    /// </summary>
    public class WeblogSettingsPanel : PreferencesPanel
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private TemporaryBlogSettings _targetBlogSettings;
        private TemporaryBlogSettings _editableBlogSettings;

        public WeblogSettingsPanel()
            : base()
        {
            InitializeComponent();
        }

        public WeblogSettingsPanel(TemporaryBlogSettings targetBlogSettings, TemporaryBlogSettings editableBlogSettings)
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            _targetBlogSettings = targetBlogSettings;
            _editableBlogSettings = editableBlogSettings;

        }

        public override void Save()
        {
            if (TemporaryBlogSettingsModified)
            {
                _targetBlogSettings.CopyFrom(_editableBlogSettings);
            }
        }

        protected TemporaryBlogSettings TemporaryBlogSettings
        {
            get
            {
                return _editableBlogSettings;
            }
        }

        protected bool TemporaryBlogSettingsModified
        {
            get
            {
                return _settingsModified;
            }
            set
            {
                _settingsModified = value;
                if (_settingsModified)
                    OnModified(EventArgs.Empty);
            }

        }
        private bool _settingsModified = false;

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
            components = new System.ComponentModel.Container();
        }
        #endregion
    }
}
