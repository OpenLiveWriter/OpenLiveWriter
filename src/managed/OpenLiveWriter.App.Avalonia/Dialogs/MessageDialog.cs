// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Threading.Tasks;
using global::Avalonia.Controls;
using global::Avalonia.Layout;
using global::Avalonia.Media;

namespace OpenLiveWriter.App.Avalonia.Dialogs
{
    /// <summary>
    /// Minimal OK-only modal used to surface informational and error messages (e.g. a
    /// publish failure or "no account configured"). Follows the same owner-based
    /// <c>ShowAsync</c> pattern as the other shell dialogs; a null owner (headless) is a
    /// no-op so callers stay testable.
    /// </summary>
    public class MessageDialog : Window
    {
        public MessageDialog(string title, string message)
        {
            Title = title ?? string.Empty;
            Width = 420;
            SizeToContent = SizeToContent.Height;
            CanResize = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var panel = new StackPanel { Margin = new global::Avalonia.Thickness(16), Spacing = 12 };

            // Long messages (e.g. a server error page snippet) must scroll instead of
            // growing the window past the screen's height.
            panel.Children.Add(new ScrollViewer
            {
                MaxHeight = 360,
                VerticalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                Content = new TextBlock
                {
                    Text = message ?? string.Empty,
                    TextWrapping = TextWrapping.Wrap
                }
            });

            var okButton = new Button
            {
                Content = "OK",
                IsDefault = true,
                IsCancel = true,
                MinWidth = 80,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            okButton.Click += (s, e) => Close();
            panel.Children.Add(okButton);

            Content = panel;
        }

        /// <summary>Shows the message modally over <paramref name="owner"/> (no-op if null).</summary>
        public static async Task ShowAsync(Window owner, string title, string message)
        {
            // Null OR non-visible owner (headless test benches that never Show() the
            // window) degrades to a no-op so callers stay testable.
            if (owner == null || !owner.IsVisible)
                return;
            var dialog = new MessageDialog(title, message);
            await dialog.ShowDialog(owner);
        }
    }
}
