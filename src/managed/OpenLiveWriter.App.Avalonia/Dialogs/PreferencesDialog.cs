// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Threading.Tasks;
using global::Avalonia.Controls;
using global::Avalonia.Layout;
using global::Avalonia.Media;
using OpenLiveWriter.App.Avalonia.Settings;
using OpenLiveWriter.Publishing.Accounts;

namespace OpenLiveWriter.App.Avalonia.Dialogs
{
    /// <summary>
    /// Tabbed Preferences dialog: General, Editing, Spelling, Web Proxy, and Accounts.
    /// Maps fields from the Windows options panels (see <c>testplan/testOptionsDialogBox</c>).
    /// Only options the macOS shell actually enforces are shown — the Windows post-window
    /// behavior, tag reminder, and paragraph-tag toggles are deliberately omitted (their
    /// <see cref="AppPreferences"/> fields are kept for forward-compat).
    /// </summary>
    public sealed class PreferencesDialog : Window
    {
        private readonly AppPreferences _working;
        private readonly BlogAccountService _accountService;
        private readonly Func<AppPreferences, Task> _applyAsync;

        // General
        private CheckBox _viewAfterPublish;
        private CheckBox _closeOnPublish;
        private CheckBox _titleReminder;
        private CheckBox _categoryReminder;
        private CheckBox _autoSave;
        private NumericUpDown _autoSaveMinutes;
        private CheckBox _wordCount;
        private CheckBox _formatHtml;

        // Editing
        private CheckBox _replaceHyphens;
        private CheckBox _replaceQuotes;
        private CheckBox _replaceSpecial;
        private CheckBox _replaceEmoticons;

        // Spelling
        private CheckBox _spellcheck;

        // Proxy
        private CheckBox _proxyEnabled;
        private TextBox _proxyHost;
        private TextBox _proxyPort;
        private TextBox _proxyUser;
        private TextBox _proxyPassword;

        private PreferencesDialog(
            AppPreferences current,
            BlogAccountService accountService,
            Func<AppPreferences, Task> applyAsync)
        {
            _working = current?.Clone() ?? AppPreferences.CreateDefault();
            _accountService = accountService;
            _applyAsync = applyAsync ?? throw new ArgumentNullException(nameof(applyAsync));

            Title = "Preferences";
            Width = 520;
            Height = 480;
            MinWidth = 480;
            MinHeight = 400;
            CanResize = true;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var tabs = new TabControl { Margin = new global::Avalonia.Thickness(16, 16, 16, 0) };
            tabs.Items.Add(new TabItem { Header = "General", Content = BuildGeneralTab() });
            tabs.Items.Add(new TabItem { Header = "Editing", Content = BuildEditingTab() });
            tabs.Items.Add(new TabItem { Header = "Spelling", Content = BuildSpellingTab() });
            tabs.Items.Add(new TabItem { Header = "Web Proxy", Content = BuildProxyTab() });
            tabs.Items.Add(new TabItem { Header = "Accounts", Content = BuildAccountsTab() });

            var okButton = new Button { Content = "OK", IsDefault = true, MinWidth = 90 };
            var cancelButton = new Button { Content = "Cancel", IsCancel = true, MinWidth = 90 };
            okButton.Click += async (s, e) => await OkAsync();
            cancelButton.Click += (s, e) => Close(false);

            var footer = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 8,
                Margin = new global::Avalonia.Thickness(16)
            };
            footer.Children.Add(okButton);
            footer.Children.Add(cancelButton);

            var root = new DockPanel();
            DockPanel.SetDock(footer, Dock.Bottom);
            root.Children.Add(footer);
            root.Children.Add(tabs);
            Content = root;

            BindFromWorking();
        }

        /// <summary>Shows the dialog modally. Returns true when the user clicked OK.</summary>
        public static async Task<bool> ShowAsync(
            Window owner,
            AppPreferences current,
            BlogAccountService accountService,
            Func<AppPreferences, Task> applyAsync)
        {
            var dialog = new PreferencesDialog(current, accountService, applyAsync);
            if (owner != null)
                return await dialog.ShowDialog<bool>(owner);
            dialog.Show();
            return false;
        }

        private async Task OkAsync()
        {
            BindToWorking();
            await _applyAsync(_working);
            Close(true);
        }

        private void BindFromWorking()
        {
            _viewAfterPublish.IsChecked = _working.ViewPostAfterPublish;
            _closeOnPublish.IsChecked = _working.CloseWindowOnPublish;
            _titleReminder.IsChecked = _working.TitleReminder;
            _categoryReminder.IsChecked = _working.CategoryReminder;
            _autoSave.IsChecked = _working.AutoSaveDrafts;
            _autoSaveMinutes.Value = _working.AutoSaveMinutes;
            _wordCount.IsChecked = _working.ShowRealTimeWordCount;
            _formatHtml.IsChecked = _working.FormatHtml;

            _replaceHyphens.IsChecked = _working.ReplaceHyphens;
            _replaceQuotes.IsChecked = _working.ReplaceSmartQuotes;
            _replaceSpecial.IsChecked = _working.ReplaceSpecialCharacters;
            _replaceEmoticons.IsChecked = _working.ReplaceEmoticons;

            _spellcheck.IsChecked = _working.SpellcheckEnabled;

            _proxyEnabled.IsChecked = _working.ProxyEnabled;
            _proxyHost.Text = _working.ProxyHostname ?? string.Empty;
            _proxyPort.Text = _working.ProxyPort.ToString();
            _proxyUser.Text = _working.ProxyUsername ?? string.Empty;
            _proxyPassword.Text = _working.ProxyPassword ?? string.Empty;
            UpdateProxyFieldsEnabled();
            UpdateAutoSaveMinutesEnabled();
        }

        private void BindToWorking()
        {
            _working.ViewPostAfterPublish = _viewAfterPublish.IsChecked == true;
            _working.CloseWindowOnPublish = _closeOnPublish.IsChecked == true;
            _working.TitleReminder = _titleReminder.IsChecked == true;
            _working.CategoryReminder = _categoryReminder.IsChecked == true;
            _working.AutoSaveDrafts = _autoSave.IsChecked == true;
            if (_autoSaveMinutes.Value.HasValue)
                _working.AutoSaveMinutes = (int)_autoSaveMinutes.Value.Value;
            _working.ShowRealTimeWordCount = _wordCount.IsChecked == true;
            _working.FormatHtml = _formatHtml.IsChecked == true;

            _working.ReplaceHyphens = _replaceHyphens.IsChecked == true;
            _working.ReplaceSmartQuotes = _replaceQuotes.IsChecked == true;
            _working.ReplaceSpecialCharacters = _replaceSpecial.IsChecked == true;
            _working.ReplaceEmoticons = _replaceEmoticons.IsChecked == true;

            _working.SpellcheckEnabled = _spellcheck.IsChecked == true;

            _working.ProxyEnabled = _proxyEnabled.IsChecked == true;
            _working.ProxyHostname = _proxyHost.Text?.Trim();
            if (int.TryParse(_proxyPort.Text?.Trim(), out int port))
                _working.ProxyPort = port;
            _working.ProxyUsername = _proxyUser.Text?.Trim();
            _working.ProxyPassword = _proxyPassword.Text;
        }

        private Control BuildGeneralTab()
        {
            _viewAfterPublish = new CheckBox { Content = "View post after publishing" };
            _closeOnPublish = new CheckBox { Content = "Close window after publishing" };
            _titleReminder = new CheckBox { Content = "Remind me to type a title before publishing" };
            _categoryReminder = new CheckBox { Content = "Remind me to add categories before publishing" };

            var publishing = Group("Publishing",
                _viewAfterPublish, _closeOnPublish, _titleReminder, _categoryReminder);

            _autoSave = new CheckBox { Content = "Save AutoRecover information every" };
            _autoSave.IsCheckedChanged += (s, e) => UpdateAutoSaveMinutesEnabled();
            _autoSaveMinutes = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 60,
                Width = 70,
                VerticalAlignment = VerticalAlignment.Center
            };

            var autoSaveRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Children =
                {
                    _autoSave,
                    _autoSaveMinutes,
                    new TextBlock { Text = "minutes", VerticalAlignment = VerticalAlignment.Center }
                }
            };

            _wordCount = new CheckBox { Content = "Show real-time word count in status bar" };
            _formatHtml = new CheckBox { Content = "Format HTML when switching to source view" };

            var general = Group("General", autoSaveRow, _wordCount, _formatHtml);

            return Scroll(new StackPanel
            {
                Spacing = 12,
                Children = { publishing, general }
            });
        }

        private Control BuildEditingTab()
        {
            _replaceHyphens = new CheckBox { Content = "Replace hyphens with en-dashes and em-dashes" };
            _replaceQuotes = new CheckBox { Content = "Replace straight quotes with curly quotes" };
            _replaceSpecial = new CheckBox { Content = "Replace other special characters" };
            _replaceEmoticons = new CheckBox { Content = "Replace emoticons with emoji" };

            return Scroll(Group("Editing Options",
                _replaceHyphens, _replaceQuotes, _replaceSpecial, _replaceEmoticons));
        }

        private Control BuildSpellingTab()
        {
            _spellcheck = new CheckBox
            {
                Content = "Check spelling as you type (underline misspelled words)"
            };

            var note = new TextBlock
            {
                Text = "Spell-check uses the macOS system dictionary in the editor.",
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66)),
                Margin = new global::Avalonia.Thickness(0, 8, 0, 0)
            };

            return Scroll(new StackPanel
            {
                Spacing = 4,
                Children = { Group("Spelling", _spellcheck), note }
            });
        }

        private Control BuildProxyTab()
        {
            _proxyEnabled = new CheckBox { Content = "Connect through a proxy server" };
            _proxyEnabled.IsCheckedChanged += (s, e) => UpdateProxyFieldsEnabled();

            _proxyHost = new TextBox { PlaceholderText = "Hostname" };
            _proxyPort = new TextBox { PlaceholderText = "Port", Width = 80 };
            _proxyUser = new TextBox { PlaceholderText = "Username" };
            _proxyPassword = new TextBox { PlaceholderText = "Password", PasswordChar = '\u2022' };

            var hostRow = Labeled("Server:", _proxyHost);
            var portRow = Labeled("Port:", _proxyPort);
            var userRow = Labeled("Username:", _proxyUser);
            var passRow = Labeled("Password:", _proxyPassword);

            return Scroll(new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    Group("Web Proxy", _proxyEnabled),
                    hostRow, portRow, userRow, passRow
                }
            });
        }

        private Control BuildAccountsTab()
        {
            var openButton = new Button { Content = "Manage Accounts\u2026", MinWidth = 160, HorizontalAlignment = HorizontalAlignment.Left };
            openButton.Click += async (s, e) =>
            {
                if (_accountService == null)
                {
                    await MessageDialog.ShowAsync(this, "Accounts", "Account management is not available.");
                    return;
                }

                await new AccountManagerDialog(_accountService).ShowDialog(this);
            };

            var note = new TextBlock
            {
                Text = "Add, edit, or remove blog accounts and choose the current blog.",
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66))
            };

            return Scroll(new StackPanel
            {
                Spacing = 12,
                Children = { note, openButton }
            });
        }

        private void UpdateProxyFieldsEnabled()
        {
            bool on = _proxyEnabled?.IsChecked == true;
            if (_proxyHost != null) _proxyHost.IsEnabled = on;
            if (_proxyPort != null) _proxyPort.IsEnabled = on;
            if (_proxyUser != null) _proxyUser.IsEnabled = on;
            if (_proxyPassword != null) _proxyPassword.IsEnabled = on;
        }

        private void UpdateAutoSaveMinutesEnabled()
        {
            if (_autoSaveMinutes != null)
                _autoSaveMinutes.IsEnabled = _autoSave?.IsChecked == true;
        }

        private static Border Group(string title, params Control[] children)
        {
            var stack = new StackPanel { Spacing = 6 };
            foreach (var child in children)
                stack.Children.Add(child);

            return new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xD8, 0xD8, 0xD8)),
                BorderThickness = new global::Avalonia.Thickness(1),
                CornerRadius = new global::Avalonia.CornerRadius(4),
                Padding = new global::Avalonia.Thickness(12),
                Child = new StackPanel
                {
                    Spacing = 8,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = title,
                            FontWeight = FontWeight.SemiBold
                        },
                        stack
                    }
                }
            };
        }

        private static Control Labeled(string label, Control field)
        {
            var labelBlock = new TextBlock
            {
                Text = label,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(labelBlock, 0);
            Grid.SetColumn(field, 1);

            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("100,*"),
                Margin = new global::Avalonia.Thickness(4, 0),
                Children = { labelBlock, field }
            };
            return grid;
        }

        private static ScrollViewer Scroll(Control content)
        {
            return new ScrollViewer
            {
                Content = content,
                VerticalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
            };
        }
    }
}
