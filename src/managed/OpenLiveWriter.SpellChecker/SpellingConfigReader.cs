// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using OpenLiveWriter.CoreServices.ResourceDownloading;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.SpellChecker
{
    internal class SpellingConfigReader
    {
        private static readonly SpellingLanguageEntry[] languages;
        private static readonly SpellingCheckerLanguage defaultLanguage;

        public static SpellingLanguageEntry[] Languages
        {
            get { return languages; }
        }

        public static SpellingCheckerLanguage DefaultLanguage
        {
            get { return defaultLanguage; }
        }


        static SpellingConfigReader()
        {
            // TODO:OLW
            if (languages == null)
            {
                languages = new SpellingLanguageEntry[]
                    {
                        CreateNullDictionaryLanguage()
                    };
            }
            defaultLanguage = SpellingCheckerLanguage.None;
        }

        private static SpellingLanguageEntry CreateNullDictionaryLanguage()
        {
            return new SpellingLanguageEntry(SpellingCheckerLanguage.None, 0xFFFF, null, new string[0], "", Res.Get(StringId.DictionaryLanguageNone));
        }
    }
}
