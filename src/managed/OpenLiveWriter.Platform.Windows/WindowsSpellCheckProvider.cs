// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.Versioning;

namespace OpenLiveWriter.Platform.Windows
{
    [SupportedOSPlatform("windows")]
    public class WindowsSpellCheckProvider : ISpellCheckProvider
    {
        public bool IsWordCorrect(string word, string language) => true;
        public string[] GetSuggestions(string word, string language) => Array.Empty<string>();
        public void AddToUserDictionary(string word, string language) { }
        public bool IsAvailable(string language) => false;
    }
}
