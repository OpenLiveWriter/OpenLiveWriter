// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using System.Threading.Tasks;
using global::Avalonia.Input;
using OpenLiveWriter.App.Avalonia.Dialogs;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.App.Avalonia
{
    /// <summary>
    /// Insert-tab behavior for the shell: routes the Insert commands that need a
    /// dialog (Insert Table, Insert Video) onto the editor. Simple bridge-only
    /// inserts (horizontal line, table-tools ops) are handled directly by
    /// <see cref="WebViewEditor.HandleCommandAsync"/>.
    /// </summary>
    public partial class MainWindow
    {
        private async Task<bool> TryHandleInsertCommandAsync(CommandId commandId)
        {
            switch (commandId)
            {
                case CommandId.InsertTable:
                case CommandId.InsertTable2:
                    await ShowInsertTableAsync();
                    return true;
                case CommandId.InsertVideoSplit:
                case CommandId.InsertVideoFromWeb:
                case CommandId.InsertVideoFromFile:
                case CommandId.InsertVideoFromService:
                    await ShowInsertVideoAsync();
                    return true;
                case CommandId.InsertEmoticon:
                    await ShowInsertEmoticonAsync();
                    return true;
                case CommandId.InsertMap:
                    await ShowInsertMapAsync();
                    return true;
                case CommandId.InsertTags:
                case CommandId.EditTags:
                    await ShowInsertTagsAsync();
                    return true;
                case CommandId.PasteSpecial:
                    await PasteSpecialAsync();
                    return true;
                default:
                    return false;
            }
        }

        private async Task ShowInsertTableAsync()
        {
            var editor = GetEditor();
            if (editor == null)
                return;

            TableDialogResult result = await TableDialog.ShowAsync(this);
            if (result == null)
                return;

            await editor.InsertTableAsync(result.Rows, result.Columns, result.HeaderRow, result.Width);
            UpdateStatus($"Inserted {result.Rows}\u00D7{result.Columns} table.");
        }

        private async Task ShowInsertVideoAsync()
        {
            var editor = GetEditor();
            if (editor == null)
                return;

            VideoDialogResult result = await VideoDialog.ShowAsync(this);
            if (result == null)
                return;

            string embed = VideoEmbedBuilder.BuildEmbedHtml(result.UrlOrEmbed);
            if (embed == null)
            {
                await MessageDialog.ShowAsync(this, "Insert Video",
                    "Could not recognize that as a video URL or embed code. Paste a YouTube/Vimeo link or an <iframe> embed.");
                return;
            }

            await editor.InsertHtmlAsync(embed);
            UpdateStatus("Inserted video embed.");
        }

        private async Task ShowInsertEmoticonAsync()
        {
            var editor = GetEditor();
            if (editor == null)
                return;

            string emoji = await EmoticonDialog.ShowAsync(this);
            if (string.IsNullOrEmpty(emoji))
                return;

            // Validate against the catalog, then insert the Unicode character.
            string payload = EmoticonGallery.BuildInsertion(emoji) ?? emoji;
            await editor.InsertHtmlAsync(payload);
            UpdateStatus("Inserted emoticon.");
        }

        private async Task ShowInsertMapAsync()
        {
            var editor = GetEditor();
            if (editor == null)
                return;

            MapDialogResult result = await MapDialog.ShowAsync(this);
            if (result == null)
                return;

            string html = MapEmbedBuilder.BuildMapHtml(result.Label, result.Coordinates, result.Zoom);
            if (html == null)
            {
                await MessageDialog.ShowAsync(this, "Insert Map",
                    "Enter a place name or coordinates (latitude, longitude) to insert a map.");
                return;
            }

            await editor.InsertHtmlAsync(html);
            UpdateStatus("Inserted map.");
        }

        // Insert Tags / keywords: manage the post's tag list, optionally inserting
        // rel="tag" links into the body and/or carrying them as post keywords
        // (mt_keywords) on the document so they flow through publish.
        private async Task ShowInsertTagsAsync()
        {
            var editor = GetEditor();
            if (editor == null)
                return;

            IEnumerable<string> existing = _draftSession?.Current.Keywords;
            TagDialogResult result = await TagDialog.ShowAsync(this, existing);
            if (result == null || result.Tags.Count == 0)
                return;

            if (result.SetAsKeywords && _draftSession != null)
            {
                _draftSession.Current.Keywords = result.Tags;
                _draftSession.Current.IsDirty = true;
            }

            if (result.InsertLinks)
            {
                string html = TagLinkBuilder.BuildTagLinksHtml(result.Tags);
                if (html != null)
                    await editor.InsertHtmlAsync(html);
            }

            UpdateStatus($"Tags: {string.Join(", ", result.Tags)}");
        }

        // Paste Special: insert the clipboard's text with formatting removed (clean
        // paste). The plain-text/clean-HTML sanitizers are covered by GroupF tests.
        private async Task PasteSpecialAsync()
        {
            var editor = GetEditor();
            if (editor == null)
                return;

            string clipboardText = null;
            try
            {
                if (Clipboard != null)
                {
                    IAsyncDataTransfer data = await Clipboard.TryGetDataAsync();
                    if (data != null)
                        clipboardText = await data.TryGetTextAsync();
                }
            }
            catch
            {
                // Clipboard access can fail on headless/unfocused windows — ignore.
            }

            if (string.IsNullOrEmpty(clipboardText))
            {
                UpdateStatus("Nothing to paste.");
                return;
            }

            await editor.PastePlainTextAsync(clipboardText);
            UpdateStatus("Pasted as plain text.");
        }
    }
}
