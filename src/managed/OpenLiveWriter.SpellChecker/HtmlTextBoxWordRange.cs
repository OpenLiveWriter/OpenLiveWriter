// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.SpellChecker
{
    /// <summary>
    /// Some test cases:
    ///
    /// [empty string]
    /// all correct words
    /// mzispalled wordz
    /// "Punctuaation."
    /// Apostraphe's
    /// 'Apostraphe's'
    /// numb3rs
    /// 1312
    /// &#x47;ood
    ///
    /// Limitations:
    /// Doesn't handle mid-word markup (e.g. t<i>e</i>st)
    /// Doesn't correct ALT attributes
    /// Doesn't know to ignore http:// and e-mail addresses
    /// </summary>
    public class HtmlTextBoxWordRange : IWordRange
    {
        private readonly TextBox _textBox;
        private readonly WordSource _src;
        private readonly int _startAt;
        private readonly int _endAt;

        private int _drift = 0;
        private TextWithOffsetAndLen _currentWord = null;
        private TextWithOffsetAndLen _nextWord;

        public HtmlTextBoxWordRange(TextBox textBox)
        {
            _textBox = textBox;
            _src = new WordSource(new HtmlTextSource(new SimpleHtmlParser(_textBox.Text)));

            if (_textBox.SelectionLength > 0)
            {
                _startAt = _textBox.SelectionStart;
                _endAt = _startAt + _textBox.SelectionLength;
            }
            else
            {
                _startAt = 0;
                _endAt = _textBox.TextLength;
            }

            AdvanceToStart();
        }

        private void AdvanceToStart()
        {
            while (null != (_nextWord = _src.Next())   // not at EOD
                && (_nextWord.Offset + _nextWord.Len <= _startAt))   // word is entirely before startAt
            {
            }

            CheckForNextWordPastEnd();
        }

        private void CheckForNextWordPastEnd()
        {
            if (_nextWord != null && _nextWord.Offset >= _endAt)
                _nextWord = null;
        }

        public bool HasNext()
        {
            return _nextWord != null;
        }

        public void Next()
        {
            _currentWord = _nextWord;
            _nextWord = _src.Next();
            CheckForNextWordPastEnd();
        }

        public string CurrentWord
        {
            get
            {
                return _currentWord.Text;
            }
        }

        public void PlaceCursor()
        {
            _textBox.Select(_currentWord.Offset - _drift + _currentWord.Len, 0);
        }

        public void Highlight(int offset, int length)
        {
            _textBox.Select(_currentWord.Offset - _drift + offset, length);
        }

        public void RemoveHighlight()
        {
            _textBox.Select(_textBox.SelectionStart, 0);
        }

        public void Replace(int offset, int length, string newText)
        {
            newText = HtmlUtils.EscapeEntities(newText);
            Highlight(offset, length);
            _textBox.SelectedText = newText;
            _drift += _currentWord.Len - newText.Length;
        }

        public bool IsCurrentWordUrlPart()
        {
            return false;
        }

        public bool FilterApplies()
        {
            return false;
        }
        public bool FilterAppliesRanged(int offset, int length)
        {
            return false;
        }

        private class TextWithOffsetAndLen
        {
            public readonly string Text;
            public readonly int Offset;
            public readonly int Len;

            public TextWithOffsetAndLen(string text, int offset, int len)
            {
                this.Text = text;
                this.Offset = offset;
                this.Len = len;
            }
        }

        private class WordSource
        {
            [Flags]
            private enum CharClass
            {
                Break = 1,
                BoundaryBreak = 2, // only counts as break if at start or end of word
                LetterOrNumber = 4,
                Letter = LetterOrNumber | 8,
                Number = LetterOrNumber | 0x10,
                IncludedBreakChar = 0x20 // counts as a break, but is also included in the word
            }

            private HtmlTextSource _src;

            private TextWithOffsetAndLen _curr;
            private int _offset = 0;

            public WordSource(HtmlTextSource src)
            {
                this._src = src;
                this._curr = src.Next();
            }

            public TextWithOffsetAndLen Next()
            {
                while (true)
                {
                    // No chunks left.
                    if (_curr == null)
                        return null;

                    // Advance until we get to the next potential start of word.
                    // Note that this may not turn out to be an actual word, e.g.
                    // if it is all numbers.
                    AdvanceUntilWordStart();

                    if (EOS())  // Reached end of this chunk
                    {
                        _offset = 0;
                        _curr = _src.Next();
                        continue;  // Try again with new chunk (or null, in which case we exit)
                    }

                    // Move to the end of the word.  Note that BoundaryWordBreak
                    // characters may not end the word.  For example, for the
                    // string "'that's'" (including single quotes), the word is
                    // "that's" (note outer single quotes dropped).
                    int start = _offset;
                    int endOfWord = _offset;
                    do
                    {
                        int charsToConsume;
                        CharClass charClass = ClassifyChar(_curr.Text, _offset, out charsToConsume);
                        if (Test(charClass, CharClass.Break))
                            break;
                        _offset += charsToConsume;
                        if (Test(charClass, CharClass.IncludedBreakChar))
                        {
                            endOfWord = _offset;
                            break;
                        }
                        if (Test(charClass, CharClass.LetterOrNumber))
                            endOfWord = _offset;
                    } while (!EOS());

                    string substring = _curr.Text.Substring(start, endOfWord - start);
                    if (substring.Length > 0)
                    {
                        return new TextWithOffsetAndLen(
                            HtmlUtils.UnEscapeEntities(substring, HtmlUtils.UnEscapeMode.NonMarkupText),
                            _curr.Offset + start,
                            substring.Length
                            );
                    }
                }
            }

            private void AdvanceUntilWordStart()
            {
                int charsToConsume;
                while (!EOS() && !Test(ClassifyChar(_curr.Text, _offset, out charsToConsume), CharClass.LetterOrNumber))
                    _offset += charsToConsume;
            }

            private bool Test(CharClass val, CharClass comparand)
            {
                return (val & comparand) == comparand;
            }

            /// <summary>
            /// Determines the type of character that is currently pointed to by _offset,
            /// </summary>
            /// <param name="charsToConsume"></param>
            /// <returns></returns>
            private CharClass ClassifyChar(string strval, int offset, out int charsToConsume)
            {
                charsToConsume = 1;
                char currChar = strval[offset];

                if (currChar == '&')
                {
                    int nextSemi = strval.IndexOf(';', offset + 1);
                    if (nextSemi != -1)
                    {
                        int code = HtmlUtils.DecodeEntityReference(strval.Substring(offset + 1, nextSemi - offset - 1));
                        if (code != -1)
                        {
                            charsToConsume = nextSemi - offset + 1;
                            currChar = (char)code;
                        }
                    }
                }

                return
                    !WordRangeHelper.IsNonSymbolChar(currChar) ? CharClass.Break :
                    char.IsLetter(currChar)		? CharClass.Letter :
                    char.IsNumber(currChar)		? CharClass.Number :
                    currChar == '\''			? CharClass.BoundaryBreak :
                    currChar == '’'				? CharClass.BoundaryBreak :
                    currChar == '.'				? CharClass.IncludedBreakChar :
                    CharClass.Break;
            }

            private bool EOS()
            {
                return _offset >= _curr.Text.Length;
            }
        }

        private class HtmlTextSource
        {
            private SimpleHtmlParser _parser;

            public HtmlTextSource(SimpleHtmlParser parser)
            {
                this._parser = parser;
            }

            public TextWithOffsetAndLen Next()
            {
                Element e;
                while (null != (e = _parser.Next()))
                {
                    if (e is Text)
                        return new TextWithOffsetAndLen(e.RawText, e.Offset, e.Length);
                }
                return null;
            }
        }
    }
}
