// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;

namespace OpenLiveWriter.App.Avalonia.Spelling
{
    /// <summary>
    /// Seam behind the on-demand spell engine used by the Spelling dialog and the
    /// check-before-publish gate. This is distinct from the as-you-type underlines
    /// (native WKWebView spellcheck) and from the platform <c>ISpellCheckProvider</c>
    /// (status reporting); implementations wrap a real dictionary
    /// (<see cref="HunspellSpellCheckEngine"/>) or an in-memory word list
    /// (<see cref="InMemorySpellCheckEngine"/>) for tests.
    ///
    /// <see cref="Check"/> also honors the session ignore list and the persisted user
    /// dictionary, so callers never need to apply those filters themselves.
    /// </summary>
    public interface ISpellCheckEngine
    {
        /// <summary>True when a real dictionary is loaded and checking can run.</summary>
        bool IsAvailable { get; }

        /// <summary>
        /// True when the word is correct: in the main dictionary, in the user
        /// dictionary, or in the session ignore list.
        /// </summary>
        bool Check(string word);

        /// <summary>Suggested corrections for a misspelled word (may be empty).</summary>
        IReadOnlyList<string> Suggest(string word);

        /// <summary>Ignores the word for the remainder of the engine's lifetime (session).</summary>
        void IgnoreAll(string word);

        /// <summary>Adds the word to the persisted user dictionary (when persistence is configured).</summary>
        void AddToUserDictionary(string word);
    }
}
