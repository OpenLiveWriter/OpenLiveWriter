// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Threading.Tasks;
using global::Avalonia.Controls;
using global::Avalonia.Layout;

namespace OpenLiveWriter.App.Avalonia.Dialogs
{
    /// <summary>Result of an unsaved-changes prompt.</summary>
    public enum ConfirmResult
    {
        /// <summary>Save the pending changes, then proceed.</summary>
        Save,
        /// <summary>Discard the pending changes and proceed.</summary>
        Discard,
        /// <summary>Abort the operation.</summary>
        Cancel
    }

    /// <summary>Result of a Yes / No / Cancel confirmation.</summary>
    public enum YesNoCancelResult
    {
        Yes,
        No,
        Cancel
    }

    /// <summary>
    /// Small modal used for confirmations. Two flavors are offered via factory
    /// methods: a three-way Save/Discard/Cancel unsaved-changes prompt (used by
    /// New/Open) and a two-way OK/Cancel prompt (used by Delete Draft).
    /// </summary>
    public class ConfirmDialog : Window
    {
        public ConfirmResult Result { get; private set; } = ConfirmResult.Cancel;

        private ConfirmDialog(string title, string message,
            (string label, ConfirmResult result, bool isDefault, bool isCancel)[] buttons)
        {
            Title = title;
            Width = 420;
            SizeToContent = SizeToContent.Height;
            CanResize = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var panel = new StackPanel { Margin = new global::Avalonia.Thickness(16), Spacing = 16 };
            panel.Children.Add(new TextBlock
            {
                Text = message,
                TextWrapping = global::Avalonia.Media.TextWrapping.Wrap
            });

            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 8
            };

            foreach (var (label, result, isDefault, isCancel) in buttons)
            {
                var button = new Button
                {
                    Content = label,
                    MinWidth = 80,
                    IsDefault = isDefault,
                    IsCancel = isCancel
                };
                var captured = result;
                button.Click += (s, e) => { Result = captured; Close(captured); };
                buttonRow.Children.Add(button);
            }

            panel.Children.Add(buttonRow);
            Content = panel;
        }

        /// <summary>
        /// Shows a Save / Don't Save / Cancel prompt. Returns
        /// <see cref="ConfirmResult.Cancel"/> when there is no owner window (e.g.
        /// headless) so callers default to the safe, non-destructive path.
        /// </summary>
        public static async Task<ConfirmResult> ShowUnsavedChangesAsync(Window owner, string title = null)
        {
            var dialog = new ConfirmDialog(
                "Unsaved Changes",
                title == null
                    ? "You have unsaved changes. Do you want to save them?"
                    : $"You have unsaved changes to \u201c{title}\u201d. Do you want to save them?",
                new[]
                {
                    ("Save", ConfirmResult.Save, true, false),
                    ("Don\u2019t Save", ConfirmResult.Discard, false, false),
                    ("Cancel", ConfirmResult.Cancel, false, true)
                });

            if (owner == null) return ConfirmResult.Cancel;
            return await dialog.ShowDialog<ConfirmResult>(owner);
        }

        /// <summary>
        /// Shows an OK / Cancel confirmation. Returns <c>true</c> only when the user
        /// confirms; a null owner (headless) returns <c>false</c>.
        /// </summary>
        public static async Task<bool> ShowConfirmAsync(Window owner, string title, string message)
        {
            var dialog = new ConfirmDialog(
                title,
                message,
                new[]
                {
                    ("OK", ConfirmResult.Save, true, false),
                    ("Cancel", ConfirmResult.Cancel, false, true)
                });

            if (owner == null) return false;
            var result = await dialog.ShowDialog<ConfirmResult>(owner);
            return result == ConfirmResult.Save;
        }

        /// <summary>
        /// Shows a Yes / No / Cancel confirmation. A null owner (headless) returns
        /// <see cref="YesNoCancelResult.Cancel"/> so callers default to aborting.
        /// </summary>
        public static async Task<YesNoCancelResult> ShowYesNoCancelAsync(Window owner, string title, string message)
        {
            var dialog = new YesNoCancelDialog(title, message);
            if (owner == null)
                return YesNoCancelResult.Cancel;
            return await dialog.ShowDialog<YesNoCancelResult>(owner);
        }
    }

    internal sealed class YesNoCancelDialog : Window
    {
        public YesNoCancelDialog(string title, string message)
        {
            Title = title;
            Width = 420;
            SizeToContent = SizeToContent.Height;
            CanResize = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var panel = new StackPanel { Margin = new global::Avalonia.Thickness(16), Spacing = 16 };
            panel.Children.Add(new TextBlock
            {
                Text = message,
                TextWrapping = global::Avalonia.Media.TextWrapping.Wrap
            });

            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 8
            };

            foreach (var (label, result, isDefault, isCancel) in new[]
            {
                ("Yes", YesNoCancelResult.Yes, true, false),
                ("No", YesNoCancelResult.No, false, false),
                ("Cancel", YesNoCancelResult.Cancel, false, true)
            })
            {
                var button = new Button
                {
                    Content = label,
                    MinWidth = 80,
                    IsDefault = isDefault,
                    IsCancel = isCancel
                };
                var captured = result;
                button.Click += (s, e) => Close(captured);
                buttonRow.Children.Add(button);
            }

            panel.Children.Add(buttonRow);
            Content = panel;
        }
    }
}
