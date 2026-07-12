// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using System.Threading.Tasks;
using global::Avalonia.Controls;
using global::Avalonia.Layout;
using OpenLiveWriter.App.Avalonia.Editor;

namespace OpenLiveWriter.App.Avalonia.Dialogs
{
    /// <summary>
    /// Result returned from <see cref="TagDialog"/>.
    /// </summary>
    public class TagDialogResult
    {
        /// <summary>The parsed, de-duplicated tag list.</summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>Insert visible <c>rel="tag"</c> links into the post body.</summary>
        public bool InsertLinks { get; set; } = true;

        /// <summary>Set these tags as the post keywords (<c>mt_keywords</c>).</summary>
        public bool SetAsKeywords { get; set; } = true;
    }

    /// <summary>
    /// A modal dialog for managing post tags/keywords, replacing the Windows TagForm
    /// (which depended on remote tag-provider services). Tags can be inserted as
    /// <c>rel="tag"</c> links (via <see cref="TagLinkBuilder"/>) and/or carried as post
    /// keywords on the document.
    /// </summary>
    public class TagDialog : Window
    {
        private readonly TextBox _tagsBox;
        private readonly CheckBox _insertLinks;
        private readonly CheckBox _setKeywords;
        private readonly Button _insertButton;

        public TagDialogResult Result { get; private set; }

        public TagDialog(IEnumerable<string> existingTags = null)
        {
            Title = "Tags";
            Width = 460;
            SizeToContent = SizeToContent.Height;
            CanResize = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            _tagsBox = new TextBox
            {
                PlaceholderText = "Enter tags separated by commas",
                AcceptsReturn = true,
                MinHeight = 64,
                TextWrapping = global::Avalonia.Media.TextWrapping.Wrap,
                Text = existingTags != null ? string.Join(", ", existingTags) : string.Empty
            };

            _insertLinks = new CheckBox { Content = "Insert tag links into the post", IsChecked = true };
            _setKeywords = new CheckBox { Content = "Set as post keywords", IsChecked = true };

            _insertButton = new Button { Content = "OK", IsDefault = true, MinWidth = 80, IsEnabled = false };
            var cancelButton = new Button { Content = "Cancel", IsCancel = true, MinWidth = 80 };

            _insertButton.Click += (s, e) =>
            {
                Result = new TagDialogResult
                {
                    Tags = TagLinkBuilder.ParseTags(_tagsBox.Text),
                    InsertLinks = _insertLinks.IsChecked == true,
                    SetAsKeywords = _setKeywords.IsChecked == true
                };
                Close(Result);
            };
            cancelButton.Click += (s, e) => Close(null);

            _tagsBox.PropertyChanged += (s, e) =>
            {
                if (e.Property == TextBox.TextProperty)
                    _insertButton.IsEnabled = !string.IsNullOrWhiteSpace(_tagsBox.Text);
            };
            _insertButton.IsEnabled = !string.IsNullOrWhiteSpace(_tagsBox.Text);

            var stack = new StackPanel { Margin = new global::Avalonia.Thickness(16), Spacing = 8 };
            stack.Children.Add(new TextBlock { Text = "Tags:" });
            stack.Children.Add(_tagsBox);
            stack.Children.Add(_insertLinks);
            stack.Children.Add(_setKeywords);

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
        public static async Task<TagDialogResult> ShowAsync(Window owner, IEnumerable<string> existingTags = null)
        {
            var dialog = new TagDialog(existingTags);
            if (owner != null)
                return await dialog.ShowDialog<TagDialogResult>(owner);

            dialog.Show();
            return null;
        }
    }
}
