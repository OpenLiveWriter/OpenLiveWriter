// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using mshtml;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.SpellChecker
{
    /// <summary>
    /// Summary description for MisspelledWordInfo.
    /// </summary>
    public class MisspelledWordInfo
    {
        private MarkupRange _range;
        private string _word;

        public MisspelledWordInfo(MarkupRange range, string word)
        {
            _range = range;
            _word = word;
        }

        public MarkupRange WordRange
        {
            get
            {
                return _range;
            }
        }

        public string Word
        {
            get
            {
                return _word;
            }
        }
    }
}
