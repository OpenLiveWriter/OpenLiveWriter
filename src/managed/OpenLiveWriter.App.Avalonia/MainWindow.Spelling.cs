// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Threading.Tasks;
using OpenLiveWriter.App.Avalonia.Dialogs;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.App.Avalonia.Settings;
using OpenLiveWriter.App.Avalonia.Spelling;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.App.Avalonia
{
    /// <summary>
    /// Spell-check behavior for the shell. As-you-type underlines stay native
    /// (macOS/WebKit on the contenteditable body, toggled by the Spelling
    /// preference); the F7 / ribbon Spelling command opens the modal
    /// <see cref="SpellingDialog"/> backed by the managed Hunspell engine
    /// (<see cref="HunspellSpellCheckEngine"/>), and this partial also owns the
    /// check-before-publish gate consulted by the publish flow.
    /// </summary>
    public partial class MainWindow
    {
        private ISpellCheckEngine _spellEngine;
        private bool _spellEngineFailed;
        private bool _spellcheckEnabled = SpellCheckController.DefaultEnabled;

        private void InitializeSpelling()
        {
            // The engine is created lazily on first use (F7 or a gated publish) so
            // shell startup never pays the dictionary load.
        }

        private async Task<bool> TryHandleSpellingCommandAsync(CommandId commandId)
        {
            switch (commandId)
            {
                case CommandId.CheckSpelling:
                case CommandId.OpenSpellingForm:
                    await RunSpellingCheckAsync();
                    return true;
                default:
                    return false;
            }
        }

        // Lazily builds the Hunspell engine; returns null (once, cached) when the
        // dictionary resources cannot be loaded so spelling degrades gracefully.
        private ISpellCheckEngine GetOrCreateSpellEngine()
        {
            if (_spellEngine == null && !_spellEngineFailed)
            {
                try
                {
                    _spellEngine = HunspellSpellCheckEngine.CreateDefault();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[OLW-Spelling] Engine unavailable: {ex.Message}");
                }
                _spellEngineFailed = _spellEngine == null;
            }
            return _spellEngine;
        }

        // F7 / ribbon Spelling: walks the post's misspellings in the modal dialog and
        // pushes the corrected HTML back into the editor when anything changed.
        private async Task RunSpellingCheckAsync()
        {
            WebViewEditor editor = GetEditor();
            if (editor == null)
            {
                UpdateStatus("Editor not ready.");
                return;
            }

            ISpellCheckEngine engine = GetOrCreateSpellEngine();
            if (engine == null || !engine.IsAvailable)
            {
                await MessageDialog.ShowAsync(this, "Spelling",
                    "Spell checking is unavailable: the spelling dictionary could not be loaded.");
                return;
            }

            string html = await editor.GetContentAsync() ?? string.Empty;
            if (SpellingSession.CountMisspellings(html, engine) == 0)
            {
                UpdateStatus("Spelling check complete — no misspellings.");
                await MessageDialog.ShowAsync(this, "Spelling",
                    "The spelling check is complete. No misspellings were found.");
                return;
            }

            SpellingDialog dialog = await SpellingDialog.ShowAsync(this, html, engine);
            if (dialog.WasModified)
            {
                await editor.SetContentAsync(dialog.ResultHtml);
                _draftSession?.MarkDirty();
                UpdateWindowTitle();
            }
            UpdateStatus("Spelling check complete.");
        }

        /// <summary>
        /// Enforces the "check spelling before publishing" preference. Returns false
        /// when the user cancels the publish from the misspelling prompt.
        /// </summary>
        private async Task<bool> ConfirmSpellingGateAsync(string html)
        {
            var prefs = _preferences ?? AppPreferences.CreateDefault();
            if (!prefs.CheckSpellingBeforePublishing)
                return true;

            ISpellCheckEngine engine = GetOrCreateSpellEngine();
            if (engine == null || !engine.IsAvailable)
                return true;

            int count = SpellingSession.CountMisspellings(html ?? string.Empty, engine);
            if (count == 0)
                return true;

            return await ConfirmDialog.ShowConfirmAsync(
                this,
                "Spelling",
                $"{count} possible misspelling{(count == 1 ? string.Empty : "s")} found. Publish anyway?");
        }

        /// <summary>Whether editor-body spell-check underlines are currently enabled.</summary>
        public bool SpellcheckEnabled => _spellcheckEnabled;

        /// <summary>
        /// Applies the spell-check enable/disable preference to the editor body (via the
        /// bridge) and remembers it. Called by the Preferences dialog.
        /// </summary>
        public async Task SetSpellcheckEnabledAsync(bool enabled)
        {
            _spellcheckEnabled = enabled;
            var editor = GetEditor();
            if (editor != null)
                await editor.SetSpellcheckEnabledAsync(enabled);
        }
    }
}
