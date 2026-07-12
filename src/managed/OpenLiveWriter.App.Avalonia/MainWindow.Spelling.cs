// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Threading.Tasks;
using OpenLiveWriter.App.Avalonia.Dialogs;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.App.Avalonia
{
    /// <summary>
    /// Spell-check behavior for the shell. Checking + underlining is native
    /// (macOS/WebKit on the contenteditable body); this surfaces a Spelling status
    /// command and the enable/disable preference that toggles the body's spellcheck
    /// attribute through the editor bridge.
    /// </summary>
    public partial class MainWindow
    {
        private SpellCheckService _spellCheck;
        private bool _spellcheckEnabled = SpellCheckController.DefaultEnabled;

        private void InitializeSpelling()
        {
            _spellCheck = SpellCheckService.CreateDefault();
        }

        private async Task<bool> TryHandleSpellingCommandAsync(CommandId commandId)
        {
            switch (commandId)
            {
                case CommandId.CheckSpelling:
                case CommandId.OpenSpellingForm:
                    await ShowSpellingStatusAsync();
                    return true;
                default:
                    return false;
            }
        }

        private async Task ShowSpellingStatusAsync()
        {
            _spellCheck ??= SpellCheckService.CreateDefault();
            string status = _spellCheck.StatusMessage(_spellcheckEnabled);
            UpdateStatus(status);
            await MessageDialog.ShowAsync(this, "Spelling", status);
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
