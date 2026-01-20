// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Spelling checking services for the HTML Editor.
    /// Extends the base SpellChecker interface with post-specific context.
    /// </summary>
    public interface IBlogPostSpellCheckingContext : SpellChecker.IBlogPostSpellCheckingContext
    {
        string PostSpellingContextDirectory { get; }
    }
}
