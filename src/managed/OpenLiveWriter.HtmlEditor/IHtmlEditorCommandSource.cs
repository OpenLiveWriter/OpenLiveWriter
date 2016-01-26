// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using mshtml;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor
{
    public interface IHtmlEditorCommandSource : ISimpleTextEditorCommandSource
    {
        void ViewSource();

        void ClearFormatting();
        bool CanApplyFormatting(CommandId? commandId);

        string SelectionFontFamily { get; }
        void ApplyFontFamily(string fontFamily);

        float SelectionFontSize { get; }
        void ApplyFontSize(float fontSize);

        int SelectionForeColor { get; }
        void ApplyFontForeColor(int color);

        int SelectionBackColor { get; }
        void ApplyFontBackColor(int? color);

        string SelectionStyleName { get; }
        void ApplyHtmlFormattingStyle(IHtmlFormattingStyle style);

        bool SelectionBold { get; }
        void ApplyBold();

        bool SelectionItalic { get; }
        void ApplyItalic();

        bool SelectionUnderlined { get; }
        void ApplyUnderline();

        bool SelectionStrikethrough { get; }
        void ApplyStrikethrough();

        bool SelectionSuperscript { get; }
        void ApplySuperscript();

        bool SelectionSubscript { get; }
        void ApplySubscript();

        bool SelectionIsLTR { get; }
        void InsertLTRTextBlock();
        bool SelectionIsRTL { get; }
        void InsertRTLTextBlock();

        EditorTextAlignment GetSelectionAlignment();
        void ApplyAlignment(EditorTextAlignment alignment);

        bool SelectionBulleted { get; }
        void ApplyBullets();

        bool SelectionNumbered { get; }
        void ApplyNumbers();

        bool CanIndent { get; }
        void ApplyIndent();

        bool CanOutdent { get; }
        void ApplyOutdent();

        void ApplyBlockquote();
        bool SelectionBlockquoted { get; }

        bool CanInsertLink { get; }
        void InsertLink();

        bool CanRemoveLink { get; }
        void RemoveLink();

        void OpenLink();

        void AddToGlossary();

        bool CanPasteSpecial { get; }
        bool AllowPasteSpecial { get; }
        void PasteSpecial();

        bool CanFind { get; }
        void Find();

        bool CanPrint { get; }
        void Print();
        void PrintPreview();

        LinkInfo DiscoverCurrentLink();

        bool CheckSpelling();

        bool FullyEditableRegionActive { get; }

        CommandManager CommandManager { get; }
    }

    public class LinkInfo
    {
        private string _anchorText;
        private string _url;
        private string _linkTitle;
        private string _rel;
        private bool _newWindow;

        public LinkInfo(string anchorText, string url, string linkTitle, string rel, bool newWindow)
        {
            _anchorText = anchorText;
            _url = url;
            _linkTitle = linkTitle;
            _rel = rel;
            _newWindow = newWindow;
        }

        public string AnchorText
        {
            get
            {
                return _anchorText;
            }
        }
        public string Url
        {
            get
            {
                return _url;
            }
        }
        public string LinkTitle
        {
            get
            {
                return _linkTitle;
            }
        }
        public string Rel
        {
            get
            {
                return _rel;
            }
        }
        public bool NewWindow
        {
            get
            {
                return _newWindow;
            }
        }
    }

    public enum EditorTextAlignment
    {
        None,
        Left,
        Center,
        Right,
        Justify
    }

    public interface IHtmlFormattingStyle
    {
        /// <summary>
        /// Returns the name of the formatting style.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// The name of the Html element to apply
        /// </summary>
        string ElementName { get; }

        /// <summary>
        /// The mshtml tag id of the element to apply.
        /// </summary>
        _ELEMENT_TAG_ID ElementTagId { get; }
    }
}
