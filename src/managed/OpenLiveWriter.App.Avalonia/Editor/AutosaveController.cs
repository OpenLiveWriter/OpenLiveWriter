// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Threading.Tasks;
using OpenLiveWriter.App.Avalonia.Settings;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    /// <summary>
    /// UI-agnostic autosave controller, mirroring the Windows "Save AutoRecover
    /// information" behavior: when the preference is enabled and the current
    /// document is dirty, each tick persists the draft through the same
    /// <see cref="DraftSession.Save"/> path as the Save Draft command. The shell
    /// drives it from a DispatcherTimer; tests drive <see cref="TickAsync"/>
    /// directly against a temp-directory store. Preferences and content are pulled
    /// through delegates so the controller stays free of Avalonia/WebView types.
    /// </summary>
    public sealed class AutosaveController
    {
        /// <summary>Interval used when the preference carries a non-positive value.</summary>
        public const int DefaultIntervalMinutes = 3;

        private readonly DraftSession _session;
        private readonly Func<AppPreferences> _getPreferences;
        private readonly Func<Task<(string Title, string BodyHtml)>> _captureContent;

        public AutosaveController(
            DraftSession session,
            Func<AppPreferences> getPreferences,
            Func<Task<(string Title, string BodyHtml)>> captureContent)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _getPreferences = getPreferences ?? throw new ArgumentNullException(nameof(getPreferences));
            _captureContent = captureContent ?? throw new ArgumentNullException(nameof(captureContent));
        }

        /// <summary>Raised after each successful autosave (for status-bar feedback).</summary>
        public event EventHandler Autosaved;

        /// <summary>True when the AutoSaveDrafts preference is currently enabled.</summary>
        public bool IsEnabled => _getPreferences()?.AutoSaveDrafts == true;

        /// <summary>Current save interval; falls back to 3 minutes when unset.</summary>
        public TimeSpan Interval
        {
            get
            {
                int minutes = _getPreferences()?.AutoSaveMinutes ?? 0;
                if (minutes <= 0)
                    minutes = DefaultIntervalMinutes;
                return TimeSpan.FromMinutes(minutes);
            }
        }

        /// <summary>
        /// One autosave pass. Saves (and clears the dirty flag) only when the
        /// preference is enabled and the document has unsaved changes. Returns
        /// true when a save was performed.
        /// </summary>
        public async Task<bool> TickAsync()
        {
            if (!IsEnabled || !_session.IsDirty)
                return false;

            var (title, bodyHtml) = await _captureContent();
            _session.Save(title, bodyHtml ?? _session.Current.BodyHtml);
            Autosaved?.Invoke(this, EventArgs.Empty);
            return true;
        }
    }
}
