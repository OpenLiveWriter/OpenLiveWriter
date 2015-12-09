// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.PostEditor
{
    public class OpenLiveWriterContentTarget : IContentTarget
    {
        public string ProductName
        {
            get { return "Open Live Writer"; }
        }

        public bool SupportsFeature(ContentEditorFeature featureName)
        {
            switch (featureName)
            {
                case ContentEditorFeature.ImageClickThroughs:
                case ContentEditorFeature.OpenDialogOnInsertLinkDialog:
                case ContentEditorFeature.TidyWhitespace:
                case ContentEditorFeature.EnableSidebar:
                case ContentEditorFeature.SpecialPaste:
                case ContentEditorFeature.UrlContentSourcePaste:
                case ContentEditorFeature.ShowAllLinkOptions:
                case ContentEditorFeature.SupportsImageClickThroughs:
                case ContentEditorFeature.ImageBorderInherit:
                case ContentEditorFeature.Table:
                case ContentEditorFeature.SourceEditor:
                case ContentEditorFeature.PreviewMode:
                case ContentEditorFeature.ShadowImageForDrafts:
                case ContentEditorFeature.UnicodeEllipsis:
                case ContentEditorFeature.SpellCheckIgnoreOnce:
                case ContentEditorFeature.CleanHtmlOnPaste:
                case ContentEditorFeature.BrokenSmartContent:
                case ContentEditorFeature.ViewNormalEditorShortcut:
                case ContentEditorFeature.AutoLinking:
                case ContentEditorFeature.ResetFocusToFixIME:
                case ContentEditorFeature.AlwaysInsertInlineImagesAsInline:
                    {
                        return true;
                    }
                case ContentEditorFeature.TabAsIndent:
                case ContentEditorFeature.CenterImageWithParagraph:
                case ContentEditorFeature.DivNewLine:
                case ContentEditorFeature.PlainTextEditor:
                case ContentEditorFeature.HideNonVisibleElements:
                case ContentEditorFeature.ShowLinkTooltipForImages:
                    {
                        return false;
                    }
                default:
                    {
                        // It could be unknown because it is a per blog setting, and not global
                        // thus its setting is controlled through IEditorOptions
                        throw new ArgumentException("Unknown feature name: " + featureName);
                    }
            }
        }
    }
}
