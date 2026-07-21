// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Threading.Tasks;
using global::Avalonia.Threading;
using OpenLiveWriter.App.Avalonia.Editor;

namespace OpenLiveWriter.App.Avalonia
{
    /// <summary>
    /// Draft autosave wiring: drives an <see cref="AutosaveController"/> from a
    /// DispatcherTimer at the preference-configured interval and reports each save
    /// on the status bar. Honors the AutoSaveDrafts / AutoSaveMinutes preferences.
    /// </summary>
    public partial class MainWindow
    {
        private AutosaveController _autosave;
        private DispatcherTimer _autosaveTimer;

        private void InitializeAutosave()
        {
            if (_draftSession == null)
                return;

            _autosave = new AutosaveController(_draftSession, () => CurrentPreferences, CaptureContentAsync);
            _autosave.Autosaved += (s, e) =>
            {
                UpdateWindowTitle();
                UpdateStatus("Draft autosaved");
            };

            _autosaveTimer = new DispatcherTimer { Interval = _autosave.Interval };
            _autosaveTimer.Tick += async (s, e) => await _autosave.TickAsync();
            _autosaveTimer.Start();
        }

        // Pulls the live title + body for an autosave pass (same inputs as Save Draft).
        private async Task<(string Title, string BodyHtml)> CaptureContentAsync()
        {
            WebViewEditor editor = GetEditor();
            string html = editor != null ? await editor.GetContentAsync() : null;
            return (_titleEditor?.Text ?? string.Empty, html);
        }

        // Re-reads the interval after the Preferences dialog saves new values.
        private void RefreshAutosaveInterval()
        {
            if (_autosaveTimer != null && _autosave != null)
                _autosaveTimer.Interval = _autosave.Interval;
        }
    }
}
