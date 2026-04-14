// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.SpellChecker
{

    /// <summary>
    /// Summary description for SpellCheckingContextMenuDefinition.
    /// 1. list of word suggestions
    /// 2. static menu with ignore all, add to dictionary (which take word as argument)
    /// 3. launch spelling dialog ??
    /// 4. static menu with cut/copy/paste
    ///
    /// </summary>
    public class SpellCheckingContextMenuDefinition : CommandContextMenuDefinition
    {
        public SpellCheckingContextMenuDefinition() : this(null, null)
        {
        }

        public SpellCheckingContextMenuDefinition(string word, SpellingManager manager)
        {
            _currentWord = word;
            _spellingManager = manager;
            Entries.AddRange(GetSpellingSuggestions());
            Entries.Add(CommandId.IgnoreOnce, true, false);
            Entries.Add(CommandId.IgnoreAll, false, false);
            Entries.Add(CommandId.AddToDictionary, false, false);
            Entries.Add(CommandId.OpenSpellingForm, false, false);
        }

        private string _currentWord;
        private SpellingManager _spellingManager;

        private MenuDefinitionEntryCollection GetSpellingSuggestions()
        {
            CommandManager commandManager = _spellingManager.CommandManager;
            MenuDefinitionEntryCollection listOfSuggestions = new MenuDefinitionEntryCollection();
            commandManager.SuppressEvents = true;
            commandManager.BeginUpdate();
            try
            {
                // provide suggestions
                SpellingSuggestion[] suggestions = _spellingManager.SpellingChecker.Suggest(_currentWord, DEFAULT_MAX_SUGGESTIONS, SUGGESTION_DEPTH ) ;
                bool foundSuggestion = false;
                if ( suggestions.Length > 0 )
                {
                    // add suggestions to list (stop adding when the quality of scores
                    // declines precipitously)
                    short lastScore = suggestions[0].Score ;
                    for (int i = 0; i < suggestions.Length; i++)
                    {
                        SpellingSuggestion suggestion = suggestions[i];

                        //note: in some weird cases, like 's, a suggestion is returned but lacks a suggested replacement, so need to check that case
                        if ( (lastScore-suggestion.Score) < SCORE_GAP_FILTER && (suggestion.Suggestion != null))
                        {
                            Command FixSpellingCommand = new Command(CommandId.FixWordSpelling);
                            FixSpellingCommand.Identifier += suggestion.Suggestion;
                            FixSpellingCommand.Text = suggestion.Suggestion;
                            FixSpellingCommand.MenuText = suggestion.Suggestion;
                            FixSpellingCommand.Execute += new EventHandler(_spellingManager.fixSpellingApplyCommand_Execute);
                            FixSpellingCommand.Tag = suggestion.Suggestion;
                            commandManager.Add(FixSpellingCommand);

                            listOfSuggestions.Add(FixSpellingCommand.Identifier, false, i == suggestions.Length - 1);
                            foundSuggestion = true;
                        }
                        else
                            break ;

                        // update last score
                        lastScore = suggestion.Score ;
                    }
                }
                if (!foundSuggestion)
                {
                    Command FixSpellingCommand = new Command(CommandId.FixWordSpelling);
                    FixSpellingCommand.Enabled = false;

                    commandManager.Add(FixSpellingCommand);
                    listOfSuggestions.Add(CommandId.FixWordSpelling, false, true);
                }
            }
            finally
            {
                commandManager.EndUpdate();
                commandManager.SuppressEvents = false;
            }
            return listOfSuggestions;
        }

        /// <summary>
        /// Default maximum suggestions to return
        /// </summary>
        private const short DEFAULT_MAX_SUGGESTIONS = 10 ;

        /// <summary>
        /// If we detect a gap between scores of this value or greater then
        /// we drop the score and all remaining
        /// </summary>
        private const short SCORE_GAP_FILTER = 20 ;

        /// <summary>
        /// Suggestion depth for searching (100 is the maximum)
        /// </summary>
        private const short SUGGESTION_DEPTH = 80 ;
    }
}
