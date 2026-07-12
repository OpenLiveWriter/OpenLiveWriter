// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Threading.Tasks;
using global::Avalonia.Controls;
using global::Avalonia.Layout;
using global::Avalonia.Media;
using OpenLiveWriter.App.Avalonia.Editor;

namespace OpenLiveWriter.App.Avalonia.Dialogs
{
    /// <summary>
    /// A simple emoji picker: a grid of emoji buttons sourced from
    /// <see cref="EmoticonGallery"/>. Clicking one closes the dialog and returns the
    /// chosen emoji character for insertion at the caret.
    /// </summary>
    public class EmoticonDialog : Window
    {
        public string SelectedEmoji { get; private set; }

        public EmoticonDialog()
        {
            Title = "Insert Emoticon";
            Width = 320;
            SizeToContent = SizeToContent.Height;
            CanResize = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var wrap = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new global::Avalonia.Thickness(12),
                MaxWidth = 300
            };

            foreach (var item in EmoticonGallery.Items)
            {
                var emoji = item.Character;
                var button = new Button
                {
                    Content = new TextBlock { Text = emoji, FontSize = 20 },
                    Width = 40,
                    Height = 40,
                    Margin = new global::Avalonia.Thickness(2),
                    Background = Brushes.Transparent,
                    [ToolTip.TipProperty] = item.Name
                };
                button.Click += (s, e) =>
                {
                    SelectedEmoji = emoji;
                    Close(emoji);
                };
                wrap.Children.Add(button);
            }

            Content = wrap;
        }

        /// <summary>
        /// Shows the dialog modally over <paramref name="owner"/> and returns the
        /// chosen emoji, or null if cancelled.
        /// </summary>
        public static async Task<string> ShowAsync(Window owner)
        {
            var dialog = new EmoticonDialog();
            if (owner != null)
                return await dialog.ShowDialog<string>(owner);

            dialog.Show();
            return null;
        }
    }
}
