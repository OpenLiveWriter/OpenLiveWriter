// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{
    public class SpellingLanguageEntry
    {
        public readonly SpellingCheckerLanguage Language;
        public readonly ushort LCID;
        public readonly string CSAPIEngine;
        public readonly string TwoLetterIsoLanguageName;
        public readonly string DisplayName;
        public string[] CSAPILex { get; private set; }

        public SpellingLanguageEntry(SpellingCheckerLanguage language, ushort lcid, string csapiEngine, string[] csapiLex, string twoLetterIsoLanguageName, string displayName)
        {
            // TODO
            this.Language = language;
            this.CSAPILex = csapiLex;
            this.CSAPIEngine = csapiEngine;
            this.LCID = lcid;
            this.TwoLetterIsoLanguageName = twoLetterIsoLanguageName;
            this.DisplayName = displayName;
        }

        public bool IsInstalled(string lexiconPath)
        {
            // TODO
            return false;
        }

        public override string ToString()
        {
            return this.DisplayName;
        }
    }
}
