// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices.ComTypes;
using OpenLiveWriter.Mshtml.Mshtml_Interop;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.PostEditor
{
    [Guid("bcf019de-cd63-4b33-8ffe-1f5e5403f3b3")]
    [ComVisible(true)]
    public enum ContentEditorFeature
    {
        /// <summary>
        /// Whether or not the blog provider supports uploading images through their API.
        /// </summary>
        ImageUpload = 1,

        /// <summary>
        /// Whether or not the blog provider allows scripts in the published HTML.
        /// </summary>
        Script = 2,

        /// <summary>
        /// Whether or not the blog provider allows embeds in the published HTML.
        /// </summary>
        Embeds = 3,

        /// <summary>
        /// Whether or not the blog provider requires valid XHTML.
        /// </summary>
        XHTML = 4,

        /// <summary>
        /// Unused.
        /// </summary>
        ImageClickThroughs = 5,

        /// <summary>
        /// By default, hitting tab starts a block quote. This flag forces the editor to insert an indent instead.
        /// </summary>
        TabAsIndent = 6,

        /// <summary>
        /// Unused.
        /// </summary>
        OpenDialogOnInsertLinkDialog = 7,

        /// <summary>
        /// Whether or not, in left-to-right languages, the option to change the reading direction of each paragraph
        /// should be enabled. This functionality is always enabled for right-to-left languages.
        /// </summary>
        RTLFeatures = 8,

        /// <summary>
        /// Whether or not "Ignore Once" is enabled.
        /// </summary>
        SpellCheckIgnoreOnce = 9,

        /// <summary>
        /// Cleans up the whitespace at the start and end of the document
        /// when it loads the html into the editor
        /// </summary>
        TidyWhitespace = 10,

        /// <summary>
        /// By default, images are centered by displaying the image as a block element and setting the left and right
        /// margins to auto. This flag forces the editor to center an image by wrapping it in an HTML
        /// &lt;p align="center"&gt; tag.
        /// </summary>
        CenterImageWithParagraph = 11,

        /// <summary>
        /// Whether or not the source code editing mode is enabled.
        /// </summary>
        SourceEditor = 12,

        /// <summary>
        /// Whether or not the WYSIWYG sidebar is enabled.
        /// </summary>
        EnableSidebar = 13,

        /// <summary>
        /// Whether or not the Paste Special command is allowed.
        /// </summary>
        SpecialPaste = 14,

        UrlContentSourcePaste = 16,
        Table = 17,
        ShowAllLinkOptions = 18,
        SupportsImageClickThroughs = 19,
        ImageBorderInherit = 20,
        DivNewLine = 21,
        HideNonVisibleElements = 22,
        ShadowImageForDrafts = 23,
        RTLDirectionDefault = 24,
        PlainTextEditor = 25,
        PreviewMode = 26,
        UnicodeEllipsis = 27,
        CleanHtmlOnPaste = 31,
        BrokenSmartContent = 32,
        ShowLinkTooltipForImages = 34,
        AlwaysInsertInlineImagesAsInline = 35,
        ViewNormalEditorShortcut = 39,
        AutoLinking = 41,

        /// <summary>
        /// Determines whether or not to reset the focus to body element after documentComplete to trigger IME notification in mshtml
        /// </summary>
        ResetFocusToFixIME
    }

    [Guid("9a98e1fd-8407-478b-af8a-ea0b1774dd07")]
    [ComVisible(true)]
    public enum ContentEditorOptions
    {
        TypographicReplacement = 1,
        SmartQuotes = 2,
        RealTimeWordCount = 3
    }

    [Guid("6a4c65cc-eb91-41af-9599-f5665e592df5")]
    [ComVisible(true)]
    public enum EditingMode
    {
        Wysiwyg = 1,
        Preview = 2,
        Source = 4,
        PlainText = 8
    }

    [Flags]
    [Guid("ac667254-1b9d-4db8-83da-b026c730eff2")]
    [ComVisible(true)]
    // Make sure this stays in sync with HtmlInsertionOptions
    public enum HtmlInsertOptions
    {
        SuppressSpellCheck = 1,
        Indent = 2,
        MoveCursorAfter = 4,
        InsertAtBeginning = 8,
        InsertAtEnd = 16,
        ClearUndoStack = 32,
        PlainText = 64,
        InsertNewLineBefore = 128,
        ExternalContent = 256,
        ApplyDefaultFont = 512,
        SelectFirstControl = 1024,
        AllowBlockBreakout = 2048
    }

    [Guid("b6a7c590-2229-4dba-bf82-2acd898cb3ca")]
    [ComVisible(true)]
    public enum SelectionPosition
    {
        BodyStart = 0,
        BodyEnd = 1
    }

    [Guid("183376ce-fef3-48d4-bcc7-453228cdd1e5")]
    [ComVisible(true)]
    public enum HR_E_PHOTOMAIL
    {
        MISSINGIMAGES = unchecked((int)0x8CCC0001),
        WPOSTXFILEMISSING = unchecked((int)0x8CCC0002),
        SIGNED_OUT = unchecked((int)0x8CCC0003),
        HTTPERROR_BASE = unchecked((int)0x8CCCC000),
        HTTPERROR_415 = unchecked((int)0x8CCCC19F),
        HTTPERROR_500 = unchecked((int)0x8CCCC1F4),
        HTTPERROR_502 = unchecked((int)0x8CCCC1F6),
        HTTPERROR_503 = unchecked((int)0x8CCCC1F7),
        HTTPERROR_504 = unchecked((int)0x8CCCC1F8),
        HTTPERROR_MAX = unchecked((int)0x8CCCCFFF),
        SKYDRIVE_BASE = unchecked((int)0x8CCCD000),
        SKYDRIVE_1 = unchecked((int)0x8CCCD001),
        SKYDRIVE_5 = unchecked((int)0x8CCCD005),
        SKYDRIVE_65 = unchecked((int)0x8CCCD041),
        SKYDRIVE_67 = unchecked((int)0x8CCCD043),
        SKYDRIVE_70 = unchecked((int)0x8CCCD046),
        SKYDRIVE_73 = unchecked((int)0x8CCCD049),
        SKYDRIVE_82 = unchecked((int)0x8CCCD052),
        SKYDRIVE_MAX = unchecked((int)0x8CCCDFFF),
        WEBERROR_BASE = unchecked((int)0x8CCCE000),
        WEBERROR_MAX = unchecked((int)0x8CCCEFFF),
    }

    [Guid("26E9B42B-F4F5-4037-95A7-E5E8E9AA5C26")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    public interface IContentEditorFactory
    {
        /// <summary>
        /// Creates and editor with a blank document
        /// </summary>
        /// <param name="contentEditorSite"></param>
        /// <param name="internetSecurityManager"></param>
        /// <param name="wysiwygHtml"></param>
        /// <returns></returns>
        IContentEditor CreateEditor(
            IContentEditorSite contentEditorSite,
            IInternetSecurityManager internetSecurityManager,
            string wysiwygHtml,
            int dlControlFlags);

        /// <summary>
        /// Creates an editor using the draft that was saved by the
        /// Save() function earlier from a IContentEditor
        /// </summary>
        /// <param name="contentEditorSite"></param>
        /// <param name="internetSecurityManager"></param>
        /// <param name="wysiwygHtml"></param>
        /// <param name="pathToDraftFile"></param>
        /// <returns></returns>
        //IContentEditor CreateEditorFromDraft(
        //    IContentEditorSite contentEditorSite,
        //    IInternetSecurityManager internetSecurityManager,
        //    string wysiwygHtml,
        //    string pathToDraftFile,
        //    int dlControlFlags);

        /// <summary>
        /// Creates a editor using a html document.  The body will
        /// become the editable region. Everything around the body
        ///  will become the template.
        /// </summary>
        /// <param name="contentEditorSite"></param>
        /// <param name="internetSecurityManager"></param>
        /// <param name="htmlDocument"></param>
        /// If true, a new line will be added to the begining of the body.
        /// If false, a new line will be added to the end of the body
        /// </param>
        /// <returns></returns>
        IContentEditor CreateEditorFromMoniker(
            IContentEditorSite contentEditorSite,
            IInternetSecurityManager internetSecurityManager,
            IMoniker htmlDocument,
            uint codepage,
            HtmlInsertOptions options,
            string color,
            int dlControlFlags,
            string wpost);

        /// <summary>
        /// Initialize the factory with settings that apply to
        /// all editors
        /// </summary>
        /// <param name="appDataPath">
        /// Sets the path of where the ContentEditor can
        /// save settings(log file, link glossary, etc...)
        /// e.g. C:\Users\user\AppData\Roaming\Open Live Writer
        /// </param>
        /// <param name="registrySettingsPath">
        /// Sets the path of where in the registry ContentEditor
        /// can save settings.
        /// e.g. HKEY_CURRENT_USER\Software\Open Live Writer\Writer
        /// </param>
        /// <param name="applicationName">
        /// Localized name of the application, will be shown to user in error messages
        /// </param>
        void Initialize(
            string registrySettingsPath,
            IContentEditorLogger logger,
            IContentTarget contentTarget,
            ISettingsProvider settingsProvider);

        /// <summary>
        /// Does clean up, and removes temp files, and
        /// disposes native resources
        /// </summary>
        void Shutdown();
        
        /// <summary>
        /// Preloads some of the costly work assoicated with creating a ContentEditor
        /// </summary>
        void DoPreloadWork();
    }

    /// <summary>
    /// IContentEditorLogger is a call back to do logging.
    /// This is not part of the IContentSite because
    /// there will need to be logging done before a ContentEditor
    /// is created and it is global to all ContentEditors
    /// thus it must be passed in when the IContentEditorFactory
    /// is created.
    /// </summary>
    [Guid("EB64D508-6B7E-404f-B832-A68E5C5D06F0")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    public interface IContentEditorLogger
    {
        void WriteLine(string message, int level);
    }

}
