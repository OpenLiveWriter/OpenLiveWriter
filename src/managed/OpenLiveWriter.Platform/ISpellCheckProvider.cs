// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Platform
{
    /// <summary>
    /// Cross-platform spell check abstraction.
    /// </summary>
    public interface ISpellCheckProvider
    {
        bool IsWordCorrect(string word, string language);
        string[] GetSuggestions(string word, string language);
        void AddToUserDictionary(string word, string language);
        bool IsAvailable(string language);
    }
}
