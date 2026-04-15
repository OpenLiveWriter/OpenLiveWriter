// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Platform.Mac
{
    public class MacSpellCheckProvider : ISpellCheckProvider
    {
        // Stub - will integrate with NSSpellChecker in a future milestone
        public bool IsWordCorrect(string word, string language) => true;
        public string[] GetSuggestions(string word, string language) => Array.Empty<string>();
        public void AddToUserDictionary(string word, string language) { }
        public bool IsAvailable(string language) => false;
    }
}
