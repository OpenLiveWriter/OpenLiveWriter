// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Platform.Mac
{
    /// <summary>
    /// Legacy platform spell provider seam, superseded: the real on-demand spell
    /// engine (Hunspell) lives in App.Avalonia (<c>Spelling/HunspellSpellCheckEngine</c>),
    /// and as-you-type underlines are native WKWebView spellcheck. This stub remains
    /// only so the platform <see cref="ISpellCheckProvider"/> surface (status
    /// reporting via <c>SpellCheckService</c>) has an implementation; it deliberately
    /// reports unavailability so the shell describes the built-in underlines.
    /// </summary>
    public class MacSpellCheckProvider : ISpellCheckProvider
    {
        public bool IsWordCorrect(string word, string language) => true;
        public string[] GetSuggestions(string word, string language) => Array.Empty<string>();
        public void AddToUserDictionary(string word, string language) { }
        public bool IsAvailable(string language) => false;
    }
}
