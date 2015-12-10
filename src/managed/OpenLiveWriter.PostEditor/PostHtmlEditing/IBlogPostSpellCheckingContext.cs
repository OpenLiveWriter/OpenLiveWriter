// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Spelling checking services for the HTML Editor.
    /// </summary>
    public interface IBlogPostSpellCheckingContext
    {
        string PostSpellingContextDirectory { get; }
    }
}
