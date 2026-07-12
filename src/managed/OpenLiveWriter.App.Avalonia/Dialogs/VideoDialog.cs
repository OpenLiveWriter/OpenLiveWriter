// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Threading.Tasks;
using global::Avalonia.Controls;
using global::Avalonia.Layout;

namespace OpenLiveWriter.App.Avalonia.Dialogs
{
    /// <summary>
    /// Result returned from <see cref="VideoDialog"/> when the user inserts a video.
    /// </summary>
    public class VideoDialogResult
    {
        /// <summary>A video URL (YouTube/Vimeo/generic) or a pasted <c>&lt;iframe&gt;</c> embed.</summary>
        public string UrlOrEmbed { get; set; }
    }

    /// <summary>
    /// A modal dialog for inserting a web video. The Windows "video from service /
    /// from file" paths relied on defunct Flash/upload APIs; this dialog implements
    /// the modern web-embed path: paste a YouTube/Vimeo link or an embed snippet and
    /// a responsive iframe is inserted.
    /// </summary>
    public class VideoDialog : Window
    {
        private readonly TextBox _urlBox;
        private readonly Button _insertButton;

        public VideoDialogResult Result { get; private set; }

        public VideoDialog()
        {
            Title = "Insert Video";
            Width = 480;
            SizeToContent = SizeToContent.Height;
            CanResize = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            _urlBox = new TextBox
            {
                PlaceholderText = "Paste a YouTube/Vimeo link or an <iframe> embed",
                AcceptsReturn = true,
                MinHeight = 64,
                TextWrapping = global::Avalonia.Media.TextWrapping.Wrap
            };

            _insertButton = new Button { Content = "Insert", IsDefault = true, MinWidth = 80, IsEnabled = false };
            var cancelButton = new Button { Content = "Cancel", IsCancel = true, MinWidth = 80 };

            _insertButton.Click += (s, e) =>
            {
                Result = new VideoDialogResult { UrlOrEmbed = _urlBox.Text?.Trim() };
                Close(Result);
            };
            cancelButton.Click += (s, e) => Close(null);

            _urlBox.PropertyChanged += (s, e) =>
            {
                if (e.Property == TextBox.TextProperty)
                    _insertButton.IsEnabled = !string.IsNullOrWhiteSpace(_urlBox.Text);
            };

            var stack = new StackPanel { Margin = new global::Avalonia.Thickness(16), Spacing = 8 };
            stack.Children.Add(new TextBlock { Text = "Video URL or embed code:" });
            stack.Children.Add(_urlBox);
            stack.Children.Add(new TextBlock
            {
                Text = "Supports YouTube, Vimeo, or any embeddable video URL.",
                FontSize = 11,
                Foreground = global::Avalonia.Media.Brushes.Gray,
                TextWrapping = global::Avalonia.Media.TextWrapping.Wrap
            });

            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 8,
                Margin = new global::Avalonia.Thickness(0, 8, 0, 0)
            };
            buttonRow.Children.Add(cancelButton);
            buttonRow.Children.Add(_insertButton);
            stack.Children.Add(buttonRow);

            Content = stack;
        }

        /// <summary>
        /// Shows the dialog modally over <paramref name="owner"/> and returns the
        /// user's input, or null if cancelled.
        /// </summary>
        public static async Task<VideoDialogResult> ShowAsync(Window owner)
        {
            var dialog = new VideoDialog();
            if (owner != null)
                return await dialog.ShowDialog<VideoDialogResult>(owner);

            dialog.Show();
            return null;
        }
    }
}
