// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.PostEditor.Tagging
{
    public class TextBoxWithAutoComplete : TextBox
    {

        public string[] AutoCompleteWords
        {
            set
            {
                _autoCompleteWords = value;
            }
        }
        private string[] _autoCompleteWords = new string[0];

        public string AutoCompleteSeparator
        {
            set
            {
                _autoCompleteSeparator = value;
            }
        }
        private string _autoCompleteSeparator = "";

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Delete || keyData == Keys.Back
                || keyData == Keys.Left || keyData == Keys.Right
                || keyData == Keys.Up || keyData == Keys.Down)
            {
                _suppressSuggestion = true;
                return false;
            }

            if (_hasSuggestion && (keyData == Keys.Enter || keyData == Keys.Tab || keyData == Keys.Oemcomma))
            {
                int cursorPos = SelectionStart + SelectionLength;
                Text = Text.Insert(cursorPos, _autoCompleteSeparator);
                cursorPos = cursorPos + _autoCompleteSeparator.Length;

                Select(cursorPos, 0);
                _hasSuggestion = false;

                if (keyData == Keys.Oemcomma)
                    return base.ProcessCmdKey(ref msg, keyData);
                return true;
            }

            _hasSuggestion = false;

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (_suppressSuggestion)
            {
                _suppressSuggestion = false;
                return;
            }

            if (SelectionLength == 0)
            {
                int cursor = SelectionStart;
                string tag = GetTagStart(cursor, Text);
                if (tag.Length > 0)
                    Suggest(cursor, tag);
            }
        }

        private void Suggest(int cursor, string tag)
        {
            string suggestedEnding = GetSuggestedEnding(tag);
            if (suggestedEnding != null)
            {
                Text = Text.Insert(cursor, suggestedEnding);
                Select(cursor, suggestedEnding.Length);
                _hasSuggestion = true;
            }
        }

        private bool _hasSuggestion = false;
        private bool _suppressSuggestion = false;

        private string GetSuggestedEnding(string tag)
        {
            string tagToLower = tag.ToLower(CultureInfo.CurrentCulture);
            string result = null;
            foreach (string t in _autoCompleteWords)
            {
                if (t.ToLower(CultureInfo.CurrentCulture).StartsWith(tagToLower) && t.ToLower(CultureInfo.CurrentCulture) != tagToLower)
                {
                    if (result != null)
                    {
                        return null;
                    }
                    result = t.Substring(tag.Length);
                }
            }
            return result;
        }

        private string GetTagStart(int cursor, string text)
        {
            char[] chars = text.ToCharArray(0, cursor);
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = chars.Length - 1; i > -1; i--)
            {
                if (IsSeparator(chars[i]))
                    break;

                stringBuilder.Insert(0, chars[i]);
            }
            return stringBuilder.ToString().TrimStart();
        }

        private bool IsSeparator(char c)
        {
            return (char.IsPunctuation(c));
        }

    }
}
