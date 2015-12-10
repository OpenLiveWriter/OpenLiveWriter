// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using System.Text.RegularExpressions;
using OpenLiveWriter.CoreServices.Settings;
using OpenLiveWriter.Localization;
using System.Globalization;
using OpenLiveWriter.Controls;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.CoreServices.Layout;
using System.Threading;

namespace OpenLiveWriter.HtmlEditor
{
    public partial class WordCountForm : ApplicationDialog
    {

        public WordCountForm(string words, bool bOnlySelectedText)
        {
            InitializeComponent();

            gbTableHeader.Font = Res.GetFont(FontSize.Normal, FontStyle.Regular);

            // Set the text that will be analyzed, this will preform the calculations
            WordCounter wc = new WordCounter(words);

            buttonClose.Text = Res.Get(StringId.CloseButton);
            this.Text = Res.Get(StringId.WordCountTitle);
            labelWordCount.Text = Res.Get(StringId.WordCount);
            labelChars.Text = Res.Get(StringId.CharCount);
            labelCharsNoSpaces.Text = Res.Get(StringId.CharNoSpacesCount);
            labelParagraphs.Text = Res.Get(StringId.Paragraphs);

            labelWordCountValue.Text = String.Format(CultureInfo.CurrentCulture, "{0}", wc.Words);
            labelCharsValue.Text = String.Format(CultureInfo.CurrentCulture, "{0}", wc.Chars);
            labelCharsNoSpacesValue.Text = String.Format(CultureInfo.CurrentCulture, "{0}", wc.CharsWithoutSpaces);
            labelParagraphsValue.Text = String.Format(CultureInfo.CurrentCulture, "{0}", wc.Paragraphs);

            // If the text is not the whole document then show the user a message to tell them
            if (bOnlySelectedText)
            {
                gbTableHeader.Text = Res.Get(StringId.StatisticsSelection);
            }
            else
            {
                gbTableHeader.Text = Res.Get(StringId.Statistics);
            }

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            using (new AutoGrow(this, AnchorStyles.Right, false))
            {
                using (new AutoGrow(gbTableHeader, AnchorStyles.Right, false))
                {
                    labelWordCount.AutoSize = true;
                    labelCharsNoSpaces.AutoSize = true;
                    labelChars.AutoSize = true;
                    labelParagraphs.AutoSize = true;

                    LayoutHelper.AutoFitLabels(labelWordCountValue, labelCharsNoSpacesValue, labelCharsValue, labelParagraphsValue);
                }
            }

            DisplayHelper.AutoFitSystemButton(buttonClose, buttonClose.Width, int.MaxValue);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }

    public class WordCounter
    {
        #region Regex To Find Words
        //private readonly static Regex regexWords = new Regex(@"[\p{Ll}\p{Lu}\p{Lt}\p{Nd}\p{Pc}\p{Pf}\p{Pi}\p{Po}\p{Sc}\u2019]+|\p{Lo}", RegexOptions.Compiled);
        private readonly static Regex regexWords = new Regex(@"[^\n\r\t\s()\p{Lo}]+|\p{Lo}", RegexOptions.Compiled);
        private readonly static Regex regexChars = new Regex("[^\n\r\t]", RegexOptions.Compiled);
        private readonly static Regex regexCharsWithoutSpace = new Regex("\\S", RegexOptions.Compiled);
        private readonly static Regex regexParagraph = new Regex(@"(\r\n){1,2}\s*", RegexOptions.Compiled);
        #endregion

        public readonly String countText;
        private LazyLoader<int> llWords = null;
        private LazyLoader<int> llChars = null;
        private LazyLoader<int> llCharsWithoutSpaces = null;
        private LazyLoader<int> llParagraphs = null;

        public WordCounter(string text)
        {
            countText = HtmlUtils.HTMLToPlainText(text);

            llWords = new LazyLoader<int>(delegate { return regexWords.Matches(countText).Count; });
            llChars = new LazyLoader<int>(delegate { return regexChars.Matches(countText).Count; });
            llCharsWithoutSpaces = new LazyLoader<int>(delegate { return regexCharsWithoutSpace.Matches(countText).Count; });
            llParagraphs = new LazyLoader<int>(delegate { return countText.Length == 0 ? 0 : regexParagraph.Matches(countText).Count + 1; });
        }

        #region Properties

        public int Words { get { return llWords.Value; } }
        public int Chars { get { return llChars.Value; } }
        public int CharsWithoutSpaces { get { return llCharsWithoutSpaces.Value; } }
        public int Paragraphs { get { return llParagraphs.Value; } }

        #endregion
    }
}
