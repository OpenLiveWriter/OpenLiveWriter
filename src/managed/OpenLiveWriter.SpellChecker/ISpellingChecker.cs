// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;

namespace OpenLiveWriter.SpellChecker
{
    /// <summary>
    /// Generic interface implemented by spell checking engines
    /// </summary>
    public interface ISpellingChecker : IDisposable
    {
        /// <summary>
        /// Notify the spell checker that we are going to start checking a document
        /// and that we would like the user's Ignore All and Replace All commands
        /// to be persisted in a context-bound dictionary
        /// </summary>
        void StartChecking() ;

        /// <summary>
        /// Notify the spell checker that we have stopped checking the document
        /// </summary>
        void StopChecking() ;

        bool IsInitialized { get; }

        /// <summary>
        /// Check the spelling of the specified word
        /// </summary>
        /// <param name="word">word to check</param>
        /// <param name="otherWord">auto or conditional replace word (returned only
        /// for certain SpellCheckResult values)</param>
        /// <returns>check-word result</returns>
        SpellCheckResult CheckWord(string word, out string otherWord, out int offset, out int length) ;

        /// <summary>
        /// Suggest alternate spellings for the specified word
        /// </summary>
        /// <param name="word">word to get suggestions for</param>
        /// <param name="maxSuggestions">maximum number of suggestions to return</param>
        /// <param name="depth">depth of search -- 0 to 100 where larger values
        /// indicated a deeper (and longer) search</param>
        /// <returns>array of spelling suggestions (up to maxSuggestions long)</returns>
        SpellingSuggestion[] Suggest(string word, short maxSuggestions, short depth ) ;

        /// <summary>
        /// Add a word to the permenant user-dictionary
        /// </summary>
        /// <param name="word"></param>
        void AddToUserDictionary( string word ) ;

        event EventHandler WordAdded;

        /// <summary>
        /// Ignore all subsequent instances of the specified word
        /// </summary>
        /// <param name="word">word to ignore</param>
        void IgnoreAll( string word ) ;

        event EventHandler WordIgnored;

        /// <summary>
        /// Replace all subsequent instances of the specified word
        /// </summary>
        /// <param name="word">word to replace</param>
        /// <param name="replaceWith">replacement word</param>
        void ReplaceAll( string word, string replaceWith ) ;
    }

    /// <summary>
    /// Possible result codes from check-word call
    /// </summary>
    public enum SpellCheckResult
    {
        /// <summary>
        /// Word is correctly spelled
        /// </summary>
        Correct,

        /// <summary>
        /// Word has an auto-replace value (value returned in otherWord parameter)
        /// </summary>
        AutoReplace,

        /// <summary>
        /// Word has a conditional-replace value (value returned in otherWord parameter)
        /// </summary>
        ConditionalReplace,

        /// <summary>
        /// Word is incorrectly capitalized
        /// </summary>
        Capitalization,

        /// <summary>
        /// Word is misspelled
        /// </summary>
        Misspelled
    }

    /// <summary>
    /// Suggestion for a misspelled word
    /// </summary>
    public struct SpellingSuggestion
    {
        /// <summary>
        /// Initialize with hte appropriate suggestion and score
        /// </summary>
        /// <param name="suggestion">suggestion</param>
        /// <param name="score">score</param>
        public SpellingSuggestion( string suggestion, short score )
        {
            Suggestion = suggestion ;
            Score = score ;
        }

        /// <summary>
        /// Suggested word
        /// </summary>
        public readonly string Suggestion ;

        /// <summary>
        /// Score (0-100 percent where 100 indicates an exact match)
        /// </summary>
        public readonly short Score ;
    }

    /// <summary>
    /// Exception that occurs during spell checking
    /// </summary>
    public class SpellingCheckerException : ApplicationException
    {
        /// <summary>
        /// Initialize with just an error message
        /// </summary>
        /// <param name="message"></param>
        public SpellingCheckerException( string message )
            : this( message, 0 )
        {
        }

        /// <summary>
        /// Initialize with a message and native error
        /// </summary>
        /// <param name="message">error message</param>
        /// <param name="nativeError">native error code</param>
        public SpellingCheckerException( string message, int nativeError )
            : base( message )
        {
            this.nativeError = nativeError ;
        }

        /// <summary>
        /// Underlying error code from native implementation
        /// </summary>
        public int NativeError { get { return nativeError; } }

        /// <summary>
        /// Underlying native error code
        /// </summary>
        private int nativeError = 0 ;
    }

}
