// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.SpellChecker
{
    /// <summary>
    /// IWordRange implementation for Windows Forms text boxes.
    /// </summary>
    public class TextBoxWordRange : IWordRange
    {
        private readonly TextBox textBox;

        // pointer for start of current word.
        private int startPos;

        // pointer for end of current word.
        private int endPos;

        private bool highlighted;
        private string text;

        // the index of the end of the textbox or selection.
        private int limit;

        public TextBoxWordRange(TextBox textBox, bool respectExistingSelection)
        {
            this.textBox = textBox;
            this.text = textBox.Text;

            this.startPos = -1;
            if (textBox.SelectionLength == 0 || !respectExistingSelection)
            {
                this.endPos = 0;
                this.limit = textBox.TextLength;
            }
            else
            {
                this.endPos = StartPosForSelection(textBox.SelectionStart);
                this.limit = textBox.SelectionStart + textBox.SelectionLength;
            }

            this.highlighted = false;
        }

        /// <summary>
        /// Is there another word in the range?
        /// </summary>
        /// <returns>true if there is another word in the range</returns>
        public bool HasNext()
        {
            return NextWordStart() != -1;
        }

        /// <summary>
        /// Advance to the next word in the range
        /// </summary>
        public void Next()
        {
            startPos = NextWordStart();
            endPos = FindEndOfWord(startPos);
        }

        /// <summary>
        /// Get the current word
        /// </summary>
        public string CurrentWord
        {
            get
            {
                return text.Substring(startPos, endPos - startPos);
            }
        }

        public void PlaceCursor()
        {
            textBox.SelectionStart = endPos;
            textBox.SelectionLength = 0;
        }

        /// <summary>
        /// Highlight the current word
        /// </summary>
        public void Highlight(int offset, int length)
        {
            textBox.SelectionStart = startPos + offset;
            textBox.SelectionLength = length;
            highlighted = true;
        }

        /// <summary>
        /// Remove highlighting from the range
        /// </summary>
        public void RemoveHighlight()
        {
            textBox.SelectionLength = 0;
        }

        /// <summary>
        /// Replace the current word
        /// </summary>
        /// <param name="newText">text to replace word with</param>
        public void Replace(int offset, int length, string newText )
        {
            text = text.Substring(0, startPos) + StringHelper.Replace(CurrentWord, offset, length, newText) + text.Substring(endPos);

            int delta = newText.Length - (endPos - startPos);
            limit += delta;
            endPos += delta;

            textBox.Text = text;

            textBox.SelectionStart = startPos;
            textBox.SelectionLength = highlighted ? endPos - startPos : 0;
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

        private int StartPosForSelection(int selectionStart)
        {
            if (selectionStart == 0)
                return 0;

            if (!(IsWordChar(text[selectionStart - 1]) && IsWordChar(text[selectionStart])))
                return selectionStart;

            int lastNonWord = 0;
            for (int i = 0; i < selectionStart; i++)
            {
                if (!IsWordChar(text[i]))
                    lastNonWord = i;
            }
            return lastNonWord + 1;
        }

        private int NextWordStart()
        {
            for (int i = endPos; i < limit; i++)
            {
                if (IsWordChar(text[i]))
                    return i;
            }
            return -1;
        }

        private int FindEndOfWord(int startIndex)
        {
            if (startIndex == -1 || startIndex >= limit)
                return -1;

            Trace.Assert(IsWordChar(text[startIndex]), "Asked to find end of word starting from invalid char " + text[startIndex]);

            for (int i = startIndex + 1; i < text.Length; i++)
            {
                if (!IsWordChar(text[i]))
                    return i;
            }
            return text.Length;
        }

        private bool IsWordChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == '\'';
        }
    }
}
