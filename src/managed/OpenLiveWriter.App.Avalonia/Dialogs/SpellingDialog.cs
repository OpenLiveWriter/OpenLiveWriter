// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Threading.Tasks;
using global::Avalonia.Controls;
using global::Avalonia.Layout;
using global::Avalonia.Media;
using OpenLiveWriter.App.Avalonia.Spelling;

namespace OpenLiveWriter.App.Avalonia.Dialogs
{
    /// <summary>
    /// Modal Spelling dialog (parity with the Windows <c>SpellCheckerForm</c>): walks
    /// the misspellings of the post one at a time, showing the word in sentence
    /// context plus engine suggestions, with Change / Change All / Ignore /
    /// Ignore All / Add to Dictionary / Close. Replacements are applied to a working
    /// copy of the post HTML by the pure <see cref="SpellingSession"/>; the caller
    /// pushes <see cref="ResultHtml"/> back into the editor when
    /// <see cref="WasModified"/> is set.
    /// </summary>
    public sealed class SpellingDialog : Window
    {
        private readonly SpellingSession _session;

        private readonly TextBlock _wordText;
        private readonly TextBlock _contextText;
        private readonly TextBox _changeTo;
        private readonly ListBox _suggestions;
        private readonly Button _changeButton;
        private readonly Button _changeAllButton;

        public SpellingDialog(string html, ISpellCheckEngine engine)
        {
            _session = new SpellingSession(html, engine);

            Title = "Spelling: English (US)";
            Width = 480;
            Height = 360;
            MinWidth = 420;
            MinHeight = 300;
            CanResize = true;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            _wordText = new TextBlock
            {
                FontWeight = FontWeight.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(0xB0, 0x00, 0x00))
            };
            _contextText = new TextBlock { TextWrapping = TextWrapping.Wrap };

            _changeTo = new TextBox();
            _changeTo.PropertyChanged += (s, e) =>
            {
                if (e.Property == TextBox.TextProperty)
                    UpdateChangeButtons();
            };

            _suggestions = new ListBox { MinHeight = 96 };
            _suggestions.SelectionChanged += (s, e) =>
            {
                if (_suggestions.SelectedItem is string suggestion)
                    _changeTo.Text = suggestion;
            };

            _changeButton = new Button { Content = "Change", MinWidth = 120 };
            _changeAllButton = new Button { Content = "Change All", MinWidth = 120 };
            var ignoreButton = new Button { Content = "Ignore", MinWidth = 120 };
            var ignoreAllButton = new Button { Content = "Ignore All", MinWidth = 120 };
            var addButton = new Button { Content = "Add to Dictionary", MinWidth = 120 };
            var closeButton = new Button { Content = "Close", MinWidth = 120, IsCancel = true };

            _changeButton.Click += (s, e) => { _session.Change(_changeTo.Text); ShowCurrent(); };
            _changeAllButton.Click += (s, e) => { _session.ChangeAll(_changeTo.Text); ShowCurrent(); };
            ignoreButton.Click += (s, e) => { _session.Ignore(); ShowCurrent(); };
            ignoreAllButton.Click += (s, e) => { _session.IgnoreAll(); ShowCurrent(); };
            addButton.Click += (s, e) => { _session.AddToDictionary(); ShowCurrent(); };
            closeButton.Click += (s, e) => Close();

            var buttons = new StackPanel
            {
                Spacing = 6,
                VerticalAlignment = VerticalAlignment.Top,
                Children =
                {
                    _changeButton,
                    _changeAllButton,
                    ignoreButton,
                    ignoreAllButton,
                    addButton,
                    closeButton
                }
            };

            var fields = new StackPanel
            {
                Spacing = 6,
                Children =
                {
                    new TextBlock { Text = "Not in dictionary:" },
                    _wordText,
                    new TextBlock { Text = "In context:" },
                    _contextText,
                    new TextBlock { Text = "Change to:" },
                    _changeTo,
                    new TextBlock { Text = "Suggestions:" },
                    _suggestions
                }
            };

            var grid = new Grid
            {
                Margin = new global::Avalonia.Thickness(16),
                ColumnDefinitions = new ColumnDefinitions("*,Auto")
            };
            Grid.SetColumn(fields, 0);
            Grid.SetColumn(buttons, 1);
            buttons.Margin = new global::Avalonia.Thickness(16, 0, 0, 0);
            grid.Children.Add(fields);
            grid.Children.Add(buttons);
            Content = grid;

            ShowCurrent();
        }

        /// <summary>The post HTML with all accepted replacements applied.</summary>
        public string ResultHtml => _session.ResultHtml;

        /// <summary>True when at least one replacement was applied.</summary>
        public bool WasModified => _session.WasModified;

        // Test seam: lets headless tests drive the walk without a live WebView.
        internal SpellingSession Session => _session;

        /// <summary>
        /// Shows the dialog modally and returns it after close so the caller can read
        /// <see cref="ResultHtml"/> / <see cref="WasModified"/>.
        /// </summary>
        public static async Task<SpellingDialog> ShowAsync(Window owner, string html, ISpellCheckEngine engine)
        {
            var dialog = new SpellingDialog(html, engine);
            if (owner != null)
                await dialog.ShowDialog(owner);
            else
                dialog.Show();
            return dialog;
        }

        // Renders the current misspelling; closes the dialog when the walk is done.
        private void ShowCurrent()
        {
            SpellingSession.Entry current = _session.Current;
            if (current == null)
            {
                Close();
                return;
            }

            _wordText.Text = current.Word;
            _contextText.Text = current.Context;

            var suggestions = _session.GetSuggestions();
            _suggestions.Items.Clear();
            foreach (string suggestion in suggestions)
                _suggestions.Items.Add(suggestion);

            if (suggestions.Count > 0)
            {
                _suggestions.SelectedIndex = 0;
                _changeTo.Text = suggestions[0];
            }
            else
            {
                _changeTo.Text = string.Empty;
            }

            UpdateChangeButtons();
        }

        private void UpdateChangeButtons()
        {
            bool hasReplacement = !string.IsNullOrWhiteSpace(_changeTo.Text);
            _changeButton.IsEnabled = hasReplacement;
            _changeAllButton.IsEnabled = hasReplacement;
        }
    }
}
