// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.PostEditor.Autoreplace;
using OpenLiveWriter.PostEditor.Emoticons;
using OpenLiveWriter.PostEditor.PostHtmlEditing;

namespace OpenLiveWriter.PostEditor
{
    public delegate void InsertHtml(MarkupPointer start, MarkupPointer end, string html);

    class TypographicCharacterHandler
    {
        public TypographicCharacterHandler(MarkupRange currentSelection, InsertHtml insertHtml, IBlogPostImageEditingContext imageEditingContext, IHTMLElement postBodyElement, char c, string htmlText, MarkupPointer blockBoundary)
        {
            _currentSelection = currentSelection.Clone();
            _currentSelection.Start.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;
            _currentSelection.End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Left;
            _insertHtml = insertHtml;
            _imageEditingContext = imageEditingContext;
            _postBodyElement = postBodyElement;

            this.c = c;
            this.htmlText = htmlText;
            this.blockBoundary = blockBoundary;
        }

        private char c;
        private string htmlText;
        private MarkupPointer blockBoundary;

        public static readonly List<string> SpecialCharacters = new List<string> { "...", "(c)", "(r)", "(tm)", };

        public bool HandleTypographicReplace()
        {
            // We're doing typographic replacement _after_ MSHTML has handled the key event.
            // There may by formatting tags that were added, e.g. <font>.
            // We only want the text, so we exclude any surrounding tags from the selection.
            _currentSelection.SelectInner();

            ReplaceDashes(blockBoundary);
            return ReplaceTypographic(c, htmlText);
        }

        public static int MaxLengthHint
        {
            get
            {
                return 3;
            }
        }

        private bool ReplaceTypographic(char c, string htmlText)
        {
            if ((c == '\"' || c == '\'') && AutoreplaceSettings.EnableSmartQuotes) // Handle smart quotes
            {
                bool isOpenQuote = true;
                if (htmlText.Length > 0)
                {
                    switch (htmlText[htmlText.Length - 1])
                    {
                        case '-':
                        case '{':
                        case '[':
                        case '(':
                            isOpenQuote = true;
                            break;
                        default:
                            if (!char.IsWhiteSpace(htmlText[htmlText.Length - 1]))
                                isOpenQuote = false;
                            break;
                    }
                }

                switch (c)
                {
                    case ('\''):
                        ReplaceValue(c, "'", (isOpenQuote ? "&#8216;" : "&#8217;"));
                        break;
                    case ('\"'):
                        ReplaceValue(c, "\"", (isOpenQuote ? "&#8220;" : "&#8221;"));
                        break;
                }

                return true;
            }
            else if ((c == ')' || c == '.') && AutoreplaceSettings.EnableSpecialCharacterReplacement) // Handling for (c), (r), ...
            {
                string replaceValue = null;
                string originalHtml = null;

                if (c == ')' && htmlText.EndsWith("(c", true, CultureInfo.InvariantCulture))
                {
                    replaceValue = "&#0169;";
                    originalHtml = "(c)";
                }
                else if (c == ')' && htmlText.EndsWith("(r", true, CultureInfo.InvariantCulture))
                {
                    replaceValue = "&#0174;";
                    originalHtml = "(r)";
                }
                else if (c == ')' && htmlText.EndsWith("(tm", true, CultureInfo.InvariantCulture))
                {
                    replaceValue = "&#x2122;";
                    originalHtml = "(tm)";
                }
                else if (c == '.' && htmlText.EndsWith("..", true, CultureInfo.InvariantCulture) && GlobalEditorOptions.SupportsFeature(ContentEditorFeature.UnicodeEllipsis))
                {
                    replaceValue = "&#8230;";
                    originalHtml = "...";
                }

                if (replaceValue != null)
                {
                    ReplaceValue(c, originalHtml, replaceValue);
                    return true;
                }

            }

            return ReplaceEmoticon(c, htmlText);
        }

        private bool ReplaceEmoticon(char c, string htmlText)
        {
            if (_imageEditingContext.EmoticonsManager.CanInsertEmoticonImage && AutoreplaceSettings.EnableEmoticonsReplacement && EmoticonsManager.IsAutoReplaceEndingCharacter(c))
            {
                string lastChar = c.ToString();
                foreach (Emoticon emoticon in _imageEditingContext.EmoticonsManager.PopularEmoticons)
                {
                    foreach (string autoReplaceText in emoticon.AutoReplaceText)
                    {
                        if (autoReplaceText.EndsWith(lastChar))
                        {
                            string remainingAutoReplaceText = autoReplaceText.Remove(autoReplaceText.Length - 1);
                            if (htmlText.EndsWith(remainingAutoReplaceText))
                            {
                                // Emoticons generally start with a colon, and therefore might interfere with a user attempting to type a URL.
                                string url = StringHelper.GetLastWord(htmlText);
                                if (UrlHelper.StartsWithKnownScheme(url) || !IsValidEmoticonInsertionPoint())
                                    return false;

                                ReplaceValue(c, autoReplaceText, _imageEditingContext.EmoticonsManager.GetHtml(emoticon));
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private bool IsValidEmoticonInsertionPoint()
        {
            MarkupRange selection = _currentSelection.Clone();

            // Check to make sure the target is not in an edit field
            if (InlineEditField.IsWithinEditField(selection.ParentElement()))
                return false;

            // Check to make sure the target is in the body of the post
            selection.MoveToElement(_postBodyElement, false);
            if (!selection.InRange(_currentSelection))
                return false;

            return true;
        }

        private void ReplaceValue(char currentCharacter, string undoValue, string replacementValue)
        {
            _currentSelection.Collapse(false);
            for (int i = 0; i < undoValue.Length; i++)
                _currentSelection.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVCHAR);

            _insertHtml(_currentSelection.Start, _currentSelection.End, replacementValue);
        }

        private void ReplaceDashes(MarkupPointer blockBoundary)
        {
            if (!AutoreplaceSettings.EnableHyphenReplacement)
                return;

            MarkupRange emRange = _currentSelection.Clone();
            for (int i = 0; i < 3 && emRange.Start.IsRightOf(blockBoundary); i++)
                emRange.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVWORDBEGIN);
            string emText = emRange.Text ?? "";

            if (emText.Contains("-"))
            {
                // \u00A0 = non breaking space
                Regex regex = new Regex(@"[^\s\u00A0\-]([ \u00A0]?(?>--?)[ \u00A0]?)[^\s\u00A0\-]");
                Match match = regex.Match(emText);
                if (match.Success)
                {
                    Debug.Assert(match.Groups.Count == 2, "Matched more than one set of dashes. Expecting only one match.");
                    string matchValue = match.Groups[1].Value.Replace((char)160, ' ');
                    MarkupRange findRange = _currentSelection.Clone();

                    // Since we're now doing this matching AFTER a character has been added, we need to jump back.
                    findRange.End.MoveUnitBounded(_MOVEUNIT_ACTION.MOVEUNIT_PREVCHAR, emRange.Start);
                    findRange.Collapse(false);
                    for (int i = 0; i < matchValue.Length; i++)
                    {
                        findRange.Start.MoveUnitBounded(_MOVEUNIT_ACTION.MOVEUNIT_PREVCHAR, emRange.Start);
                    }

                    for (int i = 0; i < emText.Length; i++)
                    {
                        if (findRange.Text == matchValue)
                        {
                            string replaceText = null;
                            switch (matchValue)
                            {
                                case ("--"):
                                case ("-- "):
                                    replaceText = "&#8212;";
                                    break;
                                case (" --"):
                                case (" -"):
                                    replaceText = "&#160;&#8211;";
                                    break;
                                case (" -- "):
                                case (" - "):
                                    replaceText = "&#160;&#8211; ";
                                    break;
                                case ("-"):
                                case ("- "):
                                    break;
                            }

                            if (replaceText != null)
                            {
                                _insertHtml(findRange.Start, findRange.End, replaceText);
                                break;
                            }
                        }
                        findRange.Start.MoveUnitBounded(_MOVEUNIT_ACTION.MOVEUNIT_PREVCHAR, emRange.Start);
                        findRange.End.MoveUnitBounded(_MOVEUNIT_ACTION.MOVEUNIT_PREVCHAR, emRange.Start);
                    }
                }
            }
        }

        private readonly MarkupRange _currentSelection;
        private readonly InsertHtml _insertHtml;
        private readonly IBlogPostImageEditingContext _imageEditingContext;
        private readonly IHTMLElement _postBodyElement;

        public void SetGravityRight()
        {
            _currentSelection.Start.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;
            _currentSelection.End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;
        }
    }
}
