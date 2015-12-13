// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.SpellChecker
{

    /// <summary>
    /// Interface representing a word-range to be spell checked
    /// </summary>
    [Guid("F4FB57BC-5DB2-484A-8CDC-1EA270BE3821")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    public interface IWordRange
    {
        /// <summary>
        /// Is there another word in the range?
        /// </summary>
        /// <returns>true if there is another word in the range</returns>
        bool HasNext() ;

        /// <summary>
        /// Advance to the next word in the range
        /// </summary>
        void Next() ;

        /// <summary>
        /// Get the current word
        /// </summary>
        string CurrentWord { get; }

        /// <summary>
        /// Place the cursor
        /// </summary>
        void PlaceCursor() ;

        /// <summary>
        /// Highlight the current word, adjusted by the offset and length.
        /// The offset and length do not change the current word,
        /// they just affect the application of the highlight.
        /// </summary>
        void Highlight(int offset, int length) ;

        /// <summary>
        /// Remove highlighting from the range
        /// </summary>
        void RemoveHighlight() ;

        /// <summary>
        /// Replace the current word
        /// </summary>
        void Replace(int offset, int length, string newText) ;

        /// <summary>
        /// Tests the current word to determine if it is part of a URL sequence.
        /// </summary>
        /// <returns></returns>
        bool IsCurrentWordUrlPart();

        /// <summary>
        /// Tests the current word to determine if it is contained in smart content.
        /// </summary>
        /// <returns></returns>
        bool FilterApplies();

        /// <summary>
        /// Tests the current word from an offset for a given length to determine if it is contained in smart content.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        bool FilterAppliesRanged(int offset, int length);
    }

    public static class WordRangeHelper
    {
        public static bool ContainsOnlySymbols(string currentWord)
        {
            // http://en.wikipedia.org/wiki/CJK_Unified_Ideographs
            // http://en.wikipedia.org/wiki/Japanese_writing_system

            // Look to see if the word is only chinese and japanese characters
            foreach (char c in currentWord)
            {
                if (IsNonSymbolChar(c))
                    return false;
            }

            return true;
        }

        public static bool IsNonSymbolChar(char c)
        {
            // Found a latin char, we should spellcheck this word
            if ((c < 19968 || c > 40959) && (c < 12352 || c > 12543))
                return true;

            return false;
        }
    }
}
