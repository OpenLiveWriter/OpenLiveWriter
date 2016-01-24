// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{
    public class SpellingLanguageEntry
    {
        public readonly string BCP47Code;
        public readonly string DisplayName;

        public SpellingLanguageEntry(string bcp47Code, string displayName)
        {
            this.BCP47Code = bcp47Code;
            this.DisplayName = displayName;
        }

        public override string ToString()
        {
            return this.DisplayName;
        }
    }
}
