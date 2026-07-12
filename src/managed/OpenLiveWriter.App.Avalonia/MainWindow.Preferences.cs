// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using OpenLiveWriter.App.Avalonia.Dialogs;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.App.Avalonia.Settings;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Publishing;

namespace OpenLiveWriter.App.Avalonia
{
    /// <summary>
    /// Options / Preferences command wiring and live preference application for the shell.
    /// </summary>
    public partial class MainWindow
    {
        private AppPreferencesStore _preferencesStore;
        private AppPreferences _preferences;

        private void InitializePreferences()
        {
            try
            {
                _preferencesStore = AppPreferencesStore.CreateDefault();
                _preferences = _preferencesStore.Load();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OLW-Preferences] Settings unavailable: {ex.Message}");
                _preferences = AppPreferences.CreateDefault();
                return;
            }

            // Apply persisted values without awaiting (startup).
            _ = ApplyPreferencesAsync(_preferences);
        }

        private async Task<bool> TryHandleOptionsCommandAsync(CommandId commandId)
        {
            if (commandId != CommandId.Options)
                return false;

            await ShowPreferencesAsync();
            return true;
        }

        private async Task ShowPreferencesAsync()
        {
            var snapshot = _preferences?.Clone() ?? AppPreferences.CreateDefault();
            bool saved = await PreferencesDialog.ShowAsync(
                this,
                snapshot,
                _accountService,
                ApplyAndPersistPreferencesAsync);

            if (saved)
                UpdateStatus("Preferences saved.");
        }

        private async Task ApplyAndPersistPreferencesAsync(AppPreferences prefs)
        {
            _preferences = prefs;
            _preferencesStore?.Save(prefs);
            await ApplyPreferencesAsync(prefs);
        }

        private async Task ApplyPreferencesAsync(AppPreferences prefs)
        {
            if (prefs == null)
                return;

            await SetSpellcheckEnabledAsync(prefs.SpellcheckEnabled);
            _showRealTimeWordCount = prefs.ShowRealTimeWordCount;
            UpdateStatusBarExtras();

            var autoreplace = AutoreplaceOptions.FromPreferences(prefs);
            WebViewEditor editor = GetEditor();
            if (editor != null)
                await editor.SetAutoreplaceOptionsAsync(autoreplace);
        }

        /// <summary>Builds a proxy-aware HTTP client from the current preference snapshot.</summary>
        internal HttpClient CreatePublishingHttpClient() =>
            PublishingHttpClientFactory.Create(WebProxyMapper.ToConfiguration(_preferences));

        /// <summary>Current preference snapshot (for tests).</summary>
        public AppPreferences CurrentPreferences => _preferences ?? AppPreferences.CreateDefault();
    }
}
