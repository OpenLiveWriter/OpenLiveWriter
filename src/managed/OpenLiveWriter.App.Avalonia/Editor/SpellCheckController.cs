// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.Platform;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    /// <summary>
    /// Pure helpers for the spell-check UI. Actual checking + underlining is done
    /// natively by macOS/WebKit on the <c>contenteditable</c> body (which ships with
    /// <c>spellcheck="true"</c>); this only decides the attribute value and status
    /// message, and builds the bridge script that toggles the body attribute. Kept
    /// pure so the toggle command and status text are unit-testable without a live
    /// WebView or the real OS spell service.
    /// </summary>
    public static class SpellCheckController
    {
        /// <summary>Editor body spell-check is on by default (matches the editor HTML).</summary>
        public const bool DefaultEnabled = true;

        /// <summary>The <c>spellcheck</c> attribute value for the given toggle state.</summary>
        public static string SpellcheckAttributeValue(bool enabled) => enabled ? "true" : "false";

        /// <summary>Builds the bridge call that toggles the editor body's spellcheck attribute.</summary>
        public static string BuildSetSpellcheckScript(bool enabled) =>
            "OLWBridge.setSpellcheck(" + (enabled ? "true" : "false") + ")";

        /// <summary>
        /// Produces a human-readable status line for the Spelling command. When the
        /// platform spell provider reports availability, the message notes the OS
        /// dictionary is in use; otherwise it explains that live underlines still come
        /// from the editor. Null provider is treated as unavailable.
        /// </summary>
        public static string DescribeStatus(ISpellCheckProvider provider, string language, bool enabled)
        {
            if (!enabled)
                return "Spell-check underlines are turned off. Enable them in Preferences \u203a Spelling.";

            bool available = provider != null && provider.IsAvailable(language);
            return available
                ? "Spelling is checked as you type using the macOS system dictionary."
                : "Spelling is checked as you type using the built-in editor underlines.";
        }
    }
}
