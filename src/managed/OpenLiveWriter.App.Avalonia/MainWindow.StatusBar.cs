// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Threading.Tasks;
using global::Avalonia.Controls;
using OpenLiveWriter.App.Avalonia.Editor;

namespace OpenLiveWriter.App.Avalonia
{
    /// <summary>Status bar polish: current blog label and optional live word count.</summary>
    public partial class MainWindow
    {
        private bool _showRealTimeWordCount;

        private void UpdateBlogStatusLabel()
        {
            var blogText = this.FindControl<TextBlock>("BlogStatusText");
            if (blogText == null)
                return;

            if (_accountService?.CurrentAccount != null)
                blogText.Text = _accountService.CurrentAccount.DisplayLabel;
            else
                blogText.Text = "No blog selected";
        }

        private void UpdateStatusBarExtras()
        {
            UpdateBlogStatusLabel();
            _ = RefreshWordCountStatusAsync();
        }

        private async Task RefreshWordCountStatusAsync()
        {
            var wordText = this.FindControl<TextBlock>("WordCountStatusText");
            if (wordText == null)
                return;

            if (!_showRealTimeWordCount)
            {
                wordText.IsVisible = false;
                return;
            }

            var editor = GetEditor();
            string html = editor != null ? await editor.GetContentAsync() : null;
            var counter = new WordCounter(html ?? string.Empty);
            wordText.Text = $"{counter.Words} words";
            wordText.IsVisible = true;
        }

        // Called when editor content changes — keep the word-count pane live when enabled.
        private void OnEditorContentChangedForWordCount()
        {
            if (!_showRealTimeWordCount)
                return;
            _ = RefreshWordCountStatusAsync();
        }
    }
}
