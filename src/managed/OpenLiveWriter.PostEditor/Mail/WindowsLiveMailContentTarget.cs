// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.PostEditor;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Mail
{
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("BE0CBA6A-1127-4278-BACD-718DEF4532A1")]
    [ComVisible(true)]
    public class EmailContentTarget : IContentTarget
    {
        private bool rtlMode;

        public EmailContentTarget()
        {
        }

        private void CheckForRtlLang()
        {
            CultureInfo culture = Thread.CurrentThread.CurrentUICulture;
            rtlMode = culture.TextInfo.IsRightToLeft;

            if (!rtlMode)
            {
                foreach (InputLanguage lang in InputLanguage.InstalledInputLanguages)
                {
                    if (lang.Culture.TextInfo.IsRightToLeft)
                    {
                        EnableRtlMode();
                        return;
                    }
                }
            }
        }

        public void EnableRtlMode()
        {
            rtlMode = true;
        }

        public string ProductName
        {
            get { return "E-mail"; }
        }

        public bool SupportsFeature(ContentEditorFeature featureName)
        {
            switch (featureName)
            {
                case ContentEditorFeature.RTLDirectionDefault:
                    return BidiHelper.IsRightToLeft || CultureHelper.IsRtlLcid(User32.GetKeyboardLayout(0) & 0xFFFF);

                case ContentEditorFeature.RTLFeatures:
                    CheckForRtlLang();
                    return rtlMode;

                case ContentEditorFeature.ImageUpload:
                case ContentEditorFeature.Script:
                case ContentEditorFeature.Embeds:
                case ContentEditorFeature.XHTML:
                case ContentEditorFeature.ImageClickThroughs:
                case ContentEditorFeature.OpenDialogOnInsertLinkDialog:
                case ContentEditorFeature.TidyWhitespace:
                case ContentEditorFeature.SpecialPaste:
                case ContentEditorFeature.UrlContentSourcePaste:
                case ContentEditorFeature.ShowAllLinkOptions:
                case ContentEditorFeature.SupportsImageClickThroughs:
                case ContentEditorFeature.ImageBorderInherit:
                case ContentEditorFeature.Table:
                case ContentEditorFeature.PreviewMode:
                case ContentEditorFeature.SourceEditor:
                case ContentEditorFeature.ShadowImageForDrafts:
                case ContentEditorFeature.UnicodeEllipsis:
                case ContentEditorFeature.CleanHtmlOnPaste:
                case ContentEditorFeature.BrokenSmartContent:
                case ContentEditorFeature.ViewNormalEditorShortcut:
                case ContentEditorFeature.AutoLinking:
                case ContentEditorFeature.ResetFocusToFixIME:
                    {
                        return false;
                    }
                case ContentEditorFeature.TabAsIndent:
                case ContentEditorFeature.SpellCheckIgnoreOnce:
                case ContentEditorFeature.CenterImageWithParagraph:
                case ContentEditorFeature.DivNewLine:
                case ContentEditorFeature.PlainTextEditor:
                case ContentEditorFeature.HideNonVisibleElements:
                case ContentEditorFeature.EnableSidebar:
                case ContentEditorFeature.ShowLinkTooltipForImages:
                case ContentEditorFeature.AlwaysInsertInlineImagesAsInline:
                    {
                        return true;
                    }
                default:
                    {
                        throw new ArgumentException("Unknown feature name: " + featureName);
                    }
            }
        }
    }

}
