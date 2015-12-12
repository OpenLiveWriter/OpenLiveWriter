// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework.Preferences;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.Commands;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.PostEditor.Configuration.Accounts;

namespace OpenLiveWriter.PostEditor
{

    public class PostEditorPreferencesEditor : IDisposable
    {
        public PostEditorPreferencesEditor(IWin32Window owner, IBlogPostEditingSite editingSite)
        {
            // save reference to owner
            _owner = owner;
            _editingSite = editingSite;

            // initialize commands
            InitializeCommands();
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Show the preferences form
        /// </summary>
        public void EditPreferences()
        {
            PreferencesHandler.Instance.ShowPreferences(_owner, _editingSite, (Type)null);
        }

        /// <summary>
        /// Show the preferences form
        /// </summary>
        public void EditAccounts()
        {
            PreferencesHandler.Instance.ShowPreferences(_owner, _editingSite, typeof(WeblogAccountPreferencesPanel));
        }

        /// <summary>
        /// Show the preferences form
        /// </summary>
        public void EditPluginsPreferences()
        {
            PreferencesHandler.Instance.ShowPreferences(_owner, _editingSite, typeof(PluginsPreferencesPanel));
        }

        private void InitializeCommands()
        {
            // initialize commands
            _editingSite.CommandManager.BeginUpdate();

            // command accounts
            _editingSite.CommandManager.Add(CommandId.Accounts, _commandAccounts_Execute);

            // command options
            _editingSite.CommandManager.Add(CommandId.Options, _commandPreferences_Execute);

            _editingSite.CommandManager.Add(CommandId.ManagePlugins, commandManagePluginsDialog_Execute);

            _editingSite.CommandManager.EndUpdate();
        }

        void commandManagePluginsDialog_Execute(object sender, EventArgs e)
        {
            EditPluginsPreferences();
        }

        private void _commandPreferences_Execute(object sender, EventArgs e)
        {
            EditPreferences();
        }

        private void _commandAccounts_Execute(object sender, EventArgs e)
        {
            EditAccounts();
        }

        private IWin32Window _owner;
        private IBlogPostEditingSite _editingSite;

    }
}
