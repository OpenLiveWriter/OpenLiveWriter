// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using System.Text.RegularExpressions;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.Ribbon.Avalonia.Controls
{
    /// <summary>
    /// Provides human-readable labels for CommandIds.
    /// Since we cannot reference the Localization project's resource system,
    /// we derive labels from the CommandId enum names using PascalCase splitting,
    /// with manual overrides for common commands.
    /// </summary>
    internal static class CommandLabelHelper
    {
        private static readonly Dictionary<CommandId, string> _overrides = new Dictionary<CommandId, string>
        {
            { CommandId.PostAndPublish, "Publish" },
            { CommandId.PostAsDraft, "Save Draft" },
            { CommandId.CopyCommand, "Copy" },
            { CommandId.InsertLink, "Hyperlink" },
            { CommandId.InsertImageSplit, "Picture" },
            { CommandId.InsertVideoSplit, "Video" },
            { CommandId.InsertHorizontalLine, "Horizontal Line" },
            { CommandId.InsertClearBreak, "Clear Break" },
            { CommandId.InsertExtendedEntry, "Split Post" },
            { CommandId.InsertTable, "Table" },
            { CommandId.InsertMap, "Map" },
            { CommandId.InsertTags, "Tags" },
            { CommandId.InsertEmoticon, "Emoticon" },
            { CommandId.InsertPictureFromFile, "From File..." },
            { CommandId.WebImage, "From Web..." },
            { CommandId.InsertVideoFromWeb, "From Web..." },
            { CommandId.InsertVideoFromFile, "From File..." },
            { CommandId.InsertVideoFromService, "From Service..." },
            { CommandId.CheckSpelling, "Spelling" },
            { CommandId.FindButton, "Find" },
            { CommandId.WordCount, "Count" },
            { CommandId.SelectAll, "Select All" },
            { CommandId.FontFamily, "Font" },
            { CommandId.FontSize, "Size" },
            { CommandId.ClearFormatting, "Clear" },
            { CommandId.FontBackgroundColor, "Highlight" },
            { CommandId.FontColorPicker, "Color" },
            { CommandId.SemanticHtmlGallery, "Style" },
            { CommandId.SelectBlog, "Blog" },
            { CommandId.AddWeblog, "Add Blog..." },
            { CommandId.Accounts, "Manage Accounts" },
            { CommandId.ConfigureWeblog, "Blog Settings" },
            { CommandId.BlogProviderButtonsGallery, "Shortcuts" },
            { CommandId.ViewUseStyles, "Use Theme" },
            { CommandId.UpdateWeblogStyle, "Update Theme" },
            { CommandId.ClosePreview, "Close Preview" },
            { CommandId.AddPlugin, "Get Plug-ins" },
            { CommandId.ManagePlugins, "Manage" },
            { CommandId.PluginsGallery, "Plug-ins" },
            { CommandId.FormatImageLockAspectRatio, "Lock Ratio" },
            { CommandId.ImageCrop, "Crop" },
            { CommandId.ImageRotateCW, "Rotate Right" },
            { CommandId.ImageRotateCCW, "Rotate Left" },
            { CommandId.ImageTilt, "Tilt" },
            { CommandId.InsertLoremIpsum, "Lorem Ipsum" },
            { CommandId.TerminateProcess, "Terminate" },
            { CommandId.RaiseAssertion, "Assert" },
            { CommandId.DiagnosticsConsole, "Diagnostics" },
            { CommandId.BlogClientOptions, "Blog Options" },
            { CommandId.ViewSource, "View Source" },
            { CommandId.ValidateHtml, "HTML" },
            { CommandId.ValidateXhtml, "XHTML" },
            { CommandId.ValidateLocalizedResources, "Resources" },
            { CommandId.AlignLeft, "Left" },
            { CommandId.AlignCenter, "Center" },
            { CommandId.AlignRight, "Right" },
            { CommandId.Justify, "Justify" },
            { CommandId.Bullets, "Bullets" },
            { CommandId.Numbers, "Numbers" },
            { CommandId.Blockquote, "Quote" },
            { CommandId.PasteSpecial, "Paste Special" },
            { CommandId.NewPost, "New Post" },
            { CommandId.OpenPost, "Open Post" },
            { CommandId.SavePost, "Save" },
            { CommandId.DeleteDraft, "Delete Draft" },
            { CommandId.PrintPreview, "Print Preview" },
            { CommandId.Print, "Print" },
            { CommandId.Options, "Options" },
            { CommandId.About, "About" },
            { CommandId.Close, "Close" },
            { CommandId.VideoWebPreview, "Web Preview" },
            { CommandId.VideoWidescreenAspectRatio, "Widescreen" },
            { CommandId.VideoStandardAspectRatio, "Standard" },
            { CommandId.ShowBetaExpiredDialogs, "Beta Expired" },
            { CommandId.ShowUpdateMessage, "Update Msg" },
            { CommandId.ShowWebLayoutWarning, "Layout Warn" },
            { CommandId.ShowErrorDialog, "Error Dialog" },
            { CommandId.ShowDisplayMessageTestForm, "Display Msg" },
            { CommandId.ShowSupportingFilesForm, "Support Files" },
            { CommandId.ShowAtomImageEndpointSelector, "Atom Image" },
            { CommandId.ShowGoogleCaptcha, "Captcha" },
        };

        /// <summary>
        /// Gets a display label for the given CommandId.
        /// </summary>
        public static string GetLabel(CommandId commandId)
        {
            if (_overrides.TryGetValue(commandId, out var label))
                return label;

            // Split PascalCase into words
            var name = commandId.ToString();
            return Regex.Replace(name, @"(?<!^)([A-Z][a-z])", " $1").Trim();
        }
    }
}
