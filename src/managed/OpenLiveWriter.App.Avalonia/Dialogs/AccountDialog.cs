// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Threading.Tasks;
using global::Avalonia.Controls;
using global::Avalonia.Layout;
using OpenLiveWriter.Publishing.Accounts;

namespace OpenLiveWriter.App.Avalonia.Dialogs
{
    /// <summary>
    /// Result returned from <see cref="AccountDialog"/> when the user saves. Carries the
    /// edited account metadata plus the (separately-stored) password. On an edit where
    /// the password field is left blank, <see cref="Password"/> is null so the existing
    /// stored secret is preserved.
    /// </summary>
    public class AccountDialogResult
    {
        public BlogAccount Account { get; set; }
        public string Password { get; set; }
    }

    /// <summary>
    /// Add / Configure blog account dialog: blog URL, username, password, and the
    /// MetaWeblog API endpoint. A "Detect" button auto-fills the endpoint (and blog id)
    /// from the blog homepage via RSD discovery; manual override is retained. Mirrors the
    /// modal <c>ShowAsync(owner)</c> pattern used by the other shell dialogs.
    /// </summary>
    public class AccountDialog : Window
    {
        private readonly TextBox _displayNameBox;
        private readonly TextBox _homepageBox;
        private readonly TextBox _endpointBox;
        private readonly TextBox _blogIdBox;
        private readonly TextBox _usernameBox;
        private readonly TextBox _passwordBox;
        private readonly Button _saveButton;
        private readonly Button _detectButton;
        private readonly TextBlock _detectStatus;
        private readonly IRsdHttpFetcher _fetcher;
        private readonly string _existingId;
        private readonly bool _isEdit;

        public AccountDialogResult Result { get; private set; }

        public AccountDialog(BlogAccount existing = null, IRsdHttpFetcher fetcher = null)
        {
            _fetcher = fetcher ?? new HttpRsdFetcher();
            _isEdit = existing != null;
            _existingId = existing?.Id ?? string.Empty;

            Title = _isEdit ? "Blog Account Settings" : "Add a Blog Account";
            Width = 480;
            MinWidth = 420;
            SizeToContent = SizeToContent.Height;
            CanResize = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            _displayNameBox = new TextBox { Text = existing?.DisplayName ?? string.Empty };
            _homepageBox = new TextBox
            {
                PlaceholderText = "https://",
                Text = existing?.HomepageUrl ?? string.Empty
            };
            _endpointBox = new TextBox
            {
                PlaceholderText = "https://example.com/xmlrpc.php",
                Text = existing?.ApiEndpointUrl ?? string.Empty
            };
            _blogIdBox = new TextBox { Text = existing?.BlogId ?? string.Empty };
            _usernameBox = new TextBox { Text = existing?.Username ?? string.Empty };
            _passwordBox = new TextBox
            {
                PasswordChar = '\u2022',
                Text = string.Empty
            };

            _saveButton = new Button
            {
                Content = "Save",
                IsDefault = true,
                MinWidth = 80,
                IsEnabled = false
            };
            var cancelButton = new Button { Content = "Cancel", IsCancel = true, MinWidth = 80 };

            _detectButton = new Button { Content = "Detect", MinWidth = 80 };
            _detectStatus = new TextBlock
            {
                Text = string.Empty,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 11,
                TextWrapping = global::Avalonia.Media.TextWrapping.Wrap,
                Foreground = new global::Avalonia.Media.SolidColorBrush(
                    global::Avalonia.Media.Color.FromRgb(0x66, 0x66, 0x66))
            };
            _detectButton.Click += async (s, e) => await DetectAsync();

            _saveButton.Click += (s, e) =>
            {
                Result = new AccountDialogResult
                {
                    Account = new BlogAccount
                    {
                        Id = _existingId,
                        DisplayName = _displayNameBox.Text?.Trim() ?? string.Empty,
                        HomepageUrl = _homepageBox.Text?.Trim() ?? string.Empty,
                        ApiEndpointUrl = _endpointBox.Text?.Trim() ?? string.Empty,
                        BlogId = _blogIdBox.Text?.Trim() ?? string.Empty,
                        Username = _usernameBox.Text?.Trim() ?? string.Empty,
                        ProviderType = existing?.ProviderType ?? BlogAccount.DefaultProviderType,
                        SupportsPages = existing?.SupportsPages ?? true,
                        SupportsCategories = existing?.SupportsCategories ?? true,
                        SupportsExtendedEntries = existing?.SupportsExtendedEntries ?? true
                    },
                    // On edit, an empty password means "keep the existing secret".
                    Password = string.IsNullOrEmpty(_passwordBox.Text)
                        ? (_isEdit ? null : string.Empty)
                        : _passwordBox.Text
                };
                Close(Result);
            };
            cancelButton.Click += (s, e) => Close(null);

            void Revalidate(object s, EventArgs e) => _saveButton.IsEnabled = CanSave();
            _endpointBox.PropertyChanged += (s, e) => { if (e.Property == TextBox.TextProperty) Revalidate(s, e); };
            _usernameBox.PropertyChanged += (s, e) => { if (e.Property == TextBox.TextProperty) Revalidate(s, e); };
            _passwordBox.PropertyChanged += (s, e) => { if (e.Property == TextBox.TextProperty) Revalidate(s, e); };
            _saveButton.IsEnabled = CanSave();

            var grid = new Grid
            {
                Margin = new global::Avalonia.Thickness(16),
                RowDefinitions = new RowDefinitions("Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto"),
                ColumnDefinitions = new ColumnDefinitions("Auto,*")
            };

            AddRow(grid, 0, "Name:", _displayNameBox);
            AddRow(grid, 1, "Blog URL:", _homepageBox);
            AddRow(grid, 2, "API endpoint:", _endpointBox);
            AddRow(grid, 3, "Blog ID:", _blogIdBox);
            AddRow(grid, 4, "Username:", _usernameBox);
            AddRow(grid, 5, "Password:", _passwordBox);

            // Detect row: pull the endpoint/blog id from the Blog URL via RSD discovery.
            var detectRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Margin = new global::Avalonia.Thickness(0, 8, 0, 0)
            };
            detectRow.Children.Add(_detectButton);
            detectRow.Children.Add(_detectStatus);
            Grid.SetRow(detectRow, 6);
            Grid.SetColumn(detectRow, 0);
            Grid.SetColumnSpan(detectRow, 2);
            grid.Children.Add(detectRow);

            var hint = new TextBlock
            {
                Text = "Enter your blog's MetaWeblog XML-RPC endpoint (e.g. .../xmlrpc.php), "
                     + "or click Detect to discover it from the Blog URL.",
                FontSize = 11,
                TextWrapping = global::Avalonia.Media.TextWrapping.Wrap,
                Foreground = new global::Avalonia.Media.SolidColorBrush(
                    global::Avalonia.Media.Color.FromRgb(0x66, 0x66, 0x66)),
                Margin = new global::Avalonia.Thickness(0, 8, 0, 0)
            };
            Grid.SetRow(hint, 7);
            Grid.SetColumn(hint, 0);
            Grid.SetColumnSpan(hint, 2);
            grid.Children.Add(hint);

            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 8,
                Margin = new global::Avalonia.Thickness(0, 12, 0, 0)
            };
            buttonRow.Children.Add(cancelButton);
            buttonRow.Children.Add(_saveButton);
            Grid.SetRow(buttonRow, 8);
            Grid.SetColumn(buttonRow, 0);
            Grid.SetColumnSpan(buttonRow, 2);
            grid.Children.Add(buttonRow);

            Content = grid;
        }

        // Runs RSD auto-detection off the Blog URL and fills in the endpoint (and blog id,
        // when the RSD provides one). The network fetch runs on a background thread via the
        // injected fetcher; the endpoint field stays editable so the user can override.
        private async Task DetectAsync()
        {
            string homepage = _homepageBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(homepage))
            {
                _detectStatus.Text = "Enter your Blog URL first.";
                return;
            }

            _detectButton.IsEnabled = false;
            _detectStatus.Text = "Detecting\u2026";
            try
            {
                RsdDetectionResult result = await Task.Run(
                    () => RsdServiceDetector.Detect(homepage, _fetcher));

                if (result.Found)
                {
                    _endpointBox.Text = result.EndpointUrl;
                    if (!string.IsNullOrEmpty(result.BlogId) && string.IsNullOrWhiteSpace(_blogIdBox.Text))
                        _blogIdBox.Text = result.BlogId;
                    _detectStatus.Text = string.IsNullOrEmpty(result.EngineName)
                        ? "Found the API endpoint."
                        : $"Detected {result.EngineName}.";
                    _saveButton.IsEnabled = CanSave();
                }
                else
                {
                    _detectStatus.Text = "Couldn't detect the endpoint. Enter it manually.";
                }
            }
            catch (Exception ex)
            {
                _detectStatus.Text = $"Detection failed: {ex.Message}";
            }
            finally
            {
                _detectButton.IsEnabled = true;
            }
        }

        private bool CanSave() => CanSave(
            _endpointBox.Text, _usernameBox.Text, _passwordBox.Text, _isEdit);

        /// <summary>
        /// Save is enabled when a non-trivial endpoint URL and username are present, and
        /// (for a new account) a password has been entered. On edit, a blank password is
        /// allowed and means "keep the existing one".
        /// </summary>
        internal static bool CanSave(string endpoint, string username, string password, bool isEdit)
        {
            if (string.IsNullOrWhiteSpace(endpoint)) return false;
            string trimmed = endpoint.Trim();
            if (trimmed.Equals("https://", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("http://", StringComparison.OrdinalIgnoreCase))
                return false;
            if (string.IsNullOrWhiteSpace(username)) return false;
            if (!isEdit && string.IsNullOrEmpty(password)) return false;
            return true;
        }

        private static void AddRow(Grid grid, int row, string label, Control field)
        {
            var text = new TextBlock
            {
                Text = label,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new global::Avalonia.Thickness(0, 4, 8, 4),
                MinWidth = 90
            };
            Grid.SetRow(text, row);
            Grid.SetColumn(text, 0);
            grid.Children.Add(text);

            field.Margin = new global::Avalonia.Thickness(0, 4, 0, 4);
            Grid.SetRow(field, row);
            Grid.SetColumn(field, 1);
            grid.Children.Add(field);
        }

        /// <summary>
        /// Shows the dialog modally over <paramref name="owner"/> and returns the saved
        /// account + password, or null if cancelled / headless (null owner).
        /// </summary>
        public static async Task<AccountDialogResult> ShowAsync(
            Window owner,
            BlogAccount existing = null,
            IRsdHttpFetcher fetcher = null)
        {
            var dialog = new AccountDialog(existing, fetcher);
            if (owner != null)
                return await dialog.ShowDialog<AccountDialogResult>(owner);

            dialog.Show();
            return null;
        }
    }
}
